import { reactive } from 'vue'

interface ConfirmOptions {
  title?: string
  message?: string
  confirmText?: string
  cancelText?: string
}

interface ConfirmState extends ConfirmOptions {
  visible: boolean
}

export const state = reactive<ConfirmState>({
  visible: false,
  title: '',
  message: '',
  confirmText: 'Confirm',
  cancelText: 'Cancel',
})

let resolver: ((value: boolean) => void) | null = null

export function confirm(options: ConfirmOptions): Promise<boolean> {
  return new Promise((resolve) => {
    Object.assign(state, options)
    state.visible = true
    resolver = resolve
  })
}

export function resolve(value: boolean) {
  state.visible = false
  resolver?.(value)
  resolver = null
}
