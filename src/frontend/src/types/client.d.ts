export interface Client {
  id: string
  firstName: string
  lastName: string
  email: string
  isArchived: boolean
}

export type CreateClientRequest = Omit<Client, 'id'>
export type UpdateClientRequest = Client
export type ClientResponse = Client
