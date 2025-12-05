import { screen, within } from '@testing-library/vue'

export async function clientGridNames() {
  const clientList = await screen.findByRole('generic', { name: /client-grid/i })
  const clients = within(clientList).queryAllByRole('link')

  return clients.map((item) => item.textContent)
}
