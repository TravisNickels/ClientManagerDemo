<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import * as bootstrap from 'bootstrap'
import { state, resolve } from '@/composables/useConfirm'

const modalRef = ref<HTMLDivElement | null>(null)
let modal: bootstrap.Modal | null = null

watch(
  () => state.visible,
  (visible) => {
    if (!modal) return
    if (visible) modal.show()
    else modal.hide()
  }
)

function confirmClick() {
  resolve(true)
  modal?.hide()
}

function cancelClick() {
  resolve(false)
  modal?.hide()
}

onMounted(() => {
  modal = new bootstrap.Modal(modalRef.value!, {
    backdrop: 'static',
    keyboard: true,
  })

  modalRef.value!.addEventListener('hidden.bs.modal', () => {
    // Only resolve if Vue hasn't already resolved manually
    if (state.visible) {
      state.visible = false
      resolve(false)
    }
  })
})
</script>

<template>
  <!-- IMPORTANT: Do NOT use v-if -->
  <!-- Bootstrap manages visibility internally -->
  <div ref="modalRef" class="modal fade" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered modal-sm">
      <div class="modal-content">
        <div class="modal-header">
          <h5 class="modal-title">{{ state.title }}</h5>
          <button type="button" class="btn-close" @click="cancelClick"></button>
        </div>

        <div class="modal-body">
          {{ state.message }}
        </div>

        <div class="modal-footer">
          <button class="btn btn-secondary" @click="cancelClick">
            {{ state.cancelText }}
          </button>

          <button class="btn btn-danger" @click="confirmClick">
            {{ state.confirmText }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
