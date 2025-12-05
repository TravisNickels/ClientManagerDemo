import { screen, within } from '@testing-library/vue'

export async function clientListNames() {
  const clientList = await screen.findByRole('generic', { name: /client-list/i })
  const clients = within(clientList).queryAllByRole('link')

  const mapped = clients.map((item) => item.textContent)
  return mapped
}
