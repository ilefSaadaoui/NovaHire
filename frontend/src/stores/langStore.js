import { defineStore } from 'pinia'
import i18n from '../i18n'

export const useLangStore = defineStore('lang', {
    state: () => ({
        currentLang: localStorage.getItem('appLang') || 'fr'
    }),
    actions: {
        setLanguage(lang) {
            this.currentLang = lang
            localStorage.setItem('appLang', lang)
            i18n.global.locale.value = lang

            // Handle RTL for Arabic
            if (lang === 'ar') {
                document.documentElement.dir = 'rtl'
                document.documentElement.lang = 'ar'
            } else {
                document.documentElement.dir = 'ltr'
                document.documentElement.lang = lang
            }
        },
        initLanguage() {
            this.setLanguage(this.currentLang)
        }
    }
})
