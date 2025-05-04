<template>
  <BlockUI :blocked="loading" class="max-w-full">
    <slot></slot>
    <ComponentSpinner :show="loading" />
  </BlockUI>
</template>

<script setup lang="ts">
import ComponentSpinner from '@/components/common/ComponentSpinner.vue'

defineProps<{ loading: boolean }>()
</script>
