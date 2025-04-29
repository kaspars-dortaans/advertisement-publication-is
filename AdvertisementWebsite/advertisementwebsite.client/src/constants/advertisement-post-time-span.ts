import { PostTimeDto, type IPostTimeDto } from '@/services/api-client'
import type { LocaleService } from '@/services/locale-service'

const defaultOptionList: { timeUnit: keyof IPostTimeDto; value: number }[] = [
  { timeUnit: 'days', value: 1 },
  { timeUnit: 'days', value: 3 },
  { timeUnit: 'weeks', value: 1 },
  { timeUnit: 'weeks', value: 2 },
  { timeUnit: 'weeks', value: 3 },
  { timeUnit: 'months', value: 1 },
  { timeUnit: 'months', value: 2 },
  { timeUnit: 'months', value: 3 }
]

export const createAdvertisementPostTimeSpanOptions = (ls: LocaleService) => {
  return defaultOptionList.map((o) => ({
    name: ls.l('time.' + o.timeUnit, o.value),
    value: new PostTimeDto({ [o.timeUnit]: o.value })
  }))
}
