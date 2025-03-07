<template>
  <div
    class="flex flex-row justify-center items-center gap-3 h-full bg-primary p-2 flex-wrap lg:flex-nowrap overflow-y-auto"
  >
    <CategoryMenu
      v-model="categoryId"
      v-model:selectedCategoryName="selectedCategoryName"
      ref="categoryMenu"
      class="flex-none w-full lg:w-64 max-h-[50%] lg:max-h-full"
    />
    <AdvertisementTable
      :categoryId="categoryId"
      :categoryName="selectedCategoryName"
      :advertisementSource="(q) => advertisementService.getAdvertisements(q)"
    ></AdvertisementTable>
  </div>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/AdvertisementTable.vue'
import CategoryMenu from '@/components/CategoryMenu.vue'
import { AdvertisementClient } from '@/services/api-client'
import { getClient } from '@/utils/client-builder'
import { ref, type Ref } from 'vue'

//Services
const advertisementService = getClient(AdvertisementClient)

//Reactive data
const categoryId: Ref<number | null> = ref(null)
const selectedCategoryName: Ref<string> = ref('')
</script>
