<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading > 0" class="flex-1 lg:flex-none flex">
      <Panel class="flex-1 flex flex-col rounded-none lg:rounded-md lg:min-w-96">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageAdvertisementNotificationSubscription' }" />
            <h3 class="page-title">{{ l.navigation.advertisementNotifications }}</h3>
          </div>
        </template>

        <form class="flex flex-col gap-3" @submit="submit">
          <div class="flex flex-col lg:flex-row gap-2">
            <fieldset class="flex-1 flex flex-col gap-2">
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

              <!-- Time period -->
              <div v-if="isEdit" class="flex gap-2">
                <FloatLabel variant="on" class="flex-1">
                  <InputText
                    :defaultValue="dateFormat.format(existingSubscription?.validTo)"
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
                    params: { type: 'notificationSubscription', ids: `[${subscriptionId}]` }
                  }"
                />
              </div>
              <template v-else>
                <FloatLabel variant="on">
                  <Select
                    v-model="fields.paidTime!.model"
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

              <!-- Keywords -->
              <FloatLabel variant="on">
                <AutoComplete
                  v-model="fields.keywords!.model"
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
            type="submit"
            class="mx-auto"
          />
        </form>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import AttributeInputGroup from '@/components/attribute-input/AttributeInputGroup.vue'
import BackButton from '@/components/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import CategorySelect from '@/components/form/CategorySelect.vue'
import FieldError from '@/components/form/FieldError.vue'
import {
  useManageAttributeInput,
  useValidateAttributeInput
} from '@/composables/manage-attribute-input'
import { createAdvertisementPostTimeSpanOptions } from '@/constants/advertisement-post-time-span'
import {
  AdvertisementClient,
  AdvertisementNotificationClient,
  AttributeFormInfo,
  CategoryItem,
  CreateOrEditNotificationSubscriptionRequest,
  Int32StringKeyValuePair,
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
import { useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { computed, onBeforeMount, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { array, number, object, string, type AnyObject } from 'yup'

const props = defineProps<{
  subscriptionId?: number | undefined
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
const advertisementService = getClient(AdvertisementClient)

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

//Form and fields
const { addAttributeValidationSchema } = useValidateAttributeInput(attributeInfo)
const validationSchema = computed(() => {
  let schemaObject: AnyObject = {
    paidTime: object().test(requiredWhen(() => !isEdit.value)),
    title: string().default('').required(),
    keyword: array(string()).default([]),
    categoryId: number().test(canAddAdvertisementToCategoryValidator(categoryList)),
    attributeValues: array().default([])
  }

  addAttributeValidationSchema(schemaObject)
  return toTypedSchema(object(schemaObject))
})

const form = useForm<PutNotificationSubscriptionForm>({ validationSchema })
const fieldHelper = new FieldHelper(form)
const { isSubmitting, handleSubmit, values, resetForm } = form
const { defineMultipleFields, handleErrors, fields, valuesChanged } = fieldHelper
defineMultipleFields(['title', 'paidTime', 'keywords', 'categoryId', 'attributeValues'])

const {
  loading,
  loadingAttributes,
  attributeValueLists,
  setCategoryInfo,
  loadCategoryInfo,
  loadCategoryList,
  handleSelectedCategory
} = useManageAttributeInput<PutNotificationSubscriptionForm>(
  form,
  fieldHelper,
  categoryList,
  attributeInfo,
  advertisementService
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
watch(LocaleService.currentLocaleName, () => {
  reloadData()
})

//Methods
const loadSubscription = async () => {
  loading.value++
  const [{ subscription, categoryInfo }] = await Promise.all([
    advertisementNotificationService.editSubscriptionsGet(props.subscriptionId),
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
        categoryId: subscription.categoryId,
        keywords: subscription.keywords?.split(','),
        title: subscription.title,
        attributeValues: attributeValues ?? []
      }
    })
  } else {
    //TODO: Display error
  }
  loading.value--
}

const reloadData = () => {
  loading.value++
  loadCategoryList()
  //If category info was loaded before, reload it
  if (attributeInfo.value.length) {
    loadCategoryInfo(values.categoryId)
  }
  loading.value--
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
      .filter((i) => i != null)

    const request = new CreateOrEditNotificationSubscriptionRequest({
      id: props.subscriptionId,
      categoryId: values.categoryId,
      paidTime: values.paidTime,
      title: values.title,
      keywords: values.keywords
        .map((k) => k.trim())
        .filter((k) => k)
        .join(','),
      attributeValues: attributeValues
    })

    if (isEdit.value) {
      await advertisementNotificationService.editSubscriptionsPost(request)
    } else {
      await advertisementNotificationService.createSubscriptions(request)
    }
    formSubmitted.value = true
    push({ name: 'manageAdvertisementNotificationSubscription' })
  } catch (error) {
    handleErrors(error)
  }
})
</script>
