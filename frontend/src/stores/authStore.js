import { defineStore } from 'pinia'
import api from '@/api/axios'
import authService from '../services/authService'

/**
 * Store Pinia pour la gestion de l'authentification
 * Centralise l'état auth au lieu de disperser dans localStorage
 */
export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('authToken') || sessionStorage.getItem('authToken') || null,
    refreshToken: localStorage.getItem('refreshToken') || sessionStorage.getItem('refreshToken') || null,
    userRole: localStorage.getItem('userRole') || sessionStorage.getItem('userRole') || null,
    tokenExpiration: localStorage.getItem('tokenExpiration') || sessionStorage.getItem('tokenExpiration') || null,
    mustChangePassword: sessionStorage.getItem('mustChangePassword') === 'true',
    user: null
  }),

  getters: {
    isAuthenticated: (state) => !!state.token,
    isSuperAdmin: (state) => (state.userRole || '').toLowerCase() === 'superadmin',
    isRecruiter: (state) => ['companyadmin', 'recruiter', 'admin', 'company'].includes((state.userRole || '').toLowerCase()),
    isAdmin: (state) => ['companyadmin', 'admin', 'company', 'superadmin'].includes((state.userRole || '').toLowerCase()),
    isCompanyAdmin: (state) => ['companyadmin', 'admin', 'company'].includes((state.userRole || '').toLowerCase()),
    themeColors: (state) => {
      const role = (state.userRole || '').toLowerCase().trim()
      if (role === 'superadmin') {
        return { accent: '#6C63FF', accentDark: '#4A47A3' }
      }
      
      // Company Admin theme is Blue
      if (['companyadmin', 'admin', 'company'].includes(role)) {
        return {
          accent: '#0ea5e9',
          accentDark: '#0369a1'
        }
      }
      
      // Recruiter and others use the Premium Gold theme (Yellow)
      return {
        accent: '#F7C902',
        accentDark: '#D4A50E'
      }
    }
  },

  actions: {
    /**
     * Connexion unifiée (Recruteur, Admin, SuperAdmin)
     */
    async login(email, password, rememberMe = false) {
      const data = await authService.login(email, password, rememberMe)
      this.token = data.token || data.accessToken
      this.userRole = data.role ? data.role.toLowerCase() : null
      this.refreshToken = data.refreshToken || null
      this.tokenExpiration = data.tokenExpiration || null
      this.mustChangePassword = data.mustChangePassword || false
      sessionStorage.setItem('mustChangePassword', this.mustChangePassword)

      // Profil : silent pour éviter déconnexion auto si /profile échoue juste après login
      await this.fetchUser({ silent: true })
      this.applyTheme()
      return data
    },

    /**
     * Inscription entreprise
     */
    async registerCompany(payload) {
      const data = await authService.registerCompany(payload)
      this.token = data.token || data.accessToken
      this.userRole = data.role ? data.role.toLowerCase() : null
      this.refreshToken = data.refreshToken || null
      this.tokenExpiration = data.tokenExpiration || null
      this.mustChangePassword = data.mustChangePassword || false
      sessionStorage.setItem('mustChangePassword', this.mustChangePassword)
      this.applyTheme()
      return data
    },

    /**
     * Connexion SuperAdmin
     */
    async loginSuperAdmin(payload) {
      const data = await authService.loginSuperAdmin(payload)
      this.token = data.accessToken || data.token
      this.userRole = 'superadmin'
      this.refreshToken = data.refreshToken || null
      this.tokenExpiration = data.tokenExpiration || null
      this.applyTheme()
      return data
    },

    /**
     * Déconnexion
     */
    logout() {
      authService.logout()
      this.token = null
      this.refreshToken = null
      this.userRole = null
      this.tokenExpiration = null
      this.mustChangePassword = false
      sessionStorage.removeItem('mustChangePassword')
      this.user = null
    },

    /**
     * Charger l'état depuis le storage (au démarrage de l'app)
     */
    loadFromStorage() {
      this.token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken') || null
      this.refreshToken = localStorage.getItem('refreshToken') || sessionStorage.getItem('refreshToken') || null
      this.userRole = localStorage.getItem('userRole') || sessionStorage.getItem('userRole') || null
      this.tokenExpiration = localStorage.getItem('tokenExpiration') || sessionStorage.getItem('tokenExpiration') || null
      this.mustChangePassword = sessionStorage.getItem('mustChangePassword') === 'true'
      this.applyTheme() // Apply theme after loading from storage
    },

    setRole(role) {
      this.userRole = role
      localStorage.setItem('userRole', role)
      this.applyTheme()
    },
    applyTheme() {
      const colors = this.themeColors
      const root = document.documentElement
      
      const hexToRgb = (hex) => {
        const r = parseInt(hex.slice(1, 3), 16)
        const g = parseInt(hex.slice(3, 5), 16)
        const b = parseInt(hex.slice(5, 7), 16)
        return `${r}, ${g}, ${b}`
      }

      const rgb = hexToRgb(colors.accent)
      root.style.setProperty('--accent-rgb', rgb)
      root.style.setProperty('--accent-color', colors.accent)
      root.style.setProperty('--accent', colors.accent)
      root.style.setProperty('--accent-dark', colors.accentDark)
      root.style.setProperty('--accent-grad', `linear-gradient(135deg, ${colors.accent}, ${colors.accentDark})`)
      
      const isBlue = colors.accent === '#00A7E1' || colors.accent === '#0ea5e9'
      const isPurple = colors.accent === '#6C63FF'
      const isYellow = colors.accent === '#F7C902' || colors.accent === '#f59e0b' || colors.accent === '#fcd34d'

      let softColor = 'rgba(247, 201, 2, 0.12)' // Default Yellow
      if (isBlue) softColor = 'rgba(0, 167, 225, 0.12)'
      if (isPurple) softColor = 'rgba(108, 99, 255, 0.12)'

      root.style.setProperty('--accent-soft', softColor)
      root.style.setProperty('--accent-glow', `radial-gradient(circle, ${softColor} 0%, transparent 70%)`)

      // Contrast Detection
      root.style.setProperty('--accent-contrast', isYellow ? '#0D1821' : '#FFFFFF')

      // Force dark mode soft colors if dark mode is on
      if (root.classList.contains('dark-mode')) {
        let softColorDark = 'rgba(247, 201, 2, 0.25)'
        if (isBlue) softColorDark = 'rgba(0, 167, 225, 0.25)'
        if (isPurple) softColorDark = 'rgba(108, 99, 255, 0.25)'
        root.style.setProperty('--accent-soft', softColorDark)
        root.style.setProperty('--accent-glow', `radial-gradient(circle, ${softColorDark} 0%, transparent 70%)`)
        root.style.setProperty('--accent-contrast', '#FFFFFF') // Always white on dark
      }
    },

    /**
     * Récupérer les infos de l'utilisateur connecté
     */
    async fetchUser({ silent = false } = {}) {
      if (!this.token) return

      const API_BASE_URL =
        (import.meta.env && import.meta.env.VITE_API_URL) ||
        '/api'

      try {
        const res = await api.get('/profile', { silentError: silent })
        const data = res.data

        // Harmonize Avatar URL for global platform use
        if (data.avatarUrl && !data.avatarUrl.startsWith('http')) {
          const apiBase = API_BASE_URL
          data.avatarUrl = `${apiBase.replace('/api', '')}${data.avatarUrl}`
        }

        this.user = data
        return data
      } catch (error) {
        console.error('Erreur lors de la récupération du profil:', error)
        if (error.response?.status === 401) {
          this.logout()
        }
      }
    }
  }
})
