<template>
  <ResponsiveLayout>
    <BlockWithSpinner :loading="loading" class="flex-1 lg:flex-none flex flex-col">
      <!-- Gallery, title, text, attributes, contacts -->
      <Panel
        :pt="panelPt"
        class="rounded-none lg:rounded-md lg:min-h-96 lg:min-w-96 flex flex-col flex-1"
      >
        <template #header>
          <div class="panel-title-container">
            <BackButton :defaultTo="{ name: 'viewAdvertisements' }" />
            <h3 class="page-title">{{ advertisement.title }}</h3>
            <Button
              v-if="isAllowedToBookmark"
              :icon="bookmarkIcon"
              :label="l.advertisements.bookmark"
              :loading="savingBookmark"
              :disabled="loading"
              severity="secondary"
              @click="bookmarkAdvertisement"
            />
            <Button
              v-if="isAllowedToEdit"
              icon="pi pi-pencil"
              :label="l.actions.edit"
              severity="secondary"
              as="RouterLink"
              :to="{ name: canEditAny ? 'editAnyAdvertisement' : 'editAdvertisement' }"
            />
          </div>
        </template>

        <div class="flex flex-col items-center gap-5 h-full">
          <Galleria
            v-if="advertisement.imageURLs?.length"
            :value="advertisement.imageURLs"
            :showThumbnails="true"
            :showItemNavigators="true"
            :circular="true"
            containerClass="w-full sm:w-full md:max-w-sm lg:max-w-xl flex-shrink-0 max-h-full flex"
          >
            <template #item="slotProps">
              <img :src="slotProps.item.url" class="object-contain max-w-full max-h-full" />
            </template>
            <template #thumbnail="slotProps">
              <img :src="slotProps.item.thumbnailUrl" />
            </template>
          </Galleria>

          <div class="overflow-y-auto h-full">
            <p class="whitespace-pre-line">{{ advertisement.advertisementText }}</p>
            <AttributeValuesList
              class="mt-3"
              :advertisementId="advertisement.id"
              :attributeValues="advertisement.attributes"
            />
          </div>
        </div>

        <template #footer>
          <h3 class="text-2xl mb-2">{{ l.advertisements.contacts }}</h3>
          <div
            class="flex flex-row flex-wrap gap-10 items-baseline justify-center xl:justify-between"
          >
            <div class="flex flex-wrap justify-center gap-2 basis-full md:basis-auto">
              <Button
                :label="l.advertisements.viewProfile"
                as="RouterLink"
                :to="{ name: 'viewUserProfile', params: { id: advertisement.ownerId } }"
              ></Button>
              <Button
                v-if="!userOwnedAdvertisement"
                as="RouterLink"
                :to="{
                  name: 'viewMessages',
                  query: {
                    newChatToUserId: advertisement.ownerId,
                    newChatToAdvertisementId: advertisement.id
                  }
                }"
                >{{ l.advertisements.sendMessage }}</Button
              >
            </div>

            <div
              v-if="advertisement.maskedAdvertiserEmail"
              class="flex flex-wrap justify-center gap-2 basis-full md:basis-auto"
            >
              <InputText v-model="advertisement.maskedAdvertiserEmail" disabled></InputText>
              <Button
                v-if="!revealedEmail"
                :loading="loadingEmail"
                :label="l.advertisements.showEmail"
                @click="revealEmail"
              />
            </div>

            <div
              v-if="advertisement.maskedAdvertiserPhoneNumber"
              class="flex flex-wrap justify-center gap-2 basis-full md:basis-auto"
            >
              <InputText v-model="advertisement.maskedAdvertiserPhoneNumber" disabled></InputText>
              <Button
                v-if="!revealedPhone"
                :loading="loadingPhoneNumber"
                :label="l.advertisements.showPhoneNumber"
                @click="revealPhoneNumber"
              />
            </div>

            <Button
              severity="danger"
              :disabled="userOwnedAdvertisement"
              :as="userOwnedAdvertisement ? 'button' : 'RouterLink'"
              :to="{ name: 'reportAdvertisement' }"
              >{{ l.advertisements.reportRuleViolation }}</Button
            >
          </div>
        </template>
      </Panel>
    </BlockWithSpinner>
  </ResponsiveLayout>
</template>

<script setup lang="ts">
import BackButton from '@/components/common/BackButton.vue'
import {
  AdvertisementHistoryStorageKey,
  AdvertisementHistoryTimeSpanInMiliSeconds
} from '@/constants/advertisement-history'
import {
  AdvertisementClient,
  AdvertisementDto,
  BookmarkAdvertisementRequest
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import type { IAdvertisementHistoryRecord } from '@/types/advertisements/advertisement-history-record'
import { getClient } from '@/utils/client-builder'
import { updateStorageObject } from '@/utils/local-storage'
import { computed, onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { AuthService } from '@/services/auth-service'
import BlockWithSpinner from '@/components/common/BlockWithSpinner.vue'
import ResponsiveLayout from '@/components/common/ResponsiveLayout.vue'
import AttributeValuesList from '@/components/advertisements/AttributeValuesList.vue'
import { Permissions } from '@/constants/api/Permissions'

//Props
const props = defineProps<{ id: number }>()

//Route
const { push } = useRouter()

//Services
const advertisementService = getClient(AdvertisementClient)
const l = LocaleService.currentLocale

//constants
const panelPt = {
  contentContainer: ['flex', 'flex-col', 'flex-1'],
  content: ['flex-1']
}

//Reactive data
const advertisement = ref<AdvertisementDto>(new AdvertisementDto())
const bookmarkIcon = computed(() => {
  return 'pi pi-bookmark' + (advertisement.value.isBookmarked ? '-fill' : '')
})
const loading = ref(false)
const loadingEmail = ref(false)
const loadingPhoneNumber = ref(false)
const savingBookmark = ref(false)
const revealedEmail = ref(false)
const revealedPhone = ref(false)
const userOwnedAdvertisement = computed(() => {
  return (
    typeof advertisement.value.ownerId === 'number' &&
    AuthService.isAuthenticated.value &&
    AuthService.profileInfo.value?.id === advertisement.value.ownerId
  )
})
const canManageAll = computed(() => AuthService.hasPermission(Permissions.ViewAllAdvertisements))
const canEditAny = computed(() => AuthService.hasPermission(Permissions.EditAnyAdvertisement))
const isAllowedToBookmark = computed(() =>
  AuthService.hasPermission(Permissions.BookmarkAdvertisement)
)
const isAllowedToEdit = computed(() => userOwnedAdvertisement.value || canEditAny.value)

//Hooks
onMounted(() => {
  if (typeof props.id !== 'number' || isNaN(props.id)) {
    push({ name: 'NotFound' })
    return
  }

  loadAdvertisement()

  //Add advertisement history record to local storage
  updateStorageObject<IAdvertisementHistoryRecord[]>(
    AdvertisementHistoryStorageKey,
    (historyRecords) => {
      //Delete old history
      const filtered = historyRecords.filter(
        (r) => Date.now() - r.timeStamp < AdvertisementHistoryTimeSpanInMiliSeconds
      )
      const existingRecord = filtered.find((r) => r.id === props.id)
      if (existingRecord) {
        existingRecord.timeStamp = Date.now()
      } else {
        filtered.push({
          id: props.id,
          timeStamp: Date.now()
        })
      }

      return filtered
    },
    []
  )
})

//Watchers
watch(LocaleService.currentLocaleName, () => {
  loadAdvertisement()
})

//Methods
const loadAdvertisement = async () => {
  loading.value = true
  if (canManageAll.value) {
    advertisement.value = await advertisementService.getAnyAdvertisement(props.id)
  } else {
    advertisement.value = await advertisementService.getAdvertisement(props.id)
  }
  loading.value = false
}

const revealPhoneNumber = async () => {
  loadingPhoneNumber.value = true
  advertisement.value.maskedAdvertiserPhoneNumber =
    await advertisementService.revealAdvertiserPhoneNumber(props.id)
  revealedPhone.value = true
  loadingPhoneNumber.value = false
}

const revealEmail = async () => {
  loadingEmail.value = true
  advertisement.value.maskedAdvertiserEmail = await advertisementService.revealAdvertiserEmail(
    props.id
  )
  revealedEmail.value = true
  loadingEmail.value = false
}

const bookmarkAdvertisement = async () => {
  savingBookmark.value = true
  await advertisementService.bookmarkAdvertisement(
    new BookmarkAdvertisementRequest({
      advertisementId: advertisement.value.id!,
      addBookmark: !advertisement.value.isBookmarked
    })
  )
  advertisement.value.isBookmarked = !advertisement.value.isBookmarked
  savingBookmark.value = false
}
</script>
