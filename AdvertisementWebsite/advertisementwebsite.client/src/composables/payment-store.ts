import { createGlobalState, useStorage } from '@vueuse/core'
import type { IPaymentState } from '@/types/store/payment-state'

export const usePaymentState = createGlobalState(() =>
  useStorage<IPaymentState>('current-payment-info', { paymentItems: [] })
)
