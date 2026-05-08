import axios from 'axios'

// Création d'une instance Axios centralisée
const api = axios.create({
    baseURL: '/api', // Proxy configuré dans vite.config.js
    timeout: 10000 // Timeout de 10 secondes pour éviter les boutons bloquants
})

// Intercepteur pour les Requetes
api.interceptors.request.use(
    async (config) => {
        // Break circular dependency by importing dynamically
        const { useAuthStore } = await import('@/stores/authStore')
        const authStore = useAuthStore()
        
        if (authStore.token) {
            config.headers.Authorization = `Bearer ${authStore.token}`
        }
        return config
    },
    (error) => {
        return Promise.reject(error)
    }
)

// Intercepteur pour les Réponses
api.interceptors.response.use(
    (response) => {
        // Si la réponse est un succès, on la retourne directement
        return response
    },
    async (error) => {
        const { useToastStore } = await import('@/stores/toastStore')
        const toast = useToastStore()

        // Si la requête a défini silentError: true, on ne montre pas de toast
            const silent = error.config?.silentError === true

            if (error.response) {
            // Le back-end a retourné un code d'erreur (4xx, 5xx)
            const status = error.response.status
            const data = error.response.data

            // Extraction du message d'erreur
            let errorMessage = 'Une erreur est survenue'
            if (data && data.message) {
                errorMessage = data.message // Message formaté par GlobalExceptionMiddleware
            } else if (data && data.title) {
                errorMessage = data.title // Format ASP.NET ProblemDetails
            }

            if (!silent) {
                // Gestion centralisée selon le code HTTP
                switch (status) {
                    case 400: // Bad Request (Validation)
                        toast.show(errorMessage, 'error')
                        break
                    case 401: // Unauthorized
                        toast.show('Session expirée ou non autorisée', 'error')
                        const { useAuthStore } = await import('@/stores/authStore')
                        const authStore = useAuthStore()
                        authStore.logout() // Purge locale
                        const { default: router } = await import('@/router')
                        router.push('/connexion') // Redirection
                        break
                    case 403: // Forbidden
                        toast.show('Action non autorisée', 'error')
                        break
                    case 404: // Not Found
                        toast.show(errorMessage || 'Ressource introuvable', 'error')
                        break
                    case 409: // Conflict
                        toast.show(errorMessage, 'error')
                        break
                    case 500: // Internal Server Error
                        toast.show('Erreur interne du serveur', 'error')
                        console.error("500 API Error Detail: ", data)
                        break
                    default:
                        toast.show(errorMessage, 'error')
                }
            }
        } else if (error.request) {
            // La requête a été faite mais pas de réponse reçue
            if (!silent) toast.show('Erreur réseau. Le serveur ne répond pas.', 'error')
        } else {
            // Une erreur de configuration de la requête
            if (!silent) toast.show(error.message, 'error')
        }

        return Promise.reject(error)
    }
)

export default api
