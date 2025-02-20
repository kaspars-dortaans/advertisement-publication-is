/* eslint-disable vue/no-reserved-component-names */
/* eslint-disable vue/multi-word-component-names */

import { type App } from 'vue'
import { LocaleService } from '@/services/locale-service'

//PrimeVue
import PrimeVue from 'primevue/config'

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
import InputNumber from 'primevue/inputnumber'
import Password from 'primevue/password'
import { usePassThrough } from 'primevue/passthrough'
import FloatLabel from 'primevue/floatlabel'
import Select from 'primevue/select'

//Data
import Tree from 'primevue/tree'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'

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
  },
  datatable: {
    pcPaginator: {
      content: {
        class: 'flex flex-row items-center justify-center w-full'
      },
      pcRowPerPageDropdown: {
        root: {
          class: 'ml-auto mr-2'
        }
      }
    }
  }
}

export function initPrimeVue(app: App<Element>) {
  const globalPassTrough = usePassThrough(customPassTrough, {
    mergeSections: true,
    mergeProps: true
  })

  app.use(PrimeVue, {
    theme: 'none',
    pt: globalPassTrough,
    ptOptions: { mergeSections: true, mergeProps: true }
  })

  const localeService = LocaleService.get(app.config.globalProperties.$primevue)
  localeService.loadSavedOrDefaultLocaleName()

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
  app.component('InputNumber', InputNumber)
  app.component('Password', Password)
  app.component('FloatLabel', FloatLabel)
  app.component('Select', Select)

  //Data
  app.component('Tree', Tree)
  app.component('DataTable', DataTable)
  app.component('Column', Column)

  //Panel
  app.component('Panel', Panel)

  //Message
  app.component('Message', Message)

  //Media
  app.component('Image', Image)
}
