<template>
  <template v-for="(attribute, i) in attributes" :key="attribute.id">
    <AttributeInput
      v-model="attributeFields[i].model.value"
      v-bind="attributeFields[i].attributes"
      :label="attribute.name"
      :valueType="attribute.attributeValueType!"
      :valueList="attribute.valueListId ? valueListDict[attribute.valueListId] : undefined"
      :invalid="attributeFields[i].hasError"
    />
    <FieldError :field="attributeFields[i]"></FieldError>
  </template>
</template>

<script lang="ts" setup generic="DtoType extends object">
import FieldError from '@/components/form/FieldError.vue'
import {
  AttributeFormInfo,
  AttributeValueListEntryItem,
  AttributeValueListItem
} from '@/services/api-client'
import type { Field, Fields } from '@/utils/field-helper'
import { computed } from 'vue'
import AttributeInput from './AttributeInput.vue'

const { fields, fieldKey, attributes, valueLists } = defineProps<{
  fields: Fields<DtoType>
  fieldKey: string
  attributes: AttributeFormInfo[]
  valueLists: AttributeValueListItem[]
}>()

const attributeFields = computed(() =>
  Object.entries(fields)
    .filter((e) => e[0].startsWith(fieldKey + '.'))
    .sort((a, b) => (a[0] < b[0] ? -1 : 1))
    .map((e) => e[1] as Field<string | number | undefined, DtoType>)
)

const valueListDict = computed(() => {
  const obj: { [key: number]: AttributeValueListEntryItem[] } = {}
  for (const valueList of valueLists) {
    if (!valueList.id || !valueList.entries) {
      continue
    }

    obj[valueList.id] = valueList.entries
  }
  return obj
})
</script>
