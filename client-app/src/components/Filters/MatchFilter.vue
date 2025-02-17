<template>
  <FloatLabel variant="on">
    <component
      :is="inputComponent"
      v-model="model"
      :options="valueList?.entries"
      :id="inputId"
      optionLabel="name"
      optionValue="id"
      showClear
      class="w-full"
    ></component>
    <label :for="inputId">{{ props.label }}</label>
  </FloatLabel>
</template>

<script lang="ts" setup>
import { ValueTypes, type AttributeValueListItem } from '@/services/api-client'
import { InputNumber, InputText, Select } from 'primevue'
import { computed, useId, type Component } from 'vue'

const props = defineProps<{
  label: string
  valueType: ValueTypes
  valueList?: AttributeValueListItem
}>()
const model = defineModel()

const inputId = useId()

const filterMap = new Map<string, Component>([
  [ValueTypes.Text, InputText],
  [ValueTypes.Decimal, InputNumber],
  [ValueTypes.Integer, InputNumber],
  [ValueTypes.ValueListEntry, Select]
])

const inputComponent = computed(() => filterMap.get(props.valueType))
</script>
