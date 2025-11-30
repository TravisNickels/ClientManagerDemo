<script setup lang="ts">
import type { Client } from '@/types/client'
import type { Phone } from '@/types/phone'
import { vMaska } from 'maska/vue'
import { computed, reactive, ref, watch } from 'vue'

const props = defineProps<{
  client: Client
  mode: 'create' | 'edit'
  saving: boolean
}>()

const emits = defineEmits<{
  (e: 'submit', payload: Client): void
  (e: 'archive', archive: boolean): void
  (e: 'delete'): void
  (e: 'cancel'): void
}>()

const touched = ref<boolean>(false)
const formIsValid = ref<boolean>(false)
const form = props.client
const saving = computed(() => props.saving)

const errors = reactive({
  firstName: '',
  lastName: '',
  email: '',
})

watch(form, validate, { deep: true })

const phoneErrors = reactive<{ phoneNumber: string; phoneType: string }[]>([])

function validate(): void {
  let valid = true
  errors.firstName = ''
  errors.lastName = ''
  errors.email = ''
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

  if (!touched.value) return

  if (!form.firstName || form.firstName.trim().length < 2) {
    errors.firstName = 'First name must be at least 2 characters.'
    valid = false
  }
  if (!form.lastName || form.lastName.trim().length < 2) {
    errors.lastName = 'Last name must be at least 2 characters.'
    valid = false
  }
  if (!form.email || form.email.trim() === '') {
    errors.email = 'Email is required.'
    valid = false
  } else if (!emailRegex.test(form.email)) {
    errors.email = 'Email is invalid.'
    valid = false
  }

  phoneErrors.forEach((err) => {
    err.phoneNumber = ''
    err.phoneType = ''
  })

  form.phones?.forEach((phone, index) => {
    if (!phoneErrors[index]) {
      phoneErrors[index] = { phoneNumber: '', phoneType: '' }
    }

    if (!phone.phoneNumber || phone.phoneNumber.trim() === '') {
      phoneErrors[index].phoneNumber = 'Phone number is required.'
      valid = false
    }

    if (!phone.phoneType || phone.phoneType.trim() === '') {
      phoneErrors[index].phoneType = 'Phone type is required.'
      valid = false
    }

    const cleaned = phone.phoneNumber.replace(/[^0-9]/g, '')
    if (cleaned.length < 10) {
      phoneErrors[index].phoneNumber = 'Phone number must be at least 10 digits.'
      valid = false
    }
  })

  formIsValid.value = valid
}

const addingPhone = (): void => {
  const newPhone: Phone = {
    id: '',
    clientId: '',
    phoneNumber: '',
    phoneType: '',
  }
  form.phones?.push(newPhone)
  phoneErrors.push({ phoneNumber: '', phoneType: '' })
}

const removePhone = (index: number): void => {
  if (form.phones === null) return
  if (form.phones!.length > 0) {
    form.phones!.splice(index, 1)
    phoneErrors.splice(index, 1)
  }
}

const attemptSubmit = (): void => {
  touched.value = true
  validate()

  if (formIsValid.value) emits('submit', form)
}
</script>

<template>
  <form @submit.prevent="attemptSubmit" novalidate>
    <!-- First Name -->
    <div class="mb-3">
      <label class="form-label">First Name</label>
      <input v-model="form.firstName" type="text" @input="validate" class="form-control" :class="errors.firstName ? 'is-invalid' : ''" required />
      <div class="invalid-feedback">{{ errors.firstName }}</div>
    </div>

    <!-- Last Name -->
    <div class="mb-3">
      <label class="form-label">Last Name</label>
      <input v-model="form.lastName" type="text" @input="validate" class="form-control" :class="errors.lastName ? 'is-invalid' : ''" required />
      <div class="invalid-feedback">{{ errors.lastName }}</div>
    </div>

    <!-- email -->
    <div class="mb-3">
      <label class="form-label">Email</label>
      <input v-model="form.email" type="text" @input="validate" class="form-control" :class="errors.email ? 'is-invalid' : ''" required />
      <div class="invalid-feedback">{{ errors.email }}</div>
    </div>

    <!-- Phone numbers -->
    <label class="form-label">Phone Numbers</label>
    <div class="mb-3">
      <div v-for="(phone, index) in form.phones" :key="index" class="border rounded p-2 mb-2 bg-light">
        <div class="row g-2 align-items-center">
          <div class="col-md-6">
            <label class="form-label">Phone {{ index + 1 }}</label>
            <input
              v-maska="'+# (###) ###-####'"
              v-model="phone.phoneNumber"
              placeholder="+1 (123) 456-7890"
              @input="validate"
              class="form-control"
              :class="{ 'is-invalid': phoneErrors[index]?.phoneNumber }"
            />
            <div class="invalid-feedback">{{ phoneErrors[index]?.phoneNumber }}</div>
          </div>
          <div class="col-md-5">
            <label class="form-label">Type</label>
            <input
              v-model="phone.phoneType"
              placeholder="mobile/work/home"
              @input="validate"
              class="form-control"
              :class="{ 'is-invalid': phoneErrors[index]?.phoneType }"
            />
            <div class="invalid-feedback">{{ phoneErrors[index]?.phoneType }}</div>
          </div>
          <div class="col-md-1 text-end">
            <button type="button" class="btn btn-outline-danger btn-sm" @click="removePhone(index)">&times;</button>
          </div>
        </div>
      </div>

      <button type="button" class="btn btn-outline-primary btn-sm" @click="addingPhone">Add Phone</button>
    </div>

    <!-- Footer -->
    <div class="d-flex modal-footer" :class="mode === 'edit' ? 'justify-content-between' : 'justify-content-end'">
      <span v-if="mode === 'edit'" class="justify-content-start gap-2 mt-4">
        <button type="button" class="btn btn-danger" @click="$emit('delete')">Delete</button>
      </span>
      <div class="d-flex justify-content-end gap-2 mt-4">
        <button type="button" class="btn btn-secondary" @click="$emit('cancel')">Cancel</button>
        <button type="button" class="btn btn-warning" @click="$emit('archive', true)" v-if="!client.isArchived && mode === 'edit'">Archive</button>
        <button type="button" class="btn btn-warning" @click="$emit('archive', false)" v-if="client.isArchived && mode === 'edit'">UnArchive</button>
        <button type="submit" class="btn btn-primary" :disabled="saving">
          <span v-if="saving" class="spinner-border spinner-border-sm me-2"></span>
          {{ saving ? 'Saving...' : mode === 'create' ? 'Save' : 'Update' }}
        </button>
      </div>
    </div>
  </form>
</template>
