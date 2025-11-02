<script setup lang="ts"></script>

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
