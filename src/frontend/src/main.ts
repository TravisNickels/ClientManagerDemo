import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import 'bootstrap/dist/css/bootstrap.css'
import 'bootstrap-icons/font/bootstrap-icons.css'
import 'vue3-toastify/dist/index.css'
import Vue3Toastify, { type ToastContainerOptions } from 'vue3-toastify'
import ConfirmationModal from '@/components/confirmation-modal.vue'

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.component('ConfirmationModal', ConfirmationModal)

app.use(Vue3Toastify, {
  position: 'bottom-right',
  pauseOnHover: true,
  draggable: true,
  showCloseButton: true,
  clearOnUrlChange: false,
} as ToastContainerOptions)

app.mount('#app')
