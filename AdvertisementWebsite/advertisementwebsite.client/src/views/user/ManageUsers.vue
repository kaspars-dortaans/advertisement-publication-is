<template>
  <LazyLoadedTable
    v-model:loading="loading"
    v-model:selection="selectedRows"
    :columns="columns"
    :dataSource="dataSource"
    ref="table"
  >
    <template #header>
      <h3 class="page-title mb-2">{{ l.navigation.manageUsers }}</h3>
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
          :to="{ name: 'createUser' }"
        />
      </div>
    </template>

    <Column selectionMode="multiple" headerStyle="width: 3rem" />

    <Column field="firstName" :header="l.manageUsers.firstName" sortable />
    <Column field="lastName" :header="l.manageUsers.lastName" sortable />
    <Column field="userName" :header="l.manageUsers.userName" sortable />
    <Column field="userRoles" :header="l.manageUsers.userRoles" sortable>
      <template #body="slotProps">
        {{ slotProps.data.userRoles.join(', ') }}
      </template>
    </Column>
    <Column field="email" :header="l.manageUsers.email" sortable />
    <Column field="phoneNumber" :header="l.manageUsers.phoneNumber" sortable />
    <Column field="createdDate" :header="l.manageUsers.createdDate" sortable>
      <template #body="slotProps">
        {{ dateFormat.format(slotProps.data.createdDate) }}
      </template>
    </Column>
    <Column field="lastActive" :header="l.manageUsers.lastActive" sortable>
      <template #body="slotProps">
        {{ slotProps.data.lastActive ? dateFormat.format(slotProps.data.lastActive) : '' }}
      </template>
    </Column>

    <Column>
      <template #body="slotProps">
        <div class="flex flex-wrap justify-end gap-2">
          <Button
            v-if="isAllowedToEdit"
            :label="l.actions.edit"
            as="RouterLink"
            :to="{
              name: 'editUser',
              params: { userId: '' + slotProps.data.id }
            }"
          />
          <Button
            :label="l.actions.view"
            severity="secondary"
            as="RouterLink"
            :to="{
              name: 'viewUser',
              params: { userId: '' + slotProps.data.id }
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
  DataTableQuery,
  NotificationSubscriptionItem,
  TableColumn,
  UserClient
} from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { confirmDelete } from '@/utils/confirm-dialog'
import { useConfirm } from 'primevue'
import { computed, ref, useTemplateRef } from 'vue'

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const confirm = useConfirm()
const userService = getClient(UserClient)

//Refs
const table = useTemplateRef('table')

//Constants
const columns: TableColumn[] = [
  new TableColumn({
    data: 'firstName',
    name: 'manageUsers.firstName',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'lastName',
    name: 'manageUsers.lastName',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'userName',
    name: 'manageUsers.userName',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'userRoles',
    name: 'manageUsers.userRoles',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'email',
    name: 'manageUsers.email',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'phoneNumber',
    name: 'manageUsers.phoneNumber',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'createdDate',
    name: 'manageUsers.createdDate',
    orderable: true
  }),
  new TableColumn({
    data: 'lastActive',
    name: 'manageUsers.lastActiveDate',
    orderable: true
  })
]

//Reactive data
const selectedRows = ref<NotificationSubscriptionItem[]>([])
const loading = ref(false)
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)
const isAllowedToCreate = computed(() => AuthService.hasPermission(Permissions.CreateUser))
const isAllowedToEdit = computed(() => AuthService.hasPermission(Permissions.EditAnyUser))
const isAllowedToDelete = computed(() => AuthService.hasPermission(Permissions.DeleteAnyUser))

//Methods
const dataSource = async (query: DataTableQuery) => {
  return await userService.getUserList(query)
}

const confirmRowDelete = async () => {
  confirmDelete(confirm, {
    header: l.value.manageUsers.confirmDeleteHeader,
    message: ls.l('manageUsers.confirmDeleteMessage', selectedRows.value.length),
    accept: () => {
      deleteRows()
    }
  })
}

const deleteRows = async () => {
  loading.value = true
  await userService.deleteUsers(selectedRows.value.map((i) => i.id!))
  table.value?.refresh()
}
</script>
