/* eslint-disable vue/no-reserved-component-names */
/* eslint-disable vue/multi-word-component-names */

import { type App } from 'vue'

//PrimeVue
import PrimeVue from 'primevue/config'

//File
import FileUpload from 'primevue/fileupload'

//Menu
import MenuBar, { type MenubarPassThroughMethodOptions } from 'primevue/menubar'

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
import { usePassThrough } from 'primevue/passthrough'
import Password from 'primevue/password'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'

//Data
import Column from 'primevue/column'
import DataTable, { type DataTablePassThroughMethodOptions } from 'primevue/datatable'
import Tree from 'primevue/tree'

//Panel
import Panel from 'primevue/panel'
import Divider from 'primevue/divider'

//Message
import Checkbox from 'primevue/checkbox'
import Message from 'primevue/message'

//Media
import Galleria from 'primevue/galleria'
import Image from 'primevue/image'

//Misc
import Avatar from 'primevue/avatar'
import BlockUI from 'primevue/blockui'
import Badge from 'primevue/badge'

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
    start: 'flex-1',
    submenu: (opts: MenubarPassThroughMethodOptions) => {
      //Fix submenu going out of screen
      const isSubmenuActive =
        opts?.instance?.items?.[0]?.parent?.parent &&
        opts.instance.isItemActive(opts.instance.items[0].parent)

      if (!isSubmenuActive) {
        return
      }

      const htmlEl = opts.instance.$el as HTMLElement
      const parentPosition = htmlEl?.parentElement?.getClientRects()
      if (!parentPosition?.length) {
        return
      }

      let elPosition = htmlEl.getClientRects()
      //If submenu is still hidden, temporary display it and get its position and size
      if (!elPosition.length) {
        const originalStyle = htmlEl.style + ''
        htmlEl.setAttribute('style', 'display: flex; visibility: hidden')
        elPosition = htmlEl.getClientRects()
        htmlEl.setAttribute('style', originalStyle)
      }

      //If submenu is going to overflow page, set right to 0, it is in absolute position
      if (elPosition.length) {
        if (parentPosition[0].left + elPosition[0].width > window.innerWidth) {
          return 'right-0'
        }
      }
    }
  },

  dataTable: {
    root: 'flex flex-col flex-nowrap',
    tableContainer: 'flex-auto',
    //Incorrect colspan fix
    rowGroupHeaderCell: (opts: DataTablePassThroughMethodOptions) => {
      const columnCount = opts?.instance?.columns?.length ? opts.instance.columns.length : 1
      return {
        colspan: columnCount
      }
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
  app.component('Divider', Divider)

  //Message
  app.component('Message', Message)

  //Media
  app.component('Image', Image)
  app.component('Galleria', Galleria)

  //Misc
  app.component('BlockUI', BlockUI)
  app.component('Avatar', Avatar)
  app.component('Badge', Badge)
}
