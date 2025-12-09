import { type SignalRClient } from '@/composables/useSignalRClient'
import { useSignalRClient } from '@/composables/useSignalRClient'

let providedClient: SignalRClient | null = null

export function provideSignalRClientForTests(mock: SignalRClient) {
  providedClient = mock
}

export function useProvidedSignalRClient(): SignalRClient {
  return providedClient ?? useSignalRClient()
}
