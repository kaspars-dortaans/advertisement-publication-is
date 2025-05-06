<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading" class="flex-1 lg:flex-none flex flex-col">
      <Panel class="rounded-none lg:rounded-md flex-1">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageCategories' }" />
            <h4 class="page-title">
              {{ l.navigation.viewCategory }}
            </h4>
            <Button
              v-if="isAllowedToEdit"
              :label="l.actions.edit"
              icon="pi pi-pencil"
              severity="secondary"
              as="RouterLink"
              :to="{ name: 'editCategory', params: { categoryId } }"
            />
          </div>
        </template>

        <div class="flex flex-col">
          <!-- Title -->
          <LocaleTextInput
            v-model="categoryNames"
            :disabled="true"
            :localeList="ls.localeList.value"
            :label="l.form.categoryForm.title"
            :lookups="[]"
          />

          <Divider />

          <!-- Parent category -->
          <FloatLabel variant="on">
            <InputText v-model="categoryInfo.parentCategoryName" :disabled="true" fluid />
            <label for="parent-category-id-input">{{ l.form.categoryForm.parentCategory }}</label>
          </FloatLabel>

          <!-- Can contain advertisements -->
          <div class="inline-flex gap-2 mt-4">
            <ToggleSwitch
              v-model="categoryInfo.canContainAdvertisements"
              :disabled="true"
              id="can-contain-advertisements-input"
            />
            <label for="can-contain-advertisements-input">{{
              l.form.categoryForm.canContainAdvertisements
            }}</label>
          </div>

          <Divider />

          <!-- Attribute list -->
          <OrderableList
            v-model="categoryInfo.categoryAttributeOrder"
            :inputLabel="l.form.categoryForm.addCategoryAttributes"
            :listLabel="l.form.categoryForm.attributeOrderList"
            :disabled="true"
            :lookups="[]"
          />
        </div>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import LocaleTextInput from '@/components/form/LocaleTextInput.vue'
import OrderableList from '@/components/form/OrderableList.vue'
import { Permissions } from '@/constants/api/Permissions'
import { CategoryClient, PutCategoryRequest } from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { computed, onBeforeMount, ref, watch } from 'vue'

const props = defineProps<{
  categoryId: number
}>()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const categoryService = getClient(CategoryClient)

//Reactive data
const loading = ref(0)
const categoryNames = ref<string[]>([])
const categoryInfo = ref<PutCategoryRequest>(
  new PutCategoryRequest({
    id: 0,
    canContainAdvertisements: false,
    categoryAttributeOrder: [],
    localizedNames: [],
    parentCategoryId: 0,
    parentCategoryName: ''
  })
)
const isAllowedToEdit = computed(() => AuthService.hasPermission(Permissions.EditCategory))

//Hooks
onBeforeMount(async () => {
  loadData()
})

//Watchers
watch(LocaleService.currentLocaleName, () => loadData())

//Methods
const loadData = async () => {
  loading.value++
  categoryInfo.value = await categoryService.getCategoryFormInfo(props.categoryId)
  categoryNames.value = ls.localeList.value.map(
    (l) => categoryInfo.value.localizedNames.find((ln) => ln.key === l)?.value ?? ''
  )
  loading.value--
}
</script>
