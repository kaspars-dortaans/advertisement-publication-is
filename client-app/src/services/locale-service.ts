import { DefaultLocale } from '@/constants/default-locale'
import { usePrimeVue, type PrimeVueLocaleOptions } from 'primevue/config'
import { ref, type Ref } from 'vue'
import { Settings } from './settings'
import { emptyLocale } from '@/init/empty-locale'

export class LocaleService {
  private static _instance: LocaleService
  private _primevue: ReturnType<typeof usePrimeVue> | undefined
  private _settings: Settings

  /** Object for locale loading */
  private _localeFiles: { [key: string]: () => Promise<unknown> } = import.meta.glob(
    '@/locales/*.json'
  )

  /** map Locale name -> locale file promise key*/
  private _localeMap: Map<string, string>

  /** List with available locales */
  readonly localeList: Ref<string[]> = ref([])

  /** Currently selected locale */
  readonly currentLocale = ref('')

  public static get(primevue?: ReturnType<typeof usePrimeVue>) {
    if (!this._instance) {
      this._instance = new LocaleService()
    }

    if (primevue) {
      this._instance._primevue = primevue
    }

    return this._instance
  }

  private constructor() {
    this._settings = Settings.get()
    this._localeMap = new Map()
    this.localeList.value = []
    LocaleService.currentLocale.value = emptyLocale

    for (const filePath in this._localeFiles) {
      const filePathSplit = filePath.split('/')
      const fileName = filePathSplit[filePathSplit.length - 1]
      const localeName = fileName.substring(0, fileName.lastIndexOf('.')).toLocaleUpperCase()
      this._localeMap.set(localeName, filePath)
      this.localeList.value.push(localeName)
    }
  }

  get primevue() {
    if (!this._primevue) this._primevue = usePrimeVue()

    return this._primevue
  }

  /** Try to get saved locale, if not present set to default locale */
  async loadSavedOrDefaultLocale() {
    if (!this._settings.userSettingCookieValue.locale) {
      this._settings.userSettingCookieValue.locale = this.getBrowserCompatibleLocale()
    }

    const locale = this._settings.userSettingCookieValue.locale
    await this.loadLocale(locale)
  }

  /** Return available app locale which also is present in navigator.languages, DefaultLocale is used as fallback */
  getBrowserCompatibleLocale() {
    const browserLocales = new Set<string>()
    const languages = window.navigator.languages ?? [window.navigator.language]
    for (const language of languages) {
      const split = language.toUpperCase().split('-')
      // From format xx-yy add both xx and yy (from en-us both 'en' and 'us' are added)
      for (const l of split) {
        browserLocales.add(l)
      }
    }

    //Iterate over set and try find existing locale which is equal to preferred browser locale
    const iterator = browserLocales.values()
    let iteratorResult = iterator.next()
    while (!iteratorResult.done) {
      if (
        this.localeList.value.some((availableLocale) => iteratorResult.value == availableLocale)
      ) {
        return iteratorResult.value
      }
      iteratorResult = iterator.next()
    }

    //If no compatible locale found return default
    return DefaultLocale
  }

  /** Load locale by name */
  async loadLocale(name: string) {
    const nameNormalized = name.toLocaleUpperCase()
    if (!this.localeList.value.some((value) => value === nameNormalized)) {
      //Locale not found, return
      return
    }

    // If no locale is assigned, assign new immediately to prevent errors
    const unassignedLocale = !this.currentLocale.value
    if (unassignedLocale) {
      this.currentLocale.value = nameNormalized
    }

    const filePath = this._localeMap.get(nameNormalized)!
    const locale = await this._localeFiles[filePath]()
    this.primevue.config.locale = locale as PrimeVueLocaleOptions

    this.currentLocale.value = nameNormalized
    this._settings.userSettingCookieValue.locale = nameNormalized
  }

  /** Localize and format multiple strings */
  localizeMultiple(keys: string[], suffix?: string, ...params: string[]) {
    return keys.map((k) => this.l((suffix ?? '') + k, ...params))
  }

  /** Localize and format string */
  l(keyString: string, ...params: (string | number)[]) {
    if (!keyString) {
      return ''
    }

    const locale = this.primevue.config.locale as object
    const keys = keyString.split('.')
    let object = locale

    for (const key of keys) {
      if (key in object) {
        object = object[key as keyof object]
      } else return keyString
    }
    return this.f(object + '', params)
  }

  /** Format string */
  private f(str: string, params: (string | number)[]) {
    if (!params.length) {
      return str
    }

    const paramsPrefix = '{'
    const paramSuffix = '}'
    let result = ''
    let lastParamIndex = 0,
      prefixIndex = -1,
      suffixIndex = -1

    for (const param of params) {
      prefixIndex = str.indexOf(paramsPrefix, lastParamIndex)
      suffixIndex = str.indexOf(paramSuffix, lastParamIndex)
      if (prefixIndex >= 0 && suffixIndex >= 0) {
        result += str.slice(lastParamIndex, prefixIndex) + param
        lastParamIndex = suffixIndex + 1
      } else {
        break
      }
    }
    result += str.slice(lastParamIndex)
    return result
  }
}
