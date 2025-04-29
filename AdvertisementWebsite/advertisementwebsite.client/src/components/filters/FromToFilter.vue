<template>
  <FloatLabel variant="on">
    <component
      :is="inputComponent"
      v-model="fromModel"
      :id="inputId"
      :options="valueList?.entries"
      optionLabel="name"
      optionValue="orderIndex"
      showClear
    ></component>
    <label :for="inputId">{{ props.label }} {{ l.dataTable.from }}</label>
  </FloatLabel>
  <FloatLabel variant="on">
    <component
      :is="inputComponent"
      v-model="toModel"
      :id="secondaryInputId"
      :options="valueList?.entries"
      optionLabel="name"
      optionValue="orderIndex"
      showClear
    ></component>
    <label :for="secondaryInputId">{{ l.dataTable.to }}</label>
  </FloatLabel>
</template>

<script lang="ts" setup>
import { AttributeValueListItem, ValueTypes } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { InputNumber, InputText, Select } from 'primevue'
import { computed, useId, type Component } from 'vue'

const props = defineProps<{
  label: string
  valueType: ValueTypes
  valueList: AttributeValueListItem
}>()
const fromModel = defineModel()
const toModel = defineModel('secondaryValue')

const l = LocaleService.currentLocale

const inputId = useId()
const secondaryInputId = useId()

const filterMap = new Map<string, Component>([
  [ValueTypes.Text, InputText],
  [ValueTypes.Decimal, InputNumber],
  [ValueTypes.Integer, InputNumber],
  [ValueTypes.ValueListEntry, Select]
])

const inputComponent = computed(() => filterMap.get(props.valueType))
</script>
