import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { Settings } from '@/services/settings'
import type { App } from 'vue'

export const initServices = (app: App) => {
  //Locale
  const localeService = LocaleService.get(app.config.globalProperties.$primevue)
  localeService.loadSavedOrDefaultLocaleName()

  //Load and sync settings cookie
  Settings.get()

  //Authorization
  new AuthService() //Load jwt token from storage
}
