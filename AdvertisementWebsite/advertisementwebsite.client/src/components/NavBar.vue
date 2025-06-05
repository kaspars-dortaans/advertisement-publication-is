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
        <i v-if="item.icon" :class="item.icon" />
        <span v-if="item.label">{{ ls.l(item.label) }}</span>
        <Badge v-if="item.badgeValue?.value" :value="item.badgeValue.value" class="rounded-xl" />
        <span v-if="hasSubmenu" class="pi pi-fw pi-angle-down"></span>
      </a>
    </template>
  </MenuBar>
</template>

<script setup lang="ts">
import defaultProfileImageUrl from '@/assets/images/default-profile-image.svg'
import logoUrl from '@/assets/images/logo.svg'
import { Permissions } from '@/constants/api/Permissions'
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

const { push, getRoutes, afterEach } = useRouter()

//Services
const ls = LocaleService.get()
const l = LocaleService.currentLocale
const messageService = getClient(MessageClient)
const messageHub = inject<MessageHub>('messageHub')!
const userService = getClient(UserClient)

//Reactive data
const searchInput = ref('')
const unreadMessageCount = ref(0)
const unsubscribeCallbacks: (() => void)[] = []
const currentUserId = computed(() => AuthService.profileInfo.value?.id)

/** All navigation items */
const navbarItems: ComputedRef<INavbarItem[]> = computed(() => {
  const allRouteItems = [
    ...constantRoutes,
    ...messageRoutes.value,
    {
      label: LocaleService.currentLocaleName.value,
      items: localeItems.value
    },
    ...profileRoutes.value
  ]

  const routes = getRoutes()
  const allowedNavbarItems = filterRoutes(allRouteItems, routes, AuthService.permissions.value)
  return allowedNavbarItems
})

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

const profileRoutes = computed(() => {
  let profileRoutes
  if (AuthService.isAuthenticated.value) {
    profileRoutes = {
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
          label: 'navigation.viewTermsOfUse',
          route: 'viewTermsOfUse'
        },
        {
          label: 'navigation.logout',
          route: 'logout'
        }
      ]
    }
  } else {
    profileRoutes = {
      icon: 'pi pi-user',
      items: [
        {
          label: 'navigation.login',
          route: 'login'
        },
        {
          label: 'navigation.viewTermsOfUse',
          route: 'viewTermsOfUse'
        }
      ]
    }
  }

  return [profileRoutes] as INavbarItem[]
})

const messageRoutes = computed(() => {
  const routes: INavbarItem[] = []
  if (AuthService.hasPermission(Permissions.ViewRuleViolationReports)) {
    routes.push({
      icon: 'pi pi-envelope',
      doNotShowWithoutItems: true,
      badgeValue: unreadMessageCount,
      items: [
        {
          label: 'navigation.messages',
          route: 'viewMessages',
          badgeValue: unreadMessageCount
        },
        {
          label: 'navigation.manageRuleViolationReports',
          route: 'manageRuleViolationReports'
        }
      ]
    })
  } else {
    routes.push({
      icon: 'pi pi-envelope',
      route: 'viewMessages',
      badgeValue: unreadMessageCount
    })
  }
  return routes
})

//Constants
/** All navigation routes */
const constantRoutes: INavbarItem[] = [
  {
    label: 'navigation.advertisements',
    doNotShowWithoutItems: true,
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
        showForUnauthenticated: true
      },
      {
        route: 'manageOwnAdvertisementNotificationSubscription',
        label: 'navigation.advertisementNotifications'
      },
      {
        route: 'createOwnAdvertisement',
        label: 'navigation.createAdvertisement',
        showForUnauthenticated: true
      },
      {
        route: 'manageOwnAdvertisements',
        label: 'navigation.myAdvertisements'
      }
    ] as INavbarItem[]
  },
  {
    label: 'navigation.manage',
    doNotShowWithoutItems: true,
    items: [
      {
        route: 'manageUsers',
        label: 'navigation.manageUsers'
      },
      {
        route: 'managePermissions',
        label: 'navigation.managePermissions'
      },
      {
        route: 'manageRoles',
        label: 'navigation.manageRoles'
      },
      {
        route: 'manageAdvertisements',
        label: 'navigation.manageAdvertisements'
      },
      {
        route: 'manageAdvertisementNotificationSubscription',
        label: 'navigation.manageAdvertisementNotificationSubscription'
      },
      {
        route: 'manageCategories',
        label: 'navigation.manageCategories'
      },
      {
        route: 'manageAttributes',
        label: 'navigation.manageAttributes'
      },
      {
        route: 'manageAttributeValueLists',
        label: 'navigation.manageAttributeValueLists'
      },
      {
        route: 'manageServicePrices',
        label: 'navigation.manageServicePrices'
      }
    ]
  },
  {
    label: 'navigation.viewPayments',
    route: 'viewPayments'
  },
  {
    label: 'navigation.viewSystemPayments',
    route: 'viewSystemPayments'
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

/** Clear search input on route leave */
afterEach((to, from) => {
  if (from?.name === 'searchAdvertisements' && to.name !== 'searchAdvertisements') {
    searchInput.value = ''
  }
})

//Methods
/** Search advertisements with debounce */
const search = () => {
  push({ name: 'searchAdvertisements', query: { search: searchInput.value } })
}
const { debounce: debouncedSearch, clear: clearDebounceSearch } = debounceFn(search, 2000)

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
        typeof requiresPermission !== 'number' ||
        AuthService.hasPermission(requiresPermission as number)

      if (hasPermission || (i.showForUnauthenticated && !AuthService.isAuthenticated.value)) {
        const items = i.items?.length ? filterRoutes(i.items, routes, userPermissions) : undefined
        if (items?.length || !i.doNotShowWithoutItems) {
          const copy = Object.assign({}, i)
          copy.items = items
          return copy as INavbarItem
        }
      }

      return null
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
    } else {
      unreadMessageCount.value = 0
    }
  },
  { immediate: true }
)
</script>
