import type { Client } from '@/types/client'
import { renderWithApp } from './render'
import Dashboard from '@/pages/client-dashboard.vue'
import DashboardHeader from '@/components/dashboard-header.vue'
import ClientForm from '@/components/client-form.vue'
import { useClientStore } from '@/stores/client-store'
import App from '@/App.vue'

const defaultClients: Client[] = [
  { id: '11111111-1111-1111-1111-111111111111', firstName: 'Luke', lastName: 'Skywalker', email: 'Luke.Skywalker@gmail.com', isArchived: false },
  { id: '22222222-2222-2222-2222-222222222222', firstName: 'Han', lastName: 'Solo', email: 'han.solo@gmail.com', isArchived: false },
  { id: '33333333-3333-3333-3333-333333333333', firstName: 'Darth', lastName: 'Vader', email: 'darth.vader@gmail.com', isArchived: true },
  { id: '44444444-4444-4444-4444-444444444444', firstName: 'Lord', lastName: 'Sidious', email: 'lord.sidious@gmail.com', isArchived: true },
]

export async function setupDefaultApp(initialRoute: string) {
  const { renderedComponent, router } = await renderWithApp(App, {
    useRealSignalR: false,
    useRealPinia: false,
    initialRoute: initialRoute,
  })

  const store = useClientStore()
  store.isLoading = false
  store.allClients = defaultClients

  return { renderedComponent, router, store }
}

export async function setupDashboard(clients: Client[] = defaultClients) {
  const { renderedComponent, router } = await renderWithApp(Dashboard, {
    useRealSignalR: false,
    useRealPinia: false,
  })

  const store = useClientStore()
  store.isLoading = false
  store.allClients = clients

  return { renderedComponent, router, store }
}

export async function setupDashboardHeader(clients: Client[] = defaultClients) {
  return await renderWithApp(DashboardHeader, {
    useRealSignalR: true,
    useRealPinia: false,
    props: {
      clients: clients,
    },
  })
}

export async function setupClientForm(client: Client, mode: 'edit' | 'create', saving: boolean) {
  await renderWithApp(ClientForm, {
    props: {
      client: client,
      mode: mode,
      saving: saving,
    },
  })
}
