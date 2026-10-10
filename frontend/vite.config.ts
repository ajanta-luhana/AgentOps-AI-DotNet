import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// The browser only talks to the API; in development Vite proxies /api to the local ASP.NET Core host.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: { '/api': { target: 'http://localhost:5080', changeOrigin: true } },
  },
})
