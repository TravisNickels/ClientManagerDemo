<script setup lang="ts">
import { ref, onMounted, type Ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { UpdateClientRequest as Client, UpdateClientRequest } from '@/types/client'
import { useClientStore } from '@/stores/clientStore'
import type { UpdatePhoneRequest } from '@/types/phone'

const route = useRoute()
const router = useRouter()
const clientId = route.params.id as string
const clientStore = useClientStore()
const emptyGuid = '00000000-0000-0000-0000-000000000000'

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

const addPhone = (): void => {
  if (client.value) {
    const newPhone: UpdatePhoneRequest = {
      id: emptyGuid,
      clientId: client.value.id,
      phoneNumber: '',
      phoneType: '',
    }
    if (!client.value.phones) {
      client.value.phones = []
    }
    client.value.phones.push(newPhone)
  }
}

const removePhone = (index: number): void => {
  if (client.value) {
    if (!client.value.phones) {
      client.value.phones = []
    }
    client.value.phones.splice(index, 1)
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
    await clientStore.updateClientRequest(updatedClient)
    router.push({ name: 'client-dashboard' })
  } catch (error) {
    console.error('Failed to save client', error)
    alert('Could not save client data')
  }
}

onMounted(loadClient)
</script>

<template>
  <div v-if="client" class="container my-4">
    <div class="card shadow-sm">
      <div class="card-body">
        <h2 class="card-title mb-4">Edit Client</h2>
        <form @submit.prevent="saveClient">
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

          <!-- Email -->
          <div class="mb-3">
            <label>Email</label>
            <input v-model="client.email" required class="form-control" />
          </div>

          <div class="mb-3">
            <!-- Phone numbers -->
            <label class="form-label">Phone Numbers</label>
            <div v-for="(phone, index) in client.phones" :key="index" class="border rounded p-3 mb-2 bg-light">
              <div class="row g-2 align-items-center">
                <!-- Phone -->
                <div class="col-md-7">
                  <label class="form-label">Phone {{ index + 1 }}</label>
                  <input v-model="phone.phoneNumber" placeholder="Number" class="form-control" />
                </div>
                <!-- Phone type -->
                <div class="col-md-4">
                  <label class="form-label">Type</label>
                  <input v-model="phone.phoneType" placeholder="mobile/work/home" class="form-control" />
                </div>
                <div class="col-md-1 text-end">
                  <button type="button" class="btn btn-outline-danger btn-sm mt-4" @click="removePhone(index)">&times;</button>
                </div>
              </div>
            </div>
            <div>
              <button type="button" class="btn btn-outline-primary btn-sm mt-3" @click="addPhone">Add Phone</button>
            </div>
          </div>

          <div class="d-flex justify-content-between">
            <div class="d-flex justify-content-start gap-2 mt-4">
              <button type="button" class="btn btn-danger" @click="deleteClient">Delete</button>
            </div>

            <div class="d-flex justify-content-end gap-2 mt-4">
              <button type="submit" class="btn btn-success">Save</button>
              <button type="button" class="btn btn-warning" @click="archiveClient(true)" v-if="!client.isArchived">Archive</button>
              <button type="button" class="btn btn-warning" @click="archiveClient(false)" v-if="client.isArchived">UnArchive</button>
              <button type="button" class="btn btn-secondary" @click="$router.push({ name: 'client-dashboard' })">Cancel</button>
            </div>
          </div>
        </form>
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
