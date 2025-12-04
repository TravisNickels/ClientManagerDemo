import { describe, it, expect } from 'vitest'
import { screen } from '@testing-library/vue'
import { renderWithApp } from './utilities/render'
import App from '@/App.vue'

describe('App', () => {
  it('renders the main heading', async () => {
    await renderWithApp(App, { useRealSignalR: false, useRealPinia: false })
    const title = screen.getByLabelText(/title/i)
    expect(title).toHaveTextContent(/client Management Demo/i)
  })
})
