<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading" class="flex-1 lg:flex-none flex">
      <Panel class="flex-1 flex flex-col rounded-none lg:rounded-md lg:min-w-96">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageAdvertisementNotificationSubscription' }" />
            <h3 class="page-title">
              {{
                isEdit
                  ? l.navigation.editAdvertisementNotificationSubscription
                  : l.navigation.createAdvertisementNotificationSubscription
              }}
            </h3>
          </div>
        </template>

        <form class="flex flex-col gap-3" @submit="submit">
          <div class="flex flex-col lg:flex-row gap-2">
            <fieldset class="flex-1 flex flex-col gap-2">
              <!-- Owner -->
              <FloatLabel v-if="forAnyUser" variant="on">
                <AutoComplete
                  v-model="fields.ownerId!.value"
                  v-bind="fields.ownerId?.attributes"
                  :invalid="fields.ownerId?.hasError"
                  :suggestions="userLookups"
                  inputId="owner-input"
                  optionLabel="value"
                  fluid
                  dropdown
                  @complete="searchUsers"
                />
                <label for="owner-input">{{ l.form.putAdvertisementNotification.owner }}</label>
              </FloatLabel>
              <FieldError :field="fields.ownerId" />

              <!-- Title -->
              <FloatLabel variant="on">
                <InputText
                  v-model="fields.title!.value"
                  v-bind="fields.title?.attributes"
                  :invalid="fields.title!.hasError"
                  id="title-input"
                  fluid
                />
                <label for="title-input">{{ l.form.putAdvertisementNotification.title }}</label>
              </FloatLabel>
              <FieldError :field="fields.title" />

              <!-- Time period -->
              <template v-if="!isEdit">
                <FloatLabel variant="on">
                  <Select
                    v-model="fields.paidTime!.value"
                    v-bind="fields.paidTime?.attributes"
                    :invalid="fields.paidTime?.hasError"
                    :options="timeOptions"
                    optionLabel="name"
                    optionValue="value"
                    id="time-period-select"
                    fluid
                  />
                  <label for="time-period-select">{{
                    l.form.putAdvertisementNotification.timePeriod
                  }}</label>
                </FloatLabel>
                <FieldError :field="fields.paidTime" />
              </template>
              <div v-else-if="existingSubscription?.validToDate" class="flex gap-2">
                <FloatLabel variant="on" class="flex-1">
                  <InputText
                    :defaultValue="dateFormat.format(existingSubscription?.validToDate)"
                    disabled
                    fluid
                  ></InputText>
                  <label for="valid-to">{{ l.form.putAdvertisementNotification.validTo }}</label>
                </FloatLabel>
                <Button
                  :label="l.actions.extend"
                  severity="secondary"
                  as="RouterLink"
                  :to="{
                    name: 'extend',
                    params: {
                      type: PaymentType.ExtendAdvertisementNotificationSubscription,
                      ids: `[${subscriptionId}]`
                    }
                  }"
                />
              </div>

              <!-- Keywords -->
              <FloatLabel variant="on">
                <AutoComplete
                  v-model="fields.keywords!.value"
                  v-bind="fields.keywords?.attributes"
                  :typeahead="false"
                  :invalid="fields.keywords?.hasError"
                  :forceSelection="true"
                  inputId="keyword-input"
                  multiple
                  fluid
                />
                <label for="keyword-input">{{
                  l.form.putAdvertisementNotification.keywords
                }}</label>
              </FloatLabel>
              <FieldError :field="fields.keywords" />

              <Divider />

              <!-- Category -->
              <CategorySelect
                :categoryList="categoryList"
                :field="fields.categoryId"
                @selectedCategory="handleSelectedCategory"
              />
            </fieldset>

            <!-- Attributes -->
            <fieldset v-if="attributeInfo.length" class="flex-1 flex flex-col gap-2">
              <Divider v-if="isSmallScreen" />
              <BlockWithSpinner class="flex flex-col gap-2 min-h-12" :loading="loadingAttributes">
                <AttributeInputGroup
                  :fields="fields"
                  :attributes="attributeInfo"
                  :valueLists="attributeValueLists"
                  fieldKey="attributeValues"
                />
              </BlockWithSpinner>
            </fieldset>
          </div>

          <Button
            :loading="isSubmitting"
            :label="isEdit ? l.actions.save : l.actions.create"
            class="mx-auto"
            @click="submit"
          />
        </form>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import AttributeInputGroup from '@/components/attribute-input/AttributeInputGroup.vue'
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import CategorySelect from '@/components/form/CategorySelect.vue'
import FieldError from '@/components/form/FieldError.vue'
import {
  useManageAttributeInput,
  useValidateAttributeInput
} from '@/composables/manage-attribute-input'
import { usePaymentState } from '@/composables/payment-store'
import { createAdvertisementPostTimeSpanOptions } from '@/constants/advertisement-post-time-span'
import {
  AdvertisementNotificationClient,
  AttributeFormInfo,
  CategoryClient,
  CategoryItem,
  CreateOrEditNotificationSubscriptionRequest,
  Int32StringKeyValuePair,
  NewPaymentItem,
  PaymentType,
  UserClient,
  ValueTypes
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { PutNotificationSubscriptionForm } from '@/types/forms/create-or-edit-notification-subscription'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper } from '@/utils/field-helper'
import { useTrackScreenSize } from '@/utils/screen-size'
import {
  canAddAdvertisementToCategoryValidator,
  requiredWhen
} from '@/validators/custom-validators'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm, type AutoCompleteCompleteEvent } from 'primevue'
import { useForm } from 'vee-validate'
import { computed, onBeforeMount, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { array, number, object, string, type AnyObject } from 'yup'
import { TakeLookupsWhenSearching } from '@/constants/search-constants'

const props = defineProps<{
  subscriptionId?: number | undefined
  forAnyUser?: boolean
}>()

//Route
const { push } = useRouter()
const confirm = useConfirm()
const formSubmitted = ref(false)
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const advertisementNotificationService = getClient(AdvertisementNotificationClient)
const categoryService = getClient(CategoryClient)
const userService = getClient(UserClient)

//Reactive data
const isEdit = computed(() => typeof props.subscriptionId === 'number')
const timeOptions = computed(() => createAdvertisementPostTimeSpanOptions(ls))
const categoryList = ref<CategoryItem[]>([])
const attributeInfo = ref<AttributeFormInfo[]>([])
const existingSubscription = ref<CreateOrEditNotificationSubscriptionRequest | undefined>()
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)
const { isSmallScreen } = useTrackScreenSize()
const paymentState = usePaymentState()
const userLookups = ref<Int32StringKeyValuePair[]>([])

//Form and fields
const { addAttributeValidationSchema } = useValidateAttributeInput(attributeInfo)
const validationSchema = computed(() => {
  let schemaObject: AnyObject = {
    ownerId: object()
      .test(requiredWhen(() => props.forAnyUser))
      .label('form.putAdvertisementNotification.owner'),
    paidTime: object()
      .test(requiredWhen(() => !isEdit.value))
      .label('form.putAdvertisementNotification.timePeriod'),
    title: string().default('').required().label('form.putAdvertisementNotification.title'),
    keywords: array(string()).label('form.putAdvertisementNotification.keywords'),
    categoryId: number().test(canAddAdvertisementToCategoryValidator(categoryList)),
    attributeValues: array().default([])
  }

  addAttributeValidationSchema(schemaObject)
  return toTypedSchema(object(schemaObject))
})

const form = useForm<PutNotificationSubscriptionForm>({ validationSchema })
const fieldHelper = new FieldHelper(form)
const { isSubmitting, handleSubmit, values, resetForm, validate } = form
const { defineMultipleFields, handleErrors, fields, valuesChanged } = fieldHelper
defineMultipleFields(['title', 'ownerId', 'paidTime', 'keywords', 'categoryId', 'attributeValues'])

const {
  loading,
  loadingAttributes,
  attributeValueLists,
  setCategoryInfo,
  loadCategoryInfo,
  loadCategoryList,
  handleSelectedCategory
} = useManageAttributeInput<PutNotificationSubscriptionForm>(
  categoryList,
  attributeInfo,
  categoryService,
  form,
  fieldHelper
)

//Hooks
onBeforeMount(() => {
  if (isEdit.value) {
    loadSubscription()
  } else {
    reloadData()
  }
})

//Watchers
watch(LocaleService.currentLocaleName, async () => {
  await reloadData()
  validate({ mode: 'validated-only' })
})

//Methods
const loadSubscription = async () => {
  loading.value++
  const [{ subscription, categoryInfo }] = await Promise.all([
    props.forAnyUser
      ? advertisementNotificationService.editAnySubscriptionsGet(props.subscriptionId)
      : advertisementNotificationService.editSubscriptionsGet(props.subscriptionId),
    loadCategoryList()
  ])
  setCategoryInfo(categoryInfo)

  existingSubscription.value = subscription
  if (subscription) {
    const attributeValues = attributeInfo.value.map((ai) => {
      const av = subscription.attributeValues?.find((av) => ai.id === av.key)

      if (ai.attributeValueType === ValueTypes.Text) {
        return av?.value ?? ''
      } else {
        return av?.value ? parseInt(av.value) : undefined
      }
    })

    resetForm({
      values: {
        ownerId: new Int32StringKeyValuePair({
          key: subscription.ownerId,
          value: subscription.ownerUsername
        }),
        categoryId: subscription.categoryId,
        keywords: subscription.keywords,
        title: subscription.title,
        attributeValues: attributeValues ?? []
      }
    })
  } else {
    //TODO: Display error
  }
  loading.value--
}

const reloadData = async () => {
  loading.value++
  const promises = []
  promises.push(loadCategoryList())

  //If category info was loaded before, reload it
  if (attributeInfo.value.length) {
    promises.push(loadCategoryInfo(values.categoryId))
  }
  await Promise.all(promises)
  loading.value--
}

const searchUsers = async (e: AutoCompleteCompleteEvent) => {
  userLookups.value = await userService.searchUserLookup(e.query, TakeLookupsWhenSearching)
}

const submit = handleSubmit(async () => {
  try {
    //Transform attributeValues to type accepted by Api
    const attributeValues = values.attributeValues
      .map((av: string | number | undefined, i: number) => {
        if (av == null) {
          return av
        }
        return new Int32StringKeyValuePair({
          key: attributeInfo.value[i].id,
          value: '' + av
        })
      })
      .filter((i) => i != null) as Int32StringKeyValuePair[]

    const request = new CreateOrEditNotificationSubscriptionRequest({
      id: props.subscriptionId,
      ownerId: values.ownerId?.key,
      paidTime: values.paidTime,
      categoryId: values.categoryId,
      title: values.title,
      keywords: values.keywords?.map((k) => k.trim())?.filter((k) => k) ?? [],
      attributeValues: attributeValues
    })

    if (isEdit.value) {
      if (props.forAnyUser) {
        //Edit subscription for any user
        await advertisementNotificationService.editAnySubscriptionsPost(request)
        formSubmitted.value = true
        push({ name: 'manageAdvertisementNotificationSubscription' })
      } else {
        //Edit own subscription
        await advertisementNotificationService.editSubscriptionsPost(request)
        formSubmitted.value = true
        push({ name: 'manageOwnAdvertisementNotificationSubscription' })
      }
    } else {
      if (props.forAnyUser) {
        //Create subscription for any user
        await advertisementNotificationService.createSubscriptionForAnyUser(request)
        formSubmitted.value = true
        push({ name: 'manageAdvertisementNotificationSubscription' })
      } else {
        //Create own subscription
        const id = await advertisementNotificationService.createSubscriptions(request)
        paymentState.value.paymentItems = [
          new NewPaymentItem({
            paymentSubjectId: id,
            type: PaymentType.CreateAdvertisementNotificationSubscription,
            timePeriod: values.paidTime!
          })
        ]
        formSubmitted.value = true
        push({ name: 'makePayment' })
      }
    }
  } catch (error) {
    handleErrors(error)
  }
})
</script>
