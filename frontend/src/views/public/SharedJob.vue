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

    <!-- Loading State -->
    <div v-if="isLoading" class="loading-state">
      <div class="spinner"></div>
      <p>Chargement de l'offre en cours...</p>
    </div>

    <!-- Main Layout -->
    <div class="page-layout" v-else-if="offer">
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
                      <span class="file-drop-hint" v-if="!uploadedDocs['cv']">PDF / DOC / DOCX</span>
                    </label>
                  </div>
                </div>

                 <!-- Consent Checkbox (RGPD & Loi Tunisienne n° 2004-63) -->
                 <div class="field-group consent-group">
                   <label class="custom-consent-check">
                     <input type="checkbox" v-model="agreeToPrivacy" required />
                     <span class="custom-check-box"></span>
                     <span class="consent-text">
                       J'accepte le traitement de mes données conformément à la <a href="#" @click.prevent="showPrivacyModal = true" class="legal-link" :style="{ color: offer?.pageColor || '#3b82f6' }">Charte de Protection des Données</a> (RGPD &amp; Loi Tunisienne n° 2004-63).
                     </span>
                   </label>
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
            <p class="success-msg">Votre dossier a bien été transmis à l'équipe de recrutement. Un e-mail de confirmation a été envoyé à l'adresse indiquée dans le formulaire.</p>

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

    <!-- CANDIDATE PRIVACY MODAL -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showPrivacyModal" class="privacy-overlay" @click="showPrivacyModal = false">
          <div class="privacy-modal" @click.stop>
            <div class="privacy-header">
              <div class="privacy-header-left">
                <div class="privacy-icon-box" :style="{ color: offer?.pageColor || '#3b82f6', background: (offer?.pageColor || '#3b82f6') + '15' }">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width:22px;height:22px;"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
                </div>
                <div>
                  <h3 class="privacy-modal-title">Charte de Protection des Données</h3>
                  <p class="privacy-modal-subtitle">Conformité RGPD et Loi Tunisienne n° 2004-63</p>
                </div>
              </div>
              <button class="privacy-close-btn" @click="showPrivacyModal = false">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width:18px;height:18px;"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>

            <div class="privacy-body">
              <h4>1. Collecte et finalité du traitement</h4>
              <p>Dans le cadre du processus de recrutement pour l'offre d'emploi <strong>{{ offer?.title }}</strong> publiée par <strong>{{ offer?.company }}</strong>, les données personnelles que vous renseignez dans ce formulaire (nom, prénom, e-mail, téléphone, profils professionnels, lettre de motivation et CV) sont collectées et traitées uniquement dans le but d'analyser et d'évaluer l'adéquation de votre candidature.</p>
              
              <h4>2. Traitement automatique et utilisation de l'IA</h4>
              <p>Le contenu de votre CV et de vos réponses peut faire l'objet d'une analyse automatisée et d'un scoring par des algorithmes d'Intelligence Artificielle (IA) intégrés à la plateforme NovaHire afin d'extraire vos compétences clés et de synthétiser vos expériences de manière objective pour l'équipe de recrutement.</p>
              
              <h4>3. Cadre réglementaire et base légale</h4>
              <p>Ce traitement repose sur votre consentement explicite et est mené en conformité avec :</p>
              <ul>
                <li>Le <strong>Règlement Général sur la Protection des Données (RGPD)</strong> de l'Union Européenne.</li>
                <li>La <strong>loi organique tunisienne n° 2004-63 du 27 juillet 2004</strong> portant sur la protection des données à caractère personnel.</li>
              </ul>
              
              <h4>4. Destinataires et conservation des données</h4>
              <p>Vos données sont confidentielles et accessibles uniquement par l'équipe de recrutement de <strong>{{ offer?.company }}</strong> et les administrateurs système de NovaHire. Elles ne seront jamais revendues ou divulguées à des tiers. Vos données personnelles sont conservées pour une durée n'excédant pas celle nécessaire aux finalités pour lesquelles elles sont collectées.</p>

              <h4>5. Vos droits (Accès, Rectification, Suppression)</h4>
              <p>Conformément aux réglementations en vigueur, vous disposez d'un droit d'accès, de rectification, d'opposition, de limitation du traitement et d'effacement (droit à l'oubli) de vos données personnelles. Pour exercer ces droits ou pour toute question relative à l'utilisation de vos données, vous pouvez contacter directement l'entreprise ou les administrateurs de la plateforme NovaHire.</p>
            </div>

            <div class="privacy-footer">
              <button class="privacy-btn-accept" :style="{ background: offer?.pageColor || '#3b82f6' }" @click="acceptPrivacyFromModal">J'ai lu et j'accepte</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
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
      isLoading: true,
      offer: null,
      agreeToPrivacy: false,
      showPrivacyModal: false,
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
      applicationSent: false,
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
        type: { FullTime: 'CDI', Contract: 'CDD', Freelance: 'Freelance', Internship: 'Stage', PartTime: 'Temps Partiel', fulltime: 'CDI', contract: 'CDD', freelance: 'Freelance', internship: 'Stage', parttime: 'Temps Partiel' }
      }
      return maps[type]?.[val] || maps[type]?.[val?.toLowerCase()] || val
    },
    async loadOffer() {
      const token = this.$route.params.token
      this.isLoading = true
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
      } finally {
        this.isLoading = false
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
    onCVChange(event) {
      const file = event.target.files[0]
      if (file) this.uploadedDocs['cv'] = file
    },
    async submitApplication() {
      if (!this.agreeToPrivacy) {
        alert("Vous devez accepter les conditions relatives à la protection de vos données personnelles pour continuer.");
        return;
      }
      this.isSubmitting = true
      try {
        const formData = new FormData()
        formData.append('shareToken', this.$route.params.token)
        Object.entries(this.form).forEach(([k, v]) => formData.append(k, v || ''))
        if (this.uploadedDocs.cv) formData.append('CVFile', this.uploadedDocs.cv)
        if (this.source) formData.append('source', this.source)

        await api.post('/public/applications', formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        })
        this.applicationSent = true
      } catch (err) {
        console.error('Erreur soumission:', err)
        alert(err.response?.data?.message || 'Une erreur est survenue. Veuillez réessayer.')
      } finally {
        this.isSubmitting = false
      }
    },
    acceptPrivacyFromModal() {
      this.agreeToPrivacy = true;
      this.showPrivacyModal = false;
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

/* ─── Loading State ─── */
.loading-state {
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
  gap: 16px;
  position: relative;
  z-index: 1;
}

.loading-state p {
  font-family: 'Sora', sans-serif;
  font-size: 16px;
  font-weight: 600;
  color: #374151;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #e5e7eb;
  border-top-color: var(--accent, #3b82f6);
  border-radius: 50%;
  animation: spin 1s linear infinite;
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

/* Consent Checkbox styling */
.consent-group {
  margin-top: 10px;
}

.custom-consent-check {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  cursor: pointer;
  font-size: 12px;
  color: #4b5563;
  line-height: 1.4;
  user-select: none;
}

.custom-consent-check input {
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
}

.custom-check-box {
  width: 18px;
  height: 18px;
  border: 1.5px solid #d1d5db;
  border-radius: 4px;
  position: relative;
  transition: all 0.2s ease;
  flex-shrink: 0;
  margin-top: 1px;
  background: #fff;
}

.custom-consent-check:hover .custom-check-box {
  border-color: var(--accent, #3b82f6);
}

.custom-consent-check input:checked + .custom-check-box {
  background: var(--accent, #3b82f6);
  border-color: var(--accent, #3b82f6);
}

.custom-consent-check input:checked + .custom-check-box::after {
  content: '✓';
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: white;
  font-weight: 800;
  font-size: 11px;
}

.consent-text {
  font-weight: 500;
}

/* Privacy Modal CSS for Candidate Portal */
.privacy-overlay {
  position: fixed;
  inset: 0;
  z-index: 9999;
  background: rgba(17, 24, 39, 0.45);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  pointer-events: auto !important;
}

.privacy-overlay * {
  pointer-events: auto !important;
}

.privacy-modal {
  background: #ffffff;
  border-radius: 20px;
  border: 1px solid #e5e7eb;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.15);
  width: 100%;
  max-width: 620px;
  overflow: hidden;
  text-align: left;
  display: flex;
  flex-direction: column;
  max-height: 85vh;
  font-family: 'DM Sans', sans-serif;
}

.privacy-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid #f3f4f6;
  background: #fafafa;
}

.privacy-header-left {
  display: flex;
  align-items: center;
  gap: 14px;
}

.privacy-icon-box {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.privacy-modal-title {
  font-family: 'Sora', sans-serif;
  margin: 0;
  font-size: 16px;
  font-weight: 700;
  color: #111827;
}

.privacy-modal-subtitle {
  margin: 4px 0 0 0;
  font-size: 12px;
  font-weight: 500;
  color: #6b7280;
}

.privacy-close-btn {
  background: transparent;
  border: none;
  color: #9ca3af;
  cursor: pointer;
  padding: 6px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.privacy-close-btn:hover {
  background: #f3f4f6;
  color: #111827;
}

.privacy-body {
  padding: 24px;
  overflow-y: auto;
  color: #4b5563;
  font-size: 13.5px;
  line-height: 1.6;
}

/* Custom Scrollbar for Privacy Body */
.privacy-body::-webkit-scrollbar {
  width: 6px;
}
.privacy-body::-webkit-scrollbar-track {
  background: rgba(0, 0, 0, 0.03);
  border-radius: 10px;
}
.privacy-body::-webkit-scrollbar-thumb {
  background: rgba(99, 102, 241, 0.35);
  border-radius: 10px;
}
.privacy-body::-webkit-scrollbar-thumb:hover {
  background: rgba(99, 102, 241, 0.6);
}

.privacy-body h4 {
  font-family: 'Sora', sans-serif;
  color: #111827;
  margin-top: 18px;
  margin-bottom: 6px;
  font-size: 14px;
  font-weight: 700;
}

.privacy-body h4:first-of-type {
  margin-top: 0;
}

.privacy-body p {
  margin: 0 0 12px 0;
}

.privacy-body ul {
  margin: 0 0 12px 0;
  padding-left: 20px;
}

.privacy-body li {
  margin-bottom: 4px;
}

.privacy-footer {
  padding: 16px 24px;
  border-top: 1px solid #f3f4f6;
  display: flex;
  justify-content: flex-end;
  background: #fafafa;
}

.privacy-btn-accept {
  padding: 11px 22px;
  color: white;
  border: none;
  border-radius: 10px;
  font-family: 'Sora', sans-serif;
  font-weight: 700;
  font-size: 13px;
  cursor: pointer;
  transition: filter 0.2s ease;
}

.privacy-btn-accept:hover {
  filter: brightness(1.08);
}

.legal-link {
  text-decoration: none;
  font-weight: 600;
  transition: opacity 0.2s ease;
}

.legal-link:hover {
  text-decoration: underline;
  opacity: 0.85;
}

/* Modal Transition */
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.3s ease;
}

.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}

.modal-fade-enter-active .privacy-modal,
.modal-fade-leave-active .privacy-modal {
  transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.modal-fade-enter-from .privacy-modal,
.modal-fade-leave-to .privacy-modal {
  transform: scale(0.9) translateY(10px);
}
</style>