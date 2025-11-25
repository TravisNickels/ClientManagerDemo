<script setup lang="ts">
import type { Client } from '@/types/client'

const props = defineProps<{
  clients: Client[]
  showArchivedClients: boolean
  loading: boolean
}>()
</script>

<template>
  <!-- Loading -->
  <ul v-if="props.loading" class="list-group">
    <li v-for="n in 6" :key="n" class="list-group-item d-flex justify-content-between align-items-center">
      <div class="placeholder-wave w-50">
        <span class="placeholder col-8"></span>
      </div>
      <span class="placeholder col-2 rounded-pill"></span>
    </li>
  </ul>
  <!-- Client List -->
  <ul v-else class="list-group">
    <li v-for="client in props.clients" :key="client.id" class="list-group-item d-flex justify-content-between align-items-center">
      <router-link v-if="client.id" :to="{ name: 'client-management', params: { id: client.id } }" class="text-decoration-none">
        {{ client.firstName }} {{ client.lastName }}
      </router-link>
      <span v-if="!client.isArchived" class="badge bg-primary rounded-pill">Active</span>
      <span v-if="client.isArchived" class="badge bg-secondary rounded-pill">Archived</span>
    </li>
  </ul>
</template>
