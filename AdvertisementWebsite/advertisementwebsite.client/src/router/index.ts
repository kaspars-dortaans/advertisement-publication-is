import { Permissions } from '@/constants/api/Permissions'
import { AuthService } from '@/services/auth-service'
import { createRouter, createWebHistory, type LocationQueryValue } from 'vue-router'

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
      props: (route) => ({ id: toNumberOrUndefined(route.params.id) })
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
          path: 'view',
          name: 'viewAdvertisement',
          component: () => import('../views/advertisement/ViewAdvertisement.vue'),
          props: (route) => ({ id: toNumberOrUndefined(route.params.id) })
        },
        {
          path: 'report',
          name: 'reportAdvertisement',
          component: () => import('../views/advertisement/ReportAdvertisement.vue'),
          props: (route) => ({ id: toNumberOrUndefined(route.params.id) })
        },
        {
          path: 'edit',
          name: 'editAdvertisement',
          component: () => import('../views/advertisement/CreateOrEditAdvertisement.vue'),
          props: (route) => ({ id: toNumberOrUndefined(route.params.id) })
        }
      ]
    },
    {
      path: '/advertisements/view',
      name: 'viewAdvertisements',
      component: () => import('../views/advertisements/ViewAdvertisements.vue')
    },
    {
      path: '/extend/:type/:ids',
      name: 'extend',
      component: () => import('../views/ExtendForm.vue'),
      props: (route) => ({
        ids: JSON.parse(firstParam(route.params.ids)),
        type: firstParam(route.params.type)
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
        return {
          chatId: toNumberOrUndefined(route.params.chatId),
          newChatToUserId: toNumberOrUndefined(route.query.newChatToUserId),
          newChatToAdvertisementId: toNumberOrUndefined(route.query.newChatToAdvertisementId)
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

    {
      path: '/payments',
      redirect: { name: 'viewPayments' },
      children: [
        {
          path: 'pay',
          name: 'makePayment',
          component: () => import('../views/payments/MakePayment.vue'),
          meta: {
            requiresPermission: Permissions[Permissions.MakePayment]
          }
        },
        {
          path: 'view',
          name: 'viewPayments',
          component: () => import('../views/payments/ViewPayments.vue'),
          meta: {
            requiresPermission: Permissions[Permissions.ViewPayments]
          }
        },
        {
          path: 'view/:paymentId',
          name: 'viewPayment',
          component: () => import('../views/payments/ViewPayment.vue'),
          props: (route) => ({ paymentId: toNumberOrUndefined(route.params.paymentId) }),
          meta: {
            requiresPermission: Permissions[Permissions.ViewPayments]
          }
        }
      ]
    },

    //Not found
    {
      path: '/:all(.*)*',
      name: 'notFound',
      component: () => import('../views/NotFound.vue')
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
