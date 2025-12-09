vi.unmock('@/stores/signalr-store')

import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useSignalRStore } from '@/stores/signalr-store'
import { createPinia, setActivePinia } from 'pinia'
import { provideSignalRClientForTests } from '@/composables/signalr-client-provider'

const manualReconnectMock = vi.fn()
const connectMock = vi.fn(async () => {})
const disconnectMock = vi.fn()
const onMock = vi.fn()
const onLifecycleMock = vi.fn()
provideSignalRClientForTests({
  manualReconnect: manualReconnectMock,
  connect: connectMock,
  disconnect: disconnectMock,
  on: onMock,
  onLifecycle: onLifecycleMock,
})

describe('Unit Tests - SignalR Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('updates state on connected event', async () => {
    const store = useSignalRStore()

    if (onLifecycleMock.mock.calls[0] === undefined) return
    const handler = onLifecycleMock.mock.calls[0][0]
    handler({ type: 'connected' })

    await store.connect()

    expect(store.status).toBe('connected')
    expect(store.reconnectAttempt).toBe(0)
    expect(store.lastSeen).not.toBeNull()
  })

  it('increments reconnect count on reconnecting', () => {
    const store = useSignalRStore()
    if (onLifecycleMock.mock.calls[0] === undefined) return
    const handler = onLifecycleMock.mock.calls[0][0]

    handler({ type: 'reconnecting', attempt: 3 })
    expect(store.status).toBe('reconnecting')
    expect(store.reconnectAttempt).toBe(3)
  })

  it('proxies event registration to the signalr client', () => {
    const store = useSignalRStore()

    const handler = vi.fn()
    store.on('ClientResponse', handler)

    expect(onMock).toHaveBeenCalledWith('ClientResponse', handler)
  })

  it('connect() calls underlying client', async () => {
    const store = useSignalRStore()
    await store.connect()
    expect(connectMock).toHaveBeenCalled()
  })

  it('disconnect() only runs when not disconnected', async () => {
    const store = useSignalRStore()

    await store.disconnect()
    expect(disconnectMock).not.toHaveBeenCalled()

    store.status = 'connected'
    await store.disconnect()
    expect(disconnectMock).toHaveBeenCalled()
  })

  it('manualReconnect() proxies', async () => {
    const manualReconnectMock = vi.fn().mockResolvedValue(undefined)

    provideSignalRClientForTests({
      manualReconnect: manualReconnectMock,
      connect: vi.fn(),
      disconnect: vi.fn(),
      on: vi.fn(),
      onLifecycle: vi.fn(),
    })
    const store = useSignalRStore()
    await store.manualReconnect()
    expect(manualReconnectMock).toHaveBeenCalled()
  })
})
