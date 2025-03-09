<template>
  <div
    class="flex flex-col justify-center items-center h-full bg-primary overflow-y-auto lg:p-2 lg:flex-row"
  >
    <AdvertisementTable
      :categoryName="l.navigation.recentlyViewedAdvertisements"
      :advertisementSource="loadAdvertisements"
      :categoryFilterList="advertisementCategories"
      class="flex-1"
    ></AdvertisementTable>
  </div>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/AdvertisementTable.vue'
import {
  AdvertisementHistoryStorageKey,
  AdvertisementHistoryTimeSpanInMiliSeconds
} from '@/constants/advertisement-history'
import {
  AdvertisementClient,
  AdvertisementQuery,
  Int32StringKeyValuePair
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { IAdvertisementHistoryRecord } from '@/types/advertisements/advertisement-history-record'
import { getClient } from '@/utils/client-builder'
import { updateStorageObject } from '@/utils/local-storage'
import { onBeforeMount, ref, type Ref } from 'vue'

//Services
const advertisementService = getClient(AdvertisementClient)
const l = LocaleService.currentLocale

//Reactive data
const advertisementIds: Ref<number[]> = ref([])
const advertisementCategories: Ref<Int32StringKeyValuePair[]> = ref([])

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

  advertisementCategories.value = await advertisementService.getCategoryListFromAdvertisementIds(
    advertisementIds.value
  )
})

//Methods
const loadAdvertisements = async (query: AdvertisementQuery) => {
  query.advertisementIds = advertisementIds.value
  return await advertisementService.getAdvertisements(query)
}
</script>
