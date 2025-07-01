<template>
  <ResponsivePanel
    :defaultBackButtonRoute="{ name: 'home' }"
    :title="userInfo?.userName"
    :loading="loadingInfo"
    panelClass="lg:!min-w-52"
    class="!flex-grow-0"
  >
    <img :src="userInfo?.profileImageUrl" class="max-w-52 max-h-52 mx-auto rounded-md" />

    <template v-if="hasContacts">
      <h3 class="text-2xl mb-2 mt-3">
        {{ l.advertisements.contacts }}
      </h3>

      <div class="flex flex-col gap-4">
        <InputGroup v-if="userInfo?.email">
          <InputGroupAddon>
            <i class="pi pi-at"></i>
          </InputGroupAddon>
          <InputText v-model="userInfo.email" disabled />
        </InputGroup>

        <InputGroup v-if="userInfo?.phoneNumber">
          <InputGroupAddon>
            <i class="pi pi-phone"></i>
          </InputGroupAddon>
          <InputText v-model="userInfo.phoneNumber" disabled />
        </InputGroup>

        <InputGroup v-if="userInfo?.linkToUserSite">
          <InputGroupAddon>
            <i class="pi pi-link"></i>
          </InputGroupAddon>
          <Button
            :label="userInfo.linkToUserSite"
            as="a"
            :href="userInfo.linkToUserSite"
            class="flex-grow justify-start"
          ></Button>
        </InputGroup>
      </div>
    </template>
  </ResponsivePanel>

  <AdvertisementTable
    :advertisementSource="loadUserAdvertisements"
    :categoryNameSource="getCategoryName"
    :title="ls.l('advertisements.userAdvertisements', userInfo?.userName ?? '')"
  ></AdvertisementTable>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/advertisements/AdvertisementTable.vue'
import ResponsivePanel from '@/components/common/ResponsivePanel.vue'
import {
  AdvertisementClient,
  AdvertisementQuery,
  PublicUserInfoDto,
  UserClient
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

//Props
const props = defineProps<{ id: number }>()

//Route
const { push } = useRouter()

//Services
const advertisementService = getClient(AdvertisementClient)
const userService = getClient(UserClient)
const l = LocaleService.currentLocale
const ls = LocaleService.get()

//Reactive data
const userInfo = ref<PublicUserInfoDto | undefined>()
const loadingInfo = ref(false)
const hasContacts = computed(
  () =>
    userInfo.value &&
    (userInfo.value.email || userInfo.value.phoneNumber || userInfo.value.linkToUserSite)
)

onMounted(() => {
  if (typeof props.id !== 'number') {
    push({ name: 'NotFound' })
  }

  loadUserInfo()
})

const loadUserAdvertisements = async (query: AdvertisementQuery) => {
  query.advertisementOwnerId = props.id
  return await advertisementService.getAdvertisements(query)
}

const loadUserInfo = async () => {
  loadingInfo.value = true
  userInfo.value = await userService.getPublicUserInfo(props.id)
  loadingInfo.value = false
}

const getCategoryName = () => {
  return Promise.resolve(l.value.categoryMenu.new) as Promise<string>
}
</script>
