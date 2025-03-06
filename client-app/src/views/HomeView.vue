<template>
  <div
    class="flex flex-row justify-center items-center gap-3 h-full bg-primary p-2 flex-wrap lg:flex-nowrap overflow-y-auto"
  >
    <CategoryMenu
      v-model="categoryId"
      ref="categoryMenu"
      class="flex-none w-full lg:w-64 max-h-[50%] lg:max-h-full"
    />
    <AdvertisementTable
      :categoryId="categoryId"
      :categoryNameSource="getCategoryName"
      :advertisementSource="(q) => advertisementService.getAdvertisements(q)"
    ></AdvertisementTable>
  </div>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/AdvertisementTable.vue'
import CategoryMenu from '@/components/CategoryMenu.vue'
import { AdvertisementClient } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { ref, useTemplateRef, type Ref } from 'vue'

//Services
const advertisementService = getClient(AdvertisementClient)

//Refs
const categoryMenu = useTemplateRef('categoryMenu')

//Reactive data
const categoryId: Ref<number | null> = ref(null)

const getCategoryName = () => {
  return categoryMenu.value?.getCategoryName(
    categoryId.value,
    LocaleService.currentLocaleName.value
  )
}
</script>
