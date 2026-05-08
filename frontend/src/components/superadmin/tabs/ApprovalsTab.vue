<template>
  <div class="sa-tab-container">
    <div class="sa-tab-header">
      <div>
        <h2 class="sa-tab-title">Approbations en attente</h2>
        <p class="sa-tab-desc">Validez les nouvelles inscriptions d'entreprises pour activer leurs accès.</p>
      </div>

    </div>

    <div v-if="loading" class="sa-loading-state">
      <div class="sa-spinner"></div>
      <p>Chargement des demandes...</p>
    </div>

    <div v-else-if="pendingCompanies.length === 0" class="sa-empty-state glass-panel animate-fade-in">
      <div class="empty-icon-wrap">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
      </div>
      <h3>Aucune demande en attente</h3>
    </div>

    <div v-else class="approvals-grid">
      <div 
        v-for="company in pendingCompanies" 
        :key="company.id" 
        class="approval-card glass-panel hov-lift animate-slide-up"
      >
        <div class="card-glow"></div>
        <div class="approval-card-header">
          <div class="company-logo-mini">
            {{ company.name.charAt(0) }}
          </div>
          <div class="company-info">
            <h4>{{ company.name }}</h4>
            <span class="industry-tag">{{ company.industry || 'Secteur non précisé' }}</span>
          </div>
          <div class="date-tag">{{ formatDate(company.createdAt) }}</div>
        </div>

        <div class="approval-card-body">
          <div class="info-row">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
            <span>{{ company.contactEmail }}</span>
          </div>
          <div class="info-row">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
            <span>Taille : {{ company.size || 'N/A' }}</span>
          </div>
        </div>

        <div class="approval-card-actions">
          <button 
            @click="rejectCompany(company)" 
            class="sa-btn sa-btn-outline sa-btn-danger"
            :disabled="actionLoading === company.id"
          >
            Refuser
          </button>
          <button 
            @click="approveCompany(company)" 
            class="sa-btn sa-btn-accent"
            :disabled="actionLoading === company.id"
          >
            <span v-if="actionLoading === company.id" class="sa-spinner-mini"></span>
            <span v-else>Approuver</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import adminService from '@/services/adminService'
import { useAdminStore } from '@/stores/adminStore'

const adminStore = useAdminStore()
const pendingCompanies = ref([])
const loading = ref(true)
const actionLoading = ref(null)

const fetchPending = async () => {
  loading.value = true
  try {
    const res = await adminService.getPendingCompanies()
    pendingCompanies.value = res.data
  } catch (err) {
    console.error('Failed to fetch pending companies', err)
  } finally {
    loading.value = false
  }
}

const approveCompany = async (company) => {
  actionLoading.value = company.id
  try {
    await adminService.approveCompany(company.id)
    pendingCompanies.value = pendingCompanies.value.filter(c => c.id !== company.id)
    // Update global admin store if needed
    adminStore.fetchCompanies()
  } catch (err) {
    alert('Erreur lors de l\'approbation')
  } finally {
    actionLoading.value = null
  }
}

const rejectCompany = async (company) => {
  if (!confirm(`Êtes-vous sûr de vouloir rejeter l'inscription de ${company.name} ?`)) return
  
  actionLoading.value = company.id
  try {
    await adminService.rejectCompany(company.id)
    pendingCompanies.value = pendingCompanies.value.filter(c => c.id !== company.id)
  } catch (err) {
    alert('Erreur lors du rejet')
  } finally {
    actionLoading.value = null
  }
}

const formatDate = (dateStr) => {
  return new Date(dateStr).toLocaleDateString('fr-FR', {
    day: 'numeric',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(fetchPending)
</script>

<style scoped>
.sa-tab-container { animation: fadeIn 0.5s ease-out; }
.sa-tab-header { display: flex; justify-content: space-between; align-items: flex-start; margin-bottom: 40px; }
.sa-tab-title { font-size: 32px; font-weight: 800; margin-bottom: 8px; background: linear-gradient(135deg, #fff 0%, rgba(255,255,255,0.7) 100%); -webkit-background-clip: text; -webkit-text-fill-color: transparent; }
.sa-tab-desc { color: var(--text-muted); font-size: 15px; }

.header-stat-card { padding: 16px 24px; display: flex; flex-direction: column; align-items: center; min-width: 120px; }
.stat-value { font-size: 24px; font-weight: 800; color: var(--accent-color); }
.stat-label { font-size: 11px; text-transform: uppercase; letter-spacing: 1px; color: var(--text-muted); }

.approvals-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(340px, 1fr)); gap: 24px; }

.approval-card { 
  position: relative; padding: 24px; overflow: hidden; 
  display: flex; flex-direction: column; gap: 20px;
}
.card-glow {
  position: absolute; top: -50%; left: -50%; width: 200%; height: 200%;
  background: radial-gradient(circle, rgba(108, 99, 255, 0.05) 0%, transparent 70%);
  pointer-events: none;
}

.approval-card-header { display: flex; align-items: center; gap: 16px; position: relative; }
.company-logo-mini {
  width: 48px; height: 48px; border-radius: 14px; background: var(--accent-grad);
  display: flex; align-items: center; justify-content: center;
  color: white; font-weight: 800; font-size: 20px; box-shadow: 0 4px 12px rgba(108, 99, 255, 0.3);
}
.company-info h4 { font-size: 18px; font-weight: 700; margin: 0; color: white; }
.industry-tag { font-size: 12px; color: var(--accent-color); font-weight: 600; opacity: 0.8; }
.date-tag { margin-left: auto; font-size: 11px; color: var(--text-muted); }

.approval-card-body { display: flex; flex-direction: column; gap: 12px; }
.info-row { display: flex; align-items: center; gap: 10px; color: var(--text-muted); font-size: 14px; }
.info-row svg { width: 16px; opacity: 0.5; }

.approval-card-actions { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-top: 8px; }

/* SA Buttons */
.sa-btn {
  padding: 12px; border-radius: 12px; font-weight: 700; font-size: 14px;
  cursor: pointer; transition: all 0.3s; border: none; display: flex; align-items: center; justify-content: center; gap: 8px;
}
.sa-btn-accent { background: var(--accent-grad); color: white; box-shadow: 0 4px 12px rgba(108, 99, 255, 0.2); }
.sa-btn-accent:hover { transform: translateY(-2px); box-shadow: 0 8px 20px rgba(108, 99, 255, 0.4); }
.sa-btn-outline { background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1); color: white; }
.sa-btn-outline:hover { background: rgba(255,255,255,0.1); }
.sa-btn-danger:hover { background: rgba(239, 68, 68, 0.1); color: #ef4444; border-color: rgba(239, 68, 68, 0.2); }

.sa-spinner-mini { width: 16px; height: 16px; border: 2px solid rgba(255,255,255,0.3); border-top-color: white; border-radius: 50%; animation: spin 0.8s linear infinite; }

.sa-empty-state {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  padding: 80px; text-align: center; gap: 16px;
}
.empty-icon-wrap { 
  width: 80px; height: 80px; border-radius: 50%; background: rgba(108, 99, 255, 0.1);
  display: flex; align-items: center; justify-content: center; color: var(--accent-color); margin-bottom: 8px;
}
.empty-icon-wrap svg { width: 40px; }

@keyframes spin { to { transform: rotate(360deg); } }
</style>
