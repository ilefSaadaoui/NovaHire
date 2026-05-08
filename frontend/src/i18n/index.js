import { createI18n } from 'vue-i18n'
import fr from './locales/fr.json'
import en from './locales/en.json'
import ar from './locales/ar.json'

const i18n = createI18n({
    legacy: false,
    locale: localStorage.getItem('appLang') || 'fr',
    fallbackLocale: 'fr',
    messages: {
        fr,
        en,
        ar
    }
})

export default i18n
