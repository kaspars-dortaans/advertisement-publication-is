import type { MessageItemDto } from '@/services/api-client'

export interface IMessageDateGroup {
  date: Date
  messages: MessageItemDto[]
}
