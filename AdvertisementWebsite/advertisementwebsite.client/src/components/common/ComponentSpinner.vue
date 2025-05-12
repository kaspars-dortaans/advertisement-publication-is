<template>
  <div
    v-if="show"
    class="absolute top-0 left-0 w-full h-full flex justify-center items-center"
    :style="{ 'z-index': zIndex }"
  >
    <i class="pi pi-spin pi-spinner text-white text-[2rem]"></i>
  </div>
</template>

<script setup lang="ts">
import { usePrimeVue } from 'primevue'
import { computed } from 'vue'

defineProps<{ show: boolean }>()
const primevue = usePrimeVue()
const zIndex = computed(() => (primevue?.config?.zIndex?.modal ?? 1000) + 10)
</script>
