import type { App } from 'vue'

//PrimeVue
import PrimeVue from 'primevue/config'
import AuraPreset from '@/presets/aura'
import MenuBar from 'primevue/menubar'

export function initPrimeVue(app: App<Element>) {
  app.use(PrimeVue, {
    unstyled: true,
    pt: AuraPreset
  })
  
  const localeService = new LocaleService(app.config.globalProperties.$primevue)
  localeService.loadLocale("en")

  //components
  app.component('MenuBar', MenuBar)
}
