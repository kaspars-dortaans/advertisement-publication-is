<template>
  <p v-if="errorMessages?.length" class="p-invalid">
    <template v-for="message in errorMessages" :key="message"> {{ message }} <br /> </template>
  </p>
</template>

<script setup lang="ts" generic="FieldType, DtoType extends GenericObject">
import { Field } from '@/utils/field-helper'
import type { GenericObject } from 'vee-validate'
import { computed } from 'vue'

const { field, messages } = defineProps<{
  field?: Field<FieldType, DtoType> | undefined
  messages?: string | string[] | undefined
}>()

const errorMessages = computed(() => {
  const errorArray = []
  if (field && field.hasError) {
    errorArray.push(field.error)
  } else if (Array.isArray(messages)) {
    errorArray.push(...messages)
  } else if (messages) {
    errorArray.push(messages)
  }
  return errorArray
})
</script>
