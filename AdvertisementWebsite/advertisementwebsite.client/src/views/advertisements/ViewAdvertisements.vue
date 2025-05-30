<template>
  <ResponsiveLayout>
    <CategoryMenu
      v-model="categoryId"
      v-model:selectedCategoryName="selectedCategoryName"
      ref="categoryMenu"
      class="w-full lg:w-64 lg:sticky lg:top-0 lg:left-0"
    />
    <AdvertisementTable
      :categoryId="categoryId"
      :title="selectedCategoryName"
      :advertisementSource="(q) => advertisementService.getAdvertisements(q)"
    ></AdvertisementTable>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/advertisements/AdvertisementTable.vue'
import CategoryMenu from '@/components/CategoryMenu.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import { AdvertisementClient } from '@/services/api-client'
import { getClient } from '@/utils/client-builder'
import { ref } from 'vue'

//Services
const advertisementService = getClient(AdvertisementClient)

//Reactive data
const categoryId = ref<number | null>(null)
const selectedCategoryName = ref<string>('')
</script>
