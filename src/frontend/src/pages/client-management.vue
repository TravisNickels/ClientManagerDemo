<script setup lang="ts">
import { ref, onMounted, type Ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { UpdateClientRequest as Client, UpdateClientRequest } from '@/types/client'
import { useClientStore } from '@/stores/clientStore'
import ClientForm from '@/components/client-form.vue'

const route = useRoute()
const router = useRouter()
const clientId = route.params.id as string
const clientStore = useClientStore()
const saving = ref<boolean>(false)

const client: Ref<Client | null> = ref(null)

const loadClient = async (): Promise<void> => {
  try {
    client.value = await clientStore.getClient(clientId)
  } catch (error) {
    console.error('Failed to load client', error)
    alert('Could not load client data')
    router.push({ name: 'client-dashboard' })
  }
}

const deleteClient = async (): Promise<void> => {
  if (!client.value) return
  try {
    await clientStore.deleteClientRequest(client.value.id)
    router.push({ name: 'client-dashboard' })
  } catch (error) {
    console.error('Failed to delete client', error)
    alert('Could not delete client')
  }
}

const archiveClient = async (archive: boolean): Promise<void> => {
  if (!client.value) return
  try {
    if (archive) await clientStore.archiveClient(client.value.id)
    else await clientStore.unArchiveClient(client.value.id)

    router.push({ name: 'client-dashboard' })
  } catch (error) {
    console.error('Failed to change archive status', error)
    alert('Could not change archive status of client')
  }
}

const saveClient = async (): Promise<void> => {
  if (!client.value) return

  const updatedClient: UpdateClientRequest = {
    id: client.value.id,
    firstName: client.value.firstName,
    lastName: client.value.lastName,
    email: client.value.email,
    isArchived: client.value.isArchived,
    phones: client.value.phones || [],
  }

  try {
    saving.value = true
    await clientStore.updateClientRequest(updatedClient)
    router.push({ name: 'client-dashboard' })
  } catch (error) {
    console.error('Failed to save client', error)
    alert('Could not save client data')
  } finally {
    saving.value = false
  }
}

onMounted(loadClient)
</script>

<template>
  <div v-if="client" class="container my-4">
    <div class="card shadow-sm">
      <div class="card-body">
        <h2 class="card-title mb-4">Edit Client</h2>
        <ClientForm
          :client="client"
          mode="edit"
          :saving="saving"
          @submit="saveClient"
          @archive="archiveClient"
          @delete="deleteClient"
          @cancel="$router.push({ name: 'client-dashboard' })"
        />
      </div>
    </div>
  </div>
</template>
