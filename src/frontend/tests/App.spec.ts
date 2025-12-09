import { describe, it, expect } from 'vitest'
import { screen } from '@testing-library/vue'
import { defaultClients, setupApp } from './utilities/test-fixtures'
import { mockAxiosGet } from './utilities/mocks/axios-mock'

describe('FEATURE: App layout', () => {
  describe('RULE: Main application should render UI', () => {
    it('EXAMPLE: The main heading is displayed', async () => {
      mockAxiosGet.mockResolvedValue(defaultClients)
      await setupApp()

      await screen.findByLabelText(/title/i)
      const title = screen.getByLabelText(/title/i)
      expect(title).toHaveTextContent(/client Management Demo/i)
    })
  })
})
