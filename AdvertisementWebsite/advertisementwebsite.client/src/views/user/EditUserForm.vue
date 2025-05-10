<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading" class="flex-1 lg:flex-none flex flex-col">
      <Panel class="rounded-none lg:rounded-md flex-1">
        <template #header>
          <div class="panel-title-container">
            <BackButton :default-to="{ name: 'manageUsers' }" />
            <h3 class="page-title">{{ l.navigation.editProfileInfo }}</h3>
          </div>
        </template>
        <form class="flex flex-col gap-3 md:items-center bg-white" @submit="onSubmit">
          <EditUserCommonInputs
            :fields="fields as Fields<EditUserInfo>"
            :hasFormErrors="hasFormErrors"
            :formErrors="formErrors"
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
              <label for="user-role-input">{{ l.form.profileInfo.userRoles }}</label>
            </FloatLabel>
            <FieldError :field="fields.userRoles" />
          </EditUserCommonInputs>

          <Button
            type="submit"
            :label="l.actions.save"
            :loading="isSubmitting"
            class="self-stretch lg:self-center"
          />
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
import EditUserCommonInputs from '@/components/form/user/EditUserCommonInputs.vue'
import {
  EditUserInfo,
  EditUserRequest,
  ImageDto,
  UserClient,
  type FileParameter
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { leaveFormGuard } from '@/utils/confirm-dialog'
import { FieldHelper, type Fields } from '@/utils/field-helper'
import { downloadFile, hashFile } from '@/utils/file-helper'
import { toTypedSchema } from '@vee-validate/yup'
import { useConfirm } from 'primevue'
import { useForm } from 'vee-validate'
import { onBeforeMount, ref, watch } from 'vue'
import { onBeforeRouteLeave, useRouter } from 'vue-router'
import { array, boolean, object, string } from 'yup'

const props = defineProps<{
  userId: number
}>()

//Route
const { push } = useRouter()
const formSubmitted = ref(false)
const confirm = useConfirm()
onBeforeRouteLeave((_to, _from, next) =>
  leaveFormGuard(confirm, formSubmitted, valuesChanged, next)
)

//Services
const l = LocaleService.currentLocale
const userService = getClient(UserClient)

//Reactive data
const roleList = ref<string[]>([])

//Forms and fields
const form = useForm<EditUserRequest>({
  validationSchema: toTypedSchema(
    object({
      userRoles: array().required().default([]).label('form.profileInfo.userRoles'),
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
  'userRoles',
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
const originalProfileImage = ref<ImageDto | undefined>()
const loading = ref(false)

//Hooks
onBeforeMount(async () => {
  loading.value = true
  const [userInfo, roles] = await Promise.all([
    userService.getUserFormInfo(props.userId),
    userService.getRoleList()
  ])
  roleList.value = roles
  originalProfileImage.value = userInfo.profileImage

  let profileImage
  if (userInfo.profileImage?.imageURLs?.url) {
    profileImage = await downloadFile(userInfo.profileImage.imageURLs.url)
  }

  resetForm({
    values: {
      userRoles: userInfo.userRoles!,
      userName: userInfo.userName!,
      firstName: userInfo.firstName!,
      lastName: userInfo.lastName!,
      email: userInfo.email!,
      isEmailPublic: userInfo.isEmailPublic,
      phoneNumber: userInfo.phoneNumber!,
      isPhoneNumberPublic: userInfo.isPhoneNumberPublic,
      linkToUserSite: userInfo.linkToUserSite,
      profileImage: profileImage
    }
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
    const profileImageChanged = fileHash !== originalProfileImage.value?.hash
    const profileImage =
      profileImageChanged && values.profileImage
        ? ({
            data: values.profileImage,
            fileName: (values.profileImage as File).name
          } as FileParameter)
        : undefined

    await userService.editUser(
      values.email,
      values.isEmailPublic,
      values.firstName,
      values.lastName,
      values.userName,
      values.phoneNumber,
      values.isPhoneNumberPublic,
      values.linkToUserSite,
      profileImageChanged,
      profileImage,
      props.userId,
      values.userRoles
    )
    formSubmitted.value = true
    push({ name: 'manageUsers' })
  } catch (error) {
    handleErrors(error)
  }
})
</script>
