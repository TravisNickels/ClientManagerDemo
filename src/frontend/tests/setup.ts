import { beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import '@testing-library/jest-dom'

beforeEach(() => {
  setActivePinia(createPinia())
})
