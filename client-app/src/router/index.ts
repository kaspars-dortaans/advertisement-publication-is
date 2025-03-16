import { Permissions } from '@/constants/api/Permissions'
import { AuthService } from '@/services/auth-service'
import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('../views/advertisements/ViewAdvertisements.vue')
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
        }
      ]
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
      //TODO: Replace when view is completed
      component: () => import('../views/NotFound.vue')
    },
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

export default router
