import type { AttributeValueListEntryDto } from '@/services/api-client'

export type AttributeValueListForm = {
  title: string[]
  entries: AttributeValueListEntryDto[]
}
