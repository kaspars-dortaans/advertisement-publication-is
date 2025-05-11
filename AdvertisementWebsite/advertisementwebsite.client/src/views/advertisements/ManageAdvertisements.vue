<template>
  <LazyLoadedTable
    v-model:loading="loading"
    v-model:selection="selectedAdvertisements"
    :columns="columns"
    :dataSource="advertisementSource"
    ref="table"
  >
    <template #header>
      <h3 class="page-title mb-2">
        {{ manageAll ? l.navigation.manageAdvertisements : l.navigation.myAdvertisements }}
      </h3>
      <div class="flex flex-wrap justify-end gap-2">
        <Button
          :label="l.actions.deactivate"
          :disabled="!selectedAdvertisements.length || !atLeastOneSelectedIsActive"
          severity="secondary"
          @click="setAdvertisementActiveState(false)"
        />
        <Button
          :label="l.actions.activate"
          :disabled="!selectedAdvertisements.length || !atLeastOneSelectedIsInactive"
          severity="primary"
          @click="setAdvertisementActiveState(true)"
        />
        <Button
          v-if="isAllowedToDelete"
          :label="l.actions.delete"
          :disabled="!selectedAdvertisements.length"
          severity="danger"
          @click="confirmDeleteAdvertisements"
        />
        <Button
          :label="l.actions.extend"
          :disabled="!selectedAdvertisements.length || allSelectedAreDrafts"
          severity="secondary"
          @click="extendAdvertisements"
        />
        <Button
          v-if="isAllowedToCreate"
          :label="l.actions.create"
          severity="primary"
          as="RouterLink"
          :to="{ name: props.manageAll ? 'createAdvertisement' : 'createOwnAdvertisement' }"
        />
      </div>
    </template>

    <Column selectionMode="multiple" headerStyle="width: 3rem" />

    <Column field="title" :header="l.manageAdvertisements.title" sortable />
    <Column
      v-if="manageAll"
      field="ownerUsername"
      :header="l.manageAdvertisements.ownerUsername"
      sortable
    />
    <Column field="categoryName" :header="l.manageAdvertisements.categoryName" sortable />
    <Column field="status" :header="l.manageAdvertisements.status" sortable>
      <template #body="slotProps">
        <Badge
          :severity="statusSeverity[slotProps.data.status]"
          :value="
            ls.l(
              'paymentSubjectStatus.' +
                PaymentSubjectStatus[slotProps.data.status as PaymentSubjectStatus]
            )
          "
        />
      </template>
    </Column>
    <Column field="validToDate" :header="l.manageAdvertisements.validTo" sortable>
      <template #body="slotProps">{{
        slotProps.data.validToDate != null ? dateFormat.format(slotProps.data.validToDate) : ''
      }}</template>
    </Column>
    <Column field="createdAtDate" :header="l.manageAdvertisements.createdAt" sortable>
      <template #body="slotProps">{{
        slotProps.data.createdAtDate != null ? dateFormat.format(slotProps.data.createdAtDate) : ''
      }}</template>
    </Column>

    <Column>
      <template #body="slotProps">
        <div class="flex flex-wrap justify-end gap-2">
          <Button
            v-if="slotProps.data.status == PaymentSubjectStatus.Draft"
            :label="l.actions.publish"
            @click="publishAdvertisement(slotProps.data)"
          />
          <Button
            v-if="isAllowedToEdit"
            :label="l.actions.edit"
            as="RouterLink"
            :to="{
              name: props.manageAll ? 'editAnyAdvertisement' : 'editAdvertisement',
              params: { id: '' + slotProps.data.id }
            }"
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
import { Permissions } from '@/constants/api/Permissions'
import { statusSeverity } from '@/constants/status-severity'
import {
  AdvertisementClient,
  AdvertisementInfo,
  DataTableQuery,
  PaymentSubjectStatus,
  PaymentType,
  SetActiveStatusRequest,
  TableColumn
} from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { confirmDelete } from '@/utils/confirm-dialog'
import { useConfirm } from 'primevue'
import { computed, ref, useTemplateRef, watch } from 'vue'
import { useRouter } from 'vue-router'

const props = defineProps<{
  manageAll: boolean
}>()
const table = useTemplateRef('table')
const { push } = useRouter()

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const advertisementService = getClient(AdvertisementClient)
const confirm = useConfirm()

//Reactive data
const selectedAdvertisements = ref<AdvertisementInfo[]>([])
const atLeastOneSelectedIsActive = computed(() =>
  selectedAdvertisements.value.some((a) => a.status == PaymentSubjectStatus.Active)
)
const atLeastOneSelectedIsInactive = computed(() =>
  selectedAdvertisements.value.some((a) => a.status == PaymentSubjectStatus.Inactive)
)
const allSelectedAreDrafts = computed(() =>
  selectedAdvertisements.value.every((a) => a.status == PaymentSubjectStatus.Draft)
)
const loading = ref(false)
const dateFormat = computed(() =>
  Intl.DateTimeFormat(LocaleService.currentLocaleName.value, {
    dateStyle: 'short',
    timeStyle: 'short'
  })
)

const isAllowedToCreate = computed(() =>
  AuthService.hasPermission(
    props.manageAll ? Permissions.CreateAdvertisement : Permissions.CreateOwnedAdvertisement
  )
)
const isAllowedToEdit = computed(() =>
  AuthService.hasPermission(
    props.manageAll ? Permissions.EditAnyAdvertisement : Permissions.EditOwnedAdvertisement
  )
)
const isAllowedToDelete = computed(() =>
  AuthService.hasPermission(
    props.manageAll ? Permissions.DeleteAnyAdvertisement : Permissions.DeleteOwnedAdvertisement
  )
)

const columns: TableColumn[] = [
  new TableColumn({
    data: 'title',
    name: 'manageAdvertisements.title',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'ownerUsername',
    name: 'manageAdvertisements.ownerUsername',
    orderable: true,
    searchable: true
  }),
  new TableColumn({
    data: 'categoryName',
    name: 'manageAdvertisements.categoryName',
    orderable: true
  }),
  new TableColumn({
    data: 'status',
    name: 'manageAdvertisements.status',
    orderable: true
  }),
  new TableColumn({
    data: 'validToDate',
    name: 'manageAdvertisements.validTo',
    orderable: true
  }),
  new TableColumn({
    data: 'createdAtDate',
    name: 'manageAdvertisements.createdAt',
    orderable: true
  })
]

//watch
watch(LocaleService.currentLocaleName, () => {
  table.value?.refresh()
})

//Methods
const advertisementSource = (query: DataTableQuery) => {
  if (props.manageAll) {
    return advertisementService.getAllAdvertisements(query)
  } else {
    return advertisementService.getOwnedAdvertisements(query)
  }
}

const setAdvertisementActiveState = async (isActive: boolean) => {
  loading.value = true
  const ids = selectedAdvertisements.value
    .filter(
      (r) =>
        (isActive && r.status === PaymentSubjectStatus.Inactive) ||
        (!isActive && r.status === PaymentSubjectStatus.Active)
    )
    .map((a) => a.id!)
  const request = new SetActiveStatusRequest({ isActive, ids })
  if (props.manageAll) {
    await advertisementService.setIsActiveAnyAdvertisements(request)
  } else {
    await advertisementService.setIsActiveOwnedAdvertisements(request)
  }
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
  if (props.manageAll) {
    await advertisementService.deleteAnyAdvertisements(
      selectedAdvertisements.value.map((a) => a.id!)
    )
  } else {
    await advertisementService.deleteOwnedAdvertisements(
      selectedAdvertisements.value.map((a) => a.id!)
    )
  }
  table.value?.refresh()
}

const extendAdvertisements = () => {
  const advertisementIds = selectedAdvertisements.value
    .filter((a) => a.status !== PaymentSubjectStatus.Draft)
    .map((a) => a.id!)

  push({
    name: 'extend',
    params: {
      ids: JSON.stringify(advertisementIds),
      type: PaymentType.ExtendAdvertisement
    }
  })
}

const publishAdvertisement = (data: AdvertisementInfo) => {
  push({
    name: 'extend',
    params: {
      ids: '[' + data.id + ']',
      type: PaymentType.CreateAdvertisement
    }
  })
}
</script>
