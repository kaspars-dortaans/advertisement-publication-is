<template>
  <MenuBar :model="items">
    <template #start>
      <RouterLink :to="{ name: 'home' }">
        <h3>Site title</h3>
      </RouterLink>
    </template>
    <template #item="{ item, props, hasSubmenu }">
      <router-link v-if="item.route" v-slot="{ href, navigate }" :to="{ name: item.route }" custom>
        <a :href="href" v-bind="props.action" @click="navigate">
          <span :class="item.icon"></span>
          <span class="ml-2">{{ ls.l(item.label) }}</span>
        </a>
      </router-link>
      <a v-else :href="item.url" :target="item.target" v-bind="props.action">
        <span :class="item.icon"></span>
        <span class="ml-2">{{ ls.l(item.label) }}</span>
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
    label: 'navigation.login'
  }
])
</script>
