<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="candidatures" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      
      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome">
          <div class="header-with-icon" style="flex-direction: row; align-items: center; gap: 16px;">
            <div class="icon-box-themed" style="width: 52px; height: 52px; border-radius: 16px;">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 26px;"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
            </div>
            <div>
              <h1 class="premium-title-themed" style="font-size: 28px;">{{ $t('pipeline.title') }}</h1>
              <p class="welcome-sub" style="margin-top: 4px;">{{ $t('pipeline.subtitle', { count: totalCandidates, offer: selectedOfferName }) }}</p>
            </div>
          </div>
        </div>
      </header>

      <!-- ACTION BAR -->
      <div class="r-card filter-card anim-reveal-down">
        <div class="action-left">
          <PremiumSelect 
            v-model="selectedOfferId" 
            :options="offerOptions" 
            @change="fetchApplications" 
            class="expert-filter"
            style="min-width: 260px;"
          />
        </div>
        
        <div class="action-right-group">
          <!-- BATTLE MODE TOGGLE -->
          <button 
            class="btn-premium battle-mode-btn" 
            :class="{ active: compareMode }"
            @click="toggleCompareMode"
          >
            <Sword :size="18" />
            <span>Battle Mode</span>
            <div v-if="selectedForComparison.length > 0" class="selection-counter">
              {{ selectedForComparison.length }}/2
            </div>
          </button>

          <button 
            v-if="compareMode" 
            class="btn-luxury primary launch-battle-btn" 
            :disabled="!canLaunchBattleComparison"
            :title="battleCompareButtonTitle"
            @click="launchBattleComparison"
          >
            Comparer 2 ({{ selectedForComparison.length }}/2)
          </button>

          <div class="view-switch">
            <button :class="{ active: viewMode === 'kanban' }" @click="viewMode = 'kanban'" :title="$t('pipeline.view.kanban')">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="3" width="4" height="18"/><rect x="10" y="3" width="4" height="18"/><rect x="17" y="3" width="4" height="10"/></svg>
            </button>
            <button :class="{ active: viewMode === 'list' }" @click="viewMode = 'list'" :title="$t('pipeline.view.list')">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/></svg>
            </button>
          </div>

        </div>
      </div>

      <div class="page-body">
        <div v-if="isLoading" class="loading-state">
          <div class="spinner"></div>
          <p>{{ $t('pipeline.loading') }}</p>
        </div>

        <div v-else-if="allCandidates.length === 0" class="r-card anim-reveal-up" style="align-items: center; justify-content: center; padding: 60px 20px; text-align: center;">
          <div style="background: var(--r-main-bg); width: 64px; height: 64px; border-radius: 20px; display: flex; align-items: center; justify-content: center; color: var(--r-text-sub); margin-bottom: 24px; border: 1px solid var(--r-border);">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width: 32px;"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
          </div>
          <h3 style="font-size: 18px; font-weight: 800; color: var(--r-text-main); margin-bottom: 8px;">{{ $t('pipeline.empty') }}</h3>
          <p style="font-size: 14px; font-weight: 500; color: var(--r-text-sub);">{{ $t('pipeline.emptySub') }}</p>
        </div>

        <!-- KANBAN BOARD -->
        <div v-else-if="viewMode === 'kanban'" class="premium-kanban">
          <div v-for="(col, index) in columns" 
            :key="col.id" 
            class="kanban-column anim-stagger-up" 
            :style="{ animationDelay: `${0.2 + (index * 0.1)}s` }"
            @dragover.prevent="handleDragOver($event, col.id)"
            @dragleave="handleDragLeave($event, col.id)"
            @drop="handleDrop($event, col.id)"
            :class="{ 'drag-over': activeDropZone === col.id }"
          >
            <div class="kanban-header-premium">
              <div class="header-main">
                <div class="status-indicator" :style="{ background: col.color }"></div>
                <h3>{{ col.title }}</h3>
              </div>
              <div style="display: flex; align-items: center; gap: 10px;">
                <button 
                  v-if="col.id === 'shortlisted' && col.candidates.length > 0" 
                  class="btn-bulk-send" 
                  @click.stop="sendQuizToAllStage(col.id)"
                  :disabled="sendingBulk"
                  title="Envoyer le quiz à tous les candidats présélectionnés"
                >
                  <i class="fas" :class="sendingBulk ? 'fa-spinner fa-spin' : 'fa-paper-plane'"></i>
                </button>
                <span class="column-count">{{ col.candidates.length }}</span>
              </div>
            </div>
            
            <div class="kanban-content">
              <div v-for="c in col.candidates" 
                :key="c.id" 
                class="r-card candidate-card anim-reveal-up" 
                @click="openCandidate(c)"
                draggable="true"
                @dragstart="handleDragStart($event, c, col.id)"
                @dragend="handleDragEnd"
              >
                <div class="card-top" style="display: flex; gap: 12px; margin-bottom: 12px;">
                  <div
                    v-if="compareMode"
                    class="card-selection-check"
                    :class="{ 'selection-disabled': isCompareSelectionDisabled(c.id) }"
                    @click.stop="toggleCandidateSelection(c)"
                  >
                    <div class="check-box" :class="{ checked: isSelected(c.id), disabled: isCompareSelectionDisabled(c.id) }">
                      <Check v-if="isSelected(c.id)" :size="14" />
                    </div>
                  </div>
                  <div v-else class="cand-avatar premium-avatar" :style="{ '--avatar-color': c.avatarBg }">
                    {{ c.initials }}
                  </div>
                  <div class="card-meta" style="flex: 1;">
                    <h4 class="r-item-title" style="font-size: 15px; margin: 0; line-height: 1.2;">{{ c.name }}</h4>
                    <p class="r-item-sub" style="font-size: 12px; margin-top: 2px;">{{ c.role }}</p>
                  </div>
                  <div v-if="c.score !== null && c.score !== undefined" class="ia-chip-premium" :class="scoreClass(c.score)">
                    <svg class="ia-spark" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2L15.09 8.26L22 9.27L17 14.14L18.18 21.02L12 17.77L5.82 21.02L7 14.14L2 9.27L8.91 8.26L12 2Z"/></svg>
                    <span>{{ c.score }}%</span>
                    <!-- Elite Badge -->
                    <div v-if="c.score >= 85" class="elite-badge-mini" title="Candidat Elite (Sélection Automatique)">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width: 10px;"><path d="M6 9l6 6 6-6"/></svg>
                    </div>
                  </div>
                  <button v-else class="ia-analyze-btn-outer" @click.stop="analyzeCandidate(c)" :class="{ 'is-analyzing': c.isAnalyzing }">
                    <div class="ia-spark-container">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v2m0 16v2M4.93 4.93l1.41 1.41m11.32 11.32l1.41 1.41M2 12h2m16 0h2M6.34 17.66l-1.41 1.41M19.07 4.93l-1.41 1.41"/></svg>
                    </div>
                    <span class="btn-text">IA</span>
                  </button>
                </div>

                <!-- Screening Note Indicator -->
                <div class="card-note-stripe" @click.stop="handleOpenNote(c)" :class="{ 'has-note': c.notes }">
                  <div class="note-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                  </div>
                  <p class="note-text">{{ c.notes || 'Note...' }}</p>
                </div>
                
                <div class="card-skills">
                  <span v-for="s in c.skills.slice(0,3)" :key="s" class="mini-tag">{{ s }}</span>
                  <span v-if="c.skills.length > 3" class="mini-tag count">+{{ c.skills.length - 3 }}</span>
                </div>
                
                <div class="card-footer">
                  <div class="date-tag">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width: 11px;"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
                    {{ c.date }}
                  </div>
                  
                  <!-- Collaborative Info -->
                  <div v-if="c.commentsCount > 0" class="card-collaboration-tag" :title="c.commentsCount + ' commentaire(s)'">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
                    <span>{{ c.commentsCount }}</span>
                  </div>

                  <div class="quick-actions" style="display: flex; gap: 6px;">
                    <!-- Actions based on column -->
                    <template v-if="col.id === 'submitted'">
                      <button class="action-btn small accent" @click.stop="moveCard(c, col.id, 'shortlisted')" :title="$t('pipeline.actions.shortlist')">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="9 18 15 12 9 6"/></svg>
                      </button>
                    </template>
                    <template v-else-if="col.id === 'shortlisted'">
                      <!-- Button removed as per user request (available in detail view) -->
                    </template>
                    <template v-else-if="col.id === 'interview'">
                      <button class="action-btn small success" @click.stop="confirmMarkInterviewed(c)" :title="$t('pipeline.actions.done')">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg>
                      </button>
                    </template>
                    <template v-else-if="col.id === 'interviewed'">
                      <button class="action-btn small accent" @click.stop="moveCard(c, col.id, 'underreview')" :title="$t('pipeline.actions.evaluate')">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>
                      </button>
                    </template>
                    <template v-else-if="col.id === 'underreview'">
                      <button class="action-btn small success" @click.stop="moveCard(c, col.id, 'accepted')" title="Engager">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg>
                      </button>
                    </template>

                    <button v-if="col.id !== 'rejected' && col.id !== 'accepted'" class="action-btn small danger" style="margin-left: 4px;" @click.stop="moveCard(c, col.id, 'rejected')" :title="$t('pipeline.actions.reject')">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- LIST VIEW -->
        <div v-else class="premium-list-wrap anim-reveal-up">
          <div class="list-header-row">
            <div class="col-rank">#</div>
            <div class="col-cand">{{ $t('pipeline.table.candidate') }}</div>
            <div class="col-score">{{ $t('pipeline.table.score') }}</div>
            <div class="col-skills">{{ $t('pipeline.table.skills') }}</div>
            <div class="col-stage">{{ $t('pipeline.table.stage') }}</div>
            <div class="col-date">{{ $t('pipeline.table.date') }}</div>
            <div class="col-actions">{{ $t('pipeline.table.action') }}</div>
          </div>
          
          <div v-for="(c, idx) in rankedCandidates" :key="c.id" class="list-item-premium" @click="openCandidate(c)">
            <div class="col-rank">
              <div class="rank-badge" :class="getRankClass(idx + 1)">
                <span v-if="idx === 0">🥇</span>
                <span v-else-if="idx === 1">🥈</span>
                <span v-else-if="idx === 2">🥉</span>
                <span v-else class="rank-num">#{{ idx + 1 }}</span>
              </div>
            </div>
            <div class="col-cand">
              <div class="cand-info-premium">
                <div class="cand-avatar-sm" :style="{ '--avatar-color': c.avatarBg }">{{ c.initials }}</div>
                <div class="cand-details-sm">
                  <div class="cand-name-sm">{{ c.name }}</div>
                  <div class="cand-role-sm">{{ c.role }}</div>
                </div>
              </div>
            </div>
            
            <div class="col-score">
              <div v-if="c.score !== null && c.score !== undefined" class="ia-chip-compact" :class="scoreClass(c.score)">
                <svg class="ia-spark" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2L15.09 8.26L22 9.27L17 14.14L18.18 21.02L12 17.77L5.82 21.02L7 14.14L2 9.27L8.91 8.26L12 2Z"/></svg>
                <span>{{ c.score }}%</span>
              </div>
              <span v-else class="score-pending-sm">—</span>
            </div>
            
            <div class="col-skills">
              <div class="skills-tags-sm">
                <span v-for="s in c.skills.slice(0,2)" :key="s" class="mini-tag-sm">{{ s }}</span>
                <span v-if="c.skills.length > 2" class="mini-tag-sm count">+{{ c.skills.length - 2 }}</span>
              </div>
            </div>
            
            <div class="col-stage">
              <span class="status-dot-pill" :style="{ '--pill-color': getStageColor(c.stage) }">
                <span class="status-dot"></span>
                {{ getStageName(c.stage) }}
              </span>
            </div>
            
            <div class="col-date">
              <span class="date-text-sm">{{ c.date }}</span>
            </div>
            
            <div class="col-actions">
              <div class="list-actions-cluster">
                <button class="list-action-btn note-btn" :class="{ active: c.notes }" @click.stop="handleOpenNote(c)" title="Notes">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                </button>
                <button v-if="c.score === null || c.score === undefined" class="list-action-btn analyze-btn" @click.stop="analyzeCandidate(c)" :class="{ 'is-loading': c.isAnalyzing }" title="Analyse IA">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 2v4m0 12v4M2 12h4m12 0h4M4.93 4.93l2.83 2.83m8.48 8.48l2.83 2.83M4.93 19.07l2.83-2.83m8.48-8.48l2.83-2.83"/></svg>
                </button>
                <button class="list-action-btn view-btn" @click.stop="openCandidate(c)" title="Voir profil">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Comparison Modal (Battle Mode) -->
      <ComparisonModal 
        :is-open="showComparisonModal" 
        :candidate-ids="selectedForComparison" 
        @close="showComparisonModal = false" 
      />
    </main>
  </div>
</template>

<script>
import Sidebar from '@/components/layout/Sidebar.vue'
import ComparisonModal from '@/components/recruiter/ComparisonModal.vue'
import { Sword, Check } from 'lucide-vue-next'
import PremiumSelect from '@/components/common/PremiumSelect.vue'
import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'
import { useRecruitmentStore } from '@/stores/recruitmentStore'
import api from '@/api/axios'
import { useToastStore } from '@/stores/toastStore'
import { useModalStore } from '@/stores/modalStore'

export default {
  name: 'Candidatures',
  components: { Sidebar, ComparisonModal, PremiumSelect, Sword, Check },
  data() {
    const authStore = useAuthStore()
    const recruitmentStore = useRecruitmentStore()
    const storedRole = (authStore.userRole || '').toLowerCase()
    const accentColor = authStore.themeColors.accent
    const accentDark = authStore.themeColors.accentDark
    const isCompanyOwner = authStore.isAdmin && !authStore.isSuperAdmin

    return {
      authStore,
      recruitmentStore,
      isLoading: true,
      sidebarCollapsed: false,
      isCompanyOwner,
      accentColor,
      accentDark,
      viewMode: 'kanban',
      selectedOfferId: '',
      offers: [],
      showAIModal: false,
      aiProgress: 0,
      // For Notes Modal
      showNoteModal: false,
      selectedApplication: null,
      currentNote: '',
      isSavingNote: false,
      // Drag and Drop state
      draggedCandidate: null,
      draggedFromColId: null,
      activeDropZone: null,
      // Battle Mode Selection
      compareMode: false,
      selectedForComparison: [],
      showComparisonModal: false,
      sendingBulk: false
    }
  },
  async mounted() {
    await this.fetchOffers()
    if (this.offers.length > 0) {
       const queryOfferId = this.$route.query.jobOfferId
       const exists = this.offers.some(o => o.id === queryOfferId)
       if (queryOfferId && exists) {
         this.selectedOfferId = queryOfferId
       } else {
         this.selectedOfferId = this.offers[0].id
       }
       await this.fetchApplications()
    } else {
       this.isLoading = false
    }
  },
  computed: {
    columns() {
      return [
        { id: 'submitted', title: this.$t('pipeline.stages.submitted'), color: '#2196F3', candidates: [] },
        { id: 'shortlisted', title: this.$t('pipeline.stages.shortlisted'), color: '#FF9800', candidates: [] },
        { id: 'interview', title: this.$t('pipeline.stages.interview'), color: '#9C27B0', candidates: [] },
        { id: 'interviewed', title: this.$t('pipeline.stages.interviewed'), color: '#673AB7', candidates: [] },
        { id: 'underreview', title: this.$t('pipeline.stages.underreview'), color: '#FFC107', candidates: [] },
        { id: 'accepted', title: this.$t('pipeline.stages.accepted'), color: '#8BC34A', candidates: [] },
        { id: 'rejected', title: this.$t('pipeline.stages.rejected'), color: '#F44336', candidates: [] },
      ]
    },
    totalCandidates() {
      return this.columns.reduce((sum, col) => sum + col.candidates.length, 0)
    },
    selectedOfferName() {
      const offer = this.offers.find(o => o.id === this.selectedOfferId)
      return offer ? offer.title : '...'
    },
    allCandidates() {
      return this.columns.flatMap(col => col.candidates)
    },
    canLaunchBattleComparison() {
      if (this.selectedForComparison.length !== 2) return false
      return this.selectedForComparison.every(id => {
        const c = this.allCandidates.find(x => x.id === id)
        return c && this.hasAiAnalysis(c)
      })
    },
    battleCompareButtonTitle() {
      if (this.selectedForComparison.length !== 2) {
        return 'Sélectionnez exactement 2 candidats'
      }
      const missing = this.selectedForComparison
        .map(id => this.allCandidates.find(x => x.id === id))
        .filter(c => c && !this.hasAiAnalysis(c))
      if (missing.length === 0) return 'Comparer les scores IA des 2 candidats'
      const names = missing.map(c => c.name).join(', ')
      return `Analyse IA requise pour : ${names}`
    },
    rankedCandidates() {
      // Sort by score descending — unscored candidates go to the end
      return [...this.allCandidates].sort((a, b) => {
        const sa = a.score ?? -1
        const sb = b.score ?? -1
        return sb - sa
      })
    },
    sidebarVars() {
      // Returned empty as Sidebar handles vars via its internal setup and global CSS
      return {}
    },
    offerOptions() {
      return this.offers.map(o => ({
        value: o.id,
        label: o.title
      }))
    }
  },
  methods: {
    scoreClass(s) {
      if (s === null) return 'pending'
      if (s >= 85) return 'elite'
      return s >= 75 ? 'high' : s >= 60 ? 'mid' : 'low'
    },
    getRankClass(rank) {
      if (rank === 1) return 'rank-gold'
      if (rank === 2) return 'rank-silver'
      if (rank === 3) return 'rank-bronze'
      return 'rank-default'
    },
    getStageColor(stage) {
      const map = { submitted: '#94a3b8', underreview: '#6366f1', shortlisted: this.accentColor, interview: '#10b981', rejected: '#ef4444' }
      return map[stage] || '#94a3b8'
    },
    getStageName(stage) {
      const map = {
        submitted: this.$t('pipeline.stages.submitted'),
        shortlisted: this.$t('pipeline.stages.shortlisted'),
        interview: this.$t('pipeline.stages.interview'),
        interviewed: this.$t('pipeline.stages.interviewed'),
        underreview: this.$t('pipeline.stages.underreview'),
        accepted: this.$t('pipeline.stages.accepted'),
        rejected: this.$t('pipeline.stages.rejected')
      }
      return map[stage] || stage
    },
    async fetchOffers() {
      await this.recruitmentStore.fetchOffers()
      this.offers = this.recruitmentStore.offers
    },
    async fetchApplications() {
      if (!this.selectedOfferId) return
      this.isLoading = true
      try {
        await this.recruitmentStore.fetchApplications(this.selectedOfferId)
        const data = this.recruitmentStore.applications

        // Clear columns
        this.columns.forEach(col => col.candidates = [])

        data.forEach(app => {
          const colId = (app.status !== undefined ? this.mapStatusToId(app.status) : app.stage) || 'submitted'
          const col = this.columns.find(c => c.id === colId) || this.columns[0]
          col.candidates.push({
            id: app.id,
            name: app.fullName,
            initials: (app.firstName[0] + (app.lastName[0] || '')).toUpperCase(),
            role: app.role || 'Candidat',
            skills: app.skills || [],
            score: app.score ?? app.aiScore ?? null,
            date: app.date || '...',
            stage: colId,
            aiSummary: app.aiSummary,
            notes: app.recruiterNotes || '',
            commentsCount: app.commentsCount || 0,
            avatarBg: this.getRandomColor(app.id),
            isAnalyzing: false
          })
        })
      } catch (err) {
        console.error('Erreur chargement candidatures:', err)
      } finally {
        this.isLoading = false
      }
    },
    mapStatusToId(status) {
      const map = { 
        0: 'submitted', 
        1: 'underreview', 
        2: 'shortlisted', 
        3: 'interview', 
        6: 'interviewed', 
        5: 'accepted',
        4: 'rejected' 
      }
      return map[status] || 'submitted'
    },
    getRandomColor(id) {
       const colors = ['#0ea5e9', '#6366f1', '#f59e0b', '#ec4899', '#10b981', '#8b5cf6']
       const seed = id.toString().split('').reduce((acc, char) => acc + char.charCodeAt(0), 0)
       return colors[seed % colors.length]
    },
    openCandidate(c) {
      this.$router.push(`/candidatures/profil/${c.id}`)
    },
    async moveCard(candidate, fromColId, toColId) {
      if (fromColId === toColId) return
      const fromCol = this.columns.find(c => c.id === fromColId)
      const toCol = this.columns.find(c => c.id === toColId)
      if (!fromCol || !toCol) return
      
      try {
        const backendStatus = this.mapIdToStatus(toColId)
        await api.patch(`/recruiter/applications/${candidate.id}/stage`, { stage: backendStatus.toString() })

        // Update Store Cache
        this.recruitmentStore.updateApplicationStage(candidate.id, backendStatus)

        const idx = fromCol.candidates.findIndex(c => c.id === candidate.id)
        if (idx !== -1) {
          const [moved] = fromCol.candidates.splice(idx, 1)
          moved.stage = toColId
          toCol.candidates.push(moved)
        }
      } catch (err) {
        console.error('Erreur move card:', err)
      }
    },
    // DRAG AND DROP HANDLERS
    handleDragStart(event, candidate, colId) {
      this.draggedCandidate = candidate
      this.draggedFromColId = colId
      event.dataTransfer.effectAllowed = 'move'
      // Add a transparent image or style if needed
      event.target.style.opacity = '0.5'
    },
    handleDragEnd(event) {
      event.target.style.opacity = '1'
      this.activeDropZone = null
    },
    handleDragOver(event, colId) {
      if (this.draggedFromColId !== colId) {
        this.activeDropZone = colId
      }
    },
    handleDragLeave(event, colId) {
      if (this.activeDropZone === colId) {
        this.activeDropZone = null
      }
    },
    handleDrop(event, colId) {
      this.activeDropZone = null
      if (this.draggedCandidate && this.draggedFromColId !== colId) {
        this.moveCard(this.draggedCandidate, this.draggedFromColId, colId)
      }
      this.draggedCandidate = null
      this.draggedFromColId = null
    },
    mapIdToStatus(id) {
      const map = { 
        submitted: 0, 
        underreview: 1, 
        shortlisted: 2, 
        interview: 3, 
        interviewed: 6, 
        accepted: 5, 
        rejected: 4 
      }
      return map[id] || 0
    },
    async analyzeCandidate(candidate) {
      if (candidate.isAnalyzing) return
      if (!candidate?.resumeUrl) {
        useToastStore().show("Impossible d'analyser : aucun CV disponible pour ce candidat.", 'error')
        return
      }
      candidate.isAnalyzing = true
      try {
        const res = await api.post(`/recruiter/applications/${candidate.id}/analyze`)
        candidate.score = res.data.score

        // Determine target column from backend-returned status (integer enum)
        const backendStatus = res.data.status
        const targetStage = this.mapStatusToId(backendStatus)
        const currentStage = candidate.stage

        if (targetStage && targetStage !== currentStage) {
          this.moveCard(candidate, currentStage, targetStage)
          if (targetStage === 'rejected') {
            useToastStore().show(
              `Candidat automatiquement rejeté — score IA ${candidate.score}% inférieur au seuil configuré.`,
              'error'
            )
          } else {
            useToastStore().show(
              `Analyse terminée — score : ${candidate.score}%. Candidat déplacé en « ${this.getStageName(targetStage)} ».`,
              'success'
            )
          }
        } else {
          useToastStore().show(`Analyse terminée — score : ${candidate.score}%.`, 'success')
        }
      } catch (err) {
        console.error('Erreur analyse:', err)
        useToastStore().show(`Erreur lors de l'analyse : ${err.response?.data?.message || err.message}`, 'error')
      } finally {
        candidate.isAnalyzing = false
      }
    },
    confirmMarkInterviewed(candidate) {
      this.moveCard(candidate, 'interview', 'interviewed')
    },
    handleOpenNote(candidate) {
      this.selectedApplication = candidate
      this.currentNote = candidate.notes || ''
      this.showNoteModal = true
    },
    async handleSaveNote() {
      if (!this.selectedApplication) return
      this.isSavingNote = true
      try {
        await api.patch(`/recruiter/applications/${this.selectedApplication.id}/notes`, { notes: this.currentNote })
        this.selectedApplication.notes = this.currentNote
        this.showNoteModal = false
      } catch (err) {
        console.error('Erreur sauvegarde note:', err)
      } finally {
        this.isSavingNote = false
      }
    },
    runAIAnalysis() {
      this.showAIModal = true
      this.aiProgress = 0
      const interval = setInterval(() => {
        this.aiProgress += 8
        if (this.aiProgress >= 100) {
          this.aiProgress = 100
          clearInterval(interval)
        }
      }, 200)
    },
    toggleCompareMode() {
      this.compareMode = !this.compareMode
      if (!this.compareMode) {
        this.selectedForComparison = []
      }
    },
    toggleCandidateSelection(candidate) {
      const maxCompareCandidates = 2
      const idx = this.selectedForComparison.indexOf(candidate.id)
      if (idx === -1) {
        if (this.selectedForComparison.length >= maxCompareCandidates) {
          return
        }
        this.selectedForComparison.push(candidate.id)
      } else {
        this.selectedForComparison.splice(idx, 1)
      }
    },
    isSelected(id) {
      return this.selectedForComparison.includes(id)
    },
    isCompareSelectionDisabled(id) {
      return (
        this.compareMode &&
        this.selectedForComparison.length >= 2 &&
        !this.isSelected(id)
      )
    },
    hasAiAnalysis(candidate) {
      return candidate.score !== null && candidate.score !== undefined
    },
    launchBattleComparison() {
      if (this.selectedForComparison.length !== 2) {
        useToastStore().show('Sélectionnez exactement 2 candidats à comparer.', 'warning')
        return
      }
      const missing = this.selectedForComparison
        .map(id => this.allCandidates.find(x => x.id === id))
        .filter(c => c && !this.hasAiAnalysis(c))
      if (missing.length > 0) {
        const names = missing.map(c => c.name).join(' et ')
        useToastStore().show(
          `Analyse IA requise avant la comparaison. Lancez l'analyse CV pour : ${names}.`,
          'warning'
        )
        return
      }
      this.showComparisonModal = true
    },
    async sendQuizToAllStage(stageId) {
      const col = this.columns.find(c => c.id === stageId);
      if (!col || col.candidates.length === 0) return;

      const modalStore = useModalStore();
      const confirmed = await modalStore.confirm({
        title: 'Envoyer le Quiz IA ?',
        message: `Envoyer le quiz IA aux ${col.candidates.length} candidat(s) de l’étape « ${col.title} » ?`,
        confirmText: 'Envoyer',
        cancelText: 'Annuler',
        type: 'warning'
      });
      if (!confirmed) return;

      this.sendingBulk = true;
      try {
        const ids = col.candidates.map(c => c.id);
        const res = await api.post('/recruiter/applications/bulk-send-quiz', ids);
        useToastStore().show(res.data.message || 'Quiz envoyé avec succès !', 'success');
      } catch (err) {
        console.error('Erreur envoi groupé:', err);
        const msg = err.response?.data?.message || "Erreur lors de l'envoi groupé.";
        useToastStore().show(msg, 'error');
      } finally {
        this.sendingBulk = false;
      }
    }
  }
}
</script>

<style>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";
</style>

<style scoped>
.dashboard-layout { background: transparent !important; }

/* COMPONENT-SPECIFIC STYLES */
.filter-card {
  position: relative;
  z-index: 10;
  flex-direction: row;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  margin-bottom: 24px;
}

.action-left {
  display: flex;
  gap: 16px;
}

.action-right-group {
  display: flex;
  align-items: center;
  gap: 16px;
}

.view-switch {
  display: flex;
  background: var(--r-main-bg);
  padding: 4px;
  border-radius: 14px;
  border: 1px solid var(--r-border);
}

.view-switch button {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  border: none;
  background: transparent;
  color: var(--r-text-sub);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.3s;
}

.view-switch button.active {
  background: var(--accent);
  color: var(--accent-contrast);
  box-shadow: 0 4px 12px var(--accent-soft);
}

.premium-kanban {
  display: flex;
  gap: 24px;
  padding-bottom: 24px;
  overflow-x: auto;
  min-height: 550px;
  width: 100%;
}

.kanban-column {
  min-width: 310px;
  max-width: 310px;
  background: rgba(0, 0, 0, 0.02);
  border-radius: 24px;
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  border: none;
  transition: all 0.3s ease;
}

.kanban-content {
  flex: 1;
  overflow-y: auto;
  padding-right: 4px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-top: 12px;
}

.kanban-content::-webkit-scrollbar { width: 4px; }
.kanban-content::-webkit-scrollbar-track { background: transparent; }
.kanban-content::-webkit-scrollbar-thumb { background: var(--r-border); border-radius: 10px; }

.candidate-card {
  background: #ffffff;
  border-radius: 20px;
  padding: 18px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.03);
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
  cursor: pointer;
  position: relative;
}

.candidate-card:hover {
  transform: translateY(-8px) scale(1.02);
  box-shadow: 0 20px 40px -15px rgba(0, 0, 0, 0.4);
}

.battle-mode-btn {
  display: flex;
  align-items: center;
  gap: 10px;
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  padding: 10px 20px;
  border-radius: 14px;
  font-weight: 800;
  color: var(--r-text-main);
  cursor: pointer;
  transition: all 0.3s;
  position: relative;
}

.battle-mode-btn.active {
  background: var(--accent-soft);
  border-color: var(--accent);
  color: var(--accent);
}

.selection-counter {
  position: absolute;
  top: -8px;
  right: -8px;
  background: var(--accent);
  color: white;
  width: 22px;
  height: 22px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 11px;
  font-weight: 900;
}

.card-selection-check {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.check-box {
  width: 24px;
  height: 24px;
  border-radius: 8px;
  border: 2px solid var(--r-border);
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.2s;
}

.check-box.checked {
  background: var(--accent);
  border-color: var(--accent);
  color: white;
}

.launch-battle-btn {
  animation: slideInRight 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
}

@keyframes slideInRight {
  from { opacity: 0; transform: translateX(20px); }
  to { opacity: 1; transform: translateX(0); }
}

.kanban-header-premium {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.status-indicator { width: 4px; height: 18px; border-radius: 4px; }

.btn-bulk-send {
  background: var(--accent-soft);
  border: 1px solid var(--accent-soft);
  color: var(--accent);
  width: 28px;
  height: 28px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s;
  font-size: 12px;
}

.btn-bulk-send:hover:not(:disabled) {
  background: var(--accent);
  color: white;
  transform: scale(1.1);
}

.btn-bulk-send:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}
.column-count { font-size: 12px; font-weight: 700; color: var(--r-text-sub); background: var(--r-main-bg); padding: 2px 10px; border-radius: 100px; }

.premium-avatar { width: 40px; height: 40px; border-radius: 12px; display: flex; align-items: center; justify-content: center; font-weight: 800; color: white; }

.ia-chip-premium {
  padding: 6px 12px;
  border-radius: 12px;
  font-size: 13px;
  font-weight: 800;
  display: flex;
  align-items: center;
  gap: 6px;
  margin-left: auto;
}
.ia-chip-premium.elite { background: linear-gradient(135deg, #10b981, #059669); color: #fff; }

.card-skills { display: flex; gap: 4px; margin: 12px 0; flex-wrap: wrap; }
.mini-tag { background: var(--r-main-bg); border: 1px solid var(--r-border); padding: 2px 8px; border-radius: 6px; font-size: 10px; font-weight: 700; }

.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.premium-modal { background: var(--r-card-bg); border: 1px solid var(--r-border); border-radius: 24px; padding: 32px; width: 400px; box-shadow: 0 20px 40px rgba(0,0,0,0.3); }

@media (max-width: 1024px) {
  .filter-card { flex-direction: column; align-items: stretch; gap: 16px; }
  .action-left, .action-right-group { justify-content: space-between; }
}
</style>
