<template>
  <ResponsivePanel
    :defaultBackButtonRoute="{ name: canViewAnyPayment ? 'viewSystemPayments' : 'viewPayments' }"
    :title="l.navigation.paymentDetails"
    :loading="loading"
  >
    <PaymentDetails :paymentInfo="paymentInfo" />
  </ResponsivePanel>
</template>

<script lang="ts" setup>
import ResponsivePanel from '@/components/common/ResponsivePanel.vue'
import PaymentDetails from '@/components/payment/PaymentDetails.vue'
import { PaymentClient, PriceInfo } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { onBeforeMount, ref } from 'vue'

const props = defineProps<{
  paymentId: number
  canViewAnyPayment?: boolean
}>()

const l = LocaleService.currentLocale
const paymentService = getClient(PaymentClient)

const loading = ref(false)
const paymentInfo = ref<PriceInfo | undefined>()

onBeforeMount(async () => {
  loading.value = true
  if (props.canViewAnyPayment) {
    paymentInfo.value = await paymentService.geSystemPayment(props.paymentId)
  } else {
    paymentInfo.value = await paymentService.getUserPayment(props.paymentId)
  }
  loading.value = false
})
</script>
