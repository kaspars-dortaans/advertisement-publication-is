<template>
  <EditUserCommonInputs
    :fields="fields as Fields<EditUserInfo>"
    :hasFormErrors="hasFormErrors"
    :formErrors="formErrors"
  >
    <slot></slot>

    <template #bottom>
      <FloatLabel variant="on">
        <Password
          v-model="fields.password!.value"
          v-bind="fields.password?.attributes"
          :invalid="fields.password!.hasError"
          id="password-input"
          fluid
        />
        <label for="password-input">{{ l.form.register.password }}</label>
      </FloatLabel>
      <FieldError :field="fields.password" />

      <FloatLabel variant="on">
        <Password
          v-model="fields.passwordConfirmation!.value"
          v-bind="fields.passwordConfirmation?.attributes"
          :feedback="false"
          :invalid="fields.passwordConfirmation!.hasError"
          id="confirm-password-input"
          fluid
        />
        <label for="confirm-password-input">{{ l.form.register.confirmPassword }}</label>
      </FloatLabel>
      <FieldError :field="fields.passwordConfirmation" />
    </template>
  </EditUserCommonInputs>
</template>

<script lang="ts" setup>
import type { EditUserInfo, RegisterDto } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { Fields } from '@/utils/field-helper'
import FieldError from '../FieldError.vue'
import EditUserCommonInputs from './EditUserCommonInputs.vue'

defineProps<{
  fields: Fields<RegisterDto>
  formErrors: string | undefined
  hasFormErrors: boolean | undefined
}>()

const l = LocaleService.currentLocale
</script>
