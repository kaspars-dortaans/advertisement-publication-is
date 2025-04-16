<template>
  <div class="flex-1 overflow-y-auto">
    <div
      class="min-w-full min-h-full bg-primary mx-auto flex flex-col flex-nowrap lg:flex-row lg:justify-center lg:items-start lg:gap-2 lg:p-2"
      :class="containerClass"
    >
      <slot> </slot>
    </div>
  </div>
</template>

<script lang="ts" setup>
const { containerClass } = defineProps<{
  containerClass?: string
}>()
</script>
