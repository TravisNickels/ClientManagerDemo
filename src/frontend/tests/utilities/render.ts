import { render, type RenderOptions } from '@testing-library/vue'
import { createTestingPinia, type TestingPinia } from '@pinia/testing'
import { createMemoryHistory, createRouter, type Router, type RouteRecordRaw } from 'vue-router'
import { type Component } from 'vue'
import { vi } from 'vitest'
import { createPinia, type Pinia } from 'pinia'
import { signalrConfig } from './signalr-mock'

export interface RenderWithAppOptions<T extends Component = Component> extends RenderOptions<T> {
  useRealPinia?: boolean
  useRealSignalR?: boolean
  routes?: RouteRecordRaw[]
}

export async function renderWithApp<T extends Component>(component: T, options: RenderWithAppOptions<T> = {}) {
  const pinia: TestingPinia | Pinia = options.useRealPinia ? createPinia() : createTestingPinia({ createSpy: (fn) => vi.fn(fn), stubActions: true })
  const router: Router = createRouter({
    history: createMemoryHistory(),
    routes: options.routes ?? [
      {
        path: '/',
        name: '',
        component: { template: '<div />' },
      },
      {
        path: '/client-dashboard',
        name: 'client-dashboard',
        component: { template: '<div />' },
      },
      {
        path: '/client/:id',
        name: 'client-management',
        component: { template: '<div />' },
      },
    ],
  })

  await router.push('/')
  await router.isReady()

  signalrConfig.useRealSignalR = !!options.useRealSignalR

  return render(component, {
    ...options,
    global: {
      ...options.global,
      plugins: [...(options.global?.plugins ?? []), pinia, router],
    },
  })
}
