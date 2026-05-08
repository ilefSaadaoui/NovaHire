<template>
  <div class="sa-tab-container animate-fade-in">
    <div class="sa-tab-header">
      <div>
        <h2 class="sa-tab-title">Utilisateurs & Candidats</h2>
        <p class="sa-tab-desc">Gérez les comptes administratifs et suivez les candidatures à travers le réseau.</p>
      </div>

    </div>

    <!-- Section: Utilisateurs -->
    <div class="premium-card">
      <div class="card-header">
        <div class="header-left">
          <div class="header-icon violet">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
          </div>
          <div>
            <span class="card-title">Utilisateurs</span>
            <div class="card-sub">{{ filteredUsers.length }} / {{ adminStore.users.length }} utilisateurs</div>
          </div>
        </div>
        <div class="header-right">
          <div class="search-wrap">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="search-icon"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            <input type="text" v-model="userFilterSearch" placeholder="Rechercher..." class="search-input" />
          </div>
          <select v-model="userFilterRole" class="filter-select">
            <option value="">Tous les rôles</option>
            <option value="0">Admin Plateforme</option>
            <option value="1">CompanyAdmin</option>
            <option value="2">Recruiter</option>
          </select>
          <button class="primary-btn" @click="$emit('add-user')">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
            Ajouter
          </button>
        </div>
      </div>
      <div class="table-scroll">
        <table class="modern-table">
          <thead>
            <tr>
              <th style="width: 28%">
                <div class="th-content">Utilisateur <svg viewBox="0 0 24 24" class="sort-icon"><path d="M7 15l5 5 5-5M7 9l5-5 5 5"/></svg></div>
              </th>
              <th style="width: 25%">
                <div class="th-content">Coordonnées</div>
              </th>
              <th style="width: 15%">
                <div class="th-content">Entreprise</div>
              </th>
              <th style="width: 12%">
                <div class="th-content">Rôle</div>
              </th>
              <th style="width: 10%">
                <div class="th-content">Statut</div>
              </th>
              <th class="th-right" style="width: 10%">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(u, idx) in filteredUsers" :key="u.id" class="table-row" :style="{ animationDelay: (idx * 0.04) + 's' }">
              <td>
                <div class="user-cell">
                  <UserAvatar
                    :src="u.avatarUrl"
                    :name="u.firstName + ' ' + u.lastName"
                    :size="34"
                    radius="8px"
                    gradient="linear-gradient(135deg, #6C63FF, #A463FF)"
                  />
                  <div class="user-info-stack">
                    <div class="cell-name">{{ u.firstName }} {{ u.lastName }}</div>
                    <div class="cell-id">ID: {{ u.id.substring(0,8) }}</div>
                  </div>
                </div>
              </td>
              <td>
                <div class="email-cell">
                  <span class="email-text">{{ u.email }}</span>
                  <span class="email-verified-tag" v-if="u.isEmailVerified">Vérifié</span>
                </div>
              </td>
              <td>
                <span class="company-link" v-if="getCompanyName(u.companyId)">{{ getCompanyName(u.companyId) }}</span>
                <span class="td-muted" v-else>—</span>
              </td>
              <td><span class="role-tag" :class="'role-' + u.role">{{ getRoleName(u.role) }}</span></td>
              <td>
                <span class="status-badge" :class="u.isActive ? 'ok' : 'down'">
                  <span class="status-dot"></span>
                  {{ u.isActive ? 'Actif' : 'Inactif' }}
                </span>
              </td>
              <td class="th-right">
                <div class="action-group">
                  <button class="action-btn" title="Modifier" @click="$emit('edit-user', u)">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                  </button>
                  <button class="action-btn action-btn--delete" title="Supprimer" @click="$emit('delete-user', u)">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="!filteredUsers.length" class="empty-state">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
          <p>Aucun utilisateur trouvé</p>
        </div>
      </div>
    </div>

    <!-- Section: Candidats -->
    <div class="premium-card" style="margin-top: 20px;">
      <div class="card-header">
        <div class="header-left">
          <div class="header-icon green">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
          </div>
          <div>
            <span class="card-title">Candidats</span>
            <div class="card-sub">{{ filteredCandidateApplications.length }} candidature(s)</div>
          </div>
        </div>
        <div class="header-right">
          <div class="search-wrap">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="search-icon"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            <input type="text" v-model="candidateFilterSearch" placeholder="Nom, email, offre..." class="search-input" />
          </div>
          <select v-model="candidateFilterStage" class="filter-select">
            <option value="">Tous les statuts</option>
            <option value="new">Nouveau</option>
            <option value="screening">Préqualification</option>
            <option value="interview">Entretien</option>
            <option value="offer">Offre</option>
            <option value="rejected">Rejeté</option>
          </select>
        </div>
      </div>

      <div class="candidate-list" v-if="filteredCandidateApplications.length">
        <div v-for="(app, idx) in filteredCandidateApplications" :key="app.id" class="candidate-row" :style="{ animationDelay: (idx * 0.04) + 's' }">
          <div class="user-cell" style="min-width:200px">
            <UserAvatar
              :src="app.avatarUrl"
              :name="app.fullName"
              :size="36"
              radius="10px"
              gradient="linear-gradient(135deg, #10b981, #059669)"
            />
            <div>
              <div class="cell-name">{{ app.fullName }}</div>
              <div class="cell-sub">{{ app.email }}</div>
            </div>
          </div>
          <div class="candidate-job">
            <div class="cell-name">{{ app.jobTitle }}</div>
            <div class="cell-sub">{{ app.appliedAt }}</div>
          </div>
          <span class="stage-tag" :class="'stage-' + app.stageId">{{ app.stageLabel }}</span>
          <div class="action-group">
            <button class="action-btn" title="Voir le candidat" @click="$emit('open-candidate', app.candidateId)">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
            </button>
            <button class="action-btn action-btn--delete" title="Supprimer" @click="$emit('delete-candidate', app)">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
            </button>
          </div>
        </div>
      </div>

      <div v-else class="empty-state">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
        <p>{{ candidateFilterSearch || candidateFilterStage ? 'Aucun résultat correspondant' : 'Aucune candidature enregistrée' }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
defineOptions({ name: 'UsersTab' })
import { ref, computed } from 'vue'
import { useAdminStore } from '@/stores/adminStore'
import UserAvatar from '@/components/shared/UserAvatar.vue'

const adminStore = useAdminStore()

defineEmits(['add-user', 'edit-user', 'delete-user', 'open-candidate', 'delete-candidate'])

const userFilterSearch = ref('')
const userFilterRole = ref('')
const candidateFilterSearch = ref('')
const candidateFilterStage = ref('')

const ROLE_NAMES = { 0: 'Admin Plateforme', 1: 'CompanyAdmin', 2: 'Recruiter' }
const getRoleName = (role) => ROLE_NAMES[parseInt(role)] ?? (role || 'Inconnu')

const getCompanyName = (companyId) => {
  if (!companyId) return null
  return adminStore.companies.find(c => c.id === companyId)?.name || null
}

const filteredUsers = computed(() => {
  return adminStore.users.filter(u => {
    const q = userFilterSearch.value.toLowerCase()
    const matchesSearch = !q ||
      u.firstName.toLowerCase().includes(q) ||
      u.lastName.toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q)
    const matchesRole = !userFilterRole.value || u.role.toString() === userFilterRole.value
    return matchesSearch && matchesRole
  })
})

const mapStatusToStage = (status) => {
  const map = { 0:'new', 1:'screening', 2:'screening', 3:'interview', 4:'rejected', 5:'offer', 6:'interview' }
  const labels = { new:'Nouveau', screening:'Préqualification', interview:'Entretien', rejected:'Rejeté', offer:'Offre' }
  const id = map[status] ?? 'new'
  return { id, label: labels[id] }
}

const formatDateTime = (ts) => ts ? new Date(ts).toLocaleString('fr-FR') : '—'

const filteredCandidateApplications = computed(() => {
  const q = candidateFilterSearch.value.trim().toLowerCase()
  return (adminStore.jobApplications || [])
    .map(app => {
      const c = app.candidate || {}
      const stage = mapStatusToStage(app.status)
      const firstName = c.firstName || ''
      const lastName  = c.lastName || ''
      return {
        id: app.id,
        candidateId: c.id || app.candidateId,
        avatarUrl: c.avatarUrl || '',
        fullName: `${firstName} ${lastName}`.trim() || 'Candidat',
        email: c.email || '—',
        initials: `${firstName[0] || 'C'}${lastName[0] || ''}`.toUpperCase(),
        stageId: stage.id,
        stageLabel: stage.label,
        jobTitle: app.jobOffer?.title || 'Offre non définie',
        appliedAt: formatDateTime(app.appliedAt)
      }
    })
    .filter(app => {
      const matchSearch = !q || app.fullName.toLowerCase().includes(q) || app.email.toLowerCase().includes(q) || app.jobTitle.toLowerCase().includes(q)
      const matchStage  = !candidateFilterStage.value || app.stageId === candidateFilterStage.value
      return matchSearch && matchStage
    })
    .slice(0, 50)
})
</script>

<style scoped>
/* Base container */
.animate-fade-in { animation: fadeIn 0.5s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }

/* Premium card */
.premium-card {
  background: var(--card-bg);
  border-radius: 20px;
  border: 1px solid var(--border-thin);
  overflow: hidden;
  box-shadow: 0 4px 24px rgba(108, 99, 255, 0.07), 0 1px 4px rgba(0,0,0,0.04);
}

.card-header {
  padding: 20px 24px;
  border-bottom: 1px solid rgba(108, 99, 255, 0.08);
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  flex-wrap: wrap;
  background: linear-gradient(135deg, rgba(108,99,255,0.03) 0%, rgba(255,255,255,0) 100%);
}

.header-left { display: flex; align-items: center; gap: 14px; }

.header-icon {
  width: 38px; height: 38px;
  border-radius: 11px;
  display: flex; align-items: center; justify-content: center;
}
.header-icon.violet {
  background: linear-gradient(135deg, rgba(108,99,255,0.12), rgba(164,99,255,0.06));
  border: 1px solid rgba(108,99,255,0.15);
  color: #6C63FF;
}
.header-icon.green {
  background: linear-gradient(135deg, rgba(16,185,129,0.12), rgba(5,150,105,0.06));
  border: 1px solid rgba(16,185,129,0.15);
  color: #10b981;
}
.header-icon svg { width: 17px; }

.card-title { font-size: 15px; font-weight: 800; color: var(--text-primary); display: block; }
.card-sub { font-size: 11px; font-weight: 600; color: var(--text-muted); margin-top: 2px; }

.header-right { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }

.search-wrap { position: relative; display: flex; align-items: center; }
.search-icon { position: absolute; left: 10px; width: 14px; color: var(--text-muted); pointer-events: none; }
.search-input {
  padding: 7px 10px 7px 30px;
  background: var(--bg-hover);
  border: 1px solid var(--border-thin);
  border-radius: 10px;
  color: var(--text-primary);
  font-family: inherit;
  font-size: 13px; width: 190px;
  transition: all 0.2s;
}
.search-input:focus { outline: none; border-color: #6C63FF; box-shadow: 0 0 0 3px rgba(108,99,255,0.1); background: var(--card-bg); }

.filter-select {
  padding: 8px 36px 8px 14px;
  background: var(--bg-hover);
  border: 1px solid var(--border-thin);
  border-radius: 10px;
  color: var(--text-primary);
  font-family: inherit; font-size: 13px; cursor: pointer;
  outline: none;
  appearance: none;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%236C63FF' stroke-width='2.5'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' d='M19 9l-7 7-7-7' /%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 10px center;
  background-size: 14px;
  transition: all 0.2s;
}
.filter-select:focus { 
  border-color: #6C63FF; 
  box-shadow: 0 0 0 3px rgba(108,99,255,0.1); 
  background-color: var(--bg-base);
}
.filter-select option {
  background-color: var(--bg-base);
  color: var(--text-primary);
  padding: 10px;
}

.primary-btn {
  display: flex; align-items: center; gap: 6px;
  padding: 7px 16px;
  border-radius: 10px; border: none;
  background: linear-gradient(135deg, #6C63FF, #A463FF);
  color: white; font-weight: 700; font-size: 13px;
  cursor: pointer; transition: 0.25s; white-space: nowrap;
  box-shadow: 0 4px 12px rgba(108,99,255,0.25);
}
.primary-btn svg { width: 14px; }
.primary-btn:hover { transform: translateY(-2px); box-shadow: 0 6px 20px rgba(108,99,255,0.4); }

/* Table */
.table-scroll { overflow-x: auto; }
.modern-table { width: 100%; border-collapse: collapse; min-width: 700px; }
.modern-table thead tr {
  background: linear-gradient(to right, rgba(108,99,255,0.04), transparent);
}
.modern-table th {
  text-align: left; padding: 14px 20px;
  font-size: 11px; font-weight: 700; color: var(--text-muted);
  text-transform: uppercase; letter-spacing: 0.8px;
  border-bottom: 2px solid var(--border-thin);
  background: rgba(108, 99, 255, 0.015);
  position: sticky; top: 0; z-index: 10;
}
.th-content { display: flex; align-items: center; gap: 6px; }
.sort-icon { width: 12px; height: 12px; opacity: 0.3; transition: 0.2s; }
.modern-table th:hover .sort-icon { opacity: 0.8; color: var(--accent-color); }

.th-right { text-align: right !important; }
.modern-table td { padding: 14px 20px; border-bottom: 1px solid var(--border-thin); color: var(--text-primary); font-size: 13.5px; transition: 0.2s; vertical-align: middle; }
.td-muted { color: var(--text-muted) !important; font-size: 13px !important; }

.table-row {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  animation: rowReveal 0.4s cubic-bezier(0.16, 1, 0.3, 1) both;
  position: relative;
}
.table-row:hover { 
  background: rgba(108, 99, 255, 0.04); 
  transform: scale(1.002) translateX(4px);
}
.table-row:last-child td { border-bottom: none; }

@keyframes rowReveal {
  from { opacity: 0; transform: translateX(-8px); }
  to   { opacity: 1; transform: translateX(0); }
}

.user-cell { display: flex; align-items: center; gap: 12px; }
.user-info-stack { display: flex; flex-direction: column; gap: 1px; }
.cell-name { font-size: 13.5px; font-weight: 700; color: var(--text-primary); letter-spacing: -0.2px; }
.cell-id { font-size: 10px; font-family: monospace; color: var(--text-muted); opacity: 0.6; }
.cell-sub { font-size: 11px; color: var(--text-muted); margin-top: 1px; }

.email-cell { display: flex; flex-direction: column; gap: 2px; }
.email-text { font-size: 13px; font-weight: 500; color: var(--text-muted); }
.email-verified-tag { font-size: 9px; font-weight: 800; color: #10b981; text-transform: uppercase; letter-spacing: 0.5px; }

.company-link { font-size: 12.5px; font-weight: 600; color: #6C63FF; }

.role-tag {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 8px;
  font-size: 11px; font-weight: 800;
  border: 1px solid transparent;
  backdrop-filter: blur(4px);
}
.role-0 { background: rgba(239,68,68,0.08); color: #ef4444; border-color: rgba(239,68,68,0.2); }
.role-1 { background: rgba(108,99,255,0.08); color: #6C63FF; border-color: rgba(108,99,255,0.2); }
.role-2 { background: rgba(245,158,11,0.08); color: #f59e0b; border-color: rgba(245,158,11,0.2); }

/* Stage tags for candidates */
.stage-tag { display: inline-block; padding: 3px 10px; border-radius: 6px; font-size: 11px; font-weight: 700; }
.stage-new       { background: rgba(108,99,255,0.1); color: #6C63FF; }
.stage-screening { background: rgba(245,158,11,0.1); color: #f59e0b; }
.stage-interview { background: rgba(14,165,233,0.1); color: #0ea5e9; }
.stage-offer     { background: rgba(16,185,129,0.1); color: #10b981; }
.stage-rejected  { background: rgba(239,68,68,0.1);  color: #ef4444; }

.status-badge {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 4px 12px; border-radius: 100px;
  font-size: 11px; font-weight: 800;
  text-transform: uppercase; letter-spacing: 0.5px;
  border: 1px solid transparent;
}
.status-badge.ok { background: rgba(16,185,129,0.08); color: #10b981; border-color: rgba(16,185,129,0.2); }
.status-badge.down { background: rgba(239,68,68,0.08); color: #ef4444; border-color: rgba(239,68,68,0.2); }
.status-dot { width: 6px; height: 6px; border-radius: 50%; background: currentColor; box-shadow: 0 0 8px currentColor; }

.action-group { display: flex; align-items: center; justify-content: flex-end; gap: 4px; }
.action-btn {
  width: 30px; height: 30px; border-radius: 9px; border: none;
  background: transparent; color: #94a3b8; cursor: pointer;
  display: inline-flex; align-items: center; justify-content: center; transition: 0.15s;
}
.action-btn:hover { background: rgba(108,99,255,0.08); color: #6C63FF; }
.action-btn--delete:hover { background: rgba(239,68,68,0.08); color: #ef4444; }
.action-btn svg { width: 15px; }

/* Candidate list */
.candidate-list { display: flex; flex-direction: column; }
.candidate-row {
  display: flex; align-items: center; gap: 16px;
  padding: 14px 24px; border-bottom: 1px solid rgba(108,99,255,0.05);
  transition: background 0.2s;
  animation: rowReveal 0.4s cubic-bezier(0.16, 1, 0.3, 1) both;
}
.candidate-row:last-child { border-bottom: none; }
.candidate-row:hover { background: rgba(108,99,255,0.025); }
.candidate-job { flex: 1; min-width: 0; }

.empty-state { padding: 50px 24px; text-align: center; color: var(--text-muted); }
.empty-state svg { width: 36px; margin-bottom: 10px; opacity: 0.4; }
.empty-state p { font-size: 14px; margin: 0; }
</style>
