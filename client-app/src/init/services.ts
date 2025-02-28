import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import type { App } from 'vue'

export const initServices = (app: App) => {
  //Locale
  const localeService = LocaleService.get(app.config.globalProperties.$primevue)
  localeService.loadSavedOrDefaultLocaleName()

  //Authorization
  new AuthService() //Load jwt token from storage
}
