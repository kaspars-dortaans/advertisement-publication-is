import { Permissions } from '@/constants/api/Permissions'
import { AuthService } from '@/services/auth-service'
import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      redirect: { name: 'viewAdvertisements' }
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/user/LoginView.vue'),
      props: (route) => ({ redirect: !!route.query.redirect })
    },
    {
      path: '/logout',
      name: 'logout',
      beforeEnter: (_, from, next) => {
        AuthService.get().logout()

        if (from.meta.requiresPermission) {
          next({ name: 'home' })
        } else {
          next(from.fullPath)
        }
      },
      //Component is not displayed, guard always redirects to another page. Component is used to make this a valid route.
      component: () => import('../views/NotFound.vue')
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('../views/user/RegisterView.vue')
    },
    {
      path: '/user/:id/',
      name: 'viewUser',
      component: () => import('../views/user/ViewUser.vue'),
      props: (route) => ({ id: parseInt(firstParam(route.params.id)) })
    },
    {
      path: '/recently-viewed-advertisements',
      name: 'recentlyViewedAdvertisements',
      component: () => import('../views/advertisements/RecentlyViewedAdvertisements.vue')
    },
    {
      path: '/search',
      name: 'searchAdvertisements',
      component: () => import('../views/advertisements/SearchAdvertisement.vue'),
      props: (route) => ({ search: route.query?.search })
    },
    {
      path: '/advertisement/create',
      name: 'createAdvertisement',
      component: () => import('../views/advertisement/CreateOrEditAdvertisement.vue'),
      meta: {
        requiresPermission: Permissions[Permissions.CreateAdvertisement]
      }
    },
    {
      path: '/advertisement/:id/',
      redirect: (route) => ({ name: 'viewAdvertisement', params: route.params }),
      children: [
        {
          path: '/advertisement/:id/view',
          name: 'viewAdvertisement',
          component: () => import('../views/advertisement/ViewAdvertisement.vue'),
          props: (route) => ({ id: parseInt(firstParam(route.params.id)) })
        },
        {
          path: '/advertisement/:id/report',
          name: 'reportAdvertisement',
          component: () => import('../views/advertisement/ReportAdvertisement.vue'),
          props: (route) => ({ id: parseInt(firstParam(route.params.id)) })
        },
        {
          path: '/advertisement/:id/edit',
          name: 'editAdvertisement',
          component: () => import('../views/advertisement/CreateOrEditAdvertisement.vue'),
          props: (route) => ({ id: parseInt(firstParam(route.params.id)) })
        }
      ]
    },
    {
      path: '/advertisements/view',
      name: 'viewAdvertisements',
      component: () => import('../views/advertisements/ViewAdvertisements.vue')
    },
    {
      path: '/advertisements/extend/:advertisementIds',
      name: 'extendAdvertisements',
      component: () => import('../views/advertisements/ExtendAdvertisements.vue'),
      props: (route) => ({
        advertisementIds: JSON.parse(firstParam(route.params.advertisementIds))
      })
    },

    {
      path: '/profile-info',
      name: 'profileInfo',
      component: () => import('../views/user/ProfileInfo.vue'),
      meta: {
        requiresPermission: Permissions[Permissions.ViewProfileInfo]
      }
    },
    {
      path: '/edit-profile-info',
      name: 'editProfileInfo',
      component: () => import('../views/user/EditProfileInfo.vue'),
      meta: {
        requiresPermission: Permissions[Permissions.EditProfileInfo]
      }
    },
    {
      path: '/change-password',
      name: 'changePassword',
      component: () => import('../views/user/ChangePassword.vue'),
      meta: {
        requiresPermission: Permissions[Permissions.ChangePassword]
      }
    },
    {
      path: '/bookmarked-advertisements',
      name: 'bookmarkedAdvertisements',
      component: () => import('../views/advertisements/BookmarkedAdvertisements.vue'),
      meta: {
        requiresPermission: Permissions[Permissions.ViewAdvertisementBookmarks]
      }
    },
    {
      path: '/manage-advertisements',
      name: 'manageAdvertisements',
      component: () => import('../views/user/ManageUserAdvertisements.vue'),
      meta: {
        requiresPermission: Permissions[Permissions.ViewOwnedAdvertisements]
      }
    },
    {
      path: '/view-messages/:chatId?',
      name: 'viewMessages',
      component: () => import('../views/messages/ViewMessages.vue'),
      props: (route) => {
        const userId = parseInt('' + route.query.newChatToUserId)
        const advertisementId = parseInt('' + route.query.newChatToAdvertisementId)
        const chatId = parseInt(firstParam(route.params.chatId))
        return {
          chatId: isNaN(chatId) ? undefined : chatId,
          newChatToUserId: isNaN(userId) ? undefined : userId,
          newChatToAdvertisementId: isNaN(advertisementId) ? undefined : advertisementId
        }
      },
      meta: {
        requiresPermission: Permissions[Permissions.ViewMessages]
      }
    },
    {
      path: '/advertisement-notification-subscriptions',
      redirect: { name: 'manageAdvertisementNotificationSubscription' },
      children: [
        {
          path: 'manage',
          name: 'manageAdvertisementNotificationSubscription',
          component: () =>
            import('../views/advertisement-notification-subscriptions/ManageSubscriptions.vue'),
          meta: {
            requiresPermission: Permissions[Permissions.ViewAdvertisementNotificationSubscriptions]
          }
        },
        {
          path: 'create',
          name: 'createAdvertisementNotificationSubscription',
          component: () =>
            import(
              '../views/advertisement-notification-subscriptions/CreateOrEditSubscription.vue'
            ),
          meta: {
            requiresPermission: Permissions[Permissions.CreateAdvertisementNotificationSubscription]
          }
        },

        {
          path: 'edit/:subscriptionId',
          name: 'editAdvertisementNotificationSubscription',
          component: () =>
            import(
              '../views/advertisement-notification-subscriptions/CreateOrEditSubscription.vue'
            ),
          props: (route) => ({ subscriptionId: toNumberOrUndefined(route.params.subscriptionId) }),
          meta: {
            requiresPermission: Permissions[Permissions.EditAdvertisementNotificationSubscriptions]
          }
        }
      ]
    },

    //Not found
    {
      path: '/not-found',
      name: 'notFound',
      component: () => import('../views/NotFound.vue')
    },
    {
      path: '/:all(.*)',
      redirect: { name: 'notFound' }
    }
  ]
})

//Add route permission guard
router.beforeEach(async (to, _, next) => {
  const requiresPermission = to.meta?.requiresPermission
  if (
    typeof requiresPermission !== 'string' ||
    (await AuthService.hasPermission(requiresPermission))
  ) {
    next()
  } else {
    if (AuthService.isAuthenticated.value) {
      next({ name: 'notFound' })
    } else {
      next({ name: 'login', query: { redirect: 'true' } })
    }
  }
})

const firstParam = (p: string | string[]) => {
  return Array.isArray(p) ? p[0] : p
}

const toNumberOrUndefined = (
  param: string | string[] | LocationQueryValue | LocationQueryValue[]
) => {
  const firstParam = Array.isArray(param) ? param[0] : param
  if (typeof firstParam !== 'string') {
    return undefined
  }

  const numberParam = parseInt(firstParam)
  return isNaN(numberParam) ? undefined : numberParam
}

export default router
