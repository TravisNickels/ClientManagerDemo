import { describe, it, expect } from 'vitest'
import type { Client } from '@/types/client'
import { errorMessage } from '@tests/client-management/questions/errorMessage'
import { clickUpdateButton } from '@tests/client-management/actions/clickUpdateButton'
import { setupClientForm } from '@tests/utilities/test-fixtures'

const clientMissingFirstName: Client = {
  id: '11111111-1111-1111-1111-111111111111',
  firstName: '',
  lastName: 'Skywalker',
  email: 'Luke.Skywalker@gmail.com',
  isArchived: false,
}
describe('FEATURE: Client form validation', () => {
  describe('RULE: Required fields must prevent submission', () => {
    it('EXAMPLE: Missing first name shows an error', async () => {
      // Arrange
      await setupClientForm(clientMissingFirstName, 'edit', false)

      // Act
      await clickUpdateButton()

      // Assert
      expect(await errorMessage('First name must be at least 2 characters')).toBeInTheDocument()
    })
  })
})
