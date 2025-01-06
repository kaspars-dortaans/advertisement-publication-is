import { usePrimeVue, type PrimeVueLocaleOptions } from 'primevue/config'
import { ref } from 'vue'

export class LocaleService {
  private static _instance: LocaleService
  private _primevue: ReturnType<typeof usePrimeVue> | undefined
  private _localeFiles: { [key: string]: () => Promise<unknown> } = import.meta.glob(
    '@/locales/*.json'
  )
  private _localeMap: Map<string, string>
  private _localeList: string[]
  private _currentLocale = ref('')

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
    this._localeMap = new Map()
    this._localeList = []

    for (const filePath in this._localeFiles) {
      const filePathSplit = filePath.split('/')
      const fileName = filePathSplit[filePathSplit.length - 1]
      const localeName = fileName.substring(0, fileName.lastIndexOf('.')).toLocaleUpperCase()
      this._localeMap.set(localeName, filePath)
      this._localeList.push(localeName)
    }
  }

  get primevue() {
    if (!this._primevue) this._primevue = usePrimeVue()

    return this._primevue
  }

  /**
   * Return available locale list. Do NOT modify it
   */
  get localeList() {
    return this._localeList
  }

  get currentLocale() {
    return this._currentLocale
  }

  async loadLocale(name: string) {
    const nameNormalized = name.toLocaleUpperCase()
    if (!this._localeList.some((value) => value === nameNormalized)) {
      //Locale not found, return
      return
    }

    this._currentLocale.value = nameNormalized
    const filePath = this._localeMap.get(nameNormalized)!
    const locale = await this._localeFiles[filePath]()
    this.primevue.config.locale = locale as PrimeVueLocaleOptions
  }

  localizeMultiple(keys: string[], suffix?: string, ...params: string[]) {
    return keys.map((k) => this.l((suffix ?? '') + k, ...params))
  }

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
    return this.insertParamsIntoString(object + '', params)
  }

  insertParamsIntoString(str: string, params: (string | number)[]) {
    if (!params.length) {
      return str
    }

    const paramsPrefix = '{'
    const paramSuffix = '}'
    let result = str
    let lastParamIndex = 0,
      prefixIndex = -1,
      suffixIndex = -1
    for (const param of params) {
      prefixIndex = result.indexOf(paramsPrefix, lastParamIndex)
      suffixIndex = result.indexOf(paramSuffix, prefixIndex)
      if (prefixIndex >= 0 && suffixIndex >= 0) {
        result = result.slice(0, prefixIndex) + param + result.slice(suffixIndex + 1)
        lastParamIndex = prefixIndex
      } else {
        break
      }
    }
    return result
  }
}
