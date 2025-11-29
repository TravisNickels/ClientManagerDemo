<script setup lang="ts">
import { onMounted } from 'vue'
import { useSignalRStore } from './stores/signalr-store'
import ConfirmationModal from './components/confirmation-modal.vue'
import SignalRBadge from '@/components/signalr-badge.vue'

const signalRStore = useSignalRStore()

onMounted(async () => {
  await signalRStore.connect()
})
</script>

<template>
  <div id="app">
    <header class="p-3 border-bottom d-flex justify-content-between">
      <div>Client Management Demo</div>
      <div><SignalRBadge /></div>
    </header>
    <RouterView />
    <ConfirmationModal />
  </div>
</template>

<style scoped></style>
