<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading" class="flex flex-col flex-1 lg:flex-none">
      <Panel class="rounded-none lg:rounded-md flex-1">
        <template #header>
          <div class="panel-title-container">
            <BackButton :default-to="{ name: 'manageUsers' }" />
            <h3 class="page-title">{{ l.navigation.viewUser }}</h3>
            <Button
              v-if="isAllowedToEdit"
              :label="l.actions.edit"
              icon="pi pi-pencil"
              severity="secondary"
              as="RouterLink"
              :to="{ name: 'editUser', params: { userId } }"
            />
          </div>
        </template>
        <div class="flex flex-col gap-5 items-center lg:flex-row">
          <div class="flex flex-col gap-2 min-w-full md:min-w-80">
            <FloatLabel variant="on">
              <InputText v-model="userInfo.firstName" id="first-name-input" fluid disabled />
              <label for="first-name-input">{{ l.form.register.firstName }}</label>
            </FloatLabel>

            <FloatLabel variant="on">
              <InputText v-model="userInfo.lastName" id="last-name-input" fluid disabled />
              <label for="last-name-input">{{ l.form.register.lastName }}</label>
            </FloatLabel>

            <FloatLabel variant="on">
              <InputText v-model="userInfo.userName" id="user-name-input" fluid disabled />
              <label for="user-name-input">{{ l.form.register.username }}</label>
            </FloatLabel>

            <FloatLabel variant="on">
              <InputText v-model="userInfo.email" id="email-input" fluid disabled />
              <label for="email-input">{{ l.form.register.email }}</label>
            </FloatLabel>

            <div class="flex items-center">
              <Checkbox
                v-model="userInfo.isEmailPublic"
                :binary="true"
                inputId="register.isEmailPublic"
                fluid
                disabled
              />
              <label class="ml-2" for="register.isEmailPublic">{{
                l.form.register.publiclyDisplayEmail
              }}</label>
            </div>

            <FloatLabel variant="on">
              <InputText v-model="userInfo.phoneNumber" id="phone-number-input" fluid disabled />
              <label for="phone-number-input">{{ l.form.register.phoneNumber }}</label>
            </FloatLabel>

            <div class="flex items-center">
              <Checkbox
                v-model="userInfo.isPhoneNumberPublic"
                :binary="true"
                inputId="register-isPhonePublic"
                fluid
                disabled
              />
              <label class="ml-2" for="register-isPhonePublic">{{
                l.form.register.publiclyDisplayPhoneNumber
              }}</label>
            </div>

            <FloatLabel variant="on">
              <InputText
                v-model="userInfo.linkToUserSite"
                id="link-to-user-site-input"
                fluid
                disabled
              />
              <label for="link-to-user-site-input">{{ l.form.register.linkToUserSite }}</label>
            </FloatLabel>
          </div>

          <img :src="userInfo.profileImage?.imageURLs?.url ?? DefaultImage" class="w-96 h-96" />
        </div>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import { UserClient, UserInfo } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { computed, onBeforeMount, ref } from 'vue'
import DefaultImage from '@/assets/images/default-profile-image.svg'
import { AuthService } from '@/services/auth-service'
import { Permissions } from '@/constants/api/Permissions'

const props = defineProps<{
  userId: number
}>()

//Services
const l = LocaleService.currentLocale
const userService = getClient(UserClient)

//Reactive data
const userInfo = ref<UserInfo>(new UserInfo())
const loading = ref(false)
const isAllowedToEdit = computed(() => AuthService.hasPermission(Permissions.EditAnyUser))

//Hooks
onBeforeMount(async () => {
  loading.value = true
  userInfo.value = await userService.getUserFormInfo(props.userId)
  loading.value = false
})
</script>
