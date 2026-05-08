import { defineStore } from 'pinia'

export const useToastStore = defineStore('toast', {
    state: () => ({
        toasts: []
    }),
    actions: {
        /**
         * Show a global notification
         * @param {string} message 
         * @param {string} type - 'success' | 'error' | 'info' | 'warning'
         * @param {number} duration - ms
         */
        show(message, type = 'info', duration = 4000) {
            const id = Date.now() + Math.random()
            this.toasts.push({ id, message, type, duration })

            if (duration > 0) {
                setTimeout(() => {
                    this.remove(id)
                }, duration)
            }
        },

        success(message, duration) { this.show(message, 'success', duration) },
        error(message, duration) { this.show(message, 'error', duration) },
        info(message, duration) { this.show(message, 'info', duration) },
        warning(message, duration) { this.show(message, 'warning', duration) },

        remove(id) {
            this.toasts = this.toasts.filter(t => t.id !== id)
        }
    }
})
