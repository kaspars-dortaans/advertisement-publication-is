<template>
  <MenuBar :model="navbarItems" class="rounded-none">
    <template #start>
      <div class="flex flex-row items-center gap-2 w-full">
        <RouterLink :to="{ name: 'viewAdvertisements' }">
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
          <i v-if="item.icon" :class="item.icon" />
          <span v-if="item.label">{{ ls.l(item.label) }}</span>
          <Badge v-if="item.badgeValue?.value" :value="item.badgeValue.value" class="rounded-xl" />
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
import { NewMessageTimeout } from '@/constants/message'
import { MessageClient, UserClient } from '@/services/api-client'
import { AuthService } from '@/services/auth-service'
import { LocaleService } from '@/services/locale-service'
import type { MessageHub } from '@/services/message-hub'
import type { INavbarItem } from '@/types/navbar/navbar-item'
import { getClient } from '@/utils/client-builder'
import { debounceFn } from '@/utils/debounce'
import { computed, inject, onBeforeMount, onBeforeUnmount, ref, watch, type ComputedRef } from 'vue'
import { useRouter, type RouteRecordNormalized } from 'vue-router'

const { push, getRoutes } = useRouter()

//Services
const ls = LocaleService.get()
const l = LocaleService.currentLocale
const messageService = getClient(MessageClient)
const messageHub = inject<MessageHub>('messageHub')!
const userService = getClient(UserClient)

//Reactive data
const searchInput = ref('')
const unreadMessageCount = ref(0)
const unreadMessageCountPositive = computed(() =>
  unreadMessageCount.value < 0 ? 0 : unreadMessageCount.value
)
const unsubscribeCallbacks: (() => void)[] = []
const currentUserId = computed(() => AuthService.profileInfo.value?.id)

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
      if (AuthService.isAuthenticated.value) {
        userService.setLanguage(localeName)
      }
    }
  }))
)

/** Navigation routes allowed for current user */
const allowedRouteItems: ComputedRef<INavbarItem[]> = computed(() => {
  const routes = getRoutes()
  return filterRoutes(allRouteItems, routes, AuthService.permissions.value)
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
      url:
        AuthService.profileInfo.value?.profileImage?.imageURLs?.thumbnailUrl ??
        defaultProfileImageUrl,
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

//Constants
/** All navigation routes */
const allRouteItems: INavbarItem[] = [
  {
    label: 'navigation.advertisements',
    items: [
      {
        route: 'viewAdvertisements',
        label: 'navigation.seeAdvertisements'
      },
      {
        route: 'recentlyViewedAdvertisements',
        label: 'navigation.recentlyViewedAdvertisements'
      },
      {
        route: 'bookmarkedAdvertisements',
        label: 'navigation.savedAdvertisements',
        showWithoutPermission: true
      },
      {
        route: 'manageAdvertisementNotificationSubscription',
        label: 'navigation.advertisementNotifications'
      },
      {
        route: 'createAdvertisement',
        label: 'navigation.createAdvertisement',
        showWithoutPermission: true
      },
      {
        route: 'manageAdvertisements',
        label: 'navigation.myAdvertisements'
      }
    ] as INavbarItem[]
  },
  {
    icon: 'pi pi-envelope',
    route: 'viewMessages',
    badgeValue: unreadMessageCountPositive
  }
]

//Hooks
onBeforeMount(async () => {
  unsubscribeCallbacks.push(
    await messageHub.subscribeNewMessages((_, message) => {
      if (message.fromUserId !== currentUserId.value) {
        setTimeout(() => {
          unreadMessageCount.value += 1
        }, NewMessageTimeout)
      }
    })
  )

  unsubscribeCallbacks.push(
    await messageHub.subscribeNewChat((newChat) => {
      if (
        typeof newChat.advertisementOwnerId === 'number' &&
        newChat.advertisementOwnerId === currentUserId.value
      ) {
        unreadMessageCount.value += 1
      }
    })
  )

  unsubscribeCallbacks.push(
    await messageHub.subscribeMarkMessageAsRead(
      (_chatId, userId, _messageIds, messagesAffected) => {
        if (userId === currentUserId.value) {
          unreadMessageCount.value -= messagesAffected
        }
      }
    )
  )

  unsubscribeCallbacks.push(
    messageHub.subscribeToReconnect(() => {
      loadUnreadMessageCount()
    })
  )
})

onBeforeUnmount(() => {
  for (const callback of unsubscribeCallbacks) {
    callback()
  }
})

//Methods
/** Search advertisements with debounce */
const search = () => {
  push({ name: 'searchAdvertisements', query: { search: searchInput.value } })
  searchInput.value = ''
}
const { debounce: debouncedSearch, clear: clearDebounceSearch } = debounceFn(search)

/** Search advertisements without debounce */
const immediateSearch = () => {
  clearDebounceSearch()
  search()
}

/** Filter route items for which current user does not have a permission to navigate */
const filterRoutes = (
  items: INavbarItem[],
  routes: RouteRecordNormalized[],
  userPermissions: string[]
): INavbarItem[] => {
  return items
    .map((i) => {
      const route = routes.find((r) => r.name === i.route)
      const requiresPermission = route?.meta.requiresPermission
      const hasPermission =
        typeof requiresPermission === 'string'
          ? userPermissions.some((p) => p === requiresPermission)
          : true

      if (hasPermission || i.showWithoutPermission) {
        return {
          label: i.label,
          route: i.route,
          icon: i.icon,
          badgeValue: i.badgeValue,
          items: i.items?.length ? filterRoutes(i.items, routes, userPermissions) : i.items
        } as INavbarItem
      } else {
        return null
      }
    })
    .filter((i) => i != null) as INavbarItem[]
}

/** Call Api to get unread message count for user */
const loadUnreadMessageCount = async () => {
  unreadMessageCount.value = await messageService.getUnreadMessageCount()
}

//Watch
/** Get unread message count when user logged ing */
watch(
  AuthService.isAuthenticated,
  (isAuthenticated) => {
    if (isAuthenticated) {
      loadUnreadMessageCount()
    }
  },
  { immediate: true }
)
</script>
