<template>
  <ResponsivePanel
    :defaultBackButtonRoute="{ name: 'home' }"
    :title="l.navigation.profileInfo"
    :loading="loading"
  >
    <div class="flex flex-col md:flex-row items-center justify-center gap-7">
      <dl class="grid grid-cols-[auto_auto] gap-x-3 gap-y-2">
        <dt>{{ l.form.profileInfo.firstName }}</dt>
        <dd>{{ profileInfo?.firstName }}</dd>

        <dt>{{ l.form.profileInfo.lastName }}</dt>
        <dd>{{ profileInfo?.lastName }}</dd>

        <dt>{{ l.form.profileInfo.userName }}</dt>
        <dd>{{ profileInfo?.userName }}</dd>

        <dt>{{ l.form.profileInfo.email }}</dt>
        <dd>{{ profileInfo?.email }}</dd>

        <dt>{{ l.form.profileInfo.publiclyDisplayEmail }}</dt>
        <dd>{{ ls.l(profileInfo?.isEmailPublic?.toString()) }}</dd>

        <dt>{{ l.form.profileInfo.phoneNumber }}</dt>
        <dd>{{ profileInfo?.phoneNumber }}</dd>

        <dt>{{ l.form.profileInfo.publiclyDisplayPhoneNumber }}</dt>
        <dd>{{ ls.l(profileInfo?.isPhoneNumberPublic?.toString()) }}</dd>

        <dt>{{ l.form.profileInfo.linkToUserSite }}</dt>
        <dd>{{ profileInfo?.linkToUserSite }}</dd>
      </dl>

      <img
        :src="profileInfo?.profileImage?.imageURLs?.url ?? defaultProfileImageUrl"
        class="w-56 h-56 rounded-md"
      />
    </div>

    <div class="flex gap-2 justify-center mt-5">
      <Button
        :label="l.actions.edit"
        as="RouterLink"
        :to="{ name: 'editProfileInfo' }"
        class="flex-auto lg:flex-none"
      />
      <Button
        :label="l.navigation.changePassword"
        as="RouterLink"
        :to="{ name: 'changePassword' }"
        class="flex-auto lg:flex-none"
      />
    </div>
  </ResponsivePanel>
</template>

<script setup lang="ts">
import defaultProfileImageUrl from '@/assets/images/default-profile-image.svg'
import ResponsivePanel from '@/components/common/ResponsivePanel.vue'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import { onBeforeMount, ref } from 'vue'

//Services
const l = LocaleService.currentLocale
const ls = LocaleService.get()
const authService = AuthService.get()

//Reactive data
const loading = ref(false)
const profileInfo = AuthService.profileInfo

//Hooks
onBeforeMount(async () => {
  loading.value = true
  await authService.refreshProfileData()
  loading.value = false
})
</script>
