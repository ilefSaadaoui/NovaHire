<template>
  <div class="auth-celestial">
    <div class="auth-layout">
      <!-- LEFT: Welcome Panel -->
      <div class="welcome-panel">
        <div class="welcome-inner">
          <div class="brand-strip">
            <img :src="logoUrl" alt="NovaHire" class="brand-logo">
            <div class="brand-glow"></div>
          </div>
          
          <h1 class="welcome-title">
            Récupérez<br>
            <span class="title-accent">votre accès</span>
          </h1>
          <p class="welcome-desc">Saisissez votre adresse e-mail pour recevoir un lien sécurisé de réinitialisation de votre mot de passe.</p>
          
          <div class="trust-badges">
            <div class="trust-item">
              <div class="trust-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
              </div>
              <div class="trust-text">
                <strong>Sécurisé</strong>
                <span>Protection des données certifiée</span>
              </div>
            </div>
            <div class="trust-item">
              <div class="trust-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
              </div>
              <div class="trust-text">
                <strong>Réponse Rapide</strong>
                <span>Lien envoyé instantanément</span>
              </div>
            </div>
            <div class="trust-item">
              <div class="trust-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
              </div>
              <div class="trust-text">
                <strong>Support 24/7</strong>
                <span>Assistance technique dédiée</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- RIGHT: Form Panel -->
      <div class="form-panel">
        <div class="login-card glass-panel">
          <button @click="$router.push('/connexion')" class="btn-back">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="15 18 9 12 15 6"/></svg>
            Retour
          </button>

          <div class="card-header">
            <span class="greeting-chip text-purple">Réinitialisation</span>
            <h2>Mot de passe oublié</h2>
            <p>Saisissez l'email associé à votre compte NovaHire pour continuer.</p>
          </div>

          <form v-if="!isSuccess" @submit.prevent="handlePasswordResetRequest" class="login-form">
            <div class="field" :class="{ 'has-error': errorMessage }">
              <label for="email">Adresse Email</label>
              <div class="input-shell">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                <input id="email" type="email" v-model="email" placeholder="nom@entreprise.com" required />
              </div>
            </div>

            <button type="submit" class="submit-btn" :disabled="isLoading">
              <span v-if="!isLoading">Envoyer le lien de secours</span>
              <span v-else class="loader-pulse">Envoi en cours...</span>
            </button>

            <transition name="msg-fade">
              <div v-if="errorMessage" class="error-banner">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                {{ errorMessage }}
              </div>
            </transition>
          </form>

          <!-- Success State -->
          <div v-else class="success-state">
            <div class="success-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
            </div>
            <h3>Email Envoyé</h3>
            <p>Si cet email existe dans notre base, vous recevrez un lien de réinitialisation d'ici quelques instants.</p>
            <button @click="isSuccess = false" class="secondary-btn">
              Ressayer avec un autre email
            </button>
          </div>

          <div class="card-footer">
            <div class="divider">
              <span>OU</span>
            </div>
            <router-link to="/connexion" class="register-link">
              Retourner à la connexion
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import logoUrl from '@/assets/Logo_NovaHire.png'
import authService from '@/services/authService'

export default {
  name: 'ForgotPassword',
  data() {
    return {
      logoUrl,
      email: '',
      isLoading: false,
      isSuccess: false,
      errorMessage: ''
    }
  },
  methods: {
    async handlePasswordResetRequest() {
      this.errorMessage = ''
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
      if (!emailRegex.test(this.email)) {
        this.errorMessage = 'Veuillez saisir une adresse email valide.'
        return
      }
      this.isLoading = true
      try {
        await authService.requestPasswordReset(this.email)
        this.isSuccess = true
      } catch (error) {
        this.errorMessage = error.message || 'Échec de l\'envoi du lien.'
      } finally {
        this.isLoading = false
      }
    }
  }
}
</script>

<style scoped>
/* ─── BASE ─── */
.auth-celestial {
  min-height: 100vh;
  background: #020617 !important;
  font-family: 'Inter', sans-serif;
  color: white;
  display: flex;
  align-items: center; justify-content: center;
  padding: 40px 20px; position: relative; overflow: hidden;
}

/* ─── LAYOUT ─── */
.auth-layout {
  position: relative; z-index: 10;
  display: grid; grid-template-columns: 1fr 1fr;
  max-width: 1080px; width: 100%; min-height: 640px;
}

/* ─── LEFT: WELCOME ─── */
.welcome-panel {
  display: flex; flex-direction: column; justify-content: center;
  padding: 60px 40px 60px 20px;
}
.brand-strip { position: relative; margin-bottom: 44px; width: fit-content; }
.brand-logo { height: 44px; position: relative; z-index: 2; }
.brand-glow { position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); width: 50px; height: 50px; background: #00A7E1; opacity: 0.2; }

.welcome-title { font-size: 46px; font-weight: 900; line-height: 1.1; letter-spacing: -2px; margin: 0 0 20px; }
.title-accent {
  background: linear-gradient(135deg, #00A7E1 0%, #8B5CF6 50%, #F7C902 100%);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent;
  background-size: 200% 200%;
  animation: gradientShift 6s ease infinite;
}
@keyframes gradientShift { 0%, 100% { background-position: 0% 50%; } 50% { background-position: 100% 50%; } }

.welcome-desc { font-size: 15px; line-height: 1.7; color: rgba(255,255,255,0.4); max-width: 400px; margin-bottom: 48px; }

.trust-badges { display: flex; flex-direction: column; gap: 20px; }
.trust-item {
  display: flex; align-items: center; gap: 16px;
  padding: 14px 18px; border-radius: 16px;
  background: rgba(255,255,255,0.03); border: 1px solid rgba(255,255,255,0.06);
}
.trust-icon { width: 40px; height: 40px; border-radius: 12px; background: rgba(0, 167, 225, 0.1); display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.trust-icon svg { width: 18px; color: #00A7E1; }
.trust-item:nth-child(2) .trust-icon { background: rgba(139, 92, 246, 0.1); }
.trust-item:nth-child(2) .trust-icon svg { color: #8B5CF6; }
.trust-item:nth-child(3) .trust-icon { background: rgba(247, 201, 2, 0.1); }
.trust-item:nth-child(3) .trust-icon svg { color: #F7C902; }

.trust-text strong { display: block; font-size: 13px; font-weight: 700; color: rgba(255,255,255,0.85); }
.trust-text span { font-size: 12px; color: rgba(255,255,255,0.35); }

/* ─── RIGHT: FORM ─── */
.form-panel { display: flex; align-items: center; justify-content: center; }
.glass-panel {
  background: rgba(13, 20, 32, 0.95); border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 32px; width: 100%; max-width: 480px; padding: 48px;
  box-shadow: 0 40px 100px rgba(0,0,0,0.8);
}

.btn-back {
  display: flex; align-items: center; gap: 6px; background: none; border: none; color: rgba(255,255,255,0.4);
  font-size: 13px; font-weight: 700; cursor: pointer; margin-bottom: 24px; transition: 0.3s;
}
.btn-back:hover { color: white; transform: translateX(-4px); }
.btn-back svg { width: 14px; }

.card-header { margin-bottom: 36px; text-align: center; }
.greeting-chip {
  display: inline-block; padding: 6px 14px; border-radius: 100px;
  background: rgba(139, 92, 246, 0.1); color: #8B5CF6;
  font-size: 12px; font-weight: 700; border: 1px solid rgba(139, 92, 246, 0.15);
  margin-bottom: 20px;
}
.card-header h2 { font-size: 26px; font-weight: 800; margin-bottom: 8px; }
.card-header p { font-size: 14px; color: rgba(255,255,255,0.4); line-height: 1.5; }

.login-form { display: flex; flex-direction: column; gap: 24px; }
.field label { display: block; font-size: 13px; font-weight: 600; color: rgba(255,255,255,0.6); margin-bottom: 10px; }
.input-shell { position: relative; }
.input-shell svg { position: absolute; left: 18px; top: 50%; transform: translateY(-50%); width: 18px; color: #00A7E1; }
.input-shell input {
  width: 100%; background: rgba(255,255,255,0.04); border: 1.5px solid rgba(255,255,255,0.08);
  border-radius: 14px; padding: 16px 18px 16px 52px; color: white; font-size: 14px; transition: 0.3s;
}
.input-shell input:focus { outline: none; border-color: rgba(0,167,225,0.5); background: rgba(0,167,225,0.06); }

.submit-btn {
  width: 100%; padding: 18px; background: linear-gradient(135deg, #00A7E1 0%, #0077B6 100%);
  color: white; border: none; border-radius: 14px; font-size: 15px; font-weight: 700; cursor: pointer; transition: 0.4s;
  box-shadow: 0 8px 24px rgba(0, 167, 225, 0.25); text-align: center;
}
.submit-btn:hover:not(:disabled) { transform: translateY(-3px); box-shadow: 0 16px 40px rgba(0, 167, 225, 0.35); }
.submit-btn:disabled { opacity: 0.5; cursor: not-allowed; }

.error-banner {
  padding: 14px 16px; border-radius: 12px; background: rgba(239, 68, 68, 0.08); color: #f87171;
  font-size: 13px; font-weight: 600; display: flex; align-items: center; gap: 10px; border: 1px solid rgba(239, 68, 68, 0.15);
}
.error-banner svg { width: 16px; }

.success-state { text-align: center; padding: 20px 0; }
.success-icon {
  width: 64px; height: 64px; border-radius: 20px; background: rgba(16, 185, 129, 0.1);
  color: #10b981; display: flex; align-items: center; justify-content: center; margin: 0 auto 20px;
}
.success-icon svg { width: 32px; }
.success-state h3 { font-size: 20px; margin-bottom: 8px; }
.success-state p { font-size: 14px; color: rgba(255,255,255,0.5); margin-bottom: 24px; }

.secondary-btn {
  background: transparent; border: 1px solid rgba(255,255,255,0.1); color: rgba(255,255,255,0.6);
  padding: 12px 24px; border-radius: 12px; font-size: 13px; font-weight: 600; cursor: pointer; transition: 0.3s;
}
.secondary-btn:hover { background: rgba(255,255,255,0.05); color: white; }

.card-footer { margin-top: 32px; }
.divider { text-align: center; position: relative; margin-bottom: 20px; }
.divider::before, .divider::after { content: ''; position: absolute; top: 50%; width: 20%; height: 1px; background: rgba(255,255,255,0.06); }
.divider::before { left: 0; } .divider::after { right: 0; }
.divider span { font-size: 12px; color: rgba(255,255,255,0.3); }

.register-link {
  display: flex; align-items: center; justify-content: center; padding: 16px;
  background: transparent; border: 1.5px solid rgba(255,255,255,0.08); border-radius: 14px;
  color: rgba(255,255,255,0.6); font-size: 14px; font-weight: 600; text-decoration: none; transition: 0.3s;
}
.register-link:hover { border-color: rgba(255,255,255,0.2); color: white; background: rgba(255,255,255,0.04); }

.loader-pulse { animation: pulse 1.5s ease-in-out infinite; }
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }

@media (max-width: 960px) {
  .auth-layout { grid-template-columns: 1fr; max-width: 480px; }
  .welcome-panel { display: none; }
}

/* ─── LIGHT MODE OVERRIDES ─── */
:global(body:not(.dark-mode) .auth-celestial) {
  background: #f4f7fe !important;
  color: #0f172a;
}
:global(body:not(.dark-mode) .welcome-desc) {
  color: #475569;
}
:global(body:not(.dark-mode) .trust-item) {
  background: rgba(255, 255, 255, 0.8);
  border-color: rgba(0, 0, 0, 0.08);
}
:global(body:not(.dark-mode) .trust-text strong) {
  color: #0f172a;
}
:global(body:not(.dark-mode) .trust-text span) {
  color: #64748b;
}
:global(body:not(.dark-mode) .glass-panel) {
  background: rgba(255, 255, 255, 0.95);
  border-color: rgba(0, 0, 0, 0.08);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.05);
}
:global(body:not(.dark-mode) .btn-back) {
  color: #64748b;
}
:global(body:not(.dark-mode) .btn-back:hover) {
  color: #0f172a;
}
:global(body:not(.dark-mode) .card-header h2) {
  color: #0f172a;
}
:global(body:not(.dark-mode) .card-header p) {
  color: #64748b;
}
:global(body:not(.dark-mode) .field label) {
  color: #475569;
}
:global(body:not(.dark-mode) .input-shell input) {
  background: rgba(0, 0, 0, 0.03);
  border-color: rgba(0, 0, 0, 0.1);
  color: #0f172a;
}
:global(body:not(.dark-mode) .input-shell input::placeholder) {
  color: rgba(0, 0, 0, 0.45);
}
:global(body:not(.dark-mode) .input-shell svg) {
  color: rgba(0, 0, 0, 0.4);
}
:global(body:not(.dark-mode) .divider::before),
:global(body:not(.dark-mode) .divider::after) {
  background: rgba(0, 0, 0, 0.08);
}
:global(body:not(.dark-mode) .divider span) {
  color: #64748b;
}
:global(body:not(.dark-mode) .register-link) {
  border-color: rgba(0, 0, 0, 0.12);
  color: #475569;
}
:global(body:not(.dark-mode) .register-link:hover) {
  border-color: rgba(0, 0, 0, 0.2);
  color: #0f172a;
  background: rgba(0, 0, 0, 0.02);
}
:global(body:not(.dark-mode) .success-state h3) {
  color: #0f172a;
}
:global(body:not(.dark-mode) .success-state p) {
  color: #475569;
}
:global(body:not(.dark-mode) .secondary-btn) {
  border-color: rgba(0, 0, 0, 0.12);
  color: #475569;
}
:global(body:not(.dark-mode) .secondary-btn:hover) {
  background: rgba(0, 0, 0, 0.02);
  color: #0f172a;
}
</style>
