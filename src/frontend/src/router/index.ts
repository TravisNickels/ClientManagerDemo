import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/client-dashboard',
    },
    {
      path: '/client-dashboard',
      name: 'client-dashboard',
      component: () => import('@/pages/client-dashboard.vue'),
      meta: { title: 'Clients' },
    },
    {
      path: '/client/:id',
      name: 'client-management',
      component: () => import('@/pages/client-management.vue'),
      props: true,
      meta: { title: 'Edit Client' },
    },
  ],
})

export default router
