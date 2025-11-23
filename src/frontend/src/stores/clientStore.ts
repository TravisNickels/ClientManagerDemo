import { defineStore } from 'pinia'
import { computed, ref, watch } from 'vue'
import axios from 'axios'
import type { Client, CreateClientRequest, UpdateClientRequest } from '@/types/client'
import { useSignalRStore } from '@/stores/signalrStore'
import { SignalrEventNames } from '@/signalr/events'

export const useClientStore = defineStore('clientStore', () => {
  const apiConnection = axios.create({ baseURL: 'http://localhost:5200' })
  const signalR = useSignalRStore()
  const allClients = ref<Client[]>([])
  const showArchivedClients = ref<boolean>(false)
  const getActiveClients = computed<Client[]>(() => allClients.value.filter((client) => !client.isArchived))

  signalR.on(SignalrEventNames.ClientResponse, async () => {
    console.log('[SignalR] Client update event received — refreshing client list.')
    await updateClientsList()
  })

  const updateClientsList = async (): Promise<void> => {
    const response: { data: Client[] } = await apiConnection.get<Client[]>('/api/client')
    allClients.value = response.data
  }

  const createClientRequest = async (newClient: CreateClientRequest): Promise<Client> => {
    const response: { data: Client } = await apiConnection.post<Client>('/api/client', newClient)
    return response.data
  }

  const getClient = async (clientId: string): Promise<Client> => {
    const response: { data: Client } = await apiConnection.get<Client>(`/api/client/${clientId}`)
    return response.data
  }

  const archiveClient = async (clientId: string): Promise<void> => {
    await apiConnection.patch(`/api/client/archive/${clientId}`)
  }

  const unArchiveClient = async (clientId: string): Promise<void> => {
    await apiConnection.patch(`/api/client/unarchive/${clientId}`)
  }

  const updateClientRequest = async (updatedClient: UpdateClientRequest): Promise<void> => {
    await apiConnection.put(`/api/client/${updatedClient.id}`, updatedClient)
  }

  watch(showArchivedClients, updateClientsList)

  return {
    allClients,
    showArchivedClients,
    getActiveClients,
    updateClientsList,
    createClientRequest,
    archiveClient,
    unArchiveClient,
    updateClientRequest,
    getClient,
  }
})
