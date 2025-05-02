import { PaymentSubjectStatus } from '@/services/api-client'

export const statusSeverity: { [k: string]: string } = {
  [PaymentSubjectStatus[PaymentSubjectStatus.Active]]: 'success',
  [PaymentSubjectStatus[PaymentSubjectStatus.Inactive]]: 'secondary',
  [PaymentSubjectStatus[PaymentSubjectStatus.Expired]]: 'warn',
  [PaymentSubjectStatus[PaymentSubjectStatus.Draft]]: 'secondary'
}
