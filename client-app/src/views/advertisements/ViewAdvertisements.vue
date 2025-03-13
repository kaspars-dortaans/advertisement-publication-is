<template>
  <ResponsiveLayout>
    <CategoryMenu
      v-model="categoryId"
      v-model:selectedCategoryName="selectedCategoryName"
      ref="categoryMenu"
      class="flex-none w-full max-h-[50%] lg:max-h-full lg:w-64"
    />
    <AdvertisementTable
      :categoryId="categoryId"
      :categoryName="selectedCategoryName"
      :advertisementSource="(q) => advertisementService.getAdvertisements(q)"
      class="flex-1"
    ></AdvertisementTable>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/AdvertisementTable.vue'
import CategoryMenu from '@/components/CategoryMenu.vue'
import ResponsiveLayout from '@/components/Common/ResponsiveLayout.vue'
import { AdvertisementClient } from '@/services/api-client'
import { getClient } from '@/utils/client-builder'
import { ref, type Ref } from 'vue'

//Services
const advertisementService = getClient(AdvertisementClient)

//Reactive data
const categoryId: Ref<number | null> = ref(null)
const selectedCategoryName: Ref<string> = ref('')
</script>
