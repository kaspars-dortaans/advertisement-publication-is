<template>
  <DataTable
    v-model:expandedRowGroups="expandedRowGroups"
    v-model:selection="selectedRows"
    :value="advertisements"
    :loading="loadCount > 0"
    :rows="DefaultPageSize"
    :rowsPerPageOptions="PageSizeOptions"
    :currentPageReportTemplate="pageReportTemplate"
    :paginatorTemplate="PaginatorTemplate"
    :totalRecords="totalRecordCount"
    :pt="dataTablePt"
    :rowGroupMode="groupByCategory ? 'subheader' : undefined"
    :expandableRowGroups="groupByCategory"
    groupRowsBy="categoryName"
    sortMode="multiple"
    class="bg-white flex-1 rounded-none lg:rounded-md"
    removableSort
    paginator
    lazy
    @page="pageTable"
    @sort="sortTable"
  >
    <template #header>
      <slot name="title">
        <h3 class="page-title mb-2">{{ categoryName ?? categoryInfo.categoryName }}</h3>
      </slot>
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
          <slot
            name="actionButtons"
            :selectedRows="selectedRows"
            :setLoading="setLoading"
            :refresh="loadAdvertisements"
          ></slot>
        </div>
      </div>
    </template>

    <template #groupheader="slotProps">
      <div class="inline-block ml-2">
        <h3 class="text-xl mb-2">{{ slotProps.data.categoryName }}</h3>
      </div>
    </template>

    <Column
      v-if="multiRowSelect"
      selectionMode="multiple"
      headerStyle="width: 3rem"
      body-style="width: 3rem"
    ></Column>

    <Column :field="advertisementColumnField">
      <template #body="slotProps">
        <RouterLink :to="{ name: 'viewAdvertisement', params: { id: slotProps.data.id } }">
          <Panel class="hover:brightness-95">
            <div class="flex flex-row gap-2 items-center">
              <img
                :src="slotProps.data.thumbnailImageUrl"
                class="flex-none w-28 h-28 object-cover"
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
import DynamicFilter from '@/components/filters/DynamicFilter.vue'
import { Direction } from '@/constants/api/Direction'
import { DefaultPageSize, PageSizeOptions, PaginatorTemplate } from '@/constants/data-table'
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
import { getPageReportTemplate } from '@/utils/data-table'
import {
  type ColumnPassThroughMethodOptions,
  type ColumnPassThroughOptions,
  type DataTablePageEvent,
  type DataTablePassThroughOptions,
  type DataTableSortEvent,
  type DataTableSortMeta
} from 'primevue'
import { computed, onMounted, ref, watch, type ComputedRef } from 'vue'

const {
  advertisementSource,
  categoryId,
  categoryName,
  categoryFilterList = [],
  groupByCategory = false,
  multiRowSelect = false
} = defineProps<{
  advertisementSource: (
    query: AdvertisementQuery
  ) => Promise<AdvertisementListItemDataTableQueryResponse>
  categoryId?: number | null | undefined
  categoryName?: string | undefined
  categoryFilterList?: Int32StringKeyValuePair[]
  groupByCategory?: boolean
  multiRowSelect?: boolean
}>()

// Services
const advertisementService = getClient(AdvertisementClient)
const l = LocaleService.currentLocale

// Table Constants
const dataTablePt = {
  //Hide empty advertisement attribute column body cells and advertisement header cell
  column: (columnOptions: ColumnPassThroughMethodOptions) => {
    const field = columnOptions?.props?.field
    const hasFieldProp = field != null
    const isAdvertisementColumn = field === advertisementColumnField

    return {
      bodyCell: {
        class: [{ hidden: hasFieldProp && !isAdvertisementColumn }],
        colspan: !hasFieldProp
          ? 1
          : isAdvertisementColumn
            ? (sortableColumns.value?.length ?? 0) + 1
            : 0
      },
      headerCell: {
        class: [{ hidden: hasFieldProp && isAdvertisementColumn }],
        colspan: !hasFieldProp ? 2 : isAdvertisementColumn ? 0 : 1
      }
    } as ColumnPassThroughOptions
  }
} as DataTablePassThroughOptions

const advertisementColumnField = 'id'

// Reactive data
/** When value is bigger than 0, it indicates that data is being loaded and data table input should be blocked.  */
const loadCount = ref(0)

/** Pagination message template, should be updated on language change */
const pageReportTemplate = ref('')

/** Holds data for selected category, if any */
const categoryInfo = ref<CategoryInfo>(new CategoryInfo())

/** In data table filter selected category id */
const categoryFilterModel = ref<number | undefined>()

/** Resulting selected category id in filter or passed as prop */
const selectedCategoryId = computed(() =>
  categoryId !== undefined ? categoryId : categoryFilterModel.value
)

/** Data table expanded group v-model */
const expandedRowGroups = ref()

/** Data table selected row model */
const selectedRows = ref()

/** Sorting query for api request */
const attributeOrderQuery = ref<AttributeOrderQuery[]>([])

/** Filter query for api request */
const attributeFilterQuery = ref<AttributeSearchQuery[]>([])

/** Index for first record on page for api request */
const pageFirstRecord = ref(0)

/** Record count to fetch from api */
const pageRecordCount = ref(DefaultPageSize)

/** Total record count in Db for request */
const totalRecordCount = ref(0)

/** Data table category filter models */
const filter = ref<{ [key: string]: (string | number)[] }>({})

/** Loaded advertisements from api */
const advertisements = ref<AdvertisementListItem[]>([])

/** Columns with sortable flag */
const sortableColumns = computed(() => categoryInfo.value.attributeInfo?.filter((a) => a.sortable))

/** Columns with filterable flag */
const filterableColumns = computed(() =>
  categoryInfo.value.attributeInfo?.filter((a) => a.searchable)
)

/** Current category select filter value lists */
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
  pageReportTemplate.value = getPageReportTemplate()
})

// Watchers
watch(LocaleService.currentLocaleName, async () => {
  handleCategoryChange(selectedCategoryId.value)
  loadAdvertisements()
  pageReportTemplate.value = getPageReportTemplate()
})

watch(
  () => selectedCategoryId.value,
  (newId) => {
    handleCategoryChange(newId)
  }
)

//Methods
const handleCategoryChange = async (selectedCategoryId?: number | null) => {
  setLoading(true)
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
  setLoading(false)
}

const loadCategoryInfo = async () => {
  if (!selectedCategoryId.value) {
    return
  }
  categoryInfo.value = await advertisementService.getCategoryInfo(selectedCategoryId.value)
}

const loadAdvertisements = async () => {
  setLoading(true)

  //Load advertisements
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

  //Expand groups
  if (groupByCategory) {
    const allCategoryNames = [...new Set(advertisements.value.map((a) => a.categoryName))]
    expandedRowGroups.value = allCategoryNames
  }

  setLoading(false)
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

/** Set data table loading state */
const setLoading = (isLoading: boolean) => {
  loadCount.value = Math.max(0, isLoading ? loadCount.value + 1 : loadCount.value - 1)
}

defineExpose({ refresh: loadAdvertisements })
</script>
