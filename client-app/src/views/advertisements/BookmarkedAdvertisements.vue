<template>
  <ResponsiveLayout>
    <AdvertisementTable
      :categoryName="l.navigation.savedAdvertisements"
      :advertisementSource="loadAdvertisements"
      :categoryFilterList="advertisementCategories"
      :groupByCategory="true"
      multiRowSelect
      class="flex-1"
    >
      <template #actionButtons="slotProps">
        <Button
          severity="danger"
          @click="removeBookmarks(slotProps.selectedRows, slotProps.setLoading, slotProps.refresh)"
          >{{ l.actions.remove }}</Button
        >
      </template>
    </AdvertisementTable>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/AdvertisementTable.vue'
import ResponsiveLayout from '@/components/Common/ResponsiveLayout.vue'
import {
  AdvertisementClient,
  AdvertisementListItem,
  Int32StringKeyValuePair,
  type AdvertisementQuery
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { onBeforeMount, ref, type Ref } from 'vue'

//Services
const l = LocaleService.currentLocale
const advertisementService = getClient(AdvertisementClient)

//Reactive data
const advertisementCategories: Ref<Int32StringKeyValuePair[]> = ref([])

//Hooks
onBeforeMount(() => {
  loadCategoryList()
})

//Methods
const loadAdvertisements = (q: AdvertisementQuery) => {
  return advertisementService.getBookmarkedAdvertisements(q)
}

const loadCategoryList = async () => {
  advertisementCategories.value =
    await advertisementService.getBookmarkedAdvertisementCategoryList()
}

const removeBookmarks = async (
  selectedRows: AdvertisementListItem[],
  setLoading: (isLoading: boolean) => void,
  refresh: () => void
) => {
  setLoading(true)

  const ids = selectedRows.map((r) => r.id).filter((id) => typeof id === 'number')
  await advertisementService.removeAdvertisementBookmarks(ids)

  refresh()
  await loadCategoryList()

  setLoading(false)
}
</script>
