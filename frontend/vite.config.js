import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

// Configuration de Vite pour Vue.js
// En développement : HTTPS activé + proxy vers le backend local
// En production (Vercel) : VITE_API_URL pointe vers le backend Render
export default defineConfig(({ command }) => {
  const isDev = command === 'serve'

  // Plugins : basicSsl uniquement en développement local (HTTPS auto-signé)
  const plugins = [vue()]
  if (isDev) {
    // Import dynamique pour éviter l'erreur lors du build production
    const { default: basicSsl } = require('@vitejs/plugin-basic-ssl')
    plugins.push(basicSsl())
  }

  return {
    plugins,
    resolve: {
      alias: {
        '@': path.resolve(__dirname, './src')
      }
    },
    server: isDev ? {
      port: 3010,
      https: true,    // Activer HTTPS sur le serveur de développement
      strictPort: true,
      proxy: {
        '/api': {
          target: 'https://localhost:7075',  // Backend ASP.NET Core en HTTPS
          changeOrigin: true,
          secure: false   // Accepter le certificat dev auto-signé du SDK .NET
        },
        '/uploads': {
          target: 'https://localhost:7075',  // Backend ASP.NET Core en HTTPS
          changeOrigin: true,
          secure: false
        }
      }
    } : {},
    build: {
      outDir: 'dist',
      sourcemap: false,
    }
  }
})

