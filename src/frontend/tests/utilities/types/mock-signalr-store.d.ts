import type { SignalrEventName, SignalrEventMap } from '@/types/signalr/signalrEvents'

declare module '@/stores/signalr-store' {
  interface SignalRStoreInstance {
    emit<K extends SignalrEventName>(event: K, payload: SignalrEventMap[K]): void
  }
}
