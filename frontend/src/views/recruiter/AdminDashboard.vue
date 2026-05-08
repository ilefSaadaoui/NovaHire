<template>
  <div class="dashboard-layout recruiter-layout">
    <Sidebar active-item="dashboard" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      <!-- CELESTIAL BACKGROUND -->
      

      <!-- CELESTIAL TOPBAR -->
      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome">
          <div class="header-with-icon" style="align-items: center; gap: 12px;">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 32px; color: var(--text-main);"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg>
            <div>
              <h1 class="premium-title-themed">{{ greeting }}, {{ userName }}</h1>
              <p class="welcome-sub">{{ $t('dashboard.admin.commandCenterSubtitle') || 'Voici le centre de commandement de votre organisation.' }}</p>
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

      <div v-else class="page-body stagger-reveal">        <!-- EMPIRE KPI GRID -->
        <div class="stats-grid-premium">
          <div class="admin-glass-card anim-reveal-up">
            <div class="a-stat-icon-wrapper light-blue"> <!-- Custom light blue from image -->
               <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="2" y="7" width="20" height="14" rx="2" ry="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
            </div>
            <div class="r-card-title">{{ $t('dashboard.admin.activeOffers') }}</div>
            <div class="r-card-val">{{ dashData.activeJobOffers || 0 }}</div>
            <div class="r-trend-badge emerald">
               <span>+12%</span>
            </div>
          </div>
          
          <div class="admin-glass-card anim-reveal-up" style="animation-delay: 0.1s">
            <div class="a-stat-icon-wrapper light-gold">
               <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
            </div>
            <div class="r-card-title">{{ $t('dashboard.admin.totalApps') }}</div>
            <div class="r-card-val">{{ dashData.totalApplications || 0 }}</div>
            <div class="r-trend-badge gold">
               <span>{{ $t('dashboard.admin.operational') || 'EXPANSION' }}</span>
            </div>
          </div>

          <div class="admin-glass-card anim-reveal-up" style="animation-delay: 0.2s">
            <div class="a-stat-icon-wrapper light-emerald">
               <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
            </div>
            <div class="r-card-title">{{ $t('dashboard.admin.velocity') || 'Vélocité Moyenne' }}</div>
            <div class="r-card-val">{{ dashData.averageTimeToHire || 14.5 }}{{ $t('reports.period.day') }}</div>
            <div class="r-trend-badge emerald">
               <span>{{ $t('dashboard.admin.operational') || 'OPTIMISÉ' }}</span>
            </div>
          </div>

          <div class="admin-glass-card anim-reveal-up" style="animation-delay: 0.3s">
            <div class="a-stat-icon-wrapper light-purple">
               <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/><polyline points="3.27 6.96 12 12.01 20.73 6.96"/><line x1="12" y1="22.08" x2="12" y2="12"/></svg>
            </div>
            <div class="r-card-title">{{ $t('dashboard.admin.aiEfficiency') || 'Efficacité IA' }}</div>
            <div class="r-card-val">{{ Math.round(dashData.aiEfficiencyRate || 0) }}%</div>
            <div class="r-progress-track">
               <div class="r-progress-fill celestial" :style="{ width: (dashData.aiEfficiencyRate || 0) + '%' }"></div>
            </div>
          </div>
        </div>
        <div class="dashboard-main-grid stagger-reveal">
          <!-- CORE SUPERVISION ROW -->
          <div class="supervision-row">
            <div class="admin-glass-card funnel-card anim-reveal-up" style="animation-delay: 0.4s">
               <div class="r-card-header">
                  <h3 class="r-card-title imperial-aura">{{ $t('dashboard.admin.funnelTitle') || 'Tour de Contrôle - Entonnoir Global' }}</h3>
                  <p class="welcome-sub">{{ $t('dashboard.admin.funnelSubtitle') || 'Flux de conversion stratégique' }}</p>
               </div>
               <div class="funnel-container-premium">
                  <div class="funnel-stage" v-for="(val, stage, idx) in dashData.funnelData" :key="stage" :style="{ width: (100 - idx*15) + '%', opacity: 1 - idx*0.15 }">
                     <span class="fs-label">{{ translateStage(stage) }}</span>
                     <span class="fs-val">{{ val }}</span>
                  </div>
               </div>
            </div>

            <div class="admin-glass-card activity-card anim-reveal-up" style="animation-delay: 0.5s">
               <div class="r-card-header">
                  <h3 class="r-card-title imperial-aura">{{ $t('dashboard.admin.empireFlow') || 'Flux Empire' }}</h3>
                  <div class="pulse-live">LIVE</div>
               </div>
               <div class="activity-stream premium-scroll">
                  <div v-for="(act, idx) in dashData.recentActivities" :key="idx" class="activity-item glass-row" :style="{ animationDelay: (0.8 + idx*0.1) + 's' }">
                     <div class="act-dot" :class="act.type?.toLowerCase() || 'application'"></div>
                     <div class="act-body">
                        <span class="act-title">{{ act.title }}</span>
                        <span class="act-time">{{ act.timeAgo }}</span>
                     </div>
                  </div>
               </div>
            </div>
          </div>

          <!-- ANALYTICS ROW -->
          <div class="analytics-row">
            <div class="admin-glass-card anim-reveal-up" style="animation-delay: 0.6s">
               <div class="r-card-header">
                  <h3 class="r-card-title">{{ $t('dashboard.admin.deptStrength') || 'Force par Département' }}</h3>
               </div>
               <div class="dept-stats-list">
                  <div v-for="dept in dashData.departmentStats" :key="dept.name" class="dept-row">
                     <div class="dept-info">
                        <span class="dept-name">{{ dept.name }}</span>
                        <span class="dept-off">{{ dept.offersCount }} {{ $t('common.offers') }}</span>
                     </div>
                     <div class="dept-progress-bar">
                        <div class="dp-fill" :style="{ width: (dept.applicationsCount / (dashData.totalApplications || 1) * 100) + '%' }"></div>
                     </div>
                     <span class="dept-count">{{ dept.applicationsCount }} {{ $t('dashboard.candidates') }}</span>
                  </div>
               </div>
            </div>

            <div class="admin-glass-card anim-reveal-up" style="animation-delay: 0.7s">
               <div class="r-card-header">
                  <h3 class="r-card-title">{{ $t('dashboard.admin.teamPerformance') || 'Top Performance Équipe' }}</h3>
               </div>
               <div class="team-grid-compact">
                  <div v-for="m in dashData.teamStats" :key="m.id" class="team-mini-card">
                     <div class="r-avatar sm" :style="{ background: 'var(--accent-soft)', color: 'var(--primary)' }">{{ m.initials }}</div>
                     <div class="tm-info">
                        <div class="tm-name">{{ m.name }}</div>
                        <div class="tm-stat">{{ m.interviewsCount }} {{ $t('dashboard.stats.plannedInterviews') }}</div>
                     </div>
                  </div>
               </div>
            </div>

            <div class="admin-glass-card anim-reveal-up" style="animation-delay: 0.8s">
              <div class="r-card-header">
                <h3 class="r-card-title imperial-aura">{{ $t('dashboard.admin.flagshipOffers') || 'Vaisseaux Amiral' }}</h3>
              </div>
              <div class="simple-list-compact">
                <div v-for="o in dashData.recentOffers.slice(0, 3)" :key="o.id" class="offer-mini-row">
                  <div class="off-title">{{ o.title }}</div>
                  <div class="off-count">{{ o.applicationsCount }}</div>
                </div>
              </div>
               <button class="btn-luxury primary full-width mt-10" @click="$router.push('/offres')">{{ $t('dashboard.seeAll') }}</button>
            </div>
          </div>
        </div>
      </div>
    </main>
    </div>
</template>

<script>
import { useAuthStore } from '@/stores/authStore'
import { useCompanyStore } from '@/stores/companyStore'
import Sidebar from '@/components/layout/Sidebar.vue'


export default {
  name: 'AdminDashboard',
  components: { Sidebar },
  setup() {
    const companyStore = useCompanyStore()
    return { companyStore }
  },
  data() {
    return {
      authStore: useAuthStore(),
      sidebarCollapsed: false
    }
  },
  async mounted() {
    await this.companyStore.fetchStats()
  },
  computed: {
    loading() { return this.companyStore.loading },
    dashData() { return this.companyStore.stats },
    userName() { return this.authStore.user?.firstName || 'Administrateur' },
    greeting() {
      const u = this.authStore.user
      const name = u?.firstName || 'Administrateur'
      const hour = new Date().getHours()
      if (hour < 12) return this.$t('dashboard.recruiter.welcomeMorning', { name })
      if (hour < 18) return this.$t('dashboard.recruiter.welcomeAfternoon', { name })
      return this.$t('dashboard.recruiter.welcomeEvening', { name })
    },
    initials() {
      const u = this.authStore.user
      return u?.firstName ? (u.firstName[0] + u.lastName[0]).toUpperCase() : 'NH'
    },
    accentColor() { return this.authStore.themeColors.accent },
    accentDark() { return this.authStore.themeColors.accentDark },
    aiPercentage() {
      if (!this.dashData.totalApplications) return 0;
      return Math.round((this.dashData.aiAnalysesCount / this.dashData.totalApplications) * 100);
    },
    maxApps() {
      return Math.max(...(this.dashData.monthlyApplications || [0]), 10);
    },
    avatarStyle() {
      return {
        background: `linear-gradient(135deg, ${this.accentColor}, ${this.accentDark})`,
        color: 'white'
      }
    }
  },
  methods: {
    getBarHeight(count) {
      if (count === undefined || isNaN(count)) return 5;
      return Math.max((count / this.maxApps) * 100, 5);
    },
    translateStage(stage) {
      const stages = { 
        'sourcing': this.$t('pipeline.stages.submitted') || 'Sourcing', 
        'screening': this.$t('pipeline.stages.underreview') || 'Filtrage', 
        'interview': this.$t('pipeline.stages.interview') || 'Entretien', 
        'offer': this.$t('pipeline.stages.accepted') || 'Offre' 
      };
      return stages[stage] || stage;
    },
    getMonthName(idx) {
      const months = ['Jan', 'Fév', 'Mar', 'Avr', 'Mai', 'Juin', 'Juil', 'Août', 'Sep', 'Oct', 'Nov', 'Déc'];
      const now = new Date();
      now.setMonth(now.getMonth() - (5 - idx));
      return months[now.getMonth()];
    }
  }
}
</script>

<style scoped>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";
@import "@/assets/admin-theme.css";

.dashboard-layout { position: relative; display: flex; min-height: 100vh; background: transparent; overflow: visible; }

.main-content { position: relative; transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1); flex: 1; z-index: 1; }
.ml-collapsed { margin-left: var(--sidebar-collapsed); }

/* TOPBAR */



/* CARDS */
.stats-grid-premium { display: grid; grid-template-columns: repeat(4, 1fr); gap: 24px; margin-bottom: 40px; }
.admin-glass-card {
  position: relative; overflow: visible;
  background: var(--glass-bg, rgba(255, 255, 255, 0.6));
  backdrop-filter: blur(20px) saturate(180%);
  -webkit-backdrop-filter: blur(20px) saturate(180%);
  border: 1px solid var(--glass-border, rgba(255, 255, 255, 0.4));
  padding: 24px; border-radius: 30px; transition: 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05);
}

.dark-mode .admin-glass-card {
  background: rgba(30, 41, 59, 0.4);
  border-color: rgba(255, 255, 255, 0.05);
  box-shadow: 0 15px 35px rgba(0, 0, 0, 0.2);
}

.admin-glass-card:hover { transform: translateY(-10px) scale(1.02); box-shadow: 0 20px 50px rgba(0, 0, 0, 0.08); }

.a-stat-icon-wrapper {
  width: 54px; height: 54px; border-radius: 18px; display: flex; align-items: center; justify-content: center;
  margin-bottom: 20px; transition: 0.3s;
}
.a-stat-icon-wrapper svg { width: 28px; height: 28px; }

.a-stat-icon-wrapper.light-blue { background: #e0f2fe; color: #0284c7; }
.a-stat-icon-wrapper.light-gold { background: #fef3c7; color: #d97706; }
.a-stat-icon-wrapper.light-emerald { background: #d1fae5; color: #059669; }
.a-stat-icon-wrapper.light-purple { background: #f3e8ff; color: #7e22ce; }

.r-card-val { font-size: 48px; font-weight: 900; color: var(--r-text-main, #0f172a); margin: 15px 0 20px 0; letter-spacing: -2px; }

.r-trend-badge {
  display: inline-flex; align-items: center; gap: 6px; padding: 6px 14px; border-radius: 100px;
  font-size: 10px; font-weight: 850; letter-spacing: 0.5px; width: fit-content;
}

.r-trend-badge.emerald { background: #d1fae5; color: #059669; }
.r-trend-badge.gold { background: #fef3c7; color: #d97706; }

/* Tour de controle & flux empire overrides for image 2 matching */
.funnel-card, .activity-card { 
  background: var(--card-bg, white); 
  border: 1px solid var(--r-border, transparent);
  border-radius: 20px; 
  box-shadow: 0 8px 30px rgba(0,0,0,0.03); 
}

.r-card-title { font-size: 14px; font-weight: 800; color: var(--r-text-main, #1e293b); letter-spacing: -0.2px; }
.welcome-sub { font-size: 12px; color: var(--r-text-sub, #64748b); font-weight: 500; }

/* MAIN GRID REORG */
.dashboard-main-grid { display: flex; flex-direction: column; gap: 32px; }
.supervision-row { display: grid; grid-template-columns: 8fr 4fr; gap: 32px; }
.analytics-row { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 32px; }

.funnel-card { height: 420px; }
.activity-card { height: 420px; }

.team-grid-compact { display: flex; flex-direction: column; gap: 12px; }
.team-mini-card { display: flex; align-items: center; gap: 12px; padding: 10px; border-radius: 12px; background: rgba(14, 165, 233, 0.03); }
.tm-name { font-weight: 700; font-size: 13px; color: var(--r-text-main); }
.tm-stat { font-size: 11px; color: var(--a-primary); font-weight: 800; }

.simple-list-compact { display: flex; flex-direction: column; gap: 8px; }
.offer-mini-row { display: flex; justify-content: space-between; align-items: center; padding: 10px; border-radius: 10px; background: rgba(255,255,255,0.02); border: 1px solid var(--r-border); }
.offer-mini-row .off-title { font-size: 12px; font-weight: 700; }
.offer-mini-row .off-count { font-size: 11px; font-weight: 900; color: var(--a-primary); }

.mt-10.sm { margin-top: 10px; padding: 8px; font-size: 11px; }

/* FUNNEL */
.funnel-container-premium { display: flex; flex-direction: column; align-items: center; gap: 8px; margin-top: 32px; }
.funnel-stage {
   height: 48px; background: var(--premium-grad); border-radius: 12px;
   display: flex; justify-content: space-between; align-items: center; padding: 0 24px;
   color: white; font-weight: 800; box-shadow: 0 4px 15px var(--accent-soft);
   transition: 0.5s cubic-bezier(0.4, 0, 0.2, 1);
}
.fs-val { font-size: 18px; }

/* DEPT STATS */
.dept-stats-list { display: flex; flex-direction: column; gap: 20px; }
.dept-row { display: flex; flex-direction: column; gap: 8px; }
.dept-info { display: flex; justify-content: space-between; font-weight: 700; font-size: 13px; }
.dept-name { color: var(--r-text-main); }
.dept-off { color: var(--r-text-sub); }
.dept-progress-bar { height: 6px; background: var(--r-border); border-radius: 100px; overflow: visible; }
.dp-fill { height: 100%; background: var(--premium-grad); border-radius: 100px; }
.dept-count { font-size: 11px; font-weight: 800; color: var(--primary); align-self: flex-end; }

/* ACTIVITY STREAM */
.activity-stream { display: flex; flex-direction: column; gap: 16px; max-height: 400px; overflow-y: auto; padding-right: 8px; }
.activity-item { display: flex; gap: 12px; align-items: flex-start; padding: 12px; border-radius: 14px; background: rgba(255,255,255,0.02); border: 1px solid var(--r-border); }
.act-dot { width: 8px; height: 8px; border-radius: 50%; margin-top: 5px; }
.act-dot.application { background: var(--primary); box-shadow: 0 0 8px var(--primary); }
.act-dot.offer { background: #10b981; }
.act-body { display: flex; flex-direction: column; gap: 2px; }
.act-title { font-size: 13px; font-weight: 700; color: var(--r-text-main); }
.act-time { font-size: 11px; color: var(--r-text-sub); }

.pulse-live {
   font-size: 10px; font-weight: 900; color: #ef4444; background: rgba(239, 68, 68, 0.1);
   padding: 2px 8px; border-radius: 6px; animation: pulse 2s infinite;
}
@keyframes pulse { 0% { opacity: 1; } 50% { opacity: 0.5; } 100% { opacity: 1; } }

/* CHARTS (Legacy cleaned) */
.r-area-chart-container { height: 120px; margin-top: 32px; width: 100%; position: relative; }
.premium-chart { width: 100%; height: 100%; overflow: visible; }
.r-area-path.celestial { fill: none; stroke: var(--primary); stroke-width: 4; stroke-linecap: round; filter: drop-shadow(0 4px 10px var(--accent-soft)); }

.chart-labels { display: flex; justify-content: space-between; margin-top: 15px; padding: 0 10px; }
.chart-labels span { font-size: 11px; font-weight: 700; color: var(--r-text-sub); text-transform: uppercase; }

/* LISTS */
.r-list-item.celestial {
  background: rgba(14, 165, 233, 0.03); border: 1px solid rgba(14, 165, 233, 0.05); transition: 0.3s;
}
.r-list-item.celestial:hover { background: var(--accent-soft); border-color: var(--primary); transform: translateX(5px); }

.offer-mini-card { padding: 16px; border-radius: 20px; background: rgba(255,255,255,0.02); border: 1px solid var(--r-border); margin-bottom: 12px; transition: 0.3s; }
.offer-mini-card:hover { border-color: var(--primary); transform: translateX(5px); background: var(--accent-soft); }
.off-title { font-weight: 800; font-size: 14px; color: var(--r-text-main); }
.off-meta { font-size: 12px; color: var(--r-text-sub); margin-top: 4px; }
.text-celestial { color: var(--primary); font-weight: 700; }
.off-status { font-size: 10px; font-weight: 900; padding: 2px 8px; border-radius: 6px; width: fit-content; margin-top: 8px; }
.off-status.active { background: var(--primary); color: white; }
.r-progress-fill.celestial { background: var(--primary); }

.full-width { width: 100%; }
.mt-20 { margin-top: 20px; }

@media (max-width: 1400px) {
  .stats-grid-premium { grid-template-columns: repeat(2, 1fr); }
  .dashboard-main-grid { grid-template-columns: 1fr; }
}

/* ANIMATIONS */
.loading-state-premium { display: flex; align-items: center; justify-content: center; height: 400px; }
.nebula-spinner { width: 60px; height: 60px; border: 4px solid var(--a-primary-soft); border-top: 4px solid var(--a-primary); border-radius: 50%; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

.anim-reveal-up { animation: revealUp 0.6s cubic-bezier(0.2, 0, 0.2, 1) both; }
@keyframes revealUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
