<template>
  <p v-if="errorMessages?.length" class="p-invalid">
    <template v-for="message in errorMessages" :key="message"> {{ message }} <br /> </template>
  </p>
</template>

<script setup lang="ts" generic="FieldType, DtoType extends GenericObject">
import { Field } from '@/utils/field-helper'
import type { GenericObject } from 'vee-validate'
import { computed } from 'vue'

const props = defineProps<{
  field?: Field<FieldType, DtoType> | undefined
  messages?: string | string[] | undefined | null
}>()

const errorMessages = computed(() => {
  const errorArray = []
  if (props.field && props.field.hasError) {
    errorArray.push(props.field.error)
  } else if (Array.isArray(props.messages)) {
    errorArray.push(...props.messages)
  } else if (props.messages) {
    errorArray.push(props.messages)
  }
  return errorArray
})
</script>
