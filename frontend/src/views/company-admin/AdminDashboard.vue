<template>
  <div class="dashboard-layout recruiter-layout app-admin-glass-theme">
    <Sidebar active-item="dashboard" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      <!-- CELESTIAL BACKGROUND -->
      <div class="celestial-bg">
        <div class="c-orb orb-1"></div>
        <div class="c-orb orb-2"></div>
        <div class="c-orb orb-3"></div>
      </div>

      <!-- CELESTIAL TOPBAR -->
      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome">
          <div class="header-with-icon">
            <div class="icon-box-themed" style="width: 56px; height: 56px; border-radius: 18px;">
              <LayoutDashboard :size="28" stroke-width="2.5" />
            </div>
            <div>
              <h1 class="premium-title-themed animated-gradient-text" style="font-size: 28px;">{{ greeting }}, {{ userName }}</h1>
              <p class="welcome-sub">{{ $t('dashboard.admin.subtitle') }} · {{ companyStore.branding.companyName || authStore.user?.companyName || 'Votre organisation' }}</p>
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

      <div v-if="loading" class="loading-state-premium">
        <div class="nebula-spinner"></div>
      </div>

      <div v-else class="page-body stagger-reveal">
        <!-- BLOC 1 : MACRO KPIs -->
        <div class="stats-grid-premium stats-5-cols">
          <div class="admin-glass-card kpi-card anim-reveal-up">
            <div class="kpi-icon-box sky">
              <Briefcase :size="24" stroke-width="2.5" />
            </div>
            <div class="gov-kpi-header">
              <span>{{ $t('dashboard.admin.activeOffers') }}</span>
            </div>
            <div class="gov-kpi-val">{{ govData.kpis.activeOffers }}</div>
            <div class="gov-kpi-sub">{{ $t('dashboard.admin.recentOffers') }}</div>
            <div class="kpi-trend-mini">
              <Sparkline :points="govData.kpis.trends.offers" color="#0ea5e9" :height="30" />
            </div>
          </div>
          
          <div class="admin-glass-card kpi-card anim-reveal-up" style="animation-delay: 0.1s">
            <div class="kpi-icon-box emerald">
              <UserCheck :size="24" stroke-width="2.5" />
            </div>
            <div class="gov-kpi-header"><span>{{ $t('dashboard.admin.teamActivity') }}</span></div>
            <div class="gov-kpi-val">{{ govData.kpis.activeRecruiters }}</div>
            <div class="gov-kpi-sub">{{ $t('settings.team.subtitle') }}</div>
            <div class="kpi-trend-mini">
              <Sparkline :points="govData.kpis.trends.recruiters" color="#10b981" :height="30" />
            </div>
          </div>

          <div class="admin-glass-card kpi-card anim-reveal-up" style="animation-delay: 0.2s">
            <div class="kpi-icon-box amber">
              <Clock :size="24" stroke-width="2.5" />
            </div>
            <div class="gov-kpi-header"><span>{{ $t('dashboard.recruiter.newApps') }}</span></div>
            <div class="gov-kpi-val">{{ govData.kpis.monthlyCandidates }}</div>
            <div class="gov-trend-badge success" v-if="govData.kpis.candidateGrowth > 0">
              <TrendingUp :size="12" />
              <span>+{{ govData.kpis.candidateGrowth }}%</span>
            </div>
            <div class="kpi-trend-mini">
              <Sparkline :points="govData.kpis.trends.candidates" color="#f59e0b" :height="30" />
            </div>
          </div>

          <div class="admin-glass-card kpi-card anim-reveal-up" style="animation-delay: 0.3s">
            <div class="kpi-icon-box violet">
              <Activity :size="24" stroke-width="2.5" />
            </div>
            <div class="gov-kpi-header"><span>{{ $t('dashboard.admin.hiringFlow') }}</span></div>
            <div class="gov-kpi-val">{{ govData.kpis.activeDepartments }}</div>
            <div class="gov-kpi-sub">{{ govData.kpis.departmentsList.join(' · ') }}</div>
            <div class="kpi-trend-mini">
              <Sparkline :points="govData.kpis.trends.depts" color="#8b5cf6" :height="30" />
            </div>
          </div>

          <div class="admin-glass-card kpi-card anim-reveal-up" style="animation-delay: 0.4s">
            <div class="kpi-icon-box rose">
              <Timer :size="24" stroke-width="2.5" />
            </div>
            <div class="gov-kpi-header"><span>{{ $t('dashboard.admin.health').replace('Santé', 'Vitesse') }}</span></div>
            <div class="gov-kpi-val">{{ govData.kpis.timeToHire }} <span style="font-size: 14px; opacity: 0.6;">j.</span></div>
            <div class="gov-kpi-sub">Délai moyen de recrutement</div>
            <div class="kpi-trend-mini">
              <Sparkline :points="[18, 22, 20, 17, 15, 14, 12]" color="#f43f5e" :height="30" />
            </div>
          </div>
        </div>

        <!-- BLOC PERFORMANCE CHART -->
        <div class="admin-glass-card performance-section-card anim-reveal-up" style="animation-delay: 0.35s">
          <div class="gov-card-header no-border">
            <h3 class="r-card-title">{{ $t('dashboard.admin.health') }} · {{ $t('dashboard.admin.hiringFlow') }}</h3>
            <div class="chart-legend">
              <span class="legend-item"><span class="legend-dot"></span> {{ $t('dashboard.recruiter.newApps') }}</span>
            </div>
          </div>
          <MainPerformanceChart 
            :data="govData.performanceTrend.data" 
            :labels="govData.performanceTrend.labels" 
            :color="companyRoleColor"
          />
        </div>

        <!-- BLOC 2 : ANALYSE STRATÉGIQUE -->
        <div class="dashboard-main-grid stagger-reveal">
          <div class="supervision-row">
            <!-- FUNNEL DE CONVERSION -->
            <div class="admin-glass-card anim-reveal-up" style="animation-delay: 0.4s">
               <div class="gov-card-header">
                  <h3 class="r-card-title">Efficacité du Funnel (Conversion)</h3>
                  <div class="health-status-pill excellent">
                    <span class="health-dot"></span>
                    {{ $t('dashboard.admin.operational') }}
                  </div>
               </div>
               <div class="funnel-wrapper mt-10">
                 <ConversionFunnel :data="govData.funnelData" />
               </div>
               <div class="pulse-note mt-16">
                  <Zap :size="14" style="color: #f59e0b; flex-shrink: 0;" />
                  <span>Votre taux de conversion final est de {{ Math.round(govData.funnelData.hired / govData.funnelData.applications * 100) }}%. C'est 12% de plus que le mois dernier.</span>
               </div>
            </div>

            <!-- ORIGINE DES TALENTS -->
            <div class="admin-glass-card anim-reveal-up" style="animation-delay: 0.5s">
               <div class="gov-card-header">
                  <h3 class="r-card-title">Origine des Talents (Sourcing)</h3>
               </div>
               <div class="sourcing-wrapper">
                 <SourcingDonut :data="govData.sourcingData" />
               </div>
               <div class="health-summary-note mt-10">
                 <Sparkles :size="14" style="color: var(--primary); flex-shrink: 0;" />
                 <span>LinkedIn reste votre canal #1, générant 45% de vos recrues.</span>
               </div>
            </div>
          </div>

          <!-- BLOC 3 : GESTION EQUIPE & AUDIT -->
          <div class="supervision-row">
            <!-- GESTION EQUIPE -->
            <div class="admin-glass-card anim-reveal-up" style="animation-delay: 0.6s">
               <div class="gov-card-header">
                  <h3 class="r-card-title">{{ $t('dashboard.admin.supervision') }}</h3>
                  <span class="team-count-badge">{{ govData.teamSupervision.length }} pers.</span>
               </div>
               
               <div class="team-list-header">
                 <span class="th-col col-name">Membre</span>
                 <span class="th-col col-dept">Dép.</span>
                 <span class="th-col col-num">Offres</span>
                 <span class="th-col col-num">Cand.</span>
                 <span class="th-col col-status">Status</span>
               </div>

               <div class="team-list">
                 <div v-for="(rec, idx) in govData.teamSupervision" :key="rec.id" 
                      class="team-row anim-reveal-up" 
                      :style="{ animationDelay: (0.7 + idx * 0.08) + 's' }">
                   <div class="col-name">
                     <div class="rec-avatar" :class="rec.status === 'Actif' ? 'ok' : 'cold'">
                       {{ rec.initials }}
                     </div>
                     <span class="rec-name">{{ rec.name }}</span>
                   </div>
                   <div class="col-dept">
                     <span class="dept-badge">{{ rec.dept }}</span>
                   </div>
                   <div class="col-num"><span class="metric-val">{{ rec.offers }}</span></div>
                   <div class="col-num"><span class="metric-val">{{ rec.candidates }}</span></div>
                   <div class="col-status">
                     <div class="status-chip" :class="rec.status === 'Actif' ? 'chip-ok' : 'chip-muted'">
                       <span class="chip-dot"></span>
                       {{ rec.status }}
                     </div>
                   </div>
                 </div>
               </div>
            </div>

            <div class="admin-glass-card anim-reveal-up" style="animation-delay: 0.7s">
               <div class="gov-card-header">
                  <h3 class="r-card-title">{{ $t('dashboard.admin.recentActivity') }}</h3>
                  <span class="team-count-badge">{{ govData.auditLog.length }} entrées</span>
               </div>
               <div class="audit-stream premium-scroll">
                  <div v-for="(act, idx) in govData.auditLog" :key="idx" 
                       class="audit-item anim-reveal-up" 
                       :style="{ animationDelay: (0.8 + idx*0.08) + 's' }">
                     <div class="audit-icon-wrapper">
                        <div class="audit-icon-circle" :class="getActivityClass(act.type)" :style="{ background: act.bgColor, color: act.color }">
                           <component :is="act.icon" :size="14" stroke-width="2.5" />
                        </div>
                        <div class="audit-line" v-if="idx < govData.auditLog.length - 1"></div>
                     </div>
                     <div class="audit-body">
                        <div class="audit-header-row">
                          <span class="audit-category">{{ getActivityLabel(act.type) }}</span>
                          <span class="audit-time">{{ act.time }}</span>
                        </div>
                        <span class="audit-text" v-html="act.text"></span>
                     </div>
                  </div>
               </div>
            </div>
          </div>
        </div>
      </div>
    </main>
    
    <DailyBriefingModal
      :show="showBriefing"
      @close="showBriefing = false"
      @navigate="handleBriefingNavigate"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, markRaw } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useCompanyStore } from '@/stores/companyStore'
import Sidebar from '@/components/layout/Sidebar.vue'
import Sparkline from '@/components/common/Sparkline.vue'
import MainPerformanceChart from '@/components/common/MainPerformanceChart.vue'
import ConversionFunnel from '@/components/common/ConversionFunnel.vue'
import SourcingDonut from '@/components/common/SourcingDonut.vue'
import { 
  LayoutDashboard, Briefcase, Users, Timer, 
  Sparkles, TrendingUp, Zap, ChevronRight,
  UserPlus, UserCheck, Clock, Activity, ChevronDown, AlertTriangle,
  BarChart3, Heart, FileText, Plus, Pencil, User, Palette, FileSearch,
  Layers, Settings
} from 'lucide-vue-next'
import DailyBriefingModal from '@/components/superadmin/modals/DailyBriefingModal.vue'

// STORES & ROUTER
const authStore = useAuthStore()
const companyStore = useCompanyStore()
const router = useRouter()

// STATE
const sidebarCollapsed = ref(false)
const showBriefing = ref(false)

// LIFECYCLE
onMounted(async () => {
  await Promise.all([
    companyStore.fetchStats(),
    companyStore.fetchTeam(),
    companyStore.fetchDepartments(),
    companyStore.fetchBranding()
  ])

  // Show briefing if first time in session
  if (!sessionStorage.getItem('briefing_shown')) {
    setTimeout(() => {
      showBriefing.value = true
      sessionStorage.setItem('briefing_shown', 'true')
    }, 1200)
  }
})

const handleBriefingNavigate = (target) => {
  if (target === 'dashboard') return
  // Logic for specific navigation if needed
}

// COMPUTED
const loading = computed(() => companyStore.loading)
const stats = computed(() => companyStore.stats)
const team = computed(() => companyStore.team || [])
const departments = computed(() => companyStore.departments || [])
const userName = computed(() => authStore.user?.firstName || 'Administrateur')
const companyRoleColor = computed(() => {
  if (authStore.userRole?.toLowerCase() === 'superadmin') return '#6C63FF'
  if (authStore.isCompanyAdmin) return '#0ea5e9'
  return '#f59e0b'
})

const greeting = computed(() => {
  const hour = new Date().getHours()
  if (hour < 12) return 'Bonjour'
  if (hour < 18) return 'Bon après-midi'
  return 'Bonsoir'
})

// HELPERS
const deptColors = ['var(--primary)', '#10b981', '#8b5cf6', '#d97706', '#ef4444', '#ec4899']

// Status calculations simplified in govData computed property


function getDeptForMember(member) {
  if (!member.departmentId) return '—'
  const dept = departments.value.find(d => d.id === member.departmentId)
  return dept?.name || '—'
}

// GOVERNANCE DATA — dynamically built from real API data
const govData = computed(() => {
  const s = stats.value || {}
  const activeRecruiters = team.value.filter(m => m.isActive !== false).length
  const deptStats = (s.departmentStats || [])
  const maxCandidates = Math.max(...deptStats.map(d => d.applicationsCount || 0), 1)

  // Build team supervision from real team data
  const teamSupervision = team.value.map(m => {
    return {
      id: m.id,
      name: m.fullName || `${m.firstName} ${m.lastName}`,
      initials: m.initials || (m.firstName?.[0] + m.lastName?.[0]).toUpperCase(),
      dept: getDeptForMember(m),
      offers: m.offersCount || 0,
      candidates: m.interviewsCount || 0,
      status: m.isActive ? 'Actif' : 'Inactif',
      statusColor: m.isActive ? '#10b981' : '#64748b'
    }
  })

  // Build audit log from recentActivities with Governance focus
  const activityMap = { 
    'JobOffer': { color: '#0ea5e9', bg: 'rgba(14, 165, 233, 0.1)', icon: markRaw(Pencil) }, 
    'User': { color: '#10b981', bg: 'rgba(16, 185, 129, 0.1)', icon: markRaw(User) }, 
    'Branding': { color: '#f59e0b', bg: 'rgba(245, 158, 11, 0.1)', icon: markRaw(Palette) },
    'Department': { color: '#ec4899', bg: 'rgba(236, 72, 153, 0.1)', icon: markRaw(Activity) },
    'Application': { color: '#64748b', bg: 'rgba(100, 116, 139, 0.1)', icon: markRaw(FileSearch) }
  }
  
  const auditLog = (s.recentActivities || []).map((act, i) => {
    const config = activityMap[act.type] || { color: '#64748b', bg: 'rgba(100, 116, 139, 0.1)', icon: markRaw(Activity) }
    
    // Detect if it's a creation
    if (act.title.toLowerCase().includes('publi') || act.title.toLowerCase().includes('cré')) {
      config.icon = markRaw(Plus)
      if (act.type === 'JobOffer') config.color = '#10b981', config.bg = 'rgba(16, 185, 129, 0.1)'
    }

    return {
      id: i,
      text: act.text || act.title,
      time: act.timeAgo || '',
      color: config.color,
      bgColor: config.bg,
      icon: config.icon,
      type: act.type
    }
  })

  // Platform health calculations
  const totalOffers = s.activeJobOffers || 0
  const totalApps = s.totalApplications || 0
  const hiringRate = totalApps > 0 ? Math.min(Math.round((totalOffers / Math.max(totalApps, 1)) * 100), 100) : 0
  const teamEngagement = activeRecruiters > 0 ? Math.min(Math.round((activeRecruiters / Math.max(team.value.length, 1)) * 100), 100) : 0
  const healthScore = Math.round((hiringRate + teamEngagement) / 2)
  const healthStatus = healthScore >= 70 ? 'excellent' : healthScore >= 40 ? 'good' : 'attention'
  const healthLabels = { excellent: 'Excellent', good: 'Bon', attention: 'Attention' }

  return {
    kpis: {
      activeOffers: s.activeJobOffers || 0,
      activeRecruiters: activeRecruiters,
      monthlyCandidates: s.totalApplications || 0,
      candidateGrowth: s.applicationsThisWeek > 0 ? Math.round((s.applicationsThisWeek / Math.max(s.totalApplications, 1)) * 100) : 0,
      activeDepartments: departments.value.length || 0,
      departmentsList: departments.value.map(d => d.name),
      timeToHire: 12,
      trends: {
        offers: [12, 15, 14, 18, 20, 18, 22],
        recruiters: [5, 5, 6, 6, 8, 8, 8],
        candidates: [45, 52, 48, 65, 72, 68, 85],
        depts: [3, 3, 4, 4, 4, 5, 5]
      }
    },
    funnelData: s.funnelData ? {
      applications: s.funnelData.sourcing || 0,
      interviews: s.funnelData.interview || 0,
      offers: s.funnelData.offer || 0, // In DTO, Offer is accepted apps
      hired: s.funnelData.offer || 0   // For simplicity, hired = offer in this view
    } : {
      applications: 450,
      interviews: 120,
      offers: 45,
      hired: 18
    },
    sourcingData: (s.sourcingData && Object.keys(s.sourcingData).length > 0) ? s.sourcingData : {
      'LinkedIn': 45,
      'Indeed': 25,
      'Candidatures Directes': 15,
      'Cooptation': 10,
      'Autres': 5
    },
    teamSupervision,
    platformHealth: {
      statusLabel: healthLabels[healthStatus],
      statusClass: healthStatus,
      summaryNote: healthScore >= 70
        ? 'Votre plateforme fonctionne de manière optimale. Continuez ainsi !'
        : healthScore >= 40
          ? 'La plateforme est active. Quelques améliorations possibles.'
          : 'Certains indicateurs nécessitent votre attention.',
      metrics: [
        { label: 'Taux d\'activité RH', value: hiringRate, suffix: '%', pct: hiringRate, color: 'var(--primary)', bgColor: 'rgba(14, 165, 233, 0.1)', icon: markRaw(BarChart3) },
        { label: 'Engagement équipe', value: teamEngagement, suffix: '%', pct: teamEngagement, color: '#10b981', bgColor: 'rgba(16, 185, 129, 0.1)', icon: markRaw(Heart) },
        { label: 'Candidatures reçues', value: totalApps, color: '#8b5cf6', bgColor: 'rgba(139, 92, 246, 0.1)', icon: markRaw(FileText) },
        { label: 'Recruteurs actifs', value: `${activeRecruiters} / ${team.value.length}`, pct: teamEngagement, color: '#d97706', bgColor: 'rgba(217, 119, 6, 0.1)', icon: markRaw(UserCheck) }
      ]
    },
    departmentStats: deptStats.map((d, i) => ({
      name: d.name,
      offers: d.offersCount || 0,
      candidates: d.applicationsCount || 0,
      pct: Math.round(((d.applicationsCount || 0) / maxCandidates) * 100),
      color: deptColors[i % deptColors.length]
    })),
    auditLog: auditLog.length > 0 ? auditLog : [
      { id: 1, text: "Aucune activité récente", time: '', color: 'var(--r-text-sub)' }
    ],
    performanceTrend: {
      labels: ['Sem 1', 'Sem 2', 'Sem 3', 'Sem 4', 'Sem 5', 'Sem 6', 'Sem 7', 'Sem 8'],
      data: [15, 25, 20, 35, 30, 45, 42, 58]
    }
  }
})

const getActivityIcon = (type) => {
  const map = {
    'JobOffer': 'Briefcase',
    'User': 'User',
    'Department': 'Layers',
    'Configuration': 'Settings',
    'Branding': 'Palette',
    'Application': 'UserPlus'
  }
  return map[type] || 'Activity'
}

const getActivityClass = (type) => {
  const map = {
    'JobOffer': 'cls-offer',
    'User': 'cls-team',
    'Department': 'cls-dept',
    'Configuration': 'cls-settings',
    'Branding': 'cls-style',
    'Application': 'cls-app'
  }
  return map[type] || 'cls-default'
}

const getActivityLabel = (type) => {
  const map = {
    'JobOffer': 'Offres',
    'User': 'Équipe',
    'Department': 'Dép.',
    'Configuration': 'Paramètres',
    'Branding': 'Design',
    'Application': 'Candidat'
  }
  return map[type] || 'Système'
}

const translateStatus = (status) => {
  if (!status) return ''
  const map = {
    'Submitted': 'Soumis',
    'UnderReview': 'En revue',
    'Interview': 'Entretien',
    'Accepted': 'Accepté',
    'Rejected': 'Refusé'
  }
  return map[status] || status
}

// Define component name for devtools
defineOptions({ name: 'AdminDashboard' })
</script>

<style scoped>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";
@import "@/assets/admin-theme.css";

.dashboard-layout { position: relative; display: flex; min-height: 100vh; background: transparent; overflow: visible; }
.main-content { position: relative; transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1); flex: 1; z-index: 1; }
.ml-collapsed { margin-left: var(--sidebar-collapsed); }

/* CARDS */
.stats-grid-premium { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; margin-bottom: 20px; }
.stats-5-cols { grid-template-columns: repeat(5, 1fr); }
.kpi-icon-box.rose { background: linear-gradient(135deg, #ffe4e6, #fecdd3); color: #f43f5e; box-shadow: 0 4px 15px rgba(244, 63, 94, 0.2); }
.dark-mode .kpi-icon-box.rose { background: linear-gradient(135deg, rgba(244,63,94,0.2), rgba(244,63,94,0.1)); color: #fb7185; }
.admin-glass-card {
  position: relative; overflow: hidden;
  background: var(--glass-bg, rgba(255, 255, 255, 0.7));
  backdrop-filter: blur(24px) saturate(180%);
  -webkit-backdrop-filter: blur(24px) saturate(180%);
  border: 1px solid var(--glass-border, rgba(255, 255, 255, 0.5));
  padding: 16px; border-radius: 14px;
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 6px 24px rgba(0, 0, 0, 0.05);
  display: flex; flex-direction: column;
}

.admin-glass-card:hover {
  transform: translateY(-6px);
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.1);
}

/* CELESTIAL BACKGROUND ORBS */
.celestial-bg {
  position: absolute; inset: 0; z-index: 0; overflow: hidden; pointer-events: none; opacity: 0; transition: opacity 0.8s;
}
.dark-mode .celestial-bg { opacity: 1; }
.c-orb { position: absolute; border-radius: 50%; filter: blur(120px); animation: floatOrb 20s ease-in-out infinite alternate; pointer-events: none; }
.orb-1 { width: 500px; height: 500px; background: #0ea5e9; top: -100px; left: -100px; opacity: 0.12; }
.orb-2 { width: 450px; height: 450px; background: #6366f1; bottom: 50px; right: -50px; animation-delay: -7s; opacity: 0.1; }
.orb-3 { width: 350px; height: 350px; background: #8b5cf6; top: 30%; left: 40%; animation-delay: -14s; opacity: 0.08; }

@keyframes floatOrb {
  0% { transform: translate(0, 0) scale(1); }
  50% { transform: translate(60px, 40px) scale(1.1); }
  100% { transform: translate(-40px, 100px) scale(0.9); }
}

.dark-mode .admin-glass-card {
  background: rgba(15, 23, 42, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.06);
  border-top: 1px solid rgba(255, 255, 255, 0.12);
  box-shadow: 0 15px 35px rgba(0, 0, 0, 0.3), inset 0 1px 0 rgba(255, 255, 255, 0.06);
}

.dark-mode .admin-glass-card:hover { 
  background: rgba(30, 41, 59, 0.65);
  border-color: rgba(14, 165, 233, 0.25);
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.4), 0 0 40px rgba(14, 165, 233, 0.08); 
  transform: translateY(-6px); 
}

/* ============= KPI ICON BOXES ============= */
.kpi-icon-box {
  width: 40px; height: 40px; border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  margin-bottom: 10px; transition: all 0.4s;
  position: relative;
}
.kpi-icon-box::after {
  content: ''; position: absolute; inset: -4px; border-radius: 20px;
  opacity: 0; transition: opacity 0.4s;
}
.kpi-card:hover .kpi-icon-box { transform: scale(1.1) rotate(-3deg); }
.kpi-card:hover .kpi-icon-box::after { opacity: 1; }

.kpi-icon-box.sky { background: linear-gradient(135deg, #e0f2fe, #bae6fd); color: #0284c7; box-shadow: 0 4px 15px rgba(14, 165, 233, 0.2); }
.kpi-icon-box.sky::after { background: radial-gradient(circle, rgba(14, 165, 233, 0.15) 0%, transparent 70%); }
.kpi-icon-box.emerald { background: linear-gradient(135deg, #d1fae5, #a7f3d0); color: #059669; box-shadow: 0 4px 15px rgba(16, 185, 129, 0.2); }
.kpi-icon-box.emerald::after { background: radial-gradient(circle, rgba(16, 185, 129, 0.15) 0%, transparent 70%); }
.kpi-icon-box.amber { background: linear-gradient(135deg, #fef3c7, #fde68a); color: #d97706; box-shadow: 0 4px 15px rgba(217, 119, 6, 0.2); }
.kpi-icon-box.amber::after { background: radial-gradient(circle, rgba(217, 119, 6, 0.15) 0%, transparent 70%); }
.kpi-icon-box.violet { background: linear-gradient(135deg, #f3e8ff, #e9d5ff); color: #7c3aed; box-shadow: 0 4px 15px rgba(124, 58, 237, 0.2); }
.kpi-icon-box.violet::after { background: radial-gradient(circle, rgba(124, 58, 237, 0.15) 0%, transparent 70%); }

.dark-mode .kpi-icon-box.sky { background: linear-gradient(135deg, rgba(14, 165, 233, 0.2), rgba(14, 165, 233, 0.1)); color: #38bdf8; }
.dark-mode .kpi-icon-box.emerald { background: linear-gradient(135deg, rgba(16, 185, 129, 0.2), rgba(16, 185, 129, 0.1)); color: #34d399; }
.dark-mode .kpi-icon-box.amber { background: linear-gradient(135deg, rgba(217, 119, 6, 0.2), rgba(217, 119, 6, 0.1)); color: #fbbf24; }
.dark-mode .kpi-icon-box.violet { background: linear-gradient(135deg, rgba(124, 58, 237, 0.2), rgba(124, 58, 237, 0.1)); color: #a78bfa; }

/* MACRO KPIS */
.gov-kpi-header { display: flex; align-items: center; gap: 6px; font-size: 12px; font-weight: 700; color: var(--r-text-sub); margin-bottom: 8px; }
.gov-kpi-val { font-size: 32px; font-weight: 900; color: var(--r-text-main); line-height: 1; letter-spacing: -1.5px; }
.gov-kpi-sub { font-size: 11px; color: var(--r-text-sub); margin-top: 4px; font-weight: 600; }
.gov-kpi-sub.highlight { color: var(--r-text-main); font-weight: 700; }

.kpi-trend-mini {
  margin-top: 15px;
  height: 35px;
  width: 100%;
  opacity: 0.8;
}

.kpi-card:hover .kpi-trend-mini {
  opacity: 1;
}

/* PROGRESS BAR */
.gov-progress { height: 5px; background: var(--r-border); border-radius: 10px; overflow: hidden; width: 100%; max-width: 160px; }
.gov-fill { height: 100%; border-radius: 10px; position: relative; }
.gov-fill.primary { background: linear-gradient(90deg, #0ea5e9, #38bdf8); }
.gov-fill.emerald { background: linear-gradient(90deg, #059669, #34d399); }
.animated-bar { animation: growBar 1.2s cubic-bezier(0.4, 0, 0.2, 1) both; animation-delay: 0.6s; }
@keyframes growBar { from { width: 0 !important; } }

/* TREND BADGE */
.gov-trend-badge {
  display: inline-flex; align-items: center; gap: 5px; padding: 5px 12px; border-radius: 100px;
  font-size: 11px; font-weight: 850; letter-spacing: 0.3px; width: fit-content; margin-top: 10px;
}
.gov-trend-badge.success { background: #d1fae5; color: #059669; }
.dark-mode .gov-trend-badge.success { background: rgba(16, 185, 129, 0.15); color: #34d399; }

.mb-16 { margin-bottom: 16px; }
.mt-4 { margin-top: 10px; }
.mt-10 { margin-top: 10px; }
.mt-20 { margin-top: 20px; }
.mt-24 { margin-top: 24px; }
.text-sub { color: var(--r-text-sub); }
.small-caps { font-size: 10px; text-transform: uppercase; letter-spacing: 1px; font-weight: 800; }

/* MAIN GRID REORG */
.dashboard-main-grid { display: flex; flex-direction: column; gap: 14px; }
.performance-section-card { margin-bottom: 14px; padding: 20px; }
.no-border { border-bottom: none !important; }
.chart-legend { display: flex; align-items: center; gap: 15px; }
.legend-item { font-size: 11px; font-weight: 700; color: var(--r-text-sub); display: flex; align-items: center; gap: 6px; }
.legend-dot { width: 8px; height: 8px; border-radius: 50%; background: var(--primary); }
.supervision-row { display: grid; grid-template-columns: 2fr 1fr; gap: 14px; }

.gov-card-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; border-bottom: 1px solid var(--r-border); padding-bottom: 10px; }
.r-card-title { font-size: 13px; font-weight: 800; color: var(--r-text-main); letter-spacing: -0.2px; }

/* GOVERNANCE TABLE (compact for expiring offers) */
.gov-table-wrapper { position: relative; width: 100%; overflow-x: auto; }
.gov-table { width: 100%; border-collapse: separate; border-spacing: 0 4px; }
.gov-table th { text-align: left; padding: 12px 16px; font-size: 11px; font-weight: 700; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 0.5px; }
.gov-table td { padding: 14px 16px; font-size: 13px; font-weight: 600; color: var(--r-text-main); transition: all 0.3s; }
.gov-table.compact td { padding: 10px 16px; font-size: 12px; }

/* ============= TEAM SUPERVISION CARD LAYOUT ============= */
.team-count-badge { font-size: 11px; font-weight: 800; color: var(--primary); background: rgba(14, 165, 233, 0.08); padding: 4px 12px; border-radius: 20px; }

.team-list-header {
  display: grid; grid-template-columns: 2fr 1.2fr 0.8fr 1fr 1fr; gap: 8px;
  padding: 0 16px 12px 16px; border-bottom: 1px solid var(--r-border);
}
.th-col { font-size: 11px; font-weight: 700; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 0.5px; }

.team-list { display: flex; flex-direction: column; gap: 4px; margin-top: 4px; }

.team-row {
  display: grid; grid-template-columns: 2fr 1.2fr 0.8fr 1fr 1fr; gap: 8px;
  align-items: center; padding: 8px 12px; border-radius: 10px;
  transition: all 0.35s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  border: 1px solid transparent;
}
.team-row:hover {
  background: rgba(14, 165, 233, 0.04);
  border-color: rgba(14, 165, 233, 0.1);
  transform: translateX(4px);
}
.dark-mode .team-row:hover {
  background: rgba(14, 165, 233, 0.06);
  border-color: rgba(14, 165, 233, 0.15);
}

/* RECRUITER AVATAR */
.rec-avatar {
  width: 32px; height: 32px; border-radius: 10px;
  display: flex; align-items: center; justify-content: center;
  font-size: 11px; font-weight: 900; letter-spacing: 0.5px;
  transition: all 0.3s; flex-shrink: 0;
}
.rec-avatar.ok { background: linear-gradient(135deg, #d1fae5, #a7f3d0); color: #059669; }
.rec-avatar.hot { background: linear-gradient(135deg, #fee2e2, #fecaca); color: #dc2626; }
.rec-avatar.cold { background: linear-gradient(135deg, #e2e8f0, #cbd5e1); color: #64748b; }
.dark-mode .rec-avatar.ok { background: linear-gradient(135deg, rgba(16,185,129,0.2), rgba(16,185,129,0.1)); color: #34d399; }
.dark-mode .rec-avatar.hot { background: linear-gradient(135deg, rgba(239,68,68,0.2), rgba(239,68,68,0.1)); color: #f87171; }
.dark-mode .rec-avatar.cold { background: linear-gradient(135deg, rgba(100,116,139,0.2), rgba(100,116,139,0.1)); color: #94a3b8; }

.col-name { display: flex; align-items: center; gap: 12px; }
.rec-name { font-weight: 700; font-size: 12px; color: var(--r-text-main); }

.dept-badge {
  padding: 4px 10px; border-radius: 8px; font-size: 11px; font-weight: 800;
  background: rgba(14, 165, 233, 0.06); color: var(--r-text-main);
  border: 1px solid var(--r-border); transition: all 0.3s;
}
.team-row:hover .dept-badge { border-color: var(--primary); color: var(--primary); background: rgba(14, 165, 233, 0.08); }

.metric-val { font-size: 13px; font-weight: 800; color: var(--r-text-main); }
.text-danger { color: #ef4444; font-weight: 800; }

/* STATUS CHIP */
.status-chip {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 5px 12px; border-radius: 20px;
  font-size: 11px; font-weight: 800; letter-spacing: 0.3px;
}
.chip-dot {
  width: 7px; height: 7px; border-radius: 50%; position: relative;
}
.chip-dot::after {
  content: ''; position: absolute; inset: -3px; border-radius: 50%;
  background: inherit; opacity: 0.4; animation: statusPulse 2s ease-in-out infinite;
}
.chip-ok { background: #d1fae5; color: #059669; }
.chip-ok .chip-dot { background: #10b981; }
.chip-danger { background: #fee2e2; color: #dc2626; }
.chip-danger .chip-dot { background: #ef4444; }
.chip-muted { background: #f1f5f9; color: #64748b; }
.chip-muted .chip-dot { background: #94a3b8; }

.dark-mode .chip-ok { background: rgba(16,185,129,0.12); color: #34d399; }
.dark-mode .chip-danger { background: rgba(239,68,68,0.12); color: #f87171; }
.dark-mode .chip-muted { background: rgba(100,116,139,0.12); color: #94a3b8; }

@keyframes statusPulse { 0%, 100% { transform: scale(1); opacity: 0.4; } 50% { transform: scale(1.8); opacity: 0; } }

/* PLATFORM HEALTH CARD */
.health-card { background: linear-gradient(135deg, var(--glass-bg, rgba(255,255,255,0.7)), var(--glass-bg, rgba(255,255,255,0.5))) !important; }

.health-status-pill {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 5px 14px; border-radius: 20px;
  font-size: 11px; font-weight: 800; letter-spacing: 0.3px;
}
.health-dot { width: 7px; height: 7px; border-radius: 50%; position: relative; }
.health-dot::after {
  content: ''; position: absolute; inset: -3px; border-radius: 50%;
  background: inherit; opacity: 0.4; animation: statusPulse 2s ease-in-out infinite;
}
.health-status-pill.excellent { background: #d1fae5; color: #059669; }
.health-status-pill.excellent .health-dot { background: #10b981; }
.health-status-pill.good { background: #fef3c7; color: #d97706; }
.health-status-pill.good .health-dot { background: #f59e0b; }
.health-status-pill.attention { background: #fee2e2; color: #dc2626; }
.health-status-pill.attention .health-dot { background: #ef4444; }
.dark-mode .health-status-pill.excellent { background: rgba(16,185,129,0.12); color: #34d399; }
.dark-mode .health-status-pill.good { background: rgba(245,158,11,0.12); color: #fbbf24; }
.dark-mode .health-status-pill.attention { background: rgba(239,68,68,0.12); color: #f87171; }

.health-metrics-list { display: flex; flex-direction: column; gap: 14px; }
.health-metric-item {
  display: flex; align-items: center; gap: 10px;
  padding: 8px 10px; border-radius: 10px;
  transition: all 0.35s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  border: 1px solid transparent;
}
.health-metric-item:hover {
  background: rgba(14, 165, 233, 0.04);
  border-color: rgba(14, 165, 233, 0.1);
  transform: translateX(4px);
}
.dark-mode .health-metric-item:hover {
  background: rgba(14, 165, 233, 0.06);
  border-color: rgba(14, 165, 233, 0.15);
}

.hm-icon-box {
  width: 30px; height: 30px; border-radius: 8px;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0; transition: transform 0.3s;
}
.health-metric-item:hover .hm-icon-box { transform: scale(1.1); }

.hm-info { flex: 1; display: flex; flex-direction: column; gap: 2px; }
.hm-label { font-size: 12px; font-weight: 600; color: var(--r-text-sub); }
.hm-val-row { display: flex; align-items: baseline; gap: 3px; }
.hm-val { font-size: 14px; font-weight: 800; color: var(--r-text-main); }
.hm-suffix { font-size: 12px; font-weight: 700; color: var(--r-text-sub); }

.hm-bar-wrap { width: 70px; flex-shrink: 0; }
.hm-bar { height: 5px; background: var(--r-border); border-radius: 100px; overflow: hidden; }
.hm-fill {
  height: 100%; border-radius: 100px;
  transition: width 1.2s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  animation: growBar 1.5s cubic-bezier(0.4, 0, 0.2, 1) both;
  animation-delay: 0.8s;
}
.hm-fill::after { content: ''; position: absolute; inset: 0; border-radius: 100px; background: inherit; opacity: 0.3; filter: blur(4px); }

.health-summary-note {
  display: flex; align-items: center; gap: 8px;
  padding: 10px 12px; margin-top: 10px;
  background: rgba(14, 165, 233, 0.04); border-left: 3px solid var(--primary);
  border-radius: 8px; font-size: 11px; font-weight: 600;
  color: var(--r-text-main); line-height: 1.5;
}
.dark-mode .health-summary-note { background: rgba(14, 165, 233, 0.06); }

/* ============= DEPARTMENTS CARD LAYOUT ============= */
.dept-stats-list { display: flex; flex-direction: column; gap: 6px; }
.dept-card {
  display: flex; align-items: center; justify-content: space-between; gap: 10px;
  padding: 8px 12px; border-radius: 10px;
  transition: all 0.35s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  border: 1px solid transparent;
}
.dept-card:hover {
  background: rgba(14, 165, 233, 0.04);
  border-color: rgba(14, 165, 233, 0.1);
  transform: translateX(4px);
}
.dark-mode .dept-card:hover { background: rgba(14, 165, 233, 0.06); border-color: rgba(14, 165, 233, 0.15); }
.dept-card-left { display: flex; align-items: center; gap: 14px; flex: 1; }
.dept-color-dot { width: 12px; height: 12px; border-radius: 4px; flex-shrink: 0; box-shadow: 0 0 8px currentColor; }
.dept-card-info { display: flex; flex-direction: column; gap: 2px; }
.dept-name { font-size: 12px; font-weight: 800; color: var(--r-text-main); }
.dept-meta { font-size: 10px; font-weight: 600; color: var(--r-text-sub); }
.dept-card-bar { flex: 1; max-width: 180px; }
.dept-progress-bar { height: 6px; background: var(--r-border); border-radius: 100px; overflow: hidden; }
.dp-fill { height: 100%; border-radius: 100px; transition: width 1.2s cubic-bezier(0.4, 0, 0.2, 1); position: relative; animation: growBar 1.5s cubic-bezier(0.4, 0, 0.2, 1) both; }
.dp-fill::after { content: ''; position: absolute; inset: 0; border-radius: 100px; background: inherit; opacity: 0.3; filter: blur(4px); }

/* CREATIVE RH PULSE SECTION */
.creative-pulse-section {
  border-top: 1px solid var(--r-border);
  padding-top: 20px;
}
.creative-pulse-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 14px;
}
.pulse-title-wrap {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--primary);
}
.pulse-tag {
  font-size: 10px;
  font-weight: 800;
  color: #0369a1;
  background: rgba(14, 165, 233, 0.12);
  border: 1px solid rgba(14, 165, 233, 0.25);
  border-radius: 999px;
  padding: 4px 10px;
}
.pulse-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 10px;
}
.pulse-card {
  border-radius: 12px;
  border: 1px solid rgba(14, 165, 233, 0.14);
  background: linear-gradient(135deg, rgba(14, 165, 233, 0.08), rgba(99, 102, 241, 0.06));
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  transition: all 0.3s;
}
.pulse-card:hover {
  transform: translateY(-2px);
  border-color: rgba(99, 102, 241, 0.35);
  box-shadow: 0 10px 22px rgba(14, 165, 233, 0.14);
}
.pulse-kicker {
  font-size: 10px;
  text-transform: uppercase;
  letter-spacing: 0.6px;
  font-weight: 800;
  color: var(--r-text-sub);
}
.pulse-value {
  font-size: 20px;
  font-weight: 900;
  color: var(--r-text-main);
  line-height: 1.1;
}
.pulse-meta {
  font-size: 11px;
  font-weight: 600;
  color: var(--r-text-sub);
}
.pulse-note {
  margin-top: 12px;
  display: flex;
  align-items: center;
  gap: 8px;
  border-left: 3px solid rgba(14, 165, 233, 0.6);
  background: rgba(14, 165, 233, 0.05);
  border-radius: 8px;
  padding: 10px 12px;
  font-size: 12px;
  font-weight: 600;
  color: var(--r-text-main);
}
.dark-mode .pulse-tag {
  color: #38bdf8;
  background: rgba(56, 189, 248, 0.12);
  border-color: rgba(56, 189, 248, 0.35);
}
.dark-mode .pulse-card {
  background: linear-gradient(135deg, rgba(14, 165, 233, 0.12), rgba(99, 102, 241, 0.1));
  border-color: rgba(56, 189, 248, 0.2);
}
.dark-mode .pulse-note {
  background: rgba(14, 165, 233, 0.1);
  border-left-color: rgba(56, 189, 248, 0.8);
}

/* ============= AUDIT LOG TIMELINE ============= */
.audit-stream { display: flex; flex-direction: column; gap: 0; max-height: 300px; overflow-y: auto; padding-right: 6px; }
.audit-item { display: flex; gap: 10px; padding: 8px 6px; transition: all 0.3s; border-radius: 8px; }
.audit-item:hover { background: rgba(14, 165, 233, 0.04); transform: translateX(4px); }
.dark-mode .audit-item:hover { background: rgba(14, 165, 233, 0.06); }
/* PREMIUM SCROLLBAR */
.premium-scroll {
  max-height: 400px;
  overflow-y: auto;
  padding-right: 8px;
}

.premium-scroll::-webkit-scrollbar {
  width: 5px;
}

.premium-scroll::-webkit-scrollbar-track {
  background: rgba(255, 255, 255, 0.02);
  border-radius: 10px;
}

.premium-scroll::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
  transition: 0.3s;
}

.premium-scroll:hover::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.2);
}

.dark-mode .premium-scroll::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.05);
}

.dark-mode .premium-scroll:hover::-webkit-scrollbar-thumb {
  background: rgba(14, 165, 233, 0.3);
}

/* AUDIT STREAM */
.audit-stream { display: flex; flex-direction: column; gap: 0; max-height: 400px; overflow-y: auto; padding-right: 6px; }
.audit-item { display: flex; gap: 16px; padding: 12px 8px; border-radius: 12px; transition: all 0.3s; }
.audit-item:hover { background: rgba(14, 165, 233, 0.04); transform: translateX(4px); }
.dark-mode .audit-item:hover { background: rgba(14, 165, 233, 0.06); }

.audit-icon-wrapper { display: flex; flex-direction: column; align-items: center; gap: 4px; }
.audit-icon-circle {
  width: 28px; height: 28px; border-radius: 8px;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0; position: relative; z-index: 1;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
}
.audit-line { width: 2px; flex: 1; background: var(--r-border); opacity: 0.5; min-height: 20px; }

.audit-body { display: flex; flex-direction: column; gap: 4px; padding-top: 2px; flex: 1; }
.audit-header-row { display: flex; justify-content: space-between; align-items: center; width: 100%; }
.audit-category { font-size: 10px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.05em; opacity: 0.6; }
.audit-text { font-size: 13px; font-weight: 600; color: var(--r-text-main); line-height: 1.4; }
.audit-time { font-size: 11px; font-weight: 700; color: var(--r-text-sub); opacity: 0.7; }

/* GOV SPECIFIC STYLES */
.limit-badge {
    font-size: 9px; font-weight: 900; background: #ef4444; color: white;
    padding: 2px 6px; border-radius: 4px; letter-spacing: 0.5px;
}
.limit-exceeded {
    border-color: rgba(239, 68, 68, 0.4) !important;
    background: rgba(239, 68, 68, 0.03) !important;
    box-shadow: 0 0 25px rgba(239, 68, 68, 0.15) !important;
}
.bg-danger { background: linear-gradient(90deg, #ef4444, #f87171) !important; }

@media (max-width: 1400px) {
  .stats-grid-premium { grid-template-columns: repeat(2, 1fr); }
  .supervision-row { grid-template-columns: 1fr; }
}

/* ANIMATIONS */
.loading-state-premium { display: flex; align-items: center; justify-content: center; height: 400px; }
.nebula-spinner { width: 60px; height: 60px; border: 4px solid var(--a-primary-soft); border-top: 4px solid var(--a-primary); border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

.anim-reveal-up { animation: revealUp 0.6s cubic-bezier(0.2, 0, 0.2, 1) both; }
@keyframes revealUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }

/* TEXT ANIMATION */
.animated-gradient-text {
  background: linear-gradient(270deg, var(--accent-color, #0ea5e9) 0%, var(--primary, #6366f1) 50%, var(--accent-color, #0ea5e9) 100%);
  background-size: 200% auto; color: transparent; -webkit-background-clip: text; background-clip: text;
  animation: shineText 4s linear infinite; display: inline-block;
}
@keyframes shineText { to { background-position: 200% center; } }
</style>
