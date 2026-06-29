<template>
  <div class="auth-celestial">
    <div class="auth-bg" aria-hidden="true">
      <div class="bg-grid"></div>
      <div class="bg-aurora"></div>
      <div class="bg-orb bg-orb-cyan"></div>
      <div class="bg-orb bg-orb-purple"></div>
      <div class="bg-orb bg-orb-gold"></div>
      <div class="bg-stars">
        <span></span><span></span><span></span><span></span><span></span>
        <span></span><span></span><span></span><span></span><span></span>
        <span></span><span></span><span></span><span></span><span></span>
      </div>
      <div class="bg-scan"></div>
    </div>

    <div class="auth-layout">
      <!-- LEFT: Welcome Panel -->
      <div class="welcome-panel">
        <div class="welcome-inner">
          <div class="brand-strip">
            <img :src="logoUrl" alt="NovaHire" class="brand-logo">
            <div class="brand-glow"></div>
          </div>
          
          <h1 class="welcome-title">
            Bienvenue<br>
            <span class="title-accent">sur NovaHire</span>
          </h1>
          <p class="welcome-desc">Votre écosystème de recrutement intelligent. Gérez vos talents, analysez les candidatures et trouvez les meilleurs profils grâce à l'IA.</p>
          
          <div class="trust-badges">
            <div class="trust-item">
              <div class="trust-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
              </div>
              <div class="trust-text">
                <strong>Sécurisé</strong>
                <span>Chiffrement de bout en bout</span>
              </div>
            </div>
            <div class="trust-item">
              <div class="trust-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
              </div>
              <div class="trust-text">
                <strong>Disponible 24/7</strong>
                <span>Accès permanent</span>
              </div>
            </div>
            <div class="trust-item">
              <div class="trust-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
              </div>
              <div class="trust-text">
                <strong class="recruiters-counter" :class="{ 'is-animating': isCounterAnimating }">{{ recruitersCountLabel }}</strong>
                <span>Utilisent NovaHire</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- RIGHT: Login Form -->
      <div class="form-panel">
        <div class="login-card glass-panel">
          <div class="card-header">
            <span class="greeting-chip">Connexion</span>
            <h2>Content de vous revoir</h2>
            <p>Entrez vos identifiants pour accéder à votre espace.</p>
          </div>

          <form @submit.prevent="handleLogin" class="login-form">
            <div class="field" :class="{ 'has-error': errors.email }">
              <label for="login-email">Adresse e-mail</label>
              <div class="input-shell">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                <input id="login-email" type="email" v-model="email" placeholder="Votre adresse e-mail" autocomplete="email" required @input="validateField('email')" />
              </div>
              <span v-if="errors.email" class="field-error">{{ errors.email }}</span>
            </div>

            <div class="field" :class="{ 'has-error': errors.password }">
              <div class="label-row">
                <label for="login-pw">Mot de passe</label>
                <router-link to="/mot-de-passe-oublie" class="forgot-link">Oublié ?</router-link>
              </div>
              <div class="input-shell">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                <input id="login-pw" type="password" v-model="password" placeholder="Votre mot de passe" autocomplete="current-password" required @input="validateField('password')" />
              </div>
              <span v-if="errors.password" class="field-error">{{ errors.password }}</span>
            </div>

            <div class="field remember-field">
              <label class="checkbox-label">
                <input type="checkbox" v-model="rememberMe" />
                <span class="checkbox-custom" aria-hidden="true"></span>
                <span class="checkbox-text">Se souvenir de moi</span>
              </label>
            </div>

            <button type="submit" class="submit-btn" :disabled="isLoading">
              <span v-if="!isLoading">Se connecter</span>
              <span v-else class="loader-pulse">Connexion en cours...</span>
            </button>

            <transition name="msg-fade">
              <div v-if="errorMessage" class="error-banner">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                {{ errorMessage }}
              </div>
            </transition>
          </form>

          <div class="card-footer">
            <div class="divider"><span>Nouveau sur NovaHire ?</span></div>
            <router-link to="/inscription" class="register-link">
              Créer un compte entreprise
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '@/api/axios'
import logoUrl from '@/assets/Logo_NovaHire.png'
import { useAuthStore } from '@/stores/authStore'

export default {
  name: 'Login',
  data() {
    return {
      logoUrl,
      email: '',
      password: '',
      rememberMe: false,
      isLoading: false,
      errorMessage: '',
      errors: { email: '', password: '' },
      authStore: useAuthStore(),
      parallaxX: 0,
      parallaxY: 0,
      recruitersCount: 2500,
      displayedRecruitersCount: 0,
      counterAnimationFrameId: null,
      isCounterAnimating: false
    }
  },
  computed: {
    parallaxStyle() {
      return {
        transform: `translate(${this.parallaxX}px, ${this.parallaxY}px)`,
        transition: 'transform 0.1s ease-out'
      }
    },
    recruitersCountLabel() {
      const count = Number.isFinite(this.displayedRecruitersCount) ? this.displayedRecruitersCount : 0
      return `+${new Intl.NumberFormat('fr-FR').format(Math.max(0, count))} recruteurs`
    }
  },
  created() {
    this.fetchRecruitersCount()
    const rememberedEmail = localStorage.getItem('rememberedEmail')
    if (rememberedEmail) {
      this.email = rememberedEmail
      this.rememberMe = true
    }
    // Ancien comportement : mot de passe mémorisé → provoque des 401 après reset
    localStorage.removeItem('rememberedPassword')
  },
  beforeUnmount() {
    if (this.counterAnimationFrameId) {
      cancelAnimationFrame(this.counterAnimationFrameId)
    }
  },
  methods: {
    animateRecruitersCount(targetCount) {
      const endCount = Math.max(0, Math.trunc(targetCount))
      const startCount = Math.max(0, Math.trunc(this.displayedRecruitersCount || 0))
      const totalDelta = Math.abs(endCount - startCount)

      if (this.counterAnimationFrameId) {
        cancelAnimationFrame(this.counterAnimationFrameId)
      }

      if (totalDelta === 0) {
        this.displayedRecruitersCount = endCount
        this.isCounterAnimating = false
        return
      }

      const duration = Math.min(1800, Math.max(900, totalDelta / 3))
      const startTime = performance.now()
      this.isCounterAnimating = true

      const step = (now) => {
        const progress = Math.min(1, (now - startTime) / duration)
        const eased = 1 - Math.pow(1 - progress, 3)
        this.displayedRecruitersCount = Math.round(startCount + (endCount - startCount) * eased)

        if (progress < 1) {
          this.counterAnimationFrameId = requestAnimationFrame(step)
        } else {
          this.displayedRecruitersCount = endCount
          this.counterAnimationFrameId = null
          this.isCounterAnimating = false
        }
      }

      this.counterAnimationFrameId = requestAnimationFrame(step)
    },
    async fetchRecruitersCount() {
      let count = 2500
      try {
        const { data } = await api.get('/public/platform-stats')
        const apiCount = Number(data?.recruitersCount)
        if (Number.isFinite(apiCount) && apiCount >= 0) {
          count = Math.trunc(apiCount)
        }
      } catch (err) {
        count = 2500
      }

      this.recruitersCount = count
      this.animateRecruitersCount(count)
    },
    async handleLogin() {
      this.validateField('email')
      this.validateField('password')
      if (this.errors.email || this.errors.password) return

      this.isLoading = true
      this.errorMessage = ''
      try {
        const data = await this.authStore.login(this.email.trim(), this.password.trim(), this.rememberMe)
        const role = (data.role || '').toLowerCase()

        if (this.rememberMe) {
          localStorage.setItem('rememberedEmail', this.email.trim())
        } else {
          localStorage.removeItem('rememberedEmail')
        }
        localStorage.removeItem('rememberedPassword')
        
        if (role === 'superadmin') {
          this.$router.push('/superadmin/dashboard')
        } else if (['companyadmin', 'recruiter', 'admin', 'company'].includes(role)) {
          this.$router.push('/dashboard')
        } else {
          this.errorMessage = "Vous n'avez pas accès à ce portail."
          this.authStore.logout()
        }
      } catch (err) {
        this.errorMessage = err.message || 'Identifiants incorrects. Veuillez réessayer.'
        setTimeout(() => { this.errorMessage = '' }, 4000)
      } finally {
        this.isLoading = false
      }
    },
    validateField(field) {
      if (field === 'email') {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
        if (!this.email) this.errors.email = 'Veuillez entrer votre email'
        else if (!emailRegex.test(this.email)) this.errors.email = 'Format d\'email invalide'
        else this.errors.email = ''
      }
      if (field === 'password') {
        if (!this.password) this.errors.password = 'Veuillez entrer votre mot de passe'
        else if (this.password.length < 6) this.errors.password = '6 caractères minimum'
        else this.errors.password = ''
      }
    }
  }
}
</script>

<style scoped>
/* ─── BASE ─── */
.auth-celestial {
  min-height: 100vh;
  background:
    radial-gradient(ellipse at 20% 10%, rgba(0,167,225,0.08) 0%, transparent 55%),
    radial-gradient(ellipse at 85% 90%, rgba(139,92,246,0.10) 0%, transparent 55%),
    #020617;
  font-family: 'Inter', sans-serif;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  position: relative;
  overflow: hidden;
}

/* ─── ANIMATED BACKGROUND ─── */
.auth-bg {
  position: absolute; inset: 0;
  z-index: 1;
  overflow: hidden;
  pointer-events: none;
}

.bg-grid {
  position: absolute; inset: -2px;
  background-image:
    linear-gradient(rgba(0,167,225,0.10) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0,167,225,0.10) 1px, transparent 1px);
  background-size: 52px 52px;
  -webkit-mask-image: radial-gradient(ellipse at center, black 35%, transparent 78%);
  mask-image: radial-gradient(ellipse at center, black 35%, transparent 78%);
  animation: gridDrift 12s linear infinite;
  opacity: 0.7;
}
@keyframes gridDrift {
  from { background-position: 0 0, 0 0; }
  to   { background-position: 52px 52px, 52px 52px; }
}

.bg-aurora {
  position: absolute; inset: -25%;
  background: conic-gradient(from 140deg at 50% 50%,
    rgba(0,167,225,0.00) 0deg,
    rgba(0,167,225,0.22) 60deg,
    rgba(139,92,246,0.28) 140deg,
    rgba(247,201,2,0.16) 220deg,
    rgba(0,167,225,0.22) 300deg,
    rgba(0,167,225,0.00) 360deg);
  filter: blur(90px);
  animation: auroraSpin 22s linear infinite;
  opacity: 0.85;
  transform-origin: center center;
}
@keyframes auroraSpin {
  from { transform: rotate(0deg); }
  to   { transform: rotate(360deg); }
}

.bg-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  mix-blend-mode: screen;
  will-change: transform;
}
.bg-orb-cyan {
  width: 560px; height: 560px;
  background: radial-gradient(circle, #00A7E1 0%, rgba(0,167,225,0) 70%);
  top: -12%; left: -10%;
  opacity: 0.65;
  animation: drift1 10s ease-in-out infinite alternate;
}
.bg-orb-purple {
  width: 480px; height: 480px;
  background: radial-gradient(circle, #8B5CF6 0%, rgba(139,92,246,0) 70%);
  bottom: -18%; right: -12%;
  opacity: 0.65;
  animation: drift2 12s ease-in-out infinite alternate;
}
.bg-orb-gold {
  width: 360px; height: 360px;
  background: radial-gradient(circle, #F7C902 0%, rgba(247,201,2,0) 70%);
  top: 42%; left: 46%;
  opacity: 0.38;
  animation: drift3 14s ease-in-out infinite alternate;
}
@keyframes drift1 {
  0%   { transform: translate(0, 0) scale(1); }
  100% { transform: translate(260px, 180px) scale(1.25); }
}
@keyframes drift2 {
  0%   { transform: translate(0, 0) scale(1); }
  100% { transform: translate(-240px, -160px) scale(1.2); }
}
@keyframes drift3 {
  0%   { transform: translate(0, 0) scale(0.85); }
  100% { transform: translate(180px, -200px) scale(1.3); }
}

.bg-stars span {
  position: absolute;
  width: 2px; height: 2px; border-radius: 50%;
  background: #ffffff;
  box-shadow: 0 0 6px rgba(255,255,255,0.85);
  opacity: 0.55;
  animation: twinkle 1.6s ease-in-out infinite;
}
.bg-stars span:nth-child(1)  { top: 12%; left: 18%; animation-delay: 0s; }
.bg-stars span:nth-child(2)  { top: 22%; left: 72%; animation-delay: 0.4s; }
.bg-stars span:nth-child(3)  { top: 38%; left: 28%; animation-delay: 0.9s; }
.bg-stars span:nth-child(4)  { top: 62%; left: 62%; animation-delay: 1.3s; width: 3px; height: 3px; }
.bg-stars span:nth-child(5)  { top: 78%; left: 12%; animation-delay: 1.8s; }
.bg-stars span:nth-child(6)  { top: 84%; left: 84%; animation-delay: 2.3s; }
.bg-stars span:nth-child(7)  { top: 8%;  left: 46%; animation-delay: 0.2s; }
.bg-stars span:nth-child(8)  { top: 52%; left: 8%;  animation-delay: 0.8s; width: 3px; height: 3px; }
.bg-stars span:nth-child(9)  { top: 30%; left: 92%; animation-delay: 1.1s; }
.bg-stars span:nth-child(10) { top: 70%; left: 38%; animation-delay: 1.6s; width: 3px; height: 3px; }
.bg-stars span:nth-child(11) { top: 18%; left: 58%; animation-delay: 2.1s; }
.bg-stars span:nth-child(12) { top: 48%; left: 78%; animation-delay: 0.5s; }
.bg-stars span:nth-child(13) { top: 6%;  left: 88%; animation-delay: 1.0s; width: 3px; height: 3px; }
.bg-stars span:nth-child(14) { top: 90%; left: 52%; animation-delay: 1.5s; }
.bg-stars span:nth-child(15) { top: 34%; left: 6%;  animation-delay: 2.0s; }
@keyframes twinkle {
  0%, 100% { opacity: 0.15; transform: scale(0.7); }
  50%     { opacity: 0.95; transform: scale(1.25); }
}

.bg-scan {
  position: absolute; left: 0; right: 0;
  height: 180px;
  background: linear-gradient(180deg,
    rgba(0,167,225,0) 0%,
    rgba(0,167,225,0.22) 50%,
    rgba(0,167,225,0) 100%);
  filter: blur(2px);
  animation: scanMove 5s linear infinite;
}
@keyframes scanMove {
  0%   { top: -180px; opacity: 0; }
  10%  { opacity: 1; }
  90%  { opacity: 1; }
  100% { top: 100%;   opacity: 0; }
}



/* ─── LAYOUT ─── */
.auth-layout {
  position: relative; z-index: 10;
  display: grid; grid-template-columns: 1fr 1fr;
  max-width: 1080px; width: 100%;
  gap: 0;
  min-height: 640px;
}

/* ─── LEFT: WELCOME ─── */
.welcome-panel {
  display: flex; flex-direction: column; justify-content: center;
  padding: 60px 56px 60px 20px;
}

.brand-strip { position: relative; margin-bottom: 44px; width: fit-content; }
.brand-logo {
  height: 180px;
  width: auto;
  position: relative;
  z-index: 2;
  display: block;
  user-select: none;
  -webkit-user-select: none;
  -webkit-user-drag: none;
  -webkit-touch-callout: none;
  pointer-events: none;
}
.brand-glow {
  position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
  width: 260px; height: 260px; border-radius: 50%;
  background: radial-gradient(circle, rgba(0,167,225,0.35) 0%, rgba(0,167,225,0) 70%);
  filter: blur(24px);
  opacity: 0.9;
  pointer-events: none;
}

.welcome-title {
  font-size: 46px; font-weight: 900; line-height: 1.1;
  letter-spacing: -2px; margin: 0 0 20px;
  color: rgba(255,255,255,0.95);
}
.title-accent {
  background: linear-gradient(135deg, #00A7E1 0%, #8B5CF6 50%, #F7C902 100%);
  background-clip: text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-size: 200% 200%;
  animation: gradientShift 6s ease infinite;
}
@keyframes gradientShift {
  0%, 100% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
}

.welcome-desc {
  font-size: 15px; line-height: 1.7; color: rgba(255,255,255,0.78);
  max-width: 400px; margin-bottom: 48px; font-weight: 400;
}

/* Trust Badges */
.trust-badges { display: flex; flex-direction: column; gap: 20px; }
.trust-item {
  display: flex; align-items: center; gap: 16px;
  padding: 14px 18px; border-radius: 16px;
  background: rgba(255,255,255,0.03);
  border: 1px solid rgba(255,255,255,0.06);
  transition: 0.3s;
}
.trust-item:hover { background: rgba(255,255,255,0.06); border-color: rgba(255,255,255,0.1); }

.trust-icon {
  width: 40px; height: 40px; border-radius: 12px;
  background: rgba(0, 167, 225, 0.1);
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}
.trust-icon svg { width: 18px; color: #00A7E1; }
.trust-item:nth-child(2) .trust-icon { background: rgba(139, 92, 246, 0.1); }
.trust-item:nth-child(2) .trust-icon svg { color: #8B5CF6; }
.trust-item:nth-child(3) .trust-icon { background: rgba(247, 201, 2, 0.1); }
.trust-item:nth-child(3) .trust-icon svg { color: #F7C902; }

.trust-text strong { display: block; font-size: 13px; font-weight: 700; color: #ffffff; }
.trust-text span { font-size: 12px; color: rgba(255,255,255,0.7); font-weight: 400; }

.recruiters-counter {
  display: inline-block;
  font-variant-numeric: tabular-nums;
}

.recruiters-counter.is-animating {
  animation: counterGlow 0.9s ease-in-out infinite alternate;
}

@keyframes counterGlow {
  from { text-shadow: 0 0 0 rgba(247, 201, 2, 0); }
  to { text-shadow: 0 0 12px rgba(247, 201, 2, 0.35); }
}

/* ─── RIGHT: FORM ─── */
.form-panel {
  display: flex; align-items: center; justify-content: center;
}

.glass-panel {
  background: rgba(13, 20, 32, 0.95); /* Opaque for absolute clarity */
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 32px;
  width: 100%;
  max-width: 480px;
  padding: 50px;
  position: relative;
  z-index: 10;
  box-shadow: 0 40px 100px rgba(0,0,0,0.8);
  border-top-color: rgba(255,255,255,0.2);
  border-left-color: rgba(255,255,255,0.2);
}

.login-card { padding: 48px; width: 420px; }

/* Card Header */
.card-header { margin-bottom: 36px; text-align: center; }
.greeting-chip {
  display: inline-block; padding: 6px 14px; border-radius: 100px;
  background: rgba(0, 167, 225, 0.1); color: #00A7E1;
  font-size: 12px; font-weight: 700; letter-spacing: 0.5px;
  margin-bottom: 20px;
  border: 1px solid rgba(0, 167, 225, 0.15);
}
.card-header h2 {
  font-size: 26px; font-weight: 800; margin: 0 0 8px;
  letter-spacing: -0.5px; color: white;
}
.card-header p {
  font-size: 14px; color: rgba(255,255,255,0.78); margin: 0;
  font-weight: 400; line-height: 1.5;
}

/* Form */
.login-form { display: flex; flex-direction: column; gap: 20px; }

.remember-field {
  display: flex; align-items: center;
  margin-top: -4px;
}

.checkbox-label {
  display: inline-flex; align-items: center; gap: 12px;
  color: rgba(255, 255, 255, 0.88);
  font-size: 13px; font-weight: 600;
  cursor: pointer;
  user-select: none;
}

.checkbox-label input {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
}

.checkbox-custom {
  width: 18px; height: 18px;
  border: 1.5px solid rgba(255,255,255,0.2);
  border-radius: 8px;
  background: rgba(255,255,255,0.05);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  box-shadow: inset 0 1px 2px rgba(0,0,0,0.15);
  margin-right: 10px;
}

.checkbox-custom::after {
  content: '';
  width: 7px; height: 11px;
  border: solid #00A7E1;
  border-width: 0 2px 2px 0;
  transform: rotate(45deg) scale(0.65);
  opacity: 0;
  transition: all 0.2s ease;
}

.checkbox-label input:checked + .checkbox-custom {
  border-color: #00A7E1;
  background: rgba(0,167,225,0.18);
}

.checkbox-label input:checked + .checkbox-custom::after {
  opacity: 1;
  transform: rotate(45deg) scale(1);
}

.checkbox-text {
  color: rgba(255, 255, 255, 0.88);
}

.field label {
  display: block; font-size: 13px; font-weight: 600;
  color: rgba(255,255,255,0.88); margin-bottom: 10px;
}
.label-row { display: flex; justify-content: space-between; align-items: center; }
.forgot-link {
  font-size: 12px;
  color: #0ea5e9;
  text-decoration: none;
  font-weight: 600;
  transition: color 0.3s ease;
}
.forgot-link:hover {
  color: #F7C902;
}

.input-shell { position: relative; }
.input-shell svg {
  position: absolute; left: 18px; top: 50%; transform: translateY(-50%);
  width: 18px; color: #00A7E1; transition: 0.3s; z-index: 2;
}

.input-shell input {
  width: 100%;
  background: rgba(255, 255, 255, 0.05);
  border: 1.5px solid rgba(255, 255, 255, 0.1);
  border-radius: 14px;
  padding: 16px 18px 16px 52px;
  color: white; /* Base color for dark theme */
  font-family: inherit; font-size: 14px; font-weight: 500;
  transition: all 0.3s ease;
}
.input-shell input::placeholder { color: rgba(255,255,255,0.5); }
.input-shell input:focus {
  outline: none;
  border-color: rgba(0,167,225,0.5);
  background: rgba(0,167,225,0.06);
  box-shadow: 0 0 0 4px rgba(0,167,225,0.08);
}
.input-shell input:focus ~ svg { color: #00A7E1; }

/* Error States */
.field.has-error .input-shell input {
  border-color: rgba(239, 68, 68, 0.4) !important;
  background: rgba(239, 68, 68, 0.04) !important;
}
.field.has-error .input-shell svg { color: #f87171 !important; }
.field-error {
  display: block; margin-top: 8px;
  color: #f87171; font-size: 12px; font-weight: 600;
  animation: slideUp 0.2s ease-out;
}
@keyframes slideUp {
  from { opacity: 0; transform: translateY(4px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Submit Button */
.submit-btn {
  display: flex; align-items: center; justify-content: center; gap: 10px;
  width: 100%; padding: 18px;
  background: linear-gradient(135deg, #00A7E1 0%, #0077B6 100%);
  color: white; border: none; border-radius: 14px;
  font-family: inherit; font-size: 15px; font-weight: 700;
  cursor: pointer; transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  margin-top: 8px;
  box-shadow: 0 8px 24px rgba(0, 167, 225, 0.25);
}
.submit-btn svg { width: 18px; transition: transform 0.3s; }
.submit-btn:hover:not(:disabled) {
  transform: translateY(-3px);
  box-shadow: 0 16px 40px rgba(0, 167, 225, 0.35);
}
.submit-btn:hover svg { transform: translateX(4px); }
.submit-btn:active:not(:disabled) { transform: translateY(-1px); }
.submit-btn:disabled { opacity: 0.5; cursor: not-allowed; }

/* Error Banner */
.error-banner {
  padding: 14px 16px; border-radius: 12px;
  background: rgba(239, 68, 68, 0.08); color: #f87171;
  font-size: 13px; font-weight: 600;
  display: flex; align-items: center; gap: 10px;
  border: 1px solid rgba(239, 68, 68, 0.15);
}
.error-banner svg { width: 16px; flex-shrink: 0; }

/* Footer */
.card-footer { margin-top: 32px; }
.divider {
  text-align: center; position: relative; margin-bottom: 20px;
}
.divider::before, .divider::after {
  content: ''; position: absolute; top: 50%;
  width: calc(50% - 80px); height: 1px;
  background: rgba(255,255,255,0.06);
}
.divider::before { left: 0; }
.divider::after { right: 0; }
.divider span { font-size: 12px; color: rgba(255,255,255,0.65); font-weight: 500; }

.register-link {
  display: flex; align-items: center; justify-content: center; gap: 8px;
  width: 100%; padding: 16px;
  background: transparent;
  border: 1.5px solid rgba(255,255,255,0.14);
  border-radius: 14px;
  color: rgba(255,255,255,0.85);
  font-size: 14px; font-weight: 600;
  text-decoration: none;
  transition: all 0.3s;
}
.register-link svg { width: 16px; transition: transform 0.3s; }
.register-link:hover {
  border-color: rgba(255,255,255,0.2);
  color: white;
  background: rgba(255,255,255,0.04);
}
.register-link:hover svg { transform: translateX(4px); }

/* Transitions */
.msg-fade-enter-active, .msg-fade-leave-active { transition: all 0.3s ease; }
.msg-fade-enter-from, .msg-fade-leave-to { opacity: 0; transform: translateY(-6px); }



.loader-pulse { animation: pulse 1.5s ease-in-out infinite; }
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.5; } }

/* ─── RESPONSIVE ─── */
@media (max-width: 960px) {
  .auth-layout { grid-template-columns: 1fr; max-width: 480px; }
  .welcome-panel { display: none; }
  .login-card { width: 100%; }
}

@media (max-width: 500px) {
  .login-card { padding: 32px 24px; }
  .card-header h2 { font-size: 22px; }
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
:global(body:not(.dark-mode) .input-shell svg) {
  color: rgba(0, 0, 0, 0.4);
}
:global(body:not(.dark-mode) .checkbox-label) {
  color: rgba(15, 23, 42, 0.88);
}
:global(body:not(.dark-mode) .checkbox-text) {
  color: rgba(15, 23, 42, 0.88);
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
</style>
