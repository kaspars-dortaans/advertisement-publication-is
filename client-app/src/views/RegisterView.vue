<template>
  <div class="flex items-center justify-center flex-1 bg-primary">
    <Panel>
      <template #header>
        <span class="text-2xl">{{ ls.l('register') }}</span>
      </template>
      <form class="flex flex-col gap-3 items-center bg-white" @submit="onSubmit">
        <div class="flex gap-5 items-center">
          <div class="flex flex-col gap-2 min-w-80">
            <Message v-if="fieldHelper.hasFormErrors.value" severity="error">{{
              fieldHelper.formErrors
            }}</Message>

            <InputText
              v-model="fields.firstName!.value"
              v-bind="fields.firstName?.attributes"
              :placeholder="ls.l('firstName')"
              :invalid="fields.firstName!.hasError"
            />
            <FieldError :field="fields.firstName" />

            <InputText
              v-model="fields.lastName!.value"
              v-bind="fields.lastName?.attributes"
              :placeholder="ls.l('lastName')"
              :invalid="fields.lastName!.hasError"
            />
            <FieldError :field="fields.lastName" />

            <InputText
              v-model="fields.userName!.value"
              v-bind="fields.userName?.attributes"
              :placeholder="ls.l('username')"
              :invalid="fields.userName!.hasError"
            />
            <FieldError :field="fields.userName" />

            <InputText
              v-model="fields.email!.value"
              v-bind="fields.email!.attributes"
              :placeholder="ls.l('email')"
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
                ls.l('publiclyDisplayEmail')
              }}</label>
            </div>
            <FieldError :field="fields.isEmailPublic" />

            <InputText
              v-model="fields.phoneNumber!.value"
              v-bind="fields.phoneNumber?.attributes"
              :placeholder="ls.l('phoneNumber')"
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
                ls.l('publiclyDisplayPhoneNumber')
              }}</label>
            </div>
            <FieldError :field="fields.isPhoneNumberPublic" />

            <Password
              v-model="fields.password!.value"
              v-bind="fields.password?.attributes"
              :placeholder="ls.l('password')"
              :invalid="fields.password!.hasError"
            />
            <FieldError :field="fields.password" />

            <Password
              v-model="fields.passwordConfirmation!.value"
              v-bind="fields.passwordConfirmation?.attributes"
              :placeholder="ls.l('confirmPassword')"
              :feedback="false"
              :invalid="fields.passwordConfirmation!.hasError"
            />
            <FieldError :field="fields.passwordConfirmation" />
          </div>

          <ImageUpload
            v-model="fields.profileImage!.value"
            v-bind="fields.profileImage?.attributes"
            :invalid="fields.profileImage?.hasError"
          />
          <FieldError :field="fields.profileImage" />
        </div>

        <Button type="submit" :label="ls.l('register')" />
        <p>
          <span>{{ ls.l('alreadyHaveAnAccount') }}</span>
          <RouterLink class="ml-1 link" :to="{ name: 'login' }">{{ ls.l('login') }}</RouterLink>
        </p>
      </form>
    </Panel>
  </div>
</template>

<script setup lang="ts">
import FieldError from '@/components/FieldError.vue'
import ImageUpload from '@/components/ImageUpload.vue'
import { UserClient, type FileParameter, type IRegisterDto } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { FieldHelper } from '@/utils/field-helper'
import { useForm } from 'vee-validate'
import { useRouter } from 'vue-router'

const api = getClient(UserClient)
const ls = new LocaleService()
const router = useRouter()

// Form and fields
const form = useForm<IRegisterDto>()
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
const { values, handleSubmit, validate, setValues } = form

setValues({
  isEmailPublic: false,
  isPhoneNumberPublic: false
})

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
