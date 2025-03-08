/* eslint-disable vue/no-reserved-component-names */
/* eslint-disable vue/multi-word-component-names */

import { type App } from 'vue'

//PrimeVue
import PrimeVue from 'primevue/config'

//File
import FileUpload from 'primevue/fileupload'

//Menu
import MenuBar from 'primevue/menubar'

//Button
import Button from 'primevue/button'

//Form
import FloatLabel from 'primevue/floatlabel'
import IconField from 'primevue/iconfield'
import InputGroup from 'primevue/inputgroup'
import InputGroupAddon from 'primevue/inputgroupaddon'
import InputIcon from 'primevue/inputicon'
import InputNumber from 'primevue/inputnumber'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import { usePassThrough } from 'primevue/passthrough'
import Password from 'primevue/password'
import Select from 'primevue/select'

//Data
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Tree from 'primevue/tree'

//Panel
import Panel from 'primevue/panel'

//Message
import Checkbox from 'primevue/checkbox'
import Message from 'primevue/message'

//Media
import Galleria from 'primevue/galleria'
import Image from 'primevue/image'

//Misc
import BlockUI from 'primevue/blockui'

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
  dataTable: {
    root: 'flex flex-col flex-nowrap',
    tableContainer: 'flex-auto',
    table: 'h-full',
    tbody: 'block',
    column: {
      bodyCell: ['align-top']
    },
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
  },
  galleria: {
    content: {
      class: 'max-h-full'
    },
    itemsContainer: {
      class: 'min-h-0 flex-1'
    },
    items: {
      class: 'galleria-item-button'
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
  app.component('Textarea', Textarea)
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
  app.component('Galleria', Galleria)

  //Misc
  app.component('BlockUI', BlockUI)
}
