<template>
  <div class="dashboard-layout recruiter-layout">
    <Sidebar 
      active-item="jobs" 
      :collapsible="true" 
      :collapsed="sidebarCollapsed" 
      @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" 
    />

    <main class="main-content recruiter-main stagger-reveal" :class="{ 'ml-collapsed': sidebarCollapsed }">
      <!-- Header -->
      <JobOfferHeader :isEdit="isEdit" :jobOfferTitle="form.title" />

      <!-- Stepper (Hide on success) -->
      <div class="stepper-wrap-lux" v-if="!showSuccessScreen">
        <JobOfferStepper 
          v-model:currentStep="currentStep" 
          :steps="steps" 
        />
      </div>

      <div class="page-body">
        <div class="creation-grid-expert">
          <div class="form-main-container">
            <div class="r-card form-inner-card-lux anim-reveal-up" style="animation-delay: 0.2s">
              
              <transition :name="transitionName" mode="out-in">
                <!-- SUCCESS SCREEN -->
                <JobOfferSuccess 
                  v-if="showSuccessScreen"
                  :publicLink="getPublicLink()"
                  @copy-link="copyLink"
                  @back="router.push('/jobs')"
                />

                <!-- FORM STEPS -->
                <div v-else class="form-step-wrapper">
                  <component 
                    :is="currentStepComponent"
                    v-model:form="form"
                    v-model:newSkill="newSkill"
                    :errors="errors"
                    :departmentOptions="departmentOptions"
                    :typeOptions="typeOptions"
                    :remoteOptions="remoteOptions"
                    :formFields="formFields"
                    :totalWeight="totalWeight"
                    :isGenerating="isGenerating"
                    @validate="validateField"
                    @add-skill="addSkill"
                    @remove-skill="removeSkill"
                    @generate-ai-description="generateAIDescription"
                  />
                  
                  <!-- Navigation Footer -->
                  <div class="form-navigation-lux">
                    <button 
                      v-if="currentStep > 0" 
                      class="btn-premium btn-secondary" 
                      @click="goToPrevStep"
                    >
                      <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/></svg>
                      Précédent
                    </button>
                    <div style="flex:1"></div>
                    <button 
                      v-if="currentStep < 2" 
                      class="btn-premium btn-accent" 
                      @click="handleNextStep"
                    >
                      Étape Suivante
                      <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
                    </button>
                    <button 
                      v-else 
                      class="btn-premium btn-accent btn-glow" 
                      @click="publishOffer" 
                      :disabled="isLoading || (totalWeight !== 100)"
                    >
                      <span v-if="isLoading" class="spinner-mini"></span>
                      <span v-else>{{ isEdit ? 'Mettre à jour' : 'Diffuser l\'Offre' }}</span>
                      <svg v-if="!isLoading" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5"><path d="m22 2-7 20-4-9-9-4Z"/><path d="M22 2 11 13"/></svg>
                    </button>
                  </div>
                </div>
              </transition>

            </div>
          </div>
        </div>
      </div>
      <ToastContainer />
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '@/api/axios'

// Common Components
import Sidebar from '@/components/layout/Sidebar.vue'
import ToastContainer from '@/components/common/ToastContainer.vue'

// Job Offer Components
import JobOfferHeader from '@/components/recruiter/job-offers/JobOfferHeader.vue'
import JobOfferStepper from '@/components/recruiter/job-offers/JobOfferStepper.vue'
import JobOfferStep1 from '@/components/recruiter/job-offers/JobOfferStep1.vue'
import JobOfferStep2 from '@/components/recruiter/job-offers/JobOfferStep2.vue'
import JobOfferStep3 from '@/components/recruiter/job-offers/JobOfferStep3.vue'
import JobOfferPreview from '@/components/recruiter/job-offers/JobOfferPreview.vue'
import JobOfferSuccess from '@/components/recruiter/job-offers/JobOfferSuccess.vue'

// Stores
import { useAuthStore } from '@/stores/authStore'
import { useToastStore } from '@/stores/toastStore'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()

// --- State ---
const id = route.params.id
const isEdit = !!id
const sidebarCollapsed = ref(false)
const currentStep = ref(0)
const isLoading = ref(false)
const isGenerating = ref(false)
const newSkill = ref('')
const showSuccessScreen = ref(false)
const shareToken = ref('')
const transitionName = ref('step-slide')

const form = ref({
  title: '',
  department: 'tech',
  otherDepartment: '',
  type: 'FullTime',
  location: '',
  description: '',
  skills: [],
  remotePolicy: 'hybrid',
  experienceLevel: 'all',
  deadline: '',
  salaryMin: null,
  salaryMax: null,
  salaryConfidential: false,
  status: 'Published',
  weightExperience: 33,
  weightEducation: 33,
  weightSkills: 34,
  autoRejectThreshold: 0
})

const formFields = ref([
  { id: 'fullname', name: 'Prénom & Nom', desc: 'Identité complète du candidat', enabled: true, required: true, canDisable: false },
  { id: 'email', name: 'Email professionnel', desc: 'Adresse de contact principale', enabled: true, required: true, canDisable: false },
  { id: 'phone', name: 'Numéro de téléphone', desc: 'Pour les contacts directs', enabled: true, required: false, canDisable: true },
  { id: 'resume', name: 'Curriculum Vitae (CV)', desc: 'Document PDF ou Word', enabled: true, required: true, canDisable: true },
  { id: 'coverLetter', name: 'Lettre de motivation', desc: 'Argumentation du candidat', enabled: true, required: false, canDisable: true },
  { id: 'linkedin', name: 'Profil LinkedIn', desc: 'Lien vers le réseau social', enabled: true, required: false, canDisable: true },
  { id: 'portfolio', name: 'Portfolio / Site web', desc: 'Lien vers les réalisations', enabled: true, required: false, canDisable: true }
])

const errors = ref({
  title: '',
  location: '',
  description: '',
  deadline: ''
})

// --- Computed ---
const totalWeight = computed(() => {
  return parseInt(form.value.weightExperience || 0) + 
         parseInt(form.value.weightEducation || 0) + 
         parseInt(form.value.weightSkills || 0)
})

const steps = [
  { id: 'details', label: 'Détails' },
  { id: 'content', label: 'Profil' },
  { id: 'publish', label: 'Diffusion' }
]

const currentStepComponent = computed(() => {
  if (currentStep.value === 0) return JobOfferStep1
  if (currentStep.value === 1) return JobOfferStep2
  return JobOfferStep3
})

const departmentOptions = [
  { 
    value: 'admin', label: 'Administration & Services Publics',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 21h18M9 8h1m-1 4h1m-1 4h1m5-8h1m-1 4h1m-1 4h1M5 21V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v16"/></svg>'
  },
  { 
    value: 'agriculture', label: 'Agriculture & Agroalimentaire',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg>'
  },
  { 
    value: 'architecture', label: 'Architecture & BTP',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>'
  },
  { 
    value: 'art', label: 'Art, Design & Culture',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><path d="M14.31 8l5.74 9.94M9.69 8h11.48M7.38 12l5.74-9.94M9.69 16L3.95 6.06M14.31 16H2.83M16.62 12l-5.74 9.94"/></svg>'
  },
  { 
    value: 'commercial', label: 'Commercial & Vente',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"/><line x1="3" y1="6" x2="21" y2="6"/><path d="M16 10a4 4 0 0 1-8 0"/></svg>'
  },
  { 
    value: 'finance', label: 'Comptabilité & Finance',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>'
  },
  { 
    value: 'data', label: 'Data & Intelligence Artificielle',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/><polyline points="3.27 6.96 12 12.01 20.73 6.96"/><line x1="12" y1="22.08" x2="12" y2="12"/></svg>'
  },
  { 
    value: 'education', label: 'Éducation & Formation',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M22 10v6M2 10l10-5 10 5-10 5z"/><path d="M6 12v5c3 3 9 3 12 0v-5"/></svg>'
  },
  { 
    value: 'energy', label: 'Énergie & Environnement',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M11 2a2 2 0 0 0-2 2v5H4a2 2 0 0 0-2 2v2c0 1.1.9 2 2 2h5v5a2 2 0 0 0 2 2h2a2 2 0 0 0 2-2v-5h5a2 2 0 0 0 2-2v-2a2 2 0 0 0-2-2h-5V4a2 2 0 0 0-2-2h-2z"/></svg>'
  },
  { 
    value: 'health', label: 'Santé & Social',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M22 12h-4l-3 9L9 3l-3 9H2"/></svg>'
  },
  { 
    value: 'hr', label: 'Ressources Humaines',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>'
  },
  { 
    value: 'legal', label: 'Juridique & Fiscalité',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg>'
  },
  { 
    value: 'logistics', label: 'Logistique & Transport',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="1" y="3" width="15" height="13"/><polygon points="16 8 20 8 23 11 23 16 16 16 16 8"/><circle cx="5.5" cy="18.5" r="2.5"/><circle cx="18.5" cy="18.5" r="2.5"/></svg>'
  },
  { 
    value: 'management', label: 'Management & Direction',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><polyline points="16 11 18 13 22 9"/></svg>'
  },
  { 
    value: 'marketing', label: 'Marketing & Communication',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M11 3.05c-3.15.54-5.64 3.03-6.18 6.18h6.18V3.05zm2 0v6.18h6.18c-.54-3.15-3.03-5.64-6.18-6.18zm-2 8.18H4.87c.54 3.15 3.03 5.64 6.18 6.18V11.23zm2 0v6.18c3.15-.54 5.64-3.03 6.18-6.18H13z"/></svg>'
  },
  { 
    value: 'production', label: 'Industrie & Production',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z"/></svg>'
  },
  { 
    value: 'security', label: 'Sécurité & Défense',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>'
  },
  { 
    value: 'support', label: 'Service Client & Support',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>'
  },
  { 
    value: 'tech', label: 'Technologie & Informatique',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="16 18 22 12 16 6"/><polyline points="8 6 2 12 8 18"/></svg>'
  },
  { 
    value: 'tourism', label: 'Tourisme, Hôtellerie & Restauration',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>'
  },
  { 
    value: 'other', label: 'Autre (Préciser...)',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>'
  }
]

const typeOptions = [
  { 
    value: 'FullTime', label: 'CDI (Contrat à Durée Indéterminée)',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><polyline points="16 11 18 13 22 9"/></svg>'
  },
  { 
    value: 'Contract', label: 'CDD (Contrat à Durée Déterminée)',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>'
  },
  { 
    value: 'PartTime', label: 'Temps Partiel',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>'
  },
  { 
    value: 'Internship', label: 'Stage Conventionné',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M22 10v6M2 10l10-5 10 5-10 5z"/><path d="M6 12v5c3 3 9 3 12 0v-5"/></svg>'
  },
  { 
    value: 'Freelance', label: 'Freelance / Indépendant',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>'
  }
]

const remoteOptions = [
  { 
    value: 'office', 
    label: 'Présence sur site', 
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 21h18M9 8h1m-1 4h1m-1 4h1m5-8h1m-1 4h1m-1 4h1M5 21V5a2 2 0 0 1 2-2h10a2 2 0 0 1 2 2v16"/></svg>' 
  },
  { 
    value: 'hybrid', 
    label: 'Hybride (Présence flexible)', 
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>' 
  },
  { 
    value: 'remote', 
    label: 'Télétravail complet (Full Remote)', 
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><path d="M2 12h20M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/></svg>' 
  },
  { 
    value: 'flexible', 
    label: 'Mode ultra-flexible', 
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/></svg>' 
  }
]

// --- Methods ---
const normalizeStatusForForm = (status) => {
  const raw = `${status || ''}`.toLowerCase()
  if (raw === 'draft' || raw === '0' || raw === 'brouillon') return 'Draft'
  if (raw === 'published' || raw === 'active' || raw === '1' || raw === 'public') return 'Published'
  return 'Draft'
}

const fetchOffer = async () => {
  if (!isEdit) return
  isLoading.value = true
  try {
    const res = await api.get(`/JobOffer/${id}`)
    const data = res.data
    
    let mappedRemote = data.remotePolicy?.toLowerCase() || 'hybrid'
    if (mappedRemote === 'onsite') mappedRemote = 'office'

    form.value = {
      ...form.value,
      title: data.title,
      department: data.department || 'tech',
      type: data.type || 'FullTime',
      location: data.location || '',
      description: data.description || '',
      skills: data.skills || [],
      remotePolicy: mappedRemote,
      experienceLevel: data.experienceLevel?.toLowerCase() || 'intermediate',
      deadline: data.deadline ? data.deadline.split('T')[0] : '',
      salaryMin: data.salaryRange?.split('-')[0] || null,
      salaryMax: data.salaryRange?.split('-')[1] || null,
      salaryConfidential: data.salaryConfidential || false,
      status: normalizeStatusForForm(data.status || data.visibility),
      weightExperience: data.weightExperience ?? 33,
      weightEducation: data.weightEducation ?? 33,
      weightSkills: data.weightSkills ?? 34,
      autoRejectThreshold: data.autoRejectThreshold ?? 0
    }
    
    if (data.formConfig) {
      const cfg = data.formConfig
      formFields.value.forEach(f => {
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
    toastStore.show('Erreur lors du chargement', 'error')
  } finally {
    isLoading.value = false
  }
}

const validateField = (field) => {
  if (field === 'title') {
    if (!form.value.title) errors.value.title = 'Requis'
    else if (form.value.title.length < 5) errors.value.title = '5 caractères min'
    else errors.value.title = ''
  }
  if (field === 'location') {
    if (!form.value.location) errors.value.location = 'Requis'
    else errors.value.location = ''
  }
  if (field === 'description') {
    if (!form.value.description) errors.value.description = 'Requis'
    else errors.value.description = ''
  }
  if (field === 'deadline') {
    if (!form.value.deadline) errors.value.deadline = 'Requis'
    else errors.value.deadline = ''
  }
}

const addSkill = () => {
  const s = newSkill.value.trim()
  if (s && !form.value.skills.includes(s)) {
    form.value.skills.push(s)
    newSkill.value = ''
  }
}

const removeSkill = (idx) => {
  form.value.skills.splice(idx, 1)
}

const generateAIDescription = async (keywords) => {
  if (!form.value.title) {
    toastStore.show('Veuillez renseigner un titre de poste', 'warning')
    return
  }

  isGenerating.value = true
  try {
    const response = await api.post('/Recruiter/job-offers/ai/generate-description', {
      jobTitle: form.value.title,
      keywords: keywords
    })
    
    if (response.data && response.data.description) {
      form.value.description = response.data.description
      toastStore.show('Description générée par IA !', 'success')
    }
  } catch (err) {
    toastStore.show('Échec de la génération par IA', 'error')
  } finally {
    isGenerating.value = false
  }
}

const handleNextStep = () => {
  if (currentStep.value === 0) {
    validateField('title')
    validateField('location')
    if (errors.value.title || errors.value.location) {
       scrollToFirstError()
       return
    }
  } else if (currentStep.value === 1) {
    validateField('description')
    if (errors.value.description) {
       scrollToFirstError()
       return
    }
  }
  transitionName.value = 'step-slide'
  currentStep.value++
}

const goToPrevStep = () => {
  transitionName.value = 'step-slide-back'
  currentStep.value--
}

const publishOffer = async () => {
  if (isLoading.value) return

  validateField('deadline')
  if (errors.value.deadline) {
    scrollToFirstError()
    return
  }

  if (totalWeight.value !== 100) {
    toastStore.show('Le total des poids IA doit être de 100%', 'error')
    return
  }

  isLoading.value = true
  try {
    const payload = { ...form.value }
    
    // Enum mappings
    const rp = payload.remotePolicy?.toLowerCase()
    if (rp === 'office') payload.remotePolicy = 'OnSite'
    else if (rp === 'remote') payload.remotePolicy = 'Remote'
    else payload.remotePolicy = 'Hybrid'

    const exp = payload.experienceLevel?.toLowerCase()
    if (exp === 'junior') payload.experienceLevel = 'Junior'
    else if (exp === 'senior') payload.experienceLevel = 'Senior'
    else if (exp === 'expert') payload.experienceLevel = 'Expert'
    else payload.experienceLevel = 'Intermediate'


    if (payload.department === 'other') {
      payload.department = payload.otherDepartment || 'Autre'
    }
    delete payload.otherDepartment
    delete payload.salaryCurrency

    payload.deadline = payload.deadline || null
    
    const minVal = parseInt(payload.salaryMin)
    const maxVal = parseInt(payload.salaryMax)
    payload.salaryMin = !isNaN(minVal) ? minVal : null
    payload.salaryMax = !isNaN(maxVal) ? maxVal : null

    payload.weightExperience = parseInt(payload.weightExperience) || 0
    payload.weightEducation = parseInt(payload.weightEducation) || 0
    payload.weightSkills = parseInt(payload.weightSkills) || 0

    const threshold = parseInt(payload.autoRejectThreshold)
    payload.autoRejectThreshold = (!isNaN(threshold) && threshold > 0) ? threshold : 0

    const formConfig = {}
    formFields.value.forEach(f => {
      let key = 'Require' + f.id.charAt(0).toUpperCase() + f.id.slice(1)
      if (f.id === 'fullname') key = 'RequireFullName'
      if (f.id === 'resume') key = 'RequireCV'
      if (f.id === 'coverLetter') key = 'RequireCoverLetter'
      if (f.id === 'linkedin') key = 'RequireLinkedIn'
      if (f.id === 'portfolio') key = 'RequirePortfolio'
      
      formConfig[key] = f.enabled
    })
    payload.FormConfig = formConfig
    
    // Assign visibility before deleting payload.status
    payload.visibility = payload.status
    delete payload.status
    
    let response
    if (isEdit) {
      response = await api.put(`/JobOffer/${id}`, payload)
    } else {
      response = await api.post('/JobOffer', payload)
    }
    
    if (payload.status === 'Published' && !isEdit) {
      shareToken.value = response.data.shareToken || ''
      showSuccessScreen.value = true
      toastStore.show('Offre publiée avec succès !', 'success')
    } else {
      toastStore.show(isEdit ? 'Mise à jour réussie' : 'Brouillon enregistré', 'success')
      setTimeout(() => {
        router.push('/jobs')
      }, 1500)
    }
  } catch (err) {
    console.error('Erreur publication:', err)
    const serverMessage = err.response?.data ? JSON.stringify(err.response.data) : err.message
    alert("Détail de l'erreur serveur : " + serverMessage)
    toastStore.show('Erreur lors de la publication', 'error')
  } finally {
    isLoading.value = false
  }
}

const getPublicLink = () => {
  return `${window.location.origin}/shared-job/${shareToken.value}`
}

const copyLink = () => {
  navigator.clipboard.writeText(getPublicLink()).then(() => {
    toastStore.show('Lien copié dans le presse-papier', 'success')
  })
}

const scrollToFirstError = () => {
  nextTick(() => {
    const errorEl = document.querySelector('.has-error')
    if (errorEl) {
      errorEl.scrollIntoView({ behavior: 'smooth', block: 'center' })
    }
  })
}

onMounted(() => {
  fetchOffer()
})
</script>

<style scoped>
.recruiter-layout {
  background: var(--r-body-bg);
  min-height: 100vh;
}

.recruiter-main {
  padding: 40px;
}

.stepper-wrap-lux {
  margin-bottom: 16px;
  max-width: 800px;
  margin-left: auto;
  margin-right: auto;
}

.creation-grid-expert {
  display: flex;
  justify-content: center;
  align-items: flex-start;
}

.form-main-container {
  width: 100%;
  max-width: 800px;
}

.form-inner-card-lux {
  padding: 32px;
  background: var(--r-surface);
  border: 1px solid var(--r-border);
  border-radius: 40px;
  box-shadow: 
    0 40px 120px -20px rgba(99, 102, 241, 0.08),
    0 0 0 1px rgba(99, 102, 241, 0.05),
    0 10px 40px -10px rgba(99, 102, 241, 0.05);
  min-height: 600px;
  position: relative;
  overflow: hidden;
  width: 100%;
}

:deep(.dark-mode) .form-inner-card-lux,
.dark-mode .form-inner-card-lux {
  background: rgba(15, 23, 42, 0.95);
  border-color: rgba(255, 255, 255, 0.08);
  box-shadow: 0 40px 120px -20px rgba(0, 0, 0, 0.4);
}

.form-navigation-lux {
  margin-top: 64px;
  padding-top: 32px;
  border-top: 1px solid var(--r-border);
  display: flex;
  align-items: center;
  gap: 20px;
}

.form-step-wrapper {
  zoom: 0.67;
  --step-text-size: 1.28rem;
  transform-origin: top center;
}

.form-step-wrapper :deep(*) {
  font-size: var(--step-text-size) !important;
}

.flex-spacer { flex: 1; }

.btn-premium.btn-accent {
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  color: white !important;
  padding: 14px 32px;
  border-radius: 18px;
  border: none;
  font-weight: 950;
  text-transform: uppercase;
  letter-spacing: 1px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 12px;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 8px 25px -5px rgba(14, 165, 233, 0.5), 0 3px 0 #0891b2;
  position: relative;
  overflow: hidden;
}

.btn-premium.btn-accent::after {
  content: ''; position: absolute; top: 0; left: -100%;
  width: 100%; height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.25), transparent);
  transition: 0.6s;
}

.btn-premium.btn-accent:hover::after { left: 100%; }

.btn-premium.btn-accent:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 30px -5px rgba(14, 165, 233, 0.6), 0 1px 0 #0891b2;
  filter: brightness(1.05);
}

.btn-premium.btn-accent:active {
  transform: translateY(2px);
  box-shadow: 0 4px 10px rgba(0,0,0,0.15);
}

.btn-secondary {
  background: var(--r-surface);
  border: 1.5px solid var(--r-border);
  color: var(--r-text-main);
  padding: 14px 28px;
  border-radius: 18px;
  font-weight: 800;
  cursor: pointer;
  transition: 0.3s;
}
.btn-secondary:hover {
  background: var(--bg-hover);
  border-color: var(--accent);
  color: var(--accent);
}



/* Transitions */
.step-slide-enter-active, .step-slide-leave-active,
.step-slide-back-enter-active, .step-slide-back-leave-active {
  transition: all 0.5s cubic-bezier(0.4, 0, 0.2, 1);
}

.step-slide-enter-from { opacity: 0; transform: translateX(60px) scale(0.98); }
.step-slide-leave-to { opacity: 0; transform: translateX(-60px) scale(0.98); }

.step-slide-back-enter-from { opacity: 0; transform: translateX(-60px) scale(0.98); }
.step-slide-back-leave-to { opacity: 0; transform: translateX(60px) scale(0.98); }

.spinner-mini {
  width: 18px; height: 18px; border: 3px solid rgba(255,255,255,0.3);
  border-top-color: white; border-radius: 50%; animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

@media (max-width: 1200px) {
  .recruiter-main { padding: 20px; }
}
</style>
