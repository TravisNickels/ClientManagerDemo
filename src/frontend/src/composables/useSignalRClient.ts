import * as signalR from '@microsoft/signalr'
import type { SignalrEvents } from '@/types/signalr/signalrEvents'

type Handler<K extends keyof SignalrEvents> = (payload: SignalrEvents[K]) => void
export type SignalRClient = ReturnType<typeof signalrClient>

export type LifecycleEvent =
  | { type: 'connected' }
  | { type: 'connecting' }
  | { type: 'disconnected'; error?: unknown }
  | { type: 'reconnecting'; attempt: number }
  | { type: 'permanently-disconnected' }

let _instance: ReturnType<typeof signalrClient> | null = null

export function useSignalRClient(url: string = 'http://localhost:5200/notifications'): ReturnType<typeof signalrClient> {
  if (_instance) return _instance
  _instance = signalrClient(url)
  return _instance
}

function signalrClient(url: string) {
  let connection: signalR.HubConnection | null = null
  let attempt: number = 0
  let reconnectTimer: number | null = null
  let manualDisconnect: boolean = false
  const maxAttempts: number = 8
  const listeners: { [K in keyof SignalrEvents]?: Handler<K>[] } = {}
  const lifecycleHandlers: ((event: LifecycleEvent) => void)[] = []

  function emit(event: LifecycleEvent) {
    for (const handler of lifecycleHandlers) handler(event)
  }

  // -------------------------------------------------------
  //   Build the SignalR connection instance
  // -------------------------------------------------------
  function createConnection() {
    if (connection) return

    const hub = new signalR.HubConnectionBuilder().withUrl(url).withAutomaticReconnect([]).configureLogging(signalR.LogLevel.Information).build()
    manualDisconnect = false

    hub.onreconnected(() => {
      attempt = 0
      registerListeners()
      emit({ type: 'connected' })
    })

    hub.onclose(async (error) => {
      emit({ type: 'disconnected', error: error })
      if (manualDisconnect) return

      scheduleReconnect()
    })

    connection = hub
  }

  function registerListeners(): void {
    if (!connection) return
    ;(Object.keys(listeners) as (keyof SignalrEvents)[]).forEach((eventName) => {
      listeners[eventName]?.forEach((handler) => {
        connection!.on(eventName, handler)
      })
    })
  }

  // -------------------------------------------------------
  //   Public Methods
  // -------------------------------------------------------
  function onLifecycle(cb: (event: LifecycleEvent) => void): void {
    lifecycleHandlers.push(cb)
  }

  async function connect(): Promise<void> {
    createConnection()
    if (!connection) return

    emit({ type: 'connecting' })

    try {
      await connection.start()
      attempt = 0
      registerListeners()
      emit({ type: 'connected' })
    } catch (error) {
      console.warn('[SignalR] Connection failed:', error)
      scheduleReconnect()
    }
  }

  async function manualReconnect() {
    if (reconnectTimer) {
      clearTimeout(reconnectTimer)
      reconnectTimer = null
    }

    attempt = 0

    await connect()
  }

  function on<K extends keyof SignalrEvents>(eventName: K, handler: Handler<K>) {
    if (!listeners[eventName]) {
      listeners[eventName] = []
    }
    listeners[eventName]!.push(handler)

    if (connection) {
      connection.on(eventName, handler)
    }
  }

  async function disconnect() {
    manualDisconnect = true
    if (reconnectTimer) {
      clearTimeout(reconnectTimer)
      reconnectTimer = null
    }

    if (connection) {
      await connection.stop()
      console.warn('[SignalR] Disconnected')
      connection = null
    }
  }

  // -------------------------------------------------------
  //   Rescheduling
  // -------------------------------------------------------
  function backOff(attempt: number): number {
    const base = Math.min(30000, Math.pow(2, attempt) * 250)
    const jitter = Math.floor(Math.random() * 250)
    return base + jitter
  }

  function scheduleReconnect() {
    if (attempt >= maxAttempts) {
      emit({ type: 'permanently-disconnected' })
      return
    }

    attempt += 1
    emit({ type: 'reconnecting', attempt: attempt })

    const delay = backOff(attempt)

    reconnectTimer = window.setTimeout(async () => {
      await connect()
      reconnectTimer = null
    }, delay)
  }

  // -------------------------------------------------------
  //   Public API
  // -------------------------------------------------------
  return { connect, onLifecycle, manualReconnect, on, disconnect }
}
