<template>
  <ResponsivePanel :title="l.navigation.register">
    <form class="flex flex-col gap-3 lg:items-center bg-white" @submit="onSubmit">
      <CreateUserCommonInputs
        :fields="fields"
        :hasFormErrors="hasFormErrors"
        :formErrors="formErrors"
      />

      <Button type="submit" :label="l.navigation.register" :loading="isSubmitting" />
      <p>
        <span>{{ l.form.register.alreadyHaveAnAccount }}</span>
        <RouterLink class="ml-1 link" :to="{ name: 'login' }">{{ l.navigation.login }}</RouterLink>
      </p>
    </form>
  </ResponsivePanel>
</template>

<script setup lang="ts">
import ResponsivePanel from '@/components/common/ResponsivePanel.vue'
import CreateUserCommonInputs from '@/components/form/user/CreateUserCommonInputs.vue'
import { RegisterDto, UserClient, type FileParameter } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { boolean, object, string } from 'yup'

//Router
const { push } = useRouter()
const confirm = useConfirm()
const formSubmitted = ref(false)
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Services
const api = getClient(UserClient)
const l = LocaleService.currentLocale

// Form and fields
const form = useForm<RegisterDto>({
  validationSchema: toTypedSchema(
    object({
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
const { values, handleSubmit, isSubmitting, validate } = form
defineMultipleFields([
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
//Watchers
watch(LocaleService.currentLocaleName, () => {
  validate({ mode: 'validated-only' })
})

//Methods
const onSubmit = handleSubmit(async () => {
  try {
    const profileImage = values.profileImage
      ? ({
          data: values.profileImage,
          fileName: (values.profileImage as File).name
        } as FileParameter)
      : undefined

    await api.register(
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
      values.linkToUserSite
    )
    formSubmitted.value = true
    push({ name: 'login' })
  } catch (error) {
    handleErrors(error)
  }
})
</script>
