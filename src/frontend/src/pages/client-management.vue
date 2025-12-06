<script setup lang="ts">
import { ref, onMounted, type Ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { UpdateClientRequest as Client, UpdateClientRequest } from '@/types/client'
import { useClientStore } from '@/stores/client-store'
import ClientForm from '@/components/client-form.vue'
import { toast } from 'vue3-toastify'
import { confirm } from '@/composables/useConfirm'

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
    toast.error('Could not load client data')
    goHome()
  }
}

const deleteClient = async (): Promise<void> => {
  if (!client.value) return
  if (!(await confirmDelete())) return
  try {
    await clientStore.sendDeleteClientRequest(client.value.id)
    goHome()
  } catch (error) {
    console.error('Failed to delete client', error)
    toast.error('Could not delete client')
  }
}

const archiveClient = async (archive: boolean): Promise<void> => {
  if (!client.value) return
  if (archive) if (!(await confirmArchive())) return
  try {
    await clientStore.toggleArchiveClient(client.value.id, archive)
    goHome()
  } catch (error) {
    console.error('Failed to change archive status', error)
    toast.error('Could not change archive status of client')
  }
}

async function confirmDelete() {
  return await confirm({
    title: 'Delete Client',
    message: 'Are you sure you want to delete this client?',
    confirmText: 'Delete',
    cancelText: 'Cancel',
  })
}

async function confirmArchive() {
  return await confirm({
    title: 'Archive Client',
    message: 'Are you sure you want to archive this client?',
    confirmText: 'Archive',
    cancelText: 'Cancel',
  })
}

const saveClient = async (): Promise<void> => {
  if (!client.value) return

  const updatedClient: UpdateClientRequest = {
    id: client.value.id,
    firstName: client.value.firstName,
    lastName: client.value.lastName,
    email: client.value.email,
    isArchived: client.value.isArchived,
    phones: client.value.phones?.map((p) => {
      return {
        id: !p.id ? undefined : p.id,
        clientId: client.value!.id,
        phoneNumber: p.phoneNumber,
        phoneType: p.phoneType,
      }
    }),
  }

  try {
    saving.value = true

    await toast.promise(
      clientStore.sendUpdateClientRequest(updatedClient),
      {
        pending: { render: 'Sending client update request...' },
        success: { render: 'Client update request sent!' },
        error: { render: 'Failed to send client update request' },
      },
      {
        position: 'bottom-right',
      }
    )
    goHome()
  } catch (error) {
    console.error('Failed to save client', error)
    toast.error('Could not save client data')
  } finally {
    saving.value = false
  }
}

function goHome() {
  router.push({ name: 'client-dashboard' })
}

onMounted(loadClient)
</script>

<template>
  <div v-if="client" class="container-lg my-4">
    <!-- <Breadcrumbs /> -->
    <nav aria-label="breadcrumb">
      <ol class="breadcrumb">
        <li class="breadcrumb-item">
          <router-link class="text-decoration-none" :to="{ name: 'client-dashboard' }">Clients </router-link>
        </li>
        <li class="breadcrumb-item active" aria-current="page">{{ client.firstName }} {{ client.lastName }}</li>
      </ol>
    </nav>
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
