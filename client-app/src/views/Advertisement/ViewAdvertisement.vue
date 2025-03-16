<template>
  <!-- Gallery, title, text, attributes, contacts -->
  <Panel :pt="panelPt">
    <template #header>
      <div class="panel-title-container">
        <BackButton :defaultTo="{ name: 'home' }" />
        <h3 class="page-title">{{ advertisement.title }}</h3>
        <Button
          :icon="bookmarkIcon"
          :label="l.advertisements.bookmark"
          :loading="savingBookmark"
          severity="secondary"
          @click="bookmarkAdvertisement"
        />
      </div>
    </template>

    <div class="flex flex-row gap-5 flex-wrap md:flex-nowrap h-full items-center">
      <Galleria
        v-if="advertisement.imageURLs?.length"
        :value="advertisement.imageURLs"
        :showThumbnails="false"
        :showIndicators="true"
        :showItemNavigators="true"
        :circular="true"
        containerClass="w-full sm:w-full md:max-w-sm lg:max-w-xl flex-shrink-0 max-h-full flex"
      >
        <template #item="slotProps">
          <img :src="slotProps.item.url" class="object-contain max-w-full max-h-full" />
        </template>
      </Galleria>
      <div class="overflow-y-auto h-full">
        <p class="whitespace-pre-line">{{ advertisement.advertisementText }}</p>
        <div class="flex flex-row flex-wrap gap-2">
          <span
            v-for="attribute in advertisement.attributes"
            :key="attribute.attributeId"
            :title="attribute.attributeName"
            >{{ attribute.valueName ?? attribute.value }}</span
          >
        </div>
      </div>
    </div>

    <template #footer>
      <h3 class="text-2xl mb-2">{{ l.advertisements.contacts }}</h3>
      <div class="flex flex-row flex-wrap gap-10 items-baseline justify-center xl:justify-between">
        <div class="flex flex-wrap justify-center gap-2 basis-full md:basis-auto">
          <Button
            :label="l.advertisements.viewProfile"
            as="RouterLink"
            :to="{ name: 'viewUser', params: { id: advertisement.ownerId } }"
          ></Button>
          <Button @click="todo">{{ l.advertisements.sendMessage }}</Button>
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

        <Button severity="danger" as="RouterLink" :to="{ name: 'reportAdvertisement' }">{{
          l.advertisements.reportRuleViolation
        }}</Button>
      </div>
    </template>
  </Panel>
</template>

<script setup lang="ts">
import BackButton from '@/components/BackButton.vue'
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

//Props
const { id: advertisementId } = defineProps<{ id: number }>()

//Route
const { push } = useRouter()

//Services
const advertisementService = getClient(AdvertisementClient)
const l = LocaleService.currentLocale

//constants
const panelPt = {
  root: ['flex', 'flex-col', 'flex-1', 'md:min-h-0'],
  contentContainer: ['flex', 'flex-col', 'flex-1', 'md:min-h-0'],
  content: ['flex-1', 'md:min-h-0']
}

//Reactive data
const advertisement = ref<AdvertisementDto>(new AdvertisementDto())
const bookmarkIcon = computed(() => {
  return 'pi pi-bookmark' + (advertisement.value.isBookmarked ? '-fill' : '')
})
const loadingEmail = ref(false)
const loadingPhoneNumber = ref(false)
const savingBookmark = ref(false)
const revealedEmail = ref(false)
const revealedPhone = ref(false)

//Hooks
onMounted(() => {
  if (typeof advertisementId !== 'number' || isNaN(advertisementId)) {
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
      const existingRecord = filtered.find((r) => r.id === advertisementId)
      if (existingRecord) {
        existingRecord.timeStamp = Date.now()
      } else {
        filtered.push({
          id: advertisementId,
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
  advertisement.value = await advertisementService.getAdvertisement(advertisementId)
}

const todo = () => {
  alert('todo')
}

const revealPhoneNumber = async () => {
  loadingPhoneNumber.value = true
  advertisement.value.maskedAdvertiserPhoneNumber =
    await advertisementService.revealAdvertiserPhoneNumber(advertisementId)
  revealedPhone.value = true
  loadingPhoneNumber.value = false
}

const revealEmail = async () => {
  loadingEmail.value = true
  advertisement.value.maskedAdvertiserEmail =
    await advertisementService.revealAdvertiserEmail(advertisementId)
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
