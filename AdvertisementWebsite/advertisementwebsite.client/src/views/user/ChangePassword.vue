<template>
  <ResponsiveLayout>
    <div class="flex-1 lg:flex-none flex flex-col gap-4 bg-white rounded-none lg:rounded-md">
      <div class="panel-title-container p-4">
        <BackButton :defaultTo="{ name: 'profileInfo' }" />
        <h3 class="page-title">{{ l.navigation.changePassword }}</h3>
      </div>

      <form
        class="flex-1 flex flex-col justify-center items-center gap-5 px-4 pb-4"
        @submit="changePassword"
      >
        <div class="flex flex-col gap-2">
          <Message v-if="hasFormErrors" severity="error">{{ formErrors }}</Message>

          <FloatLabel variant="on">
            <Password
              v-model="fields.currentPassword!.value"
              v-bind="fields.currentPassword?.attributes"
              :invalid="fields.currentPassword!.hasError"
              :feedback="false"
              id="current-password-input"
            />
            <label for="current-password-input">{{ l.form.changePassword.currentPassword }}</label>
          </FloatLabel>
          <FieldError :field="fields.currentPassword" />

          <FloatLabel variant="on">
            <Password
              v-model="fields.password!.value"
              v-bind="fields.password?.attributes"
              :invalid="fields.password!.hasError"
              id="new-password-input"
            />
            <label for="new-password-input">{{ l.form.changePassword.newPassword }}</label>
          </FloatLabel>
          <FieldError :field="fields.password" />

          <FloatLabel variant="on">
          <Password
            v-model="fields.confirmPassword!.value"
            v-bind="fields.confirmPassword?.attributes"
            :invalid="fields.confirmPassword!.hasError"
            :feedback="false"
            id="new-password-confirm-input"
          />
          <label for="new-password-confirm-input">{{ l.form.changePassword.confirmNewPassword }}</label>
          </FloatLabel>
          <FieldError :field="fields.confirmPassword" />
        </div>
        <div class="flex flex-wrap gap-2">
          <BackButton
            :default-to="{ name: 'profileInfo' }"
            :label="l.actions.cancel"
            class="flex-auto lg:flex-none"
            noIcon
          />
          <Button
            type="submit"
            :label="l.actions.save"
            :loading="isSubmitting"
            class="flex-auto lg:flex-none"
          />
        </div>
      </form>
    </div>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import { LocaleService } from '@/services/locale-service'
import { useForm } from 'vee-validate'
import { ChangePasswordRequest, UserClient } from '@/services/api-client'
import { toTypedSchema } from '@vee-validate/yup'
import { object, string } from 'yup'
import { FieldHelper } from '@/utils/field-helper'
import { getClient } from '@/utils/client-builder'
import BackButton from '@/components/common/BackButton.vue'
import FieldError from '@/components/form/FieldError.vue'
import { useRouter } from 'vue-router'
import { watch } from 'vue'

const { push } = useRouter()

//Services
const l = LocaleService.currentLocale
const userService = getClient(UserClient)

//Forms and fields
const form = useForm<ChangePasswordRequest>({
  validationSchema: toTypedSchema(
    object({
      currentPassword: string().required().label('form.changePassword.currentPassword'),
      password: string().required().label('form.changePassword.newPassword'),
      confirmPassword: string().required().label('form.changePassword.confirmNewPassword')
    })
  )
})
const { isSubmitting, handleSubmit, values, validate } = form
const { fields, formErrors, hasFormErrors, defineMultipleFields, handleErrors } = new FieldHelper(
  form
)
defineMultipleFields(['currentPassword', 'password', 'confirmPassword'])

//Watchers
watch(LocaleService.currentLocaleName, () => {
  validate({ mode: 'validated-only' })
})

//Methods
const changePassword = handleSubmit(async () => {
  try {
    await userService.changePassword(new ChangePasswordRequest(values))
    push({ name: 'profileInfo' })
  } catch (errors) {
    handleErrors(errors)
  }
})
</script>
