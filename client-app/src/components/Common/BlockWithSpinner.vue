<template>
  <BlockUI :blocked="loading">
    <slot></slot>
    <ComponentSpinner :show="loading" />
  </BlockUI>
</template>

<script setup lang="ts">
import ComponentSpinner from './ComponentSpinner.vue'

const { loading } = defineProps<{ loading: boolean }>()
</script>
