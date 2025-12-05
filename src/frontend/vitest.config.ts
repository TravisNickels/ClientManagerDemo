import { fileURLToPath } from 'node:url'
import { mergeConfig, defineConfig, configDefaults } from 'vitest/config'
import viteConfig from './vite.config'

export default mergeConfig(
  viteConfig,
  defineConfig({
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
        '@tests': fileURLToPath(new URL('./tests', import.meta.url)),
      },
    },
    test: {
      globals: true,
      environment: 'jsdom',
      setupFiles: ['tests/setup.ts', 'tests/utilities/signalr-mock.ts'],
      exclude: [...configDefaults.exclude, 'e2e/**'],
      include: ['tests/**/*.spec.ts'],
      root: fileURLToPath(new URL('./', import.meta.url)),
    },
  })
)
