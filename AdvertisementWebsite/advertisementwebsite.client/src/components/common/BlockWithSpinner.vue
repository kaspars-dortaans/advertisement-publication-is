<template>
  <BlockUI :blocked="isLoading" class="max-w-full">
    <slot></slot>
    <ComponentSpinner :show="isLoading" />
  </BlockUI>
</template>

<script setup lang="ts">
import ComponentSpinner from '@/components/common/ComponentSpinner.vue'
import { computed } from 'vue'

const props = defineProps<{ loading: boolean | number }>()
const isLoading = computed(() => !!props.loading)
</script>
