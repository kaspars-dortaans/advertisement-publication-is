import {
  CreateOrEditNotificationSubscriptionRequest,
  Int32StringKeyValuePair
} from '@/services/api-client'

export type PutNotificationSubscriptionForm = Omit<
  CreateOrEditNotificationSubscriptionRequest,
  'attributeValues' | 'keywords' | 'ownerId'
> & {
  attributeValues: (string | number | undefined)[]
  keywords: string[]
  ownerId: Int32StringKeyValuePair
}
