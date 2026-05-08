<template>
  <div class="dashboard-layout recruiter-layout">
    <Sidebar :active-item="'dashboard'" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main stagger-reveal" :class="{ 'ml-collapsed': sidebarCollapsed }">
      
      <header class="r-topbar">
        <div class="r-welcome elite-professional">
          <div class="welcome-text-wrap">
            <h1 class="lumina-text" v-html="animatedGreeting"></h1>
            <p class="welcome-sub professional-sub">{{ $t('dashboard.recruiter.subtitle') }}</p>
          </div>
          <div class="professional-accent-line"></div>
        </div>
        <div class="r-header-tools">
          <div class="period-toggle-global">
            <button 
              v-for="p in translatedPeriodOptions" 
              :key="p.value" 
              :class="{ active: globalPeriod === p.value }" 
              @click="globalPeriod = p.value"
            >
              <span class="period-icon" v-html="p.icon"></span>
              {{ p.label }}
            </button>
          </div>
          <div class="header-action-group" style="display: flex; gap: 12px; margin-left: 24px;">
            <NotificationBell />
          </div>
        </div>
      </header>


      <div class="page-body">
        <div v-if="loading" class="dashboard-loading">
          <div class="spinner"></div>
          <p>{{ $t('dashboard.loading') }}</p>
        </div>

        <template v-else>
          <!-- Top Row Grid -->
          <div class="dashboard-grid top-row recruiter-grid">
            <!-- Active Job Offers -->
            <div class="r-card kpi-card glass-panel-ultra spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <span class="r-card-title">{{ $t('dashboard.recruiter.myActiveOffers') }}</span>
              </div>
              <div class="r-card-body">
                <div class="main-val counter-animate" v-counter>{{ dashData.activeJobOffers || 0 }}</div>
                <div class="r-trend up"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="18 15 12 9 6 15"/></svg> {{ $t('dashboard.recruiter.vsPreviousPeriod') }}</div>
                <div class="r-area-chart-container">
                  <svg viewBox="0 0 100 30" preserveAspectRatio="none" style="width: 100%; height: 100%;">
                    <defs>
                      <linearGradient id="chartGradientArea1" x1="0%" y1="0%" x2="0%" y2="100%">
                        <stop offset="0%" :stop-color="accentColor" stop-opacity="0.3"></stop>
                        <stop offset="100%" :stop-color="accentColor" stop-opacity="0"></stop>
                      </linearGradient>
                    </defs>
                    <path d="M0,30 L0,25 Q25,5 50,15 T100,10 L100,30 Z" fill="url(#chartGradientArea1)" />
                    <path class="r-area-path" d="M0,25 Q25,5 50,15 T100,10" :stroke="accentColor" />
                  </svg>
                </div>
              </div>
            </div>

            <!-- Total Applications -->
            <div class="r-card kpi-card glass-panel-ultra spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <span class="r-card-title">{{ $t('dashboard.recruiter.totalApplications') }}</span>
              </div>
              <div class="r-card-body">
                <div class="main-val counter-animate" v-counter>{{ dashData.totalApplications || 0 }}</div>
                <div class="r-trend up"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="18 15 12 9 6 15"/></svg> {{ $t('dashboard.recruiter.vsPreviousPeriod') }}</div>
                <div class="r-area-chart-container">
                  <svg viewBox="0 0 100 30" preserveAspectRatio="none" style="width: 100%; height: 100%;">
                    <defs>
                      <linearGradient id="chartGradientArea2" x1="0%" y1="0%" x2="0%" y2="100%">
                        <stop offset="0%" stop-color="#f472b6" stop-opacity="0.3"></stop>
                        <stop offset="100%" stop-color="#f472b6" stop-opacity="0"></stop>
                      </linearGradient>
                    </defs>
                    <path d="M0,30 L0,15 Q25,25 50,5 T100,10 L100,30 Z" fill="url(#chartGradientArea2)" />
                    <path class="r-area-path" d="M0,15 Q25,25 50,5 T100,10" stroke="#f472b6" />
                  </svg>
                </div>
              </div>
            </div>

            <!-- Planned Interviews -->
            <div class="r-card kpi-card glass-panel-ultra spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <span class="r-card-title">{{ $t('dashboard.recruiter.plannedInterviews') || 'Planned Interviews' }}</span>
              </div>
              <div class="r-card-body">
                <div class="bank-info">
                  <div class="bank-name">{{ $t('dashboard.recruiter.upcomingInterviews') }}</div>
                  <div class="card-type" :style="{ color: accentColor }">PRIOR</div>
                </div>
                <div class="bank-amount" v-counter>{{ dashData.plannedInterviews || 0 }}</div>
                <div class="card-number">{{ $t('dashboard.recruiter.scheduledUpcoming') }}</div>
              </div>
            </div>

            <!-- Status Breakdown -->
            <div class="r-card kpi-card glass-panel-ultra spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <span class="r-card-title">{{ $t('dashboard.recruiter.statusBreakdown') }}</span>
              </div>
              <div class="r-card-body centered">
                <div class="r-doughnut">
                  <svg viewBox="0 0 36 36" class="circular-chart">
                    <path class="circle-bg" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" style="stroke: var(--r-border); stroke-width: 3.8; fill: none;" />
                    <!-- Dynamic Arcs -->
                    <path v-for="(item, idx) in breakdownWithOffsets" 
                      :key="item.name"
                      class="circle" 
                      :stroke="item.color" 
                      :stroke-dasharray="`${item.percentage}, 100`" 
                      :stroke-dashoffset="-item.offset" 
                      d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" 
                      style="stroke-width: 3.8; fill: none; transition: all 1s ease;" 
                    />
                  </svg>
                  <div class="r-doughnut-val">
                    <span>100%</span>
                    <span>{{ $t('pipeline.modal.results') }}</span>
                  </div>
                </div>
                <!-- Categories Legend -->
                <div class="legend-list grid-legend">
                  <div v-for="item in dashData.statusBreakdown" :key="item.name" class="legend-item">
                    <span class="dot" :style="{ background: item.color }"></span> 
                    {{ formatStatus(item.name) }} ({{ item.count }})
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Middle Row -->
          <div class="dashboard-grid mid-row recruiter-grid">
            <!-- Recruitment Flow (Dynamic) -->
            <div class="r-card glass-panel-ultra spotlight-card gold-glow-card span-2" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <span class="r-card-title">{{ $t('dashboard.recruiter.recruitmentActivity') }}</span>
                <div class="legend-pill-wrap">
                  <div class="legend-pill"><span class="dot" :style="{ background: accentColor }"></span> {{ $t('dashboard.recruiter.newApps') }}</div>
                  <div class="legend-pill"><span class="dot" style="background: #cbd5e1"></span> {{ $t('dashboard.recruiter.processed') }}</div>
                </div>
              </div>
              <div class="bar-chart-container extended-bar">
                <div v-for="(count, idx) in dashData.monthlyApplications" :key="idx" class="bar-group">
                  <div class="bar-pair">
                    <div class="bar bar-1" :style="{ height: getBarHeight(count) + '%', background: accentColor }"></div>
                    <div class="bar bar-2" :style="{ height: (getBarHeight(count) * 0.6) + '%' }"></div>
                  </div>
                  <span class="bar-lbl">{{ getChartLabel(idx) }}</span>
                </div>
              </div>
            </div>

            <!-- Applications Trend -->
            <div class="r-card glass-panel-ultra spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <span class="r-card-title">{{ $t('dashboard.recruiter.matchingTrends') }}</span>
              </div>
              <div class="circular-stats-grid gauge-grid">
                <!-- AI Efficiency -->
                <div class="r-doughnut mini-gauge">
                  <svg viewBox="0 0 36 36" class="circular-chart">
                    <path class="circle-bg" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" style="stroke: var(--r-border); stroke-width: 4; fill: none;"></path>
                    <path class="circle" stroke="#10b981" :stroke-dasharray="aiPercentage + ', 100'" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" style="stroke-width: 4; fill: none;"></path>
                  </svg>
                  <div class="r-doughnut-val">
                    <span style="font-size: 16px;">{{ aiPercentage }}%</span>
                  </div>
                  <span class="mini-lbl" style="color: #10b981;">{{ $t('dashboard.recruiter.analysisRate') }}</span>
                </div>
                <!-- Conversion/Retention -->
                <div class="r-doughnut mini-gauge">
                  <svg viewBox="0 0 36 36" class="circular-chart">
                    <path class="circle-bg" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" style="stroke: var(--r-border); stroke-width: 4; fill: none;"></path>
                    <path class="circle" stroke="#818cf8" :stroke-dasharray="activeOfferPercentage + ', 100'" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" style="stroke-width: 4; fill: none;"></path>
                  </svg>
                  <div class="r-doughnut-val">
                    <span style="font-size: 16px;">{{ activeOfferPercentage }}%</span>
                  </div>
                  <span class="mini-lbl" style="color: #818cf8;">{{ $t('dashboard.recruiter.processingRate') }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Skills & Quality Row -->
          <div class="dashboard-grid skills-row recruiter-grid">

            <!-- Talent Quality Distribution -->
            <div class="r-card glass-panel-ultra spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <span class="r-card-title">{{ $t('dashboard.recruiter.qualityDistribution') }}</span>
              </div>
              <div class="talent-dist-content">
                <div v-for="t in dashData.talentDistribution" :key="t.label" class="talent-bar-item">
                  <div class="talent-bar-info">
                    <span class="talent-lbl">{{ t.label }}</span>
                    <span class="talent-val">{{ $t('dashboard.recruiter.peopleCount', { count: t.count }) }}</span>
                  </div>
                  <div class="talent-track">
                    <div class="talent-fill" :style="{ width: t.percentage + '%', background: t.color }"></div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Top Skills (NEW fill for empty space) -->
            <div class="r-card glass-panel-ultra spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <span class="r-card-title">{{ $t('dashboard.recruiter.topSkills') }}</span>
              </div>
              <div class="skills-insight-grid">
                <div v-for="skill in dashData.topSkills" :key="skill.name" class="skill-stat-pill">
                  <span class="skill-name-badge">{{ skill.name }}</span>
                  <div class="skill-count-badge">{{ skill.count }}</div>
                </div>
                <div v-if="!dashData.topSkills.length" class="empty-notif" style="padding: 20px;">
                  {{ $t('dashboard.recruiter.awaitingAiAnalyses') }}
                </div>
              </div>
            </div>
          </div>

          <!-- Bottom Row -->
          <div class="dashboard-grid bottom-row recruiter-grid">
            <!-- Recent Offers (Translations styled list) -->
            <div class="r-card glass-panel-ultra spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <span class="r-card-title">{{ $t('dashboard.recruiter.recentOffers') }}</span>
              </div>
              <div class="r-list">
                <div v-for="offer in dashData.recentOffers.slice(0, 4)" :key="offer.id" class="r-list-item">
                  <div class="r-item-icon" :style="{ background: accentColor + '15', color: accentColor }">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="7" width="20" height="14" rx="2" ry="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
                  </div>
                  <div class="r-item-body">
                    <span class="r-item-title">{{ offer.title }}</span>
                    <span class="r-item-sub">{{ offer.location }}</span>
                  </div>
                  <span class="r-item-val" :style="{ color: accentColor }">{{ $t('dashboard.recruiter.appsCount', { count: offer.applicationsCount || 0 }) }}</span>
                </div>
              </div>
            </div>

            <!-- Recent Activities -->
            <div class="r-card glass-panel-ultra spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <span class="r-card-title">{{ $t('dashboard.recruiter.recentActivities') }}</span>
              </div>
              <div class="r-list activity-list-premium">
                <div v-for="act in dashData.recentActivities.slice(0, 5)" :key="act.title" class="r-list-item-activity">
                  <div class="activity-avatar" :style="{ background: getScoreColor(act.score) + '15', color: getScoreColor(act.score) }">
                    {{ getInitials(act.title) }}
                  </div>
                  <div class="activity-body">
                    <div class="activity-top">
                      <span class="activity-name">{{ formatActivityName(act.title) }}</span>
                      <div class="activity-score-pill" :style="{ background: getScoreColor(act.score) + '15', color: getScoreColor(act.score) }">
                        {{ act.score }}%
                      </div>
                    </div>
                    <div class="activity-meta">
                      <span class="activity-type">{{ $t('dashboard.recruiter.applicationReceived') }}</span>
                      <span class="activity-time">• {{ act.timeAgo }}</span>
                    </div>
                    <div class="activity-progress-wrap">
                      <div class="activity-progress-bar" :style="{ width: act.score + '%', background: getScoreColor(act.score) }"></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Global Flow (Income and Expenses styled dual line chart) -->
            <div class="r-card glass-panel-ultra spotlight-card gold-glow-card span-2" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <span class="r-card-title">{{ $t('dashboard.recruiter.applicationFlow') }}</span>
              </div>
              <div class="line-chart-full">
                <svg viewBox="0 0 800 200" class="full-line-svg">
                  <!-- Income Line -->
                  <path d="M0,150 Q200,50 400,100 T800,80" fill="none" :stroke="accentColor" stroke-width="3.5" />
                  <circle cx="600" cy="74" r="6" :fill="accentColor" />
                  <!-- Expenses Line -->
                  <path d="M0,180 Q200,120 400,160 T800,140" fill="none" stroke="#818cf8" stroke-width="3.5" opacity="0.8" />
                </svg>
                <div class="chart-labels">
                  <span>{{ $t('dashboard.recruiter.days.mon') }}</span>
                  <span>{{ $t('dashboard.recruiter.days.tue') }}</span>
                  <span>{{ $t('dashboard.recruiter.days.wed') }}</span>
                  <span>{{ $t('dashboard.recruiter.days.fri') }}</span>
                  <span>{{ $t('dashboard.recruiter.days.sat') }}</span>
                  <span>{{ $t('dashboard.recruiter.days.sun') }}</span>
                </div>
              </div>
            </div>
          </div>
        </template>
      </div>
    </main>
    
    <DailyBriefingModal
      :show="showBriefing"
      @close="showBriefing = false"
    />
  </div>
</template>

<script>
import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'
import Sidebar from '@/components/layout/Sidebar.vue'

import NotificationBell from '@/components/common/NotificationBell.vue'
import { onMounted, onUnmounted } from 'vue'
import { useToastStore } from '@/stores/toastStore'
import api from '@/api/axios'
import DailyBriefingModal from '@/components/superadmin/modals/DailyBriefingModal.vue'


export default {
  name: 'RecruiterDashboard',
  components: { Sidebar, NotificationBell, DailyBriefingModal },
  data() {
    const authStore = useAuthStore()
    const toastStore = useToastStore()
    const isCompanyOwner = authStore.isAdmin && !authStore.isSuperAdmin

    return {
      authStore,
      toastStore,
      loading: true,
      sidebarCollapsed: false,
      isCompanyOwner,
      globalPeriod: 'month',
      periodOptions: [
        { 
          label: '24h', 
          value: 'day',
          icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>'
        },
        { 
          label: '7j',  
          value: 'week',
          icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>'
        },
        { 
          label: '30j', 
          value: 'month',
          icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>'
        }
      ],
      dashData: {
        activeJobOffers: 0,
        totalApplications: 0,
        aiAnalysesCount: 0,
        plannedInterviews: 0,
        recentOffers: [],
        recentActivities: [],
        monthlyApplications: [],
        statusBreakdown: [],
        talentDistribution: [],
        topSkills: []
      },
      // Animation State
      animatedGreeting: '',
      rawGreeting: '',
      showBriefing: false
    }
  },
  async mounted() {
    await this.fetchDashboard()
    this.initProfessionalGreeting()

    // Show briefing if first time in session
    if (!sessionStorage.getItem('briefing_shown')) {
      setTimeout(() => {
        this.showBriefing = true
        sessionStorage.setItem('briefing_shown', 'true')
      }, 1500)
    }
  },
  watch: {
    async globalPeriod(newPeriod) {
      await this.fetchDashboard(newPeriod)
    }
  },
  computed: {
    userName() {
      return this.authStore.user?.firstName || 'User'
    },
    initials() {
      const u = this.authStore.user
      return u?.firstName ? (u.firstName[0] + u.lastName[0]).toUpperCase() : 'NH'
    },
    accentColor() {
      return this.authStore.themeColors.accent
    },
    accentDark() {
      return this.authStore.themeColors.accentDark
    },
    aiPercentage() {
      if (!this.dashData.totalApplications) return 0;
      return Math.round((this.dashData.aiAnalysesCount / this.dashData.totalApplications) * 100);
    },
    activeOfferPercentage() {
      if (!this.dashData.activeJobOffers) return 0;
      return Math.min(Math.round((this.dashData.activeJobOffers / 10) * 100), 100); // 10 is a mock target
    },
    maxApps() {
      return Math.max(...(this.dashData.monthlyApplications || [0]), 10);
    },
    avatarStyle() {
      return {
        background: `linear-gradient(135deg, ${this.accentColor}, ${this.accentDark})`,
        color: 'white'
      }
    },
    welcomeKey() {
      const hour = new Date().getHours()
      if (hour >= 5 && hour < 12) return 'dashboard.recruiter.welcomeMorning'
      if (hour >= 12 && hour < 18) return 'dashboard.recruiter.welcomeAfternoon'
      return 'dashboard.recruiter.welcomeEvening'
    },
    translatedPeriodOptions() {
      const map = {
        'day': '24h',
        'week': '7j',
        'month': '30j'
      }
      return this.periodOptions.map(p => ({
        ...p,
        label: map[p.value] || p.label
      }))
    },
    breakdownWithOffsets() {
      let offset = 0;
      return (this.dashData.statusBreakdown || []).map(item => {
        const currentOffset = offset;
        offset += item.percentage;
        return { ...item, offset: currentOffset };
      });
    }
  },
  methods: {
    initProfessionalGreeting() {
      const text = this.$t(this.welcomeKey, { name: this.userName })
      this.rawGreeting = text
      
      // Wrap each character in a span with a staggered delay
      this.animatedGreeting = text.split('').map((char, i) => {
        const delay = i * 0.03
        const content = char === ' ' ? '&nbsp;' : char
        return `<span class="lumina-char" style="animation-delay: ${delay}s">${content}</span>`
      }).join('')
    },
    handleSpotlight(e) {
      const card = e.currentTarget;
      const rect = card.getBoundingClientRect();
      const x = e.clientX - rect.left;
      const y = e.clientY - rect.top;
      card.style.setProperty('--mouse-x', `${x}px`);
      card.style.setProperty('--mouse-y', `${y}px`);
    },
    getBarHeight(count) {
      return Math.max((count / this.maxApps) * 100, 5);
    },
    getChartLabel(idx) {
      if (this.globalPeriod === 'day') {
        const hoursBack = (5 - idx) * 4;
        return hoursBack === 0 ? 'Maintenant' : `-${hoursBack}h`;
      }
      if (this.globalPeriod === 'week') {
        const days = ['Dim', 'Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam'];
        const d = new Date();
        d.setDate(d.getDate() - (6 - idx));
        return days[d.getDay()];
      }
      
      const months = ['Jan', 'Fév', 'Mar', 'Avr', 'Mai', 'Juin', 'Juil', 'Août', 'Sep', 'Oct', 'Nov', 'Déc'];
      const now = new Date();
      now.setMonth(now.getMonth() - (5 - idx));
      return months[now.getMonth()];
    },
    async fetchDashboard(period) {
      this.loading = true
      try {
        const p = period || this.globalPeriod
        const url = p && p !== 'all'
          ? `/recruiter/dashboard-stats?personal=true&period=${p}`
          : '/recruiter/dashboard-stats?personal=true'
        const res = await api.get(url)
        this.dashData = res.data
      } catch (error) {
        console.error('Erreur dashboard:', error)
      } finally {
        this.loading = false
      }
    },
    formatStatus(status) {
      const map = {
        'submitted': this.$t('pipeline.stages.submitted'),
        'shortlisted': this.$t('pipeline.stages.shortlisted'),
        'interview': this.$t('pipeline.stages.interview'),
        'interviewed': this.$t('pipeline.stages.interviewed'),
        'underreview': this.$t('pipeline.stages.underreview'),
        'rejected': this.$t('pipeline.stages.rejected'),
        'accepted': this.$t('pipeline.stages.accepted')
      };
      return map[status.toLowerCase()] || status;
    },
    getScoreColor(score) {
      if (score >= 80) return '#10b981'; // Success
      if (score >= 50) return this.accentColor; // Warning/Accent
      return '#f472b6'; // Danger/Pink
    },
    getInitials(title) {
      const parts = title.split(' – ');
      if (parts.length < 2) return 'NH';
      const name = parts[1];
      const nameParts = name.trim().split(' ');
      if (nameParts.length >= 2) {
        return (nameParts[0][0] + nameParts[nameParts.length - 1][0]).toUpperCase();
      }
      return name[0].toUpperCase();
    },
    formatActivityName(title) {
      const parts = title.split(' – ');
      return parts.length > 1 ? parts[1] : title;
    }
  }
}
</script>

<style>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";
</style>

<style scoped>
@import '@/assets/dashboard.css';
.dashboard-layout { background: transparent !important; }

/* COMPONENT-SPECIFIC STYLES */

/* Dashboard Layout Specifics */
.recruiter-grid {
  display: grid;
  gap: 14px;
  margin-bottom: 14px;
}

.top-row { grid-template-columns: repeat(4, 1fr); }
.mid-row { grid-template-columns: 2fr 1fr; }
.skills-row { grid-template-columns: repeat(2, 1fr); }
.bottom-row { grid-template-columns: 1fr 1fr 2fr; }

@media (max-width: 1400px) {
  .top-row, .mid-row, .bottom-row { grid-template-columns: repeat(2, 1fr); }
  .mid-row > .span-2 { grid-column: span 2; }
}

@media (max-width: 900px) {
  .top-row, .mid-row, .bottom-row { grid-template-columns: 1fr; }
  .mid-row > .span-2, .bottom-row > .span-2 { grid-column: span 1; }
}

/* Card Specific Interiors */
.main-val { font-size: 26px; font-weight: 800; color: var(--r-text-main); line-height: 1.2; }
.bank-info { display: flex; justify-content: space-between; align-items: center; }
.bank-name { font-size: 14px; font-weight: 600; color: var(--r-text-sub); }
.card-type { font-size: 12px; font-weight: 900; letter-spacing: 1px; }
.bank-amount { font-size: 28px; font-weight: 800; margin: 6px 0 2px; color: var(--r-text-main); }
.card-number { font-size: 11px; color: var(--r-text-sub); letter-spacing: 2px; }

/* Charts */
.legend-list { display: grid; gap: 8px; margin-top: 16px; }
.grid-legend { grid-template-columns: 1fr 1fr; }
.legend-item { display: flex; align-items: center; gap: 8px; font-size: 12px; font-weight: 600; color: var(--r-text-sub); }
.legend-item .dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
}

/* Header Tools Additions */
.premium-btn-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: var(--r-main-bg, rgba(255,255,255,0.05));
  border: 1px solid var(--r-border, rgba(255,255,255,0.1));
  color: var(--r-text-sub, #94a3b8);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.premium-btn-icon:hover {
  color: var(--r-text-main, #fff);
  border-color: var(--accent-color, #fbbf24);
  box-shadow: 0 0 15px rgba(251, 191, 36, 0.1);
  background: rgba(255,255,255,0.08); /* slight brightness on hover */
}

.premium-btn-icon svg {
  width: 20px;
  height: 20px;
}

.legend-pill-wrap { display: flex; gap: 12px; }
.legend-pill { display: flex; align-items: center; gap: 6px; font-size: 11px; font-weight: 700; color: var(--r-text-sub); }
.legend-pill .dot { width: 6px; height: 6px; border-radius: 50%; }

/* Bar Chart */
.bar-chart-container { display: flex; align-items: flex-end; justify-content: space-between; height: 140px; padding-top: 10px; }
.bar-group { display: flex; flex-direction: column; align-items: center; gap: 12px; height: 100%; justify-content: flex-end; }
.bar-pair { display: flex; gap: 4px; align-items: flex-end; height: calc(100% - 24px); width: 24px; }
.extended-bar .bar-pair { width: 32px; gap: 6px; }
.bar { width: 10px; border-radius: 4px; transition: height 1s cubic-bezier(0.4, 0, 0.2, 1); }
.extended-bar .bar { width: 13px; }
.bar-1 { background: var(--accent-color); }
.bar-2 { background: #cbd5e1; }
.bar-lbl { font-size: 11px; font-weight: 700; color: var(--r-text-sub); }

/* Circular Gauges */
.gauge-grid { display: flex; gap: 16px; justify-content: space-around; height: 100%; align-items: center; }
.mini-gauge { width: 70px; height: 70px; }
.mini-lbl { font-size: 10px; font-weight: 800; text-align: center; margin-top: -8px; display: block; }

/* Line Chart */
.line-chart-full { height: 180px; margin-top: 10px; }
.full-line-svg { width: 100%; height: 100%; overflow: visible; }
.chart-labels { display: flex; justify-content: space-between; padding: 0 10px; margin-top: 10px; }
.chart-labels span { font-size: 11px; font-weight: 600; color: var(--r-text-sub); }

/* Period filter selects in card headers */
/* Global period pill toggle */
.period-toggle-global {
  display: flex;
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  border-radius: 100px;
  padding: 3px;
  gap: 2px;
}
.period-toggle-global button {
  border: none;
  background: transparent;
  color: var(--r-text-sub);
  font-size: 11px;
  font-weight: 700;
  padding: 5px 12px;
  border-radius: 100px;
  cursor: pointer;
  transition: all 0.2s;
  letter-spacing: 0.3px;
  display: flex;
  align-items: center;
  gap: 6px;
}
.period-icon :deep(svg) {
  width: 14px;
  height: 14px;
}
.period-toggle-global button:hover { color: var(--r-text-main); }
.period-toggle-global button.active {
  background: var(--accent-color);
  color: #fff;
  box-shadow: 0 4px 12px var(--accent-soft);
}
.period-toggle-global button.active .period-icon :deep(svg) {
  color: #fff;
}

/* Activity List Premium Style */
.activity-list-premium {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.r-list-item-activity {
  display: flex;
  gap: 12px;
  padding: 12px;
  border-radius: 16px;
  transition: all 0.3s ease;
  cursor: pointer;
  border: 1px solid transparent;
}

.r-list-item-activity:hover {
  background: rgba(var(--accent-color-rgb), 0.03);
  border-color: var(--r-border);
  transform: translateX(4px);
}

.activity-avatar {
  width: 34px;
  height: 34px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 12px;
  flex-shrink: 0;
}

.activity-body {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 2px;
  justify-content: center;
}

.activity-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.activity-name {
  font-size: 12px;
  font-weight: 800;
  color: var(--r-text-main);
}

.activity-score-pill {
  padding: 2px 8px;
  border-radius: 6px;
  font-size: 11px;
  font-weight: 800;
}

.activity-meta {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--r-text-sub);
}

.activity-time {
  opacity: 0.7;
}

.activity-progress-wrap {
  margin-top: 6px;
  height: 4px;
  background: var(--r-border);
  border-radius: 10px;
  overflow: visible;
  width: 100%;
}

.activity-progress-bar {
  height: 100%;
  border-radius: 10px;
  transition: width 1s ease-out;
}

/* Talent Distribution Card Styles */
.talent-dist-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
  padding: 10px 0;
}

.talent-bar-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.talent-bar-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.talent-lbl {
  font-size: 13px;
  font-weight: 700;
  color: var(--r-text-main);
}

.talent-val {
  font-size: 12px;
  font-weight: 800;
  color: var(--r-text-sub);
}

.talent-track {
  height: 8px;
  background: var(--r-border);
  border-radius: 100px;
  overflow: visible;
}

.talent-fill {
  height: 100%;
  border-radius: 100px;
  transition: width 1.2s cubic-bezier(0.34, 1.56, 0.64, 1);
}

/* Skills Insights Styles */
.skills-insight-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  padding: 10px 0;
}

.skill-stat-pill {
  display: flex;
  align-items: center;
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  border-radius: 100px;
  overflow: visible;
  transition: all 0.3s ease;
}

.skill-stat-pill:hover {
  border-color: var(--accent-color);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px var(--accent-soft);
}

.skill-name-badge {
  padding: 8px 14px;
  font-size: 13px;
  font-weight: 700;
  color: var(--r-text-main);
}

.skill-count-badge {
  background: var(--accent-color);
  color: #000;
  padding: 8px 12px;
  font-size: 12px;
  font-weight: 900;
  min-width: 36px;
  text-align: center;
}
</style>
