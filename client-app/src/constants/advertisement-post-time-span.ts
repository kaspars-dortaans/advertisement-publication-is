import type { IPostTimeDto } from '@/services/api-client'
import type { LocaleService } from '@/services/locale-service'

const defaultOptionList: { timeUnit: keyof IPostTimeDto; value: number }[] = [
  { timeUnit: 'days', value: 1 },
  { timeUnit: 'days', value: 3 },
  { timeUnit: 'weeks', value: 7 },
  { timeUnit: 'weeks', value: 14 },
  { timeUnit: 'weeks', value: 21 },
  { timeUnit: 'months', value: 30 },
  { timeUnit: 'months', value: 60 },
  { timeUnit: 'months', value: 90 }
]

export const createAdvertisementPostTimeSpanOptions = (ls: LocaleService) => {
  return defaultOptionList.map((o) => ({
    name: ls.l('time.' + o.timeUnit, o.value),
    value: { [o.timeUnit]: o.value } as IPostTimeDto
  }))
}
