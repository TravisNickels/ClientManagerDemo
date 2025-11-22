import type { ClientResponse } from '@/types/client'

export const eventDefinitions = {
  ClientResponse: {} as ClientResponse,
} as const

export type SignalrEvents = {
  [K in keyof typeof eventDefinitions]: (typeof eventDefinitions)[K]
}

export type SignalrEventName = keyof typeof eventDefinitions

export const SignalrEventNames: Record<SignalrEventName, SignalrEventName> = Object.fromEntries(
  Object.keys(eventDefinitions).map((k) => [k, k])
) as Record<SignalrEventName, SignalrEventName>
