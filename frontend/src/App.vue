<template>
  <div id="app" class="app-wrapper">
    <!-- Global Background System -->
    <CelestialBackground />

    <!-- Global Notifications -->
    <ToastContainer />
    <ConfirmModal />
    <!-- Main Content Layer -->
    <div class="app-content" :class="{ 'blocked-by-modal': authStore.mustChangePassword }">
      <router-view />
    </div>

    <ChangeInitialPasswordModal />
  </div>
</template>

<script>
import { ref } from 'vue'
import { useThemeStore } from '@/stores/themeStore'
import { useLangStore } from '@/stores/langStore'
import { useAuthStore } from '@/stores/authStore'
import ToastContainer from '@/components/common/ToastContainer.vue'
import ConfirmModal from '@/components/common/ConfirmModal.vue'
import CelestialBackground from '@/components/common/CelestialBackground.vue'
import ChangeInitialPasswordModal from '@/components/auth/ChangeInitialPasswordModal.vue'

export default {
  name: 'App',
  components: { ToastContainer, ConfirmModal, CelestialBackground, ChangeInitialPasswordModal },
  setup() {
    const themeStore = useThemeStore()
    const langStore = useLangStore()
    const authStore = useAuthStore()

    // Initialize stores
    themeStore.initTheme()
    langStore.initLanguage()
    authStore.loadFromStorage()

    return { themeStore, langStore, authStore }
  }
}
</script>

<style>
/* Remove the custom app-scaling that breaks modern layouts and replace with clean reset */
html {
  margin: 0 !important;
  padding: 0 !important;
  overflow: hidden !important;
  height: 100%;
}

body {
  margin: 0 !important;
  padding: 0 !important;
  overflow-y: scroll !important; /* Force a single scrollbar here */
  overflow-x: hidden !important;
  height: 100%;
  background-color: var(--r-body-bg, #f8fafc) !important;
  transition: background-color 0.5s ease;
}

body.dark-mode {
  background-color: #020617 !important;
}

#app {
  min-height: 100vh;
  width: 100%;
  display: flex;
  flex-direction: column;
  overflow: visible !important;
  position: relative;
}

.app-wrapper, .app-content {
  overflow: visible !important;
  height: auto !important;
  min-height: 100vh;
}

.app-content {
  position: relative;
  z-index: 10 !important; /* Above background, but below modals/toasts */
  width: 100%;
  min-height: 100vh;
  pointer-events: auto !important; /* FORCE interaction on everything in router-view */
}

.app-content.blocked-by-modal {
  pointer-events: none !important;
}
</style>
