<template>
  <div class="flex flex-row justify-center items-center gap-3 h-full bg-primary p-2">
    <Panel>
      <CategoryMenu @category-selected="loadAdvertisements" />
    </Panel>

    <Panel> </Panel>
  </div>
</template>

<script setup lang="ts">
import CategoryMenu from '@/components/CategoryMenu.vue'
import { AdvertisementClient } from '@/services/api-client'
import { LocaleService } from '@/services/locale-service'
import { getClient } from '@/utils/client-builder'

const advertisementService = getClient(AdvertisementClient)
const ls = LocaleService.get()

const loadAdvertisements = () => {
  // TODO: fetch from API
}
</script>
