import { screen } from '@testing-library/vue'
import userEvent from '@testing-library/user-event'

export async function clickFilterDropdownOption(filterName: string) {
  const filterButton = await screen.findByRole('button', { name: filterName })
  await userEvent.click(filterButton)
}
