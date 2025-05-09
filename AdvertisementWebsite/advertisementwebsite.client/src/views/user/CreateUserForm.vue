<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading || isSubmitting" class="flex-1 lg:flex-none flex flex-col">
      <Panel class="rounded-none lg:rounded-md flex-1">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'manageUsers' }" />
            <h4 class="page-title">
              {{ l.navigation.createUser }}
            </h4>
          </div>
        </template>

        <form class="flex flex-col gap-2" @submit="submit">
          <CreateUserCommonInputs
            :fields="fields as Fields<RegisterDto>"
            :formErrors="formErrors"
            :hasFormErrors="hasFormErrors"
          >
            <FloatLabel variant="on">
              <!-- Filter type -->
              <MultiSelect
                v-model="fields.userRoles!.value"
                v-bind="fields.userRoles!.attributes"
                :invalid="fields.userRoles!.hasError"
                :options="roleList"
                display="chip"
                id="user-role-input"
                fluid
              />
              <label for="user-role-input">{{ l.form.register.userRoles }}</label>
            </FloatLabel>
            <FieldError :field="fields.userRoles" />
          </CreateUserCommonInputs>

          <Button
            :label="l.actions.create"
            :loading="isSubmitting"
            type="submit"
            class="mt-3 self-stretch lg:self-center"
          />
        </form>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script lang="ts" setup>
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import CreateUserCommonInputs from '@/components/form/user/CreateUserCommonInputs.vue'
import FieldError from '@/components/form/FieldError.vue'
import {
  RegisterDto,
  UserClient,
  type CreateUserRequest,
  type FileParameter
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper, type Fields } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { onBeforeMount, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { array, boolean, object, string } from 'yup'

//Route
const { push } = useRouter()
const formSubmitted = ref(false)
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Services
const l = LocaleService.currentLocale
const confirm = useConfirm()
const userService = getClient(UserClient)

//Reactive data
const loading = ref(0)
const roleList = ref<string[]>([])

//Forms and fields
const form = useForm<CreateUserRequest>({
  validationSchema: toTypedSchema(
    object({
      userRoles: array().default([]).label('form.register.userRoles'),
      firstName: string().required().default('').label('form.register.firstName'),
      lastName: string().required().default('').label('form.register.lastName'),
      userName: string().required().default('').label('form.register.username'),
      email: string().required().email().default('').label('form.register.email'),
      isEmailPublic: boolean().default(false).label('form.register.publiclyDisplayEmail'),
      phoneNumber: string().required().default('').label('form.register.phoneNumber'),
      isPhoneNumberPublic: boolean()
        .default(false)
        .label('form.register.publiclyDisplayPhoneNumber'),
      password: string().required().default('').label('form.register.password'),
      passwordConfirmation: string().required().default('').label('form.register.confirmPassword'),
      linkToUserSite: string().nullable().default('').label('form.register.linkToUserSite')
    })
  )
})
const { fields, formErrors, valuesChanged, hasFormErrors, defineMultipleFields, handleErrors } =
  new FieldHelper(form)
const { handleSubmit, values, isSubmitting, validate } = form
defineMultipleFields([
  'userRoles',
  'firstName',
  'lastName',
  'userName',
  'email',
  'isEmailPublic',
  'phoneNumber',
  'isPhoneNumberPublic',
  'password',
  'passwordConfirmation',
  'profileImage',
  'linkToUserSite'
])

//Hooks
onBeforeMount(async () => {
  reloadData()
})

//Watchers
watch(LocaleService.currentLocaleName, async () => {
  validate({ mode: 'validated-only' })
})

//Methods
const reloadData = async () => {
  loading.value++
  roleList.value = await userService.getRoleList()
  loading.value--
}

const submit = handleSubmit(async () => {
  try {
    const profileImage = values.profileImage
      ? ({
          data: values.profileImage,
          fileName: (values.profileImage as File).name
        } as FileParameter)
      : undefined

    await userService.createUser(
      values.email,
      values.isEmailPublic,
      values.password,
      values.passwordConfirmation,
      values.firstName,
      values.lastName,
      values.userName,
      values.phoneNumber,
      values.isPhoneNumberPublic,
      profileImage,
      values.linkToUserSite,
      values.userRoles
    )
    formSubmitted.value = true
    push({ name: 'manageUsers' })
  } catch (e) {
    handleErrors(e)
  }
})
</script>
