import { render, type RenderOptions } from '@testing-library/vue'
import { createTestingPinia, type TestingPinia } from '@pinia/testing'
import { createMemoryHistory, createRouter, type Router, type RouteRecordRaw } from 'vue-router'
import { type Component } from 'vue'
import { vi } from 'vitest'
import { createPinia, type Pinia } from 'pinia'
import ClientDashboard from '@/pages/client-dashboard.vue'
import ClientManagement from '@/pages/client-management.vue'
import { useSignalRStore, type SignalRStoreInstance } from '@/stores/signalr-store'
import { useClientStore } from '@/stores/client-store'

export interface RenderWithAppOptions<T extends Component = Component> extends RenderOptions<T> {
  useRealPinia?: boolean
  useRealSignalR?: boolean
  initialRoute?: string
  routes?: RouteRecordRaw[]
}

export async function renderWithApp<T extends Component>(component: T, options: RenderWithAppOptions<T> = {}) {
  const pinia: Pinia | TestingPinia = getPinia(options.useRealPinia)
  const router: Router = createRouter({
    history: createMemoryHistory(),
    routes: options.routes ?? [
      { path: '/', redirect: '/client-dashboard' },
      { path: '/client-dashboard', name: 'client-dashboard', component: ClientDashboard },
      { path: '/client/:id', name: 'client-management', component: ClientManagement },
    ],
  })

  const signalRStore = useSignalRStore(pinia) as SignalRStoreInstance
  const clientStore = useClientStore(pinia)

  await router.push(options.initialRoute ?? '/')
  await router.isReady()

  const renderedComponent = render(component, {
    ...options,
    global: {
      ...options.global,
      plugins: [...(options.global?.plugins ?? []), pinia, router],
    },
  })

  return { renderedComponent, router, pinia, signalRStore, clientStore }
}

function getPinia(useRealPinia: boolean = false, stub: boolean = false): TestingPinia | Pinia {
  return useRealPinia ? createPinia() : createTestingPinia({ createSpy: (fn) => vi.fn(fn), stubActions: stub })
}
