import { StringStringKeyValuePair } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { computed } from 'vue'

/**
 * Enumerates object property names, gets localization text, and maps them to option list
 */
export const useEnumOptionList = (e: object, localeKey: string) => {
  const l = LocaleService.currentLocale
  const list = computed(() => {
    return Object.keys(e)
      .filter((e) => isNaN(parseInt(e)))
      .map(
        (k) =>
          new StringStringKeyValuePair({
            key: k,
            value: l.value[localeKey][k]
          })
      )
  })

  return list
}
