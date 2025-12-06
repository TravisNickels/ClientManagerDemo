import { render, type RenderOptions } from '@testing-library/vue'
import { createTestingPinia, type TestingPinia } from '@pinia/testing'
import { createMemoryHistory, createRouter, type Router, type RouteRecordRaw } from 'vue-router'
import { type Component } from 'vue'
import { vi } from 'vitest'
import { createPinia, type Pinia } from 'pinia'
import { signalrConfig } from './signalr-mock'
import ClientDashboard from '@/pages/client-dashboard.vue'
import ClientManagement from '@/pages/client-management.vue'

export interface RenderWithAppOptions<T extends Component = Component> extends RenderOptions<T> {
  useRealPinia?: boolean
  useRealSignalR?: boolean
  initialRoute?: string
  routes?: RouteRecordRaw[]
}

export async function renderWithApp<T extends Component>(component: T, options: RenderWithAppOptions<T> = {}) {
  const pinia: TestingPinia | Pinia = options.useRealPinia ? createPinia() : createTestingPinia({ createSpy: (fn) => vi.fn(fn), stubActions: true })
  const router: Router = createRouter({
    history: createMemoryHistory(),
    routes: options.routes ?? [
      {
        path: '/',
        redirect: '/client-dashboard',
      },
      {
        path: '/client-dashboard',
        name: 'client-dashboard',
        component: ClientDashboard,
      },
      {
        path: '/client/:id',
        name: 'client-management',
        component: ClientManagement,
      },
    ],
  })

  await router.push(options.initialRoute ?? '/client-dashboard')
  await router.isReady()

  signalrConfig.useRealSignalR = !!options.useRealSignalR

  const renderedComponent = render(component, {
    ...options,
    global: {
      ...options.global,
      plugins: [...(options.global?.plugins ?? []), pinia, router],
    },
  })

  return { renderedComponent, router }
}
