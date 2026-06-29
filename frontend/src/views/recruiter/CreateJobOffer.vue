<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="offres" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main stagger-reveal" :class="{ 'ml-collapsed': sidebarCollapsed }">
      


      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome">
          <div class="header-details-premium">
            <h1 class="premium-title-themed">{{ isEdit ? $t('createOffer.title.edit') : $t('createOffer.title.create') }}</h1>
            <p style="color: var(--r-text-sub); font-weight: 600;">{{ isEdit ? (jobOffer?.title || '') : $t('createOffer.title.subtitle') }}</p>
          </div>
        </div>
      </header>

      <div class="r-stepper anim-reveal-down" style="animation-delay: 0.1s">
        <div v-for="(s, i) in steps" :key="i" class="r-step-pill" :class="{ active: currentStep === i, done: currentStep > i }" @click="currentStep = i">
          <div class="step-icon">
            <svg v-if="currentStep > i" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg>
            <span v-else>{{ i + 1 }}</span>
          </div>
          <span class="step-label">{{ s.label }}</span>
        </div>
      </div>

      <div class="page-body">
        <div class="creation-grid-expert">
          <div class="form-main-container anim-reveal-up" style="animation-delay: 0.2s">
            <div class="r-card form-inner-card anim-reveal-up" style="animation-delay: 0.2s">
              <transition name="step-slide" mode="out-in">
                <!-- STEP 1: IDENTITÉ DU POSTE -->
                <div v-if="currentStep === 0" :key="0" class="step-content">
                  <div class="section-header-premium">
                    <h3>{{ $t('createOffer.general.stepTitle') }}</h3>
                  </div>

                  <div class="form-section-premium">
                    <div class="section-title-premium">
                      <span class="num">01</span>
                      <h4>{{ $t('createOffer.general.basicDetails') }}</h4>
                    </div>
                    <div class="r-input-group full" :class="{ 'has-error': errors.title }">
                      <label>{{ $t('createOffer.general.jobTitle') }}</label>
                      <input type="text" v-model="form.title" :placeholder="$t('createOffer.general.jobTitle')" @input="validateField('title')">
                      <span v-if="errors.title" class="error-msg">{{ errors.title }}</span>
                    </div>

                    <div class="r-form-row">
                      <div class="r-input-group">
                        <label>{{ $t('createOffer.general.department') }}</label>
                        <PremiumSelect 
                          v-model="form.department"
                          :options="departmentOptions"
                          class="expert-select-glass"
                          style="padding: 0; min-height: 52px;"
                        />
                      </div>
                      <div class="r-input-group" v-if="form.department === 'other'">
                        <label>{{ $t('createOffer.general.deptName') }}</label>
                        <input type="text" v-model="form.otherDepartment" :placeholder="$t('createOffer.general.deptName')">
                      </div>
                    </div>

                    <div class="r-form-row mt-20">
                      <div class="r-input-group">
                        <label>{{ $t('createOffer.general.contractType') }}</label>
                        <PremiumSelect 
                          v-model="form.type"
                          :options="typeOptions"
                          class="expert-select-glass"
                          style="padding: 0; min-height: 52px;"
                        />
                      </div>
                    </div>
                  </div>

                  <div class="form-section-premium mt-32">
                    <div class="section-title-premium">
                      <span class="num">02</span>
                      <h4>{{ $t('createOffer.general.location') }}</h4>
                    </div>
                    <div class="r-form-row">
                      <div class="r-input-group" :class="{ 'has-error': errors.location }">
                        <label>{{ $t('createOffer.general.location') }}</label>
                        <input type="text" v-model="form.location" :placeholder="$t('createOffer.general.location')" @input="validateField('location')">
                        <span v-if="errors.location" class="error-msg">{{ errors.location }}</span>
                      </div>
                      <div class="r-input-group">
                        <label>{{ $t('createOffer.general.remote') }}</label>
                        <PremiumSelect 
                          v-model="form.remotePolicy"
                          :options="remoteOptions"
                          class="expert-select-glass"
                          style="padding: 0; min-height: 52px;"
                        />
                      </div>
                    </div>
                  </div>
                </div>

                <!-- STEP 2: PROFIL & CONTENU -->
                <div v-else-if="currentStep === 1" :key="1" class="step-content">
                  <div class="section-header-premium">
                    <h3>{{ $t('createOffer.profile.stepTitle') }}</h3>
                  </div>

                  <div class="form-section-premium">
                    <div class="section-title-premium">
                      <span class="num">03</span>
                      <h4>{{ $t('createOffer.profile.missions') }}</h4>
                    </div>
                    <div class="r-input-group full" :class="{ 'has-error': errors.description }">
                      <div class="flex-between">
                        <label style="margin: 0;">{{ $t('createOffer.profile.missions') }}</label>
                        <button class="btn-sparkle" @click="generateDescription" :disabled="isGenerating" style="padding: 4px 12px; font-size: 12px; gap: 6px;">
                          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14"><path d="M12 2l3 6 6 3-6 3-3 6-3-6-6-3 6-3z"/></svg>
                          {{ isGenerating ? 'Génération...' : 'Générer via l\'IA' }}
                        </button>
                      </div>
                      <textarea v-model="form.description" rows="8" :placeholder="$t('createOffer.profile.placeholders.desc')" @input="validateField('description')"></textarea>
                      <span v-if="errors.description" class="error-msg">{{ errors.description }}</span>
                    </div>
                  </div>

                  <div class="form-section-premium mt-32">
                    <div class="section-title-premium">
                      <span class="num">04</span>
                      <h4>{{ $t('createOffer.general.skills') }}</h4>
                    </div>
                    <div class="r-skills-tagger">
                      <div class="tags-container">
                        <span v-for="(skill, idx) in form.skills" :key="idx" class="r-skill-pill">
                          {{ skill }}
                          <button @click="removeSkill(idx)">&times;</button>
                        </span>
                        <input 
                          type="text" 
                          v-model="newSkill" 
                          :placeholder="$t('createOffer.profile.placeholders.addSkill')" 
                          @keydown.enter.prevent="addSkill"
                        >
                      </div>
                    </div>
                  </div>

                  <div class="form-section-premium mt-32">
                    <div class="section-title-premium">
                      <span class="num">05</span>
                      <h4>{{ $t('createOffer.profile.experienceSalary') }}</h4>
                    </div>
                    <div class="r-form-row">
                       <div class="r-input-group">
                        <label>{{ $t('createOffer.profile.reqExperience') }}</label>
                        <select v-model="form.experienceLevel">
                          <option value="beginner">{{ $t('createOffer.profile.exp.beginner') }}</option>
                          <option value="junior">{{ $t('createOffer.profile.exp.junior') }}</option>
                          <option value="intermediate">{{ $t('createOffer.profile.exp.intermediate') }}</option>
                          <option value="senior">{{ $t('createOffer.profile.exp.senior') }}</option>
                          <option value="expert">{{ $t('createOffer.profile.exp.expert') }}</option>
                        </select>
                      </div>
                      <div class="r-input-group">
                         <label>{{ $t('createOffer.profile.salary') }}</label>
                         <div class="salary-inputs">
                            <input type="number" v-model="form.salaryMin" :placeholder="$t('createOffer.profile.placeholders.salaryMin')">
                            <span class="sep">-</span>
                            <input type="number" v-model="form.salaryMax" :placeholder="$t('createOffer.profile.placeholders.salaryMax')">
                         </div>
                         <label class="r-checkbox mt-8">
                            <input type="checkbox" v-model="form.salaryConfidential">
                            <span>{{ $t('createOffer.profile.salaryConfidential') }}</span>
                         </label>
                      </div>
                    </div>
                  </div>
                </div>

                <!-- STEP 3: CANDIDATURE & PUBLICATION -->
                <div v-else-if="currentStep === 2" :key="2" class="step-content">
                  <div v-if="!showSuccessScreen">
                    <div class="section-header-premium">
                      <h3>{{ $t('createOffer.publish.stepTitle') }}</h3>
                    </div>

                    <div class="form-config-list">
                      <div v-for="field in formFields" :key="field.id" class="config-item" :class="{ 'is-off': !field.enabled, 'is-locked': !field.canDisable }">
                        <div class="field-info">
                          <div class="f-icon">
                            <svg v-if="field.id === 'fullname'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                            <svg v-else-if="field.id === 'email'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                            <svg v-else-if="field.id === 'phone'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
                            <svg v-else-if="field.id === 'resume'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z"/><polyline points="13 2 13 9 20 9"/></svg>
                            <svg v-else-if="field.id === 'coverLetter'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
                            <svg v-else-if="field.id === 'linkedin'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M16 8a6 6 0 0 1 6 6v7h-4v-7a2 2 0 0 0-2-2 2 2 0 0 0-2 2v7h-4v-7a6 6 0 0 1 6-6z"/><rect x="2" y="9" width="4" height="12"/><circle cx="4" cy="4" r="2"/></svg>
                            <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                          </div>
                          <div class="f-text">
                            <span class="f-name">{{ field.name }}</span>
                            <p class="f-desc">{{ field.desc }}</p>
                          </div>
                        </div>
                        
                        <div class="field-actions">
                          <label class="r-switch">
                            <span>{{ $t('createOffer.publish.show') }}</span>
                            <input type="checkbox" v-model="field.enabled" :disabled="!field.canDisable" @change="!field.enabled ? field.required = false : null">
                            <span class="slider"></span>
                          </label>
                          <label class="r-switch" :class="{ 'is-locked': !field.enabled }">
                            <span>{{ $t('createOffer.publish.required') }}</span>
                            <input type="checkbox" v-model="field.required" :disabled="!field.enabled" @change="field.required ? field.enabled = true : null">
                            <span class="slider"></span>
                          </label>
                        </div>
                      </div>
                    </div>

                    <div class="form-section-premium mt-32">
                      <div class="section-title-premium">
                        <span class="num">06</span>
                        <h4>{{ $t('createOffer.publish.matchingTitle') }}</h4>
                      </div>
                      <p class="section-hint">{{ $t('createOffer.publish.matchingSub') }}</p>
                      
                      <div class="ai-weights-grid">
                        <div class="weight-control">
                          <div class="weight-label-row">
                            <label>{{ $t('createOffer.publish.weights.exp') }}</label>
                            <span class="weight-val">{{ form.weightExperience }}%</span>
                          </div>
                          <input type="range" v-model.number="form.weightExperience" min="0" max="100" class="weight-slider">
                        </div>

                        <div class="weight-control">
                          <div class="weight-label-row">
                            <label>{{ $t('createOffer.publish.weights.edu') }}</label>
                            <span class="weight-val">{{ form.weightEducation }}%</span>
                          </div>
                          <input type="range" v-model.number="form.weightEducation" min="0" max="100" class="weight-slider">
                        </div>

                        <div class="weight-control">
                          <div class="weight-label-row">
                            <label>{{ $t('createOffer.publish.weights.skills') }}</label>
                            <span class="weight-val">{{ form.weightSkills }}%</span>
                          </div>
                          <input type="range" v-model.number="form.weightSkills" min="0" max="100" class="weight-slider">
                        </div>

                        <div v-if="totalWeight !== 100" class="weight-error pulse">
                          Total : {{ totalWeight }}% (doit être égal à 100%)
                        </div>
                      </div>
                    </div>

                    <div class="form-section-premium mt-32">
                      <div class="section-title-premium">
                        <span class="num">⚡</span>
                        <h4>Filtre automatique IA</h4>
                      </div>
                      <p class="section-hint">Les candidats en dessous de ce seuil seront automatiquement rejetés après l'analyse IA. Mettez à 0 pour désactiver.</p>
                      <div class="weight-control">
                        <div class="weight-label-row">
                          <label>Seuil de rejet automatique</label>
                          <span class="weight-val" :class="{ 'text-danger': form.autoRejectThreshold > 0 }">{{ form.autoRejectThreshold }}%</span>
                        </div>
                        <input type="range" v-model.number="form.autoRejectThreshold" min="0" max="80" step="5" class="weight-slider reject-slider">
                      </div>
                      <div v-if="form.autoRejectThreshold > 0" class="auto-reject-hint">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" width="14"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
                        Tout candidat avec un score IA &lt; {{ form.autoRejectThreshold }}% sera rejeté automatiquement.
                      </div>
                    </div>

                    <div class="section-header-premium mt-40">
                      <h3>{{ $t('createOffer.publish.workflowTitle') }}</h3>
                    </div>

                    <div class="form-section-premium mt-32">
                      <div class="section-title-premium">
                        <span class="num">07</span>
                        <h4>{{ $t('createOffer.publish.publicationTitle') }}</h4>
                      </div>
                      <div class="r-form-row">
                        <div class="r-input-group" :class="{ 'has-error': errors.deadline }">
                          <label>{{ $t('createOffer.publish.deadline') }}</label>
                          <input type="date" v-model.lazy="form.deadline" @change="validateField('deadline')">
                          <span v-if="errors.deadline" class="error-msg">{{ errors.deadline }}</span>
                        </div>
                        <div class="r-input-group">
                          <label>{{ $t('createOffer.publish.action') }}</label>
                          <select v-model="form.status">
                            <option value="Published">{{ $t('createOffer.publish.status.publish') }}</option>
                            <option value="Draft">{{ $t('createOffer.publish.status.draft') }}</option>
                          </select>
                        </div>
                      </div>
                    </div>
                  </div>

                  <!-- SUCCESS SCREEN -->
                  <div v-else class="success-screen anim-reveal-up">
                    <div class="success-icon">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg>
                    </div>
                    <h2>{{ $t('createOffer.publish.success.title') }}</h2>
                    <p>{{ $t('createOffer.publish.success.subtitle') }}</p>
                    
                    <div class="share-box">
                      <label>{{ $t('createOffer.publish.success.shareLink') }}</label>
                      <div class="share-input">
                        <input type="text" readonly :value="getPublicLink()">
                        <button @click="copyLink">{{ $t('common.copy') }}</button>
                      </div>
                    </div>

                    <button class="btn-premium btn-secondary" @click="$router.push('/offres')">{{ $t('createOffer.publish.success.back') }}</button>
                  </div>
                </div>
              </transition>

              <div class="form-navigation" v-if="!showSuccessScreen">
                <button v-if="currentStep > 0" class="btn-premium btn-secondary" @click="currentStep--">{{ $t('common.previous') }}</button>
                <div style="flex:1"></div>
                <button v-if="currentStep < 2" class="btn-luxury primary" @click="handleNextStep">{{ $t('common.next') }}</button>
                <button v-else class="btn-luxury primary" @click="publishOffer" :disabled="isLoading || isSubmitting">
                  {{ isEdit ? $t('common.save') : (form.status === 'Published' ? $t('createOffer.publish.status.publish') : $t('createOffer.publish.status.draft')) }}
                </button>
              </div>
            </div>
          </div>

          <!-- PREVIEW SIDEBAR -->
          <div class="preview-sidebar">
            <div class="r-card preview-card">
              <div class="mock-browser">
                <div class="browser-header">
                  <div class="dots"><span></span><span></span><span></span></div>
                </div>
                <div class="mock-content">
                  <div class="mock-hero" style="background: var(--accent);">
                    <div class="mock-logo">NH</div>
                  </div>
                  <div class="mock-body">
                    <div class="mock-badge">{{ form.department ? $t('createOffer.general.depts.' + form.department).toUpperCase() : '' }}</div>
                    <h3>{{ form.title || $t('createOffer.general.jobTitle') }}</h3>
                    <p class="mock-meta">{{ form.location || $t('createOffer.general.location') }} · {{ form.type }}</p>
                    
                    <div class="mock-details">
                      <div class="m-item"><strong>Exp:</strong> {{ form.experienceLevel }}</div>
                      <div class="m-item" v-if="!form.salaryConfidential && (form.salaryMin || form.salaryMax)">
                        <strong>Sal:</strong> {{ form.salaryMin || '?' }} - {{ form.salaryMax || '?' }}K
                      </div>
                    </div>

                    <div class="mock-skills">
                      <span v-for="s in form.skills.slice(0,3)" :key="s">{{ s }}</span>
                    </div>

                    <div class="mock-btn" style="background: var(--accent);">{{ $t('offers.apply') || 'Postuler' }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <ToastContainer />
    </main>
  </div>
</template>

<script>
import api from '@/api/axios'
import Sidebar from '@/components/layout/Sidebar.vue'

import ToastContainer from '@/components/common/ToastContainer.vue'
import PremiumSelect from '@/components/common/PremiumSelect.vue'
import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'
import { useToastStore } from '@/stores/toastStore'

export default {
  name: 'CreateJobOffer',
  components: { Sidebar,  ToastContainer, PremiumSelect },
  data() {
    const authStore = useAuthStore()
    const themeStore = useThemeStore()
    const isCompanyOwner = authStore.isAdmin && !authStore.isSuperAdmin

    return {
      authStore,
      themeStore,
      id: this.$route.params.id,
      isEdit: !!this.$route.params.id,
      sidebarCollapsed: false,
      isCompanyOwner,
      currentStep: 0,
      isLoading: false,
      isSubmitting: false,
      isGenerating: false,
      newSkill: '',
      jobOffer: null,
      form: {
        title: '',
        department: 'tech',
        otherDepartment: '',
        type: 'FullTime',
        location: '',
        description: '',
        skills: [],
        remotePolicy: 'hybrid',
        experienceLevel: 'intermediate',
        deadline: '',
        salaryMin: null,
        salaryMax: null,
        salaryConfidential: false,
        status: 'Published',
        weightExperience: 40,
        weightEducation: 30,
        weightSkills: 30,
        autoRejectThreshold: 0
      },
      showSuccessScreen: false,
      errors: {
        title: '',
        location: '',
        description: '',
        deadline: ''
      }
    }
  },
  computed: {
    formFields() {
      return [
        { id: 'fullname', name: this.$t('profile.placeholders.fullName'), desc: this.$t('profile.placeholders.fullName'), enabled: true, required: true, canDisable: false },
        { id: 'email', name: this.$t('profile.email'), desc: this.$t('profile.email'), enabled: true, required: true, canDisable: false },
        { id: 'phone', name: this.$t('profile.phone'), desc: this.$t('profile.phone'), enabled: true, required: false, canDisable: true },
        { id: 'resume', name: 'Curriculum Vitae (CV)', desc: 'PDF / Word', enabled: true, required: true, canDisable: true },
        { id: 'coverLetter', name: 'Lettre de motivation', desc: 'Motivation', enabled: true, required: false, canDisable: true },
        { id: 'linkedin', name: 'LinkedIn', desc: 'LinkedIn profile', enabled: true, required: false, canDisable: true },
        { id: 'portfolio', name: 'Portfolio', desc: 'Portfolio link', enabled: false, required: false, canDisable: true }
      ]
    },
  async mounted() {
    if (this.isEdit) {
      this.isLoading = true
      try {
        const res = await api.get(`/JobOffer/${this.id}`)
        const data = res.data
        this.form = {
          title: data.title,
          department: data.department || 'tech',
          type: data.type || 'FullTime',
          location: data.location || '',
          description: data.description || '',
          skills: data.skills || [],
          remotePolicy: data.remotePolicy?.toLowerCase() || 'hybrid',
          experienceLevel: data.experienceLevel?.toLowerCase() || 'intermediate',
          deadline: data.deadline ? data.deadline.split('T')[0] : '',
          salaryMin: data.salaryRange?.split('-')[0] || null,
          salaryMax: data.salaryRange?.split('-')[1] || null,
          salaryConfidential: data.salaryConfidential || false,
          status: this.normalizeStatusForForm(data.status || data.visibility),
          weightExperience: data.weightExperience ?? 40,
          weightEducation: data.weightEducation ?? 30,
          weightSkills: data.weightSkills ?? 30,
          autoRejectThreshold: data.autoRejectThreshold ?? 0
        }
        
        if (data.formConfig) {
          const cfg = data.formConfig
          this.formFields.forEach(f => {
            const serverKey = 'require' + f.id.charAt(0).toUpperCase() + f.id.slice(1).replace('resume', 'CV').replace('coverLetter', 'CoverLetter').replace('linkedin', 'LinkedIn').replace('portfolio', 'Portfolio')
            // Special cases for mapping
            let val = false
            if (f.id === 'resume') val = cfg.requireCV
            else if (f.id === 'coverLetter') val = cfg.requireCoverLetter
            else if (f.id === 'linkedin') val = cfg.requireLinkedIn
            else if (f.id === 'portfolio') val = cfg.requirePortfolio
            else if (f.id === 'fullname') val = cfg.requireFullName
            else if (f.id === 'email') val = cfg.requireEmail
            else if (f.id === 'phone') val = cfg.requirePhone

            f.enabled = val
            f.required = val
          })
        }
      } catch (err) {
        useToastStore().show('Erreur lors du chargement', 'error')
      } finally {
        this.isLoading = false
      }
    }
  },
  computed: {
    sidebarVars() {
      return {}
    },
    totalWeight() {
      return parseInt(this.form.weightExperience) + parseInt(this.form.weightEducation) + parseInt(this.form.weightSkills)
    },
    steps() {
      return [
        { id: 'details', label: this.$t('steps.details') },
        { id: 'content', label: this.$t('steps.profile') },
        { id: 'publish', label: this.$t('steps.diffusion') }
      ]
    },
    computedFields() {
      return [
        { id: 1, key: 'Email', enabled: true },
        { id: 2, key: 'CV', enabled: true }
      ]
    },
    departmentOptions() {
      return [
        { value: 'architecture', label: this.$t('createOffer.general.depts.architecture') },
        { value: 'commercial', label: this.$t('createOffer.general.depts.commercial') },
        { value: 'finance', label: this.$t('createOffer.general.depts.finance') },
        { value: 'data', label: this.$t('createOffer.general.depts.data') },
        { value: 'management', label: this.$t('createOffer.general.depts.management') },
        { value: 'logistics', label: this.$t('createOffer.general.depts.logistics') },
        { value: 'marketing', label: this.$t('createOffer.general.depts.marketing') },
        { value: 'hr', label: this.$t('createOffer.general.depts.hr') },
        { value: 'support', label: this.$t('createOffer.general.depts.support') },
        { value: 'tech', label: this.$t('createOffer.general.depts.tech') },
        { value: 'other', label: this.$t('createOffer.general.depts.other') }
      ]
    },
    typeOptions() {
      return [
        { value: 'FullTime', label: this.$t('createOffer.general.types.cdi') },
        { value: 'Contract', label: this.$t('createOffer.general.types.cdd') },
        { value: 'PartTime', label: this.$t('createOffer.general.types.partTime') },
        { value: 'Freelance', label: this.$t('createOffer.general.types.freelance') },
        { value: 'Internship', label: this.$t('createOffer.general.types.stage') }
      ]
    },
    remoteOptions() {
      return [
        { value: 'office', label: this.$t('createOffer.general.remoteOptions.office') },
        { value: 'hybrid', label: this.$t('createOffer.general.remoteOptions.hybrid') },
        { value: 'remote', label: this.$t('createOffer.general.remoteOptions.remote') }
      ]
    }
  },
  methods: {
    normalizeStatusForForm(status) {
      const raw = `${status || ''}`.toLowerCase()
      if (raw === 'draft' || raw === '0' || raw === 'brouillon') return 'Draft'
      if (raw === 'published' || raw === 'active' || raw === '1' || raw === 'public') return 'Published'
      return 'Draft'
    },
    validateField(field) {
      if (field === 'title') {
        if (!this.form.title) this.errors.title = this.$t('notifications.updateError') // Generic error for now
        else if (this.form.title.length < 5) this.errors.title = '5 chars min'
        else this.errors.title = ''
      }
      if (field === 'location') {
        if (!this.form.location) this.errors.location = 'Requis'
        else this.errors.location = ''
      }
      if (field === 'description') {
        if (!this.form.description) this.errors.description = 'Requis'
        else this.errors.description = ''
      }
      if (field === 'deadline') {
        if (!this.form.deadline) this.errors.deadline = 'Requis'
        else this.errors.deadline = ''
      }
    },
    async generateDescription() {
      if (!this.form.title) {
        useToastStore().show("Veuillez d'abord renseigner le titre du poste", "warning")
        this.currentStep = 0
        setTimeout(() => this.scrollToFirstError(), 100)
        return
      }

      this.isGenerating = true
      try {
        const response = await api.post('/Recruiter/job-offers/ai/generate-description', {
          jobTitle: this.form.title
        })
        
        if (response.data && response.data.description) {
          this.form.description = response.data.description
          useToastStore().show('Description générée avec succès !', 'success')
        }
      } catch (err) {
        useToastStore().show('Échec de la génération par IA.', 'error')
      } finally {
        this.isGenerating = false
      }
    },
    addSkill() {
      const s = this.newSkill.trim()
      if (s && !this.form.skills.includes(s)) {
        this.form.skills.push(s)
        this.newSkill = ''
      }
    },
    removeSkill(idx) {
      this.form.skills.splice(idx, 1)
    },
    handleNextStep() {
      if (this.currentStep === 0) {
        this.validateField('title')
        this.validateField('location')
        if (this.errors.title || this.errors.location) {
           this.scrollToFirstError()
           return
        }
      } else if (this.currentStep === 1) {
        this.validateField('description')
        if (this.errors.description) {
           this.scrollToFirstError()
           return
        }
      }
      this.currentStep++
    },
    async publishOffer() {
      // Guard against double-click / concurrent submissions
      if (this.isSubmitting) return

      this.validateField('deadline')
      if (this.errors.deadline) {
        this.scrollToFirstError()
        return
      }

      if (this.totalWeight !== 100) {
        useToastStore().show(this.$t('createOffer.publish.matchingTitle') + ': 100%', 'error')
        return
      }

      this.isSubmitting = true
      this.isLoading = true
      try {
        const payload = { ...this.form }
        
        // Handle "Other" department
        if (payload.department === 'other') {
          payload.department = payload.otherDepartment || 'Autre'
        }
        delete payload.otherDepartment

        // Sanitize nullable fields
        payload.deadline = payload.deadline || null
        
        // Ensure salary is numeric or null, avoid NaN
        const minVal = parseInt(payload.salaryMin)
        const maxVal = parseInt(payload.salaryMax)
        payload.salaryMin = !isNaN(minVal) ? minVal : null
        payload.salaryMax = !isNaN(maxVal) ? maxVal : null

        const threshold = parseInt(payload.autoRejectThreshold, 10)
        payload.autoRejectThreshold = Number.isNaN(threshold) || threshold <= 0
          ? 0
          : Math.min(80, threshold)

        // Map formFields to FormConfig object (Sync with backend DTO)
        const formConfig = {}
        this.formFields.forEach(f => {
          let key = 'Require' + f.id.charAt(0).toUpperCase() + f.id.slice(1)
          if (f.id === 'fullname') key = 'RequireFullName'
          if (f.id === 'resume') key = 'RequireCV'
          if (f.id === 'coverLetter') key = 'RequireCoverLetter'
          if (f.id === 'linkedin') key = 'RequireLinkedIn'
          if (f.id === 'portfolio') key = 'RequirePortfolio'
          
          formConfig[key] = f.enabled
        })
        payload.FormConfig = formConfig

        // Status Management
        payload.visibility = payload.status // For DTO compatibility
        
        let response
        if (this.isEdit) {
          response = await api.put(`/JobOffer/${this.id}`, payload)
        } else {
          response = await api.post('/JobOffer', payload)
        }
        
        if (payload.status === 'Published' && !this.isEdit) {
          this.shareToken = response.data.shareToken || ''
          this.showSuccessScreen = true
          useToastStore().show('Offre publiée avec succès !', 'success')
        } else {
          useToastStore().show(this.isEdit ? 'Mise à jour réussie' : 'Brouillon enregistré', 'success')
          setTimeout(() => {
            this.$router.push('/offres')
          }, 1500)
        }
      } catch (err) {
        console.error('Erreur publication:', err)
        useToastStore().show("Erreur lors de la création de l'offre", 'error')
      } finally {
        this.isLoading = false
        this.isSubmitting = false
      }
    },
    getPublicLink() {
      return `${window.location.origin}/shared-job/${this.shareToken}`
    },
    copyLink() {
      navigator.clipboard.writeText(this.getPublicLink()).then(() => {
        useToastStore().show('Lien copié dans le presse-papier', 'success')
      })
    },
    shareOnLinkedIn() {
      const url = encodeURIComponent(this.getPublicLink())
      window.open(`https://www.linkedin.com/sharing/share-offsite/?url=${url}`, '_blank')
    },
    scrollToFirstError() {
      this.$nextTick(() => {
        const errorEl = document.querySelector('.has-error')
        if (errorEl) {
          errorEl.scrollIntoView({ behavior: 'smooth', block: 'center' })
        }
      })
    }
  }
}
</script>

<style>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";
</style>

<style scoped>
.dashboard-layout { background: transparent !important; }

/* COMPONENT-SPECIFIC STYLES */
.creation-grid-expert {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 24px;
  align-items: flex-start;
}

.r-stepper {
  display: flex;
  gap: 16px;
  margin-bottom: 24px;
  justify-content: center;
}

.r-step-pill {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 20px;
  border-radius: 100px;
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  cursor: pointer;
  transition: 0.3s;
}

.r-step-pill.active {
  border-color: var(--accent);
  background: var(--accent-grad);
  color: var(--accent-contrast);
}

.flex-between {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

.btn-sparkle {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, rgba(99,102,241,0.1) 0%, rgba(168,85,247,0.1) 100%);
  color: var(--accent);
  border: 1px solid rgba(99,102,241,0.3);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 700;
}
.btn-sparkle:hover:not(:disabled) {
  background: linear-gradient(135deg, rgba(99,102,241,0.2) 0%, rgba(168,85,247,0.2) 100%);
  border-color: rgba(168,85,247,0.5);
  transform: translateY(-1px);
}
.btn-sparkle:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.r-step-pill.done {
  border-color: #10b981;
  background: rgba(16, 185, 129, 0.05);
}

.step-icon {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: var(--r-card-bg);
  border: 1px solid var(--r-border);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 800;
  color: var(--r-text-sub);
}

.r-step-pill.active .step-icon {
  background: var(--accent-contrast);
  border-color: var(--accent-contrast);
  color: var(--accent);
}

.r-step-pill.done .step-icon {
  background: #10b981;
  border-color: #10b981;
  color: white;
}

.step-label {
  font-size: 14px;
  font-weight: 700;
  color: var(--r-text-sub);
}

.r-step-pill.active .step-label { color: var(--accent-contrast); }

/* FORM SECTIONS */
.form-inner-card { padding: 24px 32px; max-width: 850px; margin: 0 auto; }
.section-header-premium { margin-bottom: 20px; border-bottom: 1px solid var(--r-border); padding-bottom: 16px; }
.section-header-premium h3 { font-size: 18px; font-weight: 900; color: var(--r-text-main); margin: 0; }

.form-section-premium { }
.section-title-premium { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; }
.section-title-premium .num { 
  font-size: 12px; font-weight: 900; color: var(--accent-color); 
  background: var(--accent-soft); width: 24px; height: 24px; 
  display: flex; align-items: center; justify-content: center; border-radius: 6px;
}
.section-title-premium h4 { font-size: 16px; font-weight: 800; color: var(--r-text-main); margin: 0; }

.r-form-row { display: grid; grid-template-columns: repeat(2, 1fr); gap: 16px; }
.mt-20 { margin-top: 16px; }
.mt-32 { margin-top: 24px; }

/* INPUT GROUPS */
.r-input-group { display: flex; flex-direction: column; gap: 6px; margin-bottom: 16px; }
.r-input-group.full { grid-column: span 2; }
.r-input-group label { font-size: 13px; font-weight: 700; color: var(--r-text-sub); }
.r-input-group input, .r-input-group select, .r-input-group textarea, .r-input-group input[type="date"] {
  padding: 12px 16px; border-radius: 12px; background: var(--r-main-bg); 
  border: 1px solid var(--r-border); color: var(--r-text-main); font-size: 14px; font-weight: 600;
  transition: 0.3s; width: 100%; box-sizing: border-box;
}
.r-input-group input:focus, .r-input-group select:focus, .r-input-group textarea:focus {
  border-color: var(--accent); outline: none; box-shadow: 0 0 0 4px var(--accent-soft);
}

.error-msg { font-size: 11px; color: #ef4444; font-weight: 700; margin-top: 4px; }
.has-error input { border-color: #ef4444 !important; }

/* SKILLS TAGGER */
.r-skills-tagger {
  background: var(--r-main-bg); border: 1px solid var(--r-border); 
  border-radius: 16px; padding: 12px;
}
.tags-container { display: flex; flex-wrap: wrap; gap: 8px; align-items: center; }
.r-skill-pill {
  display: flex; align-items: center; gap: 8px; padding: 6px 12px;
  background: var(--accent-soft); color: var(--accent);
  border-radius: 8px; font-size: 13px; font-weight: 700;
}
.r-skill-pill button { background: none; border: none; color: inherit; font-size: 16px; cursor: pointer; padding: 0; }
.tags-container input { 
  flex: 1; min-width: 150px; background: transparent !important; border: none !important; 
  padding: 8px !important; font-size: 13px !important; color: var(--r-text-main) !important; outline: none !important; box-shadow: none !important;
}

/* SALARY INPUTS */
.salary-inputs { display: flex; align-items: center; gap: 10px; }
.salary-inputs input { width: 100px; }
.salary-inputs .sep { color: var(--r-text-sub); font-weight: 800; }

/* CHECKBOX & SWITCH */
.r-checkbox { display: flex; align-items: center; gap: 10px; cursor: pointer; font-size: 13px; font-weight: 700; color: var(--r-text-sub); }
.r-checkbox input { width: 18px; height: 18px; border-radius: 4px; }

.form-config-list { display: flex; flex-direction: column; gap: 16px; margin-top: 24px; }
.config-item {
  display: flex; align-items: center; justify-content: space-between;
  padding: 24px; background: var(--r-main-bg); border: 1px solid var(--r-border);
  border-radius: 20px; transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
.config-item:hover { 
  border-color: var(--accent-color); 
  transform: translateY(-2px);
  box-shadow: 0 12px 24px rgba(0,0,0,0.03);
}
.config-item.is-off { 
  background: var(--r-card-bg);
  opacity: 0.7;
}
.config-item.is-off .f-icon {
  background: var(--r-border);
  color: var(--r-text-sub);
  box-shadow: none;
}
.config-item.is-locked .r-switch:first-child,
.r-switch.is-locked {
  opacity: 0.5;
  pointer-events: none;
}

.field-info { display: flex; align-items: center; gap: 20px; }
.f-icon { 
  width: 48px; height: 48px; background: var(--r-card-bg); border-radius: 14px; 
  display: flex; align-items: center; justify-content: center; color: var(--accent-color);
  box-shadow: 0 4px 12px var(--accent-soft);
}
.f-icon svg { width: 22px; height: 22px; }
.f-text { display: flex; flex-direction: column; gap: 4px; }
.f-text .f-name { font-size: 16px; font-weight: 800; color: var(--r-text-main); }
.f-text .f-desc { font-size: 13px; font-weight: 500; color: var(--r-text-sub); margin: 0; }

.field-actions { display: flex; gap: 32px; align-items: center; }
.r-switch { display: flex; align-items: center; gap: 12px; font-size: 12px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; cursor: pointer; letter-spacing: 0.5px; }
.r-switch input { display: none; }
.slider {
  width: 44px; height: 24px; background: var(--r-border); border-radius: 24px;
  position: relative; transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: 1.5px solid transparent;
}
.slider:before {
  content: ''; position: absolute; width: 16px; height: 16px; left: 3px; top: 2px;
  background: white; border-radius: 50%; transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 2px 4px rgba(0,0,0,0.2);
}
input:checked + .slider { 
  background: var(--accent-color); 
  border-color: var(--accent-soft);
  box-shadow: 0 0 12px var(--accent-soft); 
}
input:checked + .slider:before { transform: translateX(20px); }

/* SUCCESS SCREEN */
.success-screen { text-align: center; padding: 40px 0; }
.success-icon {
  width: 80px; height: 80px; background: #10b981; color: white; border-radius: 50%;
  display: flex; align-items: center; justify-content: center; margin: 0 auto 24px;
}
.success-icon svg { width: 40px; }
.success-screen h2 { font-size: 28px; font-weight: 900; color: var(--r-text-main); margin-bottom: 12px; }
.success-screen p { color: var(--r-text-sub); font-size: 16px; margin-bottom: 32px; }

.share-box { background: var(--r-main-bg); padding: 24px; border-radius: 20px; border: 1px solid var(--r-border); margin-bottom: 32px; text-align: left; }
.share-box label { display: block; font-size: 12px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; margin-bottom: 12px; }
.share-input { display: flex; gap: 10px; background: var(--r-card-bg); padding: 6px; border-radius: 12px; border: 1px solid var(--r-border); }
.share-input input { flex: 1; background: transparent; border: none; padding: 0 12px; color: var(--r-text-main); font-weight: 600; outline: none; }
.share-input button { background: var(--accent-color); color: white; border: none; padding: 8px 16px; border-radius: 8px; font-weight: 800; cursor: pointer; }

/* NAVIGATION */
.form-navigation { display: flex; gap: 16px; margin-top: 40px; }

/* PREVIEW SIDEBAR */
.preview-sidebar { position: sticky; top: 40px; }
.preview-card { padding: 32px; }
.mock-browser { background: #0d1117; border-radius: 16px; overflow: visible; border: 1px solid rgba(255,255,255,0.1); }
.browser-header { background: #161b22; padding: 10px 16px; }
.browser-header .dots { display: flex; gap: 6px; }
.browser-header .dots span { width: 8px; height: 8px; border-radius: 50%; background: #30363d; }

.mock-content { padding: 0; }
.mock-hero { height: 100px; display: flex; align-items: center; justify-content: center; position: relative; }
.mock-logo { width: 50px; height: 50px; background: rgba(255,255,255,0.2); backdrop-filter: blur(10px); color: white; display: flex; align-items: center; justify-content: center; border-radius: 12px; font-weight: 900; font-size: 20px; }

.mock-body { padding: 24px; text-align: center; }
.mock-badge { display: inline-block; font-size: 10px; font-weight: 900; color: var(--accent-color); background: var(--accent-soft); padding: 4px 10px; border-radius: 4px; margin-bottom: 12px; }
.mock-body h3 { font-size: 20px; font-weight: 900; color: white; margin: 0 0 8px; }
.mock-meta { font-size: 13px; color: #8b949e; margin-bottom: 16px; }
.mock-details { display: flex; justify-content: center; gap: 16px; margin-bottom: 20px; }
.m-item { font-size: 11px; color: #c9d1d9; font-weight: 600; }

.mock-skills { display: flex; flex-wrap: wrap; justify-content: center; gap: 6px; margin-bottom: 24px; }
.mock-skills span { font-size: 10px; background: rgba(255,255,255,0.05); color: #c9d1d9; padding: 4px 10px; border-radius: 4px; border: 1px solid rgba(255,255,255,0.1); }

.mock-btn { padding: 10px; border-radius: 8px; color: white; font-weight: 800; font-size: 14px; cursor: pointer; }

/* TRANSITIONS */
.step-slide-enter-active, .step-slide-leave-active { transition: 0.3s; }
.step-slide-enter-from { opacity: 0; transform: translateX(20px); }
.step-slide-leave-to { opacity: 0; transform: translateX(-20px); }

/* AI WEIGHTS STYLES */
.ai-weights-grid {
  display: flex;
  flex-direction: column;
  gap: 20px;
  background: var(--r-main-bg);
  padding: 24px;
  border-radius: 20px;
  border: 1px solid var(--r-border);
}
.weight-control { display: flex; flex-direction: column; gap: 8px; }
.weight-label-row { display: flex; justify-content: space-between; align-items: center; }
.weight-label-row label { font-size: 14px; font-weight: 700; color: var(--r-text-sub); }
.weight-val { font-size: 14px; font-weight: 900; color: var(--accent-color); background: var(--accent-soft); padding: 2px 10px; border-radius: 6px; }
.weight-slider {
  -webkit-appearance: none;
  width: 100%;
  height: 6px;
  border-radius: 5px;
  background: var(--r-border);
  outline: none;
  margin: 10px 0;
}
.weight-slider::-webkit-slider-thumb {
  -webkit-appearance: none;
  appearance: none;
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: var(--accent-color);
  cursor: pointer;
  border: 4px solid var(--r-card-bg);
  box-shadow: 0 4px 12px rgba(0,0,0,0.2);
  transition: 0.2s;
}
.weight-slider::-webkit-slider-thumb:hover { transform: scale(1.1); }
.weight-error { color: #ef4444; font-size: 12px; font-weight: 800; text-align: center; margin-top: 10px; }
.pulse { animation: pulseAnim 2s infinite; }
@keyframes pulseAnim { 0% { opacity: 0.6; } 50% { opacity: 1; } 100% { opacity: 0.6; } }
.section-hint { font-size: 13px; color: var(--r-text-sub); margin-bottom: 20px; opacity: 0.7; }

.text-danger { color: #ef4444 !important; }

.auto-reject-hint {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 14px;
  background: rgba(239, 68, 68, 0.06);
  border: 1px solid rgba(239, 68, 68, 0.15);
  border-radius: 10px;
  color: #dc2626;
  font-size: 12px;
  font-weight: 600;
  margin-top: 12px;
}

.reject-slider::-webkit-slider-thumb {
  background: #ef4444 !important;
}

@media (max-width: 1200px) { .creation-grid-expert { grid-template-columns: 1fr; } }
</style>
