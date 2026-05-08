<template>
  <div class="dashboard-layout">
    <Sidebar active-item="rapports" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content" :class="{ 'ml-collapsed': sidebarCollapsed }">
      
      <!-- TOP BAR WITH GLASS EFFECT -->
      <header class="r-topbar anim-reveal-down">
        <div class="header-glass-card">
          <div class="header-left">
            <h1 class="premium-title"><span class="premium-title-themed">{{ $t('reports.title') }}</span></h1>
            <p class="subtitle">{{ $t('reports.subtitle') }}</p>
          </div>
          
          <div class="header-right">
            <div class="filter-controls">
              <div class="control-item">
                <label>{{ $t('createOffer.general.jobTitle') }}</label>
                <div class="select-wrapper">
                  <PremiumSelect 
                    v-model="selectedOfferId" 
                    :options="offerOptions" 
                    :placeholder="$t('reports.selectMission')"
                    @change="handleOfferChange"
                  />
                </div>
              </div>
            </div>
            
            <div class="action-divider"></div>
            
            <div class="action-buttons" v-if="selectedOfferId">
              <button class="btn-glass f-pdf" @click="handleExport('PDF')" :disabled="isExporting">
                <span class="btn-icon">📄</span>
                <span class="btn-text">{{ $t('reports.exportPdf') }}</span>
              </button>
              <button class="btn-glass f-excel" @click="handleExport('Excel')" :disabled="isExporting">
                <span class="btn-icon">💎</span>
                <span class="btn-text">{{ $t('reports.exportExcel') }}</span>
              </button>
            </div>
          </div>
        </div>
      </header>

      <div class="page-body">
        <!-- LOADING STATE -->
        <div v-if="loading" class="state-container">
          <div class="ai-scanning-animation">
            <div class="scan-line"></div>
            <div class="hex-grid"></div>
          </div>
          <p class="loading-text">{{ $t('reports.preparingDashboard') || 'Préparation de vos tableaux de bord recrutement...' }}</p>
        </div>

        <!-- EMPTY STATE (NO SELECTION) -->
        <div v-else-if="!selectedOfferId" class="state-container empty-premium anim-reveal-up">
          <div class="visual-empty">
            <div class="v-circle outer"></div>
            <div class="v-circle inner"></div>
            <div class="v-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.2" stroke-linecap="round" stroke-linejoin="round">
                <path d="M21.21 15.89A10 10 0 1 1 8 2.83M22 12A10 10 0 0 0 12 2v10z"/>
                <circle cx="12" cy="12" r="3" fill="currentColor" fill-opacity="0.1"/>
              </svg>
            </div>
          </div>
          <h2 class="empty-title">{{ $t('reports.noMission') }}</h2>
          <p class="empty-desc">{{ $t('reports.noMissionSub') }}</p>
          <div class="hint-pill">{{ $t('reports.selectMission').toUpperCase() }}</div>
        </div>

        <!-- MAIN DASHBOARD -->
        <div v-else class="dashboard-render">
          <!-- KPI GRID -->
          <div class="premium-kpi-grid">
            <div v-for="(kpi, idx) in kpis" :key="idx" class="p-kpi-card anim-reveal-up" :style="{ '--delay': (idx * 0.1) + 's' }">
              <div class="kpi-overlay" :style="{ background: kpi.color }"></div>
              <div class="kpi-content">
                <div class="kpi-top">
                  <span class="kpi-tag">{{ kpi.label }}</span>
                  <div class="kpi-trend-pill" :class="kpi.trend >= 0 ? 'pos' : 'neg'">
                    {{ kpi.trend >= 0 ? '+' : '' }}{{ kpi.trend }}%
                  </div>
                </div>
                <div class="kpi-value-row">
                  <span class="kpi-value">{{ kpi.value }}</span>
                </div>
                <div class="kpi-footer-visual">
                  <div class="p-track">
                    <div class="p-fill" :style="{ width: '85%', background: 'var(--accent)' }"></div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div class="dashboard-main-columns">
            <!-- LEFT COLUMN: CONVERSION -->
            <div class="glass-column anim-reveal-left">
              <div class="column-header">
                <h3>{{ $t('pipeline.title') }}</h3>
                <p>{{ $t('dashboard.recruiter.applicationFlow') }}</p>
              </div>
              
              <div class="pipeline-visual-v2">
                <div v-for="(step, idx) in pipelineData" :key="step.name" class="p-row">
                  <div class="p-info">
                    <span class="p-name">{{ formatStepName(step.name) }}</span>
                    <span class="p-count">{{ step.count }}</span>
                  </div>
                  <div class="p-bar-track">
                    <div class="p-bar-fill" :style="{ width: calculateWidth(step.count), background: getStepColor(step.name), '--delay': (idx * 0.05) + 's' }">
                      <div class="p-shine"></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- RIGHT COLUMN: AI SCORING -->
            <div class="glass-column anim-reveal-right" style="--delay: 0.1s">
              <div class="column-header">
                <h3>{{ $t('dashboard.admin.aiAnalyses') }}</h3>
                <p>{{ $t('pipeline.modal.sub') }}</p>
              </div>

              <div class="ai-score-donut">
                <div class="donut-wrapper">
                  <svg viewBox="0 0 100 100">
                    <circle cx="50" cy="50" r="45" class="d-bg" />
                    <circle v-for="(range, i) in scoreRanges" :key="i"
                            cx="50" cy="50" r="45"
                            class="d-segment"
                            :stroke="getRangeColor(range.class)"
                            :stroke-dasharray="getDashArray(range.count, i)"
                            :stroke-dashoffset="getDashOffset(i)" />
                  </svg>
                  <div class="donut-center">
                    <span class="d-val">{{ totalAnalyses }}</span>
                    <span class="d-tag">{{ $t('offers.table.candidates').toUpperCase() }}</span>
                  </div>
                </div>
                
                <div class="range-legend">
                  <div v-for="range in scoreRanges" :key="range.label" class="range-item">
                    <div class="r-color" :style="{ background: getRangeColor(range.class) }"></div>
                    <div class="r-text">{{ range.label }}</div>
                    <div class="r-val">{{ range.count }}</div>
                  </div>
                </div>
              </div>
            </div>

            <!-- FULL WIDTH: TOP SKILLS -->
            <div class="glass-column full-width anim-reveal-up" style="--delay: 0.2s">
              <div class="column-header">
                <h3>{{ $t('createOffer.general.skills') }}</h3>
                <p>{{ $t('dashboard.recruiter.topSkills') }}</p>
              </div>
              
              <div class="skills-galaxy">
                <div v-for="skill in stats.topSkills" :key="skill.name" 
                     class="g-skill-tag" 
                     :style="{ '--size': (skill.count / maxSkillCount), '--color': 'var(--accent)' }">
                  <span class="g-name">{{ skill.name }}</span>
                  <span class="g-cnt" :style="{ background: 'var(--accent-soft)', color: 'var(--accent)' }">{{ skill.count }}</span>
                </div>
                <div v-if="!stats.topSkills?.length" class="empty-mini">
                  {{ $t('dashboard.recruiter.awaitingAiAnalyses') }}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import Sidebar from '@/components/layout/Sidebar.vue'
import PremiumSelect from '@/components/common/PremiumSelect.vue'
import { useAuthStore } from '@/stores/authStore'
import { useToastStore } from '@/stores/toastStore'
import axios from '@/api/axios'

const { t } = useI18n()

const authStore = useAuthStore()
const toastStore = useToastStore()

const sidebarCollapsed = ref(false)
const loading = ref(false)
const isExporting = ref(false)
const selectedOfferId = ref('')
const offers = ref([])
const stats = ref({
  kpis: [],
  pipelineData: [],
  scoreRanges: [],
  topSkills: []
})

const offerOptions = computed(() => {
  return offers.value.map(o => ({ label: o.title, value: o.id }))
})

const kpis = computed(() => stats.value.kpis || [])
const pipelineData = computed(() => stats.value.pipelineData || [])
const scoreRanges = computed(() => stats.value.scoreRanges || [])
const totalAnalyses = computed(() => scoreRanges.value.reduce((acc, curr) => acc + curr.count, 0))
const maxSkillCount = computed(() => {
  if (!stats.value.topSkills?.length) return 1
  return Math.max(...stats.value.topSkills.map(s => s.count))
})

const fetchOffers = async () => {
  try {
    const res = await axios.get('/joboffer')
    offers.value = (res.data.data || []).filter(o => o.status === 1 || o.status === 'Published')
  } catch (err) {
    console.error('Fetch offers error:', err)
  }
}

const fetchAnalytics = async () => {
  if (!selectedOfferId.value) return
  loading.value = true
  try {
    const res = await axios.get(`/recruiter/analytics?jobOfferId=${selectedOfferId.value}`)
    stats.value = res.data
  } catch (err) {
    toastStore.show(t('notifications.updateError'), "error")
  } finally {
    loading.value = false
  }
}

const handleOfferChange = () => {
  fetchAnalytics()
}

const handleExport = async (format) => {
  if (!selectedOfferId.value) return
  isExporting.value = true
  toastStore.show(t('notifications.reportGenerating', { format }), "info")
  
  try {
    const endpoint = format === 'PDF' ? '/recruiter/export/pdf' : '/recruiter/export/excel'
    const extension = format === 'PDF' ? 'pdf' : 'xlsx'
    
    const response = await axios.get(`${endpoint}?jobOfferId=${selectedOfferId.value}`, {
      responseType: 'blob'
    })
    
    const blob = new Blob([response.data])
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    
    const offerTitle = offers.value.find(o => o.id === selectedOfferId.value)?.title || 'Report'
    link.setAttribute('download', `Rapport_Premium_${offerTitle.replace(/\s+/g, '_')}.${extension}`)
    
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    
    toastStore.show(t('notifications.reportGenerating', { format: format }).replace('Génération', 'Prêt'), "success")
  } catch (err) {
    console.error('Export error:', err)
    
    // Check if error response is a blob (common for download endpoints)
    if (err.response?.data instanceof Blob) {
      const reader = new FileReader()
      reader.onload = () => {
          try {
            const errorData = JSON.parse(reader.result)
            console.error('Parsed blob error (JSON):', errorData)
            toastStore.show(errorData.message || t('notifications.updateError'), "error")
          } catch (e) {
            console.error('Raw blob error:', reader.result)
            toastStore.show(reader.result.substring(0, 100) + '...', "error")
          }
      }
      reader.readAsText(err.response.data)
    } else {
      toastStore.show(t('notifications.updateError'), "error")
    }
  } finally {
    isExporting.value = false
  }
}

const formatStepName = (name) => {
  const map = {
    'Submitted': 'new',
    'UnderReview': 'analyzed',
    'Shortlisted': 'preselected',
    'Interview': 'interview',
    'Interviewed': 'interview',
    'Rejected': 'rejected',
    'Accepted': 'preselected'
  }
  return t('stages.' + (map[name] || name.toLowerCase()))
}

const getStepColor = (name) => {
  const map = {
    'Submitted': '#64748b',
    'UnderReview': '#f59e0b',
    'Shortlisted': '#0ea5e9',
    'Interview': '#6366f1',
    'Interviewed': '#a855f7',
    'Rejected': '#ef4444',
    'Accepted': '#10b981'
  }
  return map[name] || '#64748b'
}

const calculateWidth = (count) => {
  if (!pipelineData.value.length) return '0%'
  const max = Math.max(...pipelineData.value.map(p => p.count), 1)
  return `${(count / max) * 100}%`
}

const getRangeColor = (cls) => {
  if (cls === 'high') return '#10b981'
  if (cls === 'mid') return '#f59e0b'
  return '#ef4444'
}

const getDashArray = (count, index) => {
  const total = totalAnalyses.value || 1
  const circumference = 2 * Math.PI * 45
  const pct = (count / total)
  return `${pct * circumference} ${circumference}`
}

const getDashOffset = (index) => {
  const circumference = 2 * Math.PI * 45
  let offset = 0
  for (let i = 0; i < index; i++) {
    const count = scoreRanges.value[i].count
    offset += (count / (totalAnalyses.value || 1))
  }
  return -(offset * circumference)
}

onMounted(() => {
  fetchOffers()
})
</script>

<style>
/* Light-mode is the DEFAULT (no class on html/body).
   themeStore adds .dark-mode to <html> and <body> for dark mode. */

/* Light mode */
.dashboard-layout {
  --r-bg:          #f4f6fb;
  --r-card-bg:     #ffffff;
  --r-card-border: rgba(0, 0, 0, 0.08);
  --r-card-shadow: 0 8px 32px rgba(0, 0, 0, 0.10);
  --r-text:        #0f172a;
  --r-text-muted:  #475569;
  --r-text-faint:  #94a3b8;
  --r-track-bg:    rgba(0, 0, 0, 0.06);
  --r-item-bg:     rgba(0, 0, 0, 0.03);
  --r-item-border: rgba(0, 0, 0, 0.07);
  --r-btn-bg:      rgba(0, 0, 0, 0.04);
  --r-btn-border:  rgba(0, 0, 0, 0.12);
  --r-btn-text:    #0f172a;
  --r-divider:     rgba(0, 0, 0, 0.1);
  --r-donut-bg:    rgba(0, 0, 0, 0.05);
  --r-label-color: #64748b;
}

/* Dark mode — themeStore toggles .dark-mode on <html> + <body> */
html.dark-mode .dashboard-layout,
body.dark-mode  .dashboard-layout {
  --r-bg:          transparent;
  --r-card-bg:     rgba(255, 255, 255, 0.03);
  --r-card-border: rgba(255, 255, 255, 0.08);
  --r-card-shadow: 0 20px 40px rgba(0, 0, 0, 0.35);
  --r-text:        #ffffff;
  --r-text-muted:  rgba(255, 255, 255, 0.45);
  --r-text-faint:  rgba(255, 255, 255, 0.25);
  --r-track-bg:    rgba(255, 255, 255, 0.05);
  --r-item-bg:     rgba(255, 255, 255, 0.03);
  --r-item-border: rgba(255, 255, 255, 0.06);
  --r-btn-bg:      rgba(255, 255, 255, 0.06);
  --r-btn-border:  rgba(255, 255, 255, 0.12);
  --r-btn-text:    #ffffff;
  --r-divider:     rgba(255, 255, 255, 0.1);
  --r-donut-bg:    rgba(255, 255, 255, 0.04);
  --r-label-color: rgba(255, 255, 255, 0.3);
}
</style>

<style scoped>
/* ─── COMPONENT STYLES ─── */


.dashboard-layout {
  display: flex;
  width: 100%;
  min-height: 100vh;
  position: relative;
  overflow: visible;
  background: var(--r-bg);
}

.main-content {
  flex: 1;
  padding: 12px !important;
  position: relative;
  z-index: 5;
  margin-left: var(--sidebar-width);
  min-height: 100vh;
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.main-content.ml-collapsed { margin-left: var(--sidebar-collapsed); }

/* ─── TOP BAR ─── */
.dashboard-layout .r-topbar {
  margin-bottom: 12px !important;
}

.dashboard-layout .header-glass-card {
  border-radius: 10px !important;
  padding: 10px 15px !important;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.premium-badge {
  background: rgba(14, 165, 233, 0.1);
  color: #0ea5e9;
  font-size: 10px;
  font-weight: 800;
  letter-spacing: 2px;
  padding: 6px 12px;
  border-radius: 100px;
  display: inline-block;
  margin-bottom: 12px;
  border: 1px solid rgba(14, 165, 233, 0.2);
}

.dashboard-layout .premium-title {
  font-size: 16px !important;
  font-weight: 900;
  margin: 0;
  letter-spacing: -1px;
  color: var(--r-text);
}

.premium-title-themed {
  background: var(--accent-grad);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
  animation: shine 3s linear infinite;
}

@keyframes shine { to { background-position: 200% center; } }

.dashboard-layout .subtitle {
  font-size: 11px !important;
  margin: 2px 0 0 0 !important;
}
  font-weight: 500;
}

.header-right { display: flex; align-items: center; gap: 30px; }

.filter-controls label {
  display: block;
  font-size: 11px;
  font-weight: 800;
  color: var(--r-label-color);
  text-transform: uppercase;
  margin-bottom: 8px;
  letter-spacing: 1px;
}
.select-wrapper { width: 220px; }

.action-divider { width: 1px; height: 50px; background: var(--r-divider); }
.action-buttons { display: flex; gap: 12px; }

.btn-glass {
  background: var(--r-btn-bg);
  border: 1px solid var(--r-btn-border);
  color: var(--r-btn-text);
  padding: 12px 20px;
  border-radius: 14px;
  font-size: 12px;
  font-weight: 800;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 10px;
  transition: all 0.3s ease;
  letter-spacing: 0.5px;
}

.btn-glass:hover:not(:disabled) {
  background: var(--r-item-bg);
  transform: translateY(-2px);
  border-color: var(--r-card-border);
}

.btn-glass:disabled { opacity: 0.5; cursor: not-allowed; }
.f-pdf:hover  { box-shadow: 0 10px 20px rgba(239, 68, 68, 0.15);  border-color: #ef4444 !important; }
.f-excel:hover{ box-shadow: 0 10px 20px rgba(16, 185, 129, 0.15); border-color: #10b981 !important; }

/* ─── KPI CARDS ─── */
.premium-kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 24px;
}

.dashboard-layout .p-kpi-card {
  border-radius: 8px !important;
  padding: 8px 12px !important;
}

.p-kpi-card:hover { transform: translateY(-5px); box-shadow: 0 16px 40px rgba(0,0,0,0.15); }

.kpi-overlay {
  position: absolute;
  top: -100px; right: -100px;
  width: 200px; height: 200px;
  filter: blur(80px);
  opacity: 0.12;
  pointer-events: none;
}

.kpi-content { position: relative; z-index: 2; }
.kpi-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }

.kpi-tag {
  font-size: 10px;
  font-weight: 800;
  color: var(--r-text-faint);
  text-transform: uppercase;
  letter-spacing: 1.5px;
}

.kpi-trend-pill { font-size: 10px; font-weight: 900; padding: 4px 8px; border-radius: 6px; }
.kpi-trend-pill.pos { background: rgba(16, 185, 129, 0.12); color: #10b981; }
.kpi-trend-pill.neg { background: rgba(239, 68, 68, 0.12); color: #ef4444; }

.dashboard-layout .kpi-value { 
  font-size: 14px !important; 
}

.p-track { height: 6px; background: var(--r-track-bg); border-radius: 10px; margin-top: 16px; }
.p-fill  { height: 100%; border-radius: 10px; }

/* ─── MAIN COLUMNS ─── */
.dashboard-main-columns {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: 24px;
}

.dashboard-layout .glass-column {
  border-radius: 10px !important;
  padding: 12px !important;
}

.full-width { grid-column: span 2; }

.column-header h3 { font-size: 20px; font-weight: 900; margin: 0 0 10px 0; color: var(--r-text); }
.column-header p  { font-size: 14px; color: var(--r-text-muted); margin: 0 0 30px 0; font-weight: 500; }

/* ─── PIPELINE ─── */
.pipeline-visual-v2 { display: flex; flex-direction: column; gap: 20px; }
.p-row   { display: flex; flex-direction: column; gap: 10px; }
.p-info  { display: flex; justify-content: space-between; align-items: center; }
.p-name  { font-size: 13px; font-weight: 700; color: var(--r-text-muted); }
.p-count { font-size: 15px; font-weight: 900; color: var(--r-text); }

.p-bar-track {
  height: 6px;
  background: var(--r-track-bg);
  border-radius: 3px;
  overflow: visible;
  position: relative;
}
.p-bar-fill {
  height: 100%; border-radius: 3px; position: relative;
  transition: width 1.2s cubic-bezier(0.16, 1, 0.3, 1);
  animation: grow 1s ease forwards;
  animation-delay: var(--delay);
}

.p-shine {
  position: absolute; top: 0; left: 0; right: 0; bottom: 0;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.15), transparent);
  animation: moveShine 3s infinite linear;
}

@keyframes grow      { from { width: 0; } }
@keyframes moveShine { 0% { transform: translateX(-100%); } 100% { transform: translateX(100%); } }

/* ─── DONUT ─── */
.ai-score-donut { display: flex; flex-direction: column; align-items: center; gap: 40px; }
.donut-wrapper  { position: relative; width: 100px !important; height: 100px !important; }

.d-bg      { fill: none; stroke: var(--r-donut-bg); stroke-width: 5; }
.d-segment { fill: none; stroke-width: 7; stroke-linecap: round; transition: all 1s ease; transform: rotate(-90deg); transform-origin: center; }

.donut-center {
  position: absolute; top: 50%; left: 50%;
  transform: translate(-50%, -50%);
  text-align: center;
}
.dashboard-layout .d-val { font-size: 16px !important; }
.d-tag { font-size: 9px; font-weight: 800; color: var(--r-text-faint); text-transform: uppercase; margin-top: 3px; display: block; }

.range-legend { width: 100%; display: flex; flex-direction: column; gap: 6px; }
.range-item   { display: flex; align-items: center; gap: 8px; padding: 6px 10px; background: var(--r-item-bg); border: 1px solid var(--r-item-border); border-radius: 8px; }
.r-color      { width: 6px; height: 6px; border-radius: 2px; flex-shrink: 0; }
.r-text       { flex: 1; font-size: 11px; font-weight: 700; color: var(--r-text-muted); }
.r-val        { font-size: 12px; font-weight: 900; color: var(--r-text); }

/* ─── SKILLS GALAXY ─── */
.skills-galaxy { display: flex; flex-wrap: wrap; gap: 15px; }
.g-skill-tag {
  background: var(--r-item-bg);
  border: 1px solid var(--r-item-border);
  padding: 6px 12px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  gap: 8px;
  transition: all 0.3s ease;
  transform: scale(calc(var(--size) * 0.4 + 0.8));
  opacity: calc(var(--size) * 0.5 + 0.5);
}

.g-skill-tag:hover {
  background: var(--r-card-bg);
  transform: translateY(-5px) scale(calc(var(--size) * 0.4 + 0.9));
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.12);
  border-color: var(--accent, #0ea5e9);
}

.g-name { font-weight: 800; font-size: 11px; color: var(--r-text); }
.g-cnt  { font-size: 9px; font-weight: 900; background: rgba(14, 165, 233, 0.12); color: #0ea5e9; padding: 0px 5px; border-radius: 3px; }
.empty-mini { color: var(--r-text-muted); font-size: 14px; font-style: italic; }

/* ─── STATE SCREENS ─── */
.state-container { height: 60vh; display: flex; flex-direction: column; align-items: center; justify-content: center; text-align: center; }

.visual-empty { position: relative; width: 240px; height: 240px; margin-bottom: 40px; display: flex; align-items: center; justify-content: center; }
.v-circle       { position: absolute; border-radius: 50%; border: 1px dashed var(--r-item-border); }
.v-circle.outer { width: 100%; height: 100%; animation: spin-slow 20s linear infinite; }
.v-circle.inner { width: 70%; height: 70%; animation: spin-slow 15s linear reverse infinite; }
.v-icon         { width: 80px; height: 80px; color: var(--r-text-faint); }

@keyframes spin-slow { to { transform: rotate(360deg); } }

.empty-title { font-size: 32px; font-weight: 900; color: var(--r-text); margin-bottom: 12px; }
.empty-desc  { color: var(--r-text-muted); max-width: 480px; line-height: 1.6; font-size: 18px; margin-bottom: 30px; }
.hint-pill   { font-size: 11px; font-weight: 900; color: #0ea5e9; background: rgba(14, 165, 233, 0.12); padding: 8px 20px; border-radius: 100px; letter-spacing: 1px; }

/* ─── ANIMATIONS ─── */
@keyframes revealUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
.anim-reveal-up    { animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; animation-delay: var(--delay, 0s); }
.anim-reveal-left  { animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; animation-delay: var(--delay, 0s); }
.anim-reveal-right { animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; animation-delay: var(--delay, 0s); }
.anim-reveal-down  { animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; transform: translateY(-30px); }

/* ─── SCANNING LOADER ─── */
.ai-scanning-animation { width: 100px; height: 100px; position: relative; margin-bottom: 30px; }
.scan-line   { position: absolute; top: 0; left: 0; width: 100%; height: 2px; background: #0ea5e9; box-shadow: 0 0 15px #0ea5e9; animation: scan 2s ease-in-out infinite; }
@keyframes scan { 0%, 100% { top: 0; } 50% { top: 100%; } }
.loading-text { font-size: 14px; font-weight: 800; color: #0ea5e9; text-transform: uppercase; letter-spacing: 2px; }

/* ─── RESPONSIVE ─── */
@media (max-width: 1400px) {
  .premium-kpi-grid        { grid-template-columns: repeat(2, 1fr); }
  .dashboard-main-columns  { grid-template-columns: 1fr; }
  .full-width              { grid-column: span 1; }
}
</style>
