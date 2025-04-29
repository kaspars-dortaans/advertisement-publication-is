import { LocaleService } from '@/services/locale-service'
import type { App } from 'vue'
import { setLocale } from 'yup'

export const initYup = (app: App<Element>) => {
  const ls = LocaleService.get(app.config.globalProperties.$primevue)
  setLocale({
    string: {
      email: () => ls.l('errors.NotAnEmail')
    },
    mixed: {
      required: ({ path }) => ls.l('errors.RequiredField', ls.l(path))
    }
  })
}
