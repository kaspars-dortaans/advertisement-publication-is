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
      component: () => import('../views/LoginView.vue'),
      props: (route) => ({ redirect: !!route.query.redirect })
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('../views/RegisterView.vue')
    },
    {
      path: '/advertisement/:id',
      name: 'advertisement',
      component: () => import('../views/AdvertisementView.vue'),
      props: (route) => {
        let id
        if (Array.isArray(route.params.id)) {
          id = parseInt(route.params.id[0])
        } else {
          id = parseInt(route.params.id)
        }
        return { id }
      }
    }
  ]
})

export default router
