<template>
  <Panel>
    <template #header>
      <p>{{ ls.l('imageUpload.imagesUploaded', images?.length ?? 0, maxImageCount) }}</p>
    </template>

    <Draggable
      v-model="images"
      :disabled="false"
      itemKey="url"
      class="flex flex-row flex-wrap gap-2"
    >
      <template #item="{ element, index }">
        <div
          class="relative border-primary border rounded-lg overflow-hidden"
          :class="{ 'p-invalid': imageFields[index]?.hasError }"
        >
          <img :src="element.imageURLs.thumbnailUrl" class="w-40 h-40 object-contain" />
          <div
            class="absolute top-0 left-0 w-10 h-10 py-1.5 rounded-lg text-white text-center font-semibold bg-black opacity-85"
          >
            {{ index + 1 }}
          </div>
          <div
            v-if="index == 0"
            class="absolute bottom-0 w-full h-10 py-1.5 rounded-b-lg text-white text-center font-semibold bg-primary opacity-85"
          >
            {{ l.imageUpload.thumbnail }}
          </div>

          <Button
            class="absolute top-0 right-0 opacity-85"
            icon="pi pi-times"
            severity="danger"
            @click="removeImage(element.url, index)"
          />
        </div>
      </template>
      <template #footer>
        <div v-if="imageUrls.length < maxImageCount">
          <Button class="w-40 h-40" variant="outlined" @click="openUploadDialog()">
            <i class="pi pi-plus" />
            <span>{{ l.imageUpload.uploadImages }}</span>
          </Button>
        </div>
      </template>
    </Draggable>

    <FieldError :messages="allErrors" />
    <input
      ref="uploadInput"
      type="file"
      :multiple="acceptMultipleImages"
      :accept="accept"
      class="hidden"
      @input="selectImagesForUpload"
    />
  </Panel>
</template>

<script setup lang="ts" generic="DtoType extends object">
import { LocaleService } from '@/services/locale-service'
import type { IImageUploadDto } from '@/types/image/image-upload-dto'
import type { Field, Fields } from '@/utils/field-helper'
import { hashFile } from '@/utils/file-helper'
import { fileSize, fileType, uniqueFile } from '@/validators/custom-validators'
import { computed, onBeforeUnmount, ref, useTemplateRef, watch } from 'vue'
import { mixed, ValidationError } from 'yup'
import FieldError from './FieldError.vue'
import Draggable from 'vuedraggable'
import { ImageUrl } from '@/services/api-client'

//Props and model
const images = defineModel<IImageUploadDto[]>()
const props = defineProps<{
  maxImageCount: number
  accept: string
  maxFileSizeInBytes: number
  fields: Fields<DtoType>
  fieldKey: string
}>()

//Services
const ls = LocaleService.get()
const l = LocaleService.currentLocale

//Refs
const uploadInput = useTemplateRef<HTMLInputElement>('uploadInput')

//Reactive data
const imageFields = computed(() =>
  Object.entries(props.fields)
    .filter((e) => e[0].startsWith(props.fieldKey + '.'))
    .sort((a, b) => (a[0] < b[0] ? -1 : 1))
    .map((e) => e[1] as Field<unknown, DtoType>)
)
const acceptMultipleImages = computed(() => props.maxImageCount > 1)

const imageUrls = ref<string[]>([])
const uploadedImageUrls = ref<string[]>([])

const validationErrors = ref<string[]>([])
const apiErrors = computed<string[]>(() =>
  imageFields.value.filter((f) => f.hasError).map((f) => f.error!)
)
const allErrors = computed(() => [...apiErrors.value, ...validationErrors.value])

const imageSchema = computed(() => {
  return mixed<File>()
    .required()
    .test(fileType(props.accept))
    .test(fileSize(props.maxFileSizeInBytes))
})
const uniqueImageSchema = mixed<IImageUploadDto>().required().test(uniqueFile(images.value!))

//Hooks
onBeforeUnmount(() => {
  const urls = uploadedImageUrls.value
  for (let i = urls.length - 1; i >= 0; i--) {
    URL.revokeObjectURL(urls[i])
  }
})

//Watchers
watch(
  images,
  (newValues) => {
    const newImages = newValues?.filter((i) => i?.imageURLs?.url) ?? []

    //Reassign existing errors to correct image
    const imageApiErrors = imageFields.value
      .map((f, i) => [imageUrls.value[i], f.error] as [string, string?])
      .filter((e) => e[1])
    clearFieldErrors()
    if (imageApiErrors.length && newImages.length) {
      for (let i = 0; i < newImages.length; i++) {
        const existingError = imageApiErrors.find(
          (error) => error[0] === newImages[i].imageURLs!.thumbnailUrl
        )
        if (existingError) {
          imageFields.value[i].setErrors(existingError[1]!)
        }
      }
    }

    //Revoke url of removed images
    for (const url of uploadedImageUrls.value) {
      if (!newImages.some((image) => image.imageURLs!.url === url)) {
        URL.revokeObjectURL(url)
      }
    }

    imageUrls.value = newImages.map((i) => i.imageURLs!.url!)
    uploadedImageUrls.value = newImages.filter((i) => i.file).map((i) => i.imageURLs!.url!)

    //Clear model of undefined values
    if (newValues?.length !== newImages.length) {
      images.value = newImages
    }
  },
  { deep: true }
)

//Methods
/** Open hidden file input file upload dialog */
const openUploadDialog = () => {
  uploadInput.value?.click()
}

/** Handle html input element input event */
const selectImagesForUpload = async (inputEvent: Event) => {
  const target = inputEvent.target
  if (!target || !('files' in target) || !(target.files instanceof FileList)) {
    return
  }

  //Validate selected files
  const validFiles = await validateSelectedFiles(target.files)
  if (!validFiles.length) {
    return
  }

  //Create object url
  for (let i = 0; i < validFiles.length; i++) {
    const url = URL.createObjectURL(validFiles[i].file!)
    validFiles[i].imageURLs = new ImageUrl({
      url: url,
      thumbnailUrl: url
    })
  }

  //Update model
  images.value!.push(...validFiles)
}

/** Remove previously or currently selected image */
const removeImage = (url: string, i: number) => {
  validationErrors.value = []
  images.value!.splice(i, 1)
}

/** Validate files which user is trying to select for uploading */
const validateSelectedFiles = async (fileList: FileList) => {
  const validImages: IImageUploadDto[] = []
  validationErrors.value = []

  //Validate each file
  for (let i = 0; i < fileList.length; i++) {
    try {
      const file = await imageSchema.value.validate(fileList.item(i))
      const imageDto = {
        file: file,
        hash: await hashFile(file)
      }
      validImages.push(await uniqueImageSchema.validate(imageDto))
    } catch (e) {
      if (e instanceof ValidationError) {
        validationErrors.value.push(...e.errors)
      } else {
        throw e
      }
    }
  }

  //Validate file count
  if (imageUrls.value.length + validImages.length > props.maxImageCount) {
    validationErrors.value.push(ls.l('errors.InvalidFileLimit', props.maxImageCount))
    validImages.splice(Math.max(props.maxImageCount - imageUrls.value.length, 0))
  }

  return validImages
}

/** Clear all component field errors */
const clearFieldErrors = () => {
  for (const field of imageFields.value) {
    field.clearErrors()
  }
}
</script>
