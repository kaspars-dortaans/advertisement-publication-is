<template>
  <BlockUI :blocked="loading">
    <slot></slot>
    <ComponentSpinner :show="loading" />
  </BlockUI>
</template>

<script setup lang="ts">
import ComponentSpinner from '@/components/common/ComponentSpinner.vue'

defineProps<{ loading: boolean }>()
</script>
