<template>
  <p v-if="props.field?.hasError" class="text-red-500 text-xs italic">{{ props.field.error }}</p>
</template>

<script setup lang="ts" generic="FieldType, DtoType extends GenericObject">
import type { GenericObject } from 'vee-validate'
import { Field } from '@/utils/field-helper'
import type { PropType } from 'vue'

const props = defineProps({
  field: { type: Object as PropType<Field<FieldType, DtoType>> }
})
</script>
