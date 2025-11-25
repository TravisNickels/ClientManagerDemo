<script setup lang="ts">
import { useClientStore } from '@/stores/clientStore'
import { computed, onMounted, ref, watch } from 'vue'
import ClientList from '@/components/client-list.vue'
import NewClientModal from '@/components/new-client-modal.vue'

const clientStore = useClientStore()
const showNewClientModal = ref<boolean>(false)
const storedValue = localStorage.getItem('showArchivedClients')
const showArchivedClients = ref(storedValue === 'true')

watch(showArchivedClients, (newValue) => {
  localStorage.setItem('showArchivedClients', String(newValue))
})

const filteredClients = computed(() => {
  return showArchivedClients.value ? clientStore.allClients : clientStore.getActiveClients
})

onMounted(async () => {
  await clientStore.updateClientsList()
})
</script>

<template>
  <div class="container mt-4">
    <div class="d-flex justify-content-between align-items-center mb-3">
      <h1>Clients</h1>
      <div class="form-check form-switch">
        <input class="form-check-input" type="checkbox" role="switch" v-model="showArchivedClients" />
        <label class="form-check-label">Show Archived</label>
      </div>
      <button class="btn btn-primary mt-3" @click="showNewClientModal = true">Create New Client</button>
    </div>
    <NewClientModal v-if="showNewClientModal" @created="clientStore.updateClientsList" @close="showNewClientModal = false" />
    <ClientList :clients="filteredClients" :loading="clientStore.isLoading" :show-archived-clients="false" @create="showNewClientModal = true" />
  </div>
</template>
