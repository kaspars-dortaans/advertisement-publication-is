import { LocaleService } from '@/services/locale-service'
import type { ConfirmationOptions } from 'primevue/confirmationoptions'
import type { ComputedRef, Ref } from 'vue'
import { useConfirm } from 'primevue'

const l = LocaleService.currentLocale

export const leaveFormDialogConfig = (next: () => void) =>
  ({
    header: l.value.form.common.leaveForm,
    message: l.value.form.common.changesWillBeDiscarded,
    acceptLabel: l.value.actions.leave,
    rejectLabel: l.value.actions.stay,
    acceptProps: {
      severity: 'danger'
    },
    rejectProps: {
      severity: 'secondary'
    },
    accept: () => {
      next()
    }
  }) as ConfirmationOptions

export const leaveFormGuard = (
  confirm: ReturnType<typeof useConfirm>,
  allowLeave: Ref<boolean>,
  valuesChanged: ComputedRef<boolean>,
  next: () => void
) => {
  if (allowLeave.value || !valuesChanged.value) {
    next()
  } else {
    confirm.require(leaveFormDialogConfig(next))
  }
}

export const confirmDelete = (
  confirm: ReturnType<typeof useConfirm>,
  config?: ConfirmationOptions
) => {
  confirm.require({
    acceptLabel: l.value.actions.delete,
    rejectLabel: l.value.actions.cancel,
    acceptProps: {
      severity: 'danger'
    },
    rejectProps: {
      severity: 'secondary'
    },
    ...config
  })
}
