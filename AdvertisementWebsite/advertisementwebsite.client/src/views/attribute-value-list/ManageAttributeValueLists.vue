<template>
  <LazyLoadedTable
    v-model:loading="loading"
    v-model:selection="selectedRows"
    :columns="columns"
    :dataSource="dataSource"
    ref="table"
  >
    <template #header>
      <h3 class="page-title mb-2">{{ l.navigation.manageAttributeValueLists }}</h3>
      <div class="flex flex-wrap justify-end gap-2">
        <Button
          v-if="isAllowedToDelete"
          :label="l.actions.delete"
          :disabled="!selectedRows.length"
          severity="danger"
          @click="confirmRowDelete"
        />
        <Button
          v-if="isAllowedToCreate"
          :label="l.actions.create"
          severity="primary"
          as="RouterLink"
          :to="{ name: 'createAttributeValueList' }"
        />
      </div>
    </template>

    <Column selectionMode="multiple" headerStyle="width: 3rem" />

    <Column field="title" :header="l.manageAttributeValueLists.title" sortable />
    <Column field="entryCount" :header="l.manageAttributeValueLists.entryCount" sortable />

    <Column>
      <template #body="slotProps">
        <div class="space-x-2 space-y-2">
          <Button
            v-if="isAllowedToEdit"
            :label="l.actions.edit"
            as="RouterLink"
            :to="{
              name: 'editAttributeValueList',
              params: { valueListId: '' + slotProps.data.id }
            }"
          />
          <Button
            :label="l.actions.view"
            severity="secondary"
            as="RouterLink"
            :to="{
              name: 'viewAttributeValueList',
              params: { valueListId: '' + slotProps.data.id }
            }"
          />
        </div>
      </template>
    </Column>
  </LazyLoadedTable>
</template>

<script lang="ts" setup>
import LazyLoadedTable from '@/components/common/LazyLoadedTable.vue'
import { Permissions } from '@/constants/api/Permissions'
import {
  AttributeClient,
  DataTableQuery,
  NotificationSubscriptionItem,
  TableColumn
} from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { confirmDelete } from '@/utils/confirm-dialog'
import { useConfirm } from 'primevue'
import { computed, ref, useTemplateRef, watch } from 'vue'

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const confirm = useConfirm()
const attributeService = getClient(AttributeClient)

//Refs
const table = useTemplateRef('table')

//Constants
const columns: TableColumn[] = [
  new TableColumn({
    data: 'title',
    name: 'manageAttributeValueLists.title',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'entryCount',
    name: 'manageAttributeValueLists.entryCount',
    orderable: true,
    searchable: true
  })
]

//Reactive data
const selectedRows = ref<NotificationSubscriptionItem[]>([])
const loading = ref(false)
const isAllowedToCreate = computed(() =>
  AuthService.hasPermission(Permissions.CreateAttributeValueList)
)
const isAllowedToEdit = computed(() =>
  AuthService.hasPermission(Permissions.EditAttributeValueList)
)
const isAllowedToDelete = computed(() =>
  AuthService.hasPermission(Permissions.DeleteAttributeValueList)
)

//watch
watch(LocaleService.currentLocaleName, () => {
  table.value?.refresh()
})

//Methods
const dataSource = async (query: DataTableQuery) => {
  return await attributeService.getAttributeValueLists(query)
}

const confirmRowDelete = async () => {
  confirmDelete(confirm, {
    header: l.value.manageAttributeValueLists.confirmDeleteHeader,
    message: ls.l('manageAttributeValueLists.confirmDeleteMessage', selectedRows.value.length),
    accept: () => {
      deleteRows()
    }
  })
}

const deleteRows = async () => {
  loading.value = true
  await attributeService.deleteAttributeValueLists(selectedRows.value.map((i) => i.id!))
  table.value?.refresh()
}
</script>
