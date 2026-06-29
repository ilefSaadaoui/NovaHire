import { defineStore } from 'pinia'
import { useAuthStore } from './authStore'

export const useThemeStore = defineStore('theme', {
    state: () => {
        const stored = localStorage.getItem('theme')
        return {
            // Dark by default on first visit (matches current public pages look)
            isDark: stored === null ? true : stored === 'dark'
        }
    },
    actions: {
        toggleTheme() {
            this.isDark = !this.isDark
            const el = document.documentElement
            const body = document.body
            el.classList.toggle('dark-mode', this.isDark)
            body.classList.toggle('dark-mode', this.isDark)
            localStorage.setItem('theme', this.isDark ? 'dark' : 'light')

            // Apply theme colors dynamically
            const authStore = useAuthStore()
            authStore.applyTheme()
        },
        initTheme() {
            const el = document.documentElement
            const body = document.body
            el.classList.toggle('dark-mode', this.isDark)
            body.classList.toggle('dark-mode', this.isDark)
        }
    }
})
