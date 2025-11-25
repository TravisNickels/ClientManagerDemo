<script setup lang="ts">
import { onMounted } from 'vue'
import { useSignalRStore } from './stores/signalr-store'

const signalRStore = useSignalRStore()

onMounted(async () => {
  await signalRStore.connect()
})
</script>

<template>
  <div id="app">
    <div id="nav">
      <router-link to="/">Home</router-link>
      |
      <router-link to="/client-dashboard">Dashboard</router-link>
    </div>
    <RouterView />
  </div>
</template>

<style scoped></style>
