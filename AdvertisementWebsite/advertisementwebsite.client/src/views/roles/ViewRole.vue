<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading" class="flex-1 flex flex-col">
      <Panel class="rounded-none lg:rounded-md flex-1">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageRoles' }" />
            <h4 class="page-title">
              {{ l.navigation.viewRole }}
            </h4>
            <Button
              v-if="isAllowedToEdit"
              :label="l.actions.edit"
              icon="pi pi-pencil"
              severity="secondary"
              as="RouterLink"
              :to="{ name: 'editRole', params: { roleId } }"
            />
          </div>
        </template>

        <div class="flex flex-col gap-2">
          <!-- title -->
          <FloatLabel variant="on">
            <InputText v-model="title" id="title-input" fluid disabled />
            <label for="title-input">{{ l.form.roleForm.title }}</label>
          </FloatLabel>

          <Divider />

          <CheckboxArrayInput
            v-model:selected="selectedPermissions"
            :options="permissionOptions"
            :label="l.form.roleForm.permissions"
            labelKey="value"
            disabled
          />
        </div>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import CheckboxArrayInput from '@/components/form/CheckboxArrayInput.vue'
import { Permissions } from '@/constants/api/Permissions'
import { Int32StringKeyValuePair, RoleClient } from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { computed, onBeforeMount, ref, watch } from 'vue'

const props = defineProps<{
  roleId?: number
}>()

//Services
const l = LocaleService.currentLocale
const roleService = getClient(RoleClient)

//Reactive data
const loading = ref(0)
const permissionOptions = ref<Int32StringKeyValuePair[]>([])
const title = ref('')
const selectedPermissions = ref<number[]>([])
const isAllowedToEdit = computed(() => AuthService.hasPermission(Permissions.EditRole))

//Hooks
onBeforeMount(async () => {
  reloadData()
})

//Watchers
watch(LocaleService.currentLocaleName, async () => {
  await reloadData()
})

//Methods
const reloadData = async () => {
  loading.value++
  const [formInfo, options] = await Promise.all([
    roleService.getRoleFormInfo(props.roleId),
    await roleService.getPermissionOptions()
  ])

  permissionOptions.value = options
  title.value = formInfo.name ?? ''
  selectedPermissions.value =
    formInfo.permissions?.map((p) => permissionOptions.value.findIndex((po) => po.key === p)) ?? []
  loading.value--
}
</script>
