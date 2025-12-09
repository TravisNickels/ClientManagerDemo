import { describe, it, expect, beforeEach } from 'vitest'
import { screen } from '@testing-library/vue'
import { clientListNames } from './questions/clientListNames'
import { clientGridNames } from './questions/clientGridNames'
import type { Client, ClientResponse } from '@/types/client'
import { clickFilterDropdownOption } from './actions/clickFilterDropdownOption'
import { clickClientGridButton, clickClientListButton } from './actions/clickClientDisplayButtonGroup'
import { setupDashboard } from '@tests/utilities/test-fixtures'
import { mockAxiosGet } from '@tests/utilities/mocks/axios-mock'
import { SignalrEventNames } from '@/types/signalr/signalrEvents'
import { asMockSignalRStore } from '@tests/utilities/mocks/signalr-mock'

const defaultClients: Client[] = [
  { id: '11111111-1111-1111-1111-111111111111', firstName: 'Luke', lastName: 'Skywalker', email: 'Luke.Skywalker@gmail.com', isArchived: false },
  { id: '22222222-2222-2222-2222-222222222222', firstName: 'Han', lastName: 'Solo', email: 'han.solo@gmail.com', isArchived: false },
  { id: '33333333-3333-3333-3333-333333333333', firstName: 'Darth', lastName: 'Vader', email: 'darth.vader@gmail.com', isArchived: true },
  { id: '44444444-4444-4444-4444-444444444444', firstName: 'Lord', lastName: 'Sidious', email: 'lord.sidious@gmail.com', isArchived: true },
]

describe('FEATURE: Client list view', () => {
  // Arrange
  beforeEach(async () => {
    await setupDashboard(defaultClients)
  })

  describe('RULE: Clients displayed in a list', () => {
    it('EXAMPLE: All clients should be displayed on load', async () => {
      // Act
      const clients = await clientListNames()

      // Assert
      expect(clients.length).toBe(4)
    })
    it('EXAMPLE: Selecting "active" filter should display only active clients', async () => {
      // Act
      await clickFilterDropdownOption('option-active')
      const clients = await clientListNames()
      const flat = clients.flat()

      // Assert
      expect(clients.length).to.equal(2)
      expect(flat.some((client) => client === 'Luke Skywalker')).toBe(true)
      expect(flat.some((client) => client === 'Han Solo')).toBe(true)
    })
    it('EXAMPLE: Selecting "archived" filter should display only archived clients', async () => {
      // Act
      await clickFilterDropdownOption('option-archived')
      const clients = await clientListNames()
      const flat = clients.flat()

      // Assert
      expect(clients.length).to.equal(2)
      expect(flat.some((client) => client === 'Darth Vader')).toBe(true)
      expect(flat.some((client) => client === 'Lord Sidious')).toBe(true)
    })
    it('EXAMPLE: All clients should be displayed when the list button is clicked', async () => {
      // Act
      await clickClientGridButton()
      await clickClientListButton()
      const clients = await clientListNames()

      // Assert
      expect(clients.length).to.equal(4)
    })
  })
})

describe('FEATURE: Client grid view', () => {
  describe('RULE: Clients displayed in a grid', () => {
    it('EXAMPLE: All clients should be displayed when the grid button is clicked', async () => {
      // Arrange
      await setupDashboard(defaultClients)

      // Act
      await clickClientGridButton()
      const clients = await clientGridNames()

      // Assert
      expect(clients.length).to.equal(4)
    })
  })
})

describe('FEATURE: SignalR client update', () => {
  describe('RULE: Dashboard refreshes its client list when a SignalR update event is received', () => {
    it('EXAMPLE: When a fake client-updated event fires, the updated client data appears on the dashboard', async () => {
      // Arrange
      const clientResponse: ClientResponse = {
        id: '11111111-1111-1111-1111-111111111111',
        firstName: 'Ahsoka',
        lastName: 'Tano',
        email: 'Ahsoka.Tano@gmail.com',
        isArchived: false,
      }
      mockAxiosGet.mockResolvedValue({ data: defaultClients })
      const { signalRStore } = await setupDashboard()
      mockAxiosGet.mockResolvedValue({ data: [clientResponse] })

      // Act
      const mock = asMockSignalRStore(signalRStore)
      mock.emit(SignalrEventNames.ClientResponse, clientResponse)

      // Assert
      await screen.findByText(/Ahsoka Tano/i)
      expect(screen.getByText(/Ahsoka Tano/i)).toBeInTheDocument()
    })
  })
})
