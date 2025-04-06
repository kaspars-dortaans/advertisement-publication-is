<template>
  <div
    v-if="show"
    class="absolute top-0 left-0 w-full h-full z-[3000] flex justify-center items-center"
  >
    <i class="pi pi-spin pi-spinner text-white text-[2rem]"></i>
  </div>
</template>

<script setup lang="ts">
const { show } = defineProps<{ show: boolean }>()
</script>
