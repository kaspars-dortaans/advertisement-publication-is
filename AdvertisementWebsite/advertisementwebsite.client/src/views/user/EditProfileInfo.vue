<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading">
      <Panel class="rounded-none lg:rounded-md">
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

            <ProfileImageUpload
              v-model="fields.profileImage!.value"
              v-bind="fields.profileImage?.attributes"
              :invalid="fields.profileImage?.hasError"
              :maxFileSize="ImageConstants.MaxFileSizeInBytes"
              :allowedFileTypes="ImageConstants.AllowedFileTypes"
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
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import FieldError from '@/components/form/FieldError.vue'
import ProfileImageUpload from '@/components/form/ProfileImageUpload.vue'
import { ImageConstants } from '@/constants/api/ImageConstants'
import {
  EditUserInfo,
  UserClient,
  type FileParameter,
  type IEditUserInfo
} from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { FieldHelper } from '@/utils/field-helper'
import { downloadFile, hashFile } from '@/utils/file-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { onBeforeMount, ref, watch } from 'vue'
import { useRouter, onBeforeRouteLeave } from 'vue-router'
import { boolean, object, string } from 'yup'
import { leaveFormGuard } from '@/utils/confirm-dialog'

//Route
const { push } = useRouter()
const formSubmitted = ref(false)
const confirm = useConfirm()
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Services
const l = LocaleService.currentLocale
const authService = AuthService.get()
const userService = getClient(UserClient)

//Forms and fields
const form = useForm<IEditUserInfo>({
  validationSchema: toTypedSchema(
    object({
      firstName: string().required().label('form.profileInfo.firstName'),
      lastName: string().required().label('form.profileInfo.lastName'),
      userName: string().required().label('form.profileInfo.userName'),
      email: string().required().email().label('form.profileInfo.email'),
      isEmailPublic: boolean().default(false).label('form.profileInfo.publiclyDisplayEmail'),
      phoneNumber: string().required().label('form.profileInfo.phoneNumber'),
      isPhoneNumberPublic: boolean()
        .default(false)
        .label('form.profileInfo.publiclyDisplayPhoneNumber'),
      linkToUserSite: string().url().nullable().label('form.profileInfo.linkToUserSite')
    })
  )
})
const { fields, hasFormErrors, formErrors, valuesChanged, defineMultipleFields, handleErrors } =
  new FieldHelper(form)
const { handleSubmit, values, isSubmitting, resetForm, validate } = form

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
  const userInfo = (await AuthService.profileInfoPromise.value)!

  let profileImage
  if (userInfo.profileImage?.imageURLs?.url) {
    profileImage = await downloadFile(userInfo.profileImage.imageURLs.url)
    originalProfileImage.value = profileImage
  }

  resetForm({
    values: new EditUserInfo({
      userName: userInfo.userName!,
      firstName: userInfo.firstName!,
      lastName: userInfo.lastName!,
      email: userInfo.email!,
      isEmailPublic: userInfo.isEmailPublic,
      phoneNumber: userInfo.phoneNumber!,
      isPhoneNumberPublic: userInfo.isPhoneNumberPublic,
      linkToUserSite: userInfo.linkToUserSite,
      profileImage: profileImage
    })
  })
  loading.value = false
})

//Watchers
watch(LocaleService.currentLocaleName, () => {
  validate({ mode: 'validated-only' })
})

//Methods
const onSubmit = handleSubmit(async () => {
  try {
    const fileHash = values.profileImage ? await hashFile(values.profileImage) : undefined
    const profileImageChanged = fileHash !== AuthService.profileInfo.value?.profileImage?.hash
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
    formSubmitted.value = true
    push({ name: 'profileInfo' })
  } catch (error) {
    handleErrors(error)
  }
})
</script>
