<script setup lang="ts">
import { useClientStore } from '@/stores/clientStore'
import { computed, onMounted, ref, watch } from 'vue'
import ClientList from '@/components/client-list.vue'

const clientStore = useClientStore()
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
    </div>
    <ClientList :clients="filteredClients" :show-archived-clients="false" />
  </div>
</template>
