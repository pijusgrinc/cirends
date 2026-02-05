import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueJsx(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  // Dev server proxy to forward /api requests to backend during development
  server: {
    proxy: {
      '/api': {
        target: process.env.BACKEND_URL || 'https://cirends.runasp.net',
        changeOrigin: true,
        secure: false,
        // do NOT rewrite the path: backend routes include the /api prefix
      }
    }
  }
})
