import { describe, it, expect } from 'vitest'
import { renderWithApp } from '../utilities/render'
import Header from '@/components/client-header.vue'
import { clickFilterDropdownOption } from './action/clickFilterDropdownOption'
import type { Client } from '@/types/client'

const defaultClients: Client[] = [
  { id: '11111111-1111-1111-1111-111111111111', firstName: 'Luke', lastName: 'Skywalker', email: 'Luke.Skywalker@gmail.com', isArchived: false },
  { id: '22222222-2222-2222-2222-222222222222', firstName: 'Han', lastName: 'Solo', email: 'han.solo@gmail.com', isArchived: false },
  { id: '33333333-3333-3333-3333-333333333333', firstName: 'Darth', lastName: 'Vader', email: 'darth.vader@gmail.com', isArchived: true },
  { id: '44444444-4444-4444-4444-444444444444', firstName: 'Lord', lastName: 'Sidious', email: 'lord.sidious@gmail.com', isArchived: true },
]

type UpdateEvent = [clients: Client[], isArchived: boolean]

describe('FEATURE: Client header', () => {
  describe('RULE: Clients should be filtered by archive status', () => {
    it('EXAMPLE: Selecting the "all" filter for clients should emit active and archived clients', async () => {
      // Arrange
      const wrapper = await renderWithApp(Header, {
        useRealSignalR: true,
        useRealPinia: false,
        props: {
          clients: defaultClients,
        },
      })
      await clickFilterDropdownOption('option-active')
      wrapper.emitted().update = []

      // Act
      await clickFilterDropdownOption('option-all')

      // Assert
      const emittedValue = wrapper.emitted<UpdateEvent>('update')
      expect(emittedValue).toBeTruthy()
      expect(emittedValue[0]).toBeTruthy()

      const [clients] = emittedValue![0] as UpdateEvent

      expect(clients.length).toBe(4)
    })
    it('EXAMPLE: Selecting the "active" filter for clients should emit only active clients', async () => {
      // Arrange
      const wrapper = await renderWithApp(Header, {
        useRealSignalR: true,
        useRealPinia: false,
        props: {
          clients: defaultClients,
        },
      })

      // Act
      await clickFilterDropdownOption('option-active')

      // Assert
      const emittedValue = wrapper.emitted<UpdateEvent>('update')
      expect(emittedValue).toBeTruthy()
      expect(emittedValue[0]).toBeTruthy()

      const [clients] = emittedValue![0] as UpdateEvent

      expect(clients.length).toBe(2)
      expect(clients[0]!.firstName).toBe('Luke')
      expect(clients[1]!.firstName).toBe('Han')
    })
    it('EXAMPLE: Selecting the "archived" filter for clients should emit only archived clients', async () => {
      // Arrange
      const wrapper = await renderWithApp(Header, {
        useRealSignalR: true,
        useRealPinia: false,
        props: {
          clients: defaultClients,
        },
      })

      // Act
      await clickFilterDropdownOption('option-archived')

      // Assert
      const emittedValue = wrapper.emitted<UpdateEvent>('update')
      expect(emittedValue).toBeTruthy()
      expect(emittedValue[0]).toBeTruthy()

      const [clients] = emittedValue![0] as UpdateEvent

      expect(clients.length).toBe(2)
      expect(clients[0]!.firstName).toBe('Lord')
      expect(clients[1]!.firstName).toBe('Darth')
    })
  })
})
