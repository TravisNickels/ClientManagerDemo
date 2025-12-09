import { vi } from 'vitest'

export const mockAxiosGet = vi.fn()
export const mockAxiosPost = vi.fn()
export const mockAxiosPut = vi.fn()
export const mockAxiosPatch = vi.fn()
export const mockAxiosDelete = vi.fn()

vi.mock('axios', () => ({
  default: {
    create: vi.fn(() => ({
      get: mockAxiosGet,
      post: mockAxiosPost,
      put: mockAxiosPut,
      patch: mockAxiosPatch,
      delete: mockAxiosDelete,
    })),
  },
}))
