<template>
  <ResponsivePanel :defaultBackButtonRoute="{ name: 'home' }" :title="title" :loading="loading">
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
          v-model="fields.extendTime!.value"
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
        <BackButton :label="l.actions.cancel" icon="" :default-to="{ name: 'home' }" />
        <Button type="submit" :loading="isSubmitting" :label="actionText" />
      </div>
    </form>
  </ResponsivePanel>
</template>

<script lang="ts" setup>
import BackButton from '@/components/common/BackButton.vue'
import ResponsivePanel from '@/components/common/ResponsivePanel.vue'
import FieldError from '@/components/form/FieldError.vue'
import { usePaymentState } from '@/composables/payment-store'
import { createAdvertisementPostTimeSpanOptions } from '@/constants/advertisement-post-time-span'
import { Permissions } from '@/constants/api/Permissions'
import {
  AdvertisementClient,
  AdvertisementNotificationClient,
  ExtendRequest,
  Int32StringKeyValuePair,
  NewPaymentItem,
  PaymentType,
  PostTimeDto
} from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useForm } from 'vee-validate'
import { computed, onBeforeMount, ref, watch } from 'vue'
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
        .label('form.extend.timePeriod')
    })
  )
})
const { fields, handleErrors, defineField } = new FieldHelper(form)
const { handleSubmit, isSubmitting, values, validate } = form
defineField('extendTime')

//Hooks
onBeforeMount(async () => {
  loading.value = true

  if (
    props.type === PaymentType.CreateAdvertisement ||
    props.type === PaymentType.ExtendAdvertisement
  ) {
    extendItems.value = await advertisementService.getAdvertisementLookupByIds(props.ids)
  } else {
    if (AuthService.hasPermission(Permissions.ViewAllAdvertisementNotificationSubscriptions)) {
      extendItems.value = await advertisementNotificationService.getAllSubscriptionsLookupByIds(
        props.ids
      )
    } else {
      extendItems.value = await advertisementNotificationService.getSubscriptionsLookupByIds(
        props.ids
      )
    }
  }

  setPageText()
  loading.value = false
})

//Watcher
watch(LocaleService.currentLocaleName, () => {
  validate({ mode: 'validated-only' })
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
    if (
      (AuthService.hasPermission(Permissions.CreateAdvertisement) &&
        props.type === PaymentType.CreateAdvertisement) ||
      (AuthService.hasPermission(Permissions.EditAnyAdvertisement) &&
        props.type === PaymentType.ExtendAdvertisement)
    ) {
      await advertisementService.extendAdvertisements(
        new ExtendRequest({
          extendTime: new PostTimeDto(values.extendTime),
          ids: props.ids
        })
      )
      push({ name: 'manageAdvertisements' })
    } else if (
      (AuthService.hasPermission(Permissions.CreateAdvertisementNotificationSubscription) &&
        props.type === PaymentType.CreateAdvertisementNotificationSubscription) ||
      (AuthService.hasPermission(Permissions.EditAnyAdvertisementNotificationSubscription) &&
        props.type === PaymentType.ExtendAdvertisementNotificationSubscription)
    ) {
      await advertisementNotificationService.extendSubscription(
        new ExtendRequest({
          extendTime: new PostTimeDto(values.extendTime),
          ids: props.ids
        })
      )
      push({ name: 'manageAdvertisementNotificationSubscription' })
    } else {
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
    }
  } catch (e) {
    handleErrors(e)
  }
})
</script>
