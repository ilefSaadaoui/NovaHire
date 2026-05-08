
<template>
  <div class="dashboard-layout recruiter-layout app-admin-glass-theme">
    <Sidebar active-item="settings" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome">
          <div class="header-with-icon">
            <div class="icon-box-themed" style="width: 56px; height: 56px; border-radius: 18px;">
              <Settings :size="28" stroke-width="2.5" />
            </div>
            <div>
              <h1 class="premium-title-themed" style="font-size: 28px;">{{ $t('settings.title') }}</h1>
              <p class="welcome-sub">{{ $t('settings.subtitle') }}</p>
            </div>
          </div>
        </div>
        
        <div class="r-header-tools">
          <div class="admin-identity-tag">
            <div class="tag-glow"></div>
            <span class="tag-dot"></span>
            <span class="tag-text">{{ $t('settings.adminMode') }}</span>
          </div>
        </div>
      </header>

      <div class="page-body stagger-reveal" :class="{ 'wide-page-body': activeTab === 'team' }">
        <nav class="admin-tabs-floating anim-reveal-up">
          <button v-for="t in tabs" :key="t.id" class="admin-tab-btn" :class="{ active: activeTab === t.id }" @click="activeTab = t.id">
            {{ t.label }}
          </button>
        </nav>

        <div class="tab-content-panel">
          <TeamSection 
            v-if="activeTab === 'team'" 
            :team="companyStore.team.filter(m => m.role && m.role.toLowerCase() === 'recruiter')"
            @invite="showInviteModal = true"
            @edit="openEditMember"
            @delete="confirmDeleteMember"
            @update-role="updateRole"
            @toggle-status="handleToggleStatus"
            @bulk-status="handleBulkStatus"
            @bulk-delete="handleBulkDelete"
            @resend-invite="handleResendInvite"
          />

          <DepartmentsSection 
            v-if="activeTab === 'departments'" 
            :departments="companyStore.departments"
            @add="openAddDept"
            @edit="openEditDept"
            @delete="confirmDeleteDept"
          />

          <BrandingSection 
            v-if="activeTab === 'branding'" 
            v-model:form="brandingForm"
            :is-saving="isSaving"
            @save="saveBranding"
          />


        </div>
      </div>
    </main>

    <!-- MODALS — placed outside main to avoid .main-content stacking context -->
    <InviteMemberModal 
      :show="showInviteModal"
      v-model:form="inviteForm"
      :departments="companyStore.departments"
      :loading="inviteLoading"
      @close="showInviteModal = false"
      @submit="inviteMember"
    />

    <EditMemberModal 
      :show="showEditModal"
      v-model:form="editForm"
      :departments="companyStore.departments"
      :loading="editLoading"
      @close="showEditModal = false"
      @submit="submitEditMember"
    />

    <DepartmentModal 
      :show="showDeptModal"
      :is-editing="isEditingDept"
      v-model:form="deptForm"
      :loading="deptLoading"
      @close="showDeptModal = false"
      @submit="submitDeptForm"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import Sidebar from '@/components/layout/Sidebar.vue'
import TeamSection from '@/components/company-admin/settings/TeamSection.vue'
import DepartmentsSection from '@/components/company-admin/settings/DepartmentsSection.vue'
import BrandingSection from '@/components/company-admin/settings/BrandingSection.vue'

import InviteMemberModal from '@/components/company-admin/settings/modals/InviteMemberModal.vue'
import EditMemberModal from '@/components/company-admin/settings/modals/EditMemberModal.vue'
import DepartmentModal from '@/components/company-admin/settings/modals/DepartmentModal.vue'
import { Settings } from 'lucide-vue-next'

import { useCompanyStore } from '@/stores/companyStore'
import { useToastStore } from '@/stores/toastStore'

const { t } = useI18n()
const companyStore = useCompanyStore()
const toast = useToastStore()

// State
const activeTab = ref('team')
const sidebarCollapsed = ref(false)
const isSaving = ref(false)
const tabs = [
  { id: 'team', label: 'Équipe' },
  { id: 'departments', label: 'Départements' },
  { id: 'branding', label: 'Branding' }
]



// Forms & Modals
const showInviteModal = ref(false)
const inviteLoading = ref(false)
const inviteForm = ref({ email: '', firstName: '', lastName: '', role: 'Recruiter', jobTitle: '', departmentId: null })

const showEditModal = ref(false)
const editLoading = ref(false)
const editForm = ref({ id: '', firstName: '', lastName: '', email: '', isActive: true, departmentId: null })

const showDeptModal = ref(false)
const isEditingDept = ref(false)
const deptLoading = ref(false)
const deptForm = ref({ id: null, name: '', description: '' })

const brandingForm = ref({
  companyName: '',
  logoUrl: '',
  primaryColor: '#FFD700',
  secondaryColor: '#000000',
  description: '',
  industry: '',
  website: '',
  country: '',
  contactPhone: ''
})

onMounted(async () => {
  await Promise.all([
    companyStore.fetchTeam(),
    companyStore.fetchDepartments(),
    companyStore.fetchBranding()
  ])
  brandingForm.value = { ...companyStore.branding }
})

// Methods: Team
const updateRole = async ({ member, role }) => {
  try {
    await companyStore.updateMemberRole(member.id, role)
    toast.show(t('notifications.roleUpdated'), 'success')
  } catch (err) {
    toast.show('Erreur lors du changement de rôle', 'error')
  }
}

const openEditMember = (member) => {
  editForm.value = {
    id: member.id,
    firstName: member.firstName,
    lastName: member.lastName,
    email: member.email,
    isActive: member.isActive,
    departmentId: member.departmentId || null
  }
  showEditModal.value = true
}

const submitEditMember = async () => {
  editLoading.value = true
  try {
    await companyStore.updateMemberDetails(editForm.value.id, editForm.value)
    showEditModal.value = false
    toast.show('Membre mis à jour avec succès', 'success')
  } finally {
    editLoading.value = false
  }
}

const handleToggleStatus = async (member) => {
  const newStatus = !member.isActive
  const actionText = newStatus ? 'activé' : 'désactivé'
  
  try {
    const payload = {
      ...member,
      isActive: newStatus
    }
    await companyStore.updateMemberDetails(member.id, payload)
    toast.show(`Membre ${actionText} avec succès`, 'success')
  } catch (err) {
    toast.show(`Erreur lors du changement de statut`, 'error')
  }
}

const handleBulkStatus = async ({ ids, isActive }) => {
  const actionText = isActive ? 'activer' : 'désactiver'
  if (confirm(`Voulez-vous ${actionText} les ${ids.length} membres sélectionnés ?`)) {
    await companyStore.bulkUpdateMembersStatus(ids, isActive)
  }
}

const handleBulkDelete = async (ids) => {
  if (confirm(`ATTENTION : Êtes-vous sûr de vouloir supprimer définitivement ces ${ids.length} membres ? Cette action est irréversible.`)) {
    await companyStore.bulkRemoveMembers(ids)
  }
}

const handleResendInvite = async (member) => {
  await companyStore.resendInvitation(member)
}

const confirmDeleteMember = async (member) => {
  if (confirm(`Êtes-vous sûr de vouloir retirer ${member.fullName} de l'équipe ?`)) {
    await companyStore.removeTeamMember(member.id)
  }
}

const inviteMember = async () => {
  inviteLoading.value = true
  try {
    await companyStore.inviteMember(inviteForm.value)
    toast.show('Invitation expédiée avec succès ! 🚀', 'success')
    showInviteModal.value = false
    inviteForm.value = { email: '', firstName: '', lastName: '', role: 'Recruiter', jobTitle: '', departmentId: null }
    await companyStore.fetchTeam()
  } finally {
    inviteLoading.value = false
  }
}

// Methods: Departments
const openAddDept = () => {
  isEditingDept.value = false
  deptForm.value = { id: null, name: '', description: '' }
  showDeptModal.value = true
}

const openEditDept = (dept) => {
  isEditingDept.value = true
  deptForm.value = { id: dept.id, name: dept.name, description: dept.description }
  showDeptModal.value = true
}

const submitDeptForm = async () => {
  if (!deptForm.value.name) {
    toast.show("Le nom du département est requis", "error")
    return
  }
  deptLoading.value = true
  try {
    if (isEditingDept.value) {
      await companyStore.updateDepartment(deptForm.value.id, deptForm.value)
    } else {
      await companyStore.createDepartment(deptForm.value)
    }
    showDeptModal.value = false
    toast.show(isEditingDept.value ? 'Département mis à jour' : 'Département créé', 'success')
  } finally {
    deptLoading.value = false
  }
}

const confirmDeleteDept = async (dept) => {
  if (confirm(`Voulez-vous vraiment supprimer le département ${dept.name} ?`)) {
    await companyStore.deleteDepartment(dept.id)
  }
}

// Methods: Branding & Plan
const saveBranding = async () => {
  isSaving.value = true
  try {
    await companyStore.updateBranding(brandingForm.value)
    toast.show('Branding mis à jour', 'success')
  } finally {
    isSaving.value = false
  }
}


</script>

<style scoped>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";
@import "@/assets/admin-theme.css";

.dashboard-layout { display: flex; min-height: 100vh; background: transparent !important; }
.main-content { transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1); flex: 1; }
.ml-collapsed { margin-left: var(--sidebar-collapsed); }

.admin-tabs-floating { 
  display: flex; gap: 8px; margin: 0 auto 32px auto; padding: 6px; 
  background: var(--r-main-bg, rgba(244, 246, 248, 0.5)); 
  border-radius: 18px; width: fit-content; 
  border: 1px solid var(--r-border);
}
.admin-tab-btn { 
  padding: 12px 28px; border-radius: 14px; border: none; background: transparent; 
  color: var(--r-text-sub); font-weight: 800; font-size: 14px; cursor: pointer; transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); 
}
.admin-tab-btn.active { 
  background: white; 
  color: var(--accent-color); 
  box-shadow: 0 8px 20px -5px var(--accent-soft), 0 2px 10px rgba(0,0,0,0.05);
}
.admin-tab-btn:not(.active):hover {
  background: rgba(255, 255, 255, 0.5);
  color: var(--accent-color);
}
 
.wide-page-body { 
  padding-left: 0; 
  padding-right: 0; 
  max-width: none !important;
  width: 100%;
}

.tab-content-panel { min-height: 400px; }

.panel-fade-enter-active, .panel-fade-leave-active { transition: opacity 0.3s ease, transform 0.3s ease; }
.panel-fade-enter-from, .panel-fade-leave-to { opacity: 0; transform: translateY(10px); }

@media (max-width: 1200px) { .branding-grid-premium { grid-template-columns: 1fr; } }
</style>

<style>
/* Fix for button text visibility in light theme */
body:not(.dark-mode) .dashboard-layout .btn-luxury,
body:not(.dark-mode) .dashboard-layout .btn-cancel {
  color: #1e293b !important;
}
</style>

