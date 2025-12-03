<script setup lang="ts">
import type { Client } from '@/types/client'

const props = defineProps<{
  clients: Client[]
  showArchivedClients: boolean
  loading: boolean
}>()

const emit = defineEmits<{
  (e: 'create'): void
  (e: 'edit', client: Client): void
  (e: 'archive', client: Client, archive: boolean): void
  (e: 'delete', client: Client): void
}>()
</script>

<template>
  <!-- Loading -->
  <div v-if="props.loading" class="row g-3 my-2">
    <div v-for="n in 6" :key="n" class="col-md-6 col-lg-4">
      <div class="card shadow-sm placeholder-wave" style="height: 150px">
        <div class="card-body">
          <span class="placeholder col-6"></span>
          <span class="placeholder col-4"></span>
          <span class="placeholder col-8"></span>
        </div>
      </div>
    </div>
  </div>

  <!-- Empty -->
  <div v-else-if="props.clients.length === 0" class="text-center py-5 border rounded bg-light my-3">
    <div class="mb-3 fs-1 text-muted">👤</div>
    <h4 class="fw-semibold">{{ props.showArchivedClients ? 'No archived clients' : 'No clients yet' }}</h4>
    <p class="text-muted mb-4">
      {{ props.showArchivedClients ? 'Archived clients will appear here.' : 'Start by adding your first client.' }}
    </p>
    <button v-if="!showArchivedClients" class="btn btn-primary px-4" @click="emit('create')">Create Client</button>
  </div>

  <!-- Cards -->
  <div v-else class="row g-3 my-1">
    <div v-for="client in props.clients" :key="client.id" class="col-md-6 col-lg-4">
      <div class="card client-card shadow-sm" :class="{ 'client-archived': client.isArchived }">
        <div class="card-body">
          <div class="d-flex justify-content-between align-items-start mb-2">
            <router-link :to="{ name: 'client-management', params: { id: client.id } }" class="fw-semibold text-decoration-none text-dark">
              {{ client.firstName }} {{ client.lastName }}
            </router-link>

            <span class="badge" :class="client.isArchived ? 'bg-secondary' : 'bg-primary'">
              {{ client.isArchived ? 'Archived' : 'Active' }}
            </span>
          </div>

          <div class="text-muted small mb-2">📞 {{ client.phones?.find((c) => c.clientId === client.id)?.phoneNumber ?? '(no phone)' }}</div>
          <div class="text-muted small mb-3">✉️ {{ client.email ?? '(no email)' }}</div>

          <div class="d-flex align-items-center gap-2">
            <button class="btn btn-sm btn-outline-primary flex-grow-1" @click.stop="emit('edit', client)">Edit</button>
            <button v-if="!client.isArchived" class="btn btn-sm btn-outline-secondary flex-grow-1" @click.stop="emit('archive', client, true)">
              Archive
            </button>
            <button v-if="client.isArchived" class="btn btn-sm btn-outline-success flex-grow-1" @click.stop="emit('archive', client, false)">
              Activate
            </button>
            <!-- <button v-if="!client.isArchived" class="btn btn-sm btn-outline-warning flex-grow-1" @click.stop="emit('archive', client, true)">
              Archive
            </button> -->
            <button class="btn btn-sm btn-outline-danger flex-grow-1" @click.stop="emit('delete', client)">Delete</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.client-card {
  transition:
    transform 0.15s ease,
    box-shadow 0.15s ease;
  cursor: pointer;
}
.client-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 6px 12px rgba(0, 0, 0, 0.1);
}
.client-archived {
  opacity: 0.7;
  filter: grayscale(30%);
}
</style>
