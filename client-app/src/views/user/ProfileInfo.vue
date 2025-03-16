<template>
  <ResponsiveLayout>
    <Panel class="flex-1 lg:flex-none">
      <template #header>
        <div class="panel-title-container">
          <BackButton :defaultTo="{ name: 'home' }"></BackButton>
          <h3 class="page-title">{{ l.navigation.profileInfo }}</h3>
        </div>
      </template>

      <div class="flex flex-col md:flex-row items-center justify-center gap-7">
        <dl class="grid grid-cols-[auto_auto] gap-x-3 gap-y-2">
          <dt>{{ l.form.profileInfo.firstName }}</dt>
          <dd>{{ profileInfo.firstName }}</dd>

          <dt>{{ l.form.profileInfo.lastName }}</dt>
          <dd>{{ profileInfo.lastName }}</dd>

          <dt>{{ l.form.profileInfo.userName }}</dt>
          <dd>{{ profileInfo.userName }}</dd>

          <dt>{{ l.form.profileInfo.email }}</dt>
          <dd>{{ profileInfo.email }}</dd>

          <dt>{{ l.form.profileInfo.publiclyDisplayEmail }}</dt>
          <dd>{{ ls.l(profileInfo.isEmailPublic?.toString()) }}</dd>

          <dt>{{ l.form.profileInfo.phoneNumber }}</dt>
          <dd>{{ profileInfo.phoneNumber }}</dd>

          <dt>{{ l.form.profileInfo.publiclyDisplayPhoneNumber }}</dt>
          <dd>{{ ls.l(profileInfo.isPhoneNumberPublic?.toString()) }}</dd>
        </dl>

        <img :src="profileInfo.profileImageUrl?.url ?? defaultProfileImageUrl" class="w-56 h-56" />
      </div>

      <div class="flex gap-2 justify-center mt-5">
        <Button
          :label="l.actions.edit"
          as="RouterLink"
          :to="{ name: 'editProfileInfo' }"
          class="flex-auto lg:flex-none"
        />
        <Button
          :label="l.form.profileInfo.changePassword"
          as="RouterLink"
          :to="{ name: 'changePassword' }"
          class="flex-auto lg:flex-none"
        />
      </div>
    </Panel>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import defaultProfileImageUrl from '@/assets/images/default-profile-image.svg'
import BackButton from '@/components/BackButton.vue'
import ResponsiveLayout from '@/components/Common/ResponsiveLayout.vue'
import { UserInfo } from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { onBeforeMount, ref, watch } from 'vue'

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const authService = AuthService.get()

//Reactive data
const profileInfo = ref<UserInfo>(new UserInfo())

//Hooks
onBeforeMount(async () => {
  await authService.refreshProfileData()
})

//watch
watch(AuthService.profileInfo, async (newProfileInfo) => {
  profileInfo.value = (await newProfileInfo) ?? new UserInfo()
})
</script>
