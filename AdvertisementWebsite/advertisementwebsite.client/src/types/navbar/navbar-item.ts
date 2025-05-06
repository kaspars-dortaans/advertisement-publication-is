import type { MenuItem } from 'primevue/menuitem'
import type { ComputedRef, Ref } from 'vue'

export interface INavbarItem extends MenuItem {
  avatar?: boolean
  showForUnauthenticated?: boolean
  badgeValue?: Ref<number | string> | ComputedRef<number | string>
  doNotShowWithoutItems?: boolean
}
