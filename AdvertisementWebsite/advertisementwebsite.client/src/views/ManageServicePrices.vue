<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading || isSubmitting" class="flex-1 lg:flex-none flex flex-col">
      <Panel class="rounded-none lg:rounded-md flex-1">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'home' }" />
            <h4 class="page-title">
              {{ l.navigation.manageServicePrices }}
            </h4>
          </div>
        </template>

        <form class="flex flex-col gap-4" @submit="submit">
          <FieldError :messages="formErrors" />

          <template v-for="field in fields" :key="field!.path">
            <FloatLabel variant="on">
              <InputNumber
                v-model="field!.value"
                v-bind="field!.attributes"
                :invalid="field!.hasError"
                :id="field!.path + '-input'"
                mode="currency"
                currency="EUR"
                :locale="LocaleService.currentLocaleName.value"
                fluid
              />
              <label :for="field!.path + '-input'">{{ l.costType[field!.path] }}</label>
            </FloatLabel>
            <FieldError :field="field" />
          </template>

          <Button :label="l.actions.save" type="submit" class="mt-3 lg:self-center" />
        </form>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import FieldError from '@/components/form/FieldError.vue'
import { PaymentClient, Prices, SetServicePricesRequest, type IPrices } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { onBeforeMount, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { number, object } from 'yup'

//Route
const { push } = useRouter()
const formSubmitted = ref(false)
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Services
const l = LocaleService.currentLocale
const confirm = useConfirm()
const paymentService = getClient(PaymentClient)

//Reactive data
const loading = ref(0)

//Forms and fields
const form = useForm<IPrices>({
  validationSchema: toTypedSchema(
    object({
      createAdvertisement: number().required().label('costType.createAdvertisement'),
      advertisementPerDay: number().required().label('costType.advertisementPerDay'),
      createAdvertisementNotificationSubscription: number()
        .required()
        .label('costType.createAdvertisementNotificationSubscription'),
      subscriptionPerDay: number().required().label('costType.subscriptionPerDay')
    })
  )
})
const { fields, formErrors, valuesChanged, defineMultipleFields, handleErrors } = new FieldHelper(
  form
)
const { handleSubmit, values, isSubmitting, resetForm, validate } = form
defineMultipleFields([
  'createAdvertisement',
  'advertisementPerDay',
  'createAdvertisementNotificationSubscription',
  'subscriptionPerDay'
])

//Hooks
onBeforeMount(async () => {
  reloadData()
})

//Watchers
watch(LocaleService.currentLocaleName, async () => {
  validate({ mode: 'validated-only' })
})

//Methods
const reloadData = async () => {
  loading.value++
  resetForm({
    values: await paymentService.getServicePrices()
  })
  loading.value--
}

const submit = handleSubmit(async () => {
  try {
    const prices: Prices = new Prices(values)
    await paymentService.setServicePrices(new SetServicePricesRequest({ prices }))

    formSubmitted.value = true
    push({ name: 'home' })
  } catch (e) {
    handleErrors(e)
  }
})
</script>
