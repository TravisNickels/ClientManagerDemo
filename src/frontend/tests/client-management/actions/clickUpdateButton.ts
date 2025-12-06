import { screen } from '@testing-library/vue'
import userEvent from '@testing-library/user-event'

export async function clickUpdateButton() {
  const updateButton = await screen.findByRole('button', { name: /update/i })
  await userEvent.click(updateButton)
}
