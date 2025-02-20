<template>
  <FileUpload
    :accept="allowedFileType"
    :maxFileSize="maxFileSize"
    :showUploadButton="false"
    :pt="pt"
    :ptOptions="{
      mergeProps: false
    }"
    @select="editImage"
    @clear="cancel()"
  >
    <template #empty>
      <div class="flex items-center justify-center flex-col">
        <i class="pi pi-cloud-upload !border-2 !rounded-full !p-8 !text-4xl !text-muted-color" />
        <p class="mt-6 mb-0">{{ l.form.dragAndDropToUpload }}</p>
      </div>
    </template>

    <template #header="{ chooseCallback, clearCallback, files }">
      <div class="flex flex-wrap justify-between items-center flex-1 gap-4">
        <div class="flex gap-2">
          <Button :label="l.actions.choose" @click="chooseCallback" icon="pi pi-plus"></Button>
          <Button
            v-if="editingImage"
            :disabled="!files || files.length === 0"
            :label="l.actions.accept"
            icon="pi pi-check"
            severity="success"
            @click="acceptEdit(files)"
          ></Button>
          <Button
            v-else
            :disabled="!files || files.length === 0"
            :label="l.actions.edit"
            icon="pi pi-pencil"
            @click="editAcceptedImage()"
          ></Button>
          <Button
            :disabled="!files || files.length === 0"
            :label="l.actions.cancel"
            icon="pi pi-times"
            severity="danger"
            @click="clearCallback()"
          ></Button>
        </div>
      </div>
    </template>

    <template #content="{ removeFileCallback, files }">
      <div class="w-96 h-96">
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
          @ready="removePreviousFiles(removeFileCallback, files.length)"
        ></Cropper>
        <Image v-else :src="resultImageUrl"></Image>
      </div>
    </template>
  </FileUpload>
</template>

<script setup lang="ts">
import { LocaleService } from '@/services/locale-service'
import { getImageMimeType } from '@/utils/image-mime-type'
import type { FileUploadSelectEvent } from 'primevue/fileupload'
import { onMounted, reactive, ref, useTemplateRef } from 'vue'
import { CircleStencil, Cropper } from 'vue-advanced-cropper'
import 'vue-advanced-cropper/dist/style.css'

//Services
const l = LocaleService.currentLocale

//Refs
const cropper = useTemplateRef('cropper')

//Child props
const allowedFileType = '.jpg, .png'
const maxFileSize = 1000000
const stencilSize = 128

const cropperSource = reactive({
  img: '',
  type: ''
})
const pt = {
  root: {
    class: ['flex', 'flex-col']
  },
  header: {
    class: [
      // Flexbox
      'flex',
      'flex-wrap',
      'justify-center',

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

      //ProgressBar
      '[&>[data-pc-name=pcprogressbar]]:absolute',
      '[&>[data-pc-name=pcprogressbar]]:w-full',
      '[&>[data-pc-name=pcprogressbar]]:top-0',
      '[&>[data-pc-name=pcprogressbar]]:left-0',
      '[&>[data-pc-name=pcprogressbar]]:h-1'
    ]
  }
}

//Flags
const editingImage = ref(false)

//Values
const resultImage = defineModel<Blob | undefined>()
const resultImageUrl = ref('')

//Hooks
onMounted(() => {
  clearCropperSource()
  clearResult()
})

//Methods
/**
 * Switch to edit mode and crop new selected file
 */
const editImage = async (e: FileUploadSelectEvent) => {
  clearCropperSource()
  const imgFile = e.files[e.files.length - 1]
  cropperSource.img = URL.createObjectURL(imgFile)
  cropperSource.type = await getImageMimeType(imgFile, imgFile.type)
  editingImage.value = true
  clearResult()
}

/**
 * Switch to edit mode and crop currently selected file
 */
const editAcceptedImage = async () => {
  editingImage.value = true
  clearResult()
}

/**
 * Switch to display mode, display {@link Cropper} result
 */
const acceptEdit = async (files: File[]) => {
  const croppedImage = await new Promise<File | null>((resolve) =>
    cropper.value?.getResult().canvas?.toBlob((blob: Blob | null) => {
      resolve(new File([blob!], files[0].name))
    }, cropperSource.type)
  )

  if (croppedImage) {
    resultImage.value = croppedImage
    resultImageUrl.value = URL.createObjectURL(croppedImage)
    editingImage.value = false
  }
}

/**
 * Removes previosly selected files
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
    cropperSource.img = ''
    cropperSource.type = ''
  }
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
}
</script>
