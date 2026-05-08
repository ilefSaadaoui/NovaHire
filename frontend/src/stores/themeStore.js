import { defineStore } from 'pinia'

export const useThemeStore = defineStore('theme', {
    state: () => ({
        isDark: localStorage.getItem('theme') === 'dark'
    }),
    actions: {
        toggleTheme() {
            this.isDark = !this.isDark
            const el = document.documentElement
            const body = document.body
            el.classList.toggle('dark-mode', this.isDark)
            body.classList.toggle('dark-mode', this.isDark)
            localStorage.setItem('theme', this.isDark ? 'dark' : 'light')
        },
        initTheme() {
            if (this.isDark) {
                document.documentElement.classList.add('dark-mode')
                document.body.classList.add('dark-mode')
            }
        }
    }
})
