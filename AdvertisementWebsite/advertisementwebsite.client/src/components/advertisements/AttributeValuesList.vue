<template>
  <div class="flex flex-row flex-wrap gap-2">
    <p
      v-for="attribute in attributeValues"
      :key="advertisementId + '-' + attribute.attributeId"
      :title="attribute.attributeName"
      class="inline-flex items-center gap-1 border border-surface-400 rounded-md py-1 px-2"
    >
      <img
        v-if="attribute.iconName"
        :src="'/src/assets/icons/' + attribute.iconName"
        :alt="attribute.attributeName"
        :title="attribute.attributeName"
        class="w-6 h-6 object-cover"
      />
      <span v-else>{{ attribute.attributeName }}:</span>
      <span>{{ attribute.valueName ?? attribute.value }}</span>
    </p>
  </div>
</template>

<script lang="ts" setup>
import type { AttributeValueItem } from '@/services/api-client'

defineProps<{
  advertisementId?: number
  attributeValues?: AttributeValueItem[]
}>()
</script>
