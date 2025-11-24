<script setup lang="ts">
import type { Client, CreateClientRequest } from '@/types/client'
import type { CreatePhoneRequest } from '@/types/phone'
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
  isArchived: false,
  phones: [],
})

const addingPhone = (): void => {
  const newPhone: CreatePhoneRequest = {
    phoneNumber: '',
    phoneType: '',
  }
  form.value.phones?.push(newPhone)
}

const removePhone = (index: number): void => {
  if (form.value.phones === null) return
  if (form.value.phones!.length > 0) form.value.phones!.splice(index, 1)
}

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

            <!-- Phone numbers -->
            <label class="form-label">Phone Numbers</label>
            <div class="mb-3">
              <div v-for="(phone, index) in form.phones" :key="index" class="border rounded p-2 mb-2 bg-light">
                <div class="row g-2 align-items-center">
                  <div class="col-md-6">
                    <label class="form-label">Phone {{ index + 1 }}</label>
                    <input v-model="phone.phoneNumber" placeholder="123-456-7890" class="form-control" />
                  </div>
                  <div class="col-md-5">
                    <label class="form-label">Type</label>
                    <input v-model="phone.phoneType" placeholder="mobile/work/home" class="form-control" />
                  </div>
                  <div class="col-md-1 text-end">
                    <button type="button" class="btn btn-outline-danger btn-sm" @click="removePhone(index)">&times;</button>
                  </div>
                </div>
              </div>

              <button type="button" class="btn btn-outline-primary btn-sm" @click="addingPhone">Add Phone</button>
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
