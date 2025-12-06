import { screen } from '@testing-library/vue'

export async function errorMessage(message: string) {
  return await screen.findByText(message, { exact: false })
}
