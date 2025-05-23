<template>
  <ResponsiveLayout>
    <AdvertisementTable
      :advertisementSource="(q: DataTableQuery) => loadAdvertisements(q)"
      :groupByCategory="true"
      ref="advertisementTable"
    >
      <template #title>
        <div class="panel-title-container">
          <BackButton :defaultTo="{ name: 'home' }" />
          <h3 class="page-title mb-2">{{ title }}</h3>
        </div>
      </template>
    </AdvertisementTable>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/AdvertisementTable.vue'
import BackButton from '@/components/common/BackButton.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import {
  AdvertisementClient,
  DataTableQuery,
  SearchQuery,
  TableColumn,
  type AdvertisementQuery
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { computed, onBeforeMount, useTemplateRef, watch } from 'vue'
import { useRouter } from 'vue-router'

const { push } = useRouter()

//Props
const props = defineProps<{ search: string }>()

//Services
const advertisementService = getClient(AdvertisementClient)
const l = LocaleService.currentLocale

//Refs
const advertisementTable = useTemplateRef('advertisementTable')

//Reactive data
const title = computed(() => {
  return `${l.value.advertisements.search} '${props.search}'`
})

//Hooks
onBeforeMount(() => {
  if (!props.search) {
    push({ name: 'home' })
  }
})

//Watchers
watch(
  () => props.search,
  (newSearchValue: string) => {
    if (newSearchValue) {
      advertisementTable.value?.refresh()
    }
  }
)

//Methods
const loadAdvertisements = (q: AdvertisementQuery) => {
  //Patch query, to search advertisements text & title for user input
  q.columns = [
    new TableColumn({
      data: 'advertisementText',
      name: 'Advertisement Text',
      searchable: true
    }),
    new TableColumn({
      data: 'title',
      name: 'Title',
      searchable: true
    })
  ]
  q.search = new SearchQuery({
    value: props.search
  })

  return advertisementService.getAdvertisements(q)
}
</script>
