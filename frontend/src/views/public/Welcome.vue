<template>
  <div class="welcome-page auth-celestial" @mousemove="onMouseMove">
    <!-- TELEPORTED BACKGROUND (MATCHES LOGIN DESIGN) -->
    <Teleport to="body">
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
        <div
          class="welcome-bg-cursor-glow"
          :style="{ transform: `translate3d(${mouse.x}px, ${mouse.y}px, 0)` }"
        ></div>
      </div>
    </Teleport>

    <!-- Custom Interactive Cursor (Premium Follower) - Teleported for global precision -->
    <Teleport to="body">
      <div class="custom-cursor-follower" :style="followerStyle">
        <div class="follower-inner"></div>
        <div class="follower-dot"></div>
      </div>
    </Teleport>

    <!-- HEADER -->
    <header class="topbar container glass-panel">
      <div class="logo-group">
        <img :src="logoUrl" alt="NovaHire" class="brand-logo" />
        <div class="brand-glow"></div>
      </div>

      <nav class="top-nav" aria-label="Main">
        <a href="#solutions" class="nav-link">Solutions</a>
        <a href="#performance" class="nav-link">Performance</a>
        <a href="#contact" class="nav-link">Contact</a>
      </nav>

      <div class="top-actions">
        <AuthThemeToggle />
        <router-link to="/connexion" class="btn btn-ghost">Se connecter</router-link>
        <router-link to="/inscription/entreprise" class="btn btn-accent">Créer un compte</router-link>
      </div>
    </header>

    <main>
      <!-- HERO SECTION -->
      <section class="hero container">
        <div class="hero-copy">
          <p class="eyebrow">Plateforme NovaHire</p>
          <h1 class="welcome-title">
            <span class="typewriter-text">{{ typingText1 }}</span><span v-if="isTyping1 && showCursor" class="cursor">|</span><br>
            <span class="title-accent typewriter-text">{{ typingText2 }}</span><span v-if="!isTyping1 && showCursor" class="cursor">|</span>
          </h1>
          <p class="hero-desc">
            Centralisez vos offres, automatisez le tri des candidatures et donnez à vos équipes
            un pilotage clair, rapide et orienté résultat grâce à l'IA.
          </p>

          <div class="hero-cta">
            <router-link to="/connexion" class="btn btn-accent">Commencer</router-link>
            <router-link to="/inscription/entreprise" class="btn btn-outline">Compte Entreprise</router-link>
          </div>
        </div>

        <div class="hero-visual-container animate-float-slow">
          <div class="perspective-wrapper">
            <div class="hero-visual-3d glass-panel" ref="visual3d">
              <div class="visual-inner">
                <img src="@/assets/Methode-de-recrutement-scaled.jpg" alt="Smart Recruitment" class="hero-img" />
                <div class="img-overlay"></div>
              </div>
              <div class="glass-accent top-right"></div>
              <div class="glass-accent bottom-left"></div>
            </div>
            <!-- Decorative elements -->
            <div class="deco-blob blob-1"></div>
            <div class="deco-blob blob-2"></div>
          </div>
        </div>
      </section>

      <!-- KPI GRID -->
      <section class="kpi-section container">
        <div class="kpi-grid">
          <article class="kpi-card glass-panel hov-lift">
            <div class="kpi-icon b-blue-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><polyline points="16 11 18 13 22 9"/></svg>
            </div>
            <h3 class="stat-blue">1200 +</h3>
            <p>Candidatures traitées avec précision chirurgicale.</p>
          </article>
          <article class="kpi-card glass-panel hov-lift">
            <div class="kpi-icon b-gold-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
            </div>
            <h3 class="stat-gold">72h</h3>
            <p>Délai moyen pour identifier le candidat idéal.</p>
          </article>
          <article class="kpi-card glass-panel hov-lift">
            <div class="kpi-icon b-purple-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
            </div>
            <h3 class="stat-purple">96%</h3>
            <p>Taux de satisfaction de nos clients recruteurs.</p>
          </article>
          <article class="kpi-card glass-panel hov-lift">
            <div class="kpi-icon b-blue-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="3" width="20" height="14" rx="2" ry="2"/><line x1="8" y1="21" x2="16" y2="21"/><line x1="12" y1="17" x2="12" y2="21"/></svg>
            </div>
            <h3 class="stat-blue">24/7</h3>
            <p>Accès cloud ultra-sécurisé pour vos équipes.</p>
          </article>
        </div>
      </section>

      <!-- SOLUTIONS -->
      <section id="solutions" class="solutions container">
        <div class="section-title">
          <p class="eyebrow">Nos Capacités</p>
          <h2 class="welcome-title">Des solutions conçues pour <span class="title-accent">vos performances</span></h2>
        </div>

        <div class="solution-grid">
          <article v-for="(item, i) in capabilities" :key="item.title" class="solution-card glass-panel hov-lift">
            <p class="mini-tag">{{ item.tag }}</p>
            <h3>{{ item.title }}</h3>
            <p>{{ item.text }}</p>
          </article>
        </div>
      </section>

      <!-- CONTACT -->
      <section id="contact" class="contact-strip container glass-panel">
        <div class="contact-header">
          <h2 class="welcome-title">Parlez à un <span class="title-accent">expert</span></h2>
          <p class="hero-desc">Analyse personnalisée de vos besoins de recrutement.</p>
        </div>

        <form class="contact-form" @submit.prevent="submitCallbackRequest">
          <div class="contact-form-grid">
            <div class="field">
              <label>Nom complet</label>
              <div class="input-shell">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                <input v-model.trim="callbackForm.fullName" type="text" placeholder="Votre nom complet" required />
              </div>
            </div>
            <div class="field">
              <label>Entreprise</label>
              <div class="input-shell">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="10" width="20" height="11" rx="2"/><path d="M7 10V5a2 2 0 0 1 2-2h6a2 2 0 0 1 2 2v5"/></svg>
                <input v-model.trim="callbackForm.company" type="text" placeholder="Nom de votre entreprise" />
              </div>
            </div>
            <div class="field">
              <label>Téléphone</label>
              <div class="input-shell">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
                <input v-model.trim="callbackForm.phone" type="tel" placeholder="+216 XX XXX XXX" />
              </div>
            </div>
            <div class="field">
              <label>E-mail</label>
              <div class="input-shell">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                <input v-model.trim="callbackForm.email" type="email" placeholder="votre@email.com" required />
              </div>
            </div>
          </div>

          <!-- Sujet : Custom Dropdown -->
          <div class="field field-full">
            <label>Sujet</label>
            <div class="custom-dropdown" :class="{ open: subjectDropdownOpen, 'is-light': isLight }" @click.stop="toggleSubjectDropdown">
              <div class="custom-dropdown-trigger">
                <svg class="dd-icon-left" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3"/><circle cx="12" cy="12" r="10"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
                <span class="dd-value" :class="{ placeholder: !callbackForm.subject }">
                  {{ callbackForm.subject || '— Choisir un sujet —' }}
                </span>
                <svg class="dd-chevron" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
              </div>
              <div class="custom-dropdown-menu" v-show="subjectDropdownOpen" @click.stop>
                <div
                  v-for="opt in subjectOptions"
                  :key="opt"
                  class="custom-dropdown-item"
                  :class="{ selected: callbackForm.subject === opt }"
                  @click="selectSubject(opt)"
                >
                  {{ opt }}
                </div>
              </div>
            </div>
          </div>

          <!-- Message -->
          <div class="field field-full">
            <label>Message</label>
            <div class="textarea-shell">
              <textarea 
                v-model.trim="callbackForm.message" 
                placeholder="Décrivez votre demande, problème ou question en détail..." 
                required 
                rows="5"
                maxlength="2000"
              ></textarea>
              <span class="char-count">{{ callbackForm.message.length }} / 2000</span>
            </div>
          </div>

          <button type="submit" class="submit-btn btn-wide" :disabled="isSubmitting">
            <span v-if="!isSubmitting">Envoyer le message</span>
            <span v-else class="btn-loading">Envoi en cours…</span>
          </button>
        </form>

        <p v-if="callbackRequested" class="callback-success animate-fade-in">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width:20px"><polyline points="20 6 9 17 4 12"/></svg>
          Demande envoyée ! Un expert vous contactera.
        </p>
      </section>
    </main>

    <footer class="neo-footer container glass-panel">
      <div class="footer-grid">
        <article class="footer-col">
          <img :src="logoUrl" alt="NovaHire" class="footer-logo" />
          <p class="hero-desc" style="font-size: 14px;">Écosystème intelligent de recrutement.</p>
        </article>
        <article class="footer-col footer-links">
          <h3>Liens rapides</h3>
          <ul class="footer-link-list">
            <li><a href="#">Solutions</a></li>
            <li><a href="#">Expertise</a></li>
            <li><a href="#">Carrières</a></li>
          </ul>
        </article>
        <article class="footer-col footer-addresses">
          <h3>Contact</h3>
          <p>Lille, Paris, Tunis</p>
        </article>
        <article class="footer-col footer-social">
          <h3>Suivez-nous</h3>
          <div class="social-list">
            <a v-for="social in socials" :key="social.name" :href="social.url" class="social-btn">
              {{ social.short }}
            </a>
          </div>
        </article>
      </div>
    </footer>
  </div>
</template>

<script>
import logoUrl from '@/assets/Logo_NovaHire.png'
import axios from 'axios'
import AuthThemeToggle from '@/components/common/AuthThemeToggle.vue'
import { useThemeStore } from '@/stores/themeStore'

export default {
  name: 'Welcome',
  components: { AuthThemeToggle },
  setup() {
    const themeStore = useThemeStore()
    return { themeStore }
  },
  data() {
    return {
      logoUrl,
      capabilities: [
        { tag: 'Workflow', title: 'Pilotage intelligent', text: 'Filtrez et priorisez automatiquement les profils.' },
        { tag: 'Collaboration', title: 'Validation fluide', text: 'Centralisez les retours entre RH et managers.' },
        { tag: 'Analytique', title: 'Tableaux de bord', text: 'Suivez vos KPI de recrutement en temps réel.' }
      ],
      socials: [
        { name: 'X', short: 'X', url: '#' },
        { name: 'LinkedIn', short: 'in', url: '#' },
        { name: 'Facebook', short: 'f', url: '#' }
      ],
      callbackForm: { fullName: '', company: '', phone: '', email: '', subject: '', message: '' },
      isSubmitting: false,
      callbackRequested: false,
      subjectDropdownOpen: false,
      subjectOptions: [
        'Problème technique',
        'Bug ou dysfonctionnement',
        "Demande d'information",
        'Demande de démonstration',
        'Réclamation',
        "Suggestion d'amélioration",
        'Autre'
      ],
      mouse: { x: -100, y: -100 },
      follower: { x: -100, y: -100 },
      isHovering: false,
      _rafId: null,
      fullText1: 'Votre transformation RH,',
      fullText2: 'en un seul espace intelligent',
      typingText1: '',
      typingText2: '',
      typingSpeed: 80,
      isTyping1: true,
      showCursor: true
    }
  },
  mounted() {
    document.body.classList.add('welcome-active')
    this.startTypewriter()
    window.addEventListener('mousemove', this.onMouseMove)
    window.addEventListener('mouseover', this.handleHover)
    window.addEventListener('click', this.closeDropdownOnClickOutside)
    this.animateFollower()
  },
  beforeUnmount() {
    document.body.classList.remove('welcome-active')
    window.removeEventListener('mousemove', this.onMouseMove)
    window.removeEventListener('mouseover', this.handleHover)
    window.removeEventListener('click', this.closeDropdownOnClickOutside)
    if (this._rafId) cancelAnimationFrame(this._rafId)
  },
  computed: {
    isLight() {
      return !this.themeStore.isDark
    },
    followerStyle() {
      return {
        transform: `translate3d(${this.follower.x}px, ${this.follower.y}px, 0)`,
        scale: this.isHovering ? '1.8' : '1',
        opacity: this.isHovering ? '0.4' : '1',
        backgroundColor: this.isHovering ? 'rgba(0, 167, 225, 0.1)' : 'transparent'
      }
    }
  },
  methods: {
    async startTypewriter() {
      // Line 1
      this.showCursor = true
      this.isTyping1 = true
      this.typingText1 = ''
      this.typingText2 = ''
      for (let i = 0; i <= this.fullText1.length; i++) {
        this.typingText1 = this.fullText1.substring(0, i)
        await new Promise(resolve => setTimeout(resolve, this.typingSpeed))
      }
      this.isTyping1 = false
      
      // Brief pause
      await new Promise(resolve => setTimeout(resolve, 400))

      // Line 2
      for (let i = 0; i <= this.fullText2.length; i++) {
        this.typingText2 = this.fullText2.substring(0, i)
        await new Promise(resolve => setTimeout(resolve, this.typingSpeed))
      }

      // Hide cursor when finished
      this.showCursor = false

      // Final pause before reset
      await new Promise(resolve => setTimeout(resolve, 4000))
      this.startTypewriter()
    },
    onMouseMove(e) {
      this.mouse.x = e.clientX
      this.mouse.y = e.clientY
      
      // Perspective logic
      const { clientX, clientY } = e
      const { innerWidth, innerHeight } = window
      const px = (clientX / innerWidth - 0.5) * 30
      const py = (clientY / innerHeight - 0.5) * 30
      
      if (this.$refs.visual3d) {
        this.$refs.visual3d.style.transform = `rotateY(${px}deg) rotateX(${-py}deg)`
      }
    },
    animateFollower() {
      // Smooth interpolation
      const dx = this.mouse.x - this.follower.x
      const dy = this.mouse.y - this.follower.y
      
      this.follower.x += dx * 0.15
      this.follower.y += dy * 0.15
      
      this._rafId = requestAnimationFrame(this.animateFollower)
    },
    handleHover(e) {
      const isInteractive = e.target.closest('a, button, .hov-lift, input')
      this.isHovering = !!isInteractive
    },
    toggleSubjectDropdown() {
      this.subjectDropdownOpen = !this.subjectDropdownOpen
    },
    selectSubject(opt) {
      this.callbackForm.subject = opt
      this.subjectDropdownOpen = false
    },
    closeDropdownOnClickOutside() {
      this.subjectDropdownOpen = false
    },
    async submitCallbackRequest() {
      if (this.isSubmitting) return
      if (!this.callbackForm.subject) return
      this.isSubmitting = true
      try {
        const fullMessage = `[${this.callbackForm.subject}] ${this.callbackForm.message}`
        await axios.post('/api/public/contact', {
          fullName: this.callbackForm.fullName,
          company: this.callbackForm.company,
          phone: this.callbackForm.phone,
          email: this.callbackForm.email,
          message: fullMessage
        })
        this.callbackRequested = true
        this.callbackForm = { fullName: '', company: '', phone: '', email: '', subject: '', message: '' }
      } catch (err) {
        console.error('Failed to submit contact request', err)
      } finally {
        this.isSubmitting = false
      }
    }
  }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700;800;900&display=swap');

.welcome-page {
  font-family: 'Inter', sans-serif;
  color: white;
  min-height: 100vh;
  position: relative;
  z-index: 10;
  overflow-x: hidden;
  padding-bottom: 60px;
}

.container {
  width: min(1200px, 95%);
  margin: 0 auto;
}

/* ─── AUTH CELESTIAL BACKGROUND (FROM LOGIN) ─── */
:global(.auth-bg) {
  position: fixed; inset: 0; z-index: -1; overflow: hidden; pointer-events: none;
  background: #020617;
}
:global(.bg-grid) {
  position: absolute; inset: -2px;
  background-image: linear-gradient(rgba(0,167,225,0.1) 1px, transparent 1px), linear-gradient(90deg, rgba(0,167,225,0.1) 1px, transparent 1px);
  background-size: 52px 52px;
  mask-image: radial-gradient(ellipse at center, black 35%, transparent 78%);
  animation: gridDrift 12s linear infinite; opacity: 0.7;
}
@keyframes gridDrift { from { background-position: 0 0; } to { background-position: 52px 52px; } }

:global(.bg-aurora) {
  position: absolute; inset: -25%;
  background: conic-gradient(from 140deg at 50% 50%, rgba(0,167,225,0) 0deg, rgba(0,167,225,0.2) 60deg, rgba(139,92,246,0.2) 140deg, rgba(247,201,2,0.1) 220deg, rgba(0,167,225,0.2) 300deg, rgba(0,167,225,0) 360deg);
  filter: blur(90px); animation: auroraSpin 22s linear infinite; opacity: 0.8;
}
@keyframes auroraSpin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

:global(.bg-orb) { position: absolute; border-radius: 50%; filter: blur(80px); mix-blend-mode: screen; }
:global(.bg-orb-cyan) { width: 560px; height: 560px; background: radial-gradient(circle, #00A7E1 0%, transparent 70%); top: -10%; left: -5%; opacity: 0.6; }
:global(.bg-orb-purple) { width: 480px; height: 480px; background: radial-gradient(circle, #8B5CF6 0%, transparent 70%); bottom: -10%; right: -5%; opacity: 0.6; }
:global(.bg-orb-gold) { width: 360px; height: 360px; background: radial-gradient(circle, #F7C902 0%, transparent 70%); top: 40%; left: 45%; opacity: 0.3; }

:global(.bg-stars span) {
  position: absolute; width: 2px; height: 2px; border-radius: 50%; background: white;
  box-shadow: 0 0 6px rgba(255,255,255,0.8); animation: twinkle 1.6s infinite;
}
@keyframes twinkle { 0%, 100% { opacity: 0.2; } 50% { opacity: 1; } }

:global(.bg-scan) {
  position: absolute; left: 0; right: 0; height: 180px;
  background: linear-gradient(180deg, transparent, rgba(0,167,225,0.2), transparent);
  animation: scanMove 5s linear infinite;
}
@keyframes scanMove { 0% { top: -180px; } 100% { top: 100%; } }

.welcome-bg-cursor-glow {
  position: absolute; width: 600px; height: 600px; border-radius: 50%;
  background: radial-gradient(circle, rgba(0,167,225,0.3) 0%, transparent 70%);
  filter: blur(40px); transition: transform 0.1s ease-out;
}

/* ─── GLASS PANELS (MATCHES LOGIN) ─── */
.glass-panel {
  background: rgba(13, 20, 32, 0.95);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-top-color: rgba(255, 255, 255, 0.2);
  border-left-color: rgba(255, 255, 255, 0.2);
  border-radius: 32px;
  box-shadow: 0 40px 100px rgba(0,0,0,0.8);
}
.glass-sub { background: rgba(255, 255, 255, 0.05); border-radius: 20px; border: 1px solid rgba(255,255,255,0.1); }

/* ─── TOPBAR ─── */
.topbar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 12px 32px; margin: 20px auto 60px;
}
.logo-group { position: relative; }
.brand-logo { height: 40px; position: relative; z-index: 2; }
.brand-glow {
  position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
  width: 100px; height: 100px; background: radial-gradient(circle, rgba(0,167,225,0.4), transparent 70%);
  filter: blur(15px);
}
.top-nav { display: flex; gap: 32px; }
.nav-link { color: white; text-decoration: none; font-weight: 600; font-size: 14px; opacity: 0.7; transition: 0.3s; }
.nav-link:hover { opacity: 1; color: #00A7E1; }
.top-actions { display: flex; gap: 12px; align-items: center; }

/* ─── TYPOGRAPHY ─── */
.welcome-title { font-size: clamp(32px, 5vw, 56px); font-weight: 900; line-height: 1.1; letter-spacing: -2px; margin-bottom: 24px; }
.title-accent {
  background: linear-gradient(135deg, #00A7E1, #8B5CF6, #F7C902);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent;
  background-size: 200%; animation: gradientShift 6s infinite;
}

.typewriter-text {
  display: inline;
}

.cursor {
  display: inline-block;
  width: 3px;
  background-color: #F7C902;
  margin-left: 4px;
  animation: blink 0.8s infinite;
  vertical-align: middle;
  height: 0.8em;
}

@keyframes blink {
  0%, 100% { opacity: 1; }
  50% { opacity: 0; }
}
@keyframes gradientShift { 0%, 100% { background-position: 0% 50%; } 50% { background-position: 100% 50%; } }

.eyebrow {
  display: inline-block; padding: 6px 14px; border-radius: 100px;
  background: rgba(0, 167, 225, 0.1); color: #00A7E1; font-size: 12px; font-weight: 700;
  margin-bottom: 16px; border: 1px solid rgba(0,167,225,0.2);
}
.hero-desc { font-size: 16px; line-height: 1.6; color: rgba(255,255,255,0.7); margin-bottom: 40px; }

/* ─── HERO SECTION ─── */
.hero { display: grid; grid-template-columns: 1fr 1fr; gap: 40px; align-items: center; margin-bottom: 100px; }
.hero-cta { display: flex; gap: 16px; }

/* ─── HERO VISUAL 3D ─── */
.hero-visual-container {
  position: relative;
  perspective: 1000px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.perspective-wrapper {
  position: relative;
  width: 100%;
  transform-style: preserve-3d;
}

.hero-visual-3d {
  position: relative;
  border-radius: 32px;
  padding: 10px;
  background: rgba(255, 255, 255, 0.03);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  box-shadow: 0 50px 100px rgba(0, 0, 0, 0.6);
  transition: transform 0.2s ease-out;
  transform: rotateY(-15deg) rotateX(5deg);
}

.visual-inner {
  border-radius: 24px;
  overflow: hidden;
  position: relative;
}

.hero-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  filter: brightness(1.1) contrast(1.1);
  transition: 0.5s;
}

.img-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, rgba(13, 20, 32, 0.5), transparent 60%);
}

.glass-accent {
  position: absolute;
  width: 80px;
  height: 80px;
  background: rgba(255, 255, 255, 0.05);
  backdrop-filter: blur(15px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 20px;
  z-index: 2;
}

.glass-accent.top-right { top: -20px; right: -20px; transform: rotate(15deg); }
.glass-accent.bottom-left { bottom: -20px; left: -20px; transform: rotate(-15deg); }

.deco-blob {
  position: absolute;
  border-radius: 50%;
  filter: blur(40px);
  z-index: -1;
  opacity: 0.4;
}

.deco-blob.blob-1 {
  width: 150px;
  height: 150px;
  background: #00A7E1;
  top: -40px;
  left: -40px;
}

.deco-blob.blob-2 {
  width: 200px;
  height: 200px;
  background: #8B5CF6;
  bottom: -60px;
  right: -40px;
}

@keyframes float-slow {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-15px); }
}

.animate-float-slow { animation: float-slow 6s infinite ease-in-out; }

.panel-stats {
  position: absolute; bottom: 15px; left: 15px; right: 15px;
  padding: 18px 24px; backdrop-filter: blur(12px);
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.1);
}
.stat-main { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 16px; }
.stat-label { font-size: 12px; font-weight: 600; color: rgba(255,255,255,0.6); }
.stat-highlight { font-size: 32px; font-weight: 900; color: #F7C902; }
.kpi-mini-grid { display: flex; gap: 24px; }
.kpi-mini { display: flex; flex-direction: column; }
.kpi-val { font-size: 18px; font-weight: 800; }
.kpi-lab { font-size: 11px; opacity: 0.5; }

/* ─── KPI GRID ─── */
.kpi-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 24px; margin-bottom: 120px; }
.kpi-card { padding: 32px; text-align: center; }
.kpi-icon { width: 48px; height: 48px; border-radius: 14px; margin: 0 auto 20px; display: flex; align-items: center; justify-content: center; }
.b-blue-icon { background: rgba(0, 167, 225, 0.1); color: #00A7E1; }
.b-gold-icon { background: rgba(247, 201, 2, 0.1); color: #F7C902; }
.b-purple-icon { background: rgba(139, 92, 246, 0.1); color: #8B5CF6; }
.stat-blue { color: #00A7E1; }
.stat-gold { color: #F7C902; }
.stat-purple { color: #8B5CF6; }

/* ─── SOLUTIONS ─── */
.section-title { text-align: center; margin-bottom: 60px; }
.solution-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 24px; margin-bottom: 120px; }
.solution-card { padding: 40px; }
.mini-tag { color: #8B5CF6; font-size: 11px; font-weight: 800; text-transform: uppercase; margin-bottom: 16px; }

/* ─── CONTACT ─── */
.contact-strip { padding: 60px; text-align: center; margin-bottom: 120px; }
.contact-header { margin-bottom: 40px; }
.contact-form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 24px; text-align: left; }
.field { margin-bottom: 0; }
.field-full { width: 100%; text-align: left; margin-top: 8px; margin-bottom: 8px; }
.field label { font-size: 13px; font-weight: 600; opacity: 0.7; margin-bottom: 8px; display: block; }
.input-shell { position: relative; }
.input-shell svg { 
  position: absolute; left: 16px; top: 50%; transform: translateY(-50%); 
  width: 18px; transition: 0.3s;
}
.field:nth-child(1) .input-shell svg { color: #00A7E1; } /* Name: Cyan */
.field:nth-child(2) .input-shell svg { color: #F7C902; } /* Company: Gold */
.field:nth-child(3) .input-shell svg { color: #8B5CF6; } /* Phone: Purple */
.field:nth-child(4) .input-shell svg { color: #00A7E1; } /* Email: Cyan */
.field-full .input-shell svg { color: #F59E0B; } /* Subject: Amber */

.input-shell input {
  width: 100%;
  background: rgba(255,255,255,0.05);
  border: 1px solid rgba(255,255,255,0.1);
  border-radius: 14px;
  padding: 14px 14px 14px 48px;
  color: white;
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  transition: border-color 0.3s, box-shadow 0.3s;
}
.input-shell input:focus {
  outline: none;
  border-color: rgba(0, 167, 225, 0.5);
  box-shadow: 0 0 0 3px rgba(0, 167, 225, 0.1);
}
.input-shell input::placeholder { color: rgba(255,255,255,0.3); }

/* ─── CUSTOM DROPDOWN ─── */
.custom-dropdown {
  position: relative;
  width: 100%;
  user-select: none;
}

/* ── Trigger bar ── */
.custom-dropdown-trigger {
  display: flex;
  align-items: center;
  gap: 12px;
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 14px;
  padding: 14px 16px;
  cursor: pointer;
  transition: border-color 0.3s, box-shadow 0.3s, background 0.2s;
  min-height: 52px;
}

/* Light mode trigger */
.custom-dropdown.is-light .custom-dropdown-trigger {
  background: #ffffff;
  border-color: rgba(15, 23, 42, 0.15);
}

.custom-dropdown.open .custom-dropdown-trigger,
.custom-dropdown-trigger:hover {
  border-color: rgba(0, 167, 225, 0.55);
  box-shadow: 0 0 0 3px rgba(0, 167, 225, 0.1);
  background: rgba(255, 255, 255, 0.08);
}

.custom-dropdown.is-light.open .custom-dropdown-trigger,
.custom-dropdown.is-light .custom-dropdown-trigger:hover {
  background: #f0f9ff;
  border-color: rgba(0, 167, 225, 0.55);
}

/* ── Left icon ── */
.dd-icon-left {
  width: 18px;
  height: 18px;
  flex-shrink: 0;
  color: #F59E0B;
}

/* ── Value text ── */
.dd-value {
  flex: 1;
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  color: white;
  text-align: left;
}
.dd-value.placeholder {
  color: rgba(255, 255, 255, 0.35);
}

.custom-dropdown.is-light .dd-value {
  color: #0f172a;
}
.custom-dropdown.is-light .dd-value.placeholder {
  color: rgba(15, 23, 42, 0.38);
}

/* ── Chevron ── */
.dd-chevron {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
  color: rgba(255, 255, 255, 0.5);
  transition: transform 0.25s ease, color 0.25s ease;
}

.custom-dropdown.is-light .dd-chevron {
  color: rgba(15, 23, 42, 0.45);
}

.custom-dropdown.open .dd-chevron {
  transform: rotate(180deg);
  color: #00A7E1;
}

/* ── Dropdown menu panel ── */
.custom-dropdown-menu {
  position: absolute;
  top: calc(100% + 6px);
  left: 0;
  right: 0;
  background: #0f172a;
  border: 1px solid rgba(255, 255, 255, 0.15);
  border-radius: 14px;
  overflow: hidden;
  z-index: 999;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.45);
  animation: dropdownIn 0.18s ease;
}

/* Light mode menu panel */
.custom-dropdown.is-light .custom-dropdown-menu {
  background: #ffffff;
  border-color: rgba(15, 23, 42, 0.12);
  box-shadow: 0 12px 32px rgba(15, 23, 42, 0.12);
}

@keyframes dropdownIn {
  from { opacity: 0; transform: translateY(-6px); }
  to   { opacity: 1; transform: translateY(0); }
}

/* ── Menu items ── */
.custom-dropdown-item {
  padding: 13px 20px;
  font-family: 'Inter', sans-serif;
  font-size: 14px;
  color: rgba(255, 255, 255, 0.78);
  cursor: pointer;
  transition: background 0.15s, color 0.15s;
}

.custom-dropdown.is-light .custom-dropdown-item {
  color: #1e293b;
}

.custom-dropdown-item:hover {
  background: rgba(0, 167, 225, 0.12);
  color: #00A7E1;
}

.custom-dropdown.is-light .custom-dropdown-item:hover {
  background: #e0f5fd;
  color: #0077B6;
}

.custom-dropdown-item.selected {
  background: rgba(0, 167, 225, 0.15);
  color: #00A7E1;
  font-weight: 600;
}

.custom-dropdown.is-light .custom-dropdown-item.selected {
  background: #cceeff;
  color: #0077B6;
}

.textarea-shell { position: relative; }
.textarea-shell textarea {
  width: 100%; background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1);
  border-radius: 14px; padding: 16px; color: white; resize: vertical;
  font-family: 'Inter', sans-serif; font-size: 14px; line-height: 1.6;
  min-height: 120px;
  transition: border-color 0.3s, box-shadow 0.3s;
}
.textarea-shell textarea:focus {
  outline: none;
  border-color: rgba(0, 167, 225, 0.5);
  box-shadow: 0 0 0 3px rgba(0, 167, 225, 0.1);
}
.textarea-shell textarea::placeholder { color: rgba(255,255,255,0.3); }
.char-count {
  position: absolute; bottom: 12px; right: 16px;
  font-size: 11px; color: rgba(255,255,255,0.3); font-weight: 500;
}

.submit-btn:disabled {
  opacity: 0.6; cursor: not-allowed; transform: none !important;
}
.btn-loading {
  display: inline-flex; align-items: center; gap: 8px;
}

.callback-success {
  margin-top: 20px; display: flex; align-items: center; justify-content: center; gap: 10px;
  color: #10b981; font-weight: 600; font-size: 15px;
  padding: 16px; border-radius: 14px; background: rgba(16, 185, 129, 0.08);
  border: 1px solid rgba(16, 185, 129, 0.2);
}

/* ─── BUTTONS (FROM LOGIN) ─── */
.btn {
  padding: 14px 28px; border-radius: 14px; font-weight: 700; text-decoration: none;
  transition: 0.3s; display: inline-flex; align-items: center; justify-content: center;
}
.btn-accent, .submit-btn {
  background: linear-gradient(135deg, #00A7E1, #0077B6); color: white; border: none;
  box-shadow: 0 8px 24px rgba(0,167,225,0.3); cursor: pointer;
  height: 56px;
  border-radius: 100px; /* Fully rounded/pill shape */
  font-size: 16px;
  letter-spacing: 0.5px;
}
.btn-accent:hover, .submit-btn:hover { 
  transform: translateY(-3px); 
  box-shadow: 0 16px 40px rgba(0,167,225,0.5);
  background: linear-gradient(135deg, #00b4ed, #0088cc);
}
.btn-ghost { border: 1px solid rgba(255,255,255,0.1); color: white; }
.btn-ghost:hover { background: rgba(255,255,255,0.05); }
.btn-outline { border: 1px solid #00A7E1; color: #00A7E1; }
.btn-wide { width: 100%; }

/* ─── FOOTER ─── */
.neo-footer { padding: 60px; }
.footer-grid { display: grid; grid-template-columns: 1.5fr 1fr 1fr 1fr; gap: 40px; }
.footer-col h3 { font-size: 16px; margin-bottom: 24px; color: #00A7E1; }
.footer-link-list { list-style: none; padding: 0; }
.footer-link-list li { margin-bottom: 12px; }
.footer-link-list a { color: rgba(255,255,255,0.6); text-decoration: none; transition: 0.3s; }
.footer-link-list a:hover { color: white; }
.social-list { display: flex; gap: 12px; }
.social-btn {
  width: 40px; height: 40px; border-radius: 10px; background: rgba(255,255,255,0.05);
  display: flex; align-items: center; justify-content: center; color: white; text-decoration: none;
}

/* ─── UTILS ─── */
.hov-lift { transition: 0.4s; }
.hov-lift:hover { transform: translateY(-10px); }
.animate-fade-in { animation: fadeIn 1s ease-out; }
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }

@media (max-width: 900px) {
  .hero, .solution-grid, .footer-grid { grid-template-columns: 1fr; }
  .top-nav { display: none; }
}
/* ─── CUSTOM CURSOR ─── */
.custom-cursor-follower {
  position: fixed;
  top: 0; left: 0;
  pointer-events: none;
  z-index: 10000;
  border-radius: 50%;
  width: 44px;
  height: 44px;
  border: 2px dashed rgba(0, 167, 225, 0.6);
  margin: -22px 0 0 -22px;
  transition: scale 0.5s cubic-bezier(0.23, 1, 0.32, 1), opacity 0.5s ease;
  animation: followerSpin 10s linear infinite;
  display: flex;
  align-items: center;
  justify-content: center;
}

.follower-inner {
  position: absolute;
  inset: 2px;
  border-radius: 50%;
  border: 1px solid rgba(0, 167, 225, 0.2);
  background: radial-gradient(circle, rgba(0, 167, 225, 0.1) 0%, transparent 70%);
}

.follower-dot {
  width: 4px;
  height: 4px;
  background: #00A7E1;
  border-radius: 50%;
  box-shadow: 0 0 10px #00A7E1;
}

@keyframes followerSpin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@media (max-width: 1024px) {
  .custom-cursor-follower { display: none; }
  body { cursor: auto !important; }
}

/* On masque la flèche par défaut mais on autorise la main (pointer) sur les liens */
.welcome-active { cursor: none; }
.welcome-active a, 
.welcome-active button, 
.welcome-active input,
.welcome-active .nav-link { 
  cursor: pointer !important; 
}

/* ─── LIGHT MODE OVERRIDES ─── */
:global(body:not(.dark-mode) .auth-bg) {
  background: #f4f7fe !important;
}
:global(body:not(.dark-mode) .bg-stars),
:global(body:not(.dark-mode) .bg-aurora) {
  display: none !important;
}
:global(body:not(.dark-mode) .bg-grid) {
  opacity: 0.3;
}
:global(body:not(.dark-mode) .auth-bg .bg-orb-cyan) {
  opacity: 0.3;
}
:global(body:not(.dark-mode) .auth-bg .bg-orb-purple) {
  opacity: 0.2;
}
:global(body:not(.dark-mode) .auth-bg .bg-orb-gold) {
  opacity: 0.2;
}
:global(body:not(.dark-mode) .welcome-page) {
  color: #0f172a;
}
:global(body:not(.dark-mode) .welcome-title),
:global(body:not(.dark-mode) .nav-link),
:global(body:not(.dark-mode) .footer-col h3) {
  color: #0f172a;
}
:global(body:not(.dark-mode) .hero-desc),
:global(body:not(.dark-mode) .welcome-desc) {
  color: #475569;
}
:global(body:not(.dark-mode) .glass-panel) {
  background: rgba(255, 255, 255, 0.9);
  border-color: rgba(0, 0, 0, 0.08);
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.05);
  color: #0f172a;
}
:global(body:not(.dark-mode) .btn-ghost) {
  color: #0f172a;
  border-color: rgba(0, 0, 0, 0.1);
}
:global(body:not(.dark-mode) .btn-ghost:hover) {
  background: rgba(0, 0, 0, 0.05);
}
:global(body:not(.dark-mode) .input-shell input),
:global(body:not(.dark-mode) .textarea-shell textarea),
:global(body:not(.dark-mode) .contact-select) {
  background: rgba(0, 0, 0, 0.03);
  border-color: rgba(0, 0, 0, 0.1);
  color: #0f172a;
}
:global(body:not(.dark-mode) .contact-select option) {
  background: #ffffff;
  color: #0f172a;
}
:global(body:not(.dark-mode) .input-shell input::placeholder),
:global(body:not(.dark-mode) .textarea-shell textarea::placeholder) {
  color: rgba(0, 0, 0, 0.4);
}
:global(body:not(.dark-mode) .footer-link-list a) {
  color: #475569;
}
:global(body:not(.dark-mode) .footer-link-list a:hover) {
  color: #00A7E1;
}
:global(body:not(.dark-mode) .social-btn) {
  background: rgba(0, 0, 0, 0.05);
  color: #0f172a;
}
:global(body:not(.dark-mode) .hero-visual-3d) {
  background: rgba(255, 255, 255, 0.6);
  border-color: rgba(0, 0, 0, 0.1);
  box-shadow: 0 30px 60px rgba(0, 0, 0, 0.08);
}
:global(body:not(.dark-mode) .img-overlay) {
  background: linear-gradient(to top, rgba(244, 247, 254, 0.8), transparent 60%);
}
:global(body:not(.dark-mode) .kpi-card p),
:global(body:not(.dark-mode) .solution-card p) {
  color: #475569;
}
:global(body:not(.dark-mode) .field label) {
  color: #475569;
}
:global(body:not(.dark-mode) .char-count) {
  color: rgba(0, 0, 0, 0.5);
}
:global(body:not(.dark-mode) .contact-select) {
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 24 24' fill='none' stroke='rgba(0,0,0,0.5)' stroke-width='2'%3E%3Cpolyline points='6 9 12 15 18 9'/%3E%3C/svg%3E");
}

/* ─── LIGHT MODE: CUSTOM DROPDOWN OVERRIDES ─── */
:global(body:not(.dark-mode) .custom-dropdown-trigger) {
  background: #ffffff !important;
  border-color: rgba(15, 23, 42, 0.15) !important;
}
:global(body:not(.dark-mode) .custom-dropdown-trigger:hover),
:global(body:not(.dark-mode) .custom-dropdown.open .custom-dropdown-trigger) {
  background: #f0f9ff !important;
  border-color: rgba(0, 167, 225, 0.55) !important;
  box-shadow: 0 0 0 3px rgba(0, 167, 225, 0.1) !important;
}
:global(body:not(.dark-mode) .dd-value) {
  color: #0f172a !important;
}
:global(body:not(.dark-mode) .dd-value.placeholder) {
  color: rgba(15, 23, 42, 0.38) !important;
}
:global(body:not(.dark-mode) .dd-chevron) {
  color: rgba(15, 23, 42, 0.45) !important;
}
:global(body:not(.dark-mode) .custom-dropdown-menu) {
  background: #ffffff !important;
  border-color: rgba(15, 23, 42, 0.12) !important;
  box-shadow: 0 12px 32px rgba(15, 23, 42, 0.12) !important;
}
:global(body:not(.dark-mode) .custom-dropdown-item) {
  color: #1e293b !important;
}
:global(body:not(.dark-mode) .custom-dropdown-item:hover) {
  background: #e0f5fd !important;
  color: #0077B6 !important;
}
:global(body:not(.dark-mode) .custom-dropdown-item.selected) {
  background: #cceeff !important;
  color: #0077B6 !important;
}
</style>
