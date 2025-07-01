<template>
  <LazyLoadedTable
    v-model:loading="loading"
    v-model:selection="selectedRows"
    :columns="columns"
    :dataSource="dataSource"
    :editMode="'row'"
    :rowEditSaveHandler="rowEditSaveHandler"
    ref="table"
  >
    <template #header="slotProps">
      <h3 class="page-title mb-2">{{ l.navigation.managePermissions }}</h3>
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
          @click="slotProps.addNewRow"
        />
      </div>
    </template>

    <Column selectionMode="multiple" headerStyle="width: 3rem" />

    <Column field="name" :header="l.managePermissions.name" sortable>
      <template #editor="{ data, field }">
        <InputText v-model="data[field]" fluid />
      </template>
    </Column>

    <Column v-if="isAllowedToEdit" :rowEditor="true" style="width: 1rem; padding-right: 0"></Column>
  </LazyLoadedTable>
</template>

<script lang="ts" setup>
import LazyLoadedTable from '@/components/common/LazyLoadedTable.vue'
import { Permissions } from '@/constants/api/Permissions'
import {
  DataTableQuery,
  NotificationSubscriptionItem,
  PermissionClient,
  PutPermissionRequest,
  TableColumn
} from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { confirmDelete } from '@/utils/confirm-dialog'
import { useConfirm, type DataTableRowEditSaveEvent } from 'primevue'
import { computed, ref, useTemplateRef } from 'vue'

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const confirm = useConfirm()
const permissionService = getClient(PermissionClient)

//Refs
const table = useTemplateRef('table')

//Constants
const columns: TableColumn[] = [
  new TableColumn({
    data: 'name',
    name: 'managePermissions.name',
    orderable: true,
    searchable: true
  })
]

//Reactive data
const selectedRows = ref<NotificationSubscriptionItem[]>([])
const loading = ref(false)
const isAllowedToCreate = computed(() => AuthService.hasPermission(Permissions.AddPermission))
const isAllowedToEdit = computed(() => AuthService.hasPermission(Permissions.EditPermission))
const isAllowedToDelete = computed(() => AuthService.hasPermission(Permissions.DeletePermission))

//Methods
const dataSource = async (query: DataTableQuery) => {
  return await permissionService.getPermissionList(query)
}

const rowEditSaveHandler = async (e: DataTableRowEditSaveEvent) => {
  loading.value = true
  const request = new PutPermissionRequest({
    id: e.newData.id,
    name: e.newData.name
  })

  if (e.data.id != null) {
    permissionService.editPermission(request)
  } else {
    permissionService.createPermission(request)
  }
  await table.value?.refresh()
}

const confirmRowDelete = async () => {
  confirmDelete(confirm, {
    header: l.value.managePermissions.confirmDeleteHeader,
    message: ls.l('managePermissions.confirmDeleteMessage', selectedRows.value.length),
    accept: () => {
      deleteRows()
    }
  })
}

const deleteRows = async () => {
  loading.value = true
  await permissionService.deletePermissions(selectedRows.value.map((i) => i.id!))
  table.value?.refresh()
}
</script>
