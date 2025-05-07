<template>
  <ResponsiveLayout>
    <BlockWithSpinner class="flex-1 lg:flex-none flex flex-col" :loading="loading">
      <div
        class="flex-1 flex flex-col gap-[1.125rem] p-[1.125rem] rounded-none lg:rounded-md bg-surface-0"
      >
        <h3 class="page-title">{{ l.navigation.makePayment }}</h3>
        <div class="flex-1 flex flex-col lg:flex-row items-center lg:items-stretch gap-3">
          <Message v-if="formErrors" severity="error">{{ formErrors }}</Message>
          <PaymentDetails :paymentInfo="priceInfo" />

          <form class="flex flex-col space-y-2" @submit="pay">
            <h4 class="font-semibold">{{ l.makePayment.paymentOptions }}</h4>
            <Listbox
              v-model="fields.paymentOption!.value"
              v-bind="fields.paymentOption?.attributes"
              :invalid="fields.paymentOption?.hasError"
              :options="paymentOptions"
              :pt="{ list: 'grid grid-cols-[auto_1fr]', option: 'rounded-md' }"
              optionValue="key"
            >
              <template #option="slotProps">
                <img
                  :src="slotProps.option.value"
                  width="200"
                  height="100"
                  class="rounded-md bg-white"
                />
              </template>
            </Listbox>
            <FieldError :field="fields.paymentOption" />
            <Button
              :label="l.actions.pay"
              :disabled="loading || !paymentState.paymentItems.length"
              :loading="isSubmitting"
              class="mx-auto"
              type="submit"
            />
          </form>
        </div>
      </div>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import FieldError from '@/components/form/FieldError.vue'
import PaymentDetails from '@/components/payment/PaymentDetails.vue'
import { usePaymentState } from '@/composables/payment-store'
import {
  Int32StringKeyValuePair,
  MakePaymentRequest,
  NewPaymentItem,
  PaymentClient,
  PaymentType,
  PriceInfo
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useForm } from 'vee-validate'
import { onBeforeMount, ref } from 'vue'
import { useRouter } from 'vue-router'
import { number, object } from 'yup'

//NOTE: Currently page is mockup, no payments can be made

//Services
const l = LocaleService.currentLocale
const paymentService = getClient(PaymentClient)
const { push } = useRouter()

//Reactive data
const loading = ref(false)
const paymentState = usePaymentState()
const priceInfo = ref<PriceInfo | undefined>()

//Payment option mockup data
const paymentOptions = ref([
  new Int32StringKeyValuePair({ key: 0, value: '/src/assets/images/swedbank-logo.png' }),
  new Int32StringKeyValuePair({ key: 1, value: '/src/assets/images/citadele-logo.png' }),
  new Int32StringKeyValuePair({ key: 2, value: '/src/assets/images/seb-logo.jpg' })
])

//Forms and fields
const form = useForm({
  validationSchema: toTypedSchema(
    object({
      paymentOption: number().required()
    })
  )
})
const { isSubmitting, handleSubmit } = form
const { handleErrors, defineField, fields, formErrors } = new FieldHelper(form)
defineField('paymentOption')

//Hooks
onBeforeMount(() => {
  loadData()
})

//Methods
const loadData = async () => {
  if (paymentState.value.paymentItems.length) {
    loading.value = true
    priceInfo.value = await paymentService.calculatePrices(paymentState.value.paymentItems)
    loading.value = false
  }
}

const pay = handleSubmit(async () => {
  if (typeof priceInfo.value?.totalAmount !== 'number' || !priceInfo.value.items) {
    return
  }

  try {
    await paymentService.makePayment(
      new MakePaymentRequest({
        totalAmountConfirmation: priceInfo.value?.totalAmount,
        paymentItems: priceInfo.value.items.map(
          (i) =>
            new NewPaymentItem({
              paymentSubjectId: i.paymentSubjectId!,
              type: i.type!,
              timePeriod: i.timePeriod!
            })
        )
      })
    )

    let isPreviousAdvertisement = null
    let mixedItemType = false
    for (const item of priceInfo.value.items!) {
      let isAdvertisement =
        item.type === PaymentType.CreateAdvertisement ||
        item.type === PaymentType.ExtendAdvertisement
      if (isPreviousAdvertisement != null) {
        if (isPreviousAdvertisement !== isAdvertisement) {
          mixedItemType = true
          break
        }
      } else {
        isPreviousAdvertisement = isAdvertisement
      }
    }
    paymentState.value = { paymentItems: [] }
    push({
      name: mixedItemType
        ? 'home'
        : isPreviousAdvertisement
          ? 'manageAdvertisements'
          : 'manageAdvertisementNotificationSubscription'
    })
  } catch (e) {
    handleErrors(e)
  }
})
</script>
