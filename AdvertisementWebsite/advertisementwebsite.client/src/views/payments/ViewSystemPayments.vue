<template>
  <LazyLoadedTable
    :columns="columns"
    :dataSource="dataSource"
    selectionMode="single"
    ref="table"
    @rowSelect="handleRowSelect"
  >
    <template #header>
      <h3 class="page-title mb-2">{{ l.navigation.viewSystemPayments }}</h3>
      <FloatLabel variant="on">
        <DatePicker
          v-model="timePeriodFilterValue"
          :manualInput="false"
          :clearButtonProps="{
            label: l.actions.clear,
            icon: 'pi pi-times',
            severity: 'secondary'
          }"
          selectionMode="range"
          view="month"
          id="payment-time-period-filter"
          showButtonBar
          @hide="refresh"
          @clearClick="timePeriodFilterValue = []"
        />
        <label for="payment-time-period-filter">{{ l.viewPayments.timePeriod }}</label>
      </FloatLabel>
      <p>{{ ls.l('viewPayments.totalForTimePeriod', totalForTimePeriod) }}</p>
    </template>

    <Column field="id" :header="l.viewPayments.id" sortable />
    <Column field="date" :header="l.viewPayments.date" sortable>
      <template #body="slotProps">{{ dateFormat.format(slotProps.data.date) }}</template>
    </Column>
    <Column field="payerUsername" :header="l.viewPayments.payerUsername" sortable />
    <Column field="amount" :header="l.viewPayments.amount" sortable>
      <template #body="slotProps">{{ currencyFormat.format(slotProps.data.amount) }}</template>
    </Column>
    <Column field="paymentItemCount" :header="l.viewPayments.paymentItemCount" sortable />
  </LazyLoadedTable>
</template>

<script lang="ts" setup>
import LazyLoadedTable from '@/components/common/LazyLoadedTable.vue'
import { PaymentClient, PaymentDataTableQuery, TableColumn } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import type { DataTableRowSelectEvent } from 'primevue'
import { computed, ref, useTemplateRef, watch } from 'vue'
import { useRouter } from 'vue-router'

const { push } = useRouter()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const paymentService = getClient(PaymentClient)

//Refs
const table = useTemplateRef('table')

//Constants
const columns: TableColumn[] = [
  new TableColumn({
    data: 'id',
    name: 'viewPayments.id',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'date',
    name: 'viewPayments.date',
    orderable: true
  }),
  new TableColumn({
    data: 'payerUsername',
    name: 'viewPayments.payerUsername',
    orderable: true
  }),
  new TableColumn({
    data: 'amount',
    name: 'viewPayments.amount',
    orderable: true,
    searchable: true,
    aggregate: true
  }),
  new TableColumn({
    data: 'paymentItemCount',
    name: 'viewPayments.paymentItemCount',
    orderable: true,
    searchable: true
  })
]

//Reactive data
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)
const currencyFormat = computed(() =>
  Intl.NumberFormat(LocaleService.currentLocaleName.value, {
    style: 'currency',
    currency: 'EUR'
  })
)
const totalForTimePeriod = computed(() => {
  const totalAmount = aggregates.value['viewPayments.amount']
  if (typeof totalAmount === 'number') {
    return currencyFormat.value.format(totalAmount)
  }
  return currencyFormat.value.format(0)
})
const aggregates = ref<{ [k: string]: unknown }>({})
const timePeriodFilterValue = ref<Date[]>([])
const filterChanged = ref(false)

//Watchers
watch(timePeriodFilterValue, () => {
  filterChanged.value = true
})

//Methods
const dataSource = async (query: PaymentDataTableQuery) => {
  query.fromDate = timePeriodFilterValue.value[0]
  query.toDate = timePeriodFilterValue.value[1]
  const response = await paymentService.getAllPayments(query)
  aggregates.value = response.aggregates as { [k: string]: unknown }
  return response
}

const refresh = () => {
  if (filterChanged.value) {
    table.value?.refresh()
  }
  filterChanged.value = false
}

const handleRowSelect = (e: DataTableRowSelectEvent) => {
  push({
    name: 'viewSystemPayment',
    params: {
      paymentId: e.data.id
    }
  })
}
</script>
