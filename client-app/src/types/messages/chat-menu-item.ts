import type { ChatListItemDto } from '@/services/api-client'
import type { MenuItem } from 'primevue/menuitem'

export interface IChatMenuItem extends MenuItem {
  chat: ChatListItemDto
}
