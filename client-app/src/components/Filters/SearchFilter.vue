<template>
  <FloatLabel variant="on">
    <InputText type="text" v-model="model" :id="inputId"></InputText>
    <label :for="inputId">{{ props.label }}</label>
  </FloatLabel>
</template>

<script lang="ts" setup>
import { useId } from 'vue'

const props = defineProps<{
  label: string
}>()
const model = defineModel<string>()
const inputId = useId()
</script>
