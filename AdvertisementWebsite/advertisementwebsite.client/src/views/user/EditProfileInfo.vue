<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading" class="flex-1 lg:flex-none flex flex-col">
      <Panel class="rounded-none lg:rounded-md flex-1">
        <template #header>
          <div class="panel-title-container">
            <BackButton :default-to="{ name: 'profileInfo' }" />
            <h3 class="page-title">{{ l.navigation.editProfileInfo }}</h3>
          </div>
        </template>
        <form class="flex flex-col gap-3 md:items-center bg-white" @submit="onSubmit">
          <EditUserCommonInputs
            :fields="fields"
            :hasFormErrors="hasFormErrors"
            :formErrors="formErrors"
          />

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
import { EditUserInfo, UserClient, type FileParameter } from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper } from '@/utils/field-helper'
import { downloadFile, hashFile } from '@/utils/file-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { onBeforeMount, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { boolean, object, string } from 'yup'
import EditUserCommonInputs from '@/components/form/user/EditUserCommonInputs.vue'

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
const form = useForm<EditUserInfo>({
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
const loading = ref(false)

//Hooks
onBeforeMount(async () => {
  loading.value = true
  await authService.refreshProfileData()
  const userInfo = (await AuthService.profileInfoPromise.value)!

  let profileImage
  if (userInfo.profileImage?.imageURLs?.url) {
    profileImage = await downloadFile(userInfo.profileImage.imageURLs.url)
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
