<script setup lang="ts">
import { computed } from 'vue'
import { useSignalRStore } from '@/stores/signalr-store'

const store = useSignalRStore()

const color = computed(() => {
  switch (store.status) {
    case 'connected':
      return 'bg-success'
    case 'connecting':
      return 'bg-warning'
    case 'reconnecting':
      return 'bg-info'
    default:
      return 'bg-secondary'
  }
})

const label = computed(() => {
  switch (store.status) {
    case 'connected':
      return 'Connected'
    case 'connecting':
      return 'Connecting'
    case 'reconnecting':
      return 'Reconnecting'
    default:
      return 'Disconnected'
  }
})

async function onManualReconnect() {
  await store.manualReconnect()
}

async function onManualDisconnect() {
  await store.disconnect()
}
</script>

<template>
  <div class="d-flex align-items-center gap-2">
    <div class="signalr-indicator" :class="color">
      <span v-if="store.isReconnecting" class="attempt"> {{ store.reconnectAttempt }}/8 </span>
    </div>
    <small class="text-muted" v-if="store.status === 'connected'">
      SignalR {{ label }} • {{ store.lastSeen?.toLocaleTimeString() }}
      <button class="btn btn-link btn-sm" @click="onManualDisconnect">Disconnect</button>
    </small>
    <small class="text-muted" v-else>
      SignalR {{ label }}
      <button class="btn btn-link btn-sm" v-if="store.status === 'disconnected'" @click="onManualReconnect">Reconnect</button>
    </small>
  </div>
</template>

<style scoped>
.signalr-indicator {
  width: 24px;
  height: 18px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.7rem;
  font-weight: 600;
  color: white;
}
.signalr-indicator .attempt {
  line-height: 1;
}

.signalr-indicator.bg-info {
  animation: pulse 0.6s infinite alternate;
}

@keyframes pulse {
  from {
    transform: scale(1);
  }
  to {
    transform: scale(1.15);
  }
}
</style>
