import { defineStore } from 'pinia'
import * as signalR from '@microsoft/signalr'

export const useSignalRStore = defineStore('signalr', {
  state: () => ({
    connection: null as signalR.HubConnection | null,
    connected: false,
    messages: [] as string[],
  }),

  actions: {
    async connect() {
      if (this.connection) return

      this.connection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5200/notifications')
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build()

      this.connection.on('clientcreated', (clientData) => {
        console.log('Client created event received:', clientData)
        this.clientCreated(clientData)
      })

      this.connection.onreconnected(() => {
        this.connected = true
        console.log('Reconnected to SignalR hub')
      })

      try {
        await this.connection.start()
        this.connected = true
        console.log('Connected to SignalR hub')
      } catch (error) {
        console.error('Connection failed:', error)
      }
    },
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    clientCreated(clientData: any) {
      this.messages.push(`Client created: ${JSON.stringify(clientData)}`)
    },
    async disconnect() {
      if (this.connection) {
        await this.connection.stop()
        this.connection = null
        this.connected = false
        console.log('Disconnected from signalR hub')
      }
    },
  },
})
