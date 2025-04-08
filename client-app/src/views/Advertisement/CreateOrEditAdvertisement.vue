<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading > 0 || isSubmitting">
      <Panel>
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: isEdit ? 'viewAdvertisement' : 'home' }" />
            <h3 class="page-title">
              {{ isEdit ? l.navigation.editAdvertisement : l.navigation.createAdvertisement }}
            </h3>
          </div>
        </template>

        <form class="flex flex-col gap-2" @submit="submit">
          <div class="flex flex-col lg:flex-row gap-3">
            <fieldset class="flex flex-col gap-2 w-full md:min-w-60">
              <!-- Time period -->
              <div v-if="isEdit" class="flex gap-2">
                <FloatLabel variant="on" class="flex-1">
                  <InputText
                    :defaultValue="dateFormat.format(existingAdvertisement?.validTo)"
                    disabled
                    fluid
                  ></InputText>
                  <label for="valid-to">{{ l.form.putAdvertisement.validTo }}</label>
                </FloatLabel>
                <Button :label="l.actions.extend" severity="secondary" @click="todo" />
              </div>
              <template v-else>
                <FloatLabel variant="on">
                  <Select
                    v-model="fields.postTime!.model"
                    v-bind="fields.postTime?.attributes"
                    :invalid="fields.postTime?.hasError"
                    :options="postTimeOptions"
                    optionLabel="name"
                    optionValue="value"
                    id="time-period-select"
                    fluid
                  />
                  <label for="time-period-select">{{ l.form.putAdvertisement.timePeriod }}</label>
                </FloatLabel>
                <FieldError :field="fields.postTime" />
              </template>

              <Divider />

              <!-- Category Select -->
              <template v-for="(categoryOptions, i) in categorySelectOptions" :key="i">
                <FloatLabel variant="on">
                  <Select
                    :defaultValue="selectedCategories[i]"
                    :options="categoryOptions"
                    :invalid="fields.categoryId?.hasError && i === categorySelectOptions.length - 1"
                    :id="'category-select-' + i"
                    optionLabel="name"
                    optionValue="id"
                    fluid
                    @change="selectCategory($event, i)"
                  />
                  <label :for="'category-select-' + i">{{
                    i == 0 ? l.form.putAdvertisement.category : l.form.putAdvertisement.subcategory
                  }}</label>
                </FloatLabel>
                <FieldError
                  v-if="i === categorySelectOptions.length - 1"
                  :field="fields.categoryId"
                />
              </template>

              <template v-if="attributeInfo.length || loadingAttributes">
                <Divider />

                <!-- Attributes -->
                <BlockWithSpinner class="flex flex-col gap-2 min-h-12" :loading="loadingAttributes">
                  <AttributeInputGroup
                    :fields="fields"
                    :attributes="attributeInfo"
                    :valueLists="attributeValueLists"
                    fieldKey="attributeValues"
                  />
                </BlockWithSpinner>
              </template>
            </fieldset>

            <fieldset class="flex flex-col gap-2 w-full md:min-w-96">
              <!-- Title -->
              <FloatLabel variant="on">
                <InputText
                  v-model="fields.title!.value"
                  v-bind="fields.title?.attributes"
                  :invalid="fields.title!.hasError"
                  id="title-input"
                  fluid
                />
                <label for="title-input">{{ l.form.putAdvertisement.title }}</label>
              </FloatLabel>
              <FieldError :field="fields.title" />

              <!-- Description -->
              <FloatLabel variant="on">
                <Textarea
                  v-model="fields.description!.value"
                  v-bind="fields.description?.attributes"
                  :invalid="fields.description?.hasError"
                  :rows="10"
                  id="description-input"
                  autoResize
                  fluid
                />
                <label for="description-input">{{ l.form.putAdvertisement.description }}</label>
              </FloatLabel>
              <FieldError :field="fields.description" />

              <!-- Images -->
            </fieldset>
            <fieldset class="md:min-w-96">
              <MultipleImageUpload
                v-model="fields.imagesToAdd!.model.value"
                :maxImageCount="ImageConstants.MaxImageCountPerAdvertisement"
                :accept="ImageConstants.AllowedFileTypes"
                :maxFileSizeInBytes="ImageConstants.MaxFileSizeInBytes"
                :fields="fields"
                fieldKey="imagesToAdd"
              />
            </fieldset>
          </div>
          <Button
            :label="isEdit ? l.actions.save : l.actions.create"
            class="mx-auto"
            type="submit"
          />
        </form>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import AttributeInputGroup from '@/components/attribute-input/AttributeInputGroup.vue'
import BackButton from '@/components/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import FieldError from '@/components/form/FieldError.vue'
import MultipleImageUpload from '@/components/form/MultipleImageUpload.vue'
import { createAdvertisementPostTimeSpanOptions } from '@/constants/advertisement-post-time-span'
import { ImageConstants } from '@/constants/api/ImageConstants'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import {
  AdvertisementClient,
  AttributeFormInfo,
  AttributeValueListItem,
  CategoryFormInfo,
  CategoryItem,
  CreateOrEditAdvertisementRequest,
  ImageDto,
  Int32StringKeyValuePair,
  RequestExceptionResponse,
  ValueTypes
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { CreateOrEditAdvertisementForm } from '@/types/forms/create-or-edit-advertisement'
import { getClient } from '@/utils/client-builder'
import { FieldHelper } from '@/utils/field-helper'
import {
  canAddAdvertisementToCategoryValidator,
  matchArrayElement,
  requiredWhen
} from '@/validators/custom-validators'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm, type SelectChangeEvent } from 'primevue'
import { useForm } from 'vee-validate'
import { computed, onBeforeMount, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { array, number, object, Schema, string, type AnyObject } from 'yup'

//Route
const { push } = useRouter()
const confirm = useConfirm()
const formSubmitted = ref(false)
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Props
const { id: advertisementId } = defineProps<{
  id?: number
}>()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const advertisementService = getClient(AdvertisementClient)

//Reactive data
const isEdit = computed(() => typeof advertisementId === 'number' && !isNaN(advertisementId))

const postTimeOptions = computed(() => createAdvertisementPostTimeSpanOptions(ls))

const loading = ref(0)
const categoryList = ref<CategoryItem[]>([])
const selectedCategories = ref<number[]>([])
const categorySelectOptions = ref<CategoryItem[][]>([])

const loadingAttributes = ref(false)
const attributeInfo = ref<AttributeFormInfo[]>([])
const attributeValueLists = ref<AttributeValueListItem[]>([])

const existingAdvertisement = ref<CreateOrEditAdvertisementRequest>()
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)

//Forms and fields
const validationSchema = computed(() => {
  let schemaObject: AnyObject = {
    categoryId: number().test(canAddAdvertisementToCategoryValidator(categoryList)),
    attributeValues: array().default([]),
    postTime: object().test(requiredWhen(() => !isEdit.value)),
    title: string().required(),
    description: string().required(),
    thumbnailImageHash: string().optional(),

    //Validation is handled in image upload component
    imagesToAdd: array().default([])
  }

  //Add attribute validation
  for (let i = attributeInfo.value.length - 1; i >= 0; i--) {
    const attribute = attributeInfo.value[i]
    let validation: Schema | undefined = undefined
    switch (attribute.attributeValueType) {
      case ValueTypes.Text:
        validation = string()
        break
      case ValueTypes.Integer:
        validation = number().integer()
        break
      case ValueTypes.Decimal:
        validation = number()
        break
      case ValueTypes.ValueListEntry:
        validation = number()
        break
    }

    if (validation && attribute.valueValidationRegex) {
      const regexp = new RegExp(attribute.valueValidationRegex)
      validation = validation.test(matchArrayElement(regexp, attribute.name))
      schemaObject[`attributeValues[${i}]`] = validation
    }
  }

  return toTypedSchema(object(schemaObject))
})

//Change attributeValues type, to allow easier validation and model binding
const form = useForm<CreateOrEditAdvertisementForm>({ validationSchema })
const { fields, valuesChanged, defineMultipleFields, handleErrors } = new FieldHelper(form)
const { values, handleSubmit, isSubmitting, setFieldValue, validateField, resetForm } = form

const imageFieldNames: `imagesToAdd.${number}`[] = [
  ...Array(ImageConstants.MaxImageCountPerAdvertisement).keys()
].map<`imagesToAdd.${number}`>((i) => `imagesToAdd.${i}`)
defineMultipleFields([
  'categoryId',
  'postTime',
  'title',
  'description',
  'imagesToAdd',
  ...imageFieldNames
])

//Hooks
onBeforeMount(() => {
  if (isEdit.value) {
    loadAdvertisementInfo()
  } else {
    reloadData()
  }
})

//Watchers
watch(LocaleService.currentLocaleName, () => {
  reloadData()
})

watch(
  () => advertisementId,
  (newId) => {
    resetForm()
    if (newId) {
      loadAdvertisementInfo()
    } else {
      attributeInfo.value = []
      attributeValueLists.value = []
      selectedCategories.value = []
      existingAdvertisement.value = undefined
      resetCategoryOptionsLists()
    }
  }
)

//Methods
const reloadData = () => {
  loadCategoryList()
  //If category info was loaded, reload it
  if (attributeInfo.value.length) {
    loadCategoryInfo(values.categoryId)
  }
}

const loadAdvertisementInfo = async () => {
  loading.value += 1
  const [{ advertisement, categoryInfo }] = await Promise.all([
    advertisementService.editAdvertisementGet(advertisementId),
    loadCategoryList()
  ])
  existingAdvertisement.value = advertisement
  setCategoryInfo(categoryInfo)

  if (!advertisement) {
    return
  }

  let categoryId: number | undefined = advertisement?.categoryId
  let category: CategoryItem | undefined
  while ((category = categoryList.value.find((c) => c.id === categoryId)) != null) {
    selectedCategories.value.unshift(category.id!)
    categoryId = category.parentCategoryId
  }
  resetCategoryOptionsLists()

  const attributeValues = advertisement.attributeValues
    ? attributeInfo.value.map((a) => {
        const value = advertisement.attributeValues!.find((av) => av.key === a.id)?.value
        if (value && a.attributeValueType !== ValueTypes.Text) {
          return parseInt(value)
        }
        return value
      })
    : []

  resetForm({
    values: {
      categoryId: advertisement.categoryId,
      attributeValues: attributeValues,
      title: advertisement.title,
      description: advertisement.description,
      imagesToAdd: advertisement.imageOrder
    }
  })
  loading.value -= 1
}

const loadCategoryList = async () => {
  loading.value += 1
  categoryList.value = await advertisementService.getCategories()

  //Check if all selected categories are present and their hierarchy has not changed
  let clearCategorySelection =
    !selectedCategories.value.length ||
    !selectedCategories.value.every((id, i) => {
      if (id == null) {
        return true
      }
      const category = categoryList.value.find((c) => c.id === id)
      return category && (i == 0 || selectedCategories.value[i - 1] == category?.parentCategoryId)
    })

  //Reset category options
  if (!clearCategorySelection) {
    resetCategoryOptionsLists()
  }

  if (clearCategorySelection) {
    selectedCategories.value = []
    categorySelectOptions.value = [categoryList.value.filter((c) => c.parentCategoryId == null)]
  }

  loading.value -= 1
}

const resetCategoryOptionsLists = () => {
  categorySelectOptions.value = [categoryList.value.filter((c) => c.parentCategoryId == null)]
  for (const id of selectedCategories.value) {
    const childCategories = categoryList.value.filter((c) => c.parentCategoryId === id)
    if (childCategories.length) {
      categorySelectOptions.value.push(childCategories)
    } else {
      break
    }
  }
}

const loadCategoryInfo = async (categoryId: number) => {
  loadingAttributes.value = true
  setFieldValue('attributeValues', [])
  const result = await advertisementService.getCategoryFormInfo(categoryId)
  setCategoryInfo(result)
  loadingAttributes.value = false
}

const setCategoryInfo = (categoryInfo?: CategoryFormInfo) => {
  if (categoryInfo?.attributeInfo) {
    ensureAttributesHaveModels(categoryInfo.attributeInfo)
  }

  attributeValueLists.value = categoryInfo?.attributeValueLists ?? []
  attributeInfo.value = categoryInfo?.attributeInfo ?? []
}

const ensureAttributesHaveModels = (attributeInfo: AttributeFormInfo[]) => {
  const existingAttributeModelCount = Object.keys(fields).filter((k) =>
    k.startsWith('attributeValues[')
  ).length

  if (attributeInfo.length && existingAttributeModelCount < attributeInfo.length) {
    const fieldNames: `attributeValues.${number}`[] = []
    for (let i = existingAttributeModelCount; i < attributeInfo.length; i++) {
      fieldNames.push(`attributeValues.${i}`)
    }
    defineMultipleFields(fieldNames)
  }
}

const selectCategory = async (e: SelectChangeEvent, i: number) => {
  const originalValue = selectedCategories.value[i]
  selectedCategories.value[i] = e.value

  //Return if selected value did not change
  if (selectedCategories.value.length > i && originalValue === e.value) {
    return
  }

  setFieldValue('categoryId', e.value)
  const categoryIdValidationResult = await validateField('categoryId')
  if (categoryIdValidationResult.valid) {
    loadCategoryInfo(values.categoryId)
  } else {
    attributeInfo.value = []
    attributeValueLists.value = []
  }

  //If one of parent category selection was changed remove selected subcategories
  if (categorySelectOptions.value.length > i + 1) {
    categorySelectOptions.value = categorySelectOptions.value.slice(0, i + 1)
    selectedCategories.value = selectedCategories.value.slice(0, i + 1)
  }

  //Display select for subcategory, if any
  const subCategories = categoryList.value.filter((c) => c.parentCategoryId === e.value)
  if (subCategories.length) {
    categorySelectOptions.value.push(subCategories)
  }
}

const submit = handleSubmit(async () => {
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

  //Transform images for Api call
  const imagesToAdd = values.imagesToAdd.filter((image) => image?.file)
  const imagesToAddFileParameter = imagesToAdd.map((image) => ({
    data: image.file,
    fileName: image.file!.name
  }))
  const imageOrder = values.imagesToAdd
    .filter((image) => image?.hash)
    .map(
      (image) =>
        new ImageDto({
          hash: image.hash
        })
    )

  try {
    if (isEdit.value) {
      await advertisementService.editAdvertisementPost(
        advertisementId,
        values.categoryId,
        attributeValues,
        undefined,
        undefined,
        values.title,
        values.description,
        imagesToAddFileParameter,
        imageOrder
      )
    } else {
      await advertisementService.createAdvertisement(
        undefined,
        values.categoryId,
        attributeValues,
        values.postTime,
        undefined,
        values.title,
        values.description,
        imagesToAddFileParameter,
        imageOrder
      )
    }
    formSubmitted.value = true
    push({ name: 'manageAdvertisements' })
  } catch (e) {
    //Transform added image index to advertisement image index
    if (
      e instanceof RequestExceptionResponse &&
      'errors' in e &&
      e.errors &&
      typeof e.errors === 'object'
    ) {
      const addImagesProperty: keyof CreateOrEditAdvertisementForm = 'imagesToAdd'
      const addImagesErrors = Object.entries(e.errors).filter((e) =>
        e[0].startsWith(addImagesProperty)
      )
      for (const addImageError of addImagesErrors) {
        const indexInAddedImages = parseInt(
          addImageError[0].match(/(?<=\[)[^[\]]+(?=\])/)?.[0] ?? ''
        )
        if (isNaN(indexInAddedImages)) {
          continue
        }

        const imageHash = imagesToAdd[indexInAddedImages].hash
        const imageIndex = values.imagesToAdd.findIndex((i) => i.hash == imageHash)
        if (imageIndex > -1) {
          e.errors[addImagesProperty + '.' + imageIndex] = addImageError[1]
          delete e.errors[addImageError[0]]
        }
      }
    }
    handleErrors(e)
  }
})

const todo = () => alert('Not implemented yet!')
</script>
