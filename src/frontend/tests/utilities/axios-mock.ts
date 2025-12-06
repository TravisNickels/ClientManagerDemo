import { vi } from 'vitest'

export const mockAxiosGet = vi.fn()

vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      get: mockAxiosGet,
      post: vi,
      put: vi.fn(),
      patch: vi.fn(),
      delete: vi.fn(),
    })),
  },
}))
