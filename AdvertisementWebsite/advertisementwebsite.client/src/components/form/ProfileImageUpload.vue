<template>
  <FileUpload
    :accept="allowedFileTypes"
    :maxFileSize="maxFileSize"
    :showUploadButton="false"
    :pt="pt"
    :ptOptions="{
      mergeProps: false
    }"
    :invalidFileLimitMessage="l.errors.InvalidFileLimit"
    :invalidFileSizeMessage="l.errors.InvalidFileSize"
    :invalidFileTypeMessage="l.errors.InvalidFileType"
    @select="handleSelectImage"
    @clear="cancel()"
  >
    <template #empty>
      <div
        v-if="!resultImage && !editingImage"
        class="h-full flex flex-col items-center justify-center"
      >
        <i class="pi pi-cloud-upload !border-2 !rounded-full !p-8 !text-4xl !text-muted-color" />
        <p class="mt-6 mb-0">{{ l.form.common.dragAndDropToUpload }}</p>
      </div>
    </template>

    <template #header="{ chooseCallback, clearCallback, files }">
      <div class="flex flex-wrap justify-center items-center flex-1 gap-4">
        <Button :label="l.actions.choose" @click="chooseCallback" icon="pi pi-plus"></Button>
        <Button
          v-if="editingImage"
          :label="l.actions.accept"
          icon="pi pi-check"
          severity="success"
          @click="acceptEdit()"
        ></Button>
        <Button
          v-else
          :disabled="files?.length === 0 && !resultImage"
          :label="l.actions.edit"
          icon="pi pi-pencil"
          @click="editImage(resultImage!)"
        ></Button>
        <Button
          :disabled="files?.length === 0 && !resultImage && !editingImage"
          :label="l.actions.cancel"
          icon="pi pi-times"
          severity="danger"
          @click="clearCallback()"
        ></Button>
      </div>
    </template>

    <template #content="{ removeFileCallback, files, messages }">
      <div :class="{ hidden: !editingImage && !resultImageUrl }" class="flex flex-col h-full">
        <Cropper
          v-if="editingImage"
          ref="cropper"
          :src="cropperSource.img"
          :stencilComponent="CircleStencil"
          :stencilProps="{
            handlers: {},
            movable: false,
            resizable: false
          }"
          :stencilSize="{
            width: stencilSize,
            height: stencilSize
          }"
          imageRestriction="stencil"
          class="flex-1"
          @ready="removePreviousFiles(removeFileCallback, files.length)"
        ></Cropper>
        <Image v-else :src="resultImageUrl"></Image>
        <FieldError :messages="messages"></FieldError>
      </div>
    </template>
  </FileUpload>
</template>

<script setup lang="ts">
import { LocaleService } from '@/services/locale-service'
import { getImageType } from '@/utils/image-mime-type'
import type { FileUploadSelectEvent } from 'primevue/fileupload'
import { onUnmounted, reactive, ref, useTemplateRef, watch } from 'vue'
import { CircleStencil, Cropper } from 'vue-advanced-cropper'
import 'vue-advanced-cropper/dist/style.css'
import FieldError from '@/components/form/FieldError.vue'

//Props & emits
defineProps<{
  maxFileSize: number
  allowedFileTypes: string
}>()

//Output
const emit = defineEmits(['selectedInvalidFile'])
const resultImage = defineModel<File | undefined>()

//Services
const l = LocaleService.currentLocale

//Refs
const cropper = useTemplateRef('cropper')

//Constants
const stencilSize = 128
const pt = {
  root: {
    class: 'flex flex-col self-stretch md:self-center w-full md:w-[416px] md:h-96'
  },
  header: {
    class: [
      // Flexbox
      'flex-none flex flex-wrap justify-center',

      // Colors
      'bg-surface-0',
      'dark:bg-surface-900',
      'text-surface-700',
      'dark:text-white/80',

      // Spacing
      'p-[1.125rem]',
      'gap-2',

      // Borders
      'border',
      'border-solid',
      'border-surface-200',
      'dark:border-surface-700',

      // Shape
      'rounded-b-lg',

      //order
      'order-2'
    ]
  },
  content: {
    class: [
      'flex-1 flex flex-col',
      'overflow-hidden',

      // Position
      'relative',

      // Colors
      'bg-surface-0',
      'dark:bg-surface-900',
      'text-surface-700',
      'dark:text-white/80',

      // Spacing
      'p-[1.125rem]',

      // Borders
      'border border-b-0',
      'border-surface-200',
      'dark:border-surface-700',

      // Shape
      'rounded-t-lg',

      '[&>[data-pc-section=]]:flex-1',

      //ProgressBar
      '[&>[data-pc-name=pcprogressbar]]:absolute',
      '[&>[data-pc-name=pcprogressbar]]:w-full',
      '[&>[data-pc-name=pcprogressbar]]:top-0',
      '[&>[data-pc-name=pcprogressbar]]:left-0',
      '[&>[data-pc-name=pcprogressbar]]:h-1'
    ]
  }
}

//Reactive data
const editingImage = ref(false)
const cropperSource = reactive({
  img: '',
  type: '',
  name: ''
})

/** Local url to result image */
const resultImageUrl = ref('')

/** Holds last user selected and cropped image */
const lastUserResult = ref<File | undefined>()

//Hooks
onUnmounted(() => {
  if (resultImageUrl.value) {
    URL.revokeObjectURL(resultImageUrl.value)
  }
})

//Watch
watch(resultImage, (newImage) => {
  //If resultImage was changed from this component, return
  if (lastUserResult.value === newImage) {
    return
  }

  if (newImage) {
    if (resultImageUrl.value) {
      URL.revokeObjectURL(resultImageUrl.value)
    }
    resultImageUrl.value = URL.createObjectURL(newImage)
    editingImage.value = false
  } else {
    cancel()
  }
})

//Methods
const handleSelectImage = (e: FileUploadSelectEvent) => {
  const file = e.files.length ? e.files[e.files.length - 1] : null
  if (!file) {
    emit('selectedInvalidFile')
    return
  }
  editImage(file)
}

/**
 * Switch to edit mode and crop new selected file
 */
const editImage = async (imgFile: File) => {
  clearCropperSource()

  cropperSource.img = URL.createObjectURL(imgFile)
  cropperSource.type = await getImageType(
    imgFile,
    imgFile.type ?? imgFile.name.slice(imgFile.name.lastIndexOf('.'))
  )
  cropperSource.name = imgFile.name
  editingImage.value = true
  clearResult()
}

/**
 * Switch to display mode, display {@link Cropper} result
 */
const acceptEdit = async () => {
  const croppedImage = await new Promise<File | null>((resolve) =>
    cropper.value?.getResult().canvas?.toBlob((blob: Blob | null) => {
      resolve(new File([blob!], cropperSource.name))
    }, cropperSource.type)
  )

  if (croppedImage) {
    resultImage.value = croppedImage
    lastUserResult.value = croppedImage
    resultImageUrl.value = URL.createObjectURL(croppedImage)
    editingImage.value = false
  }
}

/**
 * Removes previously selected files
 * @param removeFilesCallback
 * @param fileCount
 */
const removePreviousFiles = (removeFilesCallback: (index: number) => void, fileCount: number) => {
  for (let index = fileCount - 2; index >= 0; index--) {
    removeFilesCallback(index)
  }
}
/**
 * Clear selected image
 */
const cancel = () => {
  editingImage.value = false
  clearResult()
}

/**
 * Clears {@link cropperSource} object and revokes source image url if it exists
 */
const clearCropperSource = () => {
  if (cropperSource.img) {
    URL.revokeObjectURL(cropperSource.img)
  }
  cropperSource.img = ''
  cropperSource.type = ''
  cropperSource.name = ''
}

/**
 * Clears and revokes {@link resultImageUrl}, clears {@link result}
 */
const clearResult = () => {
  if (resultImageUrl.value) {
    URL.revokeObjectURL(resultImageUrl.value)
    resultImageUrl.value = ''
  }
  resultImage.value = undefined
  lastUserResult.value = undefined
}
</script>
