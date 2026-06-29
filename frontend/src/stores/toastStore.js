import { defineStore } from 'pinia'

const MAX_TOASTS = 5

export const useToastStore = defineStore('toast', {
    state: () => ({
        toasts: [],
        _timers: {}   // map id → timeoutId (pour clearTimeout propre)
    }),
    actions: {
        /**
         * Affiche une notification globale.
         * @param {string} message
         * @param {'success'|'error'|'info'|'warning'} type
         * @param {number} duration  — ms, 0 = persistent
         */
        show(message, type = 'info', duration = 4000) {
            // Déduplication : ignorer si le même message est déjà visible
            const duplicate = this.toasts.find(
                t => t.message === message && t.type === type
            )
            if (duplicate) return

            // Limite du nombre de toasts simultanés
            if (this.toasts.length >= MAX_TOASTS) {
                this.remove(this.toasts[0].id)
            }

            const id = Date.now() + Math.random()
            this.toasts.push({ id, message, type, duration })

            if (duration > 0) {
                this._timers[id] = setTimeout(() => this.remove(id), duration)
            }
        },

        success(message, duration) { this.show(message, 'success', duration) },
        error(message, duration)   { this.show(message, 'error',   duration) },
        info(message, duration)    { this.show(message, 'info',    duration) },
        warning(message, duration) { this.show(message, 'warning', duration) },

        remove(id) {
            // Annule le timer si l'utilisateur ferme manuellement
            if (this._timers[id]) {
                clearTimeout(this._timers[id])
                delete this._timers[id]
            }
            this.toasts = this.toasts.filter(t => t.id !== id)
        }
    }
})
