<template>
  <!-- Gallery, title, text, attributes, contacts -->
  <Panel :pt="panelPt">
    <template #header>
      <div class="flex flex-row items-center gap-5 w-full">
        <BackButton :defaultTo="{ name: 'home' }" />
        <h3 class="font-semibold text-2xl mr-auto">{{ advertisement.title }}</h3>
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
      <h3 class="font-semibold text-2xl mb-2">{{ l.advertisements.contacts }}</h3>
      <div class="flex flex-row flex-wrap gap-10 items-baseline justify-center xl:justify-between">
        <div class="flex flex-wrap justify-center gap-2 basis-full md:basis-auto">
          <Button @click="todo">{{ l.advertisements.viewProfile }}</Button>
          <Button @click="todo">{{ l.advertisements.sendMessage }}</Button>
        </div>

        <div
          v-if="advertisement.maskedAdvertiserEmail"
          class="flex flex-wrap justify-center gap-2 basis-full md:basis-auto"
        >
          <InputText v-model="advertisement.maskedAdvertiserEmail"></InputText>
          <Button
            :loading="loadingEmail"
            :label="l.advertisements.showPhoneNumber"
            @click="revealEmail"
          />
        </div>

        <div
          v-if="advertisement.maskedAdvertiserPhoneNumber"
          class="flex flex-wrap justify-center gap-2 basis-full md:basis-auto"
        >
          <InputText v-model="advertisement.maskedAdvertiserPhoneNumber"></InputText>
          <Button
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
  AdvertisementClient,
  AdvertisementDto,
  BookmarkAdvertisementRequest
} from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'
import { computed, onMounted, ref, watch, type Ref } from 'vue'
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
const advertisement: Ref<AdvertisementDto> = ref(new AdvertisementDto())
const bookmarkIcon = computed(() => {
  return 'pi pi-bookmark' + (advertisement.value.isBookmarked ? '-fill' : '')
})
const loadingEmail = ref(false)
const loadingPhoneNumber = ref(false)
const savingBookmark = ref(false)

//Hooks
onMounted(() => {
  if (typeof advertisementId !== 'number' || isNaN(advertisementId)) {
    push({ name: 'NotFound' })
    return
  }

  loadAdvertisement()
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
  loadingPhoneNumber.value = false
}

const revealEmail = async () => {
  loadingEmail.value = true
  advertisement.value.maskedAdvertiserEmail =
    await advertisementService.revealAdvertiserEmail(advertisementId)
  loadingEmail.value = false
}

const bookmarkAdvertisement = async () => {
  savingBookmark.value = true
  await advertisementService.bookmarkAdvertisement(
    new BookmarkAdvertisementRequest({
      advertisementId: advertisement.value.id,
      addBookmark: !advertisement.value.isBookmarked
    })
  )
  advertisement.value.isBookmarked = !advertisement.value.isBookmarked
  savingBookmark.value = false
}
</script>
