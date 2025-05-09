<template>
  <div class="flex flex-col gap-5 items-center lg:flex-row">
    <div class="flex flex-col gap-2 min-w-full md:min-w-80">
      <Message v-if="hasFormErrors" severity="error">{{ formErrors }}</Message>

      <slot></slot>

      <FloatLabel variant="on">
        <InputText
          v-model="fields.firstName!.value"
          v-bind="fields.firstName?.attributes"
          :invalid="fields.firstName!.hasError"
          id="first-name-input"
          fluid
        />
        <label for="first-name-input">{{ l.form.register.firstName }}</label>
      </FloatLabel>
      <FieldError :field="fields.firstName" />

      <FloatLabel variant="on">
        <InputText
          v-model="fields.lastName!.value"
          v-bind="fields.lastName?.attributes"
          :invalid="fields.lastName!.hasError"
          id="last-name-input"
          fluid
        />
        <label for="last-name-input">{{ l.form.register.lastName }}</label>
      </FloatLabel>
      <FieldError :field="fields.lastName" />

      <FloatLabel variant="on">
        <InputText
          v-model="fields.userName!.value"
          v-bind="fields.userName?.attributes"
          :invalid="fields.userName!.hasError"
          id="user-name-input"
          fluid
        />
        <label for="user-name-input">{{ l.form.register.username }}</label>
      </FloatLabel>
      <FieldError :field="fields.userName" />

      <FloatLabel variant="on">
        <InputText
          v-model="fields.email!.value"
          v-bind="fields.email!.attributes"
          :invalid="fields.email!.hasError"
          id="email-input"
          fluid
        />
        <label for="email-input">{{ l.form.register.email }}</label>
      </FloatLabel>
      <FieldError :field="fields.email" />

      <div class="flex items-center">
        <Checkbox
          v-model="fields.isEmailPublic!.value"
          v-bind="fields.isEmailPublic?.attributes"
          :invalid="fields.isEmailPublic!.hasError"
          :binary="true"
          inputId="register.isEmailPublic"
          fluid
        />
        <label class="ml-2" for="register.isEmailPublic">{{
          l.form.register.publiclyDisplayEmail
        }}</label>
      </div>
      <FieldError :field="fields.isEmailPublic" />

      <FloatLabel variant="on">
        <InputText
          v-model="fields.phoneNumber!.value"
          v-bind="fields.phoneNumber?.attributes"
          :invalid="fields.phoneNumber!.hasError"
          id="phone-number-input"
          fluid
        />
        <label for="phone-number-input">{{ l.form.register.phoneNumber }}</label>
      </FloatLabel>
      <FieldError :field="fields.phoneNumber" />

      <div class="flex items-center">
        <Checkbox
          v-model="fields.isPhoneNumberPublic!.value"
          v-bind="fields.isPhoneNumberPublic?.attributes"
          :invalid="!!fields.isPhoneNumberPublic!.hasError"
          :binary="true"
          inputId="register.isPhonePublic"
          fluid
        />
        <label class="ml-2" for="register.isPhonePublic">{{
          l.form.register.publiclyDisplayPhoneNumber
        }}</label>
      </div>
      <FieldError :field="fields.isPhoneNumberPublic" />

      <FloatLabel variant="on">
        <InputText
          v-model="fields.linkToUserSite!.value"
          v-bind="fields.linkToUserSite?.attributes"
          :invalid="fields.linkToUserSite!.hasError"
          id="link-to-user-site-input"
          fluid
        />
        <label for="link-to-user-site-input">{{ l.form.register.linkToUserSite }}</label>
      </FloatLabel>
      <FieldError :field="fields.linkToUserSite" />

      <slot name="bottom"></slot>
    </div>

    <ProfileImageUpload
      v-model="fields.profileImage!.value"
      v-bind="fields.profileImage?.attributes"
      :invalid="fields.profileImage?.hasError"
      :maxFileSize="ImageConstants.MaxFileSizeInBytes"
      :allowedFileTypes="ImageConstants.AllowedFileTypes"
    />
  </div>
</template>

<script lang="ts" setup>
import { ImageConstants } from '@/constants/api/ImageConstants'
import type { EditUserInfo } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { Fields } from '@/utils/field-helper'
import ProfileImageUpload from '../ProfileImageUpload.vue'
import FieldError from '../FieldError.vue'

defineProps<{
  fields: Fields<EditUserInfo>
  hasFormErrors?: boolean
  formErrors: string | undefined
}>()

const l = LocaleService.currentLocale
</script>
