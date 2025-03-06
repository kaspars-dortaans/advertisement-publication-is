<template>
  <div
    class="flex-1 flex flex-col justify-center items-center bg-primary overflow-y-auto lg:flex-row lg:gap-3 lg:p-2"
  >
    <BlockUI
      :blocked="loadingInfo"
      class="flex-grow basis-auto min-w-60 w-full lg:max-w-60 xl:max-w-80"
    >
      <ComponentSpinner :show="loadingInfo" />

      <Panel class="bg-white rounded-none lg:rounded-md">
        <template #header>
          <div class="flex flex-row gap-5">
            <BackButton :defaultTo="{ name: 'home' }"></BackButton>
            <h3 class="font-semibold text-2xl">{{ userInfo?.userName }}</h3>
          </div>
        </template>

        <img :src="userInfo?.profileImageUrl" class="max-w-52 max-h-52 mx-auto" />

        <h3 class="font-semibold text-2xl my-2">{{ l.advertisements.contacts }}</h3>

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
            ></Button>
          </InputGroup>
        </div>
      </Panel>
    </BlockUI>

    <AdvertisementTable
      :advertisementSource="loadUserAdvertisements"
      :categoryNameSource="getCategoryName"
      class="flex-grow flex-shrink basis-full rounded-none lg:rounded-md"
    ></AdvertisementTable>
  </div>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/AdvertisementTable.vue'
import BackButton from '@/components/BackButton.vue'
import ComponentSpinner from '@/components/Common/ComponentSpinner.vue'
import {
  AdvertisementClient,
  AdvertisementQuery,
  PublicUserInfoDto,
  UserClient
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { onMounted, ref, type Ref } from 'vue'
import { useRouter } from 'vue-router'

//Props
const { id: userId } = defineProps<{ id: number }>()

//Route
const { push } = useRouter()

//Services
const advertisementService = getClient(AdvertisementClient)
const userService = getClient(UserClient)
const l = LocaleService.currentLocale

//Reactive data
const userInfo: Ref<PublicUserInfoDto | undefined> = ref()
const loadingInfo = ref(false)

onMounted(() => {
  if (typeof userId !== 'number') {
    push({ name: 'NotFound' })
  }

  loadUserInfo()
})

const loadUserAdvertisements = async (query: AdvertisementQuery) => {
  query.advertisementOwnerId = userId
  await wait(1000)
  return await advertisementService.getAdvertisements(query)
}

const loadUserInfo = async () => {
  loadingInfo.value = true
  await wait(1000)
  userInfo.value = await userService.getPublicUserInfo(userId)
  loadingInfo.value = false
}

const getCategoryName = () => {
  return Promise.resolve(l.value.categoryMenu.new) as Promise<string>
}

const wait = async (waitTime: number) => {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve(null)
    }, waitTime)
  })
}
</script>
