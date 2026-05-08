import { defineStore } from 'pinia'

export const useModalStore = defineStore('modal', {
    state: () => ({
        isOpen: false,
        title: '',
        message: '',
        confirmText: '',
        cancelText: '',
        type: 'danger', // 'danger' | 'info' | 'warning'
        resolvePromise: null
    }),
    actions: {
        /**
         * Ask for confirmation with a custom modal
         * @returns {Promise<boolean>}
         */
        async confirm({ title, message, confirmText, cancelText, type = 'danger' }) {
            this.title = title || 'Confirmation'
            this.message = message || 'Êtes-vous sûr de vouloir continuer ?'
            this.confirmText = confirmText || 'Confirmer'
            this.cancelText = cancelText || 'Annuler'
            this.type = type
            this.isOpen = true

            return new Promise((resolve) => {
                this.resolvePromise = resolve
            })
        },

        handleConfirm() {
            if (this.resolvePromise) this.resolvePromise(true)
            this.close()
        },

        handleCancel() {
            if (this.resolvePromise) this.resolvePromise(false)
            this.close()
        },

        close() {
            this.isOpen = false
            // Delay resetting to allow for closing animation
            setTimeout(() => {
                this.title = ''
                this.message = ''
                this.resolvePromise = null
            }, 300)
        }
    }
})
