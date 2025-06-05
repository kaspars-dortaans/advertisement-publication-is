<template>
  <LazyLoadedTable
    v-model:loading="loading"
    :columns="columns"
    :dataSource="dataSource"
    selectionMode="single"
    ref="table"
    @rowSelect="handleRowSelect"
  >
    <template v-if="showResolved">
      <Column field="isTrue" :header="l.manageRuleViolationReports.isTrue" sortable>
        <template #body="slotProps">
          {{ typeof slotProps.data.isTrue === 'boolean' ? l[slotProps.data.isTrue] : '' }}
        </template>
      </Column>
      <Column
        field="resolutionDescription"
        :header="l.manageRuleViolationReports.resolutionDescription"
        sortable
      />
    </template>

    <Column field="description" :header="l.manageRuleViolationReports.description" sortable />
    <Column field="reporterUsername" :header="l.manageRuleViolationReports.reporter" sortable>
      <template #body="slotProps">
        <RouterLink
          v-if="slotProps.data.reporterId"
          :to="{
            name: isAllowedToViewAllUser ? 'viewUser' : 'viewUserProfile',
            params: { userId: slotProps.data.reporterId }
          }"
        >
          {{ slotProps.data.reporterUsername }}
        </RouterLink>
      </template>
    </Column>
    <Column
      field="advertisementTitle"
      :header="l.manageRuleViolationReports.reportedAdvertisement"
      sortable
    >
      <template #body="slotProps">
        <RouterLink
          :to="{
            name: 'viewAdvertisement',
            params: { id: slotProps.data.advertisementId }
          }"
        >
          {{ slotProps.data.advertisementTitle }}
        </RouterLink>
      </template>
    </Column>
    <Column
      field="advertisementOwnerUsername"
      :header="l.manageRuleViolationReports.advertisementOwner"
      sortable
    >
      <template #body="slotProps">
        <RouterLink
          :to="{
            name: isAllowedToViewAllUser ? 'viewUser' : 'viewUserProfile',
            params: { userId: slotProps.data.advertisementOwnerId }
          }"
        >
          {{ slotProps.data.advertisementOwnerUsername }}
        </RouterLink>
      </template>
    </Column>

    <Column field="reportDate" :header="l.manageRuleViolationReports.reportDate" sortable>
      <template #body="slotProps">
        {{ dateFormat.format(slotProps.data.reportDate) }}
      </template>
    </Column>
  </LazyLoadedTable>
</template>

<script lang="ts" setup>
import LazyLoadedTable from '@/components/common/LazyLoadedTable.vue'
import { Permissions } from '@/constants/api/Permissions'
import {
  DataTableQuery,
  RuleViolationReportClient,
  SearchQuery,
  TableColumn
} from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import type { DataTableRowSelectEvent } from 'primevue'
import { computed, ref, useTemplateRef, watch } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  showResolved: boolean
}>()

const { push } = useRouter()

//Services
const l = LocaleService.currentLocale
const ruleViolationReportService = getClient(RuleViolationReportClient)

//Refs
const table = useTemplateRef('table')

//Constants
const columns: TableColumn[] = [
  new TableColumn({
    data: 'isResolved',
    name: 'manageRuleViolationReports.isResolved',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'isTrue',
    name: 'manageRuleViolationReports.isTrue',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'resolutionDescription',
    name: 'manageRuleViolationReports.resolutionDescription',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'description',
    name: 'manageRuleViolationReports.description',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'reporterUsername',
    name: 'manageRuleViolationReports.reporterUsername',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'advertisementTitle',
    name: 'manageRuleViolationReports.advertisementTitle',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'advertisementOwnerUsername',
    name: 'manageRuleViolationReports.advertisementOwnerUsername',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'reportDate',
    name: 'manageRuleViolationReports.reportDate',
    orderable: true,
    searchable: true
  })
]

//Reactive data
const loading = ref(false)
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)

const isAllowedToViewAllUser = computed(() => AuthService.hasPermission(Permissions.ViewAllUsers))

//watch
watch(LocaleService.currentLocaleName, () => {
  table.value?.refresh()
})

//Methods
const dataSource = async (query: DataTableQuery) => {
  query.columns![0].search = new SearchQuery({
    value: props.showResolved ? 'true' : 'false'
  })
  return await ruleViolationReportService.getReportList(query)
}

const handleRowSelect = (e: DataTableRowSelectEvent) => {
  push({ name: 'viewRuleViolationReport', params: { id: e.data.id } })
}
</script>
