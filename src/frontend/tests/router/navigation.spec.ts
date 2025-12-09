import { describe, expect, it, vi } from 'vitest'
import { managementPageTitle } from './questions/managementPageTitle'
import { setupDefaultApp } from '@tests/utilities/test-fixtures'
import type { Client } from '@/types/client'

describe('FEATURE: Client management routing', () => {
  describe('RULE: Navigating to a client route loads the management page', () => {
    it('EXAMPLE: Visiting "/client/:id" displays the Edit Client heading', async () => {
      // Arrange
      const { router, clientStore } = await setupDefaultApp('/client-dashboard')
      clientStore.getClient = vi.fn().mockResolvedValue({
        id: '11111111-1111-1111-1111-111111111111',
        firstName: 'Luke',
        lastName: 'Skywalker',
        email: 'Luke.Skywalker@gmail.com',
        isArchived: false,
      } as Client)

      // Act
      await router.push('/client/11111111-1111-1111-1111-111111111111')
      await router.isReady()

      // Assert
      expect(await managementPageTitle()).to.equal('Edit Client')
    })
  })
})
