<template>
  <LazyLoadedTable
    v-model:loading="loading"
    v-model:selection="selectedRows"
    :columns="columns"
    :dataSource="dataSource"
    ref="table"
  >
    <template #header>
      <h3 class="page-title mb-2">{{ l.navigation.manageAttributes }}</h3>
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
          :to="{ name: 'createAttribute' }"
        />
      </div>
    </template>

    <Column selectionMode="multiple" headerStyle="width: 3rem" />

    <Column field="title" :header="l.manageAttributes.title" sortable />
    <Column field="valueType" :header="l.manageAttributes.valueType" sortable>
      <template #body="slotProps">
        {{ l.valueType[slotProps.data.valueType] }}
      </template>
    </Column>
    <Column field="filterType" :header="l.manageAttributes.filterType" sortable>
      <template #body="slotProps">
        {{ l.filterType[slotProps.data.filterType] }}
      </template>
    </Column>
    <Column
      field="attributeValueListName"
      :header="l.manageAttributes.attributeValueListName"
      sortable
    />
    <Column field="sortable" :header="l.manageAttributes.sortable" sortable>
      <template #body="slotProps">
        {{ l[slotProps.data.sortable] }}
      </template>
    </Column>
    <Column field="searchable" :header="l.manageAttributes.searchable" sortable>
      <template #body="slotProps">
        {{ l[slotProps.data.searchable] }}
      </template>
    </Column>
    <Column field="showOnListItem" :header="l.manageAttributes.showOnListItem" sortable>
      <template #body="slotProps">
        {{ l[slotProps.data.showOnListItem] }}
      </template>
    </Column>
    <Column field="iconName" :header="l.manageAttributes.iconName" sortable />

    <Column>
      <template #body="slotProps">
        <div class="flex flex-wrap justify-end gap-2">
          <Button
            v-if="isAllowedToEdit"
            :label="l.actions.edit"
            as="RouterLink"
            :to="{
              name: 'editAttribute',
              params: { attributeId: '' + slotProps.data.id }
            }"
          />
          <Button
            :label="l.actions.view"
            severity="secondary"
            as="RouterLink"
            :to="{
              name: 'viewAttribute',
              params: { attributeId: '' + slotProps.data.id }
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
    name: 'manageAttributes.title',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'valueType',
    name: 'manageAttributes.valueType',
    orderable: true
  }),
  new TableColumn({
    data: 'filterType',
    name: 'manageAttributes.filterType',
    orderable: true
  }),
  new TableColumn({
    data: 'attributeValueListName',
    name: 'manageAttributes.attributeValueListName',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'sortable',
    name: 'manageAttributes.sortable',
    orderable: true
  }),
  new TableColumn({
    data: 'searchable',
    name: 'manageAttributes.searchable',
    orderable: true
  }),
  new TableColumn({
    data: 'showOnListItem',
    name: 'manageAttributes.showOnListItem',
    orderable: true
  }),
  new TableColumn({
    data: 'iconName',
    name: 'manageAttributes.iconName',
    orderable: true,
    searchable: true
  })
]

//Reactive data
const selectedRows = ref<NotificationSubscriptionItem[]>([])
const loading = ref(false)
const isAllowedToCreate = computed(() => AuthService.hasPermission(Permissions.CreateAttribute))
const isAllowedToEdit = computed(() => AuthService.hasPermission(Permissions.EditAttribute))
const isAllowedToDelete = computed(() => AuthService.hasPermission(Permissions.DeleteAttribute))

//watch
watch(LocaleService.currentLocaleName, () => {
  table.value?.refresh()
})

//Methods
const dataSource = async (query: DataTableQuery) => {
  return await attributeService.getAttributes(query)
}

const confirmRowDelete = async () => {
  confirmDelete(confirm, {
    header: l.value.manageAttributes.confirmDeleteHeader,
    message: ls.l('manageAttributes.confirmDeleteMessage', selectedRows.value.length),
    accept: () => {
      deleteRows()
    }
  })
}

const deleteRows = async () => {
  loading.value = true
  await attributeService.deleteAttribute(selectedRows.value.map((i) => i.id!))
  table.value?.refresh()
}
</script>
