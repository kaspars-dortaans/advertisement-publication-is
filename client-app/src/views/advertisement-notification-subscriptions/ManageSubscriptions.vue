<template>
  <LazyLoadedTable
    v-model:loading="loading"
    v-model:selection="selectedRows"
    :columns="columns"
    :dataSource="advertisementNotificationSource"
    ref="table"
  >
    <template #header>
      <h3 class="page-title mb-2">{{ l.navigation.advertisementNotifications }}</h3>
      <div class="flex flex-wrap justify-end gap-2">
        <Button
          :label="l.actions.deactivate"
          :disabled="!selectedRows.length || allSelectedAreInactive"
          severity="secondary"
          @click="setActiveState(false)"
        />
        <Button
          :label="l.actions.activate"
          :disabled="!selectedRows.length || allSelectedAreActive"
          severity="primary"
          @click="setActiveState(true)"
        />
        <Button
          :label="l.actions.delete"
          :disabled="!selectedRows.length"
          severity="danger"
          @click="confirmNotificationSubscriptionDelete"
        />
        <Button
          :label="l.actions.extend"
          :disabled="!selectedRows.length"
          severity="secondary"
          @click="extend"
        />
        <Button
          :label="l.actions.create"
          severity="primary"
          as="RouterLink"
          :to="{ name: 'createAdvertisementNotificationSubscription' }"
        />
      </div>
    </template>

    <Column selectionMode="multiple" headerStyle="width: 3rem" />

    <Column field="title" :header="l.manageAdvertisementNotificationSubscriptions.title" sortable />
    <Column
      field="keywords"
      :header="l.manageAdvertisementNotificationSubscriptions.keywords"
      sortable
    >
      <template #body="slotProps">
        {{ slotProps.data.keywords?.replaceAll(',', ' ') ?? '' }}
      </template>
    </Column>
    <Column
      field="categoryName"
      :header="l.manageAdvertisementNotificationSubscriptions.categoryName"
      sortable
    />
    <Column
      field="isActive"
      :header="l.manageAdvertisementNotificationSubscriptions.isActive"
      sortable
    >
      <template #body="slotProps">
        <Badge
          :severity="slotProps.data.isActive ? 'primary' : 'secondary'"
          :value="ls.l(slotProps.data.isActive + '')"
        />
      </template>
    </Column>
    <Column
      field="validTo"
      :header="l.manageAdvertisementNotificationSubscriptions.validTo"
      sortable
    >
      <template #body="slotProps">{{ dateFormat.format(slotProps.data.validTo) }}</template>
    </Column>
    <Column
      field="createdAt"
      :header="l.manageAdvertisementNotificationSubscriptions.createdAt"
      sortable
    >
      <template #body="slotProps">{{ dateFormat.format(slotProps.data.createdAt) }}</template>
    </Column>

    <Column>
      <template #body="slotProps">
        <div class="space-x-2 space-y-2">
          <Button
            :label="l.actions.edit"
            as="RouterLink"
            :to="{
              name: 'editAdvertisementNotificationSubscription',
              params: { subscriptionId: '' + slotProps.data.id }
            }"
          />
        </div>
      </template>
    </Column>
  </LazyLoadedTable>
</template>

<script lang="ts" setup>
import LazyLoadedTable from '@/components/common/LazyLoadedTable.vue'
import {
  AdvertisementNotificationClient,
  DataTableQuery,
  NotificationSubscriptionItem,
  SetActiveStatusRequest,
  TableColumn
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { confirmDelete } from '@/utils/confirm-dialog'
import { useConfirm } from 'primevue'
import { computed, ref, useTemplateRef } from 'vue'
import { useRouter } from 'vue-router'

const { push } = useRouter()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const subscriptionService = getClient(AdvertisementNotificationClient)
const confirm = useConfirm()

//Refs
const table = useTemplateRef('table')

//Constants
const columns: TableColumn[] = [
  new TableColumn({
    data: 'title',
    name: 'manageAdvertisementNotificationSubscriptions.title',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'keywords',
    name: 'manageAdvertisementNotificationSubscriptions.keywords',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'categoryName',
    name: 'manageAdvertisementNotificationSubscriptions.categoryName',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'isActive',
    name: 'manageAdvertisementNotificationSubscriptions.isActive',
    orderable: true
  }),
  new TableColumn({
    data: 'validTo',
    name: 'manageAdvertisementNotificationSubscriptions.validTo',
    orderable: true
  }),
  new TableColumn({
    data: 'createdAt',
    name: 'manageAdvertisementNotificationSubscriptions.createdAt',
    orderable: true
  })
]

//Reactive data
const selectedRows = ref<NotificationSubscriptionItem[]>([])
const allSelectedAreActive = computed(() => selectedRows.value.every((r) => r.isActive))
const allSelectedAreInactive = computed(() => selectedRows.value.every((r) => !r.isActive))
const loading = ref(false)
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)

//Methods
const advertisementNotificationSource = async (query: DataTableQuery) => {
  return await subscriptionService.getAdvertisementNotificationSubscriptions(query)
}

const setActiveState = async (isActive: boolean) => {
  loading.value = true
  await subscriptionService.setSubscriptionActiveStatus(
    new SetActiveStatusRequest({
      ids: selectedRows.value.map((i) => i.id!),
      isActive
    })
  )
  table.value?.refresh()
  loading.value = false
}

const confirmNotificationSubscriptionDelete = async () => {
  confirmDelete(confirm, {
    header: l.value.manageAdvertisementNotificationSubscriptions.confirmDeleteHeader,
    message: ls.l(
      'manageAdvertisementNotificationSubscriptions.confirmDeleteMessage',
      selectedRows.value.length
    ),
    accept: async () => {
      loading.value = true
      await subscriptionService.deleteSubscriptions(selectedRows.value.map((i) => i.id!))
      table.value?.refresh()
      loading.value = false
    }
  })
}

const extend = () => {
  push({
    name: 'extend',
    params: {
      ids: JSON.stringify(selectedRows.value.map((r) => r.id!)),
      type: 'notificationSubscription'
    }
  })
}
</script>
