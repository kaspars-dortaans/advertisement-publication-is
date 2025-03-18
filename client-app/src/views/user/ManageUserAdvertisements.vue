<template>
  <ResponsiveLayout>
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
            @click="deleteAdvertisements"
          />
          <Button
            :label="l.actions.extend"
            :disabled="!selectedAdvertisements.length"
            severity="secondary"
            @click="todo"
          />
          <Button :label="l.actions.create" severity="primary" @click="todo"></Button>
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
            <Button :label="l.actions.edit" @click="editAdvertisement(slotProps.data)" />
            <Button
              :label="l.actions.view"
              severity="secondary"
              @click="viewAdvertisement(slotProps.data)"
            />
          </div>
        </template>
      </Column>
    </LazyLoadedTable>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import LazyLoadedTable from '@/components/Common/LazyLoadedTable.vue'
import ResponsiveLayout from '@/components/Common/ResponsiveLayout.vue'
import {
  AdvertisementClient,
  AdvertisementInfo,
  DataTableQuery,
  TableColumn
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { computed, ref, useTemplateRef } from 'vue'
import { useRouter } from 'vue-router'

const { push } = useRouter()
const table = useTemplateRef('table')

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const advertisementService = getClient(AdvertisementClient)

//Reactive data
const selectedAdvertisements = ref<AdvertisementInfo[]>([])
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
  await advertisementService.setIsActiveAdvertisements(isActive, ids)
  table.value?.refresh()
}

const deleteAdvertisements = async () => {
  loading.value = true
  const ids = selectedAdvertisements.value
    .map((a) => a.id)
    .filter((id) => typeof id === 'number') as number[]
  await advertisementService.deleteAdvertisements(ids)
  table.value?.refresh()
}

const viewAdvertisement = (advertisement: AdvertisementInfo) => {
  push({ name: 'viewAdvertisement', params: { id: '' + advertisement.id } })
}

const editAdvertisement = (advertisement: AdvertisementInfo) => {
  push({ name: 'editAdvertisement', params: { id: '' + advertisement.id } })
}

const todo = () => {
  alert('Not implemented yet')
}
</script>
