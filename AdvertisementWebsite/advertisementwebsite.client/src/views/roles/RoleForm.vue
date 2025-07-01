<template>
  <ResponsivePanel
    :defaultBackButtonRoute="{ name: 'manageRoles' }"
    :title="isEdit ? l.navigation.editRole : l.navigation.createRole"
    :loading="loading || isSubmitting"
  >
    <form class="flex flex-col gap-2" @submit="submit">
      <FieldError :messages="formErrors" />

      <!-- title -->
      <FloatLabel variant="on">
        <InputText
          v-model="fields.title!.value"
          v-bind="fields.title!.attributes"
          :invalid="fields.title!.hasError"
          id="title-input"
          fluid
        />
        <label for="title-input">{{ l.form.roleForm.title }}</label>
      </FloatLabel>
      <FieldError :field="fields.title" />

      <Divider />

      <CheckboxArrayInput
        v-model:selected="fields.permissions!.value"
        :options="permissionOptions"
        v-bind="fields.permissions!.attributes"
        :invalid="fields.permissions!.hasError"
        :label="l.form.roleForm.permissions"
        labelKey="value"
      />
      <FieldError :field="fields.permissions" />

      <Button
        :label="isEdit ? l.actions.save : l.actions.create"
        type="submit"
        class="mt-3 lg:self-center"
      />
    </form>
  </ResponsivePanel>
</template>

<script lang="ts" setup>
import ResponsivePanel from '@/components/common/ResponsivePanel.vue'
import CheckboxArrayInput from '@/components/form/CheckboxArrayInput.vue'
import FieldError from '@/components/form/FieldError.vue'
import { Int32StringKeyValuePair, RoleClient, RoleFormRequest } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { computed, onBeforeMount, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { array, number, object, string } from 'yup'

const props = defineProps<{
  roleId?: number
}>()

//Route
const { push } = useRouter()
const formSubmitted = ref(false)
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Services
const l = LocaleService.currentLocale
const confirm = useConfirm()
const roleService = getClient(RoleClient)

//Reactive data
const loading = ref(0)
const isEdit = computed(() => typeof props.roleId === 'number')
const permissionOptions = ref<Int32StringKeyValuePair[]>([])

//Forms and fields
const form = useForm({
  validationSchema: toTypedSchema(
    object({
      id: number(),
      title: string().required().default('').label('form.roleForm.title'),
      permissions: array().default([]).label('form.roleForm.permissions')
    })
  )
})
const { fields, formErrors, valuesChanged, defineMultipleFields, handleErrors } = new FieldHelper(
  form
)
const { handleSubmit, values, isSubmitting, resetForm, validate } = form
defineMultipleFields(['title', 'permissions', 'id'])

//Hooks
onBeforeMount(async () => {
  reloadData()
})

//Watchers
watch(LocaleService.currentLocaleName, async () => {
  await reloadData()
  validate({ mode: 'validated-only' })
})

//Methods
const reloadData = async () => {
  loading.value++
  const loadPermissionOptionPromise = loadPermissionOptions()
  if (isEdit.value) {
    const [formInfo] = await Promise.all([
      roleService.getRoleFormInfo(props.roleId),
      loadPermissionOptionPromise
    ])
    const selectedPermissions =
      formInfo.permissions?.map((p) => permissionOptions.value.findIndex((po) => po.key === p)) ??
      []

    resetForm({
      values: {
        id: formInfo.id,
        title: formInfo.name,
        permissions: selectedPermissions
      }
    })
  }
  loading.value--
}

const loadPermissionOptions = async () => {
  permissionOptions.value = await roleService.getPermissionOptions()
}

const submit = handleSubmit(async () => {
  try {
    const request = new RoleFormRequest({
      id: values.id,
      name: values.title,
      permissions: values.permissions?.map((pIndex) => permissionOptions.value[pIndex].key!) ?? []
    })

    if (isEdit.value) {
      await roleService.editRole(request)
    } else {
      await roleService.createRole(request)
    }
    formSubmitted.value = true
    push({ name: 'manageRoles' })
  } catch (e) {
    handleErrors(e)
  }
})
</script>
