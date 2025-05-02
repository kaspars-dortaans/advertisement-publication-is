import type { NewPaymentItem } from '@/services/api-client'

export interface IPaymentState {
  paymentItems: NewPaymentItem[]
}
