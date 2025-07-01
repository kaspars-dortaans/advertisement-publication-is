<template>
  <AdvertisementTable
    :advertisementSource="(q: DataTableQuery) => loadAdvertisements(q)"
    :groupByCategory="true"
    :categoryFilterList="categoryList"
    ref="advertisementTable"
  >
    <template #title>
      <div class="panel-title-container">
        <BackButton :defaultTo="{ name: 'home' }" />
        <h3 class="page-title mb-2">{{ title }}</h3>
      </div>
    </template>
  </AdvertisementTable>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/advertisements/AdvertisementTable.vue'
import BackButton from '@/components/common/BackButton.vue'
import {
  AdvertisementClient,
  CategoryClient,
  DataTableQuery,
  Int32StringKeyValuePair,
  SearchQuery,
  TableColumn,
  type AdvertisementQuery
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { ref } from 'vue'
import { computed, onBeforeMount, useTemplateRef, watch } from 'vue'
import { useRouter } from 'vue-router'

const { push } = useRouter()

//Props
const props = defineProps<{ search: string }>()

//Services
const advertisementService = getClient(AdvertisementClient)
const categoryService = getClient(CategoryClient)
const l = LocaleService.currentLocale

//Refs
const advertisementTable = useTemplateRef('advertisementTable')
const categoryList = ref<Int32StringKeyValuePair[]>([])

//Reactive data
const title = computed(() => {
  return `${l.value.advertisements.search} '${props.search}'`
})

//Hooks
onBeforeMount(async () => {
  if (!props.search) {
    push({ name: 'home' })
  }

  categoryList.value = await categoryService.getCategoryLookup()
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

watch(LocaleService.currentLocaleName, async () => {
  categoryList.value = await categoryService.getCategoryLookup()
})

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
