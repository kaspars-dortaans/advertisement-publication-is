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
      path: '/not-found',
      name: 'notFound',
      component: () => import('../views/NotFound.vue')
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/user/LoginView.vue'),
      props: (route) => ({ redirect: !!route.query.redirect })
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
        requiredPermission: Permissions[Permissions.ViewAdvertisementBookmarks]
      }
    }
  ]
})

//Add route permission guard
router.beforeEach(async (to, _, next) => {
  const requiredPermission = to.meta?.requiredPermission
  if (
    typeof requiredPermission !== 'string' ||
    (await AuthService.hasPermission(requiredPermission))
  ) {
    next()
  } else {
    next({ name: 'notFound' })
  }
})

const firstParam = (p: string | string[]) => {
  return Array.isArray(p) ? p[0] : p
}

export default router
