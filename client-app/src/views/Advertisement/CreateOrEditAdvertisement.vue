<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading">
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

              <Divider />

              <!-- Category Select -->
              <template v-for="(categoryOptions, i) in categorySelectOptions" :key="i">
                <FloatLabel variant="on">
                  <Select
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
          <Button :label="l.actions.create" class="mx-auto" type="submit" />
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
import {
  AdvertisementClient,
  AttributeFormInfo,
  AttributeValueListItem,
  CategoryItem,
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
  matchArrayElement
} from '@/validators/custom-validators'
import { toTypedSchema } from '@vee-validate/yup'
import type { SelectChangeEvent } from 'primevue'
import { useForm } from 'vee-validate'
import { computed, onBeforeMount, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { array, number, object, Schema, string, type AnyObject } from 'yup'

const { params } = useRoute()
const { push } = useRouter()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const advertisementService = getClient(AdvertisementClient)

//Reactive data
const isEdit = computed(() => typeof params.id === 'number' && !isNaN(params.id))

const postTimeOptions = computed(() => createAdvertisementPostTimeSpanOptions(ls))

const loading = ref(false)
const categoryList = ref<CategoryItem[]>([])
const selectedCategories = ref<number[]>([])
const categorySelectOptions = ref<CategoryItem[][]>([])

const loadingAttributes = ref(false)
const attributeInfo = ref<AttributeFormInfo[]>([])
const attributeValueLists = ref<AttributeValueListItem[]>([])

//Forms and fields
const validationSchema = computed(() => {
  let schemaObject: AnyObject = {
    categoryId: number().test(canAddAdvertisementToCategoryValidator(categoryList)),
    attributeValues: array().default([]),
    postTime: object().required(),
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
const { fields, defineMultipleFields, handleErrors } = new FieldHelper(form)
const { values, handleSubmit, setFieldValue, validateField } = form

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
  reloadData()
})

//Watchers
watch(LocaleService.currentLocaleName, () => {
  reloadData()
})

//Methods
const reloadData = () => {
  loadCategoryList()
  if (attributeInfo.value) {
    loadCategoryInfo(values.categoryId)
  }
}

const loadCategoryList = async () => {
  loading.value = true
  categoryList.value = await advertisementService.getCategories()

  //Check if all selected categories are present
  let clearCategorySelection =
    !selectedCategories.value.length ||
    !selectedCategories.value.every(
      (id) => id == null || categoryList.value.some((c) => c.id === id)
    )
  if (!clearCategorySelection) {
    //Try and swap category options
    outerLoop: for (let i = 0; i < categorySelectOptions.value.length; i++) {
      const optionList = categorySelectOptions.value[i]
      for (let j = 0; j < optionList.length; j++) {
        const newOption = categoryList.value.find((c) => c.id === optionList[j].id)
        if (!newOption) {
          clearCategorySelection = true
          break outerLoop
        }
        optionList[j] = newOption
      }
    }
  }

  if (clearCategorySelection) {
    selectedCategories.value = []
    categorySelectOptions.value = [categoryList.value.filter((c) => c.parentCategoryId == null)]
  }

  loading.value = false
}

const loadCategoryInfo = async (categoryId: number) => {
  loadingAttributes.value = true
  const result = await advertisementService.getCategoryFormInfo(categoryId)
  if (result.attributeInfo) {
    ensureAttributesHaveModels(result.attributeInfo)
  }

  attributeValueLists.value = result.attributeValueLists!
  attributeInfo.value = result.attributeInfo!
  loadingAttributes.value = false
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
  loading.value = true

  //Transform attributeValues to type accepted by Api
  const attributeValues = values.attributeValues.map(
    (av: string | number, i: number) =>
      new Int32StringKeyValuePair({
        key: attributeInfo.value[i].id,
        value: '' + av
      })
  )

  //Transform images for Api call
  const imagesToAdd = values.imagesToAdd.filter((image) => image?.file)
  const imagesToAddFileParameter = imagesToAdd.map((image) => ({
    data: image.file,
    fileName: image.file!.name
  }))

  const thumbnailImageHash =
    values.imagesToAdd.length && values.imagesToAdd[0] ? values.imagesToAdd[0].hash : undefined

  if (isEdit.value) {
    //TODO: Call Api
    //TODO: Filter images to delete
    const imagesToDelete = []
    alert('Not implemented yet')
  } else {
    try {
      await advertisementService.createAdvertisement(
        undefined,
        values.categoryId,
        attributeValues,
        values.postTime,
        values.title,
        values.description,
        thumbnailImageHash,
        imagesToAddFileParameter,
        undefined
      )
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
  }

  loading.value = false
})
</script>
