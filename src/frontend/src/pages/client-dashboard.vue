<script setup lang="ts">
import { useClientStore } from '@/stores/clientStore'
import { onMounted, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import ClientList from '@/components/client-list.vue'
import NewClientModal from '@/components/new-client-modal.vue'
import ClientHeader from '@/components/client-header.vue'
import type { Client } from '@/types/client'
import { confirm } from '@/composables/useConfirm'
import { toast } from 'vue3-toastify'
import Cards from '@/components/client-cards.vue'

const clientStore = useClientStore()
const router = useRouter()
const showNewClientModal = ref<boolean>(false)
const showArchive = ref<boolean>(false)
const visibleClients = ref<Client[]>([])
const storedValue = localStorage.getItem('showArchivedClients')
const showArchivedClients = ref(storedValue === 'true')
const storedShowGrid = ref<boolean>(localStorage.getItem('showGrid') === 'true')
const showGrid = ref<boolean>(storedShowGrid.value)

watch(showArchivedClients, (newValue) => {
  localStorage.setItem('showArchivedClients', String(newValue))
})

function goToEdit(client: Client) {
  router.push({ name: 'client-management', params: { id: client.id } })
}

async function toggleArchive(client: Client, archive: boolean) {
  if (!client) return
  if (archive) if (!(await confirmArchive())) return
  try {
    if (archive) await clientStore.sendArchiveClientRequest(client.id)
    else await clientStore.sendUnArchiveClientRequest(client.id)

    router.push({ name: 'client-dashboard' })
  } catch (error) {
    console.error('Failed to change archive status', error)
    toast.error('Could not change archive status of client')
  }
}

async function deleteClient(client: Client) {
  if (!(await confirmDelete())) return
  try {
    await clientStore.sendDeleteClientRequest(client.id)
  } catch (error) {
    console.error(`Failed to delete client`, error)
    toast.error('Could not delete client')
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

function updateVisibleClients(clients: Client[], showingArchive: boolean) {
  visibleClients.value = clients
  showArchive.value = showingArchive
}

watch(showGrid, () => {
  localStorage.setItem('showGrid', showGrid.value ? 'true' : 'false')
})

onMounted(async () => {
  await clientStore.updateClientsList()
})
</script>

<template>
  <div class="container mt-4">
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h1>Clients</h1>
      <div class="btn-group">
        <button class="btn btn-outline-secondary" :class="{ active: showGrid }" @click="showGrid = true">
          <i class="bi bi-list"></i>
        </button>

        <button class="btn btn-outline-secondary" :class="{ active: !showGrid }" @click="showGrid = false">
          <i class="bi bi-grid-3x3-gap-fill"></i>
        </button>
      </div>
    </div>
    <ClientHeader :clients="clientStore.allClients" @update="updateVisibleClients" @create="showNewClientModal = true"></ClientHeader>
    <NewClientModal v-if="showNewClientModal" @created="clientStore.updateClientsList" @close="showNewClientModal = false" />
    <ClientList
      v-if="showGrid"
      :clients="visibleClients"
      :loading="clientStore.isLoading"
      :show-archived-clients="showArchive"
      @create="showNewClientModal = true"
      @edit="goToEdit"
      @archive="toggleArchive"
      @delete="deleteClient"
    />
    <Cards
      v-if="!showGrid"
      :clients="visibleClients"
      :loading="clientStore.isLoading"
      :show-archived-clients="showArchive"
      @create="showNewClientModal = true"
      @edit="goToEdit"
      @archive="toggleArchive"
      @delete="deleteClient"
    />
  </div>
</template>
