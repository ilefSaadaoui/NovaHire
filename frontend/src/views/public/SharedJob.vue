<template>
  <div class="shared-page" :style="pageStyle">
    <!-- Subtle geometric background -->
    <div class="page-bg">
      <div class="bg-grid"></div>
      <div class="bg-blob blob-gold"></div>
      <div class="bg-blob blob-purple"></div>
      <div class="bg-blob blob-blue"></div>
    </div>

    <!-- Header -->
    <header class="site-header">
      <div class="header-inner">
        <div class="brand-group">
          <div
            class="brand-avatar"
            v-if="!offer?.companyLogo"
            :style="{ background: 'linear-gradient(135deg, ' + (offer?.pageColor || '#3b82f6') + ', #7c3aed)' }"
          >
            {{ offer?.company?.[0] || 'N' }}
          </div>
          <img v-else :src="offer.companyLogo" class="brand-logo-img" alt="Company Logo" />
          <div class="brand-text">
            <span class="company-name">{{ offer?.company }}</span>
            <span class="portal-badge">
              <span class="badge-dot"></span>
              Portail Officiel
            </span>
          </div>
        </div>
      </div>
    </header>

    <!-- Main Layout -->
    <div class="page-layout" v-if="offer">
      <!-- LEFT COLUMN -->
      <main class="content-col">

        <!-- Hero Card -->
        <div class="hero-card">
          <div class="hero-accent-bar" :style="{ background: offer?.pageColor || '#3b82f6' }"></div>
          <div class="hero-body">
            <div class="hero-top">
              <span class="active-badge">
                <svg viewBox="0 0 8 8" width="8"><circle cx="4" cy="4" r="4" fill="#22c55e"/></svg>
                Poste ouvert
              </span>
              <span class="hero-type-tag">{{ translateEnum('type', offer.type) }}</span>
            </div>

            <h1 class="job-title">{{ offer.title }}</h1>
            <p class="job-welcome">{{ offer.welcomeMsg || 'Rejoignez une équipe d\'exception et construisez votre avenir professionnel avec nous.' }}</p>

            <div class="meta-strip">
              <div class="meta-chip" v-if="offer.location">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z"/><circle cx="12" cy="10" r="3"/></svg>
                {{ offer.location }}
              </div>
              <div class="meta-chip" v-if="offer.remotePolicy">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14"><rect x="2" y="4" width="20" height="14" rx="2"/><path d="M8 20h8M12 18v2"/></svg>
                {{ translateEnum('remote', offer.remotePolicy) }}
              </div>
              <div class="meta-chip salary" v-if="offer.showSalary && offer.salaryRange">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 000 7h5a3.5 3.5 0 010 7H6"/></svg>
                {{ offer.salaryRange }} DT
              </div>
            </div>
          </div>
        </div>

        <!-- Description -->
        <section class="content-card">
          <div class="card-section-label">Mission & Rôle</div>
          <div class="prose" v-html="formattedDescription"></div>
        </section>

        <!-- Skills -->
        <section class="content-card" v-if="offer.skills && offer.skills.length">
          <div class="card-section-label">Compétences Requises</div>
          <div class="skills-cloud">
            <span class="skill-pill" v-for="(skill, i) in offer.skills" :key="i">{{ skill }}</span>
          </div>
        </section>

      </main>

      <!-- RIGHT COLUMN: Sticky Form -->
      <aside class="form-col">
        <div class="form-panel" id="apply-now">
          <div class="form-header">
            <div class="form-header-icon" :style="{ color: offer?.pageColor || '#3b82f6', background: (offer?.pageColor || '#3b82f6') + '15' }">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="22"><path d="M16 21v-2a4 4 0 00-4-4H5a4 4 0 00-4 4v2"/><circle cx="9" cy="7" r="4"/><polyline points="16 11 18 13 22 9"/></svg>
            </div>
            <div>
              <h3 class="form-title">Postuler</h3>
              <p class="form-subtitle">Complétez votre dossier de candidature</p>
            </div>
          </div>

          <div class="form-body" v-if="!applicationSent">
            <form @submit.prevent="submitApplication" class="app-form">
              <div class="fields-list">
                <template v-for="field in effectiveCandidatureFields" :key="field.id">
                  <div class="field-group">
                    <label class="field-label">
                      {{ field.name }}
                      <span class="req-star" v-if="field.required">*</span>
                    </label>
                    <div class="field-input-wrapper">
                      <input
                        v-if="field.type !== 'Texte long'"
                        v-model="form[field.id]"
                        :type="mapInputType(field.type)"
                        :placeholder="getPlaceholder(field)"
                        :required="field.required"
                        class="field-input"
                      />
                      <textarea
                        v-else
                        v-model="form[field.id]"
                        rows="4"
                        :placeholder="getPlaceholder(field)"
                        :required="field.required"
                        class="field-input field-textarea"
                      ></textarea>
                      <div class="field-icon" v-html="getFieldIcon(field.id)"></div>
                    </div>
                  </div>
                </template>

                <!-- CV Upload -->
                <div class="field-group">
                  <label class="field-label">
                    Curriculum Vitae (PDF)
                    <span class="req-star">*</span>
                  </label>
                  <div class="file-drop" :class="{ 'file-drop--filled': uploadedDocs['cv'] }">
                    <input type="file" required @change="onCVChange($event)" accept=".pdf,.docx,.doc" id="cv-upload" class="file-input-hidden" />
                    <label for="cv-upload" class="file-drop-label">
                      <div class="file-drop-icon">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="20">
                          <path v-if="!uploadedDocs['cv']" d="M21 15v4a2 2 0 01-2 2H5a2 2 0 01-2-2v-4"/><polyline v-if="!uploadedDocs['cv']" points="17 8 12 3 7 8"/><line v-if="!uploadedDocs['cv']" x1="12" y1="3" x2="12" y2="15"/>
                          <polyline v-if="uploadedDocs['cv']" points="20 6 9 17 4 12"/>
                        </svg>
                      </div>
                      <span class="file-drop-text">
                        {{ uploadedDocs['cv'] ? uploadedDocs['cv'].name : 'Déposer ou cliquer pour importer' }}
                      </span>
                      <span class="file-drop-hint" v-if="!uploadedDocs['cv']">PDF / DOCX</span>
                    </label>
                  </div>

                  <!-- CV Parsing Preview -->
                  <div v-if="isParsing" class="cv-preview-loading">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" class="spin"><path d="M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83"/></svg>
                    <span>Analyse IA de votre CV en cours…</span>
                  </div>

                  <div v-if="cvPreview" class="cv-preview-card anim-reveal-up">
                    <div class="cv-preview-header-luxury">
                      <div class="ia-badge-luxe">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="12"><path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/></svg>
                        <span>Intelligence Artificielle</span>
                      </div>
                      <div class="header-main-text">Extraction réussie !</div>
                    </div>
                    
                    <div class="cv-preview-body">
                      <p class="preview-intro">Nous avons analysé votre CV et pré-rempli les champs ci-dessus. Veuillez vérifier l'exactitude des informations.</p>
                      
                      <div class="extracted-stats">
                        <div class="stat-item">
                          <span class="stat-value">{{ cvPreview.skills?.length || 0 }}</span>
                          <span class="stat-label">Compétences</span>
                        </div>
                        <div class="stat-item">
                          <span class="stat-value">{{ cvPreview.workExperiences?.length || 0 }}</span>
                          <span class="stat-label">Expériences</span>
                        </div>
                        <div class="stat-item">
                          <span class="stat-value">{{ cvPreview.languages?.length || 0 }}</span>
                          <span class="stat-label">Langues</span>
                        </div>
                      </div>

                      <div v-if="cvPreview.skills && cvPreview.skills.length" class="cv-preview-section">
                        <strong>Compétences clés identifiées</strong>
                        <div class="cv-skills-cloud">
                          <span v-for="s in cvPreview.skills.slice(0, 8)" :key="s" class="cv-skill-chip">{{ s }}</span>
                          <span v-if="cvPreview.skills.length > 8" class="cv-skill-more">+{{ cvPreview.skills.length - 8 }}</span>
                        </div>
                      </div>
                    </div>
                    <div class="cv-preview-footer-success">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" width="12"><path d="M22 11.08V12a10 10 0 11-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
                      <span>Données synchronisées avec le formulaire</span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="form-footer">
                <button type="submit" class="submit-btn" :disabled="isSubmitting" :style="submitBtnStyle">
                  <span v-if="!isSubmitting">Envoyer ma candidature</span>
                  <span v-else class="loading-text">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" class="spin"><path d="M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83"/></svg>
                    Transmission…
                  </span>
                </button>
                <p class="privacy-note">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0110 0v4"/></svg>
                  Données traitées en toute confidentialité
                </p>
              </div>
            </form>
          </div>

          <!-- Success State -->
          <div v-else class="success-state">
            <div class="success-check" :style="{ background: (offer?.pageColor || '#3b82f6') + '15', color: offer?.pageColor || '#3b82f6' }">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="32"><polyline points="20 6 9 17 4 12"/></svg>
            </div>
            <h3 class="success-title">Candidature envoyée !</h3>
            <p class="success-msg">Votre dossier a bien été transmis à l'équipe de recrutement. Vous serez contacté prochainement.</p>
            
            <div v-if="offer?.hasQuiz" class="quiz-invitation-box anim-reveal-up">
              <div class="quiz-icon-celestial">
                <i class="fas fa-brain"></i>
              </div>
              <div class="quiz-invitation-text">
                <h4>Évaluation IA disponible</h4>
                <p>Boostez votre candidature en passant dès maintenant le test technique ({{ offer.quizTimeLimit }} min).</p>
              </div>
              <button @click="goToQuiz" class="btn-start-quiz-now">
                Passer le test
                <i class="fas fa-arrow-right"></i>
              </button>
            </div>

            <button @click="applicationSent = false" class="btn-reset">Modifier ma candidature</button>
          </div>
        </div>
      </aside>
    </div>

    <!-- Not Found -->
    <div v-else class="not-found">
      <div class="not-found-icon">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="32"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
      </div>
      <h2>Offre introuvable</h2>
      <p>Cette offre n'est plus disponible ou le lien a expiré.</p>
      <router-link to="/" class="btn-reset">Retour à l'accueil</router-link>
    </div>
  </div>
</template>

<script>
import api from '@/api/axios'
import logoUrl from '@/assets/Logo_NovaHire.png'

export default {
  name: 'SharedJob',
  data() {
    return {
      logoUrl,
      offer: null,
      form: {
        firstName: '',
        lastName: '',
        email: '',
        phone: '',
        linkedIn: '',
        portfolio: '',
        coverLetter: ''
      },
      uploadedDocs: {},
      isSubmitting: false,
      isParsing: false,
      cvPreview: null,
      applicationSent: false,
      applicationId: null,
      source: null
    }
  },
  computed: {
    formattedDescription() {
      if (!this.offer?.description) return ''
      return this.offer.description.replace(/\n/g, '<br>')
    },
    pageStyle() {
      const color = this.offer?.pageColor || '#3b82f6'
      return {
        '--accent': color,
        '--accent-10': `${color}1a`,
        '--accent-20': `${color}33`,
        '--nh-gold': '#f59e0b',
        '--nh-blue-light': '#0ea5e9',
        '--nh-blue-dark': '#1e293b',
        '--nh-purple': '#6366f1',
        '--nh-gradient': `linear-gradient(135deg, ${color} 0%, #6366f1 100%)`
      }
    },
    submitBtnStyle() {
      const color = this.offer?.pageColor || '#3b82f6'
      return { background: color }
    },
    effectiveCandidatureFields() {
      if (!this.offer) return []
      const fields = []
      const get = (key) => this.offer[key] ?? this.offer[key.charAt(0).toUpperCase() + key.slice(1)]

      if (get('requireFullName') ?? true) {
        fields.push({ id: 'firstName', name: 'Prénom', type: 'Texte', required: true })
        fields.push({ id: 'lastName', name: 'Nom', type: 'Texte', required: true })
      }
      if (get('requireEmail') ?? true)
        fields.push({ id: 'email', name: 'Email professionnel', type: 'Email', required: true })
      if (get('requirePhone'))
        fields.push({ id: 'phone', name: 'Téléphone', type: 'Téléphone', required: false })
      if (get('requireLinkedIn'))
        fields.push({ id: 'linkedIn', name: 'Profil LinkedIn', type: 'Lien', required: false })
      if (get('requirePortfolio'))
        fields.push({ id: 'portfolio', name: 'Portfolio / URL', type: 'Lien', required: false })
      if (get('requireCoverLetter'))
        fields.push({ id: 'coverLetter', name: 'Lettre de motivation', type: 'Texte long', required: false })

      return fields
    }
  },
  created() {
    this.loadOffer()
  },
  methods: {
    translateEnum(type, val) {
      if (!val) return '—'
      const maps = {
        remote: { OnSite: 'Sur site', Remote: 'Télétravail', Hybrid: 'Hybride', office: 'Sur site', hybrid: 'Hybride', remote: 'Télétravail' },
        type: { FullTime: 'CDI', Contract: 'CDD', Freelance: 'Freelance', Internship: 'Stage', fulltime: 'CDI', contract: 'CDD', freelance: 'Freelance', internship: 'Stage' }
      }
      return maps[type]?.[val] || maps[type]?.[val?.toLowerCase()] || val
    },
    async loadOffer() {
      const token = this.$route.params.token
      try {
        // silentError: true → supprime toast + console.error pour les 404 attendues
        const res = await api.get(`/public/offers/${token}`, { silentError: true })
        this.offer = res.data
        // Detect source from URL query (e.g. ?source=linkedin)
        this.source = this.$route.query.source || null
      } catch (err) {
        // 404 = offre en brouillon, archivée ou inexistante — comportement normal
        if (err.response?.status !== 404) {
          console.error('Erreur chargement offre:', err)
        }
        this.offer = null
      }
    },
    mapInputType(type) {
      if (type === 'Email') return 'email'
      if (type === 'Téléphone') return 'tel'
      return 'text'
    },
    getPlaceholder(field) {
      return `Saisissez votre ${field.name.toLowerCase()}...`
    },
    getFieldIcon(id) {
      const icons = {
        firstName: '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>',
        lastName: '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>',
        email: '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>',
        phone: '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.81 12.81 0 0 0 .62 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.62A2 2 0 0 1 22 16.92z"/></svg>',
        linkedIn: '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 8a6 6 0 0 1 6 6v7h-4v-7a2 2 0 0 0-2-2 2 2 0 0 0-2 2v7h-4v-7a6 6 0 0 1 6-6z"/><rect x="2" y="9" width="4" height="12"/><circle cx="4" cy="4" r="2"/></svg>',
        portfolio: '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="2" y1="12" x2="22" y2="12"/><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/></svg>',
        coverLetter: '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>'
      }
      return icons[id] || '<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 1 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>'
    },
    onDocChange(event, docId) {
      this.uploadedDocs[docId] = event.target.files[0]
    },
    async onCVChange(event) {
      const file = event.target.files[0]
      if (!file) return
      this.uploadedDocs['cv'] = file
      this.cvPreview = null

      // Auto-parse for transparency
      this.isParsing = true
      try {
        const formData = new FormData()
        formData.append('file', file)
        const res = await api.post('/public/parse-cv', formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
          silentError: true
        })
        this.cvPreview = res.data

        // Pre-fill form fields if they are empty
        if (res.data.firstName && !this.form.firstName) this.form.firstName = res.data.firstName
        if (res.data.lastName && !this.form.lastName) this.form.lastName = res.data.lastName
        if (res.data.email && !this.form.email) this.form.email = res.data.email
        if (res.data.phone && !this.form.phone) this.form.phone = res.data.phone

      } catch (err) {
        // Parsing is optional — if it fails, candidate can still submit
        console.warn('CV preview parsing failed:', err)
        this.cvPreview = null
      } finally {
        this.isParsing = false
      }
    },
    async submitApplication() {
      this.isSubmitting = true
      try {
        const formData = new FormData()
        formData.append('shareToken', this.$route.params.token)
        Object.entries(this.form).forEach(([k, v]) => formData.append(k, v || ''))
        if (this.uploadedDocs.cv) formData.append('CVFile', this.uploadedDocs.cv)
        if (this.source) formData.append('source', this.source)

        const res = await api.post('/public/applications', formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        })
        this.applicationId = res.data.id
        this.applicationSent = true
      } catch (err) {
        console.error('Erreur soumission:', err)
        alert(err.response?.data?.message || 'Une erreur est survenue. Veuillez réessayer.')
      } finally {
        this.isSubmitting = false
      }
    },
    goToQuiz() {
      if (!this.offer?.shareToken) return
      this.$router.push({
        name: 'PublicQuiz',
        params: { token: this.offer.shareToken },
        query: { appId: this.applicationId }
      })
    }
  }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Sora:wght@300;400;500;600;700;800&family=DM+Sans:ital,wght@0,300;0,400;0,500;0,600;1,400&display=swap');

/* ─── Reset & Base ─── */
*, *::before, *::after { box-sizing: border-box; margin: 0; padding: 0; }

.shared-page {
  min-height: 100vh;
  background: #f9fafb;
  font-family: 'DM Sans', sans-serif;
  color: #111827;
  position: relative;
  overflow-x: hidden;
}

/* ─── Background ─── */
.page-bg {
  position: fixed;
  inset: 0;
  pointer-events: none;
  z-index: 0;
}

.bg-grid {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(0,0,0,0.025) 1px, transparent 1px),
    linear-gradient(90deg, rgba(0,0,0,0.025) 1px, transparent 1px);
  background-size: 40px 40px;
}

.bg-blob {
  position: absolute;
  border-radius: 50%;
  filter: blur(100px);
  opacity: 0.12;
  z-index: -1;
}

.blob-gold {
  top: -100px; right: -100px;
  width: 500px; height: 500px;
  background: var(--nh-gold);
}

.blob-purple {
  bottom: 20%; left: -100px;
  width: 400px; height: 400px;
  background: var(--nh-purple);
}

.blob-blue {
  top: 40%; right: 20%;
  width: 300px; height: 300px;
  background: var(--nh-blue-light);
}

/* ─── Header ─── */
.site-header {
  position: sticky;
  top: 0;
  z-index: 100;
  background: rgba(255,255,255,0.9);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border-bottom: 1px solid #e5e7eb;
  height: 72px;
  display: flex;
  align-items: center;
}

.header-inner {
  width: 100%;
  max-width: 1280px;
  margin: 0 auto;
  padding: 0 32px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.brand-group {
  display: flex;
  align-items: center;
  gap: 14px;
}

.brand-avatar {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #fff;
  font-family: 'Sora', sans-serif;
  font-weight: 800;
  font-size: 18px;
  flex-shrink: 0;
  box-shadow: 0 4px 12px rgba(0,0,0,0.12);
}

.brand-logo-img {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  object-fit: contain;
}

.brand-text {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.company-name {
  font-family: 'Sora', sans-serif;
  font-weight: 700;
  font-size: 16px;
  color: #111827;
  line-height: 1;
}

.portal-badge {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 10px;
  font-weight: 600;
  color: #6b7280;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.badge-dot {
  width: 5px;
  height: 5px;
  border-radius: 50%;
  background: #22c55e;
  box-shadow: 0 0 6px #22c55e;
  flex-shrink: 0;
}

.novahire-brand {
  display: flex;
  align-items: center;
  gap: 8px;
}

.nh-label {
  font-size: 10px;
  font-weight: 500;
  color: #9ca3af;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.nh-logo {
  height: 18px;
  opacity: 0.5;
  filter: grayscale(1);
}

/* ─── Page Layout ─── */
.page-layout {
  position: relative;
  z-index: 1;
  max-width: 1280px;
  margin: 0 auto;
  padding: 40px 32px 80px;
  display: grid;
  grid-template-columns: 1fr 420px;
  gap: 32px;
  align-items: start;
}

/* ─── Left Column ─── */
.content-col {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

/* ─── Hero Card ─── */
.hero-card {
  background: #fff;
  border-radius: 20px;
  border: 1px solid #e5e7eb;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0,0,0,0.04), 0 8px 24px rgba(0,0,0,0.04);
}

.hero-accent-bar {
  height: 5px;
  width: 100%;
}

.hero-body {
  padding: 36px 40px 32px;
}

.hero-top {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 20px;
}

.active-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 5px 12px;
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
  border-radius: 100px;
  font-size: 11px;
  font-weight: 600;
  color: #16a34a;
}

.hero-type-tag {
  padding: 5px 12px;
  background: #f3f4f6;
  border-radius: 100px;
  font-size: 11px;
  font-weight: 600;
  color: #6b7280;
}

.job-title {
  font-family: 'Sora', sans-serif;
  font-size: 36px;
  font-weight: 800;
  line-height: 1.15;
  letter-spacing: -0.03em;
  color: #111827;
  margin-bottom: 14px;
}

.job-welcome {
  font-size: 15px;
  line-height: 1.65;
  color: #6b7280;
  max-width: 560px;
}

.meta-strip {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
  margin-top: 24px;
  padding-top: 24px;
  border-top: 1px solid #f3f4f6;
}

.meta-chip {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 7px 14px;
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 500;
  color: #374151;
}

.meta-chip.salary {
  background: #fffbeb;
  border-color: #fde68a;
  color: #92400e;
}

/* ─── Content Cards ─── */
.content-card {
  background: #fff;
  border-radius: 20px;
  border: 1px solid #e5e7eb;
  padding: 32px 40px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.04);
}

.card-section-label {
  font-family: 'Sora', sans-serif;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--accent, #3b82f6);
  margin-bottom: 20px;
}

.prose {
  font-size: 15px;
  line-height: 1.8;
  color: #374151;
}

.skills-cloud {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.skill-pill {
  padding: 8px 16px;
  background: #f9fafb;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 500;
  color: #374151;
  transition: all 0.2s ease;
  cursor: default;
}

.skill-pill:hover {
  border-color: var(--accent, #3b82f6);
  color: var(--accent, #3b82f6);
  background: var(--accent-10, #eff6ff);
  transform: translateY(-1px);
}

/* ─── Right Column: Form ─── */
.form-col {
  position: sticky;
  top: 88px; /* header height + gap */
}

.form-panel {
  background: #fff;
  border-radius: 20px;
  border: 1px solid #e5e7eb;
  box-shadow: 0 4px 6px rgba(0,0,0,0.04), 0 20px 40px rgba(0,0,0,0.06);
  overflow: hidden;
}

.form-header {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 28px 32px;
  border-bottom: 1px solid #f3f4f6;
  background: #fafafa;
}

.form-header-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.form-title {
  font-family: 'Sora', sans-serif;
  font-size: 18px;
  font-weight: 700;
  color: #111827;
  margin-bottom: 2px;
}

.form-subtitle {
  font-size: 13px;
  color: #9ca3af;
}

.form-body {
  padding: 24px 32px 32px;
}

.app-form {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.fields-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
  max-height: 50vh;
  overflow-y: auto;
  padding-right: 4px;
  scrollbar-width: thin;
  scrollbar-color: #e5e7eb transparent;
}

.fields-list::-webkit-scrollbar { width: 4px; }
.fields-list::-webkit-scrollbar-track { background: transparent; }
.fields-list::-webkit-scrollbar-thumb { background: #e5e7eb; border-radius: 4px; }

.field-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.field-label {
  font-size: 12px;
  font-weight: 600;
  color: #374151;
  letter-spacing: 0.02em;
}

.req-star {
  color: #ef4444;
  margin-left: 2px;
}

.field-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.field-icon {
  position: absolute;
  left: 14px;
  top: 50%;
  transform: translateY(-50%);
  color: #9ca3af;
  display: flex !important;
  align-items: center;
  justify-content: center;
  pointer-events: none;
  transition: all 0.2s ease;
  z-index: 10;
  width: 18px;
  height: 18px;
}

.field-icon svg {
  width: 100%;
  height: 100%;
  display: block;
}

.field-input {
  width: 100%;
  padding: 12px 14px 12px 42px;
  background: #f9fafb;
  border: 1.5px solid #e5e7eb;
  border-radius: 12px;
  font-family: inherit;
  font-size: 14px;
  color: #111827;
  outline: none;
  transition: all 0.2s ease;
  position: relative;
  z-index: 1;
}

.field-input:focus ~ .field-icon {
  color: var(--accent, #3b82f6);
}

.field-input::placeholder { color: #9ca3af; }

.field-input:focus {
  background: #fff;
  border-color: var(--accent, #3b82f6);
  box-shadow: 0 0 0 3px var(--accent-10, rgba(59,130,246,0.1));
}

.field-textarea {
  resize: vertical;
  min-height: 100px;
  padding-top: 12px;
}

/* File Upload */
.file-drop {
  position: relative;
}

.file-input-hidden {
  position: absolute;
  width: 0;
  height: 0;
  opacity: 0;
}

.file-drop-label {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 16px;
  background: #f9fafb;
  border: 1.5px dashed #d1d5db;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s ease;
}

.file-drop-label:hover {
  border-color: var(--accent, #3b82f6);
  background: var(--accent-10, #eff6ff);
}

.file-drop--filled .file-drop-label {
  border-color: #22c55e;
  background: #f0fdf4;
  border-style: solid;
}

.file-drop-icon {
  color: #6b7280;
  flex-shrink: 0;
}

.file-drop--filled .file-drop-icon { color: #22c55e; }

.file-drop-text {
  font-size: 13px;
  font-weight: 500;
  color: #374151;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.file-drop-hint {
  font-size: 11px;
  color: #9ca3af;
  flex-shrink: 0;
}

/* ─── CV Preview ─── */
.cv-preview-loading {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 16px;
  background: linear-gradient(135deg, #eff6ff 0%, #faf5ff 100%);
  border: 1px solid #e0e7ff;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 600;
  color: #6366f1;
  margin-top: 10px;
}

.cv-preview-card {
  margin-top: 10px;
  background: #fff;
  border: 1px solid #e0e7ff;
  border-radius: 12px;
  overflow: hidden;
  animation: cvReveal 0.4s ease-out;
}

@keyframes cvReveal {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

.cv-preview-header {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 16px;
  background: linear-gradient(135deg, #6366f1 0%, #8b5cf6 100%);
  color: white;
  font-size: 12px;
  font-weight: 700;
}

.cv-preview-card {
  margin-top: 16px;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 4px 20px rgba(0,0,0,0.06);
}

.cv-preview-header-luxury {
  padding: 14px 16px;
  background: #f8fafc;
  border-bottom: 1px solid #f1f5f9;
}

.ia-badge-luxe {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 4px 10px;
  background: linear-gradient(135deg, #6366f1, #a855f7);
  color: white;
  border-radius: 100px;
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 8px;
}

.header-main-text {
  font-family: 'Sora', sans-serif;
  font-size: 14px;
  font-weight: 700;
  color: #1e293b;
}

.cv-preview-body {
  padding: 16px;
}

.preview-intro {
  font-size: 12px;
  color: #64748b;
  line-height: 1.5;
  margin-bottom: 16px;
}

.extracted-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 12px;
  margin-bottom: 20px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 10px;
  background: #f1f5f9;
  border-radius: 12px;
}

.stat-value {
  font-family: 'Sora', sans-serif;
  font-size: 16px;
  font-weight: 800;
  color: #3b82f6;
}

.stat-label {
  font-size: 9px;
  font-weight: 700;
  color: #94a3b8;
  text-transform: uppercase;
}

.cv-preview-section strong {
  display: block;
  font-size: 11px;
  font-weight: 700;
  color: #475569;
  margin-bottom: 8px;
}

.cv-skills-cloud {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.cv-skill-chip {
  padding: 4px 10px;
  background: #eff6ff;
  color: #1d4ed8;
  border-radius: 8px;
  font-size: 11px;
  font-weight: 600;
}

.cv-skill-more {
  font-size: 11px;
  font-weight: 700;
  color: #94a3b8;
  align-self: center;
}

.cv-preview-footer-success {
  padding: 10px 16px;
  background: #f0fdf4;
  border-top: 1px solid #dcfce7;
  display: flex;
  align-items: center;
  gap: 8px;
  color: #15803d;
  font-size: 11px;
  font-weight: 700;
}

/* Form Footer */
.form-footer {
  margin-top: 20px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.submit-btn {
  width: 100%;
  padding: 14px;
  border: none;
  border-radius: 12px;
  color: #fff;
  font-family: 'Sora', sans-serif;
  font-size: 14px;
  font-weight: 700;
  letter-spacing: 0.02em;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  background: var(--nh-gradient) !important;
}

.submit-btn:hover:not(:disabled) {
  filter: brightness(1.08);
  transform: translateY(-1px);
  box-shadow: 0 8px 20px rgba(0,0,0,0.15);
}

.submit-btn:disabled {
  opacity: 0.65;
  cursor: not-allowed;
  transform: none;
}

.loading-text {
  display: flex;
  align-items: center;
  gap: 8px;
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.privacy-note {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 5px;
  font-size: 11px;
  color: #9ca3af;
  text-align: center;
}

/* ─── Success State ─── */
.success-state {
  padding: 40px 32px;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.success-check {
  width: 72px;
  height: 72px;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 8px;
}

.success-title {
  font-family: 'Sora', sans-serif;
  font-size: 22px;
  font-weight: 800;
  color: #111827;
}

.success-msg {
  font-size: 14px;
  line-height: 1.65;
  color: #6b7280;
  margin-bottom: 24px;
}

/* Quiz Invitation Box */
.quiz-invitation-box {
  background: linear-gradient(135deg, rgba(14, 165, 233, 0.05) 0%, rgba(99, 102, 241, 0.05) 100%);
  border: 1px solid rgba(14, 165, 233, 0.2);
  border-radius: 16px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
  text-align: left;
  animation: revealUp 0.6s cubic-bezier(0.2, 0, 0.2, 1) both;
}

.quiz-icon-celestial {
  width: 48px;
  height: 48px;
  background: #0ea5e9;
  color: white;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  flex-shrink: 0;
  box-shadow: 0 4px 12px rgba(14, 165, 233, 0.3);
}

.quiz-invitation-text h4 {
  font-family: 'Sora', sans-serif;
  font-size: 15px;
  font-weight: 700;
  color: #111827;
  margin-bottom: 4px;
}

.quiz-invitation-text p {
  font-size: 13px;
  color: #64748b;
  margin: 0;
  line-height: 1.4;
}

.btn-start-quiz-now {
  margin-left: auto;
  padding: 10px 20px;
  background: #111827;
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 700;
  font-size: 13px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: 0.3s;
}

.btn-start-quiz-now:hover {
  background: #000;
  transform: translateX(4px);
}

@keyframes revealUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.btn-reset {
  display: inline-block;
  margin-top: 8px;
  padding: 10px 20px;
  background: #f3f4f6;
  border: none;
  border-radius: 10px;
  font-family: inherit;
  font-size: 13px;
  font-weight: 600;
  color: #374151;
  cursor: pointer;
  transition: all 0.2s;
  text-decoration: none;
}

.btn-reset:hover {
  background: #e5e7eb;
  color: #111827;
}

/* ─── Not Found ─── */
.not-found {
  max-width: 400px;
  margin: 100px auto;
  text-align: center;
  padding: 48px 40px;
  background: #fff;
  border-radius: 20px;
  border: 1px solid #e5e7eb;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  position: relative;
  z-index: 1;
}

.not-found-icon {
  width: 64px;
  height: 64px;
  border-radius: 16px;
  background: #fef2f2;
  color: #ef4444;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 8px;
}

.not-found h2 {
  font-family: 'Sora', sans-serif;
  font-size: 22px;
  font-weight: 700;
  color: #111827;
}

.not-found p {
  font-size: 14px;
  color: #6b7280;
  line-height: 1.6;
}

/* ─── Responsive ─── */
@media (max-width: 1024px) {
  .page-layout {
    grid-template-columns: 1fr;
    padding: 24px 20px 60px;
  }

  .form-col {
    position: static;
    order: -1;
  }

  .job-title {
    font-size: 28px;
  }

  .hero-body {
    padding: 28px 24px 24px;
  }

  .content-card {
    padding: 24px;
  }

  .form-body {
    padding: 20px 24px 28px;
  }

  .fields-list {
    max-height: none;
    overflow-y: visible;
  }
}

@media (max-width: 480px) {
  .header-inner {
    padding: 0 16px;
  }

  .page-layout {
    padding: 16px 12px 48px;
  }

  .job-title {
    font-size: 24px;
  }

  .nh-label,
  .nh-logo {
    display: none;
  }
}
</style>