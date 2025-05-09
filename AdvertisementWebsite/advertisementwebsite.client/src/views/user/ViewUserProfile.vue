<template>
  <ResponsiveLayout>
    <BlockWithSpinner
      :loading="loadingInfo"
      class="flex-none min-w-60 w-full lg:max-w-60 xl:max-w-80"
    >
      <Panel class="bg-white rounded-none lg:rounded-md">
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'home' }"></BackButton>
            <h3 class="page-title">{{ userInfo?.userName }}</h3>
          </div>
        </template>

        <img :src="userInfo?.profileImageUrl" class="max-w-52 max-h-52 mx-auto rounded-md" />

        <h3 class="text-2xl my-2">{{ l.advertisements.contacts }}</h3>

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
    </BlockWithSpinner>

    <AdvertisementTable
      :advertisementSource="loadUserAdvertisements"
      :categoryNameSource="getCategoryName"
    ></AdvertisementTable>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import AdvertisementTable from '@/components/advertisements/AdvertisementTable.vue'
import BackButton from '@/components/common/BackButton.vue'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import {
  AdvertisementClient,
  AdvertisementQuery,
  PublicUserInfoDto,
  UserClient
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'

//Props
const props = defineProps<{ id: number }>()

//Route
const { push } = useRouter()

//Services
const advertisementService = getClient(AdvertisementClient)
const userService = getClient(UserClient)
const l = LocaleService.currentLocale

//Reactive data
const userInfo = ref<PublicUserInfoDto | undefined>()
const loadingInfo = ref(false)

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
