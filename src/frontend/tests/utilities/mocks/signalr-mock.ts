import { vi } from 'vitest'
import { computed, ref, type ComputedRef, type Ref } from 'vue'
import type { SignalrEventName, SignalREventHandler, SignalrEventMap, SignalrEvents } from '@/types/signalr/signalrEvents'

vi.mock('@/stores/signalr-store', async () => {
  return {
    useSignalRStore: mockSignalRStoreFactory,
  }
})

export interface MockSignalRStore {
  status: Ref<string>
  reconnectAttempt: Ref<number>
  lastSeen: Ref<Date | null>
  isReconnecting: ComputedRef<boolean>

  on: (event: SignalrEventName, handler: SignalREventHandler) => void
  emit: <K extends SignalrEventName>(event: K, payload: SignalrEventMap[K]) => void
  connect: () => Promise<void>
  disconnect: () => Promise<void>
  manualReconnect: () => void
}

export function asMockSignalRStore(store: unknown): MockSignalRStore {
  return store as MockSignalRStore
}

const callbacks = new Map<string, SignalREventHandler[]>()

const mockSignalRStoreFactory = (): MockSignalRStore => {
  return {
    status: ref('disconnected'),
    reconnectAttempt: ref(0),
    lastSeen: ref(null),
    isReconnecting: computed(() => false),

    on: vi.fn((event: SignalrEventName, handler: SignalREventHandler) => {
      if (!callbacks.has(event)) {
        callbacks.set(event, [])
      }
      callbacks.get(event)!.push(handler)
    }),

    emit: vi.fn(<k extends SignalrEventName>(event: string, payload: SignalrEvents[k]) => {
      const handlers = callbacks.get(event)
      if (handlers) {
        handlers.forEach((h) => h(payload))
      }
    }),

    connect: vi.fn(async () => {}),
    disconnect: vi.fn(async () => {}),
    manualReconnect: vi.fn(async () => {}),
  }
}
