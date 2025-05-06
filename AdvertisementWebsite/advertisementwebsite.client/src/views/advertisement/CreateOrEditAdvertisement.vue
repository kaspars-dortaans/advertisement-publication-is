<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading" class="flex-1 lg:flex-none flex">
      <Panel class="flex-1 rounded-none lg:rounded-md">
        <template #header>
          <div class="panel-title-container">
            <BackButton
              :defaultTo="{ name: isEdit ? 'viewAdvertisement' : 'viewAdvertisements' }"
            />
            <h3 class="page-title">
              {{ isEdit ? l.navigation.editAdvertisement : l.navigation.createAdvertisement }}
            </h3>
          </div>
        </template>

        <form class="flex flex-col gap-2" @submit="submit">
          <div class="flex flex-col lg:flex-row lg:justify-center flex-wrap gap-3">
            <fieldset class="flex flex-col gap-2 min-w-full lg:min-w-60">
              <!-- Time period -->
              <template v-if="!isEdit">
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

              <div v-else-if="existingAdvertisement?.validToDate" class="flex gap-2">
                <FloatLabel variant="on" class="flex-1">
                  <InputText
                    :defaultValue="dateFormat.format(existingAdvertisement?.validToDate)"
                    disabled
                    fluid
                  ></InputText>
                  <label for="valid-to">{{ l.form.putAdvertisement.validTo }}</label>
                </FloatLabel>
                <Button
                  :label="l.actions.extend"
                  severity="secondary"
                  as="RouterLink"
                  :to="{
                    name: 'extend',
                    params: { type: PaymentType.ExtendAdvertisement, ids: `[${id}]` }
                  }"
                />
              </div>

              <Divider />

              <!-- Category Select -->
              <CategorySelect
                :categoryList="categoryList"
                :field="fields.categoryId"
                @selectedCategory="handleSelectedCategory"
              />

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

              <Divider v-if="isSmallScreen" />
            </fieldset>

            <fieldset class="flex flex-col gap-2 min-w-full lg:min-w-96">
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
            :loading="isSubmitting"
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
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import CategorySelect from '@/components/form/CategorySelect.vue'
import FieldError from '@/components/form/FieldError.vue'
import MultipleImageUpload from '@/components/form/MultipleImageUpload.vue'
import {
  useManageAttributeInput,
  useValidateAttributeInput
} from '@/composables/manage-attribute-input'
import { usePaymentState } from '@/composables/payment-store'
import { createAdvertisementPostTimeSpanOptions } from '@/constants/advertisement-post-time-span'
import { ImageConstants } from '@/constants/api/ImageConstants'
import {
  AdvertisementClient,
  AttributeFormInfo,
  CategoryClient,
  CategoryItem,
  CreateOrEditAdvertisementRequest,
  ImageDto,
  Int32StringKeyValuePair,
  NewPaymentItem,
  PaymentType,
  RequestExceptionResponse,
  ValueTypes
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { CreateOrEditAdvertisementForm } from '@/types/forms/create-or-edit-advertisement'
import type { IImageUploadDto } from '@/types/image/image-upload-dto'
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

//Route
const { push } = useRouter()
const confirm = useConfirm()
const formSubmitted = ref(false)
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Props
const props = defineProps<{
  id?: number
}>()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const advertisementService = getClient(AdvertisementClient)
const categoryService = getClient(CategoryClient)

//Reactive data
const isEdit = computed(() => typeof props.id === 'number' && !isNaN(props.id))
const postTimeOptions = computed(() => createAdvertisementPostTimeSpanOptions(ls))
const existingAdvertisement = ref<CreateOrEditAdvertisementRequest>()
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)
const categoryList = ref<CategoryItem[]>([])
const attributeInfo = ref<AttributeFormInfo[]>([])
const { isSmallScreen } = useTrackScreenSize()
const paymentState = usePaymentState()

//Forms and fields
const { addAttributeValidationSchema } = useValidateAttributeInput(attributeInfo)
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

  addAttributeValidationSchema(schemaObject)
  return toTypedSchema(object(schemaObject))
})

//Change attributeValues type, to allow easier validation and model binding
const form = useForm<CreateOrEditAdvertisementForm>({ validationSchema })
const fieldHelper = new FieldHelper(form)
const { values, handleSubmit, isSubmitting, resetForm } = form
const { fields, valuesChanged, defineMultipleFields, handleErrors } = fieldHelper

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

const {
  loadingAttributes,
  loading,
  attributeValueLists,
  setCategoryInfo,
  handleSelectedCategory,
  loadCategoryInfo,
  loadCategoryList
} = useManageAttributeInput<CreateOrEditAdvertisementForm>(
  form,
  fieldHelper,
  categoryList,
  attributeInfo,
  categoryService
)

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
  () => props.id,
  (newId) => {
    resetForm()
    if (newId) {
      loadAdvertisementInfo()
    } else {
      resetForm({
        values: {
          categoryId: undefined,
          attributeValues: [],
          title: '',
          description: '',
          imagesToAdd: []
        }
      })
      attributeInfo.value = []
      attributeValueLists.value = []
      existingAdvertisement.value = undefined
    }
  }
)

//Methods
const reloadData = () => {
  loadCategoryList()
  //If category info was loaded before, reload it
  if (attributeInfo.value.length) {
    loadCategoryInfo(values.categoryId)
  }
}

const loadAdvertisementInfo = async () => {
  loading.value += 1
  const [{ advertisement, categoryInfo }] = await Promise.all([
    advertisementService.editAdvertisementGet(props.id),
    loadCategoryList()
  ])
  existingAdvertisement.value = advertisement
  setCategoryInfo(categoryInfo)

  if (!advertisement) {
    loading.value -= 1
    //TODO: display error
    return
  }

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
    .filter((i) => i != null) as Int32StringKeyValuePair[]

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
        props.id,
        values.categoryId,
        attributeValues,
        undefined,
        undefined,
        values.title,
        values.description,
        imagesToAddFileParameter,
        imageOrder
      )
      formSubmitted.value = true
      push({ name: 'manageAdvertisements' })
    } else {
      const id = await advertisementService.createAdvertisement(
        undefined,
        values.categoryId,
        attributeValues,
        undefined,
        undefined,
        values.title,
        values.description,
        imagesToAddFileParameter,
        imageOrder
      )
      formSubmitted.value = true
      paymentState.value.paymentItems = [
        new NewPaymentItem({
          paymentSubjectId: id,
          type: PaymentType.CreateAdvertisement,
          timePeriod: values.postTime!
        })
      ]
      push({ name: 'makePayment' })
    }
  } catch (e) {
    mapImageErrorToCorrectIndex(e, imagesToAdd)
    handleErrors(e)
  }
})

const mapImageErrorToCorrectIndex = (e: unknown, images: IImageUploadDto[]) => {
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
      const indexInAddedImages = parseInt(addImageError[0].match(/(?<=\[)[^[\]]+(?=\])/)?.[0] ?? '')
      if (isNaN(indexInAddedImages)) {
        continue
      }

      const imageHash = images[indexInAddedImages].hash
      const imageIndex = values.imagesToAdd.findIndex((i) => i.hash == imageHash)
      if (imageIndex > -1) {
        e.errors[addImagesProperty + '.' + imageIndex] = addImageError[1]
        delete e.errors[addImageError[0]]
      }
    }
  }
}
</script>
