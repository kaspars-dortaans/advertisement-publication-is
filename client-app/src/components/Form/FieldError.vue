<template>
  <p v-if="errorMessage" class="text-red-500 text-xs italic">{{ errorMessage }}</p>
</template>

<script setup lang="ts" generic="FieldType, DtoType extends GenericObject">
import { Field } from '@/utils/field-helper'
import type { GenericObject } from 'vee-validate'
import { computed } from 'vue'

const { field, messages } = defineProps<{
  field?: Field<FieldType, DtoType> | undefined
  messages?: string | string[] | undefined
}>()

const errorMessage = computed(() =>
  field && field.hasError ? field.error : Array.isArray(messages) ? messages.join('\n') : messages
)
</script>
