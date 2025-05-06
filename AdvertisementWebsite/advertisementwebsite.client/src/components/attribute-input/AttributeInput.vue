<template>
  <FloatLabel variant="on">
    <component
      :is="inputComponent"
      v-model="model"
      :options="valueList"
      :invalid="invalid"
      :id="inputId"
      :minFractionDigits="0"
      :maxFractionDigits="valueType === ValueTypes.Decimal ? 5 : 0"
      :disabled="disabled"
      :showClear="!disabled"
      optionLabel="name"
      optionValue="id"
      fluid
    ></component>
    <label :for="inputId">{{ props.label }}</label>
  </FloatLabel>
</template>

<script lang="ts" setup>
import { AttributeValueListItem, ValueTypes } from '@/services/api-client'
import { InputNumber, InputText, Select } from 'primevue'
import { computed, useId, type Component } from 'vue'

const props = defineProps<{
  valueType: ValueTypes
  label?: string
  valueList?: AttributeValueListItem[]
  invalid?: boolean
  disabled?: boolean
}>()

const model = defineModel<string | number>()

const inputId = useId()
const inputComponent = computed(() => inputComponentMap.get(props.valueType))
const inputComponentMap = new Map<string, Component>([
  [ValueTypes.Text, InputText],
  [ValueTypes.Integer, InputNumber],
  [ValueTypes.Decimal, InputNumber],
  [ValueTypes.ValueListEntry, Select]
])
</script>
