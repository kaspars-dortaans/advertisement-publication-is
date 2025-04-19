<template>
  <ResponsiveLayout>
    <DataTable
      v-model:selection="selectedRecords"
      :value="tableRecords"
      :loading="loading"
      :rows="DefaultPageSize"
      :rowsPerPageOptions="PageSizeOptions"
      :currentPageReportTemplate="pageReportTemplate"
      :paginatorTemplate="PaginatorTemplate"
      :totalRecords="totalRecordCount"
      sortMode="multiple"
      class="bg-white flex-1 lg:flex-none rounded-none lg:rounded-md"
      removableSort
      paginator
      lazy
      @page="pageTable"
      @sort="sortTable"
    >
      <template v-if="$slots.header" #header>
        <slot name="header"></slot>
      </template>

      <slot></slot>
    </DataTable>
  </ResponsiveLayout>
</template>

<script setup lang="ts" generic="T extends object">
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import { Direction } from '@/constants/api/Direction'
import { DefaultPageSize, PageSizeOptions, PaginatorTemplate } from '@/constants/data-table'
import {
  DataTableQuery,
  OrderQuery,
  SearchQuery,
  TableColumn,
  type IDataTableQueryResponse_1
} from '@/services/api-client'
import { getPageReportTemplate } from '@/utils/data-table'
import type { DataTablePageEvent, DataTableSortEvent, DataTableSortMeta } from 'primevue'
import { onBeforeMount, ref } from 'vue'

const props = defineProps<{
  dataSource: (query: DataTableQuery) => Promise<IDataTableQueryResponse_1>
  columns: TableColumn[]
}>()

//Output
const selectedRecords = defineModel<T[]>('selection')
const loading = defineModel<boolean>('loading')

//Reactive data
const tableRecords = ref<T[]>([])
const pageReportTemplate = ref('')
const sortQuery = ref<OrderQuery[]>([])
const searchQuery = ref<SearchQuery>()

/** Index for first record on page for api request */
const pageFirstRecord = ref(0)

/** Record count to fetch from api */
const pageRecordCount = ref(DefaultPageSize)

/** Total record count in Db for request */
const totalRecordCount = ref(0)

//Hooks
onBeforeMount(() => {
  loading.value = true
  pageReportTemplate.value = getPageReportTemplate()
  loadTableRecords()
})

//Methods
const pageTable = (event: DataTablePageEvent) => {
  pageFirstRecord.value = event.first
  pageRecordCount.value = event.rows

  loadTableRecords()
}

const sortTable = (event: DataTableSortEvent) => {
  const sortInfo = event.multiSortMeta ?? [
    { field: event.sortField, order: event.sortOrder } as DataTableSortMeta
  ]

  sortQuery.value = sortInfo.map((s) => {
    const columnField = typeof s.field === 'function' ? s.field(null) : s.field
    return new OrderQuery({
      column: props.columns.findIndex((c) => c.data === columnField),
      direction: s.order == 1 ? Direction.Ascending : Direction.Descending
    })
  })

  loadTableRecords()
}

const loadTableRecords = async () => {
  loading.value = true

  const response = await props.dataSource({
    columns: props.columns,
    start: pageFirstRecord.value,
    length: pageRecordCount.value,
    order: sortQuery.value,
    search: searchQuery.value
  } as DataTableQuery)

  tableRecords.value = (response.data as T[]) ?? []
  totalRecordCount.value = response.recordsTotal ?? 0
  selectedRecords.value = []

  loading.value = false
}

defineExpose({ refresh: loadTableRecords })
</script>
