<script setup lang="ts">
import type { Client } from '@/types/client'

const props = defineProps<{
  clients: Client[]
  showArchivedClients: boolean
}>()
</script>

<template>
  <ul class="list-group">
    <li v-for="client in props.clients" :key="client.id" class="list-group-item d-flex justify-content-between align-items-center">
      {{ client.firstName }} {{ client.lastName }}
      <span v-if="!client.isArchived" class="badge bg-primary rounded-pill">Active</span>
      <span v-if="client.isArchived" class="badge bg-secondary rounded-pill">Archived</span>
    </li>
  </ul>
</template>
