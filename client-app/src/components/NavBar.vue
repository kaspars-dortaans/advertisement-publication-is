<template>
  <MenuBar :model="items">
    <template #start>
      <div class="flex flex-row items-baseline gap-2 w-full">
        <RouterLink :to="{ name: 'home' }">
          <h3>Site title</h3>
        </RouterLink>
        <IconField class="flex-1">
          <InputIcon class="pi pi-search" />
          <InputText class="w-full" :placeholder="ls.l('actions.search')" size="small" />
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
import { reactive } from 'vue'

const ls = LocaleService.get()

const localeItems = ls.localeList.map((localeName) => ({
  label: localeName,
  command: () => ls.loadLocale(localeName)
}))

//TODO: filter items based on user permissions
const items = reactive([
  {
    label: 'navigation.advertisements',
    items: [
      {
        route: 'home',
        label: 'navigation.seeAdvertisements'
      },
      {
        route: 'home',
        label: 'navigation.savedAdvertisements'
      },
      {
        route: 'home',
        label: 'navigation.recenltyViewedAdvertisements'
      },
      {
        route: 'home',
        label: 'navigation.createAdvertisement'
      }
    ]
  },
  {
    label: ls.currentLocale,
    items: localeItems
  },
  {
    route: 'login',
    icon: 'pi pi-user'
  }
])
</script>
