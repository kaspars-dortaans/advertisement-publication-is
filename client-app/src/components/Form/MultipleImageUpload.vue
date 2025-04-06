<template>
  <Panel>
    <template #header>
      <p>{{ ls.l('imageUpload.imagesUploaded', images?.length ?? 0, maxImageCount) }}</p>
    </template>

    <div class="flex flex-row flex-wrap gap-2">
      <div
        v-for="(url, i) in imageUrls"
        :key="url"
        class="relative border-primary border rounded-lg overflow-hidden"
        :class="{ 'p-invalid': imageFields[i]?.hasError }"
      >
        <img :src="url" class="w-40 h-40 object-contain" />
        <Button
          class="absolute top-0 right-0"
          icon="pi pi-times"
          severity="danger"
          @click="removeImage(url, i)"
        />
      </div>
      <div v-if="imageUrls.length < maxImageCount">
        <Button class="w-40 h-40" variant="outlined" @click="openUploadDialog()">
          <i class="pi pi-plus" />
          <span>{{ l.imageUpload.uploadImages }}</span>
        </Button>
      </div>
      <input
        ref="uploadInput"
        type="file"
        :multiple="acceptMultipleImages"
        :accept="accept"
        class="hidden"
        @input="selectImagesForUpload"
      />
      <FieldError :messages="allErrors" />
    </div>
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

//Props and model
const images = defineModel<IImageUploadDto[]>()
const { accept, maxImageCount, maxFileSizeInBytes, fields, fieldKey } = defineProps<{
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
  Object.entries(fields)
    .filter((e) => e[0].startsWith(fieldKey + '.'))
    .sort((a, b) => (a[0] < b[0] ? -1 : 1))
    .map((e) => e[1] as Field<unknown, DtoType>)
)
const acceptMultipleImages = computed(() => maxImageCount > 1)

const imageUrls = ref<string[]>([])
const uploadedImageUrls = ref<string[]>([])

const validationErrors = ref<string[]>([])
const apiErrors = computed<string[]>(() =>
  imageFields.value.filter((f) => f.hasError).map((f) => f.error!)
)
const allErrors = computed(() => [...apiErrors.value, ...validationErrors.value])

const imageSchema = computed(() => {
  return mixed<File>().required().test(fileType(accept)).test(fileSize(maxFileSizeInBytes))
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
    const newImages = newValues?.filter((i) => !!i) ?? []

    //Reassign existing errors to correct image
    const imageApiErrors = imageFields.value
      .map((f, i) => [imageUrls.value[i], f.error] as [string, string?])
      .filter((e) => e[1])
    clearFieldErrors()
    if (imageApiErrors.length && newImages.length) {
      for (let i = 0; i < newImages.length; i++) {
        const existingError = imageApiErrors.find((error) => error[0] === newImages[i].url)
        if (existingError) {
          imageFields.value[i].setErrors(existingError[1]!)
        }
      }
    }

    //Revoke url of removed images
    for (const url of uploadedImageUrls.value) {
      if (!newImages.some((image) => image.url === url)) {
        URL.revokeObjectURL(url)
      }
    }

    imageUrls.value = newImages.map((i) => i.url)
    uploadedImageUrls.value = newImages.filter((i) => i.file).map((i) => i.url)

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
    validFiles[i].url = url
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
  if (imageUrls.value.length + validImages.length > maxImageCount) {
    validationErrors.value.push(ls.l('errors.InvalidFileLimit', maxImageCount))
    validImages.splice(Math.max(maxImageCount - imageUrls.value.length, 0))
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
