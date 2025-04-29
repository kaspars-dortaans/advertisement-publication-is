<template>
  <component
    :is="filterComponent"
    v-model="filterValue"
    v-model:secondaryValue="secondaryFilterValue"
    :label="props.label"
    :valueType="props.valueType"
    :valueList="props.valueList"
  ></component>
</template>

<script lang="ts" setup>
import { AttributeValueListItem, FilterType, ValueTypes } from '@/services/api-client'
import { computed, ref, watch, type Component } from 'vue'
import FromToFilter from './FromToFilter.vue'
import SearchFilter from './SearchFilter.vue'
import MatchFilter from './MatchFilter.vue'

const props = defineProps<{
  filterType: FilterType
  valueType: ValueTypes
  label?: string
  valueList?: AttributeValueListItem
}>()

const model = defineModel<(string | number)[]>()

const filterValue = ref()
const secondaryFilterValue = ref()

const filterComponent = computed(() => filterMap.get(props.filterType))
const filterMap = new Map<string, Component>([
  [FilterType.Search, SearchFilter],
  [FilterType.FromTo, FromToFilter],
  [FilterType.Match, MatchFilter]
])

watch([filterValue, secondaryFilterValue], () => {
  if (model.value?.[0] === filterValue.value && model.value?.[1] === secondaryFilterValue.value) {
    return
  }
  model.value = [filterValue.value, secondaryFilterValue.value]
})

watch(model, () => {
  if (model.value?.[0] === filterValue.value && model.value?.[1] === secondaryFilterValue.value) {
    return
  }
  filterValue.value = model.value?.[0]
  secondaryFilterValue.value = model.value?.[1]
})
</script>
