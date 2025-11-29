import type { CreatePhoneRequest, Phone } from './phone'

export interface Client {
  id: string
  firstName: string
  lastName: string
  email: string
  isArchived: boolean
  phones?: Phone[]
}

export interface CreateClientRequest {
  firstName: string
  lastName: string
  email: string
  isArchived: boolean
  phones?: CreatePhoneRequest[]
}

export type UpdateClientRequest = Omit<Client, 'phones'> & { phones?: (CreatePhoneRequest | UpdatePhoneRequest)[] }
export type ClientResponse = Client
