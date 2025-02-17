import { usePrimeVue, type PrimeVueLocaleOptions } from 'primevue/config'
import { nextTick, ref, type Ref } from 'vue'

export class LocaleService {
  private static _instance: LocaleService
  private _primevue: ReturnType<typeof usePrimeVue> | undefined
  private _localeFiles: { [key: string]: () => Promise<unknown> } = import.meta.glob(
    '@/locales/*.json'
  )
  private _localeMap: Map<string, string>
  readonly localeList: Ref<string[]> = ref([])
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
    this._localeMap = new Map()
    this.localeList.value = []

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

    // If no locale was assigned force ref update to take into account loaded locale
    if (unassignedLocale) {
      const locales = Object.keys(this._localeMap)
      const tempLocaleIndex =
        locales.length > 1
          ? (locales.findIndex((l) => l === nameNormalized) + 1) % locales.length
          : undefined
      this.currentLocale.value = typeof tempLocaleIndex === 'number' ? locales[tempLocaleIndex] : ''
      nextTick(() => {
        this.currentLocale.value = nameNormalized
      })
    } else {
      this.currentLocale.value = nameNormalized
    }
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
