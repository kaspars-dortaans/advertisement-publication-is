import { AppNavigation } from '@/services/app-navigation'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { MessageHub } from '@/services/message-hub'
import { Settings } from '@/services/settings'
import type { App } from 'vue'
import type { Router } from 'vue-router'

export const initServices = (app: App, router: Router) => {
  //Locale
  const localeService = LocaleService.get(app.config.globalProperties.$primevue)
  localeService.loadSavedOrDefaultLocaleName()

  //Load and sync settings cookie
  Settings.get()

  //Authorization
  AuthService.get() //Load jwt token from storage

  //App navigation history
  AppNavigation.get(router)

  app.provide('messageHub', new MessageHub())
}
