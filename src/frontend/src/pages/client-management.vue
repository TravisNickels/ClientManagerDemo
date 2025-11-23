<script setup lang="ts">
import { ref, onMounted, type Ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { Client } from '@/types/client'
import { useClientStore } from '@/stores/clientStore'

const route = useRoute()
const router = useRouter()
const clientId = route.params.id as string
const clientStore = useClientStore()

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

onMounted(loadClient)
</script>

<template>
  <div v-if="client" class="container my-4">
    <div class="card shadow-sm">
      <div class="card-body">
        <h2 class="card-title mb-4">Edit Client</h2>
        <!-- <form @submit.prevent="saveClient"> -->
        <!-- First name -->
        <div class="mb-3">
          <label>First Name</label>
          <input v-model="client.firstName" required class="form-control" />
        </div>

        <!-- Last name -->
        <div class="mb-3">
          <label>Last Name</label>
          <input v-model="client.lastName" required class="form-control" />
        </div>

        <div class="d-flex justify-content-end gap-2 mt-4">
          <button type="submit" class="btn btn-success">Save</button>
          <button type="button" class="btn btn-warning" @click="archiveClient(true)" v-if="!client.isArchived">Archive</button>
          <button type="button" class="btn btn-warning" @click="archiveClient(false)" v-if="client.isArchived">UnArchive</button>
          <button type="button" class="btn btn-secondary" @click="$router.push({ name: 'client-dashboard' })">Cancel</button>
        </div>
        <!-- </form> -->
      </div>
    </div>
  </div>
</template>

<style scoped>
.phone-entry {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}
</style>
