<template>
  <div
    class="flex flex-col justify-center items-center h-full bg-primary overflow-y-auto lg:p-2 lg:flex-row"
  >
    <AdvertisementTable
      :categoryName="l.navigation.recentlyViewedAdvertisements"
      :advertisementSource="loadAdvertisements"
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
  AdvertisementListItemDataTableQueryResponse,
  AdvertisementQuery
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { IAdvertisementHistoryRecord } from '@/types/advertisements/advertisement-history-record'
import { getClient } from '@/utils/client-builder'
import { getStorageObject, updateStorageObject } from '@/utils/local-storage'
import { onBeforeMount, ref, type Ref } from 'vue'

//Services
const advertisementService = getClient(AdvertisementClient)
const l = LocaleService.currentLocale

//Reactive data
const advertisementIds: Ref<number[]> = ref([])

//Hooks

onBeforeMount(() => {
  updateStorageObject<IAdvertisementHistoryRecord[]>(
    AdvertisementHistoryStorageKey,
    (historyRecords) => {
      //Delete old history
      return historyRecords.filter(
        (r) => Date.now() - r.timeStamp < AdvertisementHistoryTimeSpanInMiliSeconds
      )
    },
    []
  )
  advertisementIds.value = (
    getStorageObject<IAdvertisementHistoryRecord[]>(AdvertisementHistoryStorageKey) ?? []
  )
    .sort((a, b) => (a.timeStamp > b.timeStamp ? -1 : 1)) //Sort descending
    .map((r) => r.id)
})

//Methods
const loadAdvertisements = async (query: AdvertisementQuery) => {
  const queryIds = advertisementIds.value.slice(
    query.start,
    (query?.start ?? 0) + (query?.length ?? 0)
  )
  const advertisements = await advertisementService.getAdvertisementsByIds(queryIds)
  return new AdvertisementListItemDataTableQueryResponse({
    data: advertisements,
    recordsTotal: advertisementIds.value.length,
    recordsFiltered: advertisementIds.value.length
  })
}
</script>
