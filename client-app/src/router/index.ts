import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('../views/HomeView.vue')
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/User/LoginView.vue'),
      props: (route) => ({ redirect: !!route.query.redirect })
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('../views/User/RegisterView.vue')
    },
    {
      path: '/advertisement/:id/',
      redirect: (route) => ({ name: 'viewAdvertisement', params: route.params }),
      children: [
        {
          path: '/advertisement/:id/view',
          name: 'viewAdvertisement',
          component: () => import('../views/Advertisement/ViewAdvertisement.vue'),
          props: (route) => ({ id: parseInt(firstParam(route.params.id)) })
        },
        {
          path: '/advertisement/:id/report',
          name: 'reportAdvertisement',
          component: () => import('../views/Advertisement/ReportAdvertisement.vue'),
          props: (route) => ({ id: parseInt(firstParam(route.params.id)) })
        }
      ]
    }
  ]
})

const firstParam = (p: string | string[]) => {
  return Array.isArray(p) ? p[0] : p
}

export default router
