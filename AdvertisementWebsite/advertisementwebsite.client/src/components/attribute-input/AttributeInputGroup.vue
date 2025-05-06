<template>
  <template v-if="attributeValues">
    <AttributeInput
      v-for="(attribute, i) in attributes"
      :key="attribute.id"
      v-model="attributeValues[i]"
      :label="attribute.name"
      :valueType="attribute.attributeValueType!"
      :valueList="attribute.valueListId ? valueListDict[attribute.valueListId] : undefined"
      :disabled="disabled"
    />
  </template>
  <template v-else-if="attributeFields">
    <template v-for="(attribute, i) in attributes" :key="attribute.id">
      <AttributeInput
        v-model="attributeFields[i].model.value"
        v-bind="attributeFields[i].attributes"
        :label="attribute.name"
        :valueType="attribute.attributeValueType!"
        :valueList="attribute.valueListId ? valueListDict[attribute.valueListId] : undefined"
        :invalid="attributeFields[i].hasError"
        :disabled="disabled"
      />
      <FieldError :field="attributeFields[i]"></FieldError>
    </template>
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

const props = defineProps<{
  fields?: Fields<DtoType>
  values?: (string | number | undefined)[]
  fieldKey?: string
  attributes: AttributeFormInfo[]
  valueLists: AttributeValueListItem[]
  disabled?: boolean
}>()

const attributeFields = computed(() =>
  props.fields
    ? Object.entries(props.fields)
        .filter((e) => e[0].startsWith(props.fieldKey + '.'))
        .sort((a, b) => (a[0] < b[0] ? -1 : 1))
        .map((e) => e[1] as Field<string | number | undefined, DtoType>)
    : undefined
)

const attributeValues = computed(() => {
  return props.values?.map((v) => v)
})

const valueListDict = computed(() => {
  const obj: { [key: number]: AttributeValueListEntryItem[] } = {}
  for (const valueList of props.valueLists) {
    if (!valueList.id || !valueList.entries) {
      continue
    }

    obj[valueList.id] = valueList.entries
  }
  return obj
})
</script>
