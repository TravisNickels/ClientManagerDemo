import { type StoreDefinition } from 'pinia'
import { vi } from 'vitest'
import { computed, ref } from 'vue'

// Hoisted value runs before mocks & imports
const { signalrConfig } = vi.hoisted(() => ({
  signalrConfig: { useRealSignalR: false },
}))

vi.mock('@/stores/signalr-store', async () => {
  if (signalrConfig.useRealSignalR) {
    return await vi.importActual<StoreDefinition>('@/stores/signalr-store')
  }

  return {
    useSignalRStore: () => ({
      status: ref<'connected' | 'connecting' | 'reconnecting' | 'disconnected'>('disconnected'),
      reconnectAttempt: ref(0),
      lastSeen: ref<Date | null>(null),
      isReconnecting: computed(() => false),

      // Prevent real signalr
      on: vi.fn(),
      connect: vi.fn(async () => {}),
      disconnect: vi.fn(async () => {}),
      manualReconnect: vi.fn(async () => {}),
    }),
  }
})

export { signalrConfig }
