<template>
  <MenuBar :model="navbarItems">
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
import { debounceFn } from '@/utils/debounce'
import { computed, ref, type ComputedRef } from 'vue'

import logoUrl from '@/assets/logo.svg'
import { AuthService } from '@/services/auth-service'
import type { INavbarItem } from '@/types/navbar/navbar-item'
import { useRouter, type RouteRecordNormalized } from 'vue-router'

const { push, getRoutes } = useRouter()

//Services
const ls = LocaleService.get()
const l = LocaleService.currentLocale

//Reactive data
const searchInput = ref('')

//Reactive data
/** All navigation items */
const navbarItems: ComputedRef<INavbarItem[]> = computed(() => [
  ...allowedRouteItems.value,
  {
    label: LocaleService.currentLocaleName.value,
    items: localeItems.value
  },
  {
    route: 'login',
    icon: 'pi pi-user'
  }
])

/** Language select items */
const localeItems = computed(() =>
  ls.localeList.value.map((localeName) => ({
    label: localeName,
    command: () => {
      ls.loadLocale(localeName)
    }
  }))
)

/** Navigation routes allowed for current user */
const allowedRouteItems: ComputedRef<INavbarItem[]> = computed(() => {
  const routes = getRoutes()
  return filterRoutes(allRouteItems, routes)
})

//Constants
/** All navigation routes */
const allRouteItems: INavbarItem[] = [
  {
    label: 'navigation.advertisements',
    items: [
      {
        route: 'home',
        label: 'navigation.seeAdvertisements'
      },
      {
        route: 'recentlyViewedAdvertisements',
        label: 'navigation.recentlyViewedAdvertisements'
      },
      {
        route: 'bookmarkedAdvertisements',
        label: 'navigation.savedAdvertisements'
      }
      //TODO: Uncomment when view is completed
      // {
      //   route: 'createAdvertisement',
      //   label: 'navigation.createAdvertisement'
      // }
    ]
  }
]

//Methods
const search = () => {
  push({ name: 'searchAdvertisements', query: { search: searchInput.value } })
  searchInput.value = ''
}
const { debounce: debouncedSearch, clear: clearDebounceSearch } = debounceFn(search)

const immediateSearch = () => {
  clearDebounceSearch()
  search()
}

/** Filter route items for which current user does not have a permission to navigate */
const filterRoutes = (items: INavbarItem[], routes: RouteRecordNormalized[]): INavbarItem[] => {
  return items
    .map((i) => {
      const route = routes.find((r) => r.name === i.route)
      const requiredPermission = route?.meta.requiredPermission
      const hasPermission =
        typeof requiredPermission === 'string'
          ? AuthService.hasPermission(requiredPermission)
          : true

      if (hasPermission) {
        return {
          label: i.label,
          route: i.route,
          icon: i.icon,
          items: i.items?.length ? filterRoutes(i.items, routes) : i.items
        } as INavbarItem
      } else {
        return null
      }
    })
    .filter((i) => i != null)
}
</script>
