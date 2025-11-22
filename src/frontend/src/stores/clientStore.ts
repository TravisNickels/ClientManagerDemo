import { defineStore } from 'pinia'
import { computed, ref, watch } from 'vue'
import axios from 'axios'
import type { Client } from '@/types/client'
import { useSignalRStore } from '@/stores/signalrStore'
import { SignalrEventNames } from '@/signalr/events'

export const useClientStore = defineStore('clientStore', () => {
  const apiConnection = axios.create({ baseURL: 'http://localhost:5200' })
  const signalR = useSignalRStore()
  const allClients = ref<Client[]>([])
  const showArchivedClients = ref<boolean>(false)
  const getActiveClients = computed<Client[]>(() => allClients.value.filter((client) => !client.isArchived))

  async function updateClientsList(): Promise<void> {
    const response: { data: Client[] } = await apiConnection.get<Client[]>('/api/client')
    allClients.value = response.data
  }

  watch(showArchivedClients, updateClientsList)

  signalR.on(SignalrEventNames.ClientResponse, async () => {
    console.log('[SignalR] Client update event received — refreshing client list.')
    await updateClientsList()
  })

  return {
    allClients,
    showArchivedClients,
    getActiveClients,
    updateClientsList,
  }
})
