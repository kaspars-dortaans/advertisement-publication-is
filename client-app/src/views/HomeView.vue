<template>
  <div class="flex flex-row justify-center items-center gap-3 h-full bg-primary p-2">
    <div class="">
      <CategoryMenu
        v-model="categoryId"
        ref="categoryMenu"
        @category-selected="handleSelectedCategory"
      />
    </div>

    <Panel class="flex-1">
      <DataTable
        :value="advertisements"
        :rows="defaultPageSize"
        :loading="isLoading > 0"
        :rowsPerPageOptions="pageSizeOptions"
        :currentPageReportTemplate="pageReportTemplate"
        :paginatorTemplate="paginatorTemplate"
        paginator
      >
        <template #header>
          <div class="flex flex-row justify-between items-baseline flex-wrap">
            <h3>{{ categoryInfo.categoryName }}</h3>
            <DynamicFilter
              v-for="column in filterableColumns"
              :key="column.id"
              v-model="filter['' + column.id]"
              :label="column.name"
              :filterType="column.attributeFilterType!"
              :valueType="column.attributeValueType!"
              :valueList="valueLists[column.valueListId ?? 0]"
            />
            <Button @click="loadAdvertisements">{{ ls.l('actions.search') }}</Button>
          </div>
        </template>
        <Column field="id">
          <template #body="slotProps">
            <Panel>
              <div class="flex flex-row gap-2">
                <img :src="slotProps.data.thumbnailImagePath" width="100" height="100" />
                <div class="flex flex-col gap-2">
                  <h4>{{ slotProps.data.title }}</h4>
                  <p>{{ slotProps.data.advertisementText }}</p>
                  <div class="flex flex-row flex-wrap">
                    <span
                      v-for="attribute in slotProps.data.attributeValues"
                      :key="slotProps.data.id + '-' + attribute.attributeId"
                    >
                      {{ attribute.value }}</span
                    >
                  </div>
                </div>
              </div>
            </Panel>
          </template>
        </Column>
      </DataTable>
    </Panel>
  </div>
</template>

<script setup lang="ts">
import CategoryMenu from '@/components/CategoryMenu.vue'
import DynamicFilter from '@/components/Filters/DynamicFilter.vue'
import {
  AdvertisementClient,
  AdvertisementListItem,
  AdvertismentQuery,
  AttributeSearchQuery,
  AttributeValueItem,
  CategoryInfo
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { computed, onMounted, ref, useTemplateRef, watch, type ComputedRef, type Ref } from 'vue'

// Services
const advertisementService = getClient(AdvertisementClient)
const ls = LocaleService.get()

// Refs
const categoryMenu = useTemplateRef('categoryMenu')

// Table Contants
const pageSizeOptions = [5, 10, 20, 50] //TODO: create constant with list options
const defaultPageSize = pageSizeOptions[1]
const paginatorTemplate =
  'FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown CurrentPageReport'

// Reactive data
const categoryId: Ref<number | null | undefined> = ref()
const categoryInfo: Ref<CategoryInfo> = ref(new CategoryInfo())
const filterableColumns = computed(() =>
  categoryInfo.value.attributeInfo?.filter((a) => a.searchable)
)
const filter: Ref<{ [key: string]: (string | number)[] }> = ref({})
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

const sortableColumns = computed(() => categoryInfo.value.attributeInfo?.filter((a) => a.sortable))

const advertisements: Ref<AdvertisementListItem[]> = ref([])
const isLoading = ref(0)
let pageReportTemplate = ''

// Hooks
onMounted(() => {
  categoryId.value = null
  handleSelectedCategory(categoryId.value, categoryMenu.value?.getCategoryName(categoryId.value))
})

// Watchers
watch(
  ls.currentLocale,
  () => {
    pageReportTemplate = ls.l('dataTable.pageReportTemplate', '{first}', '{last}', '{totalRecords}')
  },
  { immediate: true }
)

watch(ls.currentLocale, () => {
  categoryInfo.value.categoryName = categoryMenu.value?.getCategoryName(categoryId.value)
})

//Methods
const handleSelectedCategory = async (
  selectedCategoryId?: number | null,
  selectedCategoryName?: string
) => {
  isLoading.value++
  const promises = [loadAdvertisements()]
  if (typeof selectedCategoryId === 'number' && selectedCategoryId > -1) {
    promises.push(loadCategoryInfo())
  } else {
    categoryInfo.value = new CategoryInfo({
      categoryName: selectedCategoryName,
      attributeInfo: [],
      attributeValueLists: []
    })
  }
  await Promise.all(promises)
  isLoading.value--
}

const loadCategoryInfo = async () => {
  if (!categoryId.value) {
    return
  }
  categoryInfo.value = await advertisementService.getCategoryInfo(
    categoryId.value,
    ls.currentLocale.value
  )
}

const loadAdvertisements = async () => {
  isLoading.value++
  const attributeSearch = Object.keys(filter.value)
    .map((k) => {
      const filterValue = filter.value[k]
      return new AttributeSearchQuery({
        attributeId: parseInt(k),
        value: typeof filterValue[0] === 'number' ? '' + filterValue[0] : filterValue[0],
        secondaryValue: typeof filterValue[1] === 'number' ? '' + filterValue[1] : filterValue[1]
      })
    })
    .filter((q) => q.value != null || q.secondaryValue != null)

  const response = await advertisementService.getAdvertisements(
    new AdvertismentQuery({
      categoryId: categoryId.value ?? undefined,
      locale: ls.currentLocale.value,
      attributeOrder: [], //TODO: Implement order
      attributeSearch: attributeSearch,
      order: [],
      columns: []
    })
  )
  advertisements.value = response.data ?? []
  isLoading.value--
}
</script>
