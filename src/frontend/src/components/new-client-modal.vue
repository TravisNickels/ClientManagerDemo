<script setup lang="ts">
import type { Client, CreateClientRequest } from '@/types/client'
import { reactive, ref } from 'vue'
import { useClientStore } from '@/stores/clientStore'
import ClientForm from '@/components/client-form.vue'
import { toast } from 'vue3-toastify'

const clientStore = useClientStore()
const saving = ref<boolean>(false)

const emits = defineEmits<{
  (e: 'created', client: Client): void
  (e: 'close'): void
}>()

const form = reactive<Client>({
  id: '',
  firstName: '',
  lastName: '',
  email: '',
  isArchived: false,
  phones: [],
})

function createClientRequestFromForm(): CreateClientRequest {
  return {
    firstName: form.firstName,
    lastName: form.lastName,
    email: form.email,
    isArchived: form.isArchived,
    phones:
      form.phones?.map((phone) => ({
        phoneNumber: phone.phoneNumber,
        phoneType: phone.phoneType,
      })) || [],
  }
}

const submit = async (): Promise<void> => {
  saving.value = true
  try {
    const formRequest = createClientRequestFromForm()
    const client = await toast.promise(clientStore.sendCreateClientRequest(formRequest), {
      pending: { render: 'Send create client request...' },
      success: { render: 'Create client request sent!', autoClose: 2000, position: 'top-right' },
      error: { render: 'Failed to send create client request' },
    })
    emits('created', client)
    emits('close')
  } catch (err: unknown) {
    if (err instanceof Error) {
      console.error('Failed to create client', err.message)
    } else {
      console.error('Failed to create client', err)
    }
    alert('Could not create client')
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <div class="modal fade show d-block backdrop" tabindex="-1" style="background-color: rgba(0, 0, 0, 0.5)">
    <div class="modal-dialog modal-dialog-centered modal-lg">
      <div class="modal-content shadow">
        <!-- Header -->
        <div class="modal-header">
          <h5 class="modal-title">Create New Client</h5>
          <button type="button" class="btn-close" @click="$emit('close')"></button>
        </div>

        <!-- Body -->
        <div class="modal-body">
          <ClientForm :client="form" mode="create" :saving="saving" @submit="submit" @cancel="$emit('close')" />
        </div>
      </div>
    </div>
  </div>
</template>
