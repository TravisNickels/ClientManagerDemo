import { screen } from '@testing-library/vue'

export async function managementPageTitle() {
  const title = await screen.findByRole('heading', { name: /management\-heading/i })
  return title.textContent
}
