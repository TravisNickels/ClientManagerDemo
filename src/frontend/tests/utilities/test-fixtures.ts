import type { Client } from '@/types/client'
import { renderWithApp } from './render'
import Dashboard from '@/pages/client-dashboard.vue'
import DashboardHeader from '@/components/dashboard-header.vue'
import { useClientStore } from '@/stores/clientStore'

export async function setupDashboard(clients: Client[]) {
  const wrapper = await renderWithApp(Dashboard, {
    useRealSignalR: false,
    useRealPinia: false,
  })

  const store = useClientStore()
  store.isLoading = false
  store.allClients = clients

  return { wrapper, store }
}

export async function setupDashboardHeader(clients: Client[]) {
  return await renderWithApp(DashboardHeader, {
    useRealSignalR: true,
    useRealPinia: false,
    props: {
      clients: clients,
    },
  })
}
