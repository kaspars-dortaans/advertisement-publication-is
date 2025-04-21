import { CreateOrEditNotificationSubscriptionRequest } from '@/services/api-client'

export type PutNotificationSubscriptionForm = Omit<
  CreateOrEditNotificationSubscriptionRequest,
  'attributeValues' | 'keywords'
> & {
  attributeValues: (string | number | undefined)[]
  keywords: string[]
}
