<template>
  <div class="flex flex-row flex-wrap gap-2 justify-start" :class="{ 'p-invalid': invalid }">
    <div
      v-for="attachment in model"
      :key="attachmentKeys.get(attachment)"
      class="p-2 max-w-60 text-center border border-surface-300 rounded-md"
    >
      <div class="flex align-baseline justify-center gap-2">
        <p class="line-clamp">{{ attachment.name }}</p>
        <Button
          class="max-w-6 max-h-6 flex-none"
          icon="pi pi-times"
          size="small"
          severity="danger"
          @click="removeSelected(attachment)"
        />
      </div>
      <p class="flex-1">{{ formatDataSize(attachment.size) }}</p>
    </div>
    <input ref="fileInput" type="file" class="hidden" multiple @input="handleFileInput" />
  </div>
</template>

<script lang="ts" setup>
import { formatDataSize } from '@/utils/file-helper'
import { defineExpose, ref, useTemplateRef } from 'vue'

//Model
const model = defineModel<File[]>()

//Props
const { invalid } = defineProps<{
  invalid?: boolean
}>()

//Refs
const fileInputEl = useTemplateRef('fileInput')

//Reactive data
const attachmentKeys = ref(new Map<File, string>())
const attachmentKeyCounter = ref(0)

//Methods
const removeSelected = (attachment: File) => {
  const index = model.value?.indexOf(attachment)
  if (index != null && index > -1) {
    model.value?.splice(index, 1)
  }
}

const handleFileInput = (e: Event) => {
  const fileList =
    e.target && 'files' in e.target && e.target.files instanceof FileList ? e.target.files : null

  if (fileList && fileList.length) {
    for (const file of fileList) {
      attachmentKeys.value.set(file, '' + attachmentKeyCounter.value++)
    }
    model.value?.push(...fileList)
  }
}

const openDialog = () => {
  fileInputEl.value?.click()
}

defineExpose({ openDialog })
</script>
