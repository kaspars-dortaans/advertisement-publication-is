import type { PutAttributeRequest } from '@/services/api-client'

export type AttributeForm = Omit<PutAttributeRequest, 'localizedNames'> & {
  localizedNames: string[]
}
