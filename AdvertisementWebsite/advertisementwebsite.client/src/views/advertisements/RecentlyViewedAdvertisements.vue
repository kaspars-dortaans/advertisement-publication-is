<template>
  <AdvertisementTable
    :title="l.navigation.recentlyViewedAdvertisements"
    :advertisementSource="loadAdvertisements"
    :categoryFilterList="advertisementCategories"
  ></AdvertisementTable>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/advertisements/AdvertisementTable.vue'
import {
  AdvertisementHistoryStorageKey,
  AdvertisementHistoryTimeSpanInMiliSeconds
} from '@/constants/advertisement-history'
import {
  AdvertisementClient,
  AdvertisementQuery,
  CategoryClient,
  Int32StringKeyValuePair
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { IAdvertisementHistoryRecord } from '@/types/advertisements/advertisement-history-record'
import { getClient } from '@/utils/client-builder'
import { updateStorageObject } from '@/utils/local-storage'
import { onBeforeMount, ref, watch } from 'vue'

//Services
const advertisementService = getClient(AdvertisementClient)
const categoryService = getClient(CategoryClient)
const l = LocaleService.currentLocale

//Reactive data
const advertisementIds = ref<number[]>([])
const advertisementCategories = ref<Int32StringKeyValuePair[]>([])

//Hooks
onBeforeMount(async () => {
  const advertisementHistory = updateStorageObject<IAdvertisementHistoryRecord[]>(
    AdvertisementHistoryStorageKey,
    (historyRecords) => {
      //Delete old history
      return historyRecords.filter(
        (r) => Date.now() - r.timeStamp < AdvertisementHistoryTimeSpanInMiliSeconds
      )
    },
    []
  )
  advertisementIds.value = advertisementHistory
    .sort((a, b) => (a.timeStamp > b.timeStamp ? -1 : 1)) //Sort descending
    .map((r) => r.id)

  advertisementCategories.value = await categoryService.getCategoryListFromAdvertisementIds(
    advertisementIds.value
  )
})

//watchers
watch(LocaleService.currentLocale, async () => {
  advertisementCategories.value = await categoryService.getCategoryListFromAdvertisementIds(
    advertisementIds.value
  )
})

//Methods
const loadAdvertisements = async (query: AdvertisementQuery) => {
  query.advertisementIds = advertisementIds.value
  return await advertisementService.getAdvertisements(query)
}
</script>
