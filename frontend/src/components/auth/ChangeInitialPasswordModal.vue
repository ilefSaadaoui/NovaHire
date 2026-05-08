<template>
  <Transition name="celestial-modal">
    <div v-if="authStore.mustChangePassword" class="luxury-modal-overlay mandatory" :class="themeClass">
      <div class="luxury-modal-content onboarding-card-light anim-reveal-up" :class="{ 'shake': localError }">
        
        <!-- View 1: Form -->
        <template v-if="!success">
          <div class="modal-header-simple">
            <div class="icon-circle-main">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 28px;"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
            </div>
            <h2 class="title-role">Sécurisez votre compte</h2>
            <p class="subtitle-role">Ravi de vous voir ! Pour protéger votre accès et vos données de recrutement, veuillez choisir votre nouveau mot de passe.</p>
          </div>

          <div class="modal-body-premium">
            <form @submit.prevent="updatePassword" class="role-form">
              <div class="input-grid">
                <div class="input-block">
                  <label class="input-label">Nouveau mot de passe</label>
                  <div class="input-field-wrap">
                    <input type="password" v-model="form.newPassword" required class="input-clean" placeholder="Min. 8 caractères" :disabled="loading">
                  </div>
                </div>

                <div class="input-block">
                  <label class="input-label">Confirmation</label>
                  <div class="input-field-wrap">
                    <input type="password" v-model="form.confirmPassword" required class="input-clean" placeholder="Répétez le mot de passe" :disabled="loading">
                  </div>
                </div>
              </div>

              <div v-if="localError" class="error-banner">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width: 14px;"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                {{ localError }}
              </div>

              <button type="submit" class="btn-role-primary" :disabled="loading">
                <svg v-if="loading" class="spin" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width: 16px;"><path d="M21 12a9 9 0 1 1-6.21-8.58"/></svg>
                {{ loading ? 'SÉCURISATION EN COURS...' : 'ACTIVER MON ESPACE DE TRAVAIL' }}
              </button>
            </form>
          </div>
        </template>

        <!-- View 2: Success -->
        <div v-else class="success-screen anim-scale-up">
           <div class="success-icon-wrap">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width: 40px;"><polyline points="20 6 9 17 4 12"/></svg>
           </div>
           <h2 class="title-role">Compte activé !</h2>
           <p class="subtitle-role">Votre mot de passe a été mis à jour avec succès. Bienvenue sur NovaHire.</p>
           <div class="redirect-bar">
              <div class="progress-bar-fill"></div>
           </div>
        </div>
        
        <div class="footer-simple">
            <p>NovaHire AI — Gestion des Accès Sécurisée</p>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script>
import { useAuthStore } from '@/stores/authStore'
import { useToastStore } from '@/stores/toastStore'
import api from '@/api/axios'

export default {
  name: 'ChangeInitialPasswordModal',
  setup() {
    return { 
      authStore: useAuthStore(),
      toast: useToastStore()
    }
  },
  data() {
    return {
      loading: false,
      success: false,
      localError: null,
      form: {
        newPassword: '',
        confirmPassword: ''
      }
    }
  },
  computed: {
    themeClass() {
      // Recruiter: gold/white | Admin: blue/white + gold touches
      return this.authStore.isCompanyAdmin ? 'theme-admin' : 'theme-recruiter'
    }
  },
  methods: {
    async updatePassword() {
      this.localError = null
      
      if (this.form.newPassword !== this.form.confirmPassword) {
        this.localError = 'Les mots de passe ne correspondent pas'
        return
      }

      if (this.form.newPassword.length < 8) {
        this.localError = 'Le nouveau mot de passe doit faire au moins 8 caractères'
        return
      }

      this.loading = true
      try {
        await api.post('/auth/change-initial-password', this.form)
        
        this.success = true
        
        setTimeout(async () => {
          this.authStore.mustChangePassword = false
          sessionStorage.removeItem('mustChangePassword')
          
          try {
            await this.authStore.fetchUser()
            this.toast.show('Bienvenue dans votre espace de travail !', 'success')
          } catch (e) {
            console.error("Profile fetch error", e)
          }
        }, 2000)

      } catch (err) {
        this.localError = err.response?.data?.message || 'Une erreur est survenue lors de la mise à jour'
        this.loading = false
      }
    }
  }
}
</script>

<style scoped>
/* Role Base Variables */
.theme-recruiter {
  --modal-accent: #F7C902; /* Gold */
  --modal-accent-soft: rgba(247, 201, 2, 0.1);
  --modal-accent-dark: #D4A50E;
}

.theme-admin {
  --modal-accent: #00A7E1; /* Sky Blue */
  --modal-accent-soft: rgba(0, 167, 225, 0.1);
  --modal-accent-dark: #0077B6;
  --modal-touch: #F7C902; /* Gold touches for admin */
}

.luxury-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(12px);
  z-index: 99999 !important;
  display: flex;
  align-items: center;
  justify-content: center;
  pointer-events: auto !important;
}

.onboarding-card-light {
  width: 100%;
  max-width: 500px;
  background: #ffffff;
  border-radius: 28px;
  padding: 48px;
  box-shadow: 0 40px 80px -15px rgba(0, 0, 0, 0.2);
  border: 1px solid rgba(255, 255, 255, 0.8);
  position: relative;
}

.modal-header-simple {
  text-align: center;
  margin-bottom: 32px;
}

.icon-circle-main {
  width: 64px;
  height: 64px;
  background: var(--modal-accent-soft);
  color: var(--modal-accent);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 24px;
  box-shadow: 0 4px 15px var(--modal-accent-soft);
}

/* Gold touches for Admin icon circle border */
.theme-admin .icon-circle-main {
  border: 2px solid var(--modal-touch);
}

.title-role {
  font-size: 24px;
  font-weight: 800;
  color: #0f172a;
  margin-bottom: 12px;
}

.subtitle-role {
  font-size: 15px;
  color: #64748b;
  line-height: 1.6;
}

.role-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.input-block {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.input-label {
  font-size: 13px;
  font-weight: 700;
  color: #475569;
}

.input-clean {
  width: 100%;
  padding: 14px 16px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 500;
  color: #0f172a;
  outline: none;
  transition: all 0.2s;
}

.input-clean:focus {
  border-color: var(--modal-accent);
  background: #ffffff;
  box-shadow: 0 0 0 4px var(--modal-accent-soft);
}

.error-banner {
  background: #fef2f2;
  border: 1px solid #fee2e2;
  color: #ef4444;
  padding: 12px;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 10px;
}

.btn-role-primary {
  width: 100%;
  padding: 16px;
  background: var(--modal-accent);
  color: #ffffff;
  border: none;
  border-radius: 14px;
  font-weight: 800;
  font-size: 14px;
  letter-spacing: 0.5px;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  box-shadow: 0 10px 15px -3px var(--modal-accent-soft);
}

.btn-role-primary:hover:not(:disabled) {
  background: var(--modal-accent-dark);
  transform: translateY(-2px);
  box-shadow: 0 20px 25px -5px var(--modal-accent-soft);
}

/* Gold touch for Admin primary button */
.theme-admin .btn-role-primary {
  border: 1px solid var(--modal-touch);
}

.btn-role-primary:disabled { opacity: 0.6; cursor: wait; }

/* Success Screen Styles */
.success-screen { text-align: center; padding: 20px 0; }

.success-icon-wrap {
  width: 80px;
  height: 80px;
  background: #ecfdf5;
  color: #10b981;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 24px;
  animation: bounce-in 0.6s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.redirect-bar {
  width: 100%;
  height: 4px;
  background: #f1f5f9;
  border-radius: 10px;
  margin-top: 32px;
  overflow: hidden;
}

.progress-bar-fill {
  height: 100%;
  background: var(--modal-accent);
  width: 0;
  animation: progress 2s linear forwards;
}

/* Gold touch for Admin progress bar */
.theme-admin .progress-bar-fill {
  background: linear-gradient(90deg, var(--modal-accent), var(--modal-touch));
}

.footer-simple {
  margin-top: 40px;
  text-align: center;
  font-size: 11px;
  color: #94a3b8;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.spin { animation: spin 1s linear infinite; }
@keyframes spin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
@keyframes progress { from { width: 0; } to { width: 100%; } }
@keyframes bounce-in { 0% { transform: scale(0); } 70% { transform: scale(1.1); } 100% { transform: scale(1); } }

.shake { animation: shake 0.5s ease-in-out; }
@keyframes shake {
  0%, 100% { transform: translateX(0); }
  10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
  20%, 40%, 60%, 80% { transform: translateX(5px); }
}

@media (max-width: 600px) {
  .onboarding-card-light { padding: 32px 24px; border-radius: 0; }
  .input-grid { grid-template-columns: 1fr; }
}
</style>
