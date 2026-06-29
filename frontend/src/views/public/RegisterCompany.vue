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

    <div class="auth-layout auth-layout-wide">
      <!-- LEFT: Welcome Panel -->
      <div class="welcome-panel">
        <div class="welcome-inner">
          <button @click="goBackToSelection" class="btn-back-celestial">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="15 18 9 12 15 6"/></svg>
            Retour
          </button>

          <div class="brand-strip">
            <img :src="logoUrl" alt="NovaHire" class="brand-logo">
            <div class="brand-glow"></div>
          </div>
          
          <h1 class="welcome-title">
            DÉMARREZ VOTRE<br>
            <span class="title-accent">TRANSFORMATION</span>
          </h1>
          <p class="welcome-desc">Configurez votre espace de recrutement premium et transformez votre manière d'attirer les meilleurs talents.</p>
          
          <div class="trust-badges">
            <div class="trust-item">
              <div class="trust-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
              </div>
              <div class="trust-text">
                <strong>Sécurité absolue</strong>
                <span>Données protégées et souveraines</span>
              </div>
            </div>
            <div class="trust-item">
              <div class="trust-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 8v4l3 3"/></svg>
              </div>
              <div class="trust-text">
                <strong>Déploiement rapide</strong>
                <span>Configuration en 3 minutes</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- RIGHT: Form Panel -->
      <div class="form-panel">
        <div class="login-card glass-panel">
          <div class="card-header">
            <h2 class="welcome-title-mobile" style="font-size: 28px; font-weight: 800; margin: 0 0 8px; color: white;">
              DÉMARREZ VOTRE <span class="title-accent" style="color: #00A7E1;">TRANSFORMATION</span>
            </h2>
            <p style="font-size: 14px; color: rgba(255,255,255,0.78);">Configurez votre espace de recrutement premium en 3 étapes.</p>
          </div>

          <!-- Stepper -->
          <div class="stepper-celestial">
            <div class="step-segment" :class="{ active: currentStep >= 1, current: currentStep === 1 }">
              <div class="step-dot">
                <svg v-if="currentStep > 1" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg>
                <span v-else>1</span>
              </div>
              <span class="step-txt">SOCIÉTÉ</span>
            </div>
            <div class="step-line" :style="{ '--fill': currentStep > 1 ? '100%' : '0%' }"></div>
            <div class="step-segment" :class="{ active: currentStep >= 2, current: currentStep === 2 }">
              <div class="step-dot">
                <svg v-if="currentStep > 2" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg>
                <span v-else>2</span>
              </div>
              <span class="step-txt">ADMIN</span>
            </div>
            <div class="step-line" :style="{ '--fill': currentStep > 2 ? '100%' : '0%' }"></div>
            <div class="step-segment" :class="{ active: currentStep >= 3, current: currentStep === 3 }">
              <div class="step-dot">3</div>
              <span class="step-txt">EQUIPE</span>
            </div>
          </div>

          <!-- Messages -->
          <transition name="message-fade">
            <div v-if="errorMessage" class="error-banner">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
              {{ errorMessage }}
            </div>
          </transition>

          <!-- Forms -->
          <div class="form-container-premium">
            <!-- STEP 1: COMPANY -->
            <form v-if="currentStep === 1" @submit.prevent="goToStep2" class="login-form step-fade" key="step1">
              <div class="field full" :class="{ 'has-error': errors.companyName }">
                <div class="label-row"><label>Nom de l'entreprise</label></div>
                <div class="input-shell">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="7" width="20" height="14" rx="2" ry="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
                  <input type="text" v-model="formData.companyName" placeholder="Nom de l'entreprise" @input="validateField('companyName')" />
                </div>
                <span v-if="errors.companyName" class="field-error">{{ errors.companyName }}</span>
              </div>

              <div class="form-grid-celestial stack-mobile">
                <div class="field" :class="{ 'has-error': errors.industry }">
                  <div class="label-row"><label>Secteur d'activité</label></div>
                  <div class="input-shell">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="22" y1="22" x2="15" y2="15"/></svg>
                    <div class="select-shell">
                      <PremiumSelect 
                        v-model="formData.industry"
                        :options="industryOptions"
                        :placeholder="'Sélectionner...'"
                        class="premium-select"
                        @change="onIndustryChange"
                      ></PremiumSelect>
                    </div>
                  </div>
                  <span v-if="errors.industry" class="field-error">{{ errors.industry }}</span>
                </div>

                <!-- Custom other industry input -->
                <div v-if="formData.industry === 'other'" class="field custom-other-field" :class="{ 'has-error': errors.customIndustry }">
                  <div class="label-row"><label>Précisez votre secteur</label></div>
                  <div class="input-shell">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>
                    <input type="text" v-model="formData.customIndustry" placeholder="Ex: Aéronautique, Agriculture..." @input="validateField('customIndustry')" />
                  </div>
                  <span v-if="errors.customIndustry" class="field-error">{{ errors.customIndustry }}</span>
                </div>

                <div class="field" :class="{ 'has-error': errors.employees }">
                  <div class="label-row"><label>Taille de l'effectif</label></div>
                  <div class="input-shell">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
                    <div class="select-shell">
                      <PremiumSelect 
                        v-model="formData.employees"
                        :options="employeesOptions"
                        :placeholder="'Sélectionner...'"
                        class="premium-select"
                        @change="validateField('employees')"
                      ></PremiumSelect>
                    </div>
                  </div>
                  <span v-if="errors.employees" class="field-error">{{ errors.employees }}</span>
                </div>
              </div>



              <div class="form-footer-actions">
                <button type="submit" class="submit-btn">
                  <div class="btn-shimmer"></div>
                  Continuer vers l'admin
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
                </button>
              </div>
            </form>

            <!-- STEP 2: ADMIN -->
            <form v-else-if="currentStep === 2" @submit.prevent="goToStep3" class="login-form step-fade" key="step2" autocomplete="off">
              <div class="form-grid-celestial stack-mobile">
                <div class="field" :class="{ 'has-error': errors.adminFirstName }">
                  <div class="label-row"><label>Prénom de l'admin</label></div>
                  <div class="input-shell">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                    <input type="text" v-model="formData.adminFirstName" placeholder="Votre prénom" @input="validateField('adminFirstName')" />
                  </div>
                  <span v-if="errors.adminFirstName" class="field-error">{{ errors.adminFirstName }}</span>
                </div>
                <div class="field" :class="{ 'has-error': errors.adminLastName }">
                  <div class="label-row"><label>Nom de l'admin</label></div>
                  <div class="input-shell">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                    <input type="text" v-model="formData.adminLastName" placeholder="Votre nom" @input="validateField('adminLastName')" />
                  </div>
                  <span v-if="errors.adminLastName" class="field-error">{{ errors.adminLastName }}</span>
                </div>
              </div>

              <div class="field full" :class="{ 'has-error': errors.adminEmail }">
                <div class="label-row"><label>E-mail administrateur</label></div>
                <div class="input-shell">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                  <input type="email" v-model="formData.adminEmail" placeholder="Votre adresse e-mail" @input="validateField('adminEmail')" autocomplete="nope" />
                </div>
                <span v-if="errors.adminEmail" class="field-error">{{ errors.adminEmail }}</span>
              </div>

              <div class="form-grid-celestial stack-mobile">
                <div class="field" :class="{ 'has-error': errors.adminPassword }">
                  <div class="label-row"><label>Mot de passe</label></div>
                  <div class="input-shell">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                    <input type="password" v-model="formData.adminPassword" placeholder="Votre mot de passe" @input="validateField('adminPassword')" autocomplete="new-password" />
                  </div>
                  <span v-if="errors.adminPassword" class="field-error">{{ errors.adminPassword }}</span>
                </div>
                <div class="field" :class="{ 'has-error': errors.adminConfirmPassword }">
                  <div class="label-row"><label>Confirmer le mot de passe</label></div>
                  <div class="input-shell">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                    <input type="password" v-model="formData.adminConfirmPassword" placeholder="Confirmez le mot de passe" @input="validateField('adminConfirmPassword')" autocomplete="new-password" />
                  </div>
                  <span v-if="errors.adminConfirmPassword" class="field-error">{{ errors.adminConfirmPassword }}</span>
                </div>
              </div>

              <div class="form-footer-actions dual">
                <button type="button" @click="currentStep = 1" class="btn-outline-celestial">Retour</button>
                <button type="submit" class="submit-btn">
                  <div class="btn-shimmer"></div>
                  Dernière étape
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
                </button>
              </div>
            </form>

            <!-- STEP 3: RECRUITER -->
            <form v-else-if="currentStep === 3" @submit.prevent="handleRegister" class="login-form step-fade" key="step3">
              <div class="step3-header">
                <div class="badge-premium-glow">Étape finale</div>
                <h3 class="step-title-final">Inviter un Collaborateur</h3>
                <p class="step-desc-final">Vous pouvez inviter un recruteur principal maintenant, ou le faire plus tard.</p>
              </div>
              
              <div class="skip-section">
                <label class="custom-check-celestial">
                  <input type="checkbox" v-model="skipRecruiter" />
                  <span class="check-box"></span>
                  <span>Passer cette étape pour le moment</span>
                </label>
              </div>

              <div v-if="!skipRecruiter" class="field full">
                <div class="label-row"><label>E-mail du recruteur</label></div>
                <div class="input-shell">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                  <input type="email" v-model="formData.recruiterEmail" placeholder="Email du recruteur" />
                </div>
              </div>

             <div class="legal-accept">
               <label class="custom-check-celestial">
                 <input type="checkbox" v-model="formData.agreeToTerms" />
                 <span class="check-box"></span>
                 <span>J'accepte les <a href="#" @click.prevent="showTermsModal = true" class="legal-link">conditions d'utilisation</a></span>
               </label>
             </div>

            <div class="form-footer-actions dual">
              <button type="button" @click="currentStep = 2" class="btn-outline-celestial">Retour</button>
              <button type="submit" class="submit-btn highlight" :disabled="isLoading">
                <div class="btn-shimmer"></div>
                <span v-if="!isLoading">Créer mon compte</span>
                <span v-else class="loader-pulse">Création...</span>
              </button>
            </div>
          </form>
        </div>
        </div>
      </div>
    </div>

    <!-- TERMS OF SERVICE MODAL -->
    <Teleport to="body">
      <Transition name="modal-fade">
        <div v-if="showTermsModal" class="terms-overlay">
          <div class="terms-modal" @click.stop>
            <div class="terms-header">
              <div class="terms-header-left">
                <div class="terms-icon">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><polyline points="10 9 9 9 8 9"/></svg>
                </div>
                <div>
                  <h3 class="terms-title">Conditions d'Utilisation</h3>
                  <p class="terms-subtitle">Contrat de services de la plateforme NovaHire</p>
                </div>
              </div>
              <button class="terms-close" @click="showTermsModal = false">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width:18px;height:18px;"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>

            <div class="terms-body">
              <h4>1. Acceptation des conditions</h4>
              <p>En créant un compte sur NovaHire, vous acceptez d'être lié par les présentes conditions générales d'utilisation, l'ensemble des lois et règlements applicables, et acceptez que vous êtes responsable du respect de toutes les lois locales applicables.</p>
              
              <h4>2. Utilisation de la plateforme</h4>
              <p>NovaHire fournit un outil de recrutement intelligent facilitant la gestion des offres d'emploi, la réception et l'analyse de candidatures ainsi que le scoring et l'évaluation technique des candidats.</p>
              
              <h4>3. Confidentialité et Protection des Données (RGPD &amp; Loi Tunisienne n° 2004-63)</h4>
              <p>Conformément au Règlement Général sur la Protection des Données (RGPD) et à la loi organique tunisienne n° 2004-63 du 27 juillet 2004 portant sur la protection des données à caractère personnel, les informations des candidats recueillies sur NovaHire sont traitées de manière hautement sécurisée. En tant qu'entreprise utilisatrice, vous vous engagez à respecter le droit à l'information, d'accès, de rectification, d'opposition et de suppression des données personnelles des candidats à tout moment.</p>
              
              <h4>4. Propriété intellectuelle</h4>
              <p>L'ensemble des technologies d'évaluation automatique, des interfaces, du design et du code source de la plateforme NovaHire demeurent la propriété exclusive de NovaHire.</p>

              <h4>5. Limitation de responsabilité</h4>
              <p>NovaHire s'efforce de maximiser la pertinence et la précision des rapports. Toutefois, les indicateurs fournis constituent une aide à la décision. Le choix final d'embauche relève de la responsabilité exclusive de l'entreprise.</p>
            </div>

            <div class="terms-footer">
              <button class="terms-btn-accept" @click="acceptTermsFromModal">J'ai lu et j'accepte</button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script>
import logoUrl from '@/assets/Logo_NovaHire.png'
import { useAuthStore } from '@/stores/authStore'
import PremiumSelect from '@/components/common/PremiumSelect.vue'

export default {
  name: 'RegisterCompany',
  components: { PremiumSelect },
  data() {
    return {
      logoUrl,
      currentStep: 1,
      skipRecruiter: false,
      isLoading: false,
      errorMessage: '',
      successMessage: '',
      formData: {
        companyName: '',
        industry: '',
        customIndustry: '',
        employees: '',
        adminFirstName: '',
        adminLastName: '',
        adminEmail: '',
        adminPassword: '',
        adminConfirmPassword: '',
        recruiterEmail: '',
        agreeToTerms: false
      },
      showTermsModal: false,
      errors: {
        companyName: '',
        industry: '',
        customIndustry: '',
        employees: '',
        adminFirstName: '',
        adminLastName: '',
        adminEmail: '',
        adminPassword: '',
        adminConfirmPassword: '',
        recruiterEmail: ''
      },

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
    },
    industryOptions() {
      return [
        { value: 'tech', label: 'Technologie & IT' },
        { value: 'finance', label: 'Finance, Banque & Assurances' },
        { value: 'healthcare', label: 'Santé & Pharmaceutique' },
        { value: 'education', label: 'Éducation & Formation' },
        { value: 'btp', label: 'BTP & Construction' },
        { value: 'commerce', label: 'Commerce, Vente & Distribution' },
        { value: 'transport', label: 'Transport, Logistique & Supply Chain' },
        { value: 'marketing', label: 'Marketing, Média & Communication' },
        { value: 'rh', label: 'Ressources Humaines & Conseil' },
        { value: 'other', label: 'Autre (Préciser)' }
      ]
    },
    employeesOptions() {
      return [
        { value: '1-10', label: '1 - 10 employés' },
        { value: '11-50', label: '11 - 50 employés' },
        { value: '51-200', label: '51 - 200 employés' },
        { value: '200+', label: '200+ employés' }
      ]
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
    },
    onIndustryChange() {
      if (this.formData.industry !== 'other') {
        this.formData.customIndustry = ''
        this.errors.customIndustry = ''
      }
      this.validateField('industry')
    },
    acceptTermsFromModal() {
      this.formData.agreeToTerms = true
      this.showTermsModal = false
    },
    goToStep2() {
      this.validateField('companyName')
      this.validateField('industry')
      if (this.formData.industry === 'other') {
        this.validateField('customIndustry')
      }
      this.validateField('employees')

      const hasIndustryError = this.formData.industry === 'other' ? !!this.errors.customIndustry : !!this.errors.industry
      const isIndustryFilled = this.formData.industry === 'other' ? !!this.formData.customIndustry : !!this.formData.industry

      if (this.formData.companyName && isIndustryFilled && this.formData.employees && 
          !this.errors.companyName && !hasIndustryError && !this.errors.employees) {
        this.currentStep = 2
        this.errorMessage = ''
      } else {
        this.errorMessage = 'Veuillez remplir tous les champs obligatoires.'
      }
    },
    goToStep3() {
      const adminFields = ['adminFirstName', 'adminLastName', 'adminEmail', 'adminPassword', 'adminConfirmPassword']
      adminFields.forEach(field => this.validateField(field))

      if (adminFields.some(f => this.errors[f]) || adminFields.some(f => !this.formData[f])) {
        this.errorMessage = 'Corrigez les erreurs avant de continuer.'
        return
      }
      this.currentStep = 3
      this.errorMessage = ''
    },
    validateField(field) {
      const val = this.formData[field]
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
      const nameRegex = /^[A-Za-zÀ-ÖØ-öø-ÿ\s-]+$/

      // Fields that are mandatory
      const mandatoryFields = [
        'companyName', 'industry', 'employees', 
        'adminFirstName', 'adminLastName', 'adminEmail', 'adminPassword', 'adminConfirmPassword'
      ]

      if (!val && mandatoryFields.includes(field)) {
        this.errors[field] = 'Ce champ est requis'
        return
      }

      switch (field) {
        case 'companyName':
          if (val.length < 2) this.errors.companyName = 'Trop court (min 2 car.)'
          else this.errors.companyName = ''
          break
        case 'industry':
        case 'employees':
          this.errors[field] = val ? '' : 'Ce champ est requis'
          break
        case 'customIndustry':
          if (this.formData.industry === 'other' && !val) {
            this.errors.customIndustry = 'Veuillez préciser votre secteur'
          } else {
            this.errors.customIndustry = ''
          }
          break
        case 'adminFirstName':
        case 'adminLastName':
          if (!nameRegex.test(val)) this.errors[field] = 'Lettres uniquement'
          else this.errors[field] = ''
          break
        case 'adminEmail':
        case 'recruiterEmail':
          if (val && !emailRegex.test(val)) this.errors[field] = 'Email invalide'
          else this.errors[field] = ''
          break
        case 'adminPassword':
          if (val.length < 8) this.errors.adminPassword = '8 caractères min'
          else if (!/[A-Z]/.test(val)) this.errors.adminPassword = 'Majuscule requise'
          else if (!/[0-9]/.test(val)) this.errors.adminPassword = 'Chiffre requis'
          else this.errors.adminPassword = ''
          
          if (this.formData.adminConfirmPassword) {
            this.validateField('adminConfirmPassword')
          }
          break
        case 'adminConfirmPassword':
          if (val !== this.formData.adminPassword) this.errors.adminConfirmPassword = 'Non identique'
          else this.errors.adminConfirmPassword = ''
          break
      }
    },
    async handleRegister() {
      if (!this.formData.agreeToTerms) {
        this.errorMessage = 'Veuillez accepter les conditions.'
        return
      }
      this.isLoading = true
      this.errorMessage = ''
      
      try {
        const selectedIndustry = this.formData.industry === 'other' 
          ? this.formData.customIndustry 
          : this.industryOptions.find(o => o.value === this.formData.industry)?.label || this.formData.industry

        const payload = { 
          companyName: this.formData.companyName,
          industry: selectedIndustry,
          employeesRange: this.formData.employees,
          adminFirstName: this.formData.adminFirstName,
          adminLastName: this.formData.adminLastName,
          adminEmail: this.formData.adminEmail,
          adminPassword: this.formData.adminPassword,
          recruiterEmail: this.skipRecruiter ? null : this.formData.recruiterEmail
        }
        
        const authStore = useAuthStore()
        await authStore.registerCompany(payload)
        
        // Success! The authStore handles the token storage and reactivity
        this.$router.push('/dashboard')
      } catch (err) {
        this.errorMessage = err.message || "Une erreur s'est produite lors de l'inscription."
      } finally {
        this.isLoading = false
      }
    },
    goBackToSelection() {
      this.$router.push('/login')
    }
  }
}
</script>

<style scoped>
.auth-celestial {
  min-height: 100vh;
  background:
    radial-gradient(ellipse at 20% 10%, rgba(0,167,225,0.08) 0%, transparent 55%),
    radial-gradient(ellipse at 85% 90%, rgba(139,92,246,0.10) 0%, transparent 55%),
    #020617 !important;
  font-family: 'Inter', sans-serif;
  color: white;
  display: flex; align-items: center; justify-content: center;
  padding: 60px 40px;
  position: relative;
  overflow: hidden;
  overflow-x: hidden;

  /* Force Dark Mode Variables Locally */
  --r-main-bg: #020617;
  --card-bg: rgba(13, 20, 32, 0.95);
  --r-border: rgba(255, 255, 255, 0.1);
  --r-text-main: #f8fafc;
  --r-text-sub: #94a3b8;
  --accent-soft: rgba(0, 167, 225, 0.2);
  --accent-grad: linear-gradient(135deg, #00A7E1 0%, #8B5CF6 100%);
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
  display: grid; grid-template-columns: 0.8fr 1.2fr;
  max-width: 1200px; width: 100%;
  gap: 0;
  min-height: 700px;
}

/* ─── LEFT: WELCOME ─── */
.welcome-panel {
  display: flex; flex-direction: column; justify-content: center;
  padding: 60px 40px 60px 20px;
  position: relative;
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
  -webkit-background-clip: text; -webkit-text-fill-color: transparent;
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
  background: rgba(0, 167, 225, 0.1); color: #00A7E1;
  display: flex; align-items: center; justify-content: center;
}
.trust-icon svg { width: 20px; }
.trust-text strong { display: block; font-size: 14px; font-weight: 700; color: white; margin-bottom: 4px; }
.trust-text span { font-size: 12px; color: rgba(255,255,255,0.7); }

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

.card-header { margin-bottom: 24px; text-align: center; }
.greeting-chip { display: inline-block; padding: 6px 14px; background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1); border-radius: 100px; font-size: 12px; font-weight: 700; color: #F7C902; margin-bottom: 16px; }
.card-header h2 { font-size: 28px; font-weight: 800; margin: 0 0 8px; letter-spacing: -1px; }
.card-header p { font-size: 14px; color: rgba(255,255,255,0.78); margin: 0; }

.btn-back-celestial {
  background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1); color: #00A7E1; font-weight: 700;
  display: inline-flex; align-items: center; gap: 8px; font-size: 13px;
  cursor: pointer; padding: 8px 16px; border-radius: 100px; margin-bottom: 40px;
  transition: all 0.3s; width: fit-content;
}
.btn-back-celestial:hover { background: rgba(255, 255, 255, 0.1); transform: translateX(-4px); }
.btn-back-celestial svg { width: 14px; }

/* STEPPER */
.stepper-celestial { display: flex; align-items: center; justify-content: space-between; margin-bottom: 40px; padding: 0 10px; }
.step-segment { display: flex; flex-direction: column; align-items: center; gap: 8px; position: relative; opacity: 0.4; transition: 0.4s; }
.step-segment.active { opacity: 1; }

.step-dot {
  width: 32px; height: 32px; border-radius: 12px; background: rgba(255,255,255,0.04);
  border: 1px solid rgba(255,255,255,0.1); display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 12px; transition: 0.4s; color: rgba(255,255,255,0.85);
}
.step-dot svg { width: 14px; }
.step-segment.current .step-dot { 
  background: rgba(0, 167, 225, 0.1); color: #00A7E1; border-color: rgba(0, 167, 225, 0.4); 
  box-shadow: 0 0 20px rgba(0, 167, 225, 0.2);
}
.step-segment.active .step-txt { font-weight: 700; color: rgba(255,255,255,0.9); }
.step-segment.active:not(.current) .step-dot { background: rgba(16, 185, 129, 0.1); color: #10B981; border-color: rgba(16, 185, 129, 0.3); }

.step-txt { font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: 1px; color: rgba(255,255,255,0.75); }

.step-line { flex: 1; height: 1px; background: rgba(255,255,255,0.08); margin: 0 16px; position: relative; top: -10px; }
.step-line::after { content: ''; position: absolute; left: 0; top: 0; height: 100%; width: var(--fill); background: linear-gradient(90deg, #00A7E1, #10B981); transition: 0.6s; }

/* FORMS */
.form-container-premium { min-height: 400px; }
.form-grid-celestial { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; }
.stack-mobile {  } 
.login-form { display: flex; flex-direction: column; gap: 24px; width: 100%; }

.field { display: flex; flex-direction: column; }
.field.full { grid-column: 1 / -1; }
.label-row { display: flex; margin-bottom: 10px; align-items: center; }
.field label {
  font-size: 11px;
  font-weight: 700;
  color: rgba(255,255,255,0.88);
  display: block;
  text-transform: uppercase;
  letter-spacing: 1.5px;
  font-family: 'Inter', sans-serif;
}

.input-shell { position: relative; width: 100%; border: none; background: transparent; }
.input-shell svg {
  position: absolute; left: 18px; top: 50%; transform: translateY(-50%);
  width: 18px; color: #00A7E1; transition: 0.3s; z-index: 2; pointer-events: none;
}

.input-shell input {
  width: 100%;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: 12px; padding: 16px 18px 16px 52px; color: white;
  transition: all 0.3s ease; font-family: 'Inter', sans-serif; font-size: 14px; font-weight: 500;
}
.input-shell input::placeholder { color: rgba(255,255,255,0.55); }
.input-shell input:focus {
  outline: none; border-color: rgba(0, 167, 225, 0.5);
  background: rgba(0, 167, 225, 0.06); box-shadow: 0 0 0 4px rgba(0,167,225,0.08);
}
.input-shell input:focus ~ svg, .input-shell:focus-within svg { color: #00A7E1; }
.input-shell:has(.premium-select-wrapper.is-open) > svg { color: #00A7E1; }

.field.has-error .input-shell input {
  border-color: rgba(239, 68, 68, 0.4) !important;
  background: rgba(239, 68, 68, 0.04) !important;
}
.field.has-error .input-shell svg { color: #f87171 !important; }

/* ─── Premium Select: single-box visual, matches text inputs ─── */

/* Outer shell is now purely a positioning context — no visual box of its own */
.select-shell {
  position: relative;
  width: 100%;
  background: transparent;
  border: none;
  padding: 0;
  display: block;
  min-height: 52px;
}

/* THE visible box is the wrapper itself */
.select-shell :deep(.premium-select-wrapper) {
  width: 100%;
  min-width: 0;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-radius: 12px;
  min-height: 52px;
  display: flex;
  align-items: stretch;
  cursor: pointer;
  transition: all 0.3s ease;
  box-sizing: border-box;
}

.select-shell :deep(.premium-select-wrapper.is-open),
.select-shell :deep(.premium-select-wrapper):hover {
  border-color: rgba(0, 167, 225, 0.5);
  background: rgba(0, 167, 225, 0.06);
  box-shadow: 0 0 0 4px rgba(0,167,225,0.08);
}

.field.has-error .select-shell :deep(.premium-select-wrapper) {
  border-color: rgba(239, 68, 68, 0.4);
  background: rgba(239, 68, 68, 0.04);
}

/* Inner display contributes layout, padding and text ONLY — no chrome, no hover bg */
.select-shell :deep(.premium-select-display) {
  width: 100%;
  padding: 14px 18px 14px 52px !important;
  background: transparent !important;
  border: none !important;
  border-radius: 0 !important;
  box-shadow: none !important;
  min-height: 52px !important;
  color: white !important;
  font-family: 'Inter', sans-serif !important;
  font-size: 14px !important;
  font-weight: 500 !important;
}
.select-shell :deep(.premium-select-display:hover),
.select-shell :deep(.premium-select-wrapper.is-open .premium-select-display) {
  background: transparent !important;
  border-color: transparent !important;
  box-shadow: none !important;
}

/* Keep ONLY the single .dropdown-icon chevron — hide any other svg inside
   the select that isn't the outer .input-shell icon or this chevron. */
.select-shell :deep(svg):not(.dropdown-icon):not(.check-icon) {
  display: none !important;
}

/* Chevron: size, color, smooth rotate on open */
.select-shell :deep(.dropdown-icon) {
  width: 16px !important;
  height: 16px !important;
  color: rgba(255,255,255,0.5) !important;
  flex-shrink: 0;
  transition: transform 0.3s ease, color 0.3s ease;
}
.select-shell :deep(.premium-select-wrapper.is-open .dropdown-icon) {
  color: #00A7E1 !important;
  transform: rotate(-180deg);
}

/* Placeholder / selected text */
.select-shell :deep(.placeholder-text) {
  color: rgba(255,255,255,0.55) !important;
}
.select-shell :deep(.selected-text) {
  color: #ffffff !important;
}

:deep(.premium-select-menu) {
  background: #111822 !important; /* Extremely dark solid background instead of transparent */
  border: 1px solid rgba(255,255,255,0.1) !important;
  box-shadow: 0 20px 40px rgba(0, 0, 0, 0.8) !important;
  top: calc(100% + 4px) !important;
  max-height: 220px !important;
  overflow-y: auto !important;
  z-index: 99999 !important;
}
/* Custom Scrollbar for Options Dropdown */
:deep(.premium-select-menu)::-webkit-scrollbar {
  width: 6px !important;
}
:deep(.premium-select-menu)::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.02) !important;
}
:deep(.premium-select-menu)::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.15) !important;
  border-radius: 10px !important;
}
:deep(.premium-select-menu)::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.25) !important;
}
:deep(.premium-option) {
  color: rgba(255,255,255,0.85) !important;
  padding: 14px 16px !important; /* Increase hit target size */
}
:deep(.premium-option:hover) {
  background: rgba(255, 255, 255, 0.08) !important;
  color: #fff !important;
}
:deep(.premium-option.is-selected) {
  background: rgba(0, 167, 225, 0.15) !important;
  color: #00A7E1 !important;
}
/* ERROR BANNER & FIELD ERRORS */
.error-banner {
  background: rgba(239, 68, 68, 0.08); border: 1px solid rgba(239, 68, 68, 0.15);
  border-radius: 12px; padding: 14px 16px; color: #F87171; font-size: 13px; font-weight: 600;
  display: flex; align-items: center; gap: 10px; margin-bottom: 24px;
}
.error-banner svg { width: 16px; flex-shrink: 0; }

.field-error {
  display: block; margin-top: 8px; color: #f87171; font-size: 12px; font-weight: 600;
  animation: slideUp 0.2s ease-out;
}
@keyframes slideUp { from { opacity: 0; transform: translateY(4px); } to { opacity: 1; transform: translateY(0); } }

/* PLANS VERTICAL LIST */
.plans-list { display: flex; flex-direction: column; gap: 12px; }
.plan-card-h {
  position: relative; display: flex; align-items: center; gap: 16px;
  padding: 16px 20px; background: rgba(255,255,255,0.02);
  border: 1px solid rgba(255,255,255,0.06); border-radius: 16px;
  cursor: pointer; transition: 0.3s;
}
.plan-card-h:hover { 
  background: rgba(255,255,255,0.04); border-color: rgba(255,255,255,0.15); 
  transform: translateX(4px);
}
.plan-card-h.selected {
  background: rgba(247, 201, 2, 0.05); border-color: rgba(247, 201, 2, 0.4);
  box-shadow: 0 8px 24px rgba(247, 201, 2, 0.1);
  transform: translateX(4px);
}
.plan-card-h input { 
  position: absolute; 
  opacity: 0; 
  width: 0; 
  height: 0; 
}

.plan-icon-h { 
  width: 44px; height: 44px; border-radius: 12px; background: rgba(255,255,255,0.05);
  display: flex; align-items: center; justify-content: center; flex-shrink: 0;
  transition: 0.3s;
}
.plan-icon-h svg { width: 20px; color: rgba(255,255,255,0.75); transition: 0.3s; }
.plan-card-h.selected .plan-icon-h { background: rgba(247, 201, 2, 0.1); }
.plan-card-h.selected .plan-icon-h svg { color: #F7C902; }

.plan-info-h { flex: 1; }
.plan-info-h strong { display: block; font-size: 15px; font-weight: 800; margin-bottom: 2px; color: white; transition: 0.3s; text-transform: uppercase; letter-spacing: 1px; }
.plan-info-h span { display: block; font-size: 12px; font-weight: 700; color: rgba(255,255,255,0.78); line-height: 1.3; transition: 0.3s; text-transform: uppercase; letter-spacing: 1px;}

.plan-price-h { font-size: 14px; font-weight: 900; color: #F7C902; letter-spacing: 0.5px; white-space: nowrap; text-transform: uppercase; }

/* REPLACING BUTTON STYLES WITH LOGIN STYLE */
.form-footer-actions { grid-column: 1 / -1; margin-top: 16px; }
.form-footer-actions.dual { display: grid; grid-template-columns: 140px 1fr; gap: 16px; }

.submit-btn {
  display: flex; align-items: center; justify-content: center; gap: 10px;
  width: 100%; padding: 18px; background: linear-gradient(135deg, #00A7E1 0%, #0077B6 100%);
  color: white; border: none; border-radius: 14px; font-family: inherit; font-size: 15px; font-weight: 700;
  cursor: pointer; transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 8px 24px rgba(0, 167, 225, 0.25); position: relative; overflow: hidden;
}
.submit-btn svg { width: 18px; transition: transform 0.3s; }
.submit-btn:hover:not(:disabled) { transform: translateY(-3px); box-shadow: 0 16px 40px rgba(0, 167, 225, 0.35); }
.submit-btn:hover svg { transform: translateX(4px); }
.submit-btn:disabled { opacity: 0.5; cursor: not-allowed; }
.submit-btn.highlight {
  background: white;
  color: #05080F;
  box-shadow: 0 10px 30px rgba(255,255,255,0.15);
}
.submit-btn.highlight:hover:not(:disabled) {
  background: #f8fafc;
  box-shadow: 0 16px 40px rgba(255,255,255,0.25);
  transform: translateY(-4px);
}
.btn-shimmer {
  position: absolute; top: -50%; left: -50%; width: 200%; height: 200%; background: linear-gradient(45deg, transparent, rgba(255,255,255,0.3), transparent);
  transform: rotate(45deg); transition: 0.8s; opacity: 0;
}
.submit-btn:hover .btn-shimmer { opacity: 1; left: 100%; top: 100%; }

.btn-outline-celestial {
  background: transparent; border: 1.5px solid rgba(255,255,255,0.18); color: rgba(255,255,255,0.88);
  display: flex; align-items: center; justify-content: center;
  padding: 18px; border-radius: 14px; font-weight: 600; font-size: 14px; cursor: pointer; transition: 0.3s;
  font-family: inherit;
}
.btn-outline-celestial:hover { background: rgba(255,255,255,0.05); border-color: rgba(255,255,255,0.2); color: white; transform: translateY(-2px); }

/* STEP 3 SPECIFICS */
.step3-header { grid-column: 1 / -1; text-align: center; margin-bottom: 8px; }
.step-title-final { font-size: 24px; font-weight: 700; margin: 0 0 8px; color: white; }
.step-desc-final { font-size: 14px; color: rgba(255,255,255,0.78); margin: 0; font-weight: 400; }
.skip-section, .legal-accept { grid-column: 1 / -1; }

.custom-check-celestial { display: flex; align-items: center; gap: 12px; cursor: pointer; font-size: 14px; color: rgba(255,255,255,0.85); font-weight: 500; }
.custom-check-celestial input { 
  position: absolute;
  opacity: 0;
  width: 0;
  height: 0;
}
.check-box { width: 20px; height: 20px; border: 1.5px solid rgba(255,255,255,0.15); border-radius: 6px; position: relative; transition: 0.3s; flex-shrink: 0; }
.custom-check-celestial input:checked + .check-box { background: #00A7E1; border-color: #00A7E1; }
.custom-check-celestial input:checked + .check-box::after { content: '✓'; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); color: white; font-weight: 900; font-size: 11px; }

.legal-link { color: #00A7E1; text-decoration: none; font-weight: 600; }
.legal-link:hover { text-decoration: underline; }

.badge-premium-glow { 
  display: inline-block; padding: 6px 16px; 
  background: rgba(0, 167, 225, 0.08); color: #00A7E1; font-size: 11px; font-weight: 700; letter-spacing: 1px; border-radius: 100px;
  border: 1px solid rgba(0, 167, 225, 0.15);
  margin-bottom: 16px; text-transform: uppercase;
}

.step-fade {
  animation: stepFadeIn 0.5s ease-out;
}

@keyframes stepFadeIn {
  from { opacity: 0; transform: translateX(20px); }
  to { opacity: 1; transform: translateX(0); }
}

@media (max-width: 900px) {
  .auth-layout { grid-template-columns: 1fr; }
  .welcome-panel { display: none; }
  .form-panel { padding: 0; }
  .login-card { padding: 32px 24px; }
}
@media (max-width: 768px) {
  .form-grid-celestial.stack-mobile { grid-template-columns: 1fr; }
  .stepper-celestial { padding: 0; }
  .btn-back-celestial { margin-bottom: 24px; }
  .form-footer-actions.dual { grid-template-columns: 1fr; gap: 12px; }
}
/* TERMS OF SERVICE MODAL CSS */
.terms-overlay {
  position: fixed;
  inset: 0;
  z-index: 9999;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(6px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  pointer-events: auto !important;
}

.terms-overlay * {
  pointer-events: auto !important;
}

.terms-modal {
  background: #0f172a;
  border-radius: 20px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  box-shadow: 0 32px 64px -12px rgba(0, 0, 0, 0.6);
  width: 100%;
  max-width: 600px;
  overflow: hidden;
  text-align: left;
  display: flex;
  flex-direction: column;
  max-height: 85vh;
}

.terms-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 24px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.terms-header-left {
  display: flex;
  align-items: center;
  gap: 14px;
}

.terms-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: rgba(0, 167, 225, 0.1);
  color: #00A7E1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.terms-title {
  margin: 0;
  font-size: 18px;
  font-weight: 850;
  color: white;
}

.terms-subtitle {
  margin: 4px 0 0 0;
  font-size: 13px;
  color: #94a3b8;
}

.terms-close {
  background: transparent;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  padding: 6px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.terms-close:hover {
  background: rgba(255, 255, 255, 0.05);
  color: white;
}

.terms-body {
  padding: 24px;
  overflow-y: auto;
  color: #cbd5e1;
  font-size: 14px;
  line-height: 1.6;
}

/* Custom Scrollbar for Terms Body */
.terms-body::-webkit-scrollbar {
  width: 6px;
}
.terms-body::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.04);
  border-radius: 10px;
}
.terms-body::-webkit-scrollbar-thumb {
  background: rgba(129, 140, 248, 0.5);
  border-radius: 10px;
}
.terms-body::-webkit-scrollbar-thumb:hover {
  background: rgba(129, 140, 248, 0.85);
}

.terms-body h4 {
  color: white;
  margin-top: 20px;
  margin-bottom: 8px;
  font-size: 15px;
  font-weight: 750;
}

.terms-body p {
  margin: 0 0 16px 0;
}

.terms-footer {
  padding: 20px 24px;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  justify-content: flex-end;
}

.terms-btn-accept {
  padding: 12px 24px;
  background: #00A7E1;
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 700;
  font-size: 14px;
  cursor: pointer;
  transition: 0.2s;
}

.terms-btn-accept:hover {
  background: #008cc0;
}

.custom-other-field {
  grid-column: 1 / -1;
  animation: slideUp 0.3s ease-out;
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
:global(body:not(.dark-mode) .stepper-celestial) {
  border-color: rgba(0, 0, 0, 0.08);
}
:global(body:not(.dark-mode) .step-dot) {
  background: rgba(0, 0, 0, 0.03);
  border-color: rgba(0, 0, 0, 0.08);
  color: #64748b;
}
:global(body:not(.dark-mode) .step-segment.current .step-dot) {
  background: rgba(0, 167, 225, 0.08);
  color: #00A7E1;
  border-color: rgba(0, 167, 225, 0.3);
}
:global(body:not(.dark-mode) .step-segment.active:not(.current) .step-dot) {
  background: rgba(16, 185, 129, 0.08);
  color: #10B981;
  border-color: rgba(16, 185, 129, 0.2);
}
:global(body:not(.dark-mode) .step-txt) {
  color: #64748b;
}
:global(body:not(.dark-mode) .step-segment.active .step-txt) {
  color: #0f172a;
}
:global(body:not(.dark-mode) .step-line) {
  background: rgba(0, 0, 0, 0.08);
}
:global(body:not(.dark-mode) .step-title-final) {
  color: #0f172a;
}
:global(body:not(.dark-mode) .step-desc-final) {
  color: #475569;
}
:global(body:not(.dark-mode) .custom-check-celestial) {
  color: #334155;
}
:global(body:not(.dark-mode) .check-box) {
  border-color: rgba(0, 0, 0, 0.2);
  background: rgba(0, 0, 0, 0.02);
}
:global(body:not(.dark-mode) .custom-check-celestial input:checked + .check-box) {
  background: #00A7E1;
  border-color: #00A7E1;
}
:global(body:not(.dark-mode) .btn-outline-celestial) {
  border-color: rgba(0, 0, 0, 0.15);
  color: #334155;
}
:global(body:not(.dark-mode) .btn-outline-celestial:hover) {
  background: rgba(0, 0, 0, 0.04);
  color: #0f172a;
  border-color: rgba(0, 0, 0, 0.25);
}

/* Plan Card Styles for Light Mode */
:global(body:not(.dark-mode) .plan-card-h) {
  background: rgba(0, 0, 0, 0.02);
  border-color: rgba(0, 0, 0, 0.08);
}
:global(body:not(.dark-mode) .plan-card-h:hover) {
  background: rgba(0, 0, 0, 0.04);
  border-color: rgba(0, 0, 0, 0.15);
}
:global(body:not(.dark-mode) .plan-card-h.selected) {
  background: rgba(247, 201, 2, 0.08);
  border-color: rgba(247, 201, 2, 0.5);
}
:global(body:not(.dark-mode) .plan-icon-h) {
  background: rgba(0, 0, 0, 0.04);
}
:global(body:not(.dark-mode) .plan-icon-h svg) {
  color: #64748b;
}
:global(body:not(.dark-mode) .plan-info-h strong) {
  color: #0f172a;
}
:global(body:not(.dark-mode) .plan-info-h span) {
  color: #475569;
}

/* Premium Select Light Mode overrides */
:global(body:not(.dark-mode) .select-shell .premium-select-wrapper) {
  background: rgba(0, 0, 0, 0.03) !important;
  border-color: rgba(0, 0, 0, 0.1) !important;
}
:global(body:not(.dark-mode) .select-shell .premium-select-wrapper.is-open),
:global(body:not(.dark-mode) .select-shell .premium-select-wrapper:hover) {
  border-color: rgba(0, 167, 225, 0.5) !important;
  background: rgba(0, 167, 225, 0.06) !important;
}
:global(body:not(.dark-mode) .select-shell .placeholder-text) {
  color: rgba(0, 0, 0, 0.45) !important;
}
:global(body:not(.dark-mode) .select-shell .selected-text) {
  color: #0f172a !important;
}
:global(body:not(.dark-mode) .select-shell .dropdown-icon) {
  color: rgba(0, 0, 0, 0.4) !important;
}
:global(body:not(.dark-mode) .select-shell .premium-select-wrapper.is-open .dropdown-icon) {
  color: #00A7E1 !important;
}
:global(body:not(.dark-mode) .premium-select-menu) {
  background: #ffffff !important;
  border: 1px solid rgba(0, 0, 0, 0.08) !important;
  box-shadow: 0 15px 30px rgba(0, 0, 0, 0.1) !important;
}
:global(body:not(.dark-mode) .premium-option) {
  color: #0f172a !important;
}
:global(body:not(.dark-mode) .premium-option:hover) {
  background: rgba(0, 0, 0, 0.04) !important;
  color: #000000 !important;
}
:global(body:not(.dark-mode) .premium-option.is-selected) {
  background: rgba(0, 167, 225, 0.08) !important;
  color: #00A7E1 !important;
}
:global(body:not(.dark-mode) .premium-option.is-selected .check-icon) {
  color: #00A7E1 !important;
}
</style>
