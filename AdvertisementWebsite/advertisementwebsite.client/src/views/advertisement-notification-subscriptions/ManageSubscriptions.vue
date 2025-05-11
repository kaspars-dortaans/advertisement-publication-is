<template>
  <LazyLoadedTable
    v-model:loading="loading"
    v-model:selection="selectedRows"
    :columns="columns"
    :dataSource="advertisementNotificationSource"
    ref="table"
  >
    <template #header>
      <h3 class="page-title mb-2">
        {{
          canManageAll
            ? l.navigation.manageAdvertisementNotificationSubscription
            : l.navigation.myAdvertisementNotificationSubscriptions
        }}
      </h3>
      <div class="flex flex-wrap justify-end gap-2">
        <template v-if="isAllowedToEdit">
          <Button
            :label="l.actions.deactivate"
            :disabled="!selectedRows.length || !atLeastOneSelectedIsActive"
            severity="secondary"
            @click="setActiveState(false)"
          />
          <Button
            :label="l.actions.activate"
            :disabled="!selectedRows.length || !atLeastOneSelectedIsInactive"
            severity="primary"
            @click="setActiveState(true)"
          />
        </template>
        <Button
          v-if="isAllowedToDelete"
          :label="l.actions.delete"
          :disabled="!selectedRows.length"
          severity="danger"
          @click="confirmNotificationSubscriptionDelete"
        />
        <Button
          v-if="isAllowedToEdit"
          :label="l.actions.extend"
          :disabled="!selectedRows.length || allSelectedAreDrafts"
          severity="secondary"
          @click="extend"
        />
        <Button
          v-if="isAllowedToCreate"
          :label="l.actions.create"
          severity="primary"
          as="RouterLink"
          :to="{
            name: canManageAll
              ? 'createAdvertisementNotificationSubscription'
              : 'createOwnAdvertisementNotificationSubscription'
          }"
        />
      </div>
    </template>

    <Column selectionMode="multiple" headerStyle="width: 3rem" />

    <Column field="title" :header="l.manageAdvertisementNotificationSubscriptions.title" sortable />
    <Column
      v-if="canManageAll"
      field="ownerUsername"
      :header="l.manageAdvertisementNotificationSubscriptions.ownerUsername"
      sortable
    />
    <Column
      field="keywords"
      :header="l.manageAdvertisementNotificationSubscriptions.keywords"
      sortable
    >
      <template #body="slotProps">
        {{ slotProps.data.keywords?.join(' ') }}
      </template>
    </Column>
    <Column
      field="categoryName"
      :header="l.manageAdvertisementNotificationSubscriptions.categoryName"
      sortable
    />
    <Column field="status" :header="l.manageAdvertisementNotificationSubscriptions.status" sortable>
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
    <Column
      field="validToDate"
      :header="l.manageAdvertisementNotificationSubscriptions.validTo"
      sortable
    >
      <template #body="slotProps">{{
        slotProps.data.validToDate != null ? dateFormat.format(slotProps.data.validToDate) : ''
      }}</template>
    </Column>
    <Column
      field="createdDate"
      :header="l.manageAdvertisementNotificationSubscriptions.createdAt"
      sortable
    >
      <template #body="slotProps">{{
        slotProps.data.createdDate ? dateFormat.format(slotProps.data.createdDate) : ''
      }}</template>
    </Column>

    <Column>
      <template #body="slotProps">
        <div class="flex flex-wrap justify-end gap-2">
          <Button
            v-if="isAllowedToCreate && slotProps.data.status === PaymentSubjectStatus.Draft"
            :label="l.actions.subscribe"
            @click="subscribe(slotProps.data)"
          />
          <Button
            v-if="isAllowedToEdit"
            :label="l.actions.edit"
            as="RouterLink"
            :to="{
              name: canManageAll
                ? 'editAnyAdvertisementNotificationSubscription'
                : 'editAdvertisementNotificationSubscription',
              params: { subscriptionId: '' + slotProps.data.id }
            }"
          />
          <Button
            :label="l.actions.view"
            severity="secondary"
            as="RouterLink"
            :to="{
              name: canManageAll
                ? 'viewAnyAdvertisementNotificationSubscription'
                : 'viewAdvertisementNotificationSubscription',
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
import { Permissions } from '@/constants/api/Permissions'
import { statusSeverity } from '@/constants/status-severity'
import {
  AdvertisementNotificationClient,
  DataTableQuery,
  NotificationSubscriptionItem,
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

const { push } = useRouter()
const props = defineProps<{
  canManageAll?: boolean
}>()

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
    data: 'ownerUsername',
    name: 'manageAdvertisementNotificationSubscriptions.ownerUsername',
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
    data: 'status',
    name: 'manageAdvertisementNotificationSubscriptions.status',
    orderable: true
  }),
  new TableColumn({
    data: 'validToDate',
    name: 'manageAdvertisementNotificationSubscriptions.validToDate',
    orderable: true
  }),
  new TableColumn({
    data: 'createdDate',
    name: 'manageAdvertisementNotificationSubscriptions.createdDate',
    orderable: true
  })
]

//Reactive data
const selectedRows = ref<NotificationSubscriptionItem[]>([])
const atLeastOneSelectedIsActive = computed(() =>
  selectedRows.value.some((r) => r.status === PaymentSubjectStatus.Active)
)
const atLeastOneSelectedIsInactive = computed(() =>
  selectedRows.value.some((r) => r.status === PaymentSubjectStatus.Inactive)
)
const allSelectedAreDrafts = computed(() =>
  selectedRows.value.every((a) => a.status == PaymentSubjectStatus.Draft)
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
    props.canManageAll
      ? Permissions.CreateAdvertisementNotificationSubscription
      : Permissions.CreateOwnedAdvertisementNotificationSubscription
  )
)
const isAllowedToEdit = computed(() =>
  AuthService.hasPermission(
    props.canManageAll
      ? Permissions.EditAnyAdvertisementNotificationSubscription
      : Permissions.EditOwnedAdvertisementNotificationSubscriptions
  )
)
const isAllowedToDelete = computed(() =>
  AuthService.hasPermission(
    props.canManageAll
      ? Permissions.DeleteAnyAdvertisementNotificationSubscription
      : Permissions.DeleteOwnedAdvertisementNotificationSubscriptions
  )
)

//watch
watch(LocaleService.currentLocaleName, () => {
  table.value?.refresh()
})

//Methods
const advertisementNotificationSource = async (query: DataTableQuery) => {
  if (props.canManageAll) {
    return await subscriptionService.getAllAdvertisementNotificationSubscriptions(query)
  } else {
    return await subscriptionService.getAdvertisementNotificationSubscriptions(query)
  }
}

const setActiveState = async (isActive: boolean) => {
  loading.value = true
  const request = new SetActiveStatusRequest({
    ids: selectedRows.value
      .filter(
        (r) =>
          (isActive && r.status === PaymentSubjectStatus.Inactive) ||
          (!isActive && r.status === PaymentSubjectStatus.Active)
      )
      .map((i) => i.id!),
    isActive
  })

  if (props.canManageAll) {
    await subscriptionService.setAnySubscriptionActiveStatus(request)
  } else {
    await subscriptionService.setSubscriptionActiveStatus(request)
  }
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
      const ids = selectedRows.value.map((i) => i.id!)
      if (props.canManageAll) {
        await subscriptionService.deleteAnySubscriptions(ids)
      } else {
        await subscriptionService.deleteSubscriptions(ids)
      }
      table.value?.refresh()
      loading.value = false
    }
  })
}

const extend = () => {
  push({
    name: 'extend',
    params: {
      ids: JSON.stringify(
        selectedRows.value.filter((r) => r.status !== PaymentSubjectStatus.Draft).map((r) => r.id!)
      ),
      type: PaymentType.ExtendAdvertisementNotificationSubscription
    }
  })
}

const subscribe = (row: NotificationSubscriptionItem) => {
  push({
    name: 'extend',
    params: {
      ids: '[' + row.id + ']',
      type: PaymentType.CreateAdvertisementNotificationSubscription
    }
  })
}
</script>
