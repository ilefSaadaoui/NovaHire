<template>
  <div class="auth-celestial">


    <div class="auth-layout">
      <!-- LEFT: Welcome Panel -->
      <div class="welcome-panel">
        <div class="welcome-inner">
          <button @click="$router.push('/connexion')" class="btn-back-celestial">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="15 18 9 12 15 6"/></svg>
            Retour
          </button>

          <div class="brand-strip">
            <img :src="logoUrl" alt="NovaHire" class="brand-logo">
            <div class="brand-glow"></div>
          </div>
          
          <h1 class="welcome-title">
            Rejoignez<br>
            <span class="title-accent">l'Excellence</span>
          </h1>
          <p class="welcome-desc">Démarrez votre transformation de recrutement dès aujourd'hui. Trouvez, évaluez et recrutez grâce à des algorithmes de nouvelle génération.</p>
          
          <div class="trust-badges">
            <div class="trust-item">
              <div class="trust-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
              </div>
              <div class="trust-text">
                <strong>Hautement Qualifié</strong>
                <span>Matching IA précis</span>
              </div>
            </div>
            <div class="trust-item">
              <div class="trust-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
              </div>
              <div class="trust-text">
                <strong>Temps Réel</strong>
                <span>Mises à jour instantanées</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- RIGHT: Form Panel -->
      <div class="form-panel">
        <div class="login-card glass-panel">
          <div class="card-header">
            <span class="greeting-chip">Inscription</span>
            <h2>Quel est votre profil ?</h2>
            <p>Sélectionnez le type de compte pour débuter.</p>
          </div>

          <div class="role-grid-celestial single-option">
            <router-link to="/inscription/entreprise" class="role-card-premium">
              <div class="orb-icon orb-blue">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="7" width="20" height="14" rx="2" ry="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
              </div>
              <div class="role-meta">
                <h3>Profil Entreprise</h3>
                <p>Pilotez votre recrutement avec une IA avancée</p>
              </div>
              <div class="role-arrow">→</div>
            </router-link>
          </div>

          <div class="card-footer">
            <div class="divider"><span>Déjà membre ?</span></div>
            <router-link to="/connexion" class="register-link">
              Se connecter
            </router-link>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import logoUrl from '@/assets/Logo_NovaHire.png'

export default {
  name: 'Register',
  data() {
    return {
      logoUrl,
      parallaxX: 0,
      parallaxY: 0
    }
  },
  computed: {
    parallaxStyle() {
      return {
        transform: `translate(${this.parallaxX}px, ${this.parallaxY}px)`,
        transition: 'transform 0.1s ease-out'
      }
    }
  },
  mounted() {
    window.addEventListener('mousemove', this.handleMouseMove)
  },
  beforeUnmount() {
    window.removeEventListener('mousemove', this.handleMouseMove)
  },
  methods: {
    handleMouseMove(e) {
      if (window.innerWidth > 768) {
        this.parallaxX = (e.clientX - window.innerWidth / 2) / -50;
        this.parallaxY = (e.clientY - window.innerHeight / 2) / -50;
      }
    }
  }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600;700;800&display=swap');

.auth-celestial {
  min-height: 100vh;
  background: #020617 !important; /* Force OLED Black */
  font-family: 'Outfit', sans-serif;
  color: white;
  display: flex; align-items: center; justify-content: center;
  padding: 40px;
  position: relative;
  overflow: hidden;
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
  position: relative;
}

.brand-strip { position: relative; margin-bottom: 44px; width: fit-content; }
.brand-logo { height: 44px; position: relative; z-index: 2; }
.brand-glow { position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); width: 50px; height: 50px; background: #00A7E1; opacity: 0.2; pointer-events: none; }

.welcome-title {
  font-size: 46px; font-weight: 900; line-height: 1.1;
  letter-spacing: -2px; margin: 0 0 20px;
  color: rgba(255,255,255,0.95);
}
.title-accent {
  background: linear-gradient(135deg, #00A7E1 0%, #8B5CF6 50%, #F7C902 100%);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent;
  background-size: 200% 200%;
  animation: gradientShift 6s ease infinite;
}
@keyframes gradientShift {
  0%, 100% { background-position: 0% 50%; }
  50% { background-position: 100% 50%; }
}

.welcome-desc {
  font-size: 15px; line-height: 1.7; color: rgba(255,255,255,0.4);
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
  background: rgba(0, 167, 225, 0.1); color: #00A7E1;
  display: flex; align-items: center; justify-content: center;
}
.trust-icon svg { width: 20px; }
.trust-text strong { display: block; font-size: 14px; font-weight: 700; color: white; margin-bottom: 4px; }
.trust-text span { font-size: 12px; color: rgba(255,255,255,0.4); }

/* ─── RIGHT: FORM PANEL ─── */
.form-panel {
  display: flex; flex-direction: column; justify-content: center;
  padding: 40px 0;
}

.login-card {
  background: rgba(13, 20, 32, 0.95);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-top-color: rgba(255, 255, 255, 0.2);
  border-left-color: rgba(255, 255, 255, 0.2);
  border-radius: 32px;
  padding: 48px;
  box-shadow: 0 40px 100px rgba(0,0,0,0.6);
  position: relative;
  overflow: hidden;
}

.card-header { margin-bottom: 32px; text-align: center; }
.greeting-chip { display: inline-block; padding: 6px 14px; background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1); border-radius: 100px; font-size: 12px; font-weight: 700; color: #F7C902; margin-bottom: 16px; }
.card-header h2 { font-size: 28px; font-weight: 800; margin: 0 0 8px; letter-spacing: -1px; }
.card-header p { font-size: 14px; color: rgba(255,255,255,0.4); margin: 0; }

.btn-back-celestial {
  background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1); color: #00A7E1; font-weight: 700;
  display: inline-flex; align-items: center; gap: 8px; font-size: 13px;
  cursor: pointer; padding: 8px 16px; border-radius: 100px; margin-bottom: 40px;
  transition: all 0.3s; width: fit-content;
}
.btn-back-celestial:hover { background: rgba(255, 255, 255, 0.1); transform: translateX(-4px); }
.btn-back-celestial svg { width: 14px; }

.role-grid-celestial { display: grid; gap: 20px; }
.role-card-premium {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 24px;
  padding: 24px;
  display: flex; align-items: center; gap: 20px;
  cursor: pointer; transition: 0.3s;
  text-align: left;
  text-decoration: none; color: white;
}
.role-card-premium:hover {
  transform: translateX(6px);
  border-color: rgba(0, 167, 225, 0.4);
  background: rgba(0, 167, 225, 0.06);
  box-shadow: 0 10px 30px rgba(0, 167, 225, 0.1);
}

.orb-icon { width: 56px; height: 56px; border-radius: 16px; display: flex; align-items: center; justify-content: center; }
.orb-icon svg { width: 24px; }
.orb-blue { background: rgba(0, 167, 225, 0.1); color: #00A7E1; }

.role-meta h3 { font-size: 18px; font-weight: 800; margin-bottom: 6px; }
.role-meta p { font-size: 13px; color: rgba(255,255,255,0.4); line-height: 1.4; margin: 0; }
.role-arrow { margin-left: auto; color: rgba(255,255,255,0.2); font-size: 24px; font-weight: 300; transition: 0.3s; }
.role-card-premium:hover .role-arrow { color: #00A7E1; transform: translateX(4px); }

.card-footer { margin-top: 32px; text-align: center; }
.divider { display: flex; align-items: center; text-align: center; margin-bottom: 24px; }
.divider::before, .divider::after { content: ''; flex: 1; border-bottom: 1px solid rgba(255,255,255,0.06); }
.divider span { padding: 0 16px; color: rgba(255,255,255,0.3); font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: 1px; }

.register-link {
  display: inline-block; width: 100%;
  padding: 16px; border-radius: 14px;
  background: transparent; border: 1.5px solid rgba(255,255,255,0.1);
  color: white; text-decoration: none; font-weight: 600; font-size: 14px;
  transition: 0.3s;
}
.register-link:hover { background: rgba(255,255,255,0.05); border-color: rgba(255,255,255,0.2); }



@media (max-width: 900px) {
  .auth-layout { grid-template-columns: 1fr; }
  .welcome-panel { display: none; }
  .form-panel { padding: 0; }
  .login-card { padding: 32px 24px; }
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
:global(body:not(.dark-mode) .btn-back-celestial) {
  background: rgba(0, 0, 0, 0.03);
  border-color: rgba(0, 0, 0, 0.08);
  color: #00A7E1;
}
:global(body:not(.dark-mode) .btn-back-celestial:hover) {
  background: rgba(0, 0, 0, 0.06);
  color: #008cc0;
}
:global(body:not(.dark-mode) .card-header h2) {
  color: #0f172a;
}
:global(body:not(.dark-mode) .card-header p) {
  color: #64748b;
}
:global(body:not(.dark-mode) .role-card-premium) {
  background: rgba(0, 0, 0, 0.02);
  border-color: rgba(0, 0, 0, 0.08);
  color: #0f172a;
}
:global(body:not(.dark-mode) .role-card-premium:hover) {
  background: rgba(0, 167, 225, 0.06);
  border-color: rgba(0, 167, 225, 0.4);
  color: #0f172a;
}
:global(body:not(.dark-mode) .role-meta h3) {
  color: #0f172a;
}
:global(body:not(.dark-mode) .role-meta p) {
  color: #475569;
}
:global(body:not(.dark-mode) .role-arrow) {
  color: rgba(0, 0, 0, 0.25);
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