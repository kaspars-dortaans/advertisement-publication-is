<template>
  <LazyLoadedTable
    v-model:loading="loading"
    v-model:selection="selectedAdvertisements"
    :columns="columns"
    :dataSource="advertisementSource"
    ref="table"
  >
    <template #header>
      <h3 class="page-title mb-2">{{ l.navigation.myAdvertisements }}</h3>
      <div class="flex flex-wrap justify-end gap-2">
        <Button
          :label="l.actions.deactivate"
          :disabled="!selectedAdvertisements.length || allSelectedAdvertisementsAreInactive"
          severity="secondary"
          @click="setAdvertisementActiveState(false)"
        />
        <Button
          :label="l.actions.activate"
          :disabled="!selectedAdvertisements.length || allSelectedAdvertisementsAreActive"
          severity="primary"
          @click="setAdvertisementActiveState(true)"
        />
        <Button
          :label="l.actions.delete"
          :disabled="!selectedAdvertisements.length"
          severity="danger"
          @click="confirmDeleteAdvertisements"
        />
        <Button
          :label="l.actions.extend"
          :disabled="!selectedAdvertisements.length"
          severity="secondary"
          @click="extendAdvertisements"
        />
        <Button
          :label="l.actions.create"
          severity="primary"
          as="RouterLink"
          :to="{ name: 'createAdvertisement' }"
        />
      </div>
    </template>

    <Column selectionMode="multiple" headerStyle="width: 3rem" />

    <Column field="title" :header="l.manageAdvertisements.title" sortable />
    <Column field="categoryName" :header="l.manageAdvertisements.categoryName" sortable />
    <Column field="isActive" :header="l.manageAdvertisements.isActive" sortable>
      <template #body="slotProps">
        <Badge
          :severity="slotProps.data.isActive ? 'primary' : 'secondary'"
          :value="ls.l(slotProps.data.isActive + '')"
        />
      </template>
    </Column>
    <Column field="validTo" :header="l.manageAdvertisements.validTo" sortable>
      <template #body="slotProps">{{ dateFormat.format(slotProps.data.validTo) }}</template>
    </Column>
    <Column field="createdAt" :header="l.manageAdvertisements.createdAt" sortable>
      <template #body="slotProps">{{ dateFormat.format(slotProps.data.createdAt) }}</template>
    </Column>

    <Column>
      <template #body="slotProps">
        <div class="space-x-2 space-y-2">
          <Button
            :label="l.actions.edit"
            as="RouterLink"
            :to="{ name: 'editAdvertisement', params: { id: '' + slotProps.data.id } }"
          />
          <Button
            :label="l.actions.view"
            severity="secondary"
            as="RouterLink"
            :to="{ name: 'viewAdvertisement', params: { id: '' + slotProps.data.id } }"
          />
        </div>
      </template>
    </Column>
  </LazyLoadedTable>
</template>

<script setup lang="ts">
import LazyLoadedTable from '@/components/common/LazyLoadedTable.vue'
import {
  AdvertisementClient,
  AdvertisementInfo,
  DataTableQuery,
  SetActiveStatusRequest,
  TableColumn
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { confirmDelete } from '@/utils/confirm-dialog'
import { useConfirm } from 'primevue'
import { computed, ref, useTemplateRef } from 'vue'
import { useRouter } from 'vue-router'

const table = useTemplateRef('table')
const { push } = useRouter()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const advertisementService = getClient(AdvertisementClient)
const confirm = useConfirm()

//Reactive data
const selectedAdvertisements = ref<AdvertisementInfo[]>([])
const selectedAdvertisementIds = computed(
  () =>
    selectedAdvertisements.value.map((a) => a.id).filter((id) => typeof id === 'number') as number[]
)
const allSelectedAdvertisementsAreActive = computed(() =>
  selectedAdvertisements.value.every((a) => a.isActive)
)
const allSelectedAdvertisementsAreInactive = computed(() =>
  selectedAdvertisements.value.every((a) => !a.isActive)
)
const loading = ref(false)
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)

const columns: TableColumn[] = [
  new TableColumn({
    data: 'title',
    name: 'manageAdvertisements.title',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'categoryName',
    name: 'manageAdvertisements.categoryName',
    orderable: true
  }),
  new TableColumn({
    data: 'isActive',
    name: 'manageAdvertisements.isActive',
    orderable: true
  }),
  new TableColumn({
    data: 'validTo',
    name: 'manageAdvertisements.validTo',
    orderable: true
  }),
  new TableColumn({
    data: 'createdAt',
    name: 'manageAdvertisements.createdAt',
    orderable: true
  })
]

//Methods
const advertisementSource = (query: DataTableQuery) => {
  return advertisementService.getOwnedAdvertisements(query)
}

const setAdvertisementActiveState = async (isActive: boolean) => {
  loading.value = true
  const ids = selectedAdvertisements.value
    .map((a) => a.id)
    .filter((id) => typeof id === 'number') as number[]
  await advertisementService.setIsActiveAdvertisements(
    new SetActiveStatusRequest({ isActive, ids })
  )
  table.value?.refresh()
}

const confirmDeleteAdvertisements = () => {
  confirmDelete(confirm, {
    header: l.value.manageAdvertisements.confirmDeleteHeader,
    message: ls.l('manageAdvertisements.confirmDeleteMessage', selectedAdvertisements.value.length),
    accept: () => deleteAdvertisements()
  })
}

const deleteAdvertisements = async () => {
  loading.value = true
  await advertisementService.deleteAdvertisements(selectedAdvertisementIds.value)
  table.value?.refresh()
}

const extendAdvertisements = () => {
  push({
    name: 'extend',
    params: {
      ids: JSON.stringify(selectedAdvertisementIds.value),
      type: 'advertisement'
    }
  })
}
</script>
