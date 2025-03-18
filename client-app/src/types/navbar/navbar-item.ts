import type { MenuItem } from 'primevue/menuitem'

export interface INavbarItem extends MenuItem {
  avatar?: boolean
  showWithoutPermission?: boolean
}
