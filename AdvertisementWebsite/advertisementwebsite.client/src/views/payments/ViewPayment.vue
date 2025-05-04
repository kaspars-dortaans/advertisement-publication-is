<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading" class="flex-1 lg:flex-none flex flex-col">
      <Panel class="flex-1 rounded-none lg:rounded-md">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'viewPayments' }" />
            <h3 class="page-title">{{ l.navigation.paymentDetails }}</h3>
          </div>
        </template>

        <PaymentDetails :paymentInfo="paymentInfo" />
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import PaymentDetails from '@/components/payment/PaymentDetails.vue'
import { PaymentClient, PriceInfo } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { onBeforeMount, ref } from 'vue'

const props = defineProps<{
  paymentId: number
}>()

const l = LocaleService.currentLocale
const paymentService = getClient(PaymentClient)

const loading = ref(false)
const paymentInfo = ref<PriceInfo | undefined>()

onBeforeMount(async () => {
  loading.value = true
  paymentInfo.value = await paymentService.getUserPayment(props.paymentId)
  loading.value = false
})
</script>
