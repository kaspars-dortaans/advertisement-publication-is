<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading || isSubmitting" class="flex-1 lg:flex-none flex flex-col">
      <Panel class="rounded-none lg:rounded-md flex-1">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageCategories' }" />
            <h4 class="page-title">
              {{ isEdit ? l.navigation.editCategory : l.navigation.createCategory }}
            </h4>
          </div>
        </template>

        <form class="flex flex-col" @submit="submit">
          <FieldError :messages="formErrors" />

          <!-- Title -->
          <LocaleTextInput
            v-model="fields.localizedNames!.value"
            v-bind="fields.localizedNames?.attributes"
            :invalid="fields.localizedNames?.hasError"
            :localeList="ls.localeList.value"
            :label="l.form.categoryForm.title"
          />
          <FieldError :field="fields.localizedNames" />

          <Divider />

          <!-- Parent category -->
          <FloatLabel variant="on">
            <AutoComplete
              v-model="fields.parentCategory!.value"
              v-bind="fields.parentCategory!.attributes"
              :invalid="fields.parentCategory!.hasError"
              :suggestions="suggestedLookups"
              optionLabel="value"
              id="parent-category-id-input"
              fluid
              dropdown
              @complete="searchCategories"
            />
            <label for="parent-category-id-input">{{ l.form.categoryForm.parentCategory }}</label>
          </FloatLabel>
          <FieldError :field="fields.parentCategory" />

          <!-- Can contain advertisements -->
          <div class="inline-flex gap-2 mt-4">
            <ToggleSwitch
              v-model="fields.canContainAdvertisements!.value"
              v-bind="fields.canContainAdvertisements!.attributes"
              :invalid="fields.canContainAdvertisements!.hasError"
              id="can-contain-advertisements-input"
            />
            <label for="can-contain-advertisements-input">{{
              l.form.categoryForm.canContainAdvertisements
            }}</label>
          </div>
          <FieldError :field="fields.canContainAdvertisements" />

          <Divider />

          <!-- Attribute list -->
          <OrderableList
            v-model="fields.categoryAttributeOrder!.value"
            v-bind="fields.categoryAttributeOrder?.attributes"
            :invalid="fields.categoryAttributeOrder?.hasError"
            :lookups="attributeLookup"
            :inputLabel="l.form.categoryForm.addCategoryAttributes"
            :listLabel="l.form.categoryForm.attributeOrderList"
          />

          <Button :label="isEdit ? l.actions.save : l.actions.create" type="submit" class="mt-3" />
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
import LocaleTextInput from '@/components/form/LocaleTextInput.vue'
import OrderableList from '@/components/form/OrderableList.vue'
import {
  CategoryClient,
  Int32StringKeyValuePair,
  PutCategoryRequest,
  StringStringKeyValuePair
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { CategoryForm } from '@/types/forms/category-form'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm, type AutoCompleteCompleteEvent } from 'primevue'
import { useForm } from 'vee-validate'
import { computed, onBeforeMount, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { array, bool, object } from 'yup'

const props = defineProps<{
  parentCategoryId?: number
  categoryId?: number
}>()

//Route
const { push } = useRouter()
const formSubmitted = ref(false)
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const categoryService = getClient(CategoryClient)
const confirm = useConfirm()

//Reactive data
const loading = ref(0)
const isEdit = computed(() => typeof props.categoryId === 'number')
const categoryLookup = ref<Int32StringKeyValuePair[]>([])
const suggestedLookups = ref<Int32StringKeyValuePair[]>([])
const attributeLookup = ref<Int32StringKeyValuePair[]>([])

//Forms and fields
const form = useForm<CategoryForm>({
  validationSchema: toTypedSchema(
    object({
      canContainAdvertisements: bool().default(true),
      localizedNames: array().default([]),
      categoryAttributeOrder: array().default([])
    })
  )
})
const { fields, formErrors, valuesChanged, defineMultipleFields, handleErrors } = new FieldHelper(
  form
)
const { handleSubmit, values, isSubmitting, resetForm } = form
defineMultipleFields([
  'canContainAdvertisements',
  'parentCategory',
  'localizedNames',
  'categoryAttributeOrder'
])

//Hooks
onBeforeMount(async () => {
  if (isEdit.value) {
    loadCategoryFormInfo()
  } else {
    loadLookups()
  }
})

//Watchers
watch(LocaleService.currentLocaleName, () => loadLookups())

//Methods
const loadLookups = async () => {
  loading.value++
  const [cLookup, aLookup] = await Promise.all([
    categoryService.getCategoryLookup(),
    categoryService.getAttributeLookup()
  ])
  categoryLookup.value = cLookup
  attributeLookup.value = aLookup

  if (props.parentCategoryId) {
    const parentCategoryLookup = categoryLookup.value.find(
      (cl) => cl.key === props.parentCategoryId
    )
    if (parentCategoryLookup) {
      resetForm({
        values: {
          canContainAdvertisements: true,
          categoryAttributeOrder: [],
          localizedNames: [],
          parentCategory: parentCategoryLookup
        }
      })
    }
  }
  loading.value--
}

const loadCategoryFormInfo = async () => {
  loading.value++
  const [formInfo] = await Promise.all([
    categoryService.getCategoryFormInfo(props.categoryId),
    loadLookups()
  ])

  const parentCategoryLookup = categoryLookup.value.find(
    (cl) => cl.key === formInfo.parentCategoryId
  )
  resetForm({
    values: {
      id: formInfo.id,
      canContainAdvertisements: formInfo.canContainAdvertisements,
      parentCategory: parentCategoryLookup,
      categoryAttributeOrder: formInfo.categoryAttributeOrder,
      localizedNames: ls.localeList.value.map(
        (l) => formInfo.localizedNames.find((ln) => ln.key === l)?.value ?? ''
      )
    }
  })
  loading.value--
}

const searchCategories = (e: AutoCompleteCompleteEvent) => {
  const searchStrLowercase = e.query.toLocaleLowerCase()
  suggestedLookups.value = categoryLookup.value.filter(
    (l) => l.value!.toLocaleLowerCase().indexOf(searchStrLowercase) > -1
  )
}

const submit = handleSubmit(async () => {
  try {
    const localizedNames = values.localizedNames.map(
      (ln, i) =>
        new StringStringKeyValuePair({
          key: ls.localeList.value[i],
          value: ln
        })
    )

    const request = new PutCategoryRequest({
      id: props.categoryId,
      canContainAdvertisements: values.canContainAdvertisements,
      parentCategoryId: values.parentCategory?.key,
      localizedNames: localizedNames,
      categoryAttributeOrder: values.categoryAttributeOrder
    })

    if (isEdit.value) {
      await categoryService.editCategory(request)
    } else {
      await categoryService.createCategory(request)
    }
    formSubmitted.value = true
    push({ name: 'manageCategories' })
  } catch (e) {
    handleErrors(e)
  }
})
</script>
