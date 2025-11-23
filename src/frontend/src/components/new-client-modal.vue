<script setup lang="ts">
import type { Client, CreateClientRequest } from '@/types/client'
import { ref, type Ref } from 'vue'
import { useClientStore } from '@/stores/clientStore'

const clientStore = useClientStore()

const emits = defineEmits<{
  (e: 'created', client: Client): void
  (e: 'close'): void
}>()

const form: Ref<CreateClientRequest> = ref({
  firstName: '',
  lastName: '',
  email: '',
})

const submit = async (): Promise<void> => {
  try {
    const client = await clientStore.createClientRequest(form.value)
    emits('created', client)
    emits('close')
  } catch (err: unknown) {
    if (err instanceof Error) {
      console.error('Failed to create client', err.message)
    } else {
      console.error('Failed to create client', err)
    }
    alert('Could not create client')
  }
}
</script>

<template>
  <div class="modal fade show d-block backdrop" tabindex="-1" style="background-color: rgba(0, 0, 0, 0.5)">
    <div class="modal-dialog modal-dialog-centered">
      <div class="modal-content shadow">
        <!-- Header -->
        <div class="modal-header">
          <h5 class="modal-title">Create New Client</h5>
          <button type="button" class="btn-close" @click="$emit('close')"></button>
        </div>

        <!-- Body -->
        <div class="modal-body">
          <form @submit.prevent="submit" class="needs-validation" novalidate>
            <!-- First Name -->
            <div class="mb-3">
              <label class="form-label">First Name</label>
              <input v-model="form.firstName" type="text" class="form-control" required />
            </div>

            <!-- Last Name -->
            <div class="mb-3">
              <label class="form-label">Last Name</label>
              <input v-model="form.lastName" type="text" class="form-control" required />
            </div>

            <!-- email -->
            <div class="mb-3">
              <label class="form-label">Email</label>
              <input v-model="form.email" type="text" class="form-control" required />
            </div>

            <!-- Footer -->
            <div class="modal-footer">
              <button type="button" class="btn btn-secondary" @click="$emit('close')">Cancel</button>
              <button type="submit" class="btn btn-primary">Create</button>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>
