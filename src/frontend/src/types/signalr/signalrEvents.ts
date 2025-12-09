import type { ClientResponse } from '@/types/client'

export const eventDefinitions = {
  ClientResponse: {} as ClientResponse,
} as const

export type SignalrEvents = {
  [K in keyof typeof eventDefinitions]: (typeof eventDefinitions)[K]
}

export type SignalrEventMap = typeof eventDefinitions
export type SignalrEventName = keyof SignalrEventMap
export type SignalREventHandler<K extends SignalrEventName = SignalrEventName> = (payload: SignalrEventMap[K]) => void

export const SignalrEventNames: Record<SignalrEventName, SignalrEventName> = Object.fromEntries(
  Object.keys(eventDefinitions).map((k) => [k, k])
) as Record<SignalrEventName, SignalrEventName>
