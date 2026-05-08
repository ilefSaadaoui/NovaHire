<template>
  <div class="copilot-panel" :class="{ 'live-active': liveMode }" @keydown="handleKeyboard">
    <!-- COCKPIT HEADER -->
    <div class="copilot-header">
      <div class="header-left">
        <div class="copilot-brand">
          <div class="brand-orb"><Sparkles :size="16" /></div>
          <div class="brand-text">
            <h3>Copilote d'Entretien</h3>
            <span class="brand-sub">Assistant en temps réel</span>
          </div>
        </div>
      </div>
      <div class="header-right">
        <div class="stat-chips">
          <div class="chip"><Brain :size="12" /><span>{{ localQuestions.length }} Q.</span></div>
          <div class="chip success" v-if="completedCount > 0">
            <CheckCircle :size="12" /><span>{{ completedCount }}/{{ localQuestions.length }}</span>
          </div>
        </div>
        <div class="live-control" :class="{ 'is-on': liveMode }">
          <div class="live-dot"></div>
          <span class="live-label">{{ liveMode ? 'LIVE' : 'OFF' }}</span>
          <label class="toggle-sw">
            <input type="checkbox" v-model="liveMode">
            <span class="sw-track"></span>
          </label>
        </div>
        <transition name="scale">
          <button v-if="liveMode" @click="saveEvaluation" class="save-btn" :disabled="saving">
            <Save :size="14" v-if="!saving" /><Loader2 :size="14" class="spin" v-else />
            <span>{{ saving ? 'Sync...' : 'Sauver' }}</span>
          </button>
        </transition>
      </div>
    </div>

    <!-- PROGRESS BAR -->
    <div v-if="liveMode && hasQuestions" class="progress-strip">
      <div class="progress-fill" :style="{ width: progressPct + '%' }"></div>
    </div>

    <!-- CANDIDATE QUICK CARD -->
    <div class="candidate-quick-card" v-if="candidate">
      <div class="cqc-avatar">{{ initials }}</div>
      <div class="cqc-info">
        <span class="cqc-name">{{ candidate.fullName || 'Candidat' }}</span>
        <span class="cqc-job">{{ candidate.jobTitle || 'Poste' }}</span>
      </div>
      <div class="cqc-score" v-if="candidate.overallScore">
        <span class="score-num">{{ candidate.overallScore }}%</span>
        <span class="score-label">Match IA</span>
      </div>
    </div>

    <!-- QUESTIONS STACK -->
    <div v-if="hasQuestions" class="questions-list" ref="questionsContainer">
      <div
        v-for="(q, idx) in localQuestions"
        :key="idx"
        class="q-card"
        :class="{
          'is-active': activeIdx === idx,
          'is-done': q.score > 0,
          'is-expanded': expandedIdx === idx && liveMode
        }"
        @click="selectQuestion(idx)"
        :ref="el => { if (el) qRefs[idx] = el }"
      >
        <div class="q-top">
          <div class="q-left">
            <span class="q-num" :class="{ done: q.score > 0 }">
              <CheckCircle v-if="q.score > 0" :size="14" />
              <span v-else>#{{ idx + 1 }}</span>
            </span>
            <span class="q-cat">{{ q.category }}</span>
          </div>
          <div class="q-right">
            <!-- Inline star rating (compact) -->
            <div v-if="liveMode" class="inline-stars" @click.stop>
              <button
                v-for="s in 5" :key="s"
                class="star-btn-mini"
                :class="{ filled: s <= (q.score || 0) }"
                @click="rateQuestion(idx, s)"
              >
                <Star :size="16" :fill="s <= (q.score || 0) ? '#FFD700' : 'none'" :color="s <= (q.score || 0) ? '#FFD700' : '#94a3b8'" stroke-width="2" />
              </button>
            </div>
            <div v-if="q.score > 0 && !liveMode" class="score-pill">{{ q.score }}/5</div>
            <button v-if="liveMode" @click.stop="removeQuestion(idx)" class="trash-btn"><Trash2 :size="14" /></button>
          </div>
        </div>

        <div class="q-body">
          <p v-if="!liveMode || expandedIdx !== idx" class="q-text">{{ q.question }}</p>
          <textarea
            v-if="liveMode && expandedIdx === idx"
            v-model="q.question"
            class="q-edit"
            rows="2"
            @click.stop
          ></textarea>

          <!-- Purpose tooltip on hover -->
          <div class="q-purpose" v-if="expandedIdx === idx && q.purpose">
            <Target :size="12" />
            <span>{{ q.purpose }}</span>
          </div>
        </div>

        <!-- Expanded evaluation zone -->
        <transition name="expand">
          <div v-if="liveMode && expandedIdx === idx" class="eval-zone" @click.stop>
            <div class="notes-row">
              <MessageSquare :size="12" />
              <textarea
                v-model="q.notes"
                class="notes-input"
                :placeholder="'Notes rapides... (Raccourci: N)'"
                rows="2"
                ref="notesInput"
              ></textarea>
            </div>
          </div>
        </transition>

        <div class="q-status-bar" :class="{ done: q.score > 0 }"></div>
      </div>

      <!-- Add question button -->
      <button v-if="liveMode" @click="addQuestion" class="add-q-btn">
        <Plus :size="16" /><span>Ajouter une question</span>
      </button>
    </div>

    <!-- EMPTY STATE -->
    <div v-else class="empty-state">
      <div class="empty-icon"><Brain :size="32" /></div>
      <h4>Préparation du Guide</h4>
      <p>L'IA génère les questions optimales lors de l'analyse du CV.</p>
    </div>

    <!-- EVALUATION SUMMARY FOOTER -->
    <div v-if="liveMode && hasQuestions" class="eval-footer">
      <div class="footer-stats">
        <div class="f-stat">
          <span class="f-val">{{ avgScore.toFixed(1) }}</span>
          <span class="f-label">/5 Moy.</span>
        </div>
        <div class="f-divider"></div>
        <div class="f-stat">
          <span class="f-val">{{ completedCount }}</span>
          <span class="f-label">/{{ localQuestions.length }} évaluées</span>
        </div>
      </div>
      <div class="footer-shortcuts">
        <span class="shortcut-hint"><kbd>↑↓</kbd> Naviguer</span>
        <span class="shortcut-hint"><kbd>1-5</kbd> Noter</span>
        <span class="shortcut-hint"><kbd>Ctrl+S</kbd> Sauver</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import api from '@/api/axios'
import { Sparkles, Save, Star, MessageSquare, Trash2, Plus, Brain, CheckCircle, Target, Loader2 } from 'lucide-vue-next'

const props = defineProps({ candidate: { type: Object, required: true } })
const route = useRoute()
const localQuestions = ref([])
const saving = ref(false)
const liveMode = ref(false)
const activeIdx = ref(0)
const expandedIdx = ref(-1)
const qRefs = ref({})
const questionsContainer = ref(null)

const hasQuestions = computed(() => localQuestions.value.length > 0)
const completedCount = computed(() => localQuestions.value.filter(q => q.score > 0).length)
const progressPct = computed(() => hasQuestions.value ? (completedCount.value / localQuestions.value.length * 100) : 0)
const avgScore = computed(() => {
  const scored = localQuestions.value.filter(q => q.score > 0)
  if (!scored.length) return 0
  return scored.reduce((sum, q) => sum + q.score, 0) / scored.length
})

const initials = computed(() => {
  const name = props.candidate?.fullName || ''
  const parts = name.split(' ')
  return parts.length >= 2 ? (parts[0][0] + parts[1][0]).toUpperCase() : name.substring(0, 2).toUpperCase()
})

const getScoreLabel = (score) => {
  const labels = { 1: 'Insuffisant', 2: 'Moyen', 3: 'Satisfaisant', 4: 'Très Bien', 5: 'Exceptionnel' }
  return labels[score] || ''
}

// Data fetching
const fetchQuestions = () => {
  if (props.candidate?.interviewQuestions) {
    localQuestions.value = JSON.parse(JSON.stringify(props.candidate.interviewQuestions))
  }
}
onMounted(fetchQuestions)
watch(() => props.candidate, fetchQuestions, { deep: true })

// Question management
const selectQuestion = (idx) => {
  activeIdx.value = idx
  if (liveMode.value) {
    expandedIdx.value = expandedIdx.value === idx ? -1 : idx
  }
}

const rateQuestion = (idx, score) => {
  localQuestions.value[idx].score = score
}

const addQuestion = () => {
  localQuestions.value.push({ category: 'Libre', question: '', purpose: 'Complément', score: 0, notes: '' })
  activeIdx.value = localQuestions.value.length - 1
  expandedIdx.value = activeIdx.value
}

const removeQuestion = (idx) => {
  localQuestions.value.splice(idx, 1)
  if (activeIdx.value >= localQuestions.value.length) activeIdx.value = Math.max(0, localQuestions.value.length - 1)
  if (expandedIdx.value === idx) expandedIdx.value = -1
}

// Save
const saveEvaluation = async () => {
  saving.value = true
  try {
    const appId = route.params.id
    await api.patch(`/Recruiter/applications/${appId}/interview-guide`, localQuestions.value)
    const { useToastStore } = await import('@/stores/toastStore')
    useToastStore().show('Évaluation sauvegardée', 'success')
  } catch (e) { console.error(e) }
  finally { setTimeout(() => { saving.value = false }, 600) }
}

// Keyboard shortcuts
const handleKeyboard = (e) => {
  if (!liveMode.value || !hasQuestions.value) return
  // Don't intercept if typing in an input
  const tag = e.target.tagName
  if (tag === 'TEXTAREA' || tag === 'INPUT') {
    if (e.ctrlKey && e.key === 's') { e.preventDefault(); saveEvaluation() }
    return
  }

  switch (e.key) {
    case 'ArrowDown':
      e.preventDefault()
      activeIdx.value = Math.min(activeIdx.value + 1, localQuestions.value.length - 1)
      expandedIdx.value = activeIdx.value
      scrollToActive()
      break
    case 'ArrowUp':
      e.preventDefault()
      activeIdx.value = Math.max(activeIdx.value - 1, 0)
      expandedIdx.value = activeIdx.value
      scrollToActive()
      break
    case '1': case '2': case '3': case '4': case '5':
      rateQuestion(activeIdx.value, parseInt(e.key))
      break
    case 'n': case 'N':
      e.preventDefault()
      expandedIdx.value = activeIdx.value
      nextTick(() => {
        const notes = document.querySelector('.eval-zone .notes-input')
        if (notes) notes.focus()
      })
      break
    case 's':
      if (e.ctrlKey) { e.preventDefault(); saveEvaluation() }
      break
  }
}

const scrollToActive = () => {
  nextTick(() => {
    const el = qRefs.value[activeIdx.value]
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'nearest' })
  })
}

// Focus management
onMounted(() => { document.addEventListener('keydown', handleKeyboard) })
onUnmounted(() => { document.removeEventListener('keydown', handleKeyboard) })
</script>

<style scoped>
/* === COPILOT PANEL === */
.copilot-panel {
  display: flex; flex-direction: column; gap: 16px;
  padding: 0 2px 40px; max-width: 100%; overflow-x: hidden;
  outline: none; /* for keyboard focus */
}

/* === HEADER === */
.copilot-header {
  display: flex; justify-content: space-between; align-items: center;
  padding: 10px 16px; border-radius: 16px;
  background: var(--r-card-bg); border: 1px solid var(--r-border);
  box-shadow: 0 2px 10px rgba(0,0,0,0.03);
  flex-wrap: wrap; gap: 10px;
}
.header-left { display: flex; align-items: center; gap: 12px; }
.header-right { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }

.copilot-brand { display: flex; align-items: center; gap: 10px; }
.brand-orb {
  width: 32px; height: 32px; border-radius: 10px;
  background: linear-gradient(135deg, #0ea5e9, #6366f1);
  display: flex; align-items: center; justify-content: center; color: white;
}
.brand-text h3 { font-size: 15px; font-weight: 900; margin: 0; color: var(--r-text-main); }
.brand-sub { font-size: 10px; color: var(--r-text-faint); font-weight: 700; }

.stat-chips { display: flex; gap: 6px; }
.chip {
  display: flex; align-items: center; gap: 5px; padding: 3px 10px;
  background: var(--r-main-bg); border: 1px solid var(--r-border);
  border-radius: 8px; font-size: 10px; font-weight: 800; color: var(--r-text-sub);
}
.chip.success { background: rgba(16,185,129,0.06); color: #10b981; border-color: rgba(16,185,129,0.2); }

/* Live toggle */
.live-control {
  display: flex; align-items: center; gap: 8px; padding: 4px 10px;
  background: var(--r-main-bg); border: 1px solid var(--r-border); border-radius: 10px;
}
.live-control.is-on { border-color: #ef4444; background: rgba(239,68,68,0.04); }
.live-dot { width: 6px; height: 6px; background: #94a3b8; border-radius: 50%; transition: 0.3s; }
.live-control.is-on .live-dot { background: #ef4444; box-shadow: 0 0 8px rgba(239,68,68,0.5); animation: pulse-dot 1.5s infinite; }
.live-label { font-size: 9px; font-weight: 900; color: var(--r-text-faint); letter-spacing: 1px; }
.live-control.is-on .live-label { color: #ef4444; }

@keyframes pulse-dot { 0%,100% { opacity: 1; } 50% { opacity: 0.4; } }

.toggle-sw { position: relative; width: 32px; height: 17px; cursor: pointer; }
.toggle-sw input { opacity: 0; width: 0; height: 0; position: absolute; }
.sw-track {
  position: absolute; inset: 0; background: #cbd5e0; border-radius: 20px; transition: 0.3s;
}
.sw-track::before {
  content: ''; position: absolute; height: 13px; width: 13px; left: 2px; bottom: 2px;
  background: white; border-radius: 50%; transition: 0.3s;
}
input:checked + .sw-track { background: #10b981; }
input:checked + .sw-track::before { transform: translateX(15px); }

.save-btn {
  display: flex; align-items: center; gap: 5px;
  background: linear-gradient(135deg, #4f46e5, #6366f1);
  color: white; border: none; padding: 7px 14px; border-radius: 10px;
  font-size: 12px; font-weight: 800; cursor: pointer; transition: 0.2s;
}
.save-btn:hover { transform: translateY(-1px); box-shadow: 0 6px 16px rgba(99,102,241,0.3); }
.save-btn:disabled { opacity: 0.6; cursor: not-allowed; }

/* === PROGRESS === */
.progress-strip { width: 100%; height: 3px; background: var(--r-border); border-radius: 2px; overflow: hidden; margin-top: -8px; }
.progress-fill { height: 100%; background: linear-gradient(90deg, #10b981, #0ea5e9); transition: width 0.5s ease; border-radius: 2px; }

/* === CANDIDATE QUICK CARD === */
.candidate-quick-card {
  display: flex; align-items: center; gap: 12px;
  padding: 12px 16px; border-radius: 14px;
  background: var(--r-card-bg); border: 1px solid var(--r-border);
}
.cqc-avatar {
  width: 36px; height: 36px; border-radius: 10px;
  background: linear-gradient(135deg, #0ea5e9, #6366f1);
  color: white; display: flex; align-items: center; justify-content: center;
  font-size: 12px; font-weight: 900; flex-shrink: 0;
}
.cqc-info { flex: 1; min-width: 0; }
.cqc-name { display: block; font-size: 14px; font-weight: 800; color: var(--r-text-main); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.cqc-job { display: block; font-size: 11px; color: var(--r-text-sub); font-weight: 600; }
.cqc-score { text-align: center; flex-shrink: 0; }
.score-num { display: block; font-size: 18px; font-weight: 950; color: #0ea5e9; line-height: 1; }
.score-label { font-size: 9px; font-weight: 800; color: var(--r-text-faint); text-transform: uppercase; }

/* === QUESTIONS LIST === */
.questions-list { display: flex; flex-direction: column; gap: 12px; }

.q-card {
  background: var(--r-card-bg); border: 1.5px solid var(--r-border);
  border-radius: 16px; overflow: hidden; cursor: pointer;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
}
.q-card:hover { border-color: rgba(14,165,233,0.3); }
.q-card.is-active { border-color: var(--accent, #6366f1); box-shadow: 0 0 0 3px rgba(99,102,241,0.08); }
.q-card.is-done { border-left: 3px solid #10b981; }

.q-top {
  display: flex; justify-content: space-between; align-items: center;
  padding: 10px 14px; background: var(--r-main-bg); border-bottom: 1px solid var(--r-border);
}
.q-left { display: flex; align-items: center; gap: 8px; }
.q-right { display: flex; align-items: center; gap: 6px; }

.q-num {
  background: var(--accent, #6366f1); color: white; padding: 2px 8px;
  border-radius: 6px; font-size: 11px; font-weight: 900;
  display: flex; align-items: center; justify-content: center; min-width: 28px;
}
.q-num.done { background: #10b981; }
.q-cat { font-size: 10px; font-weight: 900; color: var(--accent, #6366f1); text-transform: uppercase; letter-spacing: 0.5px; }

/* Inline stars */
.inline-stars { display: flex; gap: 2px; }
.star-btn-mini {
  background: none; border: none; padding: 2px; cursor: pointer;
  transition: transform 0.15s;
}
.star-btn-mini:hover { transform: scale(1.25); }

.score-pill {
  font-size: 11px; font-weight: 900; color: #FFD700;
  background: rgba(255,215,0,0.1); padding: 2px 8px; border-radius: 6px;
}

.trash-btn {
  background: none; border: none; color: var(--r-text-faint); cursor: pointer;
  padding: 2px; opacity: 0.5; transition: 0.2s;
}
.trash-btn:hover { opacity: 1; color: #ef4444; }

/* Question body */
.q-body { padding: 12px 14px; }
.q-text { font-size: 14px; font-weight: 700; color: var(--r-text-main); line-height: 1.5; margin: 0; }
.q-edit {
  width: 100%; background: var(--r-main-bg); border: 1.5px solid var(--r-border);
  border-radius: 10px; color: var(--r-text-main); padding: 10px; font-size: 13px;
  font-weight: 600; font-family: inherit; resize: none; outline: none; transition: 0.2s;
}
.q-edit:focus { border-color: var(--accent, #6366f1); box-shadow: 0 0 0 3px rgba(99,102,241,0.08); }

.q-purpose {
  display: flex; align-items: flex-start; gap: 8px; margin-top: 10px;
  padding: 8px 12px; background: rgba(14,165,233,0.04); border-radius: 8px;
  border: 1px solid rgba(14,165,233,0.1);
}
.q-purpose span { font-size: 12px; color: var(--r-text-sub); font-weight: 600; line-height: 1.4; }

/* Eval zone */
.eval-zone { padding: 0 14px 12px; }
.notes-row { display: flex; align-items: flex-start; gap: 8px; color: var(--r-text-faint); }
.notes-input {
  flex: 1; background: var(--r-main-bg); border: 1px solid var(--r-border);
  border-radius: 8px; padding: 8px 10px; font-size: 12px; font-weight: 600;
  color: var(--r-text-main); font-family: inherit; resize: none; outline: none; transition: 0.2s;
}
.notes-input:focus { border-color: var(--accent, #6366f1); }

.q-status-bar { height: 2px; background: transparent; transition: 0.3s; }
.q-status-bar.done { background: #10b981; }

/* Add button */
.add-q-btn {
  display: flex; align-items: center; justify-content: center; gap: 8px;
  background: var(--r-main-bg); border: 1.5px dashed var(--r-border);
  border-radius: 14px; padding: 12px; color: var(--r-text-sub);
  cursor: pointer; font-weight: 800; font-size: 13px; transition: 0.2s;
}
.add-q-btn:hover { border-color: var(--accent, #6366f1); color: var(--accent, #6366f1); }

/* === EMPTY STATE === */
.empty-state {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  min-height: 200px; text-align: center; gap: 12px; color: var(--r-text-sub);
}
.empty-icon { color: var(--accent, #6366f1); opacity: 0.5; }
.empty-state h4 { font-size: 16px; font-weight: 800; margin: 0; color: var(--r-text-main); }
.empty-state p { font-size: 13px; margin: 0; max-width: 280px; }

/* === EVALUATION FOOTER === */
.eval-footer {
  display: flex; justify-content: space-between; align-items: center;
  padding: 12px 16px; border-radius: 14px;
  background: var(--r-card-bg); border: 1px solid var(--r-border);
  position: sticky; bottom: 0;
}
.footer-stats { display: flex; align-items: center; gap: 12px; }
.f-stat { display: flex; align-items: baseline; gap: 4px; }
.f-val { font-size: 18px; font-weight: 950; color: var(--r-text-main); }
.f-label { font-size: 11px; font-weight: 700; color: var(--r-text-sub); }
.f-divider { width: 1px; height: 20px; background: var(--r-border); }

.footer-shortcuts { display: flex; gap: 10px; }
.shortcut-hint { font-size: 10px; color: var(--r-text-faint); font-weight: 700; display: flex; align-items: center; gap: 4px; }
kbd {
  display: inline-block; padding: 1px 5px; border-radius: 4px;
  background: var(--r-main-bg); border: 1px solid var(--r-border);
  font-size: 9px; font-weight: 900; font-family: inherit;
}

/* === TRANSITIONS === */
.expand-enter-active, .expand-leave-active { transition: all 0.25s ease; }
.expand-enter-from, .expand-leave-to { opacity: 0; max-height: 0; overflow: hidden; }
.expand-enter-to, .expand-leave-from { max-height: 200px; }

.scale-enter-active, .scale-leave-active { transition: all 0.2s ease; }
.scale-enter-from, .scale-leave-to { opacity: 0; transform: scale(0.8); }

.spin { animation: rotate 1.5s linear infinite; }
@keyframes rotate { from { transform: rotate(0); } to { transform: rotate(360deg); } }

/* Dark Mode Overrides - Premium Enhancement */
.dark-mode .copilot-header {
  background: #0f172a;
  border-color: var(--accent-soft);
}

.dark-mode .brand-text h3 {
  color: var(--accent);
}

.dark-mode .brand-orb {
  background: var(--accent-grad);
  box-shadow: 0 0 15px var(--accent-glow);
}

.dark-mode .candidate-quick-card {
  background: #0f172a;
  border-color: rgba(255, 255, 255, 0.05);
}

.dark-mode .cqc-name {
  color: #f8fafc;
}

.dark-mode .q-card {
  background: #0f172a;
  border-color: rgba(255, 255, 255, 0.05);
}

.dark-mode .q-card.is-active {
  border-color: var(--accent);
  background: var(--accent-soft);
  box-shadow: 0 0 20px var(--accent-glow);
}

.dark-mode .q-text {
  color: #e2e8f0;
}

.dark-mode .q-cat {
  color: var(--accent);
  text-shadow: 0 0 10px var(--accent-soft);
}

.dark-mode .q-num {
  background: #1e293b;
  color: var(--accent);
  border: 1px solid var(--accent-soft);
}

.dark-mode .q-num.done {
  background: rgba(16, 185, 129, 0.1);
  color: #10b981;
  border-color: rgba(16, 185, 129, 0.2);
}

.dark-mode .score-pill {
  background: rgba(255, 215, 0, 0.1);
  color: #fbbf24;
  box-shadow: 0 0 10px rgba(251, 191, 36, 0.1);
}

.dark-mode .eval-footer {
  background: #0f172a;
  border-color: var(--accent-soft);
}

.dark-mode .f-val {
  color: var(--accent);
}

.dark-mode .q-edit, .dark-mode .notes-input {
  background: rgba(2, 6, 23, 0.4);
  border-color: rgba(255, 255, 255, 0.1);
  color: #f8fafc;
}

.dark-mode .q-edit:focus, .dark-mode .notes-input:focus {
  border-color: var(--accent);
}
</style>
