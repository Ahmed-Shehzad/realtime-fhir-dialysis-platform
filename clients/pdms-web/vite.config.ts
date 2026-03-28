import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig(({ command }) => ({
  plugins: [react(), tailwindcss()],
  server:
    command === 'serve'
      ? {
          proxy: {
            '/health': { target: 'http://localhost:5100', changeOrigin: true },
            '/api': { target: 'http://localhost:5100', changeOrigin: true },
            '/hubs': { target: 'http://localhost:5100', changeOrigin: true, ws: true },
          },
          headers: {
            // Dev server: allow Vite/HMR (unsafe-eval). Enforce stricter CSP at your reverse proxy in production.
            'Content-Security-Policy':
              "default-src 'self'; script-src 'self' 'unsafe-eval'; script-src-elem 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; connect-src 'self' http://localhost:* https: ws: wss:; img-src 'self' data:; base-uri 'self'; form-action 'self'; frame-ancestors 'none'",
          },
        }
      : undefined,
}))
