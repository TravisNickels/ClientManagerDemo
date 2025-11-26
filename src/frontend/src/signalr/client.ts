import * as signalR from '@microsoft/signalr'
import { type SignalrEvents } from '@/signalr/events'
import { ref } from 'vue'

type Handler<K extends keyof SignalrEvents> = (payload: SignalrEvents[K]) => void
export type ConnectionState = 'connected' | 'connecting' | 'reconnecting' | 'disconnected'

export class SignalRClient {
  private connection: signalR.HubConnection | null = null
  private listeners: { [K in keyof SignalrEvents]?: Handler<K>[] } = {}
  private foo = ref<ConnectionState>('disconnected')

  constructor(private readonly url: string = 'http://localhost:5200/notifications') {}

  public async connect(): Promise<void> {
    if (this.connection) return

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.url)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build()

    this.connection.onreconnected(() => {
      console.log('[SignalR] Reconnected — restoring event handlers.')
      this.registerListeners()
    })

    this.connection.onclose(async (error) => {
      console.warn('[SignalR] connection closed', error)
    })

    try {
      await this.connection.start()
      console.log('[SignalR] Connected')
      this.registerListeners()
    } catch (error) {
      console.error('[SignalR] Connection failed:', error)
    }
  }

  private registerListeners(): void {
    if (!this.connection) return

    for (const eventName in this.listeners) {
      const handlers = this.listeners[eventName as keyof SignalrEvents]
      if (handlers) {
        handlers.forEach((handler) => {
          this.connection!.on(eventName, handler)
        })
      }
    }
  }

  public on<K extends keyof SignalrEvents>(eventName: K, handler: Handler<K>): void {
    if (!this.listeners[eventName]) this.listeners[eventName] = []

    this.listeners[eventName]!.push(handler)

    if (this.connection) {
      this.connection.on(eventName, handler)
    } else {
      console.warn(`[SignalR] Connection is not established. Cannot register handler for event: ${String(eventName)}`)
    }
  }

  public async disconnect(): Promise<void> {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
      console.log('[SignalR] Disconnected')
    }
  }
}
