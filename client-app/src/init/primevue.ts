/* eslint-disable vue/no-reserved-component-names */
/* eslint-disable vue/multi-word-component-names */

import { type App } from 'vue'

//PrimeVue
import PrimeVue from 'primevue/config'
import AuraPreset from '@/presets/aura'

//File
import FileUpload from 'primevue/fileupload'

//Menu
import MenuBar from 'primevue/menubar'

//Button
import Button from 'primevue/button'

//Form
import InputGroup from 'primevue/inputgroup'
import InputGroupAddon from 'primevue/inputgroupaddon'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import { usePassThrough } from 'primevue/passthrough'
import { LocaleService } from '@/services/locale-service'

//Panel
import Panel from 'primevue/panel'

//Message
import Message from 'primevue/message'
import Checkbox from 'primevue/checkbox'

//Media
import Image from 'primevue/image'

const customPassTrough = {
  password: {
    root: {
      class: 'flex-1'
    },
    pcInput: {
      root: {
        class: 'flex-1'
      }
    }
  }
}

export function initPrimeVue(app: App<Element>) {
  const globalPassTrough = usePassThrough(AuraPreset, customPassTrough, {
    mergeSections: true,
    mergeProps: true
  })

  app.use(PrimeVue, {
    unstyled: true,
    pt: globalPassTrough,
    ptOptions: { mergeSections: true, mergeProps: true }
  })

  const localeService = new LocaleService(app.config.globalProperties.$primevue)
  localeService.loadLocale('en')

  //components
  //File
  app.component('FileUpload', FileUpload)

  //Menu
  app.component('MenuBar', MenuBar)

  //Button
  app.component('Button', Button)

  //Form
  app.component('Checkbox', Checkbox)
  app.component('InputGroup', InputGroup)
  app.component('InputGroupAddon', InputGroupAddon)
  app.component('InputText', InputText)
  app.component('Password', Password)

  //Panel
  app.component('Panel', Panel)

  //Message
  app.component('Message', Message)

  //Media
  app.component("Image", Image)
}
