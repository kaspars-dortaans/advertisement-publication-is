import { PostTimeDto, type IPostTimeDto } from '@/services/api-client'
import type { LocaleService } from '@/services/locale-service'

export type postTimeSpanOptionList = { timeUnit: keyof IPostTimeDto; value: number }

const defaultOptionList: postTimeSpanOptionList[] = [
  { timeUnit: 'days', value: 1 },
  { timeUnit: 'days', value: 3 },
  { timeUnit: 'weeks', value: 1 },
  { timeUnit: 'weeks', value: 2 },
  { timeUnit: 'weeks', value: 3 },
  { timeUnit: 'months', value: 1 },
  { timeUnit: 'months', value: 2 },
  { timeUnit: 'months', value: 3 }
]

export const createAdvertisementPostTimeSpanOptions = (
  ls: LocaleService,
  optionList = defaultOptionList
) => {
  return optionList.map((o) => ({
    name: ls.l('time.' + o.timeUnit, o.value),
    value: new PostTimeDto({ [o.timeUnit]: o.value })
  }))
}

export const getPostTimeTitle = (ls: LocaleService, time?: PostTimeDto) => {
  if (time) {
    let key: keyof PostTimeDto
    for (key in time) {
      if (typeof time[key] === 'number' && time[key]) {
        return ls.l('time.' + key, '' + time[key])
      }
    }
  }
  return ls.l('time.days', 0)
}
