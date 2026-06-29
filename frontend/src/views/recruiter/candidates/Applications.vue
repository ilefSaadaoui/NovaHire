<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="pipeline" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      
      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome">
          <div class="header-with-icon">
            <div class="icon-box-themed" style="width: 56px; height: 56px; border-radius: 18px;">
              <Users :size="28" stroke-width="2.5" />
            </div>
            <div>
              <h1 class="premium-title-themed" style="font-size: 28px;">{{ $t('pipeline.title') }}</h1>
              <p class="welcome-sub">{{ $t('pipeline.subtitle', { count: totalCandidates, offer: selectedOfferName }) }}</p>
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
            style="min-width: 350px;"
            :placeholder="$t('pipeline.selectOffer') || 'Choisir une offre...'"
          />
        </div>
        
        <div class="action-right-group">
          <!-- BATTLE MODE GROUP -->
          <div class="battle-mode-group anim-reveal-right">
            <div class="battle-switch-container" @click="toggleCompareMode" :class="{ active: compareMode }">
              <div class="battle-switch-track">
                <div class="battle-switch-thumb">
                  <Zap v-if="compareMode" :size="14" stroke-width="3" />
                  <Sword v-else :size="14" stroke-width="3" />
                </div>
              </div>
              <span class="battle-label">BATTLE MODE</span>
            </div>

            <transition name="fade-slide">
              <button 
                v-if="compareMode && selectedForComparison.length > 0" 
                class="launch-battle-btn-elite" 
                :disabled="!canLaunchBattleComparison"
                :title="battleCompareButtonTitle"
                @click="launchBattleComparison"
              >
                <div class="btn-content">
                  <span class="btn-text">COMPARER 2 CANDIDATS</span>
                  <div class="btn-badge">{{ selectedForComparison.length }}/2</div>
                </div>
                <div class="btn-glow"></div>
              </button>
            </transition>
          </div>

          <div class="view-switch">
            <button :class="{ active: viewMode === 'kanban' }" @click="viewMode = 'kanban'" :title="$t('pipeline.view.kanban')">
              <Columns :size="18" />
            </button>
            <button :class="{ active: viewMode === 'list' }" @click="viewMode = 'list'" :title="$t('pipeline.view.list')">
              <List :size="18" />
            </button>
          </div>

          <button v-if="selectedOfferId" class="btn-quiz-toolbar anim-reveal-left" @click="showQuizModal = true">
            <div class="icon-pulse-gold">
              <FlaskConical :size="16" stroke-width="2.5" />
            </div>
            <span>Gérer Quiz IA</span>
          </button>
        </div>
      </div>

      <div class="page-body">
        <div v-if="isLoading" class="loading-state">
          <div class="spinner"></div>
          <p>{{ $t('pipeline.loading') }}</p>
        </div>

        <div v-else-if="allCandidates.length === 0" class="empty-state-luxury anim-reveal-up">
          <div class="empty-icon-box">
            <Search v-if="!selectedOfferId" :size="40" stroke-width="1.5" />
            <Users v-else :size="40" stroke-width="1.5" />
          </div>
          <h3 class="empty-title">{{ !selectedOfferId ? ($t('pipeline.selectOfferPrompt') || 'Sélectionnez une mission') : $t('pipeline.empty') }}</h3>
          <p class="empty-text">{{ !selectedOfferId ? ($t('pipeline.selectOfferSub') || 'Veuillez choisir une offre pour visualiser les candidatures.') : $t('pipeline.emptySub') }}</p>
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
              <span class="column-count">{{ col.candidates.length }}</span>
            </div>
            
            <div class="kanban-content">
              <div v-for="c in col.candidates" 
                :key="c.id" 
                class="r-card candidate-card anim-reveal-up" 
                :class="{ 'selected-for-battle': isSelected(c.id) }"
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
                    <Sparkles :size="12" class="ia-spark" />
                    <span>{{ c.score }}%</span>
                  </div>
                  <button v-else class="ia-analyze-btn-outer" @click.stop="analyzeCandidate(c)" :class="{ 'is-analyzing': c.isAnalyzing }">
                    <div class="ia-spark-container">
                      <Zap :size="16" />
                    </div>
                    <span class="btn-text">IA</span>
                  </button>
                  
                  <div v-if="c.quizSent && (c.quizScore === null || c.quizScore === undefined)" class="quiz-badge-mini pending" title="Quiz en attente de réponse">
                    <Clock :size="12" class="anim-spin-slow" />
                    <span>En attente</span>
                  </div>
                  
                  <div v-if="c.quizScore !== null && c.quizScore !== undefined" class="quiz-badge-mini" title="Score Quiz IA">
                    <FlaskConical :size="12" stroke-width="2.5" />
                    <span>{{ c.quizScore }}%</span>
                  </div>
                </div>

                <div class="card-footer">
                  <div class="date-tag">
                    <Clock :size="11" />
                    {{ c.date }}
                  </div>
                  <div class="quick-actions" style="display: flex; gap: 8px; align-items: center;">
                    <!-- Universal Next Action -->
                    <button v-if="getNextStage(col.id)" 
                      class="action-btn next-step-btn" 
                      @click.stop="moveCard(c, col.id, getNextStage(col.id))" 
                      :title="`Passer à : ${getStageName(getNextStage(col.id))}`"
                    >
                      <ChevronRight :size="16" stroke-width="3" />
                    </button>

                    <button v-if="col.id !== 'rejected' && col.id !== 'accepted'" 
                      class="action-btn small danger" 
                      @click.stop="moveCard(c, col.id, 'rejected')" 
                      title="Refuser"
                    >
                      <X :size="14" stroke-width="3" />
                    </button>
                    
                    <button 
                      v-if="col.id !== 'rejected' && col.id !== 'accepted' && col.id !== 'offersent'"
                      class="action-btn small info" 
                      @click.stop="openQuizForCandidate(c)" 
                      title="Envoyer Quiz IA"
                    >
                      <FlaskConical :size="14" stroke-width="2.5" />
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
            <div v-if="compareMode" class="col-selection"></div>
            <div class="col-cand">{{ $t('pipeline.table.candidate') }}</div>
            <div class="col-score">{{ $t('pipeline.table.score') }}</div>
            <div class="col-quiz">Quiz IA</div>
            <div class="col-stage">{{ $t('pipeline.table.stage') }}</div>
            <div class="col-date">{{ $t('pipeline.table.date') }}</div>
            <div class="col-actions">{{ $t('pipeline.table.action') }}</div>
          </div>
          
          <div v-for="c in allCandidates" :key="c.id" class="list-item-premium" :class="{ 'selected-for-battle': isSelected(c.id) }" @click="openCandidate(c)">
            <div
              v-if="compareMode"
              class="col-selection"
              :class="{ 'selection-disabled': isCompareSelectionDisabled(c.id) }"
              @click.stop="toggleCandidateSelection(c)"
            >
              <div class="check-box" :class="{ checked: isSelected(c.id), disabled: isCompareSelectionDisabled(c.id) }">
                <Check v-if="isSelected(c.id)" :size="14" />
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
                <Sparkles :size="12" class="ia-spark" />
                <span>{{ c.score }}%</span>
              </div>
              <span v-else class="score-pending-sm">—</span>
            </div>

            <div class="col-quiz">
              <div v-if="c.quizScore !== null && c.quizScore !== undefined" class="quiz-badge-mini list-v">
                <FlaskConical :size="12" stroke-width="2.5" />
                <span>{{ c.quizScore }}%</span>
              </div>
              <span v-else class="score-pending-sm">—</span>
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
                <button v-if="c.resumeUrl" class="list-action-btn download-btn" @click.stop="downloadCV(c)" title="Télécharger CV">
                  <Download :size="16" stroke-width="2.5" />
                </button>
                <button v-if="c.stage !== 'rejected' && c.stage !== 'accepted'" class="list-action-btn calendar-btn" @click.stop="openInterviewModal(c)" title="Planifier Entretien">
                  <Calendar :size="16" stroke-width="2.5" />
                </button>
                <button v-if="c.score === null || c.score === undefined" class="list-action-btn analyze-btn" @click.stop="analyzeCandidate(c)" :class="{ 'is-loading': c.isAnalyzing }" title="Analyse IA">
                  <Zap :size="16" stroke-width="2.5" />
                </button>
                <button v-if="c.stage !== 'rejected' && c.stage !== 'accepted'" class="list-action-btn reject-btn danger" @click.stop="moveCard(c, c.stage, 'rejected')" title="Refuser">
                  <X :size="16" stroke-width="2.5" />
                </button>
                <button class="list-action-btn view-btn" @click.stop="openCandidate(c)" title="Voir profil">
                  <ChevronRight :size="16" stroke-width="2.5" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- AI Analysis Modal -->
      <div v-if="showAIModal" class="modal-overlay" @click="showAIModal = false">
        <div class="premium-modal" @click.stop>
          <div class="ai-burst">
            <Zap :size="32" />
          </div>
          <h3>Analyses en masse</h3>
          <p>L'IA analyse simultanément tous les nouveaux profils...</p>
          
          <div class="ai-progress-container">
            <div class="progress-track"><div class="progress-bar" :style="{ width: aiProgress + '%' }"></div></div>
            <div class="progress-stats"><span>{{ aiProgress }}%</span> <span>{{ totalCandidates }} {{ $t('pipeline.modal.profiles') }}</span></div>
          </div>
          
          <button v-if="aiProgress >= 100" class="btn-luxury primary" @click="showAIModal = false">{{ $t('pipeline.modal.results') }}</button>
        </div>
      </div>

      <!-- Note Modal -->
      <div v-if="showNoteModal" class="modal-overlay" @click="showNoteModal = false">
        <div class="modal-content premium-modal" style="max-width: 450px;" @click.stop>
          <div class="modal-header">
            <h3>Note de filtrage</h3>
            <button class="close-btn" @click="showNoteModal = false">
              <X :size="24" stroke-width="2.5" />
            </button>
          </div>
          <div class="modal-body">
            <p style="font-size: 13px; color: var(--r-text-sub); margin-bottom: 16px;">Ajoutez une note rapide pour justifier votre décision ou pour plus tard.</p>
            <div class="form-group">
              <textarea v-model="currentNote" rows="5" placeholder="Votre note ici..." style="width: 100%; border-radius: 12px; padding: 12px; background: var(--r-main-bg); color: var(--r-text-main); border: 1px solid var(--r-border); font-family: inherit; font-size: 14px; outline: none; transition: 0.3s;"></textarea>
            </div>
          </div>
          <div class="modal-footer" style="padding-top: 16px; display: flex; justify-content: flex-end; gap: 12px;">
            <button class="btn-premium btn-secondary" @click="showNoteModal = false">Annuler</button>
            <button class="btn-luxury primary" @click="handleSaveNote" :disabled="isSavingNote">
              {{ isSavingNote ? 'Enregistrement...' : 'Enregistrer' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Comparison Modal (Battle Mode) -->
      <ComparisonModal 
        :is-open="showComparisonModal" 
        :candidate-ids="selectedForComparison" 
        @close="showComparisonModal = false" 
      />

      <InterviewModal 
        v-if="showInterviewModal"
        :form="interviewForm"
        @update:form="val => interviewForm = val"
        :loading="sendingInterview"
        @close="showInterviewModal = false"
        @save="confirmScheduleInterview"
        @generate-template="generateEmailTemplate"
      />

      <QuizManagementModal
        v-if="showQuizModal"
        :show="showQuizModal"
        :jobOffer="selectedOffer"
        :applicationId="selectedApplicationId"
        @close="showQuizModal = false; selectedApplicationId = null"
      />

      <!-- Rejection Email Modal -->
      <Teleport to="body">
        <Transition name="modal-fade">
          <div v-if="showRejectionModal" class="rejection-overlay" @click="cancelRejection">
            <div class="rejection-modal" @click.stop>
              <!-- Header -->
              <div class="rej-header">
                <div class="rej-header-left">
                  <div class="rej-icon">
                    <X :size="22" stroke-width="3" />
                  </div>
                  <div class="rej-header-text">
                    <h3 class="rej-title">Refuser la candidature</h3>
                    <p class="rej-subtitle">Email de refus pour <strong>{{ selectedCandidateName }}</strong></p>
                  </div>
                </div>
                <button class="rej-close" @click="cancelRejection">
                  <X :size="18" stroke-width="2.5" />
                </button>
              </div>

              <!-- Body -->
              <div class="rej-body">
                <label class="rej-label">Motif du refus <span class="rej-label-opt">(optionnel)</span></label>
                <textarea
                  v-model="rejectionReason"
                  class="rej-textarea"
                  rows="5"
                  placeholder="Ex: Profil trop junior, manque d'expérience sur React..."
                ></textarea>
                <p class="rej-hint">Ce message sera inclus dans l'email envoyé au candidat.</p>
              </div>

              <!-- Footer -->
              <div class="rej-footer">
                <button class="rej-btn-cancel" @click="cancelRejection">Annuler</button>
                <button class="rej-btn-confirm" @click="confirmRejection" :disabled="isSendingRejection">
                  <X v-if="!isSendingRejection" :size="15" stroke-width="3" />
                  <span>{{ isSendingRejection ? 'Envoi en cours...' : 'Confirmer et envoyer' }}</span>
                </button>
              </div>
            </div>
          </div>
        </Transition>
      </Teleport>
    </main>
  </div>
</template>

<script>
import Sidebar from '@/components/layout/Sidebar.vue'
import { 
  Users, Columns, List, Sparkles, 
  FileEdit, Clock, ChevronRight, Check, 
  Trash2, Search, X, Zap, Download, Calendar, RefreshCw, FlaskConical, Sword
} from 'lucide-vue-next'
import InterviewModal from '@/components/recruiter/modals/InterviewModal.vue'
import QuizManagementModal from '@/components/recruiter/quiz/QuizManagementModal.vue'

import PremiumSelect from '@/components/common/PremiumSelect.vue'
import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'
import { useRecruitmentStore } from '@/stores/recruitmentStore'
import { useToastStore } from '@/stores/toastStore'
import api from '@/api/axios'
import ComparisonModal from '@/components/recruiter/ComparisonModal.vue'


export default {
  name: 'Applications',
  components: { 
    Sidebar, PremiumSelect,
    Users, Columns, List, Sparkles, 
    FileEdit, Clock, ChevronRight, Check, 
    Trash2, Search, X, Zap, Download, Calendar, Sword, RefreshCw, FlaskConical,
    InterviewModal, ComparisonModal, QuizManagementModal
  },
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
      selectedOffer: null,
      offers: [],
      showQuizModal: false,
      showAIModal: false,
      aiProgress: 0,
      showInterviewModal: false,
      selectedApplicationId: null,
      selectedCandidateName: '',
      interviewForm: { date: '', time: '', type: 'visio', subject: '', message: '' },
      sendingInterview: false,
      columns: [
        { id: 'submitted', title: 'Identification', color: '#fbbf24', candidates: [] },
        { id: 'underreview', title: 'Évaluation', color: '#f59e0b', candidates: [] },
        { id: 'shortlisted', title: 'Présélection', color: '#6366f1', candidates: [] },
        { id: 'interview', title: 'Entretien prévu', color: '#818cf8', candidates: [] },
        { id: 'interviewed', title: 'Entretien fait', color: '#a78bfa', candidates: [] },
        { id: 'offersent', title: 'Offre envoyée', color: '#3b82f6', candidates: [] },
        { id: 'accepted', title: 'Engagé', color: '#10b981', candidates: [] },
        { id: 'rejected', title: 'Non retenu', color: '#f43f5e', candidates: [] },
      ],
      // For Notes Modal
      showNoteModal: false,
      selectedApplication: null,
      currentNote: '',
      isSavingNote: false,
      // Drag and Drop state
      draggedCandidate: null,
      draggedFromColId: null,
      activeDropZone: null,
      // Battle Mode Selection (exactement 2 candidats)
      compareMode: false,
      maxCompareCandidates: 2,
      selectedForComparison: [],
      showComparisonModal: false,
      // Rejection state
      showRejectionModal: false,
      rejectionReason: '',
      isSendingRejection: false,
      pendingRejection: null,
      useAI: false
    }
  },
  async mounted() {
    await this.fetchOffers()
    this.isLoading = false
  },
  computed: {
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
      if (this.selectedForComparison.length !== this.maxCompareCandidates) return false
      return this.selectedForComparison.every(id => {
        const c = this.allCandidates.find(x => x.id === id)
        return c && this.hasAiAnalysis(c)
      })
    },
    battleCompareButtonTitle() {
      if (this.selectedForComparison.length !== this.maxCompareCandidates) {
        return 'Sélectionnez exactement 2 candidats'
      }
      const missing = this.selectedForComparison
        .map(id => this.allCandidates.find(x => x.id === id))
        .filter(c => c && !this.hasAiAnalysis(c))
      if (missing.length === 0) return 'Comparer les scores IA des 2 candidats'
      const names = missing.map(c => c.name).join(', ')
      return `Analyse IA requise pour : ${names}`
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
      return s >= 80 ? 'high' : s >= 60 ? 'mid' : 'low'
    },
    getStageColor(stage) {
      const map = { submitted: '#94a3b8', underreview: '#6366f1', shortlisted: this.accentColor, interview: '#10b981', offersent: '#3b82f6', rejected: '#ef4444' }
      return map[stage] || '#94a3b8'
    },
    getStageName(stage) {
      const map = {
        submitted: 'Identification',
        underreview: 'Évaluation',
        shortlisted: 'Présélection',
        interview: 'Entretien prévu',
        interviewed: 'Entretien fait',
        offersent: 'Offre envoyée',
        accepted: 'Engagé',
        rejected: 'Non retenu'
      }
      return map[stage] || stage
    },
    getNextStage(currentStage) {
      const stages = ['submitted', 'underreview', 'shortlisted', 'interview', 'interviewed', 'offersent', 'accepted']
      const idx = stages.indexOf(currentStage)
      if (idx !== -1 && idx < stages.length - 1) return stages[idx + 1]
      return null
    },
    async fetchOffers() {
      await this.recruitmentStore.fetchOffers()
      this.offers = this.recruitmentStore.offers
    },
    async fetchApplications() {
      if (!this.selectedOfferId) {
        this.selectedOffer = null;
        return;
      }
      this.selectedOffer = this.offers.find(o => o.id === this.selectedOfferId);
      this.isLoading = true;
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
            quizScore: app.quizScore,
            quizSent: app.quizSent,
            quizExpiresAt: app.quizExpiresAt,
            date: app.date || '...',
            stage: colId,
            resumeUrl: app.resumeUrl,
            aiSummary: app.aiSummary,
            notes: app.recruiterNotes || '',
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
        4: 'rejected', 
        5: 'accepted',
        7: 'offersent'
      }
      return map[status] || 'submitted'
    },
    getRandomColor(id) {
       const colors = ['#0ea5e9', '#6366f1', '#f59e0b', '#ec4899', '#10b981', '#8b5cf6']
       const seed = id.toString().split('').reduce((acc, char) => acc + char.charCodeAt(0), 0)
       return colors[seed % colors.length]
    },
    openCandidate(c) {
      if (this.compareMode || this.showComparisonModal) return
      this.$router.push(`/applications/profile/${c.id}`)
    },
    async moveCard(candidate, fromColId, toColId, skipRejectionModal = false) {
      if (fromColId === toColId) return

      // Intercept rejection (unless bypassed, e.g. for AI auto-reject)
      if (toColId === 'rejected' && !skipRejectionModal && !this.showRejectionModal) {
        this.selectedCandidateName = candidate.name
        this.pendingRejection = { candidate, fromColId, toColId }
        this.showRejectionModal = true
        this.rejectionReason = ''
        return
      }

      const fromCol = this.columns.find(c => c.id === fromColId)
      const toCol = this.columns.find(c => c.id === toColId)
      if (!fromCol || !toCol) return
      
      try {
        const backendStatus = this.mapIdToStatus(toColId)
        await api.patch(`/recruiter/applications/${candidate.id}/stage`, { stage: backendStatus.toString() })

        // 2.2 Notification "Mise à l'étude" (Shortlisted)
        if (toColId === 'shortlisted') {
          api.post(`/recruiter/applications/${candidate.id}/send-shortlisted-email`).catch(e => console.error(e))
          useToastStore().show("Email de mise à l'étude envoyé au candidat.", "success")
        }

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
    cancelRejection() {
      this.showRejectionModal = false
      this.pendingRejection = null
    },
    async confirmRejection() {
      if (!this.pendingRejection) return
      const { candidate, fromColId, toColId } = this.pendingRejection
      
      this.isSendingRejection = true
      try {
        // 1. Update status in backend
        const backendStatus = this.mapIdToStatus(toColId)
        await api.patch(`/recruiter/applications/${candidate.id}/stage`, { stage: backendStatus.toString() })
        
        // 2. Send rejection email
        await api.post(`/recruiter/applications/${candidate.id}/send-rejection-email`, {
          reason: this.rejectionReason,
          useAI: this.useAI
        })

        // 3. Update UI
        const fromCol = this.columns.find(c => c.id === fromColId)
        const toCol = this.columns.find(c => c.id === toColId)
        const idx = fromCol.candidates.findIndex(c => c.id === candidate.id)
        if (idx !== -1) {
          const [moved] = fromCol.candidates.splice(idx, 1)
          moved.stage = toColId
          toCol.candidates.push(moved)
        }
        
        this.recruitmentStore.updateApplicationStage(candidate.id, backendStatus)
        useToastStore().show("Candidat refusé et email envoyé.", "info")
        this.showRejectionModal = false
        this.pendingRejection = null
      } catch (err) {
        console.error('Erreur rejet:', err)
        useToastStore().show("Erreur lors du rejet du candidat.", "error")
      } finally {
        this.isSendingRejection = false
      }
    },
    mapIdToStatus(id) {
      const map = { 
        submitted: 0, 
        underreview: 1, 
        shortlisted: 2, 
        interview: 3, 
        interviewed: 6, 
        rejected: 4, 
        accepted: 5,
        offersent: 7
      }
      return map[id] || 0
    },
    async analyzeCandidate(candidate) {
      if (candidate.isAnalyzing) return
      if (!candidate?.resumeUrl) {
        const toast = useToastStore()
        toast.error("Impossible d'analyser: aucun CV n'est disponible pour ce candidat.")
        return
      }
      candidate.isAnalyzing = true
      try {
        const res = await api.post(`/recruiter/applications/${candidate.id}/analyze`, {}, { timeout: 600000 })
        candidate.score = res.data.score

        // Determine target column from backend-returned status
        const backendStatus = res.data.status  // integer enum value
        const targetStage = this.mapStatusToId(backendStatus)
        const toast = useToastStore()

        if (targetStage && targetStage !== candidate.stage) {
          if (targetStage === 'rejected') {
            // Auto-rejection: bypass the manual rejection modal (status already saved by backend)
            await this.moveCard(candidate, candidate.stage, 'rejected', true)
            toast.error(`Candidat automatiquement rejeté — score IA ${candidate.score}% inférieur au seuil configuré.`)
          } else {
            await this.moveCard(candidate, candidate.stage, targetStage)
            toast.success(`Analyse terminée — score : ${candidate.score}%. Candidat déplacé en « ${this.getStageName(targetStage)} ».`)
          }
        } else {
          toast.success(`Analyse terminée — score : ${candidate.score}%.`)
        }
      } catch (err) {
        console.error('Erreur analyse:', err)
        const toast = useToastStore()
        toast.error(`Erreur lors de l'analyse : ${err.response?.data?.message || err.message}`)
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
    downloadCV(candidate) {
      if (!candidate.resumeUrl) return
      const url = candidate.resumeUrl.startsWith('http') 
        ? candidate.resumeUrl 
        : `${import.meta.env.VITE_API_URL || 'http://localhost:5147'}/${candidate.resumeUrl.replace(/^\//, '')}`
      window.open(url, '_blank')
    },
    openInterviewModal(candidate) {
      this.selectedApplicationId = candidate.id
      this.selectedCandidateName = candidate.name
      this.interviewForm = { date: '', time: '', type: 'visio', subject: '', message: '' }
      this.showInterviewModal = true
      setTimeout(() => this.generateEmailTemplate(), 0)
    },
    parseDateInput(value) {
      if (!value) return null
      const normalized = String(value).trim()
      const match = normalized.match(/^(\d{1,2})[\/\-.](\d{1,2})[\/\-.](\d{2,4})$/)
      if (match) {
        const day = Number(match[1])
        const month = Number(match[2])
        let year = Number(match[3])
        if (year < 100) year += 2000
        const date = new Date(year, month - 1, day)
        if (date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day) {
          return date
        }
      }
      const parsed = new Date(normalized)
      return isNaN(parsed.getTime()) ? null : parsed
    },
    formatLocalizedDate(value) {
      const date = this.parseDateInput(value)
      return date ? date.toLocaleDateString('fr-FR', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) : value
    },
    generateEmailTemplate() {
      const name = this.selectedCandidateName || 'Candidat'
      const job = this.selectedOfferName || 'le poste'
      const typeLabels = { visio: 'en visioconférence', phone: 'par téléphone', onsite: 'dans nos locaux' }
      const typeLabel = typeLabels[this.interviewForm.type] || 'en visioconférence'

      this.interviewForm.subject = `Invitation à un entretien pour le poste : ${job}`

      // Format date/time if available
      let dateTimeInfo = ''
      if (this.interviewForm.date || this.interviewForm.time) {
        const datePart = this.interviewForm.date
          ? this.formatLocalizedDate(this.interviewForm.date)
          : '[date à confirmer]'
        const timePart = this.interviewForm.time || '[heure à confirmer]'
        dateTimeInfo = `\n\nDate : ${datePart}\nHeure : ${timePart}`
      }

      let details = ''
      if (this.interviewForm.type === 'visio') {
        details = '\n\nVoici le lien pour rejoindre la réunion :\n[Insérez votre lien Google Meet / Zoom / Teams ici]'
      } else if (this.interviewForm.type === 'phone') {
        details = '\n\nNous vous contacterons au numéro que vous nous avez communiqué. Merci de vous assurer d’être disponible à l’heure indiquée.'
      } else {
        details = '\n\nAdresse :\n[Insérez l’adresse complète de vos bureaux ici]\n\nMerci de vous présenter à l’accueil 10 minutes avant l’heure prévue.'
      }

      this.interviewForm.message = `Bonjour ${name},\n\nSuite à l’examen de votre candidature pour le poste de ${job}, nous avons le plaisir de vous informer que votre profil a retenu toute notre attention.\n\nNous souhaiterions vous rencontrer ${typeLabel} afin d’échanger sur votre parcours et vos motivations.${dateTimeInfo}${details}\n\nMerci de confirmer votre disponibilité en répondant à cet e-mail.\n\nCordialement`
    },
    async confirmScheduleInterview() {
      this.sendingInterview = true
      try {
        const payload = {
          date: this.interviewForm.date,
          time: this.interviewForm.time,
          type: this.interviewForm.type,
          subject: this.interviewForm.subject,
          message: this.interviewForm.message
        }
        console.debug('Interview request payload:', payload)
        await api.post(`/recruiter/applications/${this.selectedApplicationId}/interviews`, payload)
        this.showInterviewModal = false
        const candidate = this.allCandidates.find(c => c.id === this.selectedApplicationId)
        if (candidate) {
          this.moveCard(candidate, candidate.stage, 'interview')
        }
        const toast = useToastStore()
        toast.success('Invitation d\'entretien envoyée avec succès !')
      } catch (err) {
        const response = err?.response
        const data = response?.data
        const message = data?.message || (typeof data === 'string' ? data : JSON.stringify(data)) || err?.message || 'Erreur lors de l\'envoi de l\'invitation.'
        console.error('Erreur planification entretien:', { status: response?.status, data, message, error: err })
        const toast = useToastStore()
        toast.error(message)
      } finally {
        this.sendingInterview = false
      }
    },
    openQuizForCandidate(candidate) {
      this.selectedApplicationId = candidate.id;
      this.showQuizModal = true;
    },
    runAIAnalysis() {
      // Keep existing batch analysis logic but could be updated later
      this.showAIModal = true
      this.aiProgress = 0
      const interval = setInterval(() => {
        this.aiProgress += 8
        if (this.aiProgress >= 100) {
          this.aiProgress = 100
          clearInterval(interval)
          // Also set to state for persistence later?
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
      const idx = this.selectedForComparison.indexOf(candidate.id)
      if (idx === -1) {
        if (this.selectedForComparison.length >= this.maxCompareCandidates) {
          useToastStore().show('Vous ne pouvez comparer que 2 candidats. Désélectionnez-en un pour en choisir un autre.', 'warning')
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
        this.selectedForComparison.length >= this.maxCompareCandidates &&
        !this.isSelected(id)
      )
    },
    hasAiAnalysis(candidate) {
      return candidate.score !== null && candidate.score !== undefined
    },
    launchBattleComparison() {
      if (this.selectedForComparison.length !== this.maxCompareCandidates) {
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
    }
  },
  watch: {
    '$route.query.jobOfferId': {
      immediate: true,
      async handler(newVal) {
        if (newVal) {
          this.selectedOfferId = newVal
          // Ensure offers are loaded to populate the dropdown correctly
          if (!this.offers || this.offers.length === 0) {
            await this.fetchOffers()
          }
          await this.fetchApplications()
        }
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
  display: flex !important;
  flex-direction: row !important;
  justify-content: space-between !important;
  align-items: center !important;
  padding: 16px 24px !important;
  margin-bottom: 24px;
  overflow: visible;
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
  background: var(--r-main-bg, #f4f6f8);
  padding: 4px;
  border-radius: 12px;
  border: 1px solid var(--r-border);
  gap: 4px;
}

.view-switch button {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  border: none;
  background: transparent;
  color: var(--r-text-sub);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.view-switch button.active {
  background: var(--accent-grad);
  color: white;
  box-shadow: 0 6px 15px var(--accent-soft);
}

.view-switch button:not(.active):hover {
  background: rgba(255, 255, 255, 0.5);
  color: var(--accent-color);
}

/* Empty State Luxury */
.empty-state-luxury {
  background: var(--card-bg, #ffffff);
  border-radius: 28px;
  padding: 80px 40px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  border: 1px solid var(--r-border);
  box-shadow: 0 10px 40px rgba(0,0,0,0.02);
  margin-top: 20px;
}

.empty-icon-box {
  width: 80px;
  height: 80px;
  background: var(--accent-soft);
  color: var(--accent-color);
  border-radius: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 24px;
  box-shadow: 0 12px 25px -10px var(--accent-soft);
}

.empty-title {
  font-size: 22px;
  font-weight: 850;
  color: var(--r-text-main);
  margin-bottom: 12px;
  letter-spacing: -0.5px;
}

.empty-text {
  font-size: 15px;
  color: var(--r-text-sub);
  font-weight: 500;
  max-width: 400px;
  line-height: 1.6;
}
.premium-kanban {
  display: flex;
  gap: 24px;
  padding-bottom: 24px;
  overflow-x: auto;
  min-height: 550px;
  width: 100%;
}

.premium-kanban::-webkit-scrollbar {
  height: 8px;
}
.premium-kanban::-webkit-scrollbar-track {
  background: transparent;
}
.premium-kanban::-webkit-scrollbar-thumb {
  background: var(--r-border);
  border-radius: 4px;
}
.premium-kanban::-webkit-scrollbar-thumb:hover {
  background: var(--accent-soft);
}

.premium-list-wrap {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.list-header-row {
  display: flex;
  padding: 12px 24px;
  color: var(--r-text-sub);
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.list-item-premium {
  display: flex;
  align-items: center;
  padding: 16px 24px;
  background: var(--card-bg);
  border-radius: 20px;
  border: 1px solid var(--r-border);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.03);
  transition: all 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
  cursor: pointer;
}

.list-item-premium:hover {
  transform: translateX(5px);
  box-shadow: 0 15px 40px rgba(0,0,0,0.06);
  z-index: 10;
}

.col-cand { flex: 2.5; display: flex; align-items: center; }
.col-score { flex: 1; }
.col-stage { flex: 1.2; }
.col-date { flex: 0.8; }
.col-actions { flex: 1.2; display: flex; justify-content: flex-end; }

.cand-info-premium { display: flex; align-items: center; gap: 12px; }
.cand-avatar-sm { 
  width: 36px; height: 36px; border-radius: 10px; 
  background: var(--avatar-color); color: #fff; 
  display: flex; align-items: center; justify-content: center;
  font-weight: 800; font-size: 12px;
}
.cand-name-sm { font-weight: 800; color: var(--r-text-main); font-size: 14px; }
.cand-role-sm { font-size: 12px; color: var(--r-text-sub); }

.ia-chip-compact {
  padding: 4px 10px; border-radius: 8px; font-size: 12px; font-weight: 800;
  display: inline-flex; align-items: center; gap: 6px;
  box-shadow: 0 2px 6px rgba(0,0,0,0.1);
}
.ia-chip-compact.high { background: linear-gradient(135deg, #10b981, #059669); color: #fff; }
.ia-chip-compact.mid { background: linear-gradient(135deg, #f59e0b, #d97706); color: #fff; }
.ia-chip-compact.low { background: linear-gradient(135deg, #ef4444, #dc2626); color: #fff; }

.mini-tag-sm {
  background: rgba(255,255,255,0.05); border: 1px solid var(--r-border);
  padding: 2px 8px; border-radius: 6px; font-size: 11px; font-weight: 600; color: var(--r-text-sub);
}
.mini-tag-sm.count { background: var(--accent-soft); border-color: transparent; color: var(--accent-color); }

.status-dot-pill {
  display: inline-flex; align-items: center; gap: 8px;
  padding: 4px 12px; border-radius: 100px;
  background: var(--pill-color)11; color: var(--pill-color);
  font-size: 12px; font-weight: 700;
}
.status-dot { width: 6px; height: 6px; border-radius: 50%; background: currentColor; }

.list-actions-cluster { display: flex; gap: 4px; }
.list-action-btn {
  width: 32px; height: 32px; border-radius: 10px; border: 1px solid var(--r-border);
  background: transparent; color: var(--r-text-sub);
  display: flex; align-items: center; justify-content: center;
  cursor: pointer; transition: 0.3s;
}
.list-action-btn svg { width: 14px; height: 14px; }
.list-action-btn:hover { background: var(--r-main-bg); color: var(--accent-color); border-color: var(--accent-color); }
.list-action-btn.active { background: var(--accent-soft); color: var(--accent-color); border-color: transparent; }

.list-action-btn.download-btn:hover { color: #0284c7; background: #e0f2fe; border-color: #0284c7; }
.list-action-btn.calendar-btn:hover { color: #7e22ce; background: #f3e8ff; border-color: #7e22ce; }
.list-action-btn.analyze-btn:hover { color: #d97706; background: #fff7ed; border-color: #d97706; }
.list-action-btn.reject-btn:hover { color: #e11d48; background: #fff1f2; border-color: #e11d48; }

.list-item-premium:hover {
  transform: translateX(5px);
  box-shadow: 0 15px 40px rgba(14, 165, 233, 0.1);
  border-left: 4px solid var(--accent-color);
  z-index: 10;
}

.is-loading { animation: spin 2s linear infinite; }

.kanban-column {
  min-width: 310px;
  max-width: 310px;
  background: rgba(0, 0, 0, 0.02);
  border-radius: 24px;
  padding: 18px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  border: 1px solid transparent;
  transition: all 0.3s ease;
}

.dark-mode .kanban-column {
  background: rgba(255, 255, 255, 0.03);
  border-color: rgba(255, 255, 255, 0.05);
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

/* Premium Scrollbar */
.kanban-content::-webkit-scrollbar {
  width: 4px;
}

.kanban-content::-webkit-scrollbar-track {
  background: transparent;
}

.kanban-content::-webkit-scrollbar-thumb {
  background: var(--r-border);
  border-radius: 10px;
}

.kanban-content::-webkit-scrollbar-thumb:hover {
  background: var(--accent-soft);
}

.kanban-column.drag-over {
  background: rgba(var(--accent-color-rgb), 0.05);
  border-color: var(--accent-color);
  transform: scale(1.01);
}

.candidate-card {
  background: var(--card-bg);
  border: 1px solid var(--r-border);
  border-radius: 20px;
  padding: 18px;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.03);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  cursor: pointer;
  position: relative;
  overflow: visible;
}

.candidate-card::after {
  content: '';
  position: absolute;
  inset: 0;
  background: rgba(255,255,255,0.05);
  pointer-events: none;
  opacity: 0;
  border-radius: inherit;
  transition: 0.4s;
}

.candidate-card:hover {
  transform: translateY(-6px) scale(1.01);
  background: var(--card-bg);
  border-color: var(--accent-color);
  box-shadow: 0 20px 40px -15px rgba(0, 0, 0, 0.4), 0 0 15px var(--accent-soft);
  z-index: 15;
}

.candidate-card:hover::after {
  opacity: 1;
}

.kanban-header-premium {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}

.header-main {
  display: flex;
  align-items: center;
  gap: 12px;
}

.status-indicator {
  width: 4px;
  height: 18px;
  border-radius: 4px;
}

.kanban-header-premium h3 {
  font-size: 13px;
  font-weight: 800;
  color: var(--r-text-main);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.column-count {
  font-size: 12px;
  font-weight: 700;
  color: var(--r-text-sub);
  background: var(--r-main-bg);
  padding: 2px 10px;
  border-radius: 100px;
  border: 1px solid var(--r-border);
}

.card-top {
  display: flex;
  align-items: center;
  gap: 12px;
}

.premium-avatar {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 14px;
  color: white;
  background: var(--avatar-color);
}

.ia-chip-premium {
  padding: 6px 12px;
  border-radius: 12px;
  font-size: 13px;
  font-weight: 800;
  display: flex;
  align-items: center;
  gap: 6px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  border: 1px solid rgba(255,255,255,0.1);
  margin-left: auto;
}

.ia-chip-premium.high { background: linear-gradient(135deg, #10b981, #059669); color: #fff; }
.ia-chip-premium.mid { background: linear-gradient(135deg, #f59e0b, #d97706); color: #fff; }
.ia-chip-premium.low { background: linear-gradient(135deg, #ef4444, #dc2626); color: #fff; }

.ia-spark { width: 14px; height: 14px; }

.btn-quiz-toolbar {
  display: flex;
  align-items: center;
  gap: 10px;
  background: rgba(247, 201, 2, 0.1);
  border: 1px solid rgba(247, 201, 2, 0.3);
  color: #F7C902;
  padding: 8px 16px;
  border-radius: 12px;
  font-weight: 700;
  font-size: 13px;
  cursor: pointer;
  transition: 0.3s;
  height: 42px;
}

.btn-quiz-toolbar:hover {
  background: rgba(247, 201, 2, 0.2);
  transform: translateY(-2px);
  box-shadow: 0 5px 15px rgba(247, 201, 2, 0.15);
}

.icon-pulse-gold {
  display: flex;
  align-items: center;
  justify-content: center;
}

.ia-analyze-btn-outer {
  margin-left: auto;
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 5px 12px;
  background: rgba(var(--accent-color-rgb), 0.1);
  border: 1px solid var(--accent-color);
  border-radius: 10px;
  color: var(--accent-color);
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  font-weight: 700;
  font-size: 11px;
}

.ia-analyze-btn-outer:hover {
  background: var(--accent-color);
  color: #fff;
  box-shadow: 0 0 15px var(--accent-soft);
  transform: translateY(-2px);
}

.ia-spark-container {
  display: flex;
  align-items: center;
  justify-content: center;
}

.ia-spark-container svg {
  width: 14px;
  height: 14px;
}

.is-analyzing .ia-spark-container {
  animation: spin 2s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.card-note-stripe {
  margin: 14px 0;
  padding: 12px;
  background: rgba(255,255,255,0.03);
  border-radius: 16px;
  border: 1px solid var(--r-border);
  display: flex;
  align-items: flex-start;
  gap: 12px;
  cursor: pointer;
  transition: 0.3s;
}

.card-note-stripe:hover {
  background: rgba(255,255,255,0.06);
  border-color: var(--accent-color);
}

.card-note-stripe.has-note {
  background: var(--accent-soft);
  border-color: var(--accent-color);
  border-left: 4px solid var(--accent-color);
}

.note-icon {
  color: var(--r-text-sub);
  flex-shrink: 0;
}

.card-note-stripe.has-note .note-icon {
  color: var(--accent-color);
}

.note-icon svg { width: 14px; height: 14px; }

.note-text {
  font-size: 11px;
  color: var(--r-text-sub);
  margin: 0;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: visible;
}

.card-note-stripe.has-note .note-text {
  color: var(--r-text-main);
  font-weight: 500;
}

.card-skills {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-bottom: 20px;
}

.mini-tag {
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  padding: 4px 10px;
  border-radius: 8px;
  font-size: 11px;
  font-weight: 800;
  color: var(--r-text-sub);
  transition: 0.3s;
}

.mini-tag:hover {
  border-color: var(--accent-color);
  color: var(--accent-color);
  background: var(--accent-soft);
  transform: translateY(-1px);
}

.card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.date-tag {
  font-size: 12px;
  color: var(--r-text-sub);
  display: flex;
  align-items: center;
  gap: 6px;
  font-weight: 500;
}

.date-tag svg {
  width: 14px;
  height: 14px;
}

.action-btn {
  width: 26px;
  height: 26px;
  border-radius: 8px;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.action-btn svg {
  width: 14px;
  height: 14px;
}

.action-btn.accent { background: var(--accent-soft); color: var(--accent-color); }
.action-btn.success { background: #dcfce7; color: #10b981; }
.action-btn.danger { background: #fee2e2; color: #ef4444; }

.action-btn.next-step-btn {
  background: var(--accent-grad);
  color: white;
  width: 38px;
  height: 38px;
  border-radius: 12px;
  box-shadow: 0 4px 12px var(--accent-soft);
}

.action-btn.next-step-btn:hover {
  transform: translateX(3px) scale(1.05);
  box-shadow: 0 6px 18px var(--accent-soft);
}

.action-btn:hover { transform: scale(1.1); filter: brightness(0.95); }

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
}

.modal-header h3 { font-size: 18px; margin: 0; }

.close-btn {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--r-text-sub);
}

.close-btn svg { width: 24px; }

/* Modal styles cleanup */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.4);
  backdrop-filter: blur(4px);
  display: flex; 
  align-items: center; 
  justify-content: center; 
  padding: 40px;
  pointer-events: auto; /* Ensure it blocks clicks to background */
}

.premium-modal {
  background: var(--r-main-bg);
  border-radius: 24px;
  padding: 40px;
  width: 440px;
  text-align: center;
  border: 1px solid var(--r-border);
  box-shadow: 0 25px 50px -12px rgba(0,0,0,0.25);
}

@media (max-width: 1024px) {
  .filter-card { flex-direction: column; align-items: stretch; gap: 16px; }
  .action-left, .action-right-group { justify-content: space-between; }
  .expert-select-glass { min-width: 0; width: 100%; }
}

/* BATTLE MODE STYLES */
/* NEW BATTLE MODE STYLES */
.battle-switch-container {
  display: flex; align-items: center; gap: 12px;
  padding: 6px 16px 6px 8px; border-radius: 100px;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.08);
  cursor: pointer; transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  user-select: none;
}

.battle-switch-container.active {
  background: var(--accent-soft);
  border-color: var(--accent);
  box-shadow: 0 0 20px var(--accent-soft);
}

.battle-switch-track {
  width: 48px; height: 26px; border-radius: 100px;
  background: rgba(0,0,0,0.3); position: relative;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.battle-switch-thumb {
  position: absolute; top: 2px; left: 2px;
  width: 20px; height: 20px; border-radius: 50%;
  background: #fff; display: flex; align-items: center; justify-content: center;
  color: #000; transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 2px 5px rgba(0,0,0,0.2);
}

.active .battle-switch-thumb {
  left: 24px; background: var(--accent);
  box-shadow: 0 0 15px var(--accent-soft);
}

.battle-label {
  font-size: 11px; font-weight: 900; letter-spacing: 1.5px;
  color: #94a3b8; transition: 0.3s;
}

.dark-mode .battle-label { color: rgba(255, 255, 255, 0.4); }

.active .battle-label { color: var(--accent); text-shadow: 0 0 10px var(--accent-soft); }
.dark-mode .active .battle-label { color: var(--accent); }

.launch-battle-btn-elite {
  position: relative; overflow: hidden;
  background: var(--accent-grad);
  border: none; border-radius: 14px; padding: 1px;
  cursor: pointer; transition: all 0.3s;
}

.launch-battle-btn-elite:disabled {
  opacity: 0.5; cursor: not-allowed; filter: grayscale(1);
}

.btn-content {
  background: #fff; color: var(--accent);
  padding: 10px 20px; border-radius: 13px;
  display: flex; align-items: center; gap: 12px;
  font-weight: 950; font-size: 12px; letter-spacing: 1px;
  transition: all 0.3s;
  border: 1px solid var(--accent-soft);
}

.dark-mode .btn-content {
  background: #000;
  border: none;
}

.launch-battle-btn-elite:not(:disabled):hover .btn-content {
  background: transparent; color: #000;
}

.btn-badge {
  background: var(--accent); color: white;
  width: 20px; height: 20px; border-radius: 6px;
  display: flex; align-items: center; justify-content: center;
  font-size: 11px; font-weight: 900;
}

.launch-battle-btn-elite:not(:disabled):hover .btn-badge {
  background: #000; color: var(--accent);
}
.dark-mode .launch-battle-btn-elite:not(:disabled):hover .btn-badge {
  background: #fff;
}

.btn-glow {
  position: absolute; inset: 0;
  background: radial-gradient(circle at center, var(--accent-soft), transparent 70%);
  opacity: 0; transition: 0.3s;
}

.launch-battle-btn-elite:not(:disabled):hover .btn-glow { opacity: 1; }

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
  box-shadow: 0 4px 10px var(--accent-soft);
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
  background: white;
}

.check-box.checked {
  background: var(--accent);
  border-color: var(--accent);
  color: white;
}

.check-box.disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.card-selection-check.selection-disabled,
.col-selection.selection-disabled {
  cursor: not-allowed;
}

.launch-battle-btn {
  animation: slideInRight 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
}

@keyframes slideInRight {
  from { opacity: 0; transform: translateX(20px); }
  to { opacity: 1; transform: translateX(0); }
}

.candidate-card.selected-for-battle {
  border-color: #FFD700 !important;
  box-shadow: 0 0 20px rgba(255, 215, 0, 0.4), 0 20px 40px -15px rgba(0, 0, 0, 0.3);
  background: rgba(255, 215, 0, 0.05) !important;
  transform: scale(1.02) translateY(-5px);
}

.kanban-header-premium {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: rgba(255, 255, 255, 0.03);
  border-radius: 14px;
  margin-bottom: 16px;
}

.kanban-header-premium h3 {
  font-size: 14px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 1px;
  color: var(--r-text-main);
  margin: 0;
}

.column-count {
  background: rgba(255, 255, 255, 0.05);
  padding: 4px 10px;
  border-radius: 8px;
  font-size: 12px;
  font-weight: 800;
  color: var(--r-text-sub);
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.status-indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  box-shadow: 0 0 10px currentColor;
}

.battle-mode-group {
  display: flex;
  gap: 12px;
  padding: 4px;
  background: rgba(255, 255, 255, 0.02);
  border-radius: 18px;
  border: 1px solid rgba(255, 255, 255, 0.05);
}

/* Quiz Badges */
.quiz-badge-mini {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 10px;
  background: rgba(139, 92, 246, 0.1);
  border: 1px solid rgba(139, 92, 246, 0.2);
  color: #8b5cf6;
  border-radius: 10px;
  font-size: 11px;
  font-weight: 800;
  white-space: nowrap;
}

.quiz-badge-mini.pending {
  background: rgba(245, 158, 11, 0.1);
  border: 1px solid rgba(245, 158, 11, 0.2);
  color: #f59e0b;
}

.anim-spin-slow {
  animation: spin 3s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@media (max-width: 1200px) {
  .dashboard-layout { padding: 0; }
}

/* ===== REJECTION MODAL ===== */
.rejection-overlay {
  position: fixed;
  inset: 0;
  z-index: 9999;
  background: rgba(0, 0, 0, 0.55);
  backdrop-filter: blur(6px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
}

.rejection-modal {
  background: var(--r-main-bg, #ffffff);
  border-radius: 20px;
  border: 1px solid var(--r-border, rgba(0,0,0,0.08));
  box-shadow: 0 32px 64px -12px rgba(0, 0, 0, 0.25), 0 0 0 1px rgba(244, 63, 94, 0.06);
  width: 100%;
  max-width: 520px;
  overflow: hidden;
  text-align: left;
}

.dark-mode .rejection-modal {
  background: #0f172a;
  border-color: rgba(255,255,255,0.08);
  box-shadow: 0 32px 64px -12px rgba(0, 0, 0, 0.6);
}

/* Header */
.rej-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 24px 24px 0 24px;
  gap: 12px;
}

.rej-header-left {
  display: flex;
  align-items: center;
  gap: 14px;
  flex: 1;
  min-width: 0;
}

.rej-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: rgba(244, 63, 94, 0.1);
  color: #f43f5e;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  border: 1px solid rgba(244, 63, 94, 0.15);
}

.dark-mode .rej-icon {
  background: rgba(244, 63, 94, 0.12);
  border-color: rgba(244, 63, 94, 0.2);
}

.rej-header-text { min-width: 0; }

.rej-title {
  margin: 0;
  font-size: 17px;
  font-weight: 800;
  color: var(--r-text-main, #1e293b);
  line-height: 1.25;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.dark-mode .rej-title { color: #f1f5f9; }

.rej-subtitle {
  margin: 3px 0 0 0;
  font-size: 13px;
  color: var(--r-text-sub, #64748b);
  line-height: 1.4;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.rej-subtitle strong { color: var(--r-text-main, #1e293b); font-weight: 700; }
.dark-mode .rej-subtitle strong { color: #f1f5f9; }

.rej-close {
  background: transparent;
  border: none;
  color: var(--r-text-sub, #64748b);
  cursor: pointer;
  padding: 6px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
  flex-shrink: 0;
}

.rej-close:hover {
  background: rgba(244, 63, 94, 0.08);
  color: #f43f5e;
}

/* Body */
.rej-body {
  padding: 20px 24px;
}

.rej-label {
  display: block;
  font-size: 13px;
  font-weight: 700;
  color: var(--r-text-main, #1e293b);
  margin-bottom: 10px;
}

.dark-mode .rej-label { color: #e2e8f0; }

.rej-label-opt {
  font-weight: 500;
  font-size: 12px;
  color: var(--r-text-sub, #94a3b8);
}

.rej-textarea {
  width: 100%;
  border-radius: 12px;
  padding: 14px;
  background: var(--r-card-bg, #f8fafc);
  color: var(--r-text-main, #1e293b);
  border: 1.5px solid var(--r-border, #e2e8f0);
  font-family: inherit;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
  resize: none;
  line-height: 1.6;
  box-sizing: border-box;
}

.rej-textarea:focus {
  border-color: #f43f5e;
  box-shadow: 0 0 0 3px rgba(244, 63, 94, 0.08);
}

.dark-mode .rej-textarea {
  background: rgba(255,255,255,0.04);
  border-color: rgba(255,255,255,0.1);
  color: #f1f5f9;
}

.dark-mode .rej-textarea:focus {
  border-color: #f43f5e;
  box-shadow: 0 0 0 3px rgba(244, 63, 94, 0.12);
}

.rej-hint {
  margin: 10px 0 0 0;
  font-size: 12px;
  color: var(--r-text-sub, #94a3b8);
  line-height: 1.5;
}

/* Footer */
.rej-footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 10px;
  padding: 0 24px 24px 24px;
}

.rej-btn-cancel {
  padding: 10px 20px;
  border-radius: 10px;
  border: 1.5px solid var(--r-border, #e2e8f0);
  background: transparent;
  color: var(--r-text-sub, #64748b);
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
}

.rej-btn-cancel:hover {
  background: var(--r-card-bg, #f8fafc);
  color: var(--r-text-main, #1e293b);
  border-color: var(--r-text-sub, #94a3b8);
}

.dark-mode .rej-btn-cancel {
  border-color: rgba(255,255,255,0.1);
  color: #94a3b8;
}

.dark-mode .rej-btn-cancel:hover {
  background: rgba(255,255,255,0.05);
  color: #e2e8f0;
}

.rej-btn-confirm {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border-radius: 10px;
  border: none;
  background: #f43f5e;
  color: white;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  white-space: nowrap;
  box-shadow: 0 4px 12px rgba(244, 63, 94, 0.3);
}

.rej-btn-confirm:hover:not(:disabled) {
  background: #e11d48;
  transform: translateY(-1px);
  box-shadow: 0 6px 18px rgba(244, 63, 94, 0.4);
}

.rej-btn-confirm:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

/* Modal transition */
.modal-fade-enter-active, .modal-fade-leave-active {
  transition: opacity 0.2s ease;
}
.modal-fade-enter-from, .modal-fade-leave-to {
  opacity: 0;
}
</style>
