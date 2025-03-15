<template>
  <ResponsiveLayout>
    <Panel>
      <template #header>
        <span class="text-2xl">{{ l.navigation.register }}</span>
      </template>
      <form class="flex flex-col gap-3 lg:items-center bg-white" @submit="onSubmit">
        <div class="flex flex-col gap-5 lg:items-center lg:flex-row">
          <div class="flex flex-col gap-2 min-w-full md:min-w-80">
            <Message v-if="fieldHelper.hasFormErrors.value" severity="error">{{
              fieldHelper.formErrors
            }}</Message>

            <InputText
              v-model="fields.firstName!.value"
              v-bind="fields.firstName?.attributes"
              :placeholder="l.form.register.firstName"
              :invalid="fields.firstName!.hasError"
            />
            <FieldError :field="fields.firstName" />

            <InputText
              v-model="fields.lastName!.value"
              v-bind="fields.lastName?.attributes"
              :placeholder="l.form.register.lastName"
              :invalid="fields.lastName!.hasError"
            />
            <FieldError :field="fields.lastName" />

            <InputText
              v-model="fields.userName!.value"
              v-bind="fields.userName?.attributes"
              :placeholder="l.form.register.username"
              :invalid="fields.userName!.hasError"
            />
            <FieldError :field="fields.userName" />

            <InputText
              v-model="fields.email!.value"
              v-bind="fields.email!.attributes"
              :placeholder="l.form.register.email"
              :invalid="fields.email!.hasError"
            />
            <FieldError :field="fields.email" />

            <div class="flex items-center">
              <Checkbox
                v-model="fields.isEmailPublic!.value"
                v-bind="fields.isEmailPublic?.attributes"
                :invalid="fields.isEmailPublic!.hasError"
                :binary="true"
                inputId="register.isEmailPublic"
              />
              <label class="ml-2" for="register.isEmailPublic">{{
                l.form.register.publiclyDisplayEmail
              }}</label>
            </div>
            <FieldError :field="fields.isEmailPublic" />

            <InputText
              v-model="fields.phoneNumber!.value"
              v-bind="fields.phoneNumber?.attributes"
              :placeholder="l.form.register.phoneNumber"
              :invalid="fields.phoneNumber!.hasError"
            />
            <FieldError :field="fields.phoneNumber" />

            <div class="flex items-center">
              <Checkbox
                v-model="fields.isPhoneNumberPublic!.value"
                v-bind="fields.isPhoneNumberPublic?.attributes"
                :invalid="!!fields.isPhoneNumberPublic!.hasError"
                :binary="true"
                inputId="register.isPhonePublic"
              />
              <label class="ml-2" for="register.isPhonePublic">{{
                l.form.register.publiclyDisplayPhoneNumber
              }}</label>
            </div>
            <FieldError :field="fields.isPhoneNumberPublic" />

            <Password
              v-model="fields.password!.value"
              v-bind="fields.password?.attributes"
              :placeholder="l.form.register.password"
              :invalid="fields.password!.hasError"
            />
            <FieldError :field="fields.password" />

            <Password
              v-model="fields.passwordConfirmation!.value"
              v-bind="fields.passwordConfirmation?.attributes"
              :placeholder="l.form.register.confirmPassword"
              :feedback="false"
              :invalid="fields.passwordConfirmation!.hasError"
            />
            <FieldError :field="fields.passwordConfirmation" />
          </div>

          <ImageUpload
            v-model="fields.profileImage!.value"
            v-bind="fields.profileImage?.attributes"
            :invalid="fields.profileImage?.hasError"
            :maxFileSize="ProfileImageConstants.MaxFileSizeInBytes"
            :allowedFileTypes="ProfileImageConstants.AllowedFileTypes"
          />
        </div>

        <Button type="submit" :label="l.navigation.register" :loading="isSubmitting" />
        <p>
          <span>{{ l.form.register.alreadyHaveAnAccount }}</span>
          <RouterLink class="ml-1 link" :to="{ name: 'login' }">{{
            l.navigation.login
          }}</RouterLink>
        </p>
      </form>
    </Panel>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import ResponsiveLayout from '@/components/Common/ResponsiveLayout.vue'
import FieldError from '@/components/Form/FieldError.vue'
import ImageUpload from '@/components/Form/ImageUpload.vue'
import { ProfileImageConstants } from '@/constants/api/ProfileImageConstants'
import { UserClient, type FileParameter, type IRegisterDto } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { FieldHelper } from '@/utils/field-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useForm } from 'vee-validate'
import { useRouter } from 'vue-router'
import { boolean, object, string } from 'yup'

//Services
const api = getClient(UserClient)
const l = LocaleService.currentLocale
const router = useRouter()

// Form and fields
const form = useForm<IRegisterDto>({
  validationSchema: toTypedSchema(
    object({
      firstName: string().required(),
      lastName: string().required(),
      userName: string().required(),
      email: string().required().email(),
      isEmailPublic: boolean().default(false),
      phoneNumber: string().required(),
      isPhoneNumberPublic: boolean().default(false),
      password: string().required(),
      passwordConfirmation: string().required()
    })
  )
})
const fieldHelper = new FieldHelper<IRegisterDto>(form)
const fields = fieldHelper.defineMultipleFields([
  'firstName',
  'lastName',
  'userName',
  'email',
  'isEmailPublic',
  'phoneNumber',
  'isPhoneNumberPublic',
  'password',
  'passwordConfirmation',
  'profileImage'
])
const { values, handleSubmit, validate, isSubmitting } = form

const onSubmit = handleSubmit(async () => {
  fieldHelper.clearErrors()
  await validate()

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
      profileImage
    )
    router.push({ name: 'login' })
  } catch (error) {
    fieldHelper.handleErrors(error)
  }
})
</script>
