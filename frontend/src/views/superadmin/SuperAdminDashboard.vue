<template>
  <div class="sa-layout" :class="{ 'theme-dark': darkMode }">
    <!-- CELESTIAL BACKGROUND -->
    <div class="celestial-bg">
      <div class="c-orb orb-1"></div>
      <div class="c-orb orb-2"></div>
      <div class="c-orb orb-3"></div>
    </div>

    <!-- Sidebar Component -->
    <SuperAdminSidebar
      v-model:activeSection="activeSection"
      :navItems="navItems"
      :accentColor="accentColor"
      :accentGrad="accentGrad"
      :darkMode="darkMode"
      @toggleDarkMode="toggleDarkMode"
      @logout="handleLogout"
    />

    <main class="sa-main-content">
      <!-- Topbar Component -->
      <SuperAdminTopbar
        :accentGrad="accentGrad"
        @logout="handleLogout"
      />

      <!-- Tab Content Components -->
      <transition name="fade-slide" mode="out-in">
        <component
          :is="currentTabComponent"
          :key="activeSection"
          :accentGrad="accentGrad"
          :accentColor="accentColor"
          @add-company="openCompanyModal()"
          @edit-company="openCompanyModal"
          @delete-company="deleteCompanyConfirm"
          @add-user="openUserModal()"
          @edit-user="openUserModal"
          @delete-user="deleteUserConfirm"

          @open-candidate="openCandidateModal"
          @delete-candidate="deleteCandidateConfirm"
          @view-all-logs="activeSection = 'logs'"
        />
      </transition>
    </main>

    <!-- Modals -->
    <CompanyModal
      v-if="showCompanyModal"
      :editing="editingCompany"
      :companyForm="companyForm"
      :loading="adminLoading"
      @close="showCompanyModal = false"
      @save="saveCompany"
    />

    <UserModal
      v-if="showUserModal"
      :editing="editingUser"
      :userForm="userForm"
      :companies="adminStore.companies"
      :loading="adminLoading"
      @close="showUserModal = false"
      @save="saveUser"
    />



    <CandidateModal
      v-if="showCandidateModal"
      :form="candidateDetailForm"
      :applications="candidateDetailApplications"
      :loading="candidateDetailsLoading"
      :adminLoading="adminLoading"
      @close="showCandidateModal = false"
      @save="saveCandidateDetails"
    />

    <DeleteConfirmModal
      v-if="showDeleteConfirm"
      :title="deleteTargetName"
      :loading="deleteConfirmLoading"
      @close="showDeleteConfirm = false"
      @confirm="executeDeletion"
    />

    <DailyBriefingModal
      :show="showBriefing"
      @close="showBriefing = false"
      @navigate="activeSection = $event"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, markRaw } from 'vue'
import { useRouter } from 'vue-router'
import { useAdminStore } from '@/stores/adminStore'
import { useAuthStore } from '@/stores/authStore'

// Components
import SuperAdminSidebar from '@/components/superadmin/layout/SuperAdminSidebar.vue'
import SuperAdminTopbar from '@/components/superadmin/layout/SuperAdminTopbar.vue'
import MonitoringTab from '@/components/superadmin/tabs/MonitoringTab.vue'
import CompaniesTab from '@/components/superadmin/tabs/CompaniesTab.vue'
import UsersTab from '@/components/superadmin/tabs/UsersTab.vue'
import ProfileTab from '@/components/superadmin/tabs/ProfileTab.vue'
import LogsTab from '@/components/superadmin/tabs/LogsTab.vue'
import ApprovalsTab from '@/components/superadmin/tabs/ApprovalsTab.vue'
import SupportTab from '@/components/superadmin/tabs/SupportTab.vue'

// Modals
import CompanyModal from '@/components/superadmin/modals/CompanyModal.vue'
import UserModal from '@/components/superadmin/modals/UserModal.vue'
import CandidateModal from '@/components/superadmin/modals/CandidateModal.vue'
import DeleteConfirmModal from '@/components/superadmin/modals/DeleteConfirmModal.vue'
import DailyBriefingModal from '@/components/superadmin/modals/DailyBriefingModal.vue'

const router = useRouter()
const authStore = useAuthStore()
const adminStore = useAdminStore()

const handleLogout = () => {
  authStore.logout()
  router.push('/login')
}
const activeSection = ref('supervision')
const darkMode = ref(localStorage.getItem('sa_dark_mode') === 'true')
const adminLoading = ref(false)
const showBriefing = ref(false)

const navItems = [
  { id: 'supervision',  label: 'Supervision' },
  { id: 'entreprises',  label: 'Entreprises' },
  { id: 'utilisateurs', label: 'Utilisateurs' },
  { id: 'approbations', label: 'Approbations' },
  { id: 'support',      label: 'Support Hub' },
  { id: 'logs',         label: 'Logs Système' },
  { id: 'profile',      label: 'Mon Profil' }
]

const accentColor = '#6C63FF'
const accentGrad = 'linear-gradient(135deg, #6C63FF 0%, #00D9FF 100%)'

const currentTabComponent = computed(() => {
  const tabs = {
    supervision: markRaw(MonitoringTab),
    entreprises: markRaw(CompaniesTab),
    approbations: markRaw(ApprovalsTab),
    support: markRaw(SupportTab),
    utilisateurs: markRaw(UsersTab),
    profile: markRaw(ProfileTab),
    logs: markRaw(LogsTab)
  }
  return tabs[activeSection.value] || markRaw(MonitoringTab)
})

// --- Modal Handlers: Companies ---
const showCompanyModal = ref(false)
const editingCompany = ref(false)
const companyForm = ref({})

const openCompanyModal = (company = null) => {
  editingCompany.value = !!company
  if (company) {
    companyForm.value = { ...company }
  } else {
    companyForm.value = {
      name: '', size: '', industry: '', website: '', description: '',
      contactEmail: '', contactPhone: '', address: '', city: '', postalCode: '', country: '',
      isActive: true,
      primaryColor: '#6C63FF', secondaryColor: '#00D9FF'
    }
  }
  showCompanyModal.value = true
}

const saveCompany = async () => {
  adminLoading.value = true
  try {
    const success = editingCompany.value
      ? await adminStore.updateCompany(companyForm.value.id, companyForm.value)
      : await adminStore.createCompany(companyForm.value)
    if (success) showCompanyModal.value = false
  } finally {
    adminLoading.value = false
  }
}

// --- Modal Handlers: Users ---
const showUserModal = ref(false)
const editingUser = ref(false)
const userForm = ref({})

const openUserModal = (user = null) => {
  editingUser.value = !!user
  if (user) {
    userForm.value = { ...user }
  } else {
    userForm.value = { firstName: '', lastName: '', email: '', password: '', role: 1, companyId: '', isActive: true }
  }
  showUserModal.value = true
}

const saveUser = async () => {
  adminLoading.value = true
  try {
    const success = editingUser.value
      ? await adminStore.updateUser(userForm.value.id, userForm.value)
      : await adminStore.createUser(userForm.value)
    if (success) showUserModal.value = false
  } finally {
    adminLoading.value = false
  }
}



// --- Candidate Detailed View ---
const showCandidateModal = ref(false)
const candidateDetailsLoading = ref(false)
const candidateDetailForm = ref({})
const candidateDetailApplications = ref([])

const openCandidateModal = async (candidateId) => {
  showCandidateModal.value = true
  candidateDetailsLoading.value = true
  try {
    const res = await adminStore.fetchCandidateWithApplications(candidateId)
    if (res) {
      candidateDetailForm.value = res.candidate
      candidateDetailApplications.value = res.applications
    }
  } finally {
    candidateDetailsLoading.value = false
  }
}

const saveCandidateDetails = async () => {
  adminLoading.value = true
  try {
    await adminStore.updateCandidate(candidateDetailForm.value.id, candidateDetailForm.value)
    showCandidateModal.value = false
  } finally {
    adminLoading.value = false
  }
}

// --- Generic Delete Confirmation ---
const showDeleteConfirm = ref(false)
const deleteConfirmLoading = ref(false)
const deleteTargetType = ref('') // 'company', 'user', 'candidate'
const deleteTargetId = ref('')
const deleteTargetName = ref('')

const deleteCompanyConfirm = (c) => {
  deleteTargetType.value = 'company'
  deleteTargetId.value = c.id
  deleteTargetName.value = c.name
  showDeleteConfirm.value = true
}

const deleteUserConfirm = (u) => {
  deleteTargetType.value = 'user'
  deleteTargetId.value = u.id
  deleteTargetName.value = `${u.firstName} ${u.lastName}`
  showDeleteConfirm.value = true
}



const deleteCandidateConfirm = (app) => {
  deleteTargetType.value = 'candidate'
  deleteTargetId.value = app.id
  deleteTargetName.value = `la candidature de ${app.fullName}`
  showDeleteConfirm.value = true
}

const executeDeletion = async () => {
  deleteConfirmLoading.value = true
  try {
    let ok = false
    if (deleteTargetType.value === 'company') ok = await adminStore.deleteCompany(deleteTargetId.value)
    else if (deleteTargetType.value === 'user') ok = await adminStore.deleteUser(deleteTargetId.value)
    else if (deleteTargetType.value === 'candidate') ok = await adminStore.deleteCandidateApplication(deleteTargetId.value)
    
    if (ok) showDeleteConfirm.value = false
  } finally {
    deleteConfirmLoading.value = false
  }
}

// --- Utils ---
const toggleDarkMode = () => {
  darkMode.value = !darkMode.value
  localStorage.setItem('sa_dark_mode', darkMode.value)
}


onMounted(async () => {
  await adminStore.fetchAll()
  
  // Afficher le briefing seulement si c'est la première fois de la session
  const briefingShown = sessionStorage.getItem('briefing_shown')
  if (!briefingShown) {
    setTimeout(() => {
      showBriefing.value = true
      sessionStorage.setItem('briefing_shown', 'true')
    }, 1000)
  }
})
</script>

<style>
/* Global CSS variables - Shared by all sa components */
.sa-layout {
  --sidebar-bg: #ffffff;
  --bg-main: #f8fafc;
  --bg-solid: #f8fafc;
  --card-bg: #ffffff;
  --text-primary: #1e293b;
  --text-muted: #64748b;
  --border-thin: #e2e8f0;
  --accent-color: #6C63FF;
  --accent-soft: rgba(108, 99, 255, 0.1);
  --accent-grad: linear-gradient(135deg, #6C63FF 0%, #A463FF 100%);
  --bg-hover: #f8fafc;
  --bg-base: #ffffff;
  --glass-border: rgba(108, 99, 255, 0.15);
  --glass-shadow: 0 4px 24px rgba(108, 99, 255, 0.08);
  --violet-glow: rgba(108, 99, 255, 0.15);
  --violet-glow-strong: rgba(108, 99, 255, 0.25);

  /* Celestial Orbs - light violet for light mode */
  --c-orb-1: radial-gradient(circle, #6C63FF, #A463FF);
  --c-orb-2: radial-gradient(circle, #A463FF, #6C63FF);
  --c-orb-3: radial-gradient(circle, #00D9FF, #6C63FF);
}

.theme-dark {
  --sidebar-bg: rgba(15, 23, 42, 0.8);
  --bg-main: #0a0f1d;
  --bg-solid: #0a0f1d;
  --card-bg: rgba(30, 41, 59, 0.7);
  --text-primary: #f8fafc;
  --text-muted: #94a3b8;
  --border-thin: rgba(255, 255, 255, 0.06);
  --bg-hover: rgba(255, 255, 255, 0.03);
  --bg-base: #0f172a;
  --glass-border: rgba(255, 255, 255, 0.1);
  --glass-shadow: 0 15px 35px rgba(0,0,0,0.5);
}

/* CELESTIAL BACKGROUND ORBS */
.celestial-bg {
  position: absolute; inset: 0; z-index: 0; overflow: hidden; pointer-events: none;
}
.c-orb {
  position: absolute;
  border-radius: 50%;
  filter: blur(120px);
  pointer-events: none;
  transition: opacity 0.5s;
}
.orb-1 {
  width: 700px; height: 700px;
  background: var(--c-orb-1);
  top: -250px; left: -100px;
  opacity: 0.02;
  animation: floatOrb1 20s ease-in-out infinite alternate;
}
.orb-2 {
  width: 600px; height: 600px;
  background: var(--c-orb-2);
  bottom: -150px; right: -100px;
  opacity: 0.02;
  animation: floatOrb2 26s ease-in-out infinite alternate;
}
.orb-3 {
  width: 500px; height: 500px;
  background: var(--c-orb-3);
  top: 30%; left: 40%;
  opacity: 0.015;
  animation: floatOrb3 18s ease-in-out infinite alternate;
}

@keyframes floatOrb1 {
  0%   { transform: translate(0, 0) scale(1); }
  50%  { transform: translate(80px, 60px) scale(1.12); }
  100% { transform: translate(-30px, 120px) scale(0.93); }
}
@keyframes floatOrb2 {
  0%   { transform: translate(0, 0) scale(1); }
  50%  { transform: translate(-70px, -50px) scale(1.08); }
  100% { transform: translate(50px, -110px) scale(0.95); }
}
@keyframes floatOrb3 {
  0%   { transform: translate(0, 0) scale(1); }
  50%  { transform: translate(40px, -60px) scale(1.15); }
  100% { transform: translate(-60px, 40px) scale(0.9); }
}

.theme-dark .c-orb { opacity: 0.18; }

/* Global Components */
.glass-panel {
  background: rgba(255, 255, 255, 0.7);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  border: 1px solid var(--glass-border);
  border-radius: 20px;
  box-shadow: var(--glass-shadow);
}

.theme-dark .glass-panel {
  background: rgba(30, 41, 59, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.sa-tab-container {
  padding: 30px;
  animation: tabReveal 0.6s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes tabReveal {
  from { opacity: 0; transform: translateY(10px); }
  to   { opacity: 1; transform: translateY(0); }
}

.sa-tab-title {
  font-size: 32px;
  font-weight: 800;
  color: var(--text-primary);
  margin-bottom: 8px;
  letter-spacing: -0.8px;
}

.sa-tab-desc {
  color: var(--text-muted);
  font-size: 15px;
  margin-bottom: 30px;
}
</style>

<style scoped>
.sa-layout {
  display: flex;
  min-height: 100vh;
  background-color: var(--bg-solid);
  color: var(--text-primary);
  font-family: 'Inter', system-ui, sans-serif;
  position: relative;
  z-index: 1;
  animation: layoutReveal 0.6s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes layoutReveal {
  from { opacity: 0; }
  to   { opacity: 1; }
}

.sa-main-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
  position: relative;
  z-index: 10;
  margin-left: 320px;
  padding-right: 24px;
  animation: contentSlideIn 0.7s cubic-bezier(0.16, 1, 0.3, 1) 0.1s both;
}

@keyframes contentSlideIn {
  from { opacity: 0; transform: translateX(20px); }
  to   { opacity: 1; transform: translateX(0); }
}

/* Transitions */
.fade-slide-enter-active, .fade-slide-leave-active { transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1); }
.fade-slide-enter-from { opacity: 0; transform: translateY(15px) scale(0.99); }
.fade-slide-leave-to { opacity: 0; transform: translateY(-12px) scale(0.99); }

@media (max-width: 1024px) {
  .sa-main { padding: 24px; }
}
</style>