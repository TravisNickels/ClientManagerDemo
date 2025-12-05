import { screen } from '@testing-library/vue'
import userEvent from '@testing-library/user-event'

export async function clickClientListButton() {
  const listButton = await screen.findByRole('button', { name: /list\-view/i })
  await userEvent.click(listButton)
}

export async function clickClientGridButton() {
  const gridButton = await screen.findByRole('button', { name: /grid\-view/i })
  await userEvent.click(gridButton)
}
