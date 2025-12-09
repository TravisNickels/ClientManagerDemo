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
      silent: false,
      environment: 'jsdom',
      setupFiles: ['tests/setup.ts'],
      exclude: [...configDefaults.exclude, 'e2e/**'],
      include: ['tests/**/*.spec.ts'],
      root: fileURLToPath(new URL('./', import.meta.url)),
    },
  })
)
