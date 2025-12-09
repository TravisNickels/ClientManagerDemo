import '@tests/utilities/mocks/signalr-mock'
import '@tests/utilities/mocks/axios-mock'
import '@testing-library/jest-dom'

import { afterEach, beforeEach, vi } from 'vitest'
import { configure } from '@testing-library/vue'
import { mockAxiosGet } from '@tests/utilities/mocks/axios-mock'

beforeEach(() => {
  mockAxiosGet.mockReset()
  vi.clearAllMocks()
})

afterEach(() => {
  localStorage.clear()
})

// Remove DOM dump from errors
configure({
  getElementError(message) {
    return new Error(message ?? undefined) // do NOT append PrettyDOM
  },
})
