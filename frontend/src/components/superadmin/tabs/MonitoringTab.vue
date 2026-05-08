<template>
  <div class="sa-tab-container tab-content animate-fade-in premium-dashboard">
    <div class="sa-tab-header">
      <div>
        <h2 class="sa-tab-title">Supervision</h2>
        <p class="sa-tab-desc">Vue d'ensemble de la plateforme et indicateurs de performance en temps réel.</p>
      </div>

    </div>
    
    <!-- ─── Command Center : Critical Tasks ─── -->
    <div v-if="pendingCompaniesCount > 0 || newSupportCount > 0" class="command-center-banner animate-slide-down">
      <div class="banner-shimmer"></div>
      <div class="banner-left">
        <div class="banner-icon-pulse">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg>
        </div>
        <div class="banner-text">
          <div class="banner-title">Centre de Commandes</div>
          <div class="banner-desc">
            Analyse effectuée : <b>{{ pendingCompaniesCount + newSupportCount }}</b> alertes nécessitent votre attention.
          </div>
        </div>
      </div>
      <div class="banner-actions">
        <button v-if="pendingCompaniesCount > 0" class="action-pill approval" @click="$emit('navigate', 'approbations')">
          <span class="pill-dot"></span>
          {{ pendingCompaniesCount }} Validations
        </button>
        <button v-if="newSupportCount > 0" class="action-pill support" @click="$emit('navigate', 'support')">
          <span class="pill-dot"></span>
          {{ newSupportCount }} Tickets
        </button>
      </div>
    </div>
    
    <!-- ─── Accueil & En-tête ─── -->
    <!-- Row 1: Primary KPI Cards -->

    <!-- ─── Row 1 : Primary KPI Cards (Premium Style) ─── -->
    <div class="kpi-grid">
      <div 
        class="platinum-glass-card premium-kpi-card" 
        v-for="(m, i) in metrics" 
        :key="m.label" 
        :style="{'--delay': i * 0.1 + 's', '--card-accent': m.color}"
        @click="handleKPIClick(m.id)"
      >
        <div class="kpi-hologram"></div>
        <div class="kpi-icon-wrap" :style="{ background: m.iconBg, color: m.color }">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="kpi-icon"><path :d="m.iconPath"/></svg>
        </div>
        <div class="kpi-content">
          <div class="kpi-label">{{ m.label }}</div>
          <div class="kpi-value-container">
            <span class="kpi-value" :style="{ background: m.textGradient, WebkitBackgroundClip: 'text', WebkitTextFillColor: 'transparent' }">
              {{ m.value }}
            </span>
          </div>
        </div>
        <div class="kpi-mini-chart">
          <svg viewBox="0 0 100 30" preserveAspectRatio="none"><path d="M0,25 Q25,10 50,20 T100,5" fill="none" :stroke="m.color" stroke-width="2" opacity="0.3" /></svg>
        </div>
      </div>
    </div>

    <!-- ─── Row 2 : Health & Distribution Charts ─── -->
    <div class="chart-grid">
      <!-- Role Distribution Donut -->
      <div class="platinum-glass-card chart-card">
        <div class="card-header">
          <span class="card-title">Répartition des Rôles</span>
          <span class="card-title-badge violet">{{ totalUsers }} utilisateurs</span>
        </div>
        <div class="chart-container-small">
          <Doughnut :data="roleChartData" :options="donutOptions" />
        </div>
      </div>

      <!-- Industry Analysis Bar -->
      <div class="platinum-glass-card chart-card">
        <div class="card-header">
          <span class="card-title">Top Secteurs d'Activité</span>
          <span class="card-title-badge">{{ totalCompanies }} entreprises</span>
        </div>
        <div class="chart-container-small">
          <Bar :data="industryChartData" :options="barOptions" />
        </div>
      </div>
    </div>
 
    <!-- ─── Row 3 : Analytics Charts (Pulse & Distribution) ─── -->
    <div class="bottom-grid">
      <!-- Activity Pulse Chart -->
      <div class="platinum-glass-card chart-card analytics-card animate-slide-up">
        <div class="card-header">
          <div class="header-info">
            <span class="card-title">Flux du Marché (Offres vs Candidatures)</span>
            <span class="card-title-badge violet">Derniers 7 jours</span>
          </div>
          <button class="ghost-btn" @click="$emit('navigate', 'logs')">Explorer les logs →</button>
        </div>
        
        <div class="chart-container-main">
          <Line :data="marketPulseData" :options="lineOptions" />
        </div>
      </div>

      <!-- Platform Health Sentinel (Visual Matrix) -->
      <div class="platinum-glass-card chart-card analytics-card animate-slide-up" style="animation-delay: 0.2s">
        <div class="card-header">
          <span class="card-title">Sentinelle Système</span>
          <span class="card-title-badge">Online</span>
        </div>
        
        <div class="sentinel-grid">
          <div class="sentinel-item">
            <div class="sentinel-dot" :class="adminStore.health.api.status === 'Stable' ? 'active' : 'down'"></div>
            <div class="sentinel-info">
              <span class="sentinel-label">API Core</span>
              <span class="sentinel-val">{{ adminStore.health.api.message }}</span>
            </div>
          </div>
          <div class="sentinel-item">
            <div class="sentinel-dot" :class="adminStore.health.database.status === 'Stable' ? 'active' : 'down'"></div>
            <div class="sentinel-info">
              <span class="sentinel-label">Base de Données</span>
              <span class="sentinel-val">{{ adminStore.health.database.message }}</span>
            </div>
          </div>
          <div class="sentinel-item">
            <div class="sentinel-dot" :class="adminStore.health.ai.status === 'Stable' ? 'active' : 'down'"></div>
            <div class="sentinel-info">
              <span class="sentinel-label">Service IA</span>
              <span class="sentinel-val">{{ adminStore.health.ai.message }}</span>
            </div>
          </div>
          <div class="sentinel-item">
            <div class="sentinel-dot" :class="adminStore.health.storage.status === 'Stable' ? 'active' : 'down'"></div>
            <div class="sentinel-info">
              <span class="sentinel-label">Stockage</span>
              <span class="sentinel-val">{{ adminStore.health.storage.message }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineOptions({ name: 'MonitoringTab' })
import { computed } from 'vue'
import { useAdminStore } from '@/stores/adminStore'
import { Line, Doughnut, Bar } from 'vue-chartjs'
import { 
  Chart as ChartJS, Title, Tooltip, Legend, LineElement, LinearScale, PointElement, CategoryScale, ArcElement, BarElement, Filler 
} from 'chart.js'

ChartJS.register(Title, Tooltip, Legend, LineElement, LinearScale, PointElement, CategoryScale, ArcElement, BarElement, Filler)

const adminStore = useAdminStore()
const emit = defineEmits(['view-all-logs', 'navigate'])

const pendingCompaniesCount = computed(() => (adminStore.companies || []).filter(c => c.status === 0 || c.status === 'Pending').length)
const newSupportCount = computed(() => (adminStore.contactMessages || []).filter(m => m.status === 0 || m.status === 'New').length)

const handleKPIClick = (id) => {
  if (id === 'companies') emit('navigate', 'entreprises')
  if (id === 'candidates') emit('navigate', 'utilisateurs')
  if (id === 'offers') emit('navigate', 'entreprises')
  if (id === 'applications') emit('navigate', 'logs')
}

// KPI Metrics
const metrics = computed(() => {
  const s = adminStore.summary
  return [
    { id: 'companies', label: 'Entreprises', value: s.totalCompanies, iconBg: 'rgba(108,99,255,0.1)', color: '#6C63FF', glowColor: 'rgba(108,99,255,0.15)', textGradient: 'linear-gradient(135deg, #6C63FF, #4A47A3)', iconPath: 'M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6' },
    { id: 'candidates', label: 'Candidats', value: s.totalCandidates, iconBg: 'rgba(164,99,255,0.1)', color: '#A463FF', glowColor: 'rgba(164,99,255,0.15)', textGradient: 'linear-gradient(135deg, #A463FF, #7D48D1)', iconPath: 'M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z' },
    { id: 'offers', label: 'Offres Actives', value: s.totalJobOffers, iconBg: 'rgba(245,158,11,0.1)', color: '#f59e0b', glowColor: 'rgba(245,158,11,0.15)', textGradient: 'linear-gradient(135deg, #f59e0b, #d97706)', iconPath: 'M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8zM14 2v6h6M16 13H8M16 17H8M10 9H8' },
    { id: 'applications', label: 'Candidatures Reçues', value: s.totalJobApplications, iconBg: 'rgba(16,185,129,0.1)', color: '#10b981', glowColor: 'rgba(16,185,129,0.15)', textGradient: 'linear-gradient(135deg, #10b981, #059669)', iconPath: 'M22 11.08V12a10 10 0 1 1-5.93-9.14M22 4 12 14.01l-3-3' }
  ]
})

// --- Chart.js: Market Pulse ---
const marketPulseData = computed(() => {
  const daysShort = ['Dim', 'Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam']
  const now = new Date()
  const dataPoints = []
  
  for (let i = 6; i >= 0; i--) {
    const d = new Date()
    d.setDate(now.getDate() - i)
    d.setHours(0,0,0,0)
    dataPoints.push({ date: d, label: daysShort[d.getDay()], offers: 0, apps: 0 })
  }

  adminStore.logs.forEach(log => {
    const logDate = new Date(log.timestamp)
    logDate.setHours(0,0,0,0)
    const dp = dataPoints.find(p => p.date.getTime() === logDate.getTime())
    if (dp) {
      if (log.entityType === 'JobOffer') dp.offers++
      if (log.entityType === 'JobApplication') dp.apps++
    }
  })

  return {
    labels: dataPoints.map(p => p.label),
    datasets: [
      {
        label: 'Offres',
        data: dataPoints.map(p => p.offers),
        borderColor: '#6C63FF',
        backgroundColor: 'rgba(108, 99, 255, 0.1)',
        fill: true,
        tension: 0.4,
        pointRadius: 4,
        pointHoverRadius: 6
      },
      {
        label: 'Candidatures',
        data: dataPoints.map(p => p.apps),
        borderColor: '#10b981',
        backgroundColor: 'rgba(16, 185, 129, 0.1)',
        fill: true,
        tension: 0.4,
        pointRadius: 4,
        pointHoverRadius: 6
      }
    ]
  }
})

// --- Chart.js: Role Distribution ---
const roleChartData = computed(() => {
  const dist = adminStore.summary.roleDistribution || {}
  return {
    labels: ['Candidats', 'Admins', 'Recruteurs', 'Admin Plateformes'],
    datasets: [{
      data: [dist.Candidate || 0, dist.CompanyAdmin || 0, dist.Recruiter || 0, dist.SuperAdmin || 0],
      backgroundColor: ['#10b981', '#6C63FF', '#A463FF', '#ef4444'],
      hoverOffset: 12,
      borderWidth: 0
    }]
  }
})

// --- Chart.js: Industry Distribution ---
const industryChartData = computed(() => {
  const counts = {}
  adminStore.companies.forEach(c => {
    const ind = c.industry || 'Autre'
    counts[ind] = (counts[ind] || 0) + 1
  })
  const sorted = Object.entries(counts).sort((a,b) => b[1] - a[1]).slice(0, 5)
  return {
    labels: sorted.map(s => s[0]),
    datasets: [{
      label: 'Entreprises',
      data: sorted.map(s => s[1]),
      backgroundColor: 'rgba(108, 99, 255, 0.6)',
      borderRadius: 10,
      borderWidth: 0
    }]
  }
})

const lineOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false }, tooltip: { padding: 10, cornerRadius: 8 } },
  scales: {
    y: { beginAtZero: true, grid: { color: 'rgba(255,255,255,0.05)' }, ticks: { font: { size: 10 }, color: '#94a3b8' } },
    x: { grid: { display: false }, ticks: { font: { size: 10 }, color: '#94a3b8' } }
  }
}

const donutOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { position: 'right', labels: { color: '#94a3b8', font: { size: 11 }, padding: 15 } } },
  cutout: '70%'
}

const barOptions = {
  responsive: true,
  maintainAspectRatio: false,
  indexAxis: 'y',
  plugins: { legend: { display: false } },
  scales: {
    x: { beginAtZero: true, grid: { color: 'rgba(255,255,255,0.05)' }, ticks: { font: { size: 10 }, color: '#94a3b8' } },
    y: { grid: { display: false }, ticks: { font: { size: 11, weight: 'bold' }, color: '#f8fafc' } }
  }
}

const totalCompanies = computed(() => adminStore.summary.totalCompanies || 0)
const activeCompanies = computed(() => adminStore.summary.activeCompanies || 0)
const activeCompanyPct = computed(() => totalCompanies.value ? Math.round(activeCompanies.value / totalCompanies.value * 100) : 0)

const totalUsers = computed(() => adminStore.summary.totalUsers || 0)
const activeUsers = computed(() => adminStore.summary.activeUsers || 0)
const activeUserPct = computed(() => totalUsers.value ? Math.round(activeUsers.value / totalUsers.value * 100) : 0)

const formatTime = (timestamp) => {
  const date = new Date(timestamp)
  const now = new Date()
  const diff = Math.max(now - date, 0)
  const mins = Math.floor(diff / (1000 * 60))
  const hours = diff / (1000 * 60 * 60)
  if (mins < 1) return `À l'instant`
  if (hours < 1) return `Il y a ${mins} min`
  if (hours < 24) return `Il y a ${Math.floor(hours)} h`
  return date.toLocaleDateString('fr-FR', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })
}
</script>

<style scoped>
.premium-dashboard {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding-bottom: 30px;
}

.sa-tab-header {
  margin: -10px 0 0 0;
}

.sa-tab-title {
  font-size: 28px;
  font-weight: 900;
  color: var(--text-primary);
  margin: 0;
  letter-spacing: -1px;
}

.sa-tab-desc {
  font-size: 14px;
  color: var(--text-muted);
  margin: 4px 0 0 0;
}

/* ─── Command Center Banner ─── */
.command-center-banner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: rgba(108, 99, 255, 0.1);
  border: 1px solid rgba(108, 99, 255, 0.25);
  border-radius: 24px;
  padding: 20px 28px;
  margin-bottom: 12px;
  position: relative;
  overflow: hidden;
  backdrop-filter: blur(20px);
  box-shadow: 0 10px 40px -10px rgba(108, 99, 255, 0.2);
}

.banner-shimmer {
  position: absolute;
  top: 0; left: -100%;
  width: 50%; height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.1), transparent);
  animation: bannerShimmer 4s infinite linear;
}

@keyframes bannerShimmer {
  0% { left: -100%; }
  100% { left: 200%; }
}

.banner-left { display: flex; align-items: center; gap: 16px; z-index: 1; }

.banner-icon-pulse {
  width: 44px; height: 44px;
  background: linear-gradient(135deg, #6C63FF, #A463FF);
  border-radius: 14px;
  display: flex; align-items: center; justify-content: center;
  color: white;
  animation: bannerIconPulse 2s infinite alternate;
}
.banner-icon-pulse svg { width: 24px; }

@keyframes bannerIconPulse {
  from { transform: scale(1); box-shadow: 0 4px 15px rgba(108, 99, 255, 0.3); }
  to { transform: scale(1.08); box-shadow: 0 8px 25px rgba(108, 99, 255, 0.5); }
}

.banner-title { font-size: 16px; font-weight: 900; color: var(--text-primary); letter-spacing: -0.3px; }
.banner-desc { font-size: 13.5px; color: var(--text-muted); margin-top: 2px; }
.banner-desc b { color: #6C63FF; font-weight: 800; }

.banner-actions { display: flex; gap: 12px; z-index: 1; }

.action-pill {
  display: flex; align-items: center; gap: 10px;
  padding: 10px 18px; border-radius: 14px; border: 1px solid transparent;
  font-weight: 800; font-size: 13px; cursor: pointer; transition: all 0.3s;
}

.action-pill.approval { background: rgba(16, 185, 129, 0.1); color: #10b981; border-color: rgba(16, 185, 129, 0.1); }
.action-pill.support { background: rgba(108, 99, 255, 0.1); color: #6C63FF; border-color: rgba(108, 99, 255, 0.1); }
.action-pill:hover { transform: translateY(-3px) scale(1.05); }
.action-pill.approval:hover { background: #10b981; color: white; box-shadow: 0 8px 20px rgba(16, 185, 129, 0.3); }
.action-pill.support:hover { background: #6C63FF; color: white; box-shadow: 0 8px 20px rgba(108, 99, 255, 0.3); }

.pill-dot { width: 6px; height: 6px; border-radius: 50%; background: currentColor; }

/* ─── KPI Cards ─── */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 24px;
}
.premium-kpi-card {
  position: relative;
  display: flex; flex-direction: column; align-items: flex-start; gap: 16px;
  padding: 24px !important;
  background: var(--card-bg);
  border: 1px solid var(--border-thin) !important;
  box-shadow: 0 4px 20px rgba(108, 99, 255, 0.04);
  border-radius: 24px !important;
  transition: all 0.5s cubic-bezier(0.16, 1, 0.3, 1);
  overflow: hidden;
  cursor: pointer;
}

.kpi-hologram {
  position: absolute; inset: 0;
  background: linear-gradient(135deg, transparent 40%, rgba(255,255,255,0.05) 50%, transparent 60%);
  background-size: 200% 200%;
  transition: 0.8s;
  pointer-events: none;
}

.premium-kpi-card:hover {
  transform: translateY(-8px);
  border-color: var(--card-accent) !important;
  box-shadow: 0 20px 40px -10px rgba(108, 99, 255, 0.15);
}

.premium-kpi-card:hover .kpi-hologram { background-position: 100% 100%; }

.kpi-icon-wrap {
  width: 52px; height: 52px; border-radius: 16px;
  display: flex; align-items: center; justify-content: center;
  box-shadow: inset 0 0 12px rgba(255,255,255,0.1);
}

.kpi-icon { width: 24px; }

.kpi-content { position: relative; z-index: 2; width: 100%; }
.kpi-label { font-size: 11px; font-weight: 800; color: var(--text-muted); text-transform: uppercase; letter-spacing: 1px; margin-bottom: 2px; }
.kpi-value-container { display: flex; align-items: baseline; gap: 6px; }
.kpi-value { font-size: 44px; font-weight: 900; line-height: 1; letter-spacing: -1.5px; }

.kpi-mini-chart { position: absolute; bottom: 0; left: 0; right: 0; height: 40px; pointer-events: none; }
.kpi-mini-chart svg { width: 100%; height: 100%; }

/* ─── Chart Layout ─── */
.chart-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
}
.bottom-grid {
  display: grid;
  grid-template-columns: 2fr 1.2fr;
  gap: 24px;
}
.chart-card {
  padding: 24px !important;
  border-radius: 20px !important;
  border: 1px solid var(--border-thin) !important;
  background: var(--card-bg);
  box-shadow: var(--glass-shadow);
}

.card-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 16px; }
.card-title { font-size: 16px; font-weight: 800; color: var(--text-primary); }
.card-title-badge {
  font-size: 10px; font-weight: 800; text-transform: uppercase;
  padding: 3px 10px; border-radius: 100px;
  background: rgba(16, 185, 129, 0.1); color: #10b981;
}
.card-title-badge.violet { background: rgba(108, 99, 255, 0.1); color: #6C63FF; }

.chart-container-small { height: 220px; position: relative; }
.chart-container-main { height: 280px; position: relative; }

/* ─── Sentinel Grid ─── */
.sentinel-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
  margin-top: 10px;
}
.sentinel-item {
  display: flex; align-items: center; gap: 12px;
  padding: 16px; background: var(--bg-hover);
  border-radius: 16px; border: 1px solid var(--border-thin);
  transition: 0.3s;
}
.sentinel-dot { width: 10px; height: 10px; border-radius: 50%; position: relative; }
.sentinel-dot.active { background: #10b981; box-shadow: 0 0 10px rgba(16, 185, 129, 0.5); }
.sentinel-dot.active::after {
  content: ''; position: absolute; inset: -4px;
  border-radius: 50%; border: 2px solid #10b981;
  animation: sentinelPulse 2s infinite;
}
.sentinel-dot.down { background: #ef4444; box-shadow: 0 0 10px rgba(239, 68, 68, 0.5); }
.sentinel-dot.down::after {
  content: ''; position: absolute; inset: -4px;
  border-radius: 50%; border: 2px solid #ef4444;
  animation: sentinelPulse 2s infinite;
}
@keyframes sentinelPulse {
  0% { transform: scale(1); opacity: 0.5; }
  100% { transform: scale(2); opacity: 0; }
}
.sentinel-info { display: flex; flex-direction: column; gap: 2px; }
.sentinel-label { font-size: 10px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.5px; }
.sentinel-val { font-size: 13px; font-weight: 800; color: var(--text-primary); }

/* ─── Animations ─── */
.animate-slide-up { animation: slideUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes slideUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
.animate-fade-in { animation: fadeIn 0.6s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }

.ghost-btn { background: none; border: none; color: var(--accent-color); font-size: 13px; font-weight: 700; cursor: pointer; }
</style>