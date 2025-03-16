<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading">
      <Panel>
        <template #header>
          <div class="panel-title-container">
            <BackButton :default-to="{ name: 'profileInfo' }" />
            <h3 class="page-title">{{ l.navigation.editProfileInfo }}</h3>
          </div>
        </template>
        <form class="flex flex-col gap-3 md:items-center bg-white" @submit="onSubmit">
          <div class="flex flex-col gap-5 items-center lg:flex-row">
            <div class="flex flex-col gap-2 min-w-full md:min-w-80">
              <Message v-if="hasFormErrors" severity="error">{{ formErrors }}</Message>

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

              <InputText
                v-model="fields.linkToUserSite!.value"
                v-bind="fields.linkToUserSite?.attributes"
                :placeholder="l.form.register.linkToUserSite"
                :invalid="fields.linkToUserSite!.hasError"
              />
              <FieldError :field="fields.linkToUserSite" />
            </div>

            <ImageUpload
              v-model="fields.profileImage!.value"
              v-bind="fields.profileImage?.attributes"
              :invalid="fields.profileImage?.hasError"
              :maxFileSize="ProfileImageConstants.MaxFileSizeInBytes"
              :allowedFileTypes="ProfileImageConstants.AllowedFileTypes"
            />
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
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import BackButton from '@/components/BackButton.vue'
import BlockWithSpinner from '@/components/Common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/Common/ResponsiveLayout.vue'
import FieldError from '@/components/Form/FieldError.vue'
import ImageUpload from '@/components/Form/ImageUpload.vue'
import { ProfileImageConstants } from '@/constants/api/ProfileImageConstants'
import { EditUserInfo, UserClient, type FileParameter } from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { FieldHelper } from '@/utils/field-helper'
import { downloadFile } from '@/utils/file-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useForm } from 'vee-validate'
import { onBeforeMount, ref } from 'vue'
import { useRouter } from 'vue-router'
import { boolean, object, string } from 'yup'

const { push } = useRouter()

//Services
const l = LocaleService.currentLocale
const authService = AuthService.get()
const userService = getClient(UserClient)

//Forms and fields
const form = useForm<EditUserInfo>({
  validationSchema: toTypedSchema(
    object({
      firstName: string().required(),
      lastName: string().required(),
      userName: string().required(),
      email: string().required().email(),
      isEmailPublic: boolean().default(false),
      phoneNumber: string().required(),
      isPhoneNumberPublic: boolean().default(false),
      linkToUserSite: string().url().nullable()
    })
  )
})
const { fields, hasFormErrors, formErrors, defineMultipleFields, handleErrors } = new FieldHelper(
  form
)
const { handleSubmit, values, isSubmitting, setValues } = form

defineMultipleFields([
  'firstName',
  'lastName',
  'userName',
  'email',
  'isEmailPublic',
  'phoneNumber',
  'isPhoneNumberPublic',
  'profileImage',
  'linkToUserSite'
])

//Reactive data
const originalProfileImage = ref<File | undefined>()
const loading = ref(false)

//Hooks
onBeforeMount(async () => {
  loading.value = true
  await authService.refreshProfileData()
  const userInfo = await AuthService.profileInfo.value

  let profileImage
  if (userInfo?.profileImageUrl?.url) {
    profileImage = await downloadFile(userInfo.profileImageUrl.url)
    originalProfileImage.value = profileImage
  }

  setValues(
    new EditUserInfo({
      userName: userInfo?.userName!,
      firstName: userInfo?.firstName!,
      lastName: userInfo?.lastName!,
      email: userInfo?.email!,
      isEmailPublic: userInfo?.isEmailPublic,
      phoneNumber: userInfo?.phoneNumber!,
      isPhoneNumberPublic: userInfo?.isPhoneNumberPublic,
      linkToUserSite: userInfo?.linkToUserSite,
      profileImage: profileImage
    })
  )
  loading.value = false
})

//Methods
const onSubmit = handleSubmit(async () => {
  try {
    const profileImageChanged = values.profileImage !== originalProfileImage.value
    const profileImage =
      profileImageChanged && values.profileImage
        ? ({
            data: values.profileImage,
            fileName: (values.profileImage as File).name
          } as FileParameter)
        : undefined

    await userService.updateUserInfo(
      values.email,
      values.isEmailPublic,
      values.firstName,
      values.lastName,
      values.userName,
      values.phoneNumber,
      values.isPhoneNumberPublic,
      values.linkToUserSite,
      profileImageChanged,
      profileImage
    )
    push({ name: 'profileInfo' })
  } catch (error) {
    handleErrors(error)
  }
})
</script>
