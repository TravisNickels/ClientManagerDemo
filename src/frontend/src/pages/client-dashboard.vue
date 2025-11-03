<script setup lang="ts">
import { useSignalRStore } from '@/stores/signalrStore'
import { onMounted } from 'vue'

const signalR = useSignalRStore()

const clients: string[] = signalR.messages

onMounted(() => {
  signalR.connect()
})
</script>

<template>
  <div>Testsing</div>
  <div v-for="client in clients" v-bind:key="client">
    <div>{{ client }}</div>
  </div>
</template>
