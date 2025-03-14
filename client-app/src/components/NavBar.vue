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
      <Avatar v-else-if="item.avatar" :image="item.url" shape="circle" />
      <a v-else :href="item.url" :target="item.target" v-bind="props.action">
        <i v-if="item.icon" class="mr-2" :class="item.icon" />
        <span v-if="item.label">{{ ls.l(item.label) }}</span>
        <span v-if="hasSubmenu" class="pi pi-fw pi-angle-down ml-2"></span>
      </a>
    </template>
  </MenuBar>
</template>

<script setup lang="ts">
import defaultProfileImageUrl from '@/assets/images/default-profile-image.svg'
import logoUrl from '@/assets/images/logo.svg'
import { UserInfo } from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import type { INavbarItem } from '@/types/navbar/navbar-item'
import { debounceFn } from '@/utils/debounce'
import { computed, ref, watch, type ComputedRef, type Ref } from 'vue'
import { useRouter, type RouteRecordNormalized } from 'vue-router'

const { push, getRoutes } = useRouter()

//Services
const ls = LocaleService.get()
const l = LocaleService.currentLocale

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

//Reactive data
const profileInfo: Ref<UserInfo | null> = ref(null)
const searchInput = ref('')

/** All navigation items */
const navbarItems: ComputedRef<INavbarItem[]> = computed(() => [
  ...allowedRouteItems.value,
  {
    label: LocaleService.currentLocaleName.value,
    items: localeItems.value
  },
  ...profileRoutes.value
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

const profileRoutes = computed(() => {
  if (!AuthService.isAuthenticated.value) {
    return [
      {
        route: 'login',
        icon: 'pi pi-user'
      }
    ] as INavbarItem[]
  }

  return [
    {
      avatar: true,
      url: profileInfo.value?.profileImageUrl ?? defaultProfileImageUrl,
      items: [
        {
          label: 'navigation.profileInfo',
          route: 'profileInfo'
        },
        {
          label: 'navigation.logout',
          route: 'logout'
        }
      ]
    }
  ] as INavbarItem[]
})

//watchers
watch(
  AuthService.profileInfo,
  async (newValue) => {
    profileInfo.value = await newValue
  },
  { immediate: true }
)

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
      const requiresPermission = route?.meta.requiresPermission
      const hasPermission =
        typeof requiresPermission === 'string'
          ? AuthService.hasPermission(requiresPermission)
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
