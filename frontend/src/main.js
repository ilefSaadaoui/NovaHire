// Importation de Vue et du composant principal
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import i18n from './i18n'
import './assets/style.css'
import './assets/vivid-bg.css'
import { useAuthStore } from './stores/authStore'
import { vIntersect } from './directives/v-intersect'

// Création de l'application Vue
const app = createApp(App)

// Enregistrement de la directive custom pour les animations au scroll
app.directive('intersect', vIntersect)

// Directive for numeric counter animations
app.directive('counter', {
  mounted(el, binding) {
    const target = parseFloat(el.innerText) || 0
    const duration = 2000
    const frameDuration = 1000 / 60
    const totalFrames = Math.round(duration / frameDuration)
    let frame = 0

    const animate = () => {
      frame++
      const progress = frame / totalFrames
      const current = Math.round(target * (1 - Math.pow(1 - progress, 3))) // Ease out cubic
      el.innerText = current
      if (frame < totalFrames) {
        requestAnimationFrame(animate)
      } else {
        el.innerText = target
      }
    }

    const observer = new IntersectionObserver((entries) => {
      if (entries[0].isIntersecting) {
        animate()
        observer.disconnect()
      }
    })
    observer.observe(el)
  }
})

// Création du store Pinia
const pinia = createPinia()
app.use(pinia)

// Utilisation du router pour la navigation
app.use(router)

// Utilisation du plugin i18n
app.use(i18n)

// Montage de l'application sur l'élément #app
const authStore = useAuthStore(pinia)
authStore.loadFromStorage()

app.mount('#app')
