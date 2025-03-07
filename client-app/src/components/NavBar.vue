<template>
  <MenuBar :model="items">
    <template #start>
      <div class="flex flex-row items-center gap-2 w-full">
        <RouterLink :to="{ name: 'home' }">
          <img class="h-9" :src="logoUrl" :alt="l.navigation.siteLogo" />
        </RouterLink>
        <IconField class="flex-1">
          <InputIcon class="pi pi-search" @click="immediateSearch" />
          <InputText
            v-model="searchInput"
            class="w-full"
            :placeholder="l.actions.search"
            size="small"
            @input="debouncedSearch"
            @keydown.enter="immediateSearch"
          />
        </IconField>
      </div>
    </template>
    <template #item="{ item, props, hasSubmenu }">
      <router-link v-if="item.route" v-slot="{ href, navigate }" :to="{ name: item.route }" custom>
        <a :href="href" v-bind="props.action" @click="navigate">
          <i v-if="item.icon" class="mr-2" :class="item.icon" />
          <span v-if="item.label">{{ ls.l(item.label) }}</span>
        </a>
      </router-link>
      <a v-else :href="item.url" :target="item.target" v-bind="props.action">
        <i v-if="item.icon" class="mr-2" :class="item.icon" />
        <span v-if="item.label">{{ ls.l(item.label) }}</span>
        <span v-if="hasSubmenu" class="pi pi-fw pi-angle-down ml-2"></span>
      </a>
    </template>
  </MenuBar>
</template>

<script setup lang="ts">
import { LocaleService } from '@/services/locale-service'
import { reactive, ref } from 'vue'
import { debounceFn } from '@/utils/debounce'

import logoUrl from '@/assets/logo.svg'

const ls = LocaleService.get()
const l = LocaleService.currentLocale

// TODO: Move to constant file?
const inputDebounceTime = 3000

const searchInput = ref('')

const localeItems = ls.localeList.value.map((localeName) => ({
  label: localeName,
  command: () => {
    ls.loadLocale(localeName)
  }
}))

// TODO: filter items based on user permissions
const items = reactive([
  {
    label: 'navigation.advertisements',
    items: [
      {
        route: 'home',
        label: 'navigation.seeAdvertisements'
      },
      {
        route: 'home', //TODO: Change when view is completed
        label: 'navigation.savedAdvertisements'
      },
      {
        route: 'recentlyViewedAdvertisements',
        label: 'navigation.recentlyViewedAdvertisements'
      },
      {
        route: 'home', //TODO: Change when view is completed
        label: 'navigation.createAdvertisement'
      }
    ]
  },
  {
    label: LocaleService.currentLocaleName,
    items: localeItems
  },
  {
    route: 'login',
    icon: 'pi pi-user'
  }
])

const search = () => {
  // TODO: Navigate when view is completed
  alert('Not implemented')
}

const immediateSearch = () => {
  clearDebounceSearch()
  search()
}

const { debounce: debouncedSearch, clear: clearDebounceSearch } = debounceFn(
  search,
  inputDebounceTime
)
</script>
