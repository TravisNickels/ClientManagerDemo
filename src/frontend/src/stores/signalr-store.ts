import { defineStore } from 'pinia'
import { ref } from 'vue'
import { SignalRClient } from '@/signalr/client'
import type { SignalrEventName, SignalrEvents } from '@/signalr/events'

export const useSignalRStore = defineStore('signalr', () => {
  const connected = ref<boolean>(false)
  const client = new SignalRClient('http://localhost:5200/notifications')
  type Handler<K extends SignalrEventName> = (payload: SignalrEvents[K]) => void

  const connect = async (): Promise<void> => {
    await client.connect()
    connected.value = true
  }

  const on = <K extends SignalrEventName>(eventName: K, handler: Handler<K>): void => {
    client.on(eventName, handler)
  }

  const disconnect = async () => {
    await client.disconnect()
    connected.value = false
  }

  return {
    connected,
    connect,
    disconnect,
    on,
  }
})
