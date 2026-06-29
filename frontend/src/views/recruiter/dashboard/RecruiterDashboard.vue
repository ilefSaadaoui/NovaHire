<template>
  <div class="dashboard-layout recruiter-layout">
    <Sidebar :active-item="'dashboard'" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main stagger-reveal" :class="{ 'ml-collapsed': sidebarCollapsed }">
      
      <header class="r-topbar">
        <div class="r-welcome elite-professional">
          <div class="welcome-text-wrap">
            <h1 class="lumina-text">{{ rawGreeting }}</h1>
            <p class="welcome-sub professional-sub" style="margin-top: 6px; font-size: 16px;">
              {{ $t('dashboard.recruiter.subtitle') }}
            </p>
          </div>
          <div class="professional-accent-line"></div>
        </div>
        <div class="r-header-tools">
          <div class="period-toggle-elite">
            <button 
              v-for="p in periodOptions" 
              :key="p.value" 
              :class="{ active: globalPeriod === p.value }" 
              @click="globalPeriod = p.value"
            >
              <component :is="p.lucideIcon" :size="14" class="period-icon-lucide" />
              {{ p.label }}
            </button>
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
            <div class="r-card kpi-card glass-panel-elite spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <div class="header-with-icon">
                  <Briefcase :size="18" class="card-icon-gold" />
                  <span class="r-card-title">{{ $t('dashboard.recruiter.myActiveOffers') }}</span>
                </div>
              </div>
              <div class="r-card-body">
                <div class="main-val counter-animate" v-counter>{{ dashData.activeJobOffers || 0 }}</div>
                <div class="r-trend" :class="trendClass(dashData.offersTrend)">
                  <component :is="trendIcon(dashData.offersTrend)" :size="12" stroke-width="3" />
                  {{ formatTrend(dashData.offersTrend) }} {{ $t('dashboard.recruiter.vsPreviousPeriod') }}
                </div>
                <div class="r-area-chart-container">
                  <svg viewBox="0 0 100 30" preserveAspectRatio="none" style="width: 100%; height: 100%;">
                    <defs>
                      <linearGradient id="chartGradientArea1" x1="0%" y1="0%" x2="0%" y2="100%">
                        <stop offset="0%" :stop-color="accentColor" stop-opacity="0.3"></stop>
                        <stop offset="100%" :stop-color="accentColor" stop-opacity="0"></stop>
                      </linearGradient>
                    </defs>
                    <path :d="offersSparklineArea" fill="url(#chartGradientArea1)" />
                    <path class="r-area-path" :d="offersSparklinePath" :stroke="accentColor" fill="none" />
                  </svg>
                </div>
              </div>
            </div>
            <!-- Total Applications -->
            <div class="r-card kpi-card glass-panel-elite spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <div class="header-with-icon">
                  <Users :size="18" class="card-icon-gold" />
                  <span class="r-card-title">{{ $t('dashboard.recruiter.totalApplications') }}</span>
                </div>
              </div>
              <div class="r-card-body">
                <div class="main-val counter-animate" v-counter>{{ dashData.totalApplications || 0 }}</div>
                <div class="r-trend" :class="trendClass(dashData.applicationsTrend)">
                  <component :is="trendIcon(dashData.applicationsTrend)" :size="12" stroke-width="3" />
                  {{ formatTrend(dashData.applicationsTrend) }} {{ $t('dashboard.recruiter.vsPreviousPeriod') }}
                </div>
                <div class="r-area-chart-container">
                  <svg viewBox="0 0 100 30" preserveAspectRatio="none" style="width: 100%; height: 100%;">
                    <defs>
                      <linearGradient id="chartGradientArea2" x1="0%" y1="0%" x2="0%" y2="100%">
                        <stop offset="0%" stop-color="#f472b6" stop-opacity="0.3"></stop>
                        <stop offset="100%" stop-color="#f472b6" stop-opacity="0"></stop>
                      </linearGradient>
                    </defs>
                    <path :d="applicationsSparklineArea" fill="url(#chartGradientArea2)" />
                    <path class="r-area-path" :d="applicationsSparklinePath" stroke="#f472b6" fill="none" />
                  </svg>
                </div>
              </div>
            </div>

            <!-- Planned Interviews -->
            <div class="r-card kpi-card glass-panel-elite spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <div class="header-with-icon">
                  <Calendar :size="18" class="card-icon-gold" />
                  <span class="r-card-title">{{ $t('dashboard.recruiter.plannedInterviews') || 'Planned Interviews' }}</span>
                </div>
              </div>
              <div class="r-card-body">
                <div class="bank-info">
                  <div class="bank-name">{{ $t('dashboard.recruiter.upcomingInterviews') }}</div>
                  <div class="card-type" :style="{ color: accentColor }" :class="trendClass(dashData.interviewsTrend)">
                    {{ formatTrend(dashData.interviewsTrend) }}
                  </div>
                </div>
                <div class="bank-amount" v-counter>{{ dashData.plannedInterviews || 0 }}</div>
                <div class="card-number">{{ $t('dashboard.recruiter.scheduledUpcoming') }}</div>
              </div>
            </div>

            <!-- Status Breakdown -->
            <div class="r-card kpi-card glass-panel-elite spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <div class="header-with-icon">
                  <BarChart3 :size="18" class="card-icon-gold" />
                  <span class="r-card-title">Status Breakdown</span>
                </div>
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
                    <span>Total</span>
                  </div>
                </div>
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
            <div class="r-card glass-panel-elite spotlight-card gold-glow-card span-2" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <div class="header-with-icon">
                  <Activity :size="18" class="card-icon-gold" />
                  <span class="r-card-title">{{ $t('dashboard.recruiter.recruitmentActivity') }}</span>
                </div>
                <div class="legend-pill-wrap">
                  <div class="legend-pill"><span class="dot" :style="{ background: accentColor }"></span> {{ $t('dashboard.recruiter.newApps') }}</div>
                  <div class="legend-pill"><span class="dot" style="background: #cbd5e1"></span> {{ $t('dashboard.recruiter.processed') }}</div>
                </div>
              </div>
              <div class="bar-chart-container extended-bar">
                <div v-for="(count, idx) in monthlyApplicationsSeries" :key="idx" class="bar-group">
                  <div class="bar-pair">
                    <div class="bar bar-1" :style="{ height: getBarHeight(count) + '%', background: accentColor }" :title="`${$t('dashboard.recruiter.newApps')}: ${count}`">
                      <span class="bar-hover-value">{{ count }}</span>
                    </div>
                    <div class="bar bar-2" :style="{ height: getBarHeight(monthlyProcessedSeries[idx]) + '%' }" :title="`${$t('dashboard.recruiter.processed')}: ${monthlyProcessedSeries[idx]}`">
                      <span class="bar-hover-value">{{ monthlyProcessedSeries[idx] }}</span>
                    </div>
                  </div>
                  <span class="bar-lbl">{{ getChartLabel(idx) }}</span>
                </div>
              </div>
            </div>

            <!-- Applications Trend -->
            <div class="r-card glass-panel-elite spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <div class="header-with-icon">
                  <TrendingUp :size="18" class="card-icon-gold" />
                  <span class="r-card-title">{{ $t('dashboard.recruiter.matchingTrends') }}</span>
                </div>
              </div>
              <div class="circular-stats-grid gauge-grid">
                <!-- AI Analysis Rate -->
                <div class="gauge-wrapper">
                  <div class="r-doughnut mini-gauge">
                    <svg viewBox="0 0 36 36" class="circular-chart">
                      <path class="circle-bg" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" style="stroke: var(--r-border); stroke-width: 4; fill: none;"></path>
                      <path class="circle" stroke="#10b981" :stroke-dasharray="aiPercentage + ', 100'" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" style="stroke-width: 4; fill: none;"></path>
                    </svg>
                    <div class="r-doughnut-val">
                      <span style="font-size: 16px;">{{ aiPercentage }}%</span>
                    </div>
                  </div>
                  <span class="mini-lbl" style="color: #10b981;">{{ $t('dashboard.recruiter.analysisRate') }}</span>
                </div>
                <!-- Processing Rate -->
                <div class="gauge-wrapper">
                  <div class="r-doughnut mini-gauge">
                    <svg viewBox="0 0 36 36" class="circular-chart">
                      <path class="circle-bg" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" style="stroke: var(--r-border); stroke-width: 4; fill: none;"></path>
                      <path class="circle" stroke="#818cf8" :stroke-dasharray="processedPercentage + ', 100'" d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" style="stroke-width: 4; fill: none;"></path>
                    </svg>
                    <div class="r-doughnut-val">
                      <span style="font-size: 16px;">{{ processedPercentage }}%</span>
                    </div>
                  </div>
                  <span class="mini-lbl" style="color: #818cf8;">{{ $t('dashboard.recruiter.processingRate') }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Skills & Quality Row -->
          <div class="dashboard-grid skills-row recruiter-grid">

            <!-- Talent Quality Distribution -->
            <div class="r-card glass-panel-elite spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <div class="header-with-icon">
                  <CheckCircle2 :size="18" class="card-icon-gold" />
                  <span class="r-card-title">{{ $t('dashboard.recruiter.qualityDistribution') }}</span>
                </div>
              </div>
              <div class="talent-dist-content">
                <div v-if="!dashData.talentDistribution || !dashData.talentDistribution.length" class="empty-notif" style="padding: 20px;">
                  {{ $t('dashboard.recruiter.awaitingAiAnalyses') }}
                </div>
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

            <!-- Top Skills -->
            <div class="r-card glass-panel-elite spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <div class="header-with-icon">
                  <Search :size="18" class="card-icon-gold" />
                  <span class="r-card-title">{{ $t('dashboard.recruiter.topSkills') }}</span>
                </div>
              </div>
              <div class="skills-insight-grid">
                <div v-for="skill in dashData.topSkills" :key="skill.name" class="skill-stat-pill">
                  <span class="skill-name-badge">{{ skill.name }}</span>
                  <div class="skill-count-badge">{{ skill.count }}</div>
                </div>
                <div v-if="!dashData.topSkills || !dashData.topSkills.length" class="empty-notif" style="padding: 20px;">
                  {{ $t('dashboard.recruiter.awaitingAiAnalyses') }}
                </div>
              </div>
            </div>
          </div>

          <!-- Bottom Row -->
          <div class="dashboard-grid bottom-row recruiter-grid">
            <!-- Recent Offers -->
            <div class="r-card glass-panel-elite spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <div class="header-with-icon">
                  <Briefcase :size="18" class="card-icon-gold" />
                  <span class="r-card-title">{{ $t('dashboard.recruiter.recentOffers') }}</span>
                </div>
              </div>
              <div class="r-list">
                <div v-if="!dashData.recentOffers || !dashData.recentOffers.length" class="empty-notif" style="padding: 20px;">
                  {{ $t('dashboard.recruiter.noRecentOffers') }}
                </div>
                <div v-for="offer in (dashData.recentOffers || []).slice(0, 4)" :key="offer.id" class="r-list-item">
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
            <div class="r-card glass-panel-elite spotlight-card gold-glow-card" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <div class="header-with-icon">
                  <Activity :size="18" class="card-icon-gold" />
                  <span class="r-card-title">{{ $t('dashboard.recruiter.recentActivities') }}</span>
                </div>
              </div>
              <div class="r-list activity-list-premium">
                <div v-if="!dashData.recentActivities || !dashData.recentActivities.length" class="empty-notif" style="padding: 20px;">
                  {{ $t('dashboard.recruiter.noRecentActivities') }}
                </div>
                <div v-for="act in (dashData.recentActivities || []).slice(0, 5)" :key="act.title" class="r-list-item-activity">
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

            <!-- Global Flow -->
            <div class="r-card glass-panel-elite spotlight-card gold-glow-card span-2" @mousemove="handleSpotlight">
              <div class="r-card-header">
                <div class="header-with-icon">
                  <TrendingUp :size="18" class="card-icon-gold" />
                  <span class="r-card-title">{{ $t('dashboard.recruiter.applicationFlow') }}</span>
                </div>
              </div>
              <div class="flow-legend">
                <div class="legend-pill"><span class="dot" :style="{ background: accentColor }"></span> {{ $t('dashboard.recruiter.received') }}</div>
                <div class="legend-pill"><span class="dot" style="background: #818cf8"></span> {{ $t('dashboard.recruiter.processed') }}</div>
              </div>
              <div class="line-chart-full">
                <svg viewBox="0 0 800 200" class="full-line-svg" preserveAspectRatio="none">
                  <path v-if="flowChartPaths.received" :d="flowChartPaths.received" fill="none" :stroke="accentColor" stroke-width="3.5" />
                  <circle v-if="flowChartPaths.received" :cx="flowChartPaths.receivedEnd.x" :cy="flowChartPaths.receivedEnd.y" r="6" :fill="accentColor" />
                  <path v-if="flowChartPaths.processed" :d="flowChartPaths.processed" fill="none" stroke="#818cf8" stroke-width="3.5" opacity="0.8" />
                </svg>
                <div class="chart-labels">
                  <span v-for="lbl in weeklyLabels" :key="lbl">{{ lbl }}</span>
                </div>
              </div>
            </div>
          </div>
        </template>
      </div>
    </main>
  </div>
</template>

<script>
import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'
import Sidebar from '@/components/layout/Sidebar.vue'

import {
  Users,
  Briefcase,
  Calendar,
  TrendingUp,
  TrendingDown,
  Minus,
  Activity,
  Clock,
  BarChart3,
  CheckCircle2,
  LayoutDashboard,
  Search,
  MessageSquare
} from 'lucide-vue-next'
import { onMounted, onUnmounted } from 'vue'
import { useToastStore } from '@/stores/toastStore'
import api from '@/api/axios'


export default {
  name: 'RecruiterDashboard',
  components: { 
    Sidebar,
    Users, 
    Briefcase,
    Calendar,
    TrendingUp,
    TrendingDown,
    Minus,
    Activity,
    Clock, 
    BarChart3, 
    CheckCircle2, 
    LayoutDashboard,
    Search,
    MessageSquare
  },
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
          lucideIcon: Clock
        },
        { 
          label: '7j',  
          value: 'week',
          lucideIcon: Calendar
        },
        { 
          label: '30j', 
          value: 'month',
          lucideIcon: Activity
        }
      ],
      dashData: {
        activeJobOffers: 0,
        totalApplications: 0,
        aiAnalysesCount: 0,
        plannedInterviews: 0,
        offersTrend: 0,
        applicationsTrend: 0,
        interviewsTrend: 0,
        processedApplications: 0,
        avgApplicationsPerOffer: 0,
        recentOffers: [],
        recentActivities: [],
        monthlyApplications: [],
        monthlyProcessed: [],
        weeklyFlow: [],
        weeklyProcessed: [],
        statusBreakdown: [],
        talentDistribution: [],
        topSkills: []
      },
      // Animation State
      animatedGreeting: '',
      rawGreeting: ''
    }
  },
  async mounted() {
    await this.fetchDashboard()
    this.initProfessionalGreeting()
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
    processedPercentage() {
      if (!this.dashData.totalApplications) return 0;
      return Math.round((this.dashData.processedApplications / this.dashData.totalApplications) * 100);
    },
    expectedBarPoints() {
      return this.globalPeriod === 'week' ? 7 : 6;
    },
    monthlyApplicationsSeries() {
      return this.normalizeSeries(this.dashData.monthlyApplications, this.expectedBarPoints);
    },
    monthlyProcessedSeries() {
      return this.normalizeSeries(this.dashData.monthlyProcessed, this.expectedBarPoints);
    },
    weeklyFlowSeries() {
      return this.normalizeSeries(this.dashData.weeklyFlow, 7);
    },
    weeklyProcessedSeries() {
      return this.normalizeSeries(this.dashData.weeklyProcessed, 7);
    },
    maxApps() {
      const apps = this.monthlyApplicationsSeries;
      const processed = this.monthlyProcessedSeries;
      return Math.max(...apps, ...processed, 10);
    },
    offersSparklinePath() {
      return this.buildSparkline(this.monthlyApplicationsSeries);
    },
    offersSparklineArea() {
      return this.buildSparklineArea(this.monthlyApplicationsSeries);
    },
    applicationsSparklinePath() {
      const series = this.monthlyApplicationsSeries.map((v, i) => v + (this.monthlyProcessedSeries[i] || 0));
      return this.buildSparkline(series);
    },
    applicationsSparklineArea() {
      const series = this.monthlyApplicationsSeries.map((v, i) => v + (this.monthlyProcessedSeries[i] || 0));
      return this.buildSparklineArea(series);
    },
    weeklyLabels() {
      const keys = ['sun', 'mon', 'tue', 'wed', 'thu', 'fri', 'sat'];
      const labels = [];
      const today = new Date();
      for (let i = 6; i >= 0; i--) {
        const d = new Date(today);
        d.setDate(d.getDate() - i);
        labels.push(this.$t(`dashboard.recruiter.days.${keys[d.getDay()]}`));
      }
      return labels;
    },
    flowChartPaths() {
      const received = this.weeklyFlowSeries;
      const processed = this.weeklyProcessedSeries;
      const peak = Math.max(...received, ...processed, 1);
      const toPath = (arr) => {
        if (!arr.length) return '';
        const step = 800 / Math.max(arr.length - 1, 1);
        return arr.map((v, i) => {
          const x = i * step;
          const y = 190 - (v / peak) * 170;
          return `${i === 0 ? 'M' : 'L'}${x.toFixed(1)},${y.toFixed(1)}`;
        }).join(' ');
      };
      const last = (arr) => arr.length ? arr[arr.length - 1] : 0;
      return {
        received: toPath(received),
        processed: toPath(processed),
        receivedEnd: {
          x: 800,
          y: 190 - (last(received) / peak) * 170
        }
      };
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
    normalizeSeries(series, expectedLength) {
      const source = Array.isArray(series)
        ? series
        : [];
      const normalized = source.map((value) => {
        const num = Number(value);
        return Number.isFinite(num) ? num : 0;
      });

      if (normalized.length >= expectedLength) {
        return normalized.slice(0, expectedLength);
      }

      return normalized.concat(Array(expectedLength - normalized.length).fill(0));
    },
    getBarHeight(count) {
      return Math.max((count / this.maxApps) * 100, 5);
    },
    buildSparkline(series) {
      if (!series || series.length === 0) return 'M0,20 L100,20';
      const peak = Math.max(...series, 1);
      const step = 100 / Math.max(series.length - 1, 1);
      return series.map((v, i) => {
        const x = i * step;
        const y = 28 - (v / peak) * 24;
        return `${i === 0 ? 'M' : 'L'}${x.toFixed(1)},${y.toFixed(1)}`;
      }).join(' ');
    },
    buildSparklineArea(series) {
      if (!series || series.length === 0) return 'M0,30 L100,30 Z';
      const peak = Math.max(...series, 1);
      const step = 100 / Math.max(series.length - 1, 1);
      const parts = series.map((v, i) => {
        const x = i * step;
        const y = 28 - (v / peak) * 24;
        return `${i === 0 ? 'M' : 'L'}${x.toFixed(1)},${y.toFixed(1)}`;
      });
      parts.push(`L100,30`);
      parts.push(`L0,30 Z`);
      return parts.join(' ');
    },
    formatTrend(value) {
      if (value === null || value === undefined || isNaN(value)) return '0%';
      const sign = value > 0 ? '+' : '';
      return `${sign}${value.toFixed(1)}%`;
    },
    trendClass(value) {
      if (value > 0) return 'up';
      if (value < 0) return 'down';
      return 'flat';
    },
    trendIcon(value) {
      if (value > 0) return TrendingUp;
      if (value < 0) return TrendingDown;
      return Minus;
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
        const payload = res?.data || {}
        
        const pickArray = (...candidates) => {
          const found = candidates.find(Array.isArray)
          return found || []
        }

        const pickVal = (v1, v2, fallback) => {
          if (v1 !== undefined && v1 !== null) return v1
          if (v2 !== undefined && v2 !== null) return v2
          return fallback
        }

        this.dashData = {
          ...this.dashData,
          ...payload,
          activeJobOffers: pickVal(payload.activeJobOffers, payload.ActiveJobOffers, this.dashData.activeJobOffers),
          totalApplications: pickVal(payload.totalApplications, payload.TotalApplications, this.dashData.totalApplications),
          aiAnalysesCount: pickVal(payload.aiAnalysesCount, payload.AiAnalysesCount, this.dashData.aiAnalysesCount),
          plannedInterviews: pickVal(payload.plannedInterviews, payload.PlannedInterviews, this.dashData.plannedInterviews),
          processedApplications: pickVal(payload.processedApplications, payload.ProcessedApplications, this.dashData.processedApplications),
          offersTrend: pickVal(payload.offersTrend, payload.OffersTrend, this.dashData.offersTrend),
          applicationsTrend: pickVal(payload.applicationsTrend, payload.ApplicationsTrend, this.dashData.applicationsTrend),
          interviewsTrend: pickVal(payload.interviewsTrend, payload.InterviewsTrend, this.dashData.interviewsTrend),
          avgApplicationsPerOffer: pickVal(payload.avgApplicationsPerOffer, payload.AvgApplicationsPerOffer, this.dashData.avgApplicationsPerOffer),
          
          monthlyApplications: pickArray(payload.monthlyApplications, payload.MonthlyApplications),
          monthlyProcessed: pickArray(payload.monthlyProcessed, payload.MonthlyProcessed),
          weeklyFlow: pickArray(payload.weeklyFlow, payload.WeeklyFlow),
          weeklyProcessed: pickArray(payload.weeklyProcessed, payload.WeeklyProcessed),
          recentOffers: pickArray(payload.recentOffers, payload.RecentOffers),
          recentActivities: pickArray(payload.recentActivities, payload.RecentActivities),
          statusBreakdown: pickArray(payload.statusBreakdown, payload.StatusBreakdown),
          talentDistribution: pickArray(payload.talentDistribution, payload.TalentDistribution),
          topSkills: pickArray(payload.topSkills, payload.TopSkills)
        }
      } catch (error) {
        console.error('Erreur dashboard:', error)
      } finally {
        this.loading = false
      }
    },
    formatStatus(status) {
      const map = {
        'submitted': 'Soumis',
        'shortlisted': 'Retenu',
        'interview': 'Entretien',
        'interviewed': 'Réalisé',
        'underreview': 'Évaluation',
        'rejected': 'Refusé',
        'accepted': 'Accepté'
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

.dashboard-layout { 
  background: transparent !important; 
  min-height: 100vh;
}

.glass-panel-elite {
  background: var(--glass-bg);
  backdrop-filter: blur(25px) saturate(200%);
  border: 1px solid var(--glass-border-color);
  border-radius: 32px;
  box-shadow: var(--premium-shadow);
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  position: relative;
  overflow: hidden;
}

.glass-panel-elite:hover {
  border-color: var(--accent-color);
  box-shadow: 0 20px 40px rgba(0,0,0,0.4), 0 0 20px var(--accent-soft);
  transform: translateY(-4px);
}

.gold-glow-card::after {
  content: '';
  position: absolute;
  inset: 0;
  background: radial-gradient(circle at var(--mouse-x) var(--mouse-y), var(--accent-soft) 0%, transparent 60%);
  opacity: 0;
  transition: opacity 0.3s ease;
  pointer-events: none;
  border-radius: inherit;
  z-index: 1;
}

.gold-glow-card:hover::after { opacity: 0.4; }
</style>

<style scoped>
@import '@/assets/dashboard.css';

/* Dashboard Layout Specifics */
.recruiter-grid {
  display: grid;
  gap: 24px;
  margin-bottom: 24px;
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
.main-val { font-size: 36px; font-weight: 900; color: var(--text-main); line-height: 1.1; letter-spacing: -1px; }
.bank-info { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
.bank-name { font-size: 13px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.5px; }
.card-type { font-size: 10px; font-weight: 900; letter-spacing: 1.5px; background: var(--accent-soft); padding: 2px 8px; border-radius: 4px; }
.bank-amount { font-size: 44px; font-weight: 900; margin: 12px 0 4px; color: var(--text-main); line-height: 1; }
.card-number { font-size: 11px; color: var(--text-muted); letter-spacing: 1px; font-weight: 600; text-transform: uppercase; opacity: 0.7; }

/* Charts */
.legend-list { display: grid; gap: 8px; margin-top: 16px; }
.grid-legend { grid-template-columns: 1fr 1fr; }
.legend-item { display: flex; align-items: center; gap: 8px; font-size: 12px; font-weight: 700; color: var(--text-muted); }
.legend-item .dot { width: 8px; height: 8px; border-radius: 50%; box-shadow: 0 0 5px currentColor; }
.legend-pill-wrap { display: flex; gap: 12px; flex-wrap: wrap; }
.legend-pill { display: flex; align-items: center; gap: 6px; font-size: 11px; font-weight: 700; color: var(--text-muted); }
.legend-pill .dot { width: 6px; height: 6px; border-radius: 50%; }

/* Header Tools */
.period-toggle-elite {
  display: flex;
  background: rgba(255, 255, 255, 0.03);
  backdrop-filter: blur(10px);
  border: 1px solid var(--glass-border-color);
  border-radius: 100px;
  padding: 4px;
  gap: 4px;
}

.period-toggle-elite button {
  border: none;
  background: transparent;
  color: var(--text-muted);
  font-size: 11px;
  font-weight: 800;
  padding: 8px 16px;
  border-radius: 100px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  letter-spacing: 0.5px;
  display: flex;
  align-items: center;
  gap: 8px;
  text-transform: uppercase;
}

.period-toggle-elite button:hover { color: var(--text-main); background: rgba(255, 255, 255, 0.05); }

.period-toggle-elite button.active {
  background: var(--accent-grad);
  color: #000;
  box-shadow: 0 4px 15px var(--accent-glow);
}

.header-with-icon { display: flex; align-items: center; gap: 12px; }
.card-icon-gold { color: var(--accent); filter: drop-shadow(0 0 5px var(--accent-soft)); }

/* Activity List Premium Style */
.activity-list-premium { display: flex; flex-direction: column; gap: 8px; }

.r-list-item-activity {
  display: flex; gap: 16px; padding: 16px; border-radius: 20px;
  transition: all 0.3s ease; cursor: pointer;
  background: rgba(255,255,255,0.02);
  border: 1px solid var(--glass-border-color);
}

.r-list-item-activity:hover {
  background: rgba(255,255,255,0.05);
  transform: translateX(6px);
  border-color: var(--accent-soft);
}

.activity-avatar {
  width: 48px; height: 48px; border-radius: 14px;
  display: flex; align-items: center; justify-content: center;
  font-weight: 900; font-size: 16px; flex-shrink: 0;
  box-shadow: inset 0 0 10px rgba(0,0,0,0.2);
}

.activity-body { flex: 1; display: flex; flex-direction: column; gap: 4px; }
.activity-top { display: flex; justify-content: space-between; align-items: center; }
.activity-name { font-size: 15px; font-weight: 900; color: var(--text-main); }
.activity-score-pill { padding: 4px 10px; border-radius: 8px; font-size: 11px; font-weight: 900; }
.activity-meta { display: flex; align-items: center; gap: 8px; font-size: 12px; color: var(--text-muted); font-weight: 600; }
.activity-progress-wrap { margin-top: 8px; height: 5px; background: rgba(255,255,255,0.05); border-radius: 10px; overflow: hidden; width: 100%; border: 1px solid var(--border-thin); }
.activity-progress-bar { height: 100%; border-radius: 10px; transition: width 1.5s cubic-bezier(0.16, 1, 0.3, 1); }

/* Talent Distribution */
.talent-dist-content { display: flex; flex-direction: column; gap: 20px; padding: 10px 0; }
.talent-bar-item { display: flex; flex-direction: column; gap: 10px; }
.talent-bar-info { display: flex; justify-content: space-between; align-items: center; }
.talent-lbl { font-size: 14px; font-weight: 800; color: var(--text-main); }
.talent-val { font-size: 12px; font-weight: 900; color: var(--text-muted); text-transform: uppercase; }
.talent-track { height: 8px; background: rgba(255,255,255,0.05); border-radius: 100px; border: 1px solid var(--border-thin); overflow: hidden; }
.talent-fill { height: 100%; border-radius: 100px; transition: width 1.2s cubic-bezier(0.34, 1.56, 0.64, 1); }

/* Skills Insights */
.skills-insight-grid { display: flex; flex-wrap: wrap; gap: 12px; padding: 10px 0; }
.skill-stat-pill {
  display: flex; align-items: center; background: rgba(255,255,255,0.03);
  border: 1px solid var(--glass-border-color); border-radius: 100px;
  overflow: hidden; transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.skill-stat-pill:hover {
  background: rgba(255,255,255,0.08);
  border-color: var(--accent);
  transform: translateY(-3px) scale(1.05);
  box-shadow: 0 10px 20px var(--accent-glow);
}

.skill-name-badge { padding: 10px 16px; font-size: 14px; font-weight: 800; color: var(--text-main); }
.skill-count-badge { background: var(--accent); color: #000; padding: 10px 14px; font-size: 12px; font-weight: 900; min-width: 40px; text-align: center; }

/* Charts */
.bar-chart-container { display: flex; align-items: flex-end; justify-content: space-between; height: 200px; padding-top: 20px; }
.bar-group { display: flex; flex-direction: column; align-items: center; gap: 12px; height: 100%; justify-content: flex-end; flex: 1; min-width: 0; }
.bar-pair { display: flex; gap: 4px; align-items: flex-end; height: calc(100% - 24px); width: 24px; }
.extended-bar .bar-pair { width: 32px; gap: 6px; }
.bar { width: 10px; border-radius: 10px 10px 0 0; box-shadow: 0 0 15px rgba(0,0,0,0.2); position: relative; }
.extended-bar .bar { width: 13px; }
.bar-2 { background: #cbd5e1; }
.bar-lbl { font-size: 11px; font-weight: 700; color: var(--text-muted); }
.bar-hover-value {
  position: absolute;
  left: 50%;
  bottom: calc(100% + 8px);
  transform: translate(-50%, 4px);
  padding: 4px 8px;
  border-radius: 8px;
  font-size: 11px;
  font-weight: 800;
  line-height: 1;
  color: #fff;
  background: rgba(15, 23, 42, 0.92);
  border: 1px solid rgba(148, 163, 184, 0.35);
  white-space: nowrap;
  opacity: 0;
  pointer-events: none;
  transition: opacity 0.2s ease, transform 0.2s ease;
  z-index: 5;
}
.bar:hover .bar-hover-value {
  opacity: 1;
  transform: translate(-50%, 0);
}
.line-chart-full { height: 260px; margin-top: 20px; filter: drop-shadow(0 0 10px var(--accent-soft)); display: flex; flex-direction: column; }
.full-line-svg { flex: 1; min-height: 0; width: 100%; }
.chart-labels {
  display: flex;
  justify-content: space-between;
  padding: 10px 20px 0;
  font-size: 13px;
  font-weight: 700;
  color: var(--text-muted);
}

/* Custom Gauge Grid Layout */
.gauge-grid {
  display: flex;
  justify-content: space-around;
  align-items: center;
  padding: 20px 0 10px;
  gap: 16px;
}
.gauge-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}
.gauge-wrapper .mini-lbl {
  font-size: 13px;
  font-weight: 700;
  text-align: center;
}

:deep(.r-trend.flat) { color: var(--text-muted); background: rgba(148, 163, 184, 0.1); }

.flow-legend {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  margin-top: 4px;
}
.flow-legend .legend-pill {
  display: flex;
  align-items: center;
  gap: 8px;
  background: rgba(255,255,255,0.03);
  border: 1px solid var(--glass-border-color);
  border-radius: 100px;
  padding: 6px 12px;
  font-size: 11px;
  font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.flow-legend .dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  box-shadow: 0 0 6px currentColor;
}

.empty-notif {
  text-align: center;
  font-size: 13px;
  color: var(--text-muted);
  font-weight: 600;
  font-style: italic;
}
</style>
