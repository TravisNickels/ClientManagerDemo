import ClientDashboard from '@/pages/client-dashboard.vue'
import ClientHome from '@/pages/client-home.vue'
import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'client-home',
      component: ClientHome,
    },
    {
      path: '/client-dashboard',
      name: 'client-dashboard',
      component: ClientDashboard,
    },
  ],
})

export default router
