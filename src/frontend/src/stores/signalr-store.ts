import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import type { SignalrEventName, SignalrEvents } from '@/types/signalr/signalrEvents'
import { useSignalRClient, type LifecycleEvent, type ConnectionState } from '@/composables/useSignalRClient'
import { toast } from 'vue3-toastify'

export const useSignalRStore = defineStore('signalr', () => {
  const status = ref<ConnectionState>('disconnected')
  const reconnectAttempt = ref<number>(0)
  const lastSeen = ref<Date | null>(null)
  const isReconnecting = computed(() => status.value === 'reconnecting')

  const client = useSignalRClient()

  client.onLifecycle(async (event: LifecycleEvent) => {
    switch (event.type) {
      case 'connected':
        status.value = 'connected'
        toast.success('Connected to SignalR')
        reconnectAttempt.value = 0
        lastSeen.value = new Date()
        break
      case 'connecting':
        status.value = 'connecting'
        break
      case 'reconnecting':
        status.value = 'reconnecting'
        reconnectAttempt.value = event.attempt
        break
      case 'disconnected':
        status.value = 'disconnected'
        toast.warn('[SignalR] Disconnected')
        break
      case 'permanently-disconnected':
        status.value = 'disconnected'
        toast.error('SignalR connection permanently lost')
        break
    }
  })

  // -------------------------------------------------------
  //  Event registration
  // -------------------------------------------------------
  function on<K extends SignalrEventName>(eventName: K, handler: (payload: SignalrEvents[K]) => void): void {
    client.on(eventName, handler)
  }

  // -------------------------------------------------------
  //  Connection controls
  // -------------------------------------------------------
  async function connect() {
    await toast.promise(
      client.connect(),
      {
        pending: { render: 'Connecting to the SignalR...' },
        success: { render: 'Connected to SignalR' },
        error: { render: 'Failed to connect to SignalR' },
      },
      {
        position: 'bottom-right',
      }
    )
  }

  async function disconnect() {
    if (client && status.value !== 'disconnected') await client.disconnect()
  }

  async function manualReconnect(): Promise<void> {
    await client.manualReconnect()
  }

  return {
    status,
    reconnectAttempt,
    lastSeen,
    isReconnecting,
    connect,
    disconnect,
    manualReconnect,
    on,
  }
})
