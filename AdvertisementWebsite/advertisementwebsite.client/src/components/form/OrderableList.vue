<template>
  <div :class="{ 'p-invalid': invalid }">
    <FloatLabel v-if="!disabled" variant="on">
      <AutoComplete
        v-model="addAttributeModel"
        :suggestions="suggestedLookups"
        optionLabel="value"
        id="attribute-input"
        dropdown
        fluid
        @complete="search"
      />
      <label for="attribute-input">{{ inputLabel }}</label>
    </FloatLabel>
    <Draggable
      v-if="model.length"
      v-model="model"
      :disabled="disabled"
      itemKey="key"
      class="flex flex-col gap-2 mt-2"
    >
      <template #header>
        <p class="text-surface-500">{{ listLabel }}</p>
      </template>
      <template #item="{ element, index }">
        <p
          class="inline-flex gap-2 items-center px-2 bg-surface-100 rounded-md"
          :class="{ 'cursor-pointer hover:bg-surface-200': !disabled }"
        >
          <i class="pi pi-sort" />
          <span>{{ index + 1 }}. {{ element.value }}</span>
          <Button
            :disabled="disabled"
            class="ml-auto"
            icon="pi pi-trash"
            severity="danger"
            variant="text"
            rounded
            @click="removeAttribute(index)"
          />
        </p>
      </template>
    </Draggable>
  </div>
</template>

<script setup lang="ts">
import { Int32StringKeyValuePair } from '@/services/api-client'
import type { AutoCompleteCompleteEvent } from 'primevue'
import { nextTick, onBeforeMount, ref, watch } from 'vue'
import Draggable from 'vuedraggable'

const model = defineModel<Int32StringKeyValuePair[]>({
  default: []
})

const props = defineProps<{
  lookups: Int32StringKeyValuePair[]
  inputLabel: string
  listLabel: string
  invalid?: boolean
  disabled?: boolean
}>()

const suggestedLookups = ref<Int32StringKeyValuePair[]>([])
const addAttributeModel = ref<Int32StringKeyValuePair | undefined>()

onBeforeMount(() => {
  search({ query: '', originalEvent: null! })
})

watch(addAttributeModel, (newValue) => {
  if (newValue) {
    model.value.push(newValue)
    nextTick(() => {
      addAttributeModel.value = undefined
    })
  }
})

const search = (e: AutoCompleteCompleteEvent) => {
  const queryLowercase = e.query.toLocaleLowerCase()
  suggestedLookups.value = props.lookups.filter(
    (l) =>
      l.value!.toLocaleLowerCase().indexOf(queryLowercase) > -1 &&
      model.value.every((ml) => ml.key !== l.key)
  )
}

const removeAttribute = (i: number) => {
  model.value.splice(i, 1)
}
</script>
