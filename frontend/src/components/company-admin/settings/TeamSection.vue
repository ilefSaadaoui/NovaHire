<template>
  <div class="admin-glass-card anim-reveal-up">
    <div class="section-header-premium">
      <div>
        <h4 class="celestial-title imperial-aura">{{ $t('settings.team.title') }}</h4>
        <p class="section-subtitle">{{ $t('settings.team.subtitle') }}</p>
      </div>
      <div class="header-actions-group">
        <button v-if="selectedMemberIds.length > 0" class="btn-clear-selection" @click="selectedMemberIds = []">
          Effacer la sélection ({{ selectedMemberIds.length }})
        </button>
        <button class="btn-invite-member" @click="$emit('invite')">
          <Plus :size="18" stroke-width="3" />
          <span>{{ $t('settings.team.invite') || 'Inviter un membre' }}</span>
        </button>
      </div>
    </div>
    
    <div class="team-filters-bar">
      <div class="search-box">
        <Search :size="16" class="search-icon" />
        <input type="text" v-model="searchQuery" placeholder="Rechercher par nom ou email..." class="search-input" />
      </div>
      <div class="filter-box premium-filter-box">
        <Filter :size="16" class="filter-icon" />
        <PremiumSelect 
          v-model="selectedDepartment" 
          :options="departmentOptions" 
          class="team-filter-select"
        />
      </div>
      <div class="filter-box premium-filter-box">
        <UserCheck :size="16" class="filter-icon" />
        <PremiumSelect 
          v-model="selectedStatus" 
          :options="statusOptions" 
          class="team-filter-select"
        />
      </div>
    </div>
    
    <div class="glass-table-wrap">
      <table class="celestial-table">
        <thead>
          <tr>
            <th style="width: 40px; padding-right: 0;">
              <div class="lux-checkbox-wrap">
                <input 
                  type="checkbox" 
                  :checked="isAllSelected" 
                  @change="toggleSelectAll"
                  class="lux-checkbox"
                >
              </div>
            </th>
            <th style="min-width: 220px;">Membre</th>
            <th style="min-width: 140px;">Poste & Accès</th>
            <th style="min-width: 120px;">Département</th>
            <th style="min-width: 120px;">Activité</th>
            <th style="width: 100px;">Statut</th>
            <th style="width: 210px; text-align: right; white-space: nowrap;">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="m in filteredTeam" :key="m.id" :class="{ 'row-selected': selectedMemberIds.includes(m.id), 'row-pending': !m.lastLoginAt }">
            <td style="padding-right: 0;">
              <div class="lux-checkbox-wrap">
                <input 
                  type="checkbox" 
                  :value="m.id" 
                  v-model="selectedMemberIds"
                  class="lux-checkbox"
                >
              </div>
            </td>
            <td>
              <div class="user-cell">
                <div class="r-avatar sm" :class="{'initials-mode': !m.avatarUrl}">
                  <img v-if="m.avatarUrl" :src="m.avatarUrl" :alt="m.fullName" class="avatar-img-fit" />
                  <span v-else>{{ m.initials }}</span>
                </div>
                <div class="user-meta">
                  <div class="u-name">{{ m.fullName }}</div>
                  <div class="u-email">{{ m.email }} • <span style="opacity:0.7">Depuis {{ formatDate(m.createdAt) }}</span></div>
                </div>
              </div>
            </td>
            <td>
              <div class="user-role-cell">
                <div class="job-title-text">{{ m.jobTitle || 'Non renseigné' }}</div>
                <div class="user-role-badge" :class="m.role.toLowerCase()">
                  {{ m.role === 'CompanyAdmin' ? $t('roles.admin') || 'Administrateur' : $t('roles.recruiter') || 'Recruteur' }}
                </div>
              </div>
            </td>
            <td>
               <div class="dept-badge" :class="{'unassigned': !m.departmentId}">
                 {{ getDepartmentName(m.departmentId) }}
               </div>
            </td>
            <td>
              <div class="activity-metrics-inline">
                <div class="metric"><Briefcase :size="14" /> <span><b>{{ m.offersCount || 0 }}</b> offres</span></div>
                <div class="metric"><Calendar :size="14" /> <span><b>{{ m.interviewsCount || 0 }}</b> entretiens</span></div>
              </div>
            </td>
            <td>
              <div v-if="!m.lastLoginAt" class="status-pill-glowing pending" title="En attente de première connexion">
                <span class="pulse-dot"></span>
                INVITATION
              </div>
              <div v-else class="status-pill-glowing" :class="m.isActive ? 'active' : 'disabled'">
                <span class="pulse-dot"></span>
                {{ m.isActive ? 'ACTIF' : 'INACTIF' }}
              </div>
            </td>
            <td style="text-align: right;">
              <div class="action-group">
                <!-- Resend Invitation (Only for pending) -->
                <button 
                  v-if="!m.lastLoginAt"
                  class="action-btn resend" 
                  @click="$emit('resend-invite', m)" 
                  title="Renvoyer l'invitation"
                >
                   <SendHorizontal :size="18" />
                </button>

                <!-- Toggle Status Button -->
                <button 
                  v-if="m.isActive"
                  class="action-btn deactivate" 
                  @click="$emit('toggle-status', m)" 
                  title="Désactiver le membre"
                >
                   <UserMinus :size="18" />
                </button>
                <button 
                  v-else
                  class="action-btn activate" 
                  @click="$emit('toggle-status', m)" 
                  title="Activer le membre"
                >
                   <UserCheck :size="18" />
                </button>

                <button class="action-btn view" @click="$emit('edit', m)" title="Éditer le profil">
                   <Eye :size="18" />
                </button>
                <button class="action-btn remove" @click="$emit('delete', m)" title="Supprimer définitivement">
                   <Trash2 :size="18" />
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- FLOATING BULK ACTION BAR -->
    <Transition name="floating-slide">
      <div v-if="selectedMemberIds.length > 0" class="bulk-action-bar-floating">
        <div class="bulk-bar-content">
          <div class="selection-info">
            <span class="selection-count">{{ selectedMemberIds.length }}</span>
            <span class="selection-text">membre(s) sélectionné(s)</span>
          </div>
          
          <div class="bulk-actions-group">
            <button class="bulk-btn activate" @click="$emit('bulk-status', { ids: selectedMemberIds, isActive: true })">
              <UserCheck :size="18" />
              <span>Activer</span>
            </button>
            <button class="bulk-btn deactivate" @click="$emit('bulk-status', { ids: selectedMemberIds, isActive: false })">
              <UserMinus :size="18" />
              <span>Désactiver</span>
            </button>
            <div class="bulk-divider"></div>
            <button class="bulk-btn delete" @click="$emit('bulk-delete', selectedMemberIds)">
              <Trash2 :size="18" />
              <span>Supprimer</span>
            </button>
            <button class="bulk-btn-close" @click="selectedMemberIds = []">
              <X :size="18" />
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { Plus, Eye, Trash2, Search, Filter, Briefcase, Calendar, UserCheck, UserMinus, SendHorizontal, X } from 'lucide-vue-next'
import { useCompanyStore } from '@/stores/companyStore'
import PremiumSelect from '@/components/common/PremiumSelect.vue'

const props = defineProps({
  team: Array
})

defineEmits(['invite', 'edit', 'delete', 'update-role', 'toggle-status', 'bulk-status', 'bulk-delete', 'resend-invite'])

const companyStore = useCompanyStore()

const searchQuery = ref('')
const selectedDepartment = ref('')
const selectedStatus = ref('all')
const selectedMemberIds = ref([])

const isAllSelected = computed(() => {
  return filteredTeam.value.length > 0 && selectedMemberIds.value.length === filteredTeam.value.length
})

const toggleSelectAll = () => {
  if (isAllSelected.value) {
    selectedMemberIds.value = []
  } else {
    selectedMemberIds.value = filteredTeam.value.map(m => m.id)
  }
}

const departments = computed(() => companyStore.departments || [])

const departmentOptions = computed(() => [
  { value: '', label: 'Tous les départements' },
  ...departments.value.map(d => ({ value: d.id, label: d.name }))
])

const statusOptions = [
  { value: 'all', label: 'Tous les statuts' },
  { value: 'active', label: 'Membres Actifs' },
  { value: 'inactive', label: 'Membres Inactifs' },
  { value: 'pending', label: 'Invitations (En attente)' }
]

const filteredTeam = computed(() => {
  if (!props.team) return []
  return props.team.filter(m => {
    const matchSearch = m.fullName.toLowerCase().includes(searchQuery.value.toLowerCase()) || 
                        m.email.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchDept = selectedDepartment.value ? m.departmentId === selectedDepartment.value : true
    
    let matchStatus = true
    if (selectedStatus.value === 'active') {
      matchStatus = m.isActive && m.lastLoginAt
    } else if (selectedStatus.value === 'inactive') {
      matchStatus = !m.isActive
    } else if (selectedStatus.value === 'pending') {
      matchStatus = !m.lastLoginAt
    }

    return matchSearch && matchDept && matchStatus
  })
})

const getDepartmentName = (id) => {
  const dept = departments.value.find(d => d.id === id)
  return dept ? dept.name : 'Non assigné'
}

const formatDate = (dateString) => {
  if (!dateString) return ''
  const d = new Date(dateString)
  return d.toLocaleDateString('fr-FR', { month: 'short', year: 'numeric' })
}

// getSmartStatus removed as we now use simple isActive boolean in template

</script>

<style scoped>
.admin-glass-card { background: var(--card-bg); border: 1px solid var(--r-border); border-radius: 32px; padding: 24px 28px; width: 100%; box-sizing: border-box; }
.section-header-premium { display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px; }
.header-actions-group { display: flex; align-items: center; gap: 20px; }
.celestial-title { font-size: 20px; font-weight: 900; color: var(--r-text-main); letter-spacing: -0.5px; }
.section-subtitle { font-size: 13px; color: var(--r-text-sub); margin-top: 4px; font-weight: 600; }

.glass-table-wrap { border-radius: 20px; border: 1px solid var(--r-border); overflow: visible; background: var(--card-bg); box-shadow: 0 8px 30px rgba(0, 0, 0, 0.03); }
.celestial-table { width: 100%; border-collapse: collapse; table-layout: auto; }
.celestial-table th { text-align: left; padding: 14px 16px; font-size: 10.5px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 1.2px; border-bottom: 1px solid var(--r-border); }
.celestial-table td { padding: 12px 16px; border-bottom: 1px solid var(--r-border); vertical-align: middle; }
.celestial-table tr:last-child td { border-bottom: none; }

.team-filters-bar { display: flex; gap: 16px; margin-bottom: 24px; }
.search-box, .filter-box { display: flex; align-items: center; gap: 10px; background: rgba(0,0,0,0.02); border: 1px solid var(--r-border); border-radius: 14px; padding: 10px 16px; flex: 1; }
.filter-box { flex: 0.5; }
.premium-filter-box { padding: 0 16px; } /* Override padding for premium select integration */
.search-input, .filter-select { border: none; background: transparent; outline: none; flex: 1; font-weight: 600; color: var(--r-text-main); font-size: 13px; }
.search-icon, .filter-icon { color: var(--r-text-sub); }

/* Premium Select Integration Styles */
.team-filter-select { flex: 1; }
.team-filter-select :deep(.premium-select-display) { border: none; background: transparent; padding: 10px 0; box-shadow: none; font-size: 13px; font-weight: 600; color: var(--r-text-main); height: 100%; border-radius: 0; }
.team-filter-select :deep(.premium-select-display:hover),
.team-filter-select :deep(.premium-select-wrapper.is-open .premium-select-display) { box-shadow: none; background: transparent; border-color: transparent; }
.team-filter-select :deep(.dropdown-icon) { width: 16px; height: 16px; color: var(--r-text-sub); }

.dept-badge { font-size: 12px; font-weight: 800; background: rgba(var(--accent-rgb), 0.1); color: var(--accent); padding: 6px 12px; border-radius: 8px; width: fit-content; text-align: center; }
.dept-badge.unassigned { background: rgba(0,0,0,0.04); color: var(--r-text-sub); }

.activity-metrics-inline { display: flex; flex-direction: column; gap: 6px; }
.activity-metrics-inline .metric { display: flex; align-items: center; gap: 6px; font-size: 12px; color: var(--r-text-sub); font-weight: 500; }
.activity-metrics-inline .metric b { color: var(--r-text-main); font-weight: 800; }
.activity-metrics-inline .metric svg { color: var(--accent); opacity: 0.8; }

.user-cell { display: flex; align-items: center; gap: 10px; }
.r-avatar.sm { width: 44px; height: 44px; border-radius: 14px; display: flex; align-items: center; justify-content: center; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.08); transition: all 0.3s; }
.r-avatar.sm:hover { transform: scale(1.05); box-shadow: 0 6px 15px rgba(0,0,0,0.12); }
.r-avatar.sm.initials-mode { background: var(--accent-grad); color: white; font-size: 14px; font-weight: 800; border: 1px solid rgba(255,255,255,0.1); }
.avatar-img-fit { width: 100%; height: 100%; object-fit: cover; }
.u-name { font-weight: 800; font-size: 14px; color: var(--r-text-main); }
.u-email { font-size: 11.5px; color: var(--r-text-sub); font-weight: 600; line-height: 1.3; }

.user-role-cell { display: flex; flex-direction: column; gap: 6px; align-items: flex-start; }
.job-title-text { font-size: 13px; font-weight: 800; color: var(--r-text-main); }

.user-role-badge { font-weight: 800; font-size: 10px; padding: 4px 10px; border-radius: 6px; display: inline-block; text-transform: uppercase; letter-spacing: 0.5px; }
.user-role-badge.companyadmin { background: rgba(234, 179, 8, 0.1); color: #ca8a04; border: 1px solid rgba(234, 179, 8, 0.2); }
.user-role-badge.recruiter { background: rgba(59, 130, 246, 0.1); color: #2563eb; border: 1px solid rgba(59, 130, 246, 0.2); }

.status-pill-glowing { display: flex; align-items: center; gap: 8px; padding: 6px 12px; border-radius: 100px; font-size: 11px; font-weight: 800; background: rgba(0,0,0,0.03); color: var(--r-text-sub); width: fit-content; }
.status-pill-glowing.active { background: rgba(16, 185, 129, 0.1); color: #10b981; }
.status-pill-glowing.charged { background: rgba(239, 68, 68, 0.1); color: #ef4444; border: 1px solid rgba(239, 68, 68, 0.1); }
.status-pill-glowing.idle { background: rgba(245, 158, 11, 0.1); color: #f59e0b; }
.status-pill-glowing.inactive { background: rgba(239, 68, 68, 0.1); color: #ef4444; }
.status-pill-glowing.disabled { background: rgba(100, 116, 139, 0.1); color: #64748b; }
.status-pill-glowing .pulse-dot { width: 6px; height: 6px; border-radius: 50%; background: currentColor; }
 
.action-group { display: flex; gap: 8px; justify-content: flex-end; min-width: 180px; white-space: nowrap; }

.action-btn {
  width: 36px; height: 36px; border-radius: 12px; border: none;
  display: flex; align-items: center; justify-content: center;
  cursor: pointer; transition: all 0.3s cubic-bezier(0.25, 0.46, 0.45, 0.94);
  position: relative;
}

.action-btn.view {
  background: linear-gradient(135deg, rgba(59, 130, 246, 0.08), rgba(99, 102, 241, 0.12));
  color: #6366f1;
  border: 1px solid rgba(99, 102, 241, 0.12);
}
.action-btn.view:hover {
  background: linear-gradient(135deg, rgba(59, 130, 246, 0.18), rgba(99, 102, 241, 0.25));
  transform: translateY(-3px);
  box-shadow: 0 8px 20px -4px rgba(99, 102, 241, 0.3);
  color: #4f46e5;
}

.action-btn.remove {
  background: linear-gradient(135deg, rgba(244, 63, 94, 0.1), rgba(239, 68, 68, 0.15));
  color: #f87171;
  border: 1px solid rgba(239, 68, 68, 0.15);
}
.action-btn.remove:hover {
  background: linear-gradient(135deg, rgba(244, 63, 94, 0.2), rgba(239, 68, 68, 0.3));
  transform: translateY(-3px);
  box-shadow: 0 8px 20px -4px rgba(239, 68, 68, 0.35);
  color: #ef4444;
}

.action-btn.deactivate {
  background: linear-gradient(135deg, rgba(249, 115, 22, 0.08), rgba(239, 68, 68, 0.12));
  color: #f97316;
  border: 1px solid rgba(249, 115, 22, 0.12);
}
.action-btn.deactivate:hover {
  background: linear-gradient(135deg, rgba(249, 115, 22, 0.18), rgba(239, 68, 68, 0.25));
  transform: translateY(-3px);
  box-shadow: 0 8px 20px -4px rgba(249, 115, 22, 0.3);
  color: #ea580c;
}

.action-btn.activate {
  background: linear-gradient(135deg, rgba(16, 185, 129, 0.08), rgba(6, 182, 212, 0.12));
  color: #10b981;
  border: 1px solid rgba(16, 185, 129, 0.12);
}
.action-btn.activate:hover {
  background: linear-gradient(135deg, rgba(16, 185, 129, 0.18), rgba(6, 182, 212, 0.25));
  transform: translateY(-3px);
  box-shadow: 0 8px 20px -4px rgba(16, 185, 129, 0.3);
  color: #059669;
}

.action-btn:active { transform: translateY(0); box-shadow: none; }

/* ─── Invite Button (matching dept style) ─── */
.btn-invite-member {
  display: flex; align-items: center; gap: 10px;
  padding: 12px 24px; border-radius: 14px; border: none;
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  color: white; font-weight: 800; font-size: 13px;
  cursor: pointer; transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 6px 20px -4px rgba(14, 165, 233, 0.4), 0 2px 0 #0891b2;
  text-transform: uppercase; letter-spacing: 0.5px;
  position: relative; overflow: hidden;
}
.btn-invite-member::after {
  content: ''; position: absolute; top: 0; left: -100%;
  width: 100%; height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent);
  transition: 0.5s;
}
.btn-invite-member:hover::after { left: 100%; }
.btn-invite-member:hover { transform: translateY(-2px); box-shadow: 0 10px 28px -6px rgba(14, 165, 233, 0.5), 0 1px 0 #0891b2; }
.btn-invite-member:active { transform: translateY(1px); box-shadow: 0 2px 6px rgba(0,0,0,0.15); }

/* --- Checkboxes --- */
.lux-checkbox-wrap { display: flex; align-items: center; justify-content: center; }
.lux-checkbox {
  appearance: none; -webkit-appearance: none;
  width: 18px; height: 18px;
  border: 2px solid var(--r-border); border-radius: 6px;
  background: transparent; cursor: pointer; position: relative;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}
.lux-checkbox:checked { background: var(--accent); border-color: var(--accent); }
.lux-checkbox:checked::after {
  content: '✓'; position: absolute; top: 50%; left: 50%;
  transform: translate(-50%, -55%); color: white;
  font-size: 12px; font-weight: 900;
}
.lux-checkbox:hover { border-color: var(--accent); transform: scale(1.1); }

/* --- Row States --- */
.celestial-table tr.row-selected { background: rgba(14, 165, 233, 0.03); }
.celestial-table tr.row-pending { background: rgba(245, 158, 11, 0.02); }

/* --- Status Pending --- */
.status-pill-glowing.pending { background: rgba(245, 158, 11, 0.1); color: #f59e0b; border: 1px solid rgba(245, 158, 11, 0.1); }

/* --- Action Resend --- */
.action-btn.resend {
  background: linear-gradient(135deg, rgba(14, 165, 233, 0.12), rgba(6, 182, 212, 0.16));
  color: var(--accent);
  border: 1px solid rgba(14, 165, 233, 0.2);
}
.action-btn.resend:hover {
  background: linear-gradient(135deg, rgba(14, 165, 233, 0.25), rgba(6, 182, 212, 0.3));
  transform: translateY(-3px) rotate(-10deg);
  box-shadow: 0 8px 20px -4px rgba(14, 165, 233, 0.35);
}

/* --- Floating Bulk Bar --- */
.bulk-action-bar-floating {
  position: fixed; bottom: 32px; left: 50%;
  transform: translateX(-50%);
  z-index: 1000; width: fit-content;
  min-width: 500px;
}
.bulk-bar-content {
  background: var(--glass-bg-base);
  backdrop-filter: blur(25px) saturate(180%);
  -webkit-backdrop-filter: blur(25px) saturate(180%);
  border: 1px solid var(--border-thin);
  border-radius: 24px; padding: 14px 28px;
  display: flex; align-items: center; justify-content: space-between; gap: 32px;
  box-shadow: var(--glass-shadow);
}
.selection-info { display: flex; align-items: center; gap: 12px; }
.selection-count {
  background: var(--accent); color: white;
  width: 32px; height: 32px; border-radius: 10px;
  display: flex; align-items: center; justify-content: center;
  font-weight: 900; font-size: 15px;
  box-shadow: 0 4px 12px rgba(var(--accent-rgb), 0.3);
}
.selection-text { font-size: 14px; font-weight: 800; color: var(--r-text-main); letter-spacing: -0.2px; }

.bulk-actions-group { display: flex; align-items: center; gap: 8px; }
.bulk-btn {
  display: flex; align-items: center; gap: 8px;
  padding: 10px 18px; border-radius: 12px; border: none;
  font-weight: 800; font-size: 12.5px; cursor: pointer;
  transition: all 0.3s; color: white;
  text-transform: uppercase; letter-spacing: 0.5px;
}
.bulk-divider { width: 1px; height: 24px; background: var(--border-thin); margin: 0 8px; }
 
.bulk-btn.activate { background: rgba(16, 185, 129, 0.12); color: #10b981; }
.bulk-btn.activate:hover { background: #10b981; color: white; transform: translateY(-2px); box-shadow: 0 8px 15px rgba(16, 185, 129, 0.2); }
.bulk-btn.deactivate { background: rgba(245, 158, 11, 0.12); color: #f59e0b; }
.bulk-btn.deactivate:hover { background: #f59e0b; color: white; transform: translateY(-2px); box-shadow: 0 8px 15px rgba(245, 158, 11, 0.2); }
.bulk-btn.delete { background: rgba(239, 68, 68, 0.1); color: #ef4444; }
.bulk-btn.delete:hover { background: #ef4444; color: white; transform: translateY(-2px); box-shadow: 0 8px 15px rgba(239, 68, 68, 0.2); }
 
.bulk-btn-close {
  background: var(--border-thin); border: none;
  width: 34px; height: 34px; border-radius: 10px;
  color: var(--r-text-sub); cursor: pointer; display: flex;
  align-items: center; justify-content: center; opacity: 0.7;
  transition: 0.3s;
}
.bulk-btn-close:hover { opacity: 1; background: var(--accent-soft); color: var(--accent); }

.btn-clear-selection {
  background: transparent; border: 1px solid var(--r-border);
  padding: 10px 16px; border-radius: 10px;
  font-size: 12px; font-weight: 700; color: var(--r-text-sub);
  cursor: pointer; transition: all 0.2s;
}
.btn-clear-selection:hover { background: rgba(0,0,0,0.03); color: var(--r-text-main); }

/* --- Animations --- */
.floating-slide-enter-active, .floating-slide-leave-active { transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1); }
.floating-slide-enter-from { opacity: 0; transform: translate(-50%, 40px) scale(0.9); }
.floating-slide-leave-to { opacity: 0; transform: translate(-50%, 20px) scale(0.95); }

.anim-reveal-up { animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes revealUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
