<template>
  <DataTable
    :value="advertisements"
    :loading="isLoading > 0"
    :rows="DefaultPageSize"
    :rowsPerPageOptions="PageSizeOptions"
    :currentPageReportTemplate="pageReportTemplate"
    :paginatorTemplate="paginatorTemplate"
    :totalRecords="totalRecordCount"
    :pt="dataTablePt"
    sortMode="multiple"
    class="flex-auto bg-white"
    removableSort
    paginator
    lazy
    @page="pageTable"
    @sort="sortTable"
  >
    <template #header>
      <h3 class="font-semibold text-2xl mb-2">{{ categoryName ?? categoryInfo.categoryName }}</h3>
      <div
        v-if="filterableColumns?.length || categoryFilterList.length"
        class="flex flex-row gap-2 flex-wrap justify-center"
      >
        <div class="flex-auto flex flex-row justify-center flex-wrap gap-2">
          <Select
            v-if="categoryFilterList.length"
            v-model="categoryFilterModel"
            :options="categoryFilterList"
            :placeholder="l.advertisements.selectCategory"
            optionLabel="value"
            optionValue="key"
          ></Select>
          <DynamicFilter
            v-for="column in filterableColumns"
            :key="column.id"
            v-model="filter['' + column.id]"
            :label="column.name"
            :filterType="column.attributeFilterType!"
            :valueType="column.attributeValueType!"
            :valueList="valueLists[column.valueListId ?? 0]"
            class="min-w-52"
          />
        </div>
        <div class="space-x-2">
          <Button severity="secondary" @click="clearFilter">{{ l.actions.clear }}</Button>
          <Button severity="primary" @click="filterTable">{{ l.actions.search }}</Button>
        </div>
      </div>
    </template>
    <Column field="id">
      <template #body="slotProps">
        <RouterLink :to="{ name: 'viewAdvertisement', params: { id: slotProps.data.id } }">
          <Panel class="hover:brightness-95">
            <div class="flex flex-row gap-2 items-center">
              <img
                :src="slotProps.data.thumbnailImageUrl"
                class="flex-none"
                width="100"
                height="100"
              />
              <div class="flex flex-col gap-2">
                <h4>{{ slotProps.data.title }}</h4>
                <p class="line-clamp-2">{{ slotProps.data.advertisementText }}</p>
                <div class="flex flex-row flex-wrap gap-2">
                  <span
                    v-for="attribute in slotProps.data.attributeValues"
                    :key="slotProps.data.id + '-' + attribute.attributeId"
                    :title="attribute.attributeName"
                  >
                    {{ attribute.valueName ?? attribute.value }}</span
                  >
                </div>
              </div>
            </div>
          </Panel>
        </RouterLink>
      </template>
    </Column>
    <Column
      v-for="column in sortableColumns"
      :key="column.id"
      :field="'' + column.id"
      :header="column.name"
      sortable
    >
    </Column>
  </DataTable>
</template>

<script setup lang="ts">
import DynamicFilter from '@/components/Filters/DynamicFilter.vue'
import { Direction } from '@/constants/api/Direction'
import { DefaultPageSize, PageSizeOptions } from '@/constants/data-table'
import {
  AdvertisementClient,
  AdvertisementListItem,
  AdvertisementListItemDataTableQueryResponse,
  AdvertisementQuery,
  AttributeOrderQuery,
  AttributeSearchQuery,
  AttributeValueItem,
  CategoryInfo,
  Int32StringKeyValuePair
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import {
  type ColumnPassThroughMethodOptions,
  type ColumnPassThroughOptions,
  type DataTablePageEvent,
  type DataTablePassThroughOptions,
  type DataTableSortEvent,
  type DataTableSortMeta
} from 'primevue'
import { computed, onMounted, ref, watch, type ComputedRef, type Ref } from 'vue'

const {
  advertisementSource,
  categoryId,
  categoryName,
  categoryFilterList = []
} = defineProps<{
  advertisementSource: (
    query: AdvertisementQuery
  ) => Promise<AdvertisementListItemDataTableQueryResponse>
  categoryId?: number | null | undefined
  categoryName?: string | undefined
  categoryFilterList?: Int32StringKeyValuePair[]
}>()

// Services
const advertisementService = getClient(AdvertisementClient)
const ls = LocaleService.get()
const l = LocaleService.currentLocale

// Table Constants
const paginatorTemplate =
  'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport'

const dataTablePt = {
  column: (columnOptions: ColumnPassThroughMethodOptions) => {
    const index = columnOptions?.context?.index
    const hasIndex = typeof index === 'number' && !isNaN(index)

    return {
      bodyCell: {
        class: [{ hidden: hasIndex && index > 0 }],
        colspan: hasIndex && index > 0 ? 0 : (sortableColumns.value?.length ?? 0) + 1
      },
      headerCell: {
        class: [{ hidden: hasIndex && index === 0 }]
      }
    } as ColumnPassThroughOptions
  }
} as DataTablePassThroughOptions

// Reactive data
const isLoading = ref(0)
const pageReportTemplate = ref('')
const categoryInfo: Ref<CategoryInfo> = ref(new CategoryInfo())

/** In data table filter selected category id */
const categoryFilterModel: Ref<number | undefined> = ref()

/** Resulting selected category id in filter or passed as prop */
const selectedCategoryId = computed(() =>
  categoryId !== undefined ? categoryId : categoryFilterModel.value
)

const attributeOrderQuery: Ref<AttributeOrderQuery[]> = ref([])
const attributeFilterQuery: Ref<AttributeSearchQuery[]> = ref([])
const pageFirstRecord = ref(0)
const pageRecordCount = ref(DefaultPageSize)
const totalRecordCount = ref(0)
const filter: Ref<{ [key: string]: (string | number)[] }> = ref({})
const advertisements: Ref<AdvertisementListItem[]> = ref([])

const sortableColumns = computed(() => categoryInfo.value.attributeInfo?.filter((a) => a.sortable))
const filterableColumns = computed(() =>
  categoryInfo.value.attributeInfo?.filter((a) => a.searchable)
)

const valueLists: ComputedRef<{ [key: number]: AttributeValueItem }> = computed(() => {
  const result: { [key: number]: AttributeValueItem } = {}
  if (!categoryInfo.value.attributeValueLists) {
    return result
  }

  for (const list of categoryInfo.value.attributeValueLists) {
    if (list.id) {
      result[list.id] = list
    }
  }
  return result
})

// Hooks
onMounted(() => {
  handleCategoryChange(selectedCategoryId.value)
  updatePagingTemplate()
})

// Watchers
watch(LocaleService.currentLocaleName, async () => {
  handleCategoryChange(selectedCategoryId.value)
  loadAdvertisements()
  updatePagingTemplate()
})

watch(
  () => selectedCategoryId.value,
  (newId) => {
    handleCategoryChange(newId)
  }
)

//Methods
const handleCategoryChange = async (selectedCategoryId?: number | null) => {
  isLoading.value++
  const promises = [loadAdvertisements()]
  if (typeof selectedCategoryId === 'number' && selectedCategoryId > -1) {
    promises.push(loadCategoryInfo())
  } else {
    categoryInfo.value = new CategoryInfo({
      categoryName: categoryName,
      attributeInfo: [],
      attributeValueLists: []
    })
  }
  await Promise.all(promises)
  isLoading.value--
}

const loadCategoryInfo = async () => {
  if (!selectedCategoryId.value) {
    return
  }
  categoryInfo.value = await advertisementService.getCategoryInfo(selectedCategoryId.value)
}

const loadAdvertisements = async () => {
  isLoading.value++

  const response = await advertisementSource(
    new AdvertisementQuery({
      categoryId: selectedCategoryId.value ?? undefined,
      attributeOrder: attributeOrderQuery.value,
      attributeSearch: attributeFilterQuery.value,
      start: pageFirstRecord.value,
      length: pageRecordCount.value,
      order: [],
      columns: []
    })
  )
  advertisements.value = response.data ?? []
  totalRecordCount.value = response.recordsFiltered ?? 0
  isLoading.value--
}

const pageTable = (event: DataTablePageEvent) => {
  pageFirstRecord.value = event.first
  pageRecordCount.value = event.rows

  loadAdvertisements()
}

const sortTable = (event: DataTableSortEvent) => {
  const sortInfo = event.multiSortMeta ?? [
    { field: event.sortField, order: event.sortOrder } as DataTableSortMeta
  ]
  attributeOrderQuery.value = sortInfo
    .map((s) => {
      //Make sure that order properties have value and convert them to needed data type
      const attributeIdString = typeof s.field === 'function' ? s.field(null) : s.field
      const attributeId = attributeIdString ? parseInt(attributeIdString) : undefined
      if ((s.order !== 1 && s.order !== -1) || attributeId == null || isNaN(attributeId)) {
        return undefined
      }

      return new AttributeOrderQuery({
        attributeId: attributeId,
        direction: s.order == 1 ? Direction.Ascending : Direction.Descending
      })
    })
    .filter((s) => s != null) as AttributeOrderQuery[]

  loadAdvertisements()
}

const filterTable = () => {
  attributeFilterQuery.value = Object.keys(filter.value)
    .map((k) => {
      const filterValue = filter.value[k]
      return new AttributeSearchQuery({
        attributeId: parseInt(k),
        value: typeof filterValue[0] === 'number' ? '' + filterValue[0] : filterValue[0],
        secondaryValue: typeof filterValue[1] === 'number' ? '' + filterValue[1] : filterValue[1]
      })
    })
    .filter((q) => q.value != null || q.secondaryValue != null)

  loadAdvertisements()
}

const clearFilter = () => {
  filter.value = {}
  categoryFilterModel.value = undefined
  filterTable()
}

const updatePagingTemplate = () => {
  pageReportTemplate.value = ls.l(
    'dataTable.pageReportTemplate',
    '{first}',
    '{last}',
    '{totalRecords}'
  )
}
</script>
