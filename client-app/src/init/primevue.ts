/* eslint-disable vue/no-reserved-component-names */
/* eslint-disable vue/multi-word-component-names */

import { type App } from 'vue'
import { LocaleService } from '@/services/locale-service'

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
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import { usePassThrough } from 'primevue/passthrough'

//Data
import Tree from 'primevue/tree'

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
  },
  menubar: {
    start: 'flex-1'
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

  const localeService = LocaleService.get(app.config.globalProperties.$primevue)
  // TODO: define default locale as constant or env variable
  localeService.loadLocale('eng')

  //components
  //File
  app.component('FileUpload', FileUpload)

  //Menu
  app.component('MenuBar', MenuBar)

  //Button
  app.component('Button', Button)

  //Form
  app.component('Checkbox', Checkbox)
  app.component('IconField', IconField)
  app.component('InputIcon', InputIcon)
  app.component('InputGroup', InputGroup)
  app.component('InputGroupAddon', InputGroupAddon)
  app.component('InputText', InputText)
  app.component('Password', Password)

  //Data
  app.component('Tree', Tree)

  //Panel
  app.component('Panel', Panel)

  //Message
  app.component('Message', Message)

  //Media
  app.component('Image', Image)
}
