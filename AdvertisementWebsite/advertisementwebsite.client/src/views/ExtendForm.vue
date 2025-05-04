<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading">
      <Panel class="rounded-none flex-1 lg:flex-none lg:rounded-md">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'home' }" />
            <h3 class="page-title">{{ title }}</h3>
          </div>
        </template>
        <form class="flex gap-3 flex-col" @submit="submit">
          <Panel v-if="extendItems?.length" toggleable>
            <template #header>
              <p>
                {{ text }}
              </p>
            </template>
            <ul class="list-disc pl-4">
              <li v-for="item in extendItems" :key="item.key">
                {{ item.value }}
              </li>
            </ul>
          </Panel>
          <FloatLabel variant="on">
            <Select
              v-model="fields.extendTime!.model"
              v-bind="fields.extendTime?.attributes"
              :options="timeOptions"
              :invalid="fields.extendTime?.hasError"
              optionLabel="name"
              optionValue="value"
              id="time-input"
              fluid
            />
            <label for="time-input">{{ l.form.extend.timePeriod }}</label>
          </FloatLabel>
          <FieldError :field="fields.extendTime" />

          <div class="flex flex-row gap-2 justify-center">
            <BackButton
              :label="l.actions.cancel"
              icon=""
              :default-to="{ name: 'manageAdvertisements' }"
            />
            <Button type="submit" :loading="isSubmitting" :label="actionText" />
          </div>
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
import { usePaymentState } from '@/composables/payment-store'
import { createAdvertisementPostTimeSpanOptions } from '@/constants/advertisement-post-time-span'
import {
  AdvertisementClient,
  AdvertisementNotificationClient,
  Int32StringKeyValuePair,
  NewPaymentItem,
  PaymentType,
  PostTimeDto
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useForm } from 'vee-validate'
import { computed, onBeforeMount, ref } from 'vue'
import { useRouter } from 'vue-router'
import { number, object } from 'yup'

//Route
const { push } = useRouter()

//Props
const props = defineProps<{
  ids: number[]
  type: PaymentType
}>()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const advertisementService = getClient(AdvertisementClient)
const advertisementNotificationService = getClient(AdvertisementNotificationClient)

//Reactive data
const timeOptions = computed(() => {
  return createAdvertisementPostTimeSpanOptions(ls)
})
const loading = ref(false)
const extendItems = ref<Int32StringKeyValuePair[]>()
const paymentState = usePaymentState()
const text = ref('')
const title = ref('')
const actionText = ref('')

//Forms and fields
const form = useForm({
  validationSchema: toTypedSchema(
    object({
      extendTime: object({
        days: number(),
        weeks: number(),
        months: number()
      })
        .default(undefined)
        .required()
    })
  )
})
const { fields, handleErrors, defineField } = new FieldHelper(form)
const { handleSubmit, isSubmitting, values } = form
defineField('extendTime')

//Hooks
onBeforeMount(async () => {
  loading.value = true
  setPageText()

  if (
    props.type === PaymentType.CreateAdvertisement ||
    props.type === PaymentType.ExtendAdvertisement
  ) {
    extendItems.value = await advertisementService.getAdvertisementLookupByIds(props.ids)
  } else {
    extendItems.value = await advertisementNotificationService.getSubscriptionsLookupByIds(
      props.ids
    )
  }

  loading.value = false
})

//Methods
const setPageText = () => {
  const length = extendItems.value?.length ?? 0
  title.value = ls.l('paymentType.' + props.type)
  switch (props.type) {
    case PaymentType.CreateAdvertisement:
      text.value = ls.l('form.extend.createText', ls.l('form.extend.advertisement'), length)
      actionText.value = ls.l('actions.create')
      break
    case PaymentType.ExtendAdvertisement:
      text.value = ls.l('form.extend.extendText', ls.l('form.extend.advertisement'), length)
      actionText.value = ls.l('actions.extend')
      break
    case PaymentType.CreateAdvertisementNotificationSubscription:
      text.value = ls.l(
        'form.extend.createText',
        ls.l('form.extend.advertisementNotificationSubscription'),
        length
      )
      actionText.value = ls.l('actions.create')
      break
    case PaymentType.ExtendAdvertisementNotificationSubscription:
      text.value = ls.l(
        'form.extend.extendText',
        ls.l('form.extend.advertisementNotificationSubscription'),
        length
      )
      actionText.value = ls.l('actions.extend')
      break
  }
}

const submit = handleSubmit(async () => {
  try {
    const timePeriod = new PostTimeDto(values.extendTime!)
    paymentState.value.paymentItems = extendItems.value!.map(
      (i) =>
        new NewPaymentItem({
          paymentSubjectId: i.key!,
          timePeriod,
          type: props.type
        })
    )

    push({ name: 'makePayment' })
  } catch (e) {
    handleErrors(e)
  }
})
</script>
