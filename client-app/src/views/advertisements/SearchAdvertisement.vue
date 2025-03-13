<template>
  <ResponsiveLayout>
    <AdvertisementTable
      :advertisementSource="(q) => loadAdvertisements(q)"
      :groupByCategory="true"
      ref="advertisementTable"
      class="flex-1"
    >
      <template #title>
        <div class="flex flex-row gap-4 items-baseline">
          <BackButton :defaultTo="{ name: 'home' }" />
          <h3 class="font-semibold text-2xl mb-2">{{ title }}</h3>
        </div>
      </template>
    </AdvertisementTable>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/AdvertisementTable.vue'
import BackButton from '@/components/BackButton.vue'
import ResponsiveLayout from '@/components/Common/ResponsiveLayout.vue'
import {
  AdvertisementClient,
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
const { search } = defineProps<{ search: string }>()

//Services
const advertisementService = getClient(AdvertisementClient)
const l = LocaleService.currentLocale

//Refs
const advertisementTable = useTemplateRef('advertisementTable')

//Reactive data
const title = computed(() => {
  return `${l.value.advertisements.search} '${search}'`
})

//Hooks
onBeforeMount(() => {
  if (!search) {
    push({ name: 'home' })
  }
})

//Watchers
watch(
  () => search,
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
    value: search
  })

  return advertisementService.getAdvertisements(q)
}
</script>
