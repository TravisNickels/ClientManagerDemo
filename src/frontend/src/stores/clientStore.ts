import { defineStore } from 'pinia'
import { computed, ref, watch } from 'vue'
import axios from 'axios'
import type { Client, CreateClientRequest, UpdateClientRequest } from '@/types/client'
import { useSignalRStore } from '@/stores/signalr-store'
import { SignalrEventNames } from '@/signalr/events'

export const useClientStore = defineStore('clientStore', () => {
  const apiConnection = axios.create({ baseURL: 'http://localhost:5200' })
  const signalR = useSignalRStore()
  const allClients = ref<Client[]>([])
  const isLoading = ref<boolean>(false)
  const showArchivedClients = ref<boolean>(false)
  const getActiveClients = computed<Client[]>(() => allClients.value.filter((client) => !client.isArchived))

  signalR.on(SignalrEventNames.ClientResponse, async () => {
    console.log('[SignalR] Client update event received — refreshing client list.')
    await updateClientsList()
  })

  const updateClientsList = async (): Promise<void> => {
    try {
      isLoading.value = true

      //TODO: TEMP delay (remove later)
      await new Promise((resolve) => setTimeout(resolve, 1000))

      const response: { data: Client[] } = await apiConnection.get<Client[]>('/api/client')
      allClients.value = response.data
    } finally {
      isLoading.value = false
    }
  }

  const sendCreateClientRequest = async (newClient: CreateClientRequest): Promise<Client> => {
    //TODO: TEMP delay (remove later)
    await new Promise((resolve) => setTimeout(resolve, 1500))
    const response: { data: Client } = await apiConnection.post<Client>('/api/client', newClient)
    return response.data
  }

  const getClient = async (clientId: string): Promise<Client> => {
    const response: { data: Client } = await apiConnection.get<Client>(`/api/client/${clientId}`)
    return response.data
  }

  const sendArchiveClientRequest = async (clientId: string): Promise<void> => {
    await apiConnection.patch(`/api/client/archive/${clientId}`)
  }

  const sendUnArchiveClientRequest = async (clientId: string): Promise<void> => {
    await apiConnection.patch(`/api/client/unarchive/${clientId}`)
  }

  const sendUpdateClientRequest = async (updatedClient: UpdateClientRequest): Promise<void> => {
    await new Promise((resolve) => setTimeout(resolve, 1500))
    await apiConnection.put(`/api/client/${updatedClient.id}`, updatedClient)
  }

  const sendDeleteClientRequest = async (clientId: string): Promise<void> => {
    await apiConnection.delete(`/api/client/${clientId}`)
  }

  watch(showArchivedClients, updateClientsList)

  return {
    allClients,
    showArchivedClients,
    getActiveClients,
    isLoading,
    updateClientsList,
    sendCreateClientRequest,
    sendArchiveClientRequest,
    sendUnArchiveClientRequest,
    sendUpdateClientRequest,
    getClient,
    sendDeleteClientRequest,
  }
})
