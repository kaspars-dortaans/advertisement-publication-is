<template>
  <ResponsiveLayout>
    <DataTable
      v-model:selection="selectedRecords"
      v-model:editingRows="editingRecords"
      :value="tableRecords"
      :loading="loading"
      :rows="DefaultPageSize"
      :rowsPerPageOptions="PageSizeOptions"
      :currentPageReportTemplate="pageReportTemplate"
      :paginatorTemplate="PaginatorTemplate"
      :totalRecords="totalRecordCount"
      :selectionMode="selectionMode"
      :editMode="editMode"
      sortMode="multiple"
      class="bg-white flex-1 lg:flex-grow-0 lg:flex-shrink-1 lg:basis-auto lg:max-w-full rounded-none lg:rounded-md"
      removableSort
      paginator
      lazy
      @page="pageTable"
      @sort="sortTable"
      @rowSelect="emitRowSelect"
      @rowEditCancel="cancelRowEdit"
      @rowEditSave="saveRowEdit"
    >
      <template v-if="$slots.header" #header>
        <slot name="header" :addNewRow="addNewRow"></slot>
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
import type {
  DataTablePageEvent,
  DataTableRowEditCancelEvent,
  DataTableRowEditSaveEvent,
  DataTableRowSelectEvent,
  DataTableSortEvent,
  DataTableSortMeta
} from 'primevue'
import type { GenericObject } from 'vee-validate'
import { onBeforeMount, ref } from 'vue'

const props = defineProps<{
  dataSource: (query: DataTableQuery) => Promise<IDataTableQueryResponse_1>
  columns: TableColumn[]
  selectionMode?: 'single' | 'multiple'
  editMode?: string
  rowEditCancelHandler?: (e: DataTableRowEditCancelEvent) => void
  rowEditSaveHandler?: (e: DataTableRowEditSaveEvent) => void
}>()

//Output
const selectedRecords = defineModel<T[]>('selection')
const loading = defineModel<boolean>('loading')
const emit = defineEmits(['rowSelect'])

//Reactive data
const tableRecords = ref<T[]>([])
const editingRecords = ref<T[]>([])
const pageReportTemplate = ref('')
const sortQuery = ref<OrderQuery[]>([])
const searchQuery = ref<SearchQuery>()
const editingNewRow = ref(false)

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

const emitRowSelect = (e: DataTableRowSelectEvent) => {
  emit('rowSelect', e)
}

const addNewRow = () => {
  //Create empty row
  let row: GenericObject = {}

  for (const column of props.columns) {
    row[column.data!] = undefined
  }

  //Add row to table row source and set row status to edit
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  tableRecords.value.unshift(row as any)
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  editingRecords.value = [row as any]
  editingNewRow.value = true
}

const saveRowEdit = (e: DataTableRowEditSaveEvent) => {
  if (props.rowEditSaveHandler) {
    props.rowEditSaveHandler(e)
  } else {
    tableRecords.value[0] = e.newData
  }

  if (editingNewRow.value && e.index === 0) {
    editingNewRow.value = false
  }
}

const cancelRowEdit = (e: DataTableRowEditCancelEvent) => {
  if (props.rowEditCancelHandler) {
    props.rowEditCancelHandler(e)
  } else if (editingNewRow.value) {
    tableRecords.value.splice(0, 1)
  }

  if (editingNewRow.value && e.index === 0) {
    editingNewRow.value = false
  }
}

defineExpose({ refresh: loadTableRecords })
</script>
