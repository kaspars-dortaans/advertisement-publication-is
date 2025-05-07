<template>
  <template v-for="(categoryOptions, i) in categorySelectOptions" :key="i">
    <FloatLabel variant="on">
      <Select
        :defaultValue="selectedCategories[i]"
        :options="categoryOptions"
        :invalid="field?.hasError && i === categorySelectOptions.length - 1"
        :id="'category-select-' + i"
        :disabled="disabled"
        optionLabel="name"
        optionValue="id"
        fluid
        @change="selectCategory($event, i)"
      />
      <label :for="'category-select-' + i">{{
        i == 0 ? l.form.putAdvertisement.category : l.form.putAdvertisement.subcategory
      }}</label>
    </FloatLabel>
    <FieldError v-if="i === categorySelectOptions.length - 1" :field="field" />
  </template>
</template>

<script lang="ts" setup generic="T extends GenericObject">
import type { CategoryItem } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { Field } from '@/utils/field-helper'
import type { SelectChangeEvent } from 'primevue'
import type { GenericObject } from 'vee-validate'
import { ref, watch } from 'vue'
import FieldError from './FieldError.vue'

const props = defineProps<{
  categoryList: CategoryItem[]
  field?: Field<number, T>
  value?: number
  disabled?: boolean
}>()
const emit = defineEmits(['selectedCategory'])

//Services
const l = LocaleService.currentLocale

//Reactive data
const selectedCategories = ref<number[]>([])
const categorySelectOptions = ref<CategoryItem[][]>([])

//Methods
const selectCategory = async (e: SelectChangeEvent, i: number) => {
  //Return if selected value did not change
  if (selectedCategories.value.length > i && selectedCategories.value[i] === e.value) {
    return
  }

  selectedCategories.value[i] = e.value
  emit('selectedCategory', e.value)

  //If one of parent category selection was changed remove selected subcategories
  if (categorySelectOptions.value.length > i + 1) {
    categorySelectOptions.value = categorySelectOptions.value.slice(0, i + 1)
    selectedCategories.value = selectedCategories.value.slice(0, i + 1)
  }

  //Display select for subcategory, if any
  const subCategories = props.categoryList.filter((c) => c.parentCategoryId === e.value)
  if (subCategories.length) {
    categorySelectOptions.value.push(subCategories)
  }
}

const resetCategoryOptionsLists = () => {
  categorySelectOptions.value = [props.categoryList.filter((c) => c.parentCategoryId == null)]
  for (const id of selectedCategories.value) {
    const childCategories = props.categoryList.filter((c) => c.parentCategoryId === id)
    if (childCategories.length) {
      categorySelectOptions.value.push(childCategories)
    } else {
      break
    }
  }
}

//Watch
watch(
  () => props.categoryList,
  (newList) => {
    //Check if all selected categories are present and their hierarchy has not changed
    const clearCategorySelection = selectedCategories.value.some((id, i) => {
      //Return false if last category is not yet selected
      if (id == null) {
        return false
      }
      const category = newList.find((c) => c.id === id)
      return !category || (i !== 0 && selectedCategories.value[i - 1] !== category.parentCategoryId)
    })

    if (clearCategorySelection) {
      selectedCategories.value = []
    }
    resetCategoryOptionsLists()
  }
)

watch(
  () => props.field?.value || props.value,
  (newValue) => {
    const selectedLength = selectedCategories.value.length
    //If selection has not changed return
    if (
      (selectedLength && newValue === selectedCategories.value[selectedLength - 1]) ||
      (selectedCategories.value[selectedLength - 1] == null &&
        selectedCategories.value[selectedLength - 2] === newValue)
    ) {
      return
    }

    //Rebuild selection list
    selectedCategories.value = []
    let categoryId: number | undefined = newValue
    let category: CategoryItem | undefined
    while ((category = props.categoryList.find((c) => c.id === categoryId)) != null) {
      selectedCategories.value.unshift(category.id!)
      categoryId = category.parentCategoryId
    }
    resetCategoryOptionsLists()
  }
)
</script>
