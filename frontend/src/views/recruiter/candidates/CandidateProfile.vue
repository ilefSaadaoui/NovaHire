<template>
  <div class="dashboard-layout recruiter-layout">
    <Sidebar 
      active-item="pipeline" 
      :collapsible="true" 
      :collapsed="sidebarCollapsed" 
      @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" 
    />

    <main class="main-content recruiter-main stagger-reveal" :class="{ 'ml-collapsed': sidebarCollapsed }">
      <!-- Header Component -->
      <CandidateHeader 
        :candidate="candidate" 
        :accentColor="accentColor" 
        :accentDark="accentDark"
        :analyzing="isAnalyzing"
        @back="router.push({ path: '/applications', query: { jobOfferId: candidate.jobOfferId } })"
        @view-cv="viewCV"
        @schedule-interview="scheduleInterview"
        @reanalyze="reanalyzeCandidate"
        @generate-offer="showOfferModal = true"
      />

      <div class="page-body">
        <div class="profile-grid-container">
          <!-- LEFT SIDE: CANDIDATE IDENTITY & IA -->
          <div class="side-col">
            <CandidateIdentityCard 
              :candidate="candidate" 
              :currentStage="currentStage" 
              :avatarStyle="avatarStyle"
              @update-stage="updateStage"
            />

            <CandidateAIInsights 
              :candidate="candidate" 
              :criteria="criteria" 
            />

            <CollaborativeRating 
              :candidate="candidate" 
              :applicationId="route.params.id" 
              @update="fetchCandidate"
            />
          </div>

          <!-- RIGHT SIDE: DETAILS & TIMELINE -->
          <div class="main-col">
            <CandidateTabsNav 
              v-model="activeTab" 
              :tabs="tabs" 
            />

            <div class="tab-content-panel">
              <transition name="fade-slide" mode="out-in">
                <component
                  :is="currentTabComponent"
                  :key="activeTab"
                  :candidate="candidate"
                  :timeline="timeline"
                  :accentColor="accentColor"
                  :onSaveExtractedData="saveExtractedData"
                  :applicationId="route.params.id"
                />
              </transition>
            </div>

            <!-- Note Input Component -->
            <CandidateNoteInput 
              v-model="recruiterNotes" 
              :loading="isSavingNote"
              @post="postComment"
            />
          </div>
        </div>
      </div>
    </main>

    <!-- Interview Modal Component -->
    <InterviewModal 
      v-if="showInterviewModal"
      :form="interviewForm"
      @update:form="val => interviewForm = val"
      :loading="sendingInterview"
      @close="showInterviewModal = false"
      @save="confirmScheduleInterview"
      @generate-template="generateEmailTemplate"
    />

    <OfferModal
      v-if="showOfferModal"
      :loading="isGeneratingOffer"
      @close="showOfferModal = false"
      @generate="onGenerateOffer"
      @confirm="onConfirmOffer"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch, markRaw } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '@/api/axios'
import Sidebar from '@/components/layout/Sidebar.vue'

import { useAuthStore } from '@/stores/authStore'
import { useToastStore } from '@/stores/toastStore'
import { useModalStore } from '@/stores/modalStore'
import { useRecruitmentStore } from '@/stores/recruitmentStore'

// Components
import CandidateHeader from '@/components/recruiter/layout/CandidateHeader.vue'
import CandidateIdentityCard from '@/components/recruiter/layout/CandidateIdentityCard.vue'
import CandidateAIInsights from '@/components/recruiter/layout/CandidateAIInsights.vue'
import CandidateTabsNav from '@/components/recruiter/layout/CandidateTabsNav.vue'
import CandidateNoteInput from '@/components/recruiter/layout/CandidateNoteInput.vue'
import CollaborativeRating from '@/components/recruiter/CollaborativeRating.vue'

// Tab Components
import CandidateCVDetails from '@/components/recruiter/tabs/CandidateCVDetails.vue'
import CandidateMotivation from '@/components/recruiter/tabs/CandidateMotivation.vue'
import CandidateHistory from '@/components/recruiter/tabs/CandidateHistory.vue'
import CandidateDiscussion from '@/components/recruiter/tabs/CandidateDiscussion.vue'
import CandidateInterviewGuide from '@/components/recruiter/tabs/CandidateInterviewGuide.vue'
import CandidateQuizDetails from '@/components/recruiter/tabs/CandidateQuizDetails.vue'

// Modals
import InterviewModal from '@/components/recruiter/modals/InterviewModal.vue'
import OfferModal from '@/components/recruiter/modals/OfferModal.vue'

import { useI18n } from 'vue-i18n'

const { t } = useI18n()

import { 
  FileText, X, Calendar, Sparkles, Loader2, Send, Copy, 
  User, MessageSquare, History, Wand2, Star, ClipboardList 
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()
const modalStore = useModalStore()
const recruitmentStore = useRecruitmentStore()

// --- UI State ---
const sidebarCollapsed = ref(false)
const activeTab = ref('cv')
const recruiterNotes = ref('')
const currentStage = ref('submitted')
const isSavingNote = ref(false)
const showInterviewModal = ref(false)
const sendingInterview = ref(false)
const isAnalyzing = ref(false)
const showOfferModal = ref(false)
const isGeneratingOffer = ref(false)

const storedRole = (authStore.userRole || '').toLowerCase()
const isCompanyOwner = storedRole === 'company' || storedRole === 'admin'
const accentColor = authStore.themeColors?.accent || (isCompanyOwner ? '#00A7E1' : '#F7C902')
const accentDark = authStore.themeColors?.accentDark || (isCompanyOwner ? '#0077B6' : '#D4A50E')

const tabs = [
  { id: 'cv', label: 'Extraits du CV', icon: User },
  { id: 'lettre', label: 'Motivation', icon: MessageSquare },
  { id: 'quiz', label: 'Quiz IA', icon: ClipboardList },
  { id: 'interview_guide', label: 'Guide d\'Entretien', icon: Wand2 },
  { id: 'timeline', label: 'Historique', icon: History },
  { id: 'comments', label: 'Commentaires', icon: Star }
]

const candidate = ref({
  fullName: 'Chargement...',
  initials: '..',
  jobTitle: '',
  jobOfferId: '',
  location: '',
  email: '',
  phone: '',
  overallScore: 0,
  aiSummary: '',
  experiences: [],
  education: [],
  skillGroups: [],
  coverLetter: '',
  comments: []
})

const criteria = ref([])
const timeline = ref([])
const interviewForm = ref({ date: '', time: '', type: 'visio', subject: '', message: '' })

// --- Computed ---
const avatarStyle = computed(() => ({
  background: `linear-gradient(135deg, ${accentColor}, ${accentDark})`
}))

const currentTabComponent = computed(() => {
  const map = {
    cv: markRaw(CandidateCVDetails),
    lettre: markRaw(CandidateMotivation),
    timeline: markRaw(CandidateHistory),
    interview_guide: markRaw(CandidateInterviewGuide),
    quiz: markRaw(CandidateQuizDetails),
    comments: markRaw(CandidateDiscussion)
  }
  return map[activeTab.value] || markRaw(CandidateCVDetails)
})

// --- Methods ---
const fetchCandidate = async () => {
  const id = route.params.id
  if (!id) return
  
  try {
    const res = await api.get(`/recruiter/applications/${id}`)
    candidate.value = res.data
    criteria.value = res.data.criteria
    currentStage.value = res.data.stage || 'submitted'
    
    // Use timeline from API if available, otherwise fallback
    if (res.data.timeline && res.data.timeline.length > 0) {
      timeline.value = res.data.timeline
    } else {
      timeline.value = [
        { id: 1, title: 'Candidature Reçue', time: 'Récemment', color: '#94a3b8', desc: 'Candidature enregistrée dans le système.', icon: 'submit' }
      ]
    }
  } catch (err) {
    console.error('Erreur chargement candidat:', err)
    toastStore.show('Erreur lors du chargement des données', 'error')
  }
}

const updateStage = async (stage) => {
  const id = route.params.id
  try {
    await api.patch(`/recruiter/applications/${id}/stage`, { stage })
    currentStage.value = stage
    recruitmentStore.updateApplicationStage(id, stage)
    toastStore.show('Statut mis à jour avec succès', 'success')
  } catch (err) {
    console.error('Erreur mise à jour statut:', err)
  }
}

const postComment = async () => {
  if (!recruiterNotes.value.trim()) return
  const id = route.params.id
  isSavingNote.value = true
  try {
    const res = await api.post(`/recruiter/applications/${id}/comments`, { content: recruiterNotes.value })
    candidate.value.comments.unshift(res.data)
    recruiterNotes.value = ''
    toastStore.show('Commentaire ajouté', 'success')
  } catch (err) {
    console.error('Erreur ajout commentaire:', err)
    toastStore.show('Erreur lors de l\'ajout du commentaire', 'error')
  } finally {
    isSavingNote.value = false
  }
}

const scheduleInterview = () => {
  interviewForm.value = { date: '', time: '', type: 'visio', subject: '', message: '' }
  showInterviewModal.value = true
  setTimeout(() => generateEmailTemplate(), 0)
}

watch(
  () => [interviewForm.value.date, interviewForm.value.time, interviewForm.value.type],
  () => {
    generateEmailTemplate()
  }
)

const parseDateInput = (value) => {
  if (!value) return null
  const normalized = String(value).trim()
  const match = normalized.match(/^(\d{1,2})[\/\-.](\d{1,2})[\/\-.](\d{2,4})$/)
  if (match) {
    const day = Number(match[1])
    const month = Number(match[2])
    let year = Number(match[3])
    if (year < 100) year += 2000
    const date = new Date(year, month - 1, day)
    if (date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day) {
      return date
    }
  }
  const parsed = new Date(normalized)
  return isNaN(parsed.getTime()) ? null : parsed
}

const formatLocalizedDate = (value) => {
  const date = parseDateInput(value)
  return date ? date.toLocaleDateString('fr-FR', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) : value
}

const generateEmailTemplate = () => {
  const name = candidate.value?.fullName || 'Candidat'
  const job = candidate.value?.jobTitle || 'le poste'
  const typeLabels = { visio: 'en visioconférence', phone: 'par téléphone', onsite: 'dans nos locaux' }
  const typeLabel = typeLabels[interviewForm.value.type] || 'en visioconférence'

  interviewForm.value.subject = `Invitation à un entretien pour le poste : ${job}`

  // Format date/time if available
  let dateTimeInfo = ''
  if (interviewForm.value.date || interviewForm.value.time) {
    const datePart = interviewForm.value.date
      ? formatLocalizedDate(interviewForm.value.date)
      : '[date à confirmer]'
    const timePart = interviewForm.value.time || '[heure à confirmer]'
    dateTimeInfo = `\n\nDate : ${datePart}\nHeure : ${timePart}`
  }

  let details = ''
  if (interviewForm.value.type === 'visio') {
    details = '\n\nVoici le lien pour rejoindre la réunion :\n[Insérez votre lien Google Meet / Zoom / Teams ici]'
  } else if (interviewForm.value.type === 'phone') {
    details = '\n\nNous vous contacterons au numéro que vous nous avez communiqué. Merci de vous assurer d\'être disponible à l\'heure indiquée.'
  } else {
    details = '\n\nAdresse :\n[Insérez l\'adresse complète de vos bureaux ici]\n\nMerci de vous présenter à l\'accueil 10 minutes avant l\'heure prévue.'
  }

  interviewForm.value.message = `Bonjour ${name},\n\nSuite à l'examen de votre candidature pour le poste de ${job}, nous avons le plaisir de vous informer que votre profil a retenu toute notre attention.\n\nNous souhaiterions vous rencontrer ${typeLabel} afin d'échanger sur votre parcours et vos motivations.${dateTimeInfo}${details}\n\nMerci de confirmer votre disponibilité en répondant à cet e-mail.\n\nCordialement`
}

const confirmScheduleInterview = async () => {
  sendingInterview.value = true
  const id = route.params.id
  const payload = {
    date: interviewForm.value.date,
    time: interviewForm.value.time,
    type: interviewForm.value.type,
    subject: interviewForm.value.subject,
    message: interviewForm.value.message
  }
  console.debug('Interview request payload:', payload)
  try {
    await api.post(`/recruiter/applications/${id}/interviews`, payload)
    timeline.value.unshift({
      id: Date.now(),
      title: 'Entretien Planifié',
      time: `${interviewForm.value.date} à ${interviewForm.value.time}`,
      color: accentColor,
      desc: interviewForm.value.type === 'visio' ? 'Entretien en visio.' : 'Entretien programmé.'
    })
    showInterviewModal.value = false
    currentStage.value = 'interview'
    toastStore.show('Invitation d\'entretien envoyée avec succès !', 'success')
  } catch (err) {
    const response = err?.response
    const data = response?.data
    const message = data?.message || (typeof data === 'string' ? data : JSON.stringify(data)) || err?.message || 'Erreur lors de l\'envoi de l\'invitation.'
    console.error('Erreur planification entretien:', { status: response?.status, data, message, error: err })
    toastStore.show(message, 'error')
  } finally {
    sendingInterview.value = false
  }
}

const viewCV = () => {
  if (candidate.value && candidate.value.resumeUrl) {
    if (candidate.value.resumeUrl.includes('placeholder')) {
      toastStore.show('Ceci est un candidat de test. Aucun vrai CV n\'a été fourni.', 'warning')
    } else {
      window.open(candidate.value.resumeUrl, '_blank')
    }
  } else {
    toastStore.show('Le CV n\'est pas disponible pour ce candidat.', 'warning')
  }
}

const statusToStageMap = {
  0: 'submitted',
  1: 'underreview',
  2: 'shortlisted',
  3: 'interview',
  4: 'rejected',
  5: 'accepted',
  6: 'interviewed',
  7: 'offersent'
}

const reanalyzeCandidate = async () => {
  const id = route.params.id
  if (!id) return
  isAnalyzing.value = true
  try {
    const res = await api.post(`/recruiter/applications/${id}/analyze`, {}, { timeout: 600000 })

    // Update stage in UI to match what the backend decided
    const newStage = statusToStageMap[res.data.status] || currentStage.value
    currentStage.value = newStage
    recruitmentStore.updateApplicationStage(id, res.data.status)

    if (newStage === 'rejected') {
      toastStore.show(
        `Candidat automatiquement rejeté — score IA ${res.data.score}% inférieur au seuil configuré.`,
        'error'
      )
    } else {
      toastStore.show(`Analyse IA terminée — Score: ${res.data.score}%`, 'success')
    }

    // Refresh profile data to show updated AI results
    await fetchCandidate()
  } catch (err) {
    console.error('Erreur réanalyse IA:', err)
    const msg = err.response?.data?.message || 'Erreur lors de l\'analyse par l\'IA.'
    toastStore.show(msg, 'error')
  } finally {
    isAnalyzing.value = false
  }
}

const saveExtractedData = async (data) => {
  const id = route.params.id
  if (!id) return
  try {
    const res = await api.patch(`/recruiter/applications/${id}/extracted-data`, data)
    toastStore.show(`Donnees mises a jour — Score: ${res.data.overallScore}%`, 'success')
    await fetchCandidate()
  } catch (err) {
    console.error('Erreur mise a jour donnees extraites:', err)
    toastStore.show('Erreur lors de la mise a jour des donnees.', 'error')
    throw err
  }
}

const onGenerateOffer = async (formData, callback) => {
  const id = route.params.id
  isGeneratingOffer.value = true
  try {
    const res = await api.post(`/recruiter/applications/${id}/generate-offer-letter`, formData, { timeout: 600000 })
    if (callback) callback(res.data.offerLetter)
    return res.data.offerLetter
  } catch (err) {
    console.error('Erreur generation offre:', err)
    toastStore.show('Erreur lors de la generation de l\'offre.', 'error')
    return null
  } finally {
    isGeneratingOffer.value = false
  }
}

const onConfirmOffer = async (content) => {
  const id = route.params.id
  try {
    // Send the email via the backend
    await api.post(`/recruiter/applications/${id}/send-offer-email`, { content })
    toastStore.show('Offre validée et envoyée par email au candidat !', 'success')
  } catch (err) {
    console.error('Erreur envoi email offre:', err)
    toastStore.show('Erreur lors de l\'envoi de l\'email.', 'error')
  }
  showOfferModal.value = false
  currentStage.value = 'offersent'
  // Refresh timeline
  await fetchCandidate()
}

onMounted(() => {
  fetchCandidate()
})
</script>

<style>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";

/* Extra CSS for Candidate View */
:root {
  --accent: v-bind(accentColor);
  --accent-grad: linear-gradient(135deg, v-bind(accentColor) 0%, v-bind(accentDark) 100%);
  --accent-soft: v-bind('accentColor + "1a"'); /* Adds transparency (approx 0.1) */
  --accent-glow: v-bind('accentColor + "33"'); /* Adds transparency (approx 0.2) */
}
</style>

<style scoped>
.dashboard-layout { 
  background: #f8fafc !important; 
  min-height: 100vh;
}

.dark-mode .dashboard-layout {
  background: #020617 !important;
}

.main-content {
  transition: all 0.5s cubic-bezier(0.2, 0.8, 0.2, 1);
  margin-left: calc(var(--sidebar-width) + 20px);
  width: calc(100% - var(--sidebar-width) - 40px);
}

.main-content.ml-collapsed {
  margin-left: calc(var(--sidebar-collapsed) + 20px);
  width: calc(100% - var(--sidebar-collapsed) - 40px);
}

.recruiter-main {
  padding: 40px 20px;
  max-width: 1600px;
}

.profile-grid-container {
  display: grid;
  grid-template-columns: 310px 1fr;
  gap: 24px;
  align-items: flex-start;
}

.side-col {
  display: flex;
  flex-direction: column;
  gap: 24px;
  position: sticky;
  top: 40px;
}

.tab-content-panel { 
  background: white;
  border-radius: 28px;
  padding: 24px;
  border: 1px solid rgba(226, 232, 240, 0.8);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.02);
  min-height: 500px; 
  margin-bottom: 32px; 
}

.dark-mode .tab-content-panel {
  background: #0f172a;
  border-color: rgba(255, 255, 255, 0.08);
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
}

/* Transitions */
.fade-slide-enter-active, .fade-slide-leave-active { 
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1); 
}
.fade-slide-enter-from { 
  opacity: 0; 
  transform: translateY(20px); 
}
.fade-slide-leave-to { 
  opacity: 0; 
  transform: translateY(-20px); 
}

.stagger-reveal {
  animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes revealUp {
  from { opacity: 0; transform: translateY(30px); }
  to { opacity: 1; transform: translateY(0); }
}

@media (max-width: 1200px) { 
  .main-content, .main-content.ml-collapsed {
    margin-left: 0;
    padding: 20px;
  }
  .profile-grid-container { 
    grid-template-columns: 1fr; 
  }
  .side-col {
    position: static;
  }
}
</style>
