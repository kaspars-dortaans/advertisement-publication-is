<template>
  <ResponsiveLayout>
    <AdvertisementTable
      :title="l.navigation.savedAdvertisements"
      :advertisementSource="loadAdvertisements"
      :categoryFilterList="advertisementCategories"
      :groupByCategory="true"
      multiRowSelect
      ref="table"
    >
      <template #actionButtons="slotProps">
        <Button
          severity="secondary"
          :disabled="!slotProps.selectedRows?.length"
          @click="printSelected"
        >
          {{ l.actions.print }}
        </Button>
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
import { onBeforeMount, ref, useTemplateRef } from 'vue'

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const advertisementService = getClient(AdvertisementClient)
const confirm = useConfirm()

//Refs
const table = useTemplateRef('table')

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

const printSelected = async () => {
  //Get table html
  const tableHtml = table.value!.$el.outerHTML

  //Get page styles
  let styleHtml = ''
  const pageStyles = document.querySelectorAll('style')
  for (const pageStyle of pageStyles) {
    styleHtml += pageStyle.outerHTML + '\n'
  }

  //Open new tab with printable table
  const newWindow = window.open()!
  newWindow.document.head.innerHTML = styleHtml
  newWindow.document.body.innerHTML = tableHtml
  newWindow.document.title = l.value.navigation.savedAdvertisements

  //Remove rows that are not selected
  const removableElements = []
  const rows = newWindow.document.querySelectorAll('tbody tr')
  let rowsInGroup = 0
  let currentGroup = rows[0]
  for (let i = 1; i < rows.length; i++) {
    if (rows[i].getAttribute('data-pc-section') === 'rowgroupheader') {
      if (rowsInGroup < 1) {
        removableElements.push(currentGroup)
      }
      currentGroup = rows[i]
      rowsInGroup = 0
    } else {
      if (!rows[i].querySelector('input')?.hasAttribute('checked')) {
        removableElements.push(rows[i])
      } else {
        rowsInGroup++
      }
    }
  }
  if (rowsInGroup < 1) {
    removableElements.push(currentGroup)
  }

  //Remove unnecessary table elements
  removableElements.push(...newWindow.document.querySelectorAll('nav'))
  removableElements.push(...newWindow.document.querySelectorAll('h3 ~ div'))
  removableElements.push(...newWindow.document.querySelectorAll('thead'))
  removableElements.push(
    ...newWindow.document.querySelectorAll('tr[data-pc-section="bodyrow"] td:first-child')
  )
  removableElements.forEach((n) => n.remove())

  //Await when images has loaded
  await Promise.all(
    Array.from(document.images)
      .filter((img) => !img.complete)
      .map(
        (img) =>
          new Promise((resolve) => {
            img.onload = img.onerror = resolve
          })
      )
  )

  //Wait for page to fully render in Google Chrome
  await new Promise((resolve) => setTimeout(resolve, 500))

  //Print and close
  newWindow.print()
  setTimeout(() => newWindow.close(), 0)
}
</script>
