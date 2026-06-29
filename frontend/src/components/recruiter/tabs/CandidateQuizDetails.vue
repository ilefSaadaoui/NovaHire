<template>
  <div class="panel-fade-celestial">
    <div class="quiz-details-elite">
      
      <div v-if="loading" class="quiz-loading-elite">
        <div class="loader-ripple"><div></div><div></div></div>
        <p>Analyse des performances en cours...</p>
      </div>

      <template v-else-if="result">
        <!-- ═══ EXECUTIVE SUMMARY HEADER ═══ -->
        <div class="elite-summary-dashboard">
          <div class="summary-visual-core">
            <div class="radial-score-elite" :class="performanceClass" :style="{ '--score-pct': result.score }">
              <svg viewBox="0 0 100 100">
                <defs>
                  <linearGradient id="scoreGrad" x1="0%" y1="0%" x2="100%" y2="100%">
                    <stop offset="0%" style="stop-color:var(--accent)" />
                    <stop offset="100%" style="stop-color:var(--accent-dark)" />
                  </linearGradient>
                  <filter id="glow">
                    <feGaussianBlur stdDeviation="2.5" result="coloredBlur"/>
                    <feMerge>
                      <feMergeNode in="coloredBlur"/><feMergeNode in="SourceGraphic"/>
                    </feMerge>
                  </filter>
                </defs>
                <circle cx="50" cy="50" r="42" class="rail" />
                <circle cx="50" cy="50" r="42" class="progress" stroke="url(#scoreGrad)" filter="url(#glow)" />
              </svg>
              <div class="score-content-elite">
                <span class="score-value">{{ Math.round(result.score) }}%</span>
                <span class="score-label">Global</span>
              </div>
            </div>
          </div>
          
          <div class="summary-intel-grid">
            <div class="intel-card">
              <div class="intel-icon-wrap correct">
                <CheckCircle2 :size="20" />
              </div>
              <div class="intel-data">
                <span class="intel-val">{{ correctCount }}</span>
                <span class="intel-lab">Réponses Correctes</span>
              </div>
            </div>

            <div class="intel-card">
              <div class="intel-icon-wrap error">
                <XCircle :size="20" />
              </div>
              <div class="intel-data">
                <span class="intel-val">{{ result.reviews.length - correctCount }}</span>
                <span class="intel-lab">Erreurs Relevées</span>
              </div>
            </div>

            <div class="intel-card">
              <div class="intel-icon-wrap time">
                <CalendarDays :size="20" />
              </div>
              <div class="intel-data">
                <span class="intel-val">{{ formatDateShort(result.completedAt) }}</span>
                <span class="intel-lab">Date de Passage</span>
              </div>
            </div>
          </div>

          <div class="summary-executive-verdict">
            <div class="verdict-tag" :class="performanceClass">
              <Sparkles v-if="result.score >= 80" :size="16" />
              <Zap v-else-if="result.score >= 50" :size="16" />
              <AlertCircle v-else :size="16" />
              <span>{{ performanceLabel }}</span>
            </div>
            <p class="verdict-desc">
              {{ performanceDescription }}
            </p>
          </div>
        </div>

        <!-- ═══ DETAILED QUESTIONS NAVIGATION ═══ -->
        <div class="elite-questions-navigation">
          <div class="navigation-toolbar">
            <div class="nav-info">
              <span class="nav-count">Question {{ currentIndex + 1 }} <span>sur {{ result.reviews.length }}</span></span>
            </div>
            
            <div class="nav-controls">
              <button class="nav-btn-step" :disabled="currentIndex === 0" @click="currentIndex--">
                <ChevronLeft :size="18" />
                <span>Précédent</span>
              </button>
              
              <div class="nav-jump-dots">
                <button 
                  v-for="(q, idx) in result.reviews" 
                  :key="idx"
                  class="jump-dot"
                  :class="{ 'is-active': currentIndex === idx, 'is-error': !q.isCorrect }"
                  @click="currentIndex = idx"
                  :title="`Question ${idx + 1}`"
                ></button>
              </div>

              <button class="nav-btn-step next" :disabled="currentIndex === result.reviews.length - 1" @click="currentIndex++">
                <span>Suivant</span>
                <ChevronRight :size="18" />
              </button>
            </div>
          </div>

          <transition name="q-slide" mode="out-in">
            <div :key="currentIndex" class="elite-question-card" 
              :class="{ 'is-invalid': !currentQuestion.isCorrect, 'is-valid': currentQuestion.isCorrect }"
            >
              <div class="card-side-indicator"></div>
              
              <div class="q-header-elite">
                <div class="q-badge">Détails de l'Évaluation</div>
                <div class="q-result-badge" :class="currentQuestion.isCorrect ? 'valid' : 'invalid'">
                  <template v-if="currentQuestion.isCorrect">
                    <Check :size="12" stroke-width="4" /> <span>Validé</span>
                  </template>
                  <template v-else>
                    <X :size="12" stroke-width="4" /> <span>Échec</span>
                  </template>
                </div>
              </div>

              <h3 class="q-title-elite">{{ currentQuestion.text }}</h3>

              <div class="elite-options-grid">
                <div v-for="(opt, oIdx) in currentQuestion.options" :key="oIdx" 
                  class="elite-option-item"
                  :class="{ 
                    'is-correct': oIdx === currentQuestion.correctAnswerIndex, 
                    'is-selected': oIdx === currentQuestion.candidateAnswerIndex,
                    'is-wrong-selection': oIdx === currentQuestion.candidateAnswerIndex && !currentQuestion.isCorrect
                  }"
                >
                  <div class="option-marker">
                    <span class="letter">{{ String.fromCharCode(65 + oIdx) }}</span>
                    <div class="dot-indicator"></div>
                  </div>
                  
                  <div class="option-body">
                    <span class="text">{{ cleanOption(opt) }}</span>
                    <div class="status-tags">
                      <span v-if="oIdx === currentQuestion.correctAnswerIndex" class="tag-correct">
                        <Check :size="10" /> Réponse attendue
                      </span>
                      <span v-if="oIdx === currentQuestion.candidateAnswerIndex" class="tag-candidate">
                        Son choix
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              <div v-if="currentQuestion.explanation" class="elite-explanation-box">
                <div class="exp-glow"></div>
                <div class="exp-header">
                  <div class="ai-brain-icon">
                    <Sparkles :size="14" />
                  </div>
                  <span>Analyse de l'Intelligence Artificielle</span>
                </div>
                <div class="exp-content">
                  <p>{{ currentQuestion.explanation }}</p>
                </div>
              </div>
            </div>
          </transition>
        </div>
      </template>

      <div v-else class="elite-empty-state">
        <div class="empty-glass-circle">
          <ClipboardCheck :size="40" />
        </div>
        <template v-if="candidate?.hasQuizGenerated">
          <h2>Évaluation Introuvable</h2>
          <p>Le candidat n'a pas encore complété son test technique IA.</p>
          <button class="pill-btn primary mt-4" @click="fetchResults">Actualiser</button>
        </template>
        <template v-else>
          <h2>Quiz non généré</h2>
          <p>Le quiz technique IA n'a pas encore été généré pour cette offre d'emploi.</p>
        </template>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { 
  Loader2, CheckCircle2, XCircle, Trophy, Star, 
  AlertCircle, Sparkles, Zap, ClipboardCheck,
  Check, X, CalendarDays, ChevronLeft, ChevronRight
} from 'lucide-vue-next'
import api from '@/api/axios'

const props = defineProps({
  applicationId: { type: String, required: true },
  candidate: { type: Object, default: () => ({}) }
})

const result = ref(null)
const loading = ref(true)
const currentIndex = ref(0)

const currentQuestion = computed(() => {
  if (!result.value || !result.value.reviews) return null
  return result.value.reviews[currentIndex.value]
})

const fetchResults = async () => {
  loading.value = true
  try {
    const res = await api.get(`/quiz/results/${props.applicationId}`)
    result.value = res.data
  } catch (err) {
    if (err.response && err.response.status === 404) {
      result.value = null
    } else {
      console.error('Erreur quiz details:', err)
    }
  } finally {
    loading.value = false
  }
}

const cleanOption = (text) => {
  if (!text) return ''
  return text.replace(/^[A-Z0-9][\).]\s*/i, '')
}

const correctCount = computed(() => {
  if (!result.value) return 0
  return result.value.reviews.filter(r => r.isCorrect).length
})

const performanceLabel = computed(() => {
  if (!result.value) return ''
  const s = result.value.score
  if (s >= 90) return 'Excellence Critique'
  if (s >= 75) return 'Profil Hautement Qualifié'
  if (s >= 50) return 'Compétences Validées'
  return 'Ajustements Requis'
})

const performanceDescription = computed(() => {
  if (!result.value) return ''
  const s = result.value.score
  if (s >= 90) return 'Le candidat démontre une maîtrise parfaite du sujet avec une précision exceptionnelle.'
  if (s >= 75) return 'Une très bonne compréhension des concepts clés. Le profil est solide pour ce poste.'
  if (s >= 50) return 'Le niveau est satisfaisant pour les tâches opérationnelles de base.'
  return 'Certaines lacunes techniques ont été identifiées. Un accompagnement pourrait être nécessaire.'
})

const performanceClass = computed(() => {
  if (!result.value) return ''
  const s = result.value.score
  if (s >= 75) return 'elite-high'
  if (s >= 50) return 'elite-mid'
  return 'elite-low'
})

const formatDateShort = (dateStr) => {
  if (!dateStr) return ''
  return new Date(dateStr).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric' })
}

onMounted(fetchResults)
</script>

<style scoped>
.quiz-details-elite {
  padding: 10px 0 40px;
}

/* ─── LOADER ─── */
.quiz-loading-elite {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  color: #64748b;
}

.loader-ripple {
  display: inline-block;
  position: relative;
  width: 80px;
  height: 80px;
  margin-bottom: 20px;
}
.loader-ripple div {
  position: absolute;
  border: 4px solid var(--accent);
  opacity: 1;
  border-radius: 50%;
  animation: ripple 1s cubic-bezier(0, 0.2, 0.8, 1) infinite;
}
.loader-ripple div:nth-child(2) { animation-delay: -0.5s; }
@keyframes ripple {
  0% { top: 36px; left: 36px; width: 0; height: 0; opacity: 1; }
  100% { top: 0; left: 0; width: 72px; height: 72px; opacity: 0; }
}

/* ─── SUMMARY DASHBOARD ─── */
.elite-summary-dashboard {
  display: grid;
  grid-template-columns: auto 1fr 1.5fr;
  gap: 40px;
  background: white;
  border: 1px solid rgba(226, 232, 240, 0.8);
  border-radius: 32px;
  padding: 32px;
  margin-bottom: 40px;
  box-shadow: 0 15px 35px -10px rgba(0, 0, 0, 0.05);
  align-items: center;
}

.dark-mode .elite-summary-dashboard {
  background: #0f172a;
  border-color: rgba(255, 255, 255, 0.08);
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.3);
}

.radial-score-elite {
  position: relative;
  width: 140px;
  height: 140px;
}

.radial-score-elite svg {
  width: 140px;
  height: 140px;
  transform: rotate(-90deg);
}

.radial-score-elite .rail {
  fill: none;
  stroke: #f1f5f9;
  stroke-width: 6;
}

.dark-mode .radial-score-elite .rail { stroke: rgba(255, 255, 255, 0.05); }

.radial-score-elite .progress {
  fill: none;
  stroke-width: 7;
  stroke-linecap: round;
  stroke-dasharray: 264; /* 2 * PI * 42 */
  stroke-dashoffset: calc(264 - (264 * var(--score-pct)) / 100);
  transition: stroke-dashoffset 2s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.score-content-elite {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.score-value { font-size: 32px; font-weight: 900; color: #0f172a; line-height: 1; letter-spacing: -1px; }
.score-label { font-size: 10px; font-weight: 800; color: #94a3b8; text-transform: uppercase; margin-top: 4px; letter-spacing: 1px; }

.dark-mode .score-value { color: white; }

/* Metrics */
.summary-intel-grid {
  display: flex;
  flex-direction: column;
  gap: 16px;
  border-left: 1px solid #f1f5f9;
  padding-left: 40px;
}

.dark-mode .summary-intel-grid { border-color: rgba(255, 255, 255, 0.05); }

.intel-card {
  display: flex;
  align-items: center;
  gap: 14px;
}

.intel-icon-wrap {
  width: 36px;
  height: 36px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.intel-icon-wrap.correct { background: rgba(16, 185, 129, 0.1); color: #10b981; }
.intel-icon-wrap.error { background: rgba(239, 68, 68, 0.1); color: #ef4444; }
.intel-icon-wrap.time { background: rgba(99, 102, 241, 0.1); color: #6366f1; }

.intel-data { display: flex; flex-direction: column; }
.intel-val { font-size: 16px; font-weight: 800; color: #1e293b; line-height: 1.2; }
.intel-lab { font-size: 11px; font-weight: 600; color: #94a3b8; }

.dark-mode .intel-val { color: #f1f5f9; }

/* Verdict */
.summary-executive-verdict {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.verdict-tag {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 14px;
  border-radius: 100px;
  font-size: 12px;
  font-weight: 900;
  width: fit-content;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.verdict-tag.elite-high { background: #10b981; color: white; box-shadow: 0 4px 15px rgba(16, 185, 129, 0.3); }
.verdict-tag.elite-mid { background: #f59e0b; color: white; box-shadow: 0 4px 15px rgba(245, 158, 11, 0.3); }
.verdict-tag.elite-low { background: #ef4444; color: white; box-shadow: 0 4px 15px rgba(239, 68, 68, 0.3); }

.verdict-desc { font-size: 14px; color: #64748b; line-height: 1.6; margin: 0; }
.dark-mode .verdict-desc { color: #94a3b8; }

/* ─── QUESTIONS FEED ─── */
/* ─── QUESTIONS NAVIGATION ─── */
.elite-questions-navigation {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.navigation-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 10px;
  margin-bottom: 10px;
}

.nav-count {
  font-size: 16px;
  font-weight: 900;
  color: #1e293b;
}

.dark-mode .nav-count { color: white; }

.nav-count span {
  font-size: 13px;
  font-weight: 600;
  color: #94a3b8;
  margin-left: 4px;
}

.nav-controls {
  display: flex;
  align-items: center;
  gap: 24px;
}

.nav-btn-step {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border-radius: 14px;
  font-size: 13px;
  font-weight: 800;
  border: 1px solid #e2e8f0;
  background: white;
  color: #475569;
  cursor: pointer;
  transition: all 0.3s;
}

.dark-mode .nav-btn-step {
  background: rgba(255, 255, 255, 0.05);
  border-color: rgba(255, 255, 255, 0.1);
  color: #cbd5e1;
}

.nav-btn-step:hover:not(:disabled) {
  border-color: var(--accent);
  color: var(--accent);
  background: var(--accent-soft);
  transform: translateY(-2px);
}

.nav-btn-step:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.nav-btn-step.next {
  background: #1e293b;
  color: white;
  border: none;
}

.dark-mode .nav-btn-step.next {
  background: var(--accent);
  color: #0f172a;
}

.nav-btn-step.next:hover:not(:disabled) {
  background: #0f172a;
  color: white;
}

.dark-mode .nav-btn-step.next:hover:not(:disabled) {
  background: white;
  color: #0f172a;
}

.nav-jump-dots {
  display: flex;
  gap: 6px;
}

.jump-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #cbd5e1;
  border: none;
  cursor: pointer;
  transition: all 0.3s;
  padding: 0;
}

.jump-dot.is-active {
  width: 24px;
  border-radius: 10px;
  background: var(--accent);
}

.jump-dot.is-error {
  background: #fca5a5;
}

.jump-dot.is-error.is-active {
  background: #ef4444;
}

.dark-mode .jump-dot { background: rgba(255, 255, 255, 0.1); }
.dark-mode .jump-dot.is-active { background: var(--accent); }

/* Slide Transition */
.q-slide-enter-active, .q-slide-leave-active {
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.q-slide-enter-from {
  opacity: 0;
  transform: translateX(30px);
}

.q-slide-leave-to {
  opacity: 0;
  transform: translateX(-30px);
}

.elite-questions-feed {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.elite-question-card {
  position: relative;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 24px;
  padding: 32px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  overflow: hidden;
}

.dark-mode .elite-question-card { background: rgba(255, 255, 255, 0.02); border-color: rgba(255, 255, 255, 0.05); }

.elite-question-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 20px 40px -20px rgba(0, 0, 0, 0.1);
  border-color: var(--accent-soft);
}

.card-side-indicator {
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  background: #cbd5e1;
}

.is-valid .card-side-indicator { background: #10b981; }
.is-invalid .card-side-indicator { background: #ef4444; }

.q-header-elite {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.q-badge {
  background: #f1f5f9;
  padding: 4px 12px;
  border-radius: 8px;
  font-size: 11px;
  font-weight: 800;
  color: #64748b;
  text-transform: uppercase;
}

.dark-mode .q-badge { background: rgba(255, 255, 255, 0.05); color: #94a3b8; }

.q-result-badge {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
}

.q-result-badge.valid { color: #10b981; }
.q-result-badge.invalid { color: #ef4444; }

.q-title-elite {
  font-size: 18px;
  font-weight: 800;
  color: #0f172a;
  line-height: 1.5;
  margin: 0 0 28px 0;
}

.dark-mode .q-title-elite { color: #f1f5f9; }

/* Options Grid */
.elite-options-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-bottom: 32px;
}

.elite-option-item {
  display: flex;
  gap: 16px;
  padding: 16px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  transition: all 0.2s ease;
  position: relative;
}

.dark-mode .elite-option-item { background: rgba(255, 255, 255, 0.01); border-color: rgba(255, 255, 255, 0.05); }

.elite-option-item.is-selected { border-color: #1e293b; background: white; z-index: 2; }
.dark-mode .elite-option-item.is-selected { border-color: var(--accent); background: rgba(255, 255, 255, 0.03); }

.elite-option-item.is-correct { border-color: #10b981; background: rgba(16, 185, 129, 0.05); }
.elite-option-item.is-wrong-selection { border-color: #ef4444; background: rgba(239, 68, 68, 0.05); }

.option-marker { position: relative; }

.letter {
  width: 32px;
  height: 32px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 900;
  font-size: 12px;
  color: #64748b;
  transition: all 0.3s;
}

.dark-mode .letter { background: #0f172a; border-color: rgba(255, 255, 255, 0.1); }

.is-selected .letter { background: #1e293b; color: white; border-color: #1e293b; }
.is-correct .letter { background: #10b981; color: white; border-color: #10b981; }
.is-wrong-selection .letter { background: #ef4444; color: white; border-color: #ef4444; }

.dot-indicator {
  position: absolute;
  top: -2px;
  right: -2px;
  width: 10px;
  height: 10px;
  border-radius: 50%;
  border: 2px solid white;
  background: var(--accent);
  opacity: 0;
  transform: scale(0);
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.is-selected .dot-indicator { opacity: 1; transform: scale(1); }
.dark-mode .dot-indicator { border-color: #0f172a; }

.option-body { flex: 1; display: flex; flex-direction: column; gap: 4px; }
.text { font-size: 14px; font-weight: 600; color: #475569; }
.dark-mode .text { color: #cbd5e1; }

.status-tags { display: flex; gap: 8px; }
.tag-correct { font-size: 10px; font-weight: 800; color: #10b981; text-transform: uppercase; display: flex; align-items: center; gap: 4px; }
.tag-candidate { font-size: 10px; font-weight: 800; color: #1e293b; text-transform: uppercase; padding: 2px 8px; background: #f1f5f9; border-radius: 4px; }

.dark-mode .tag-candidate { background: rgba(255, 255, 255, 0.1); color: white; }

/* AI Explanation Box */
.elite-explanation-box {
  position: relative;
  background: #fffbeb;
  border: 1px solid #fde68a;
  border-radius: 20px;
  padding: 24px;
  overflow: hidden;
}

.dark-mode .elite-explanation-box { background: rgba(245, 158, 11, 0.03); border-color: rgba(245, 158, 11, 0.1); }

.exp-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 12px;
}

.ai-brain-icon {
  width: 28px;
  height: 28px;
  background: var(--accent);
  color: white;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 10px var(--accent-glow);
}

.exp-header span { font-size: 12px; font-weight: 900; color: #b45309; text-transform: uppercase; letter-spacing: 0.5px; }
.dark-mode .exp-header span { color: var(--accent); }

.exp-content p { font-size: 14px; color: #78350f; line-height: 1.6; margin: 0; }
.dark-mode .exp-content p { color: #d97706; opacity: 0.9; }

/* Empty State */
.elite-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 80px 20px;
  background: white;
  border-radius: 32px;
  border: 1px dashed #cbd5e1;
}

.dark-mode .elite-empty-state { background: transparent; border-color: rgba(255, 255, 255, 0.1); }

.empty-glass-circle {
  width: 100px;
  height: 100px;
  background: #f1f5f9;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 24px;
  color: #94a3b8;
}

.elite-empty-state h2 { font-size: 24px; font-weight: 800; color: #0f172a; margin-bottom: 8px; }
.dark-mode .elite-empty-state h2 { color: white; }
.elite-empty-state p { color: #64748b; max-width: 300px; }

@media (max-width: 1024px) {
  .elite-summary-dashboard { grid-template-columns: 1fr; text-align: center; }
  .summary-intel-grid { border-left: none; padding-left: 0; flex-direction: row; justify-content: center; border-top: 1px solid #f1f5f9; padding-top: 24px; }
  .summary-executive-verdict { align-items: center; }
  .elite-options-grid { grid-template-columns: 1fr; }
}
</style>
