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
  <div v-if="props.loading" class="p-3">
    <div v-for="n in 6" :key="n" class="skeleton-row"></div>
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

  <!-- Rows -->
  <div v-else class="list-wrapper">
    <div v-for="client in props.clients" :key="client.id" class="client-row" :class="{ 'client-archived': client.isArchived }">
      <!-- Left content -->
      <div class="d-flex align-items-center gap-2 flex-grow-1 overflow-hidden">
        <router-link :to="{ name: 'client-management', params: { id: client.id } }" class="client-name text-truncate">
          {{ client.firstName }} {{ client.lastName }}
        </router-link>

        <span class="badge rounded-pill status-badge" :class="client.isArchived ? 'bg-secondary' : 'bg-primary'">
          {{ client.isArchived ? 'Archived' : 'Active' }}
        </span>
      </div>

      <!-- Actions -->
      <div class="actions d-flex align-items-center gap-2">
        <button class="btn btn-sm btn-outline-primary" @click.stop="emit('edit', client)">Edit</button>
        <button v-if="!client.isArchived" class="btn btn-sm btn-outline-secondary" @click.stop="emit('archive', client, true)">Archive</button>
        <button v-if="client.isArchived" class="btn btn-sm btn-outline-success" @click.stop="emit('archive', client, false)">Activate</button>
        <button class="btn btn-sm btn-outline-danger" @click.stop="emit('delete', client)">Delete</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.list-wrapper {
  padding: 14px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

/* Material-inspired row */
.client-row {
  background: #fff;
  border-radius: 12px;
  padding: 14px 18px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
  transition:
    box-shadow 0.23s ease,
    transform 0.15s ease;
}

.client-row:hover {
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.12);
  transform: translateY(-1px);
}

/* Name */
.client-name {
  font-size: 1rem;
  font-weight: 600;
  color: #222;
  text-decoration: none;
}

.client-name:hover {
  text-decoration: underline;
}

.client-archived {
  opacity: 0.7;
  filter: grayscale(30%);
  background: #dedede;
}

/* Status badge */
.status-badge {
  font-size: 0.74rem;
  padding: 4px 10px;
}

/* Hover-only actions */
.actions {
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.2s ease;
}

.client-row:hover .actions {
  opacity: 1;
  pointer-events: auto;
}

/* Skeletons */
.skeleton-row {
  height: 52px;
  border-radius: 12px;
  background: #eef1f4;
  margin-bottom: 10px;
  animation: pulse 1.2s infinite ease-in-out;
}

@keyframes pulse {
  0% {
    opacity: 0.5;
  }
  50% {
    opacity: 1;
  }
  100% {
    opacity: 0.5;
  }
}

/* Empty state */
.empty-state {
  text-align: center;
  padding: 60px 20px;
}

.empty-state .emoji {
  font-size: 50px;
  margin-bottom: 16px;
}
</style>
