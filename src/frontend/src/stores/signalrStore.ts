import { defineStore } from 'pinia'
import { ref } from 'vue'
import * as signalR from '@microsoft/signalr'

export const useSignalRStore = defineStore('signalr', () => {
  const connection = ref<signalR.HubConnection | null>()
  const connected = ref<boolean>(false)
  const messages = ref<string[]>([])

  const connect = async (): Promise<void> => {
    if (connection.value) return

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5200/notifications')
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build()

    connection.value.on('clientresponse', (clientData) => {
      console.log('Client created event received:', clientData)
      clientResponse(clientData)
    })

    connection.value.onreconnected(() => {
      connected.value = true
      console.log('Reconnected to SignalR hub')
    })

    try {
      await connection.value.start()
      connected.value = true
      console.log('Connected to SignalR hub')
    } catch (error) {
      console.error('Connection failed:', error)
    }
  }
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const clientResponse = (clientData: any) => {
    messages.value.push(`Client created: ${JSON.stringify(clientData)}`)
  }

  const disconnect = async () => {
    if (connection.value) {
      await connection.value.stop()
      connection.value = null
      connected.value = false
      console.log('Disconnected from signalR hub')
    }
  }

  return {
    connect,
    disconnect,
    clientResponse,
  }
})
