<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading" class="flex-1 lg:flex-none flex">
      <Panel class="flex-1 flex flex-col rounded-none lg:rounded-md lg:min-w-96">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageAdvertisementNotificationSubscription' }" />
            <h3 class="page-title">{{ l.navigation.advertisementNotifications }}</h3>
            <Button
              v-if="isAllowedToEdit"
              :label="l.actions.edit"
              icon="pi pi-pencil"
              severity="secondary"
              as="RouterLink"
              :to="{ name: 'editAdvertisementNotificationSubscription' }"
            />
          </div>
        </template>

        <div class="flex flex-col gap-3">
          <div class="flex flex-col lg:flex-row gap-2">
            <fieldset class="flex-1 flex flex-col gap-2">
              <!-- Title -->
              <FloatLabel variant="on">
                <InputText v-model="existingSubscription.title" id="title-input" fluid disabled />
                <label for="title-input">{{ l.form.putAdvertisementNotification.title }}</label>
              </FloatLabel>

              <!-- Time period -->
              <FloatLabel variant="on">
                <InputText v-model="validToDate" id="time-period-select" fluid disabled />
                <label for="time-period-select">{{
                  l.form.putAdvertisementNotification.validTo
                }}</label>
              </FloatLabel>

              <!-- Keywords -->
              <FloatLabel variant="on">
                <AutoComplete
                  v-model="existingSubscription.keywords"
                  :typeahead="false"
                  inputId="keyword-input"
                  multiple
                  fluid
                  disabled
                />
                <label for="keyword-input">{{
                  l.form.putAdvertisementNotification.keywords
                }}</label>
              </FloatLabel>

              <Divider />

              <!-- Category -->
              <CategorySelect
                :categoryList="categoryList"
                :value="existingSubscription.categoryId"
                disabled
              />
            </fieldset>

            <!-- Attributes -->
            <fieldset v-if="attributeInfo.length" class="flex-1 flex flex-col gap-2">
              <Divider v-if="isSmallScreen" />
              <div class="flex flex-col gap-2 min-h-12">
                <AttributeInputGroup
                  :values="attributeValues"
                  :attributes="attributeInfo"
                  :valueLists="attributeValueLists"
                  disabled
                />
              </div>
            </fieldset>
          </div>
        </div>
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
import { useManageAttributeInput } from '@/composables/manage-attribute-input'
import { Permissions } from '@/constants/api/Permissions'
import {
  AdvertisementNotificationClient,
  AttributeFormInfo,
  CategoryClient,
  CategoryItem,
  CreateOrEditNotificationSubscriptionRequest,
  PostTimeDto,
  ValueTypes
} from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import type { PutNotificationSubscriptionForm } from '@/types/forms/create-or-edit-notification-subscription'
import { getClient } from '@/utils/client-builder'
import { useTrackScreenSize } from '@/utils/screen-size'
import { computed, onBeforeMount, ref, watch } from 'vue'

const props = defineProps<{
  subscriptionId: number
}>()

//Services
const l = LocaleService.currentLocale
const advertisementNotificationService = getClient(AdvertisementNotificationClient)
const categoryService = getClient(CategoryClient)

//Reactive data
const attributeInfo = ref<AttributeFormInfo[]>([])
const existingSubscription = ref<CreateOrEditNotificationSubscriptionRequest>(
  new CreateOrEditNotificationSubscriptionRequest({
    categoryId: 0,
    title: '',
    attributeValues: [],
    id: 0,
    keywords: [],
    paidTime: new PostTimeDto({ days: 0 }),
    validToDate: new Date()
  })
)
const validToDate = computed(() => dateFormat.value.format(existingSubscription.value.validToDate))
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)
const { isSmallScreen } = useTrackScreenSize()
const isAllowedToEdit = computed(() =>
  AuthService.hasPermission(Permissions.EditOwnedAdvertisementNotificationSubscriptions)
)
const categoryList = ref<CategoryItem[]>([])
const attributeValues = ref<(string | number | undefined)[]>([])

const { loading, attributeValueLists, setCategoryInfo, loadCategoryList } =
  useManageAttributeInput<PutNotificationSubscriptionForm>(
    categoryList,
    attributeInfo,
    categoryService
  )

//Hooks
onBeforeMount(() => {
  reloadData()
})

//Watchers
watch(LocaleService.currentLocaleName, () => {
  reloadData()
})

//Methods
const reloadData = async () => {
  loading.value++
  const [{ subscription, categoryInfo }] = await Promise.all([
    advertisementNotificationService.editSubscriptionsGet(props.subscriptionId),
    loadCategoryList()
  ])
  setCategoryInfo(categoryInfo)

  if (subscription) {
    existingSubscription.value = subscription
    attributeValues.value = attributeInfo.value.map((ai) => {
      const av = subscription.attributeValues?.find((av) => ai.id === av.key)

      if (ai.attributeValueType === ValueTypes.Text) {
        return av?.value ?? ''
      } else {
        return av?.value ? parseInt(av.value) : undefined
      }
    })
  }
  loading.value--
}
</script>
