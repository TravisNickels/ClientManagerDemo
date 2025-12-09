import { describe, it, expect, beforeEach } from 'vitest'
import { mockAxiosGet } from '@tests/utilities/mocks/axios-mock'
import { setActivePinia, createPinia } from 'pinia'
import { useClientStore } from '@/stores/client-store'

describe('FEATURE: Client store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    mockAxiosGet.mockReset()
  })
  describe('RULE: Client list should be kept in state', () => {
    it('EXAMPLE: Fetched clients are stored in allClients', async () => {
      // Arrange
      const store = useClientStore()
      const clients = [
        {
          id: '11111111-1111-1111-1111-111111111111',
          firstName: 'Luke',
          lastName: 'Skywalker',
          email: 'Luke.Skywalker@gmail.com',
          isArchived: false,
        },
      ]
      mockAxiosGet.mockResolvedValue({ data: clients })

      // Act
      await store.updateClientsList()

      // Arrange
      expect(store.allClients.length).toBe(1)
      expect(store.allClients[0]!.firstName).toBe('Luke')
    })
  })
})
