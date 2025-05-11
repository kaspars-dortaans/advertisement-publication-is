<template>
  <div class="flex flex-col gap-2" :class="{ 'p-invalid': invalid }">
    <p v-if="label">{{ label }}</p>
    <FloatLabel variant="on">
      <InputText v-model="searchQuery" id="option-filter-input" fluid @input="debouncedSearch" />
      <label for="option-filter-input">{{ l.actions.filter }}</label>
    </FloatLabel>
    <div class="grid grid-cols-[repeat(auto-fit,20rem)] gap-x-4 gap-y-2 justify-center pt-2">
      <div
        v-for="(option, i) in filteredArrayOptions"
        :key="(labelKey ? (option.key as T)[labelKey] : option.key) as string"
        class="inline-flex gap-2"
      >
        <Checkbox
          v-model="option.value"
          :id="'option-input-' + i"
          :binary="true"
          :disabled="disabled"
          @change="handleValueChange"
        />
        <label :for="'option-input-' + i" class="flex-1 break-words">{{
          labelKey ? (option.key as T)[labelKey] : option.key
        }}</label>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup generic="T extends object">
import { DefaultDebounceInMilliseconds } from '@/constants/search-constants'
import { LocaleService } from '@/services/locale-service'
import type { IOption } from '@/types/option'
import { useDebounceFn } from '@vueuse/core'
import { computed, ref, watch } from 'vue'

const props = defineProps<{
  options: (string | T)[]
  labelKey?: keyof T
  label?: string
  disabled?: boolean
  invalid?: boolean
}>()
const selectedOptions = defineModel<number[]>('selected', {
  default: []
})

//Services
const l = LocaleService.currentLocale

//Reactive data
const arrayOptions = computed<IOption<T | string, boolean>[]>(() =>
  props.options.map((o, i) => ({
    key: o,
    value: selectedOptions.value.some((optionIndex) => optionIndex === i)
  }))
)
const filteredArrayOptions = ref<IOption<T | string, boolean>[]>([])
const searchQuery = ref('')

//Methods
const debouncedSearch = useDebounceFn(() => search(), DefaultDebounceInMilliseconds)

const search = () => {
  if (!searchQuery.value) {
    filteredArrayOptions.value = arrayOptions.value
    return
  }

  const searchLowercase = searchQuery.value.toLocaleLowerCase()
  filteredArrayOptions.value = arrayOptions.value.filter((o) => {
    const label: string = ((props.labelKey ? (o.key as T)[props.labelKey] : o.key) as string) ?? ''
    return label.toLocaleLowerCase().indexOf(searchLowercase) > -1
  })
}

const handleValueChange = () => {
  const selectedIndexes = []
  for (let i = 0; i < arrayOptions.value.length; i++) {
    if (arrayOptions.value[i].value) {
      selectedIndexes.push(i)
    }
  }
  selectedOptions.value = selectedIndexes
}

//Watchers
watch(arrayOptions, () => search(), { immediate: true })
</script>
