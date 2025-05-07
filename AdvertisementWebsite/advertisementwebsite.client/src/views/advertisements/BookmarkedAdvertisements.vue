<template>
  <ResponsiveLayout>
    <AdvertisementTable
      :categoryName="l.navigation.savedAdvertisements"
      :advertisementSource="loadAdvertisements"
      :categoryFilterList="advertisementCategories"
      :groupByCategory="true"
      multiRowSelect
    >
      <template #actionButtons="slotProps">
        <Button
          severity="danger"
          :disabled="!slotProps.selectedRows?.length"
          @click="
            confirmBookmarkDelete(slotProps.selectedRows, slotProps.setLoading, slotProps.refresh)
          "
          >{{ l.actions.remove }}</Button
        >
      </template>
    </AdvertisementTable>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/advertisements/AdvertisementTable.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import { confirmDelete } from '@/utils/confirm-dialog'
import {
  AdvertisementClient,
  AdvertisementListItem,
  Int32StringKeyValuePair,
  type AdvertisementQuery
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { useConfirm } from 'primevue'
import { onBeforeMount, ref } from 'vue'

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const advertisementService = getClient(AdvertisementClient)
const confirm = useConfirm()

//Reactive data
const advertisementCategories = ref<Int32StringKeyValuePair[]>([])

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

const confirmBookmarkDelete = (...params: Parameters<typeof removeBookmarks>) => {
  confirmDelete(confirm, {
    header: l.value.advertisementBookmark.confirmDeleteHeader,
    message: ls.l('advertisementBookmark.confirmDeleteMessage', params[0].length),
    accept: () => removeBookmarks(params[0], params[1], params[2])
  })
}

const removeBookmarks = async (
  selectedRows: AdvertisementListItem[],
  setLoading: (isLoading: boolean) => void,
  refresh: () => void
) => {
  setLoading(true)

  const ids = selectedRows.map((r) => r.id).filter((id) => typeof id === 'number') as number[]
  await advertisementService.removeAdvertisementBookmarks(ids)

  refresh()
  await loadCategoryList()

  setLoading(false)
}
</script>
