<template>
  <div class="r-card ia-insight-card">
    <div class="ia-card-head">
      <div class="icon-box-themed" style="width: 44px; height: 44px; border-radius: 12px; display: flex; align-items: center; justify-content: center; background: var(--accent-grad); color: white;">
        <Sparkles :size="24" stroke-width="2.5" />
      </div>
      <div class="ia-head-text">
        <h4>Diagnostic IA</h4>
        <p v-if="isAiAnalyzed">Matching basé sur {{ criteria.length }} points</p>
        <p v-else>Analyse en attente</p>
      </div>
    </div>

    <!-- PENDING ANALYSIS STATE -->
    <template v-if="!isAiAnalyzed">
      <div class="ia-score-viz-large">
        <div class="score-ring-large pending-ring">
          <svg viewBox="0 0 100 100">
            <circle cx="50" cy="50" r="45" fill="none" stroke="var(--r-border)" stroke-width="8" stroke-dasharray="8 6" opacity="0.5"/>
          </svg>
          <div class="score-center">
            <div class="pending-icon-pulse">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width: 28px; color: var(--r-text-sub);">
                <circle cx="12" cy="12" r="10"/>
                <polyline points="12 6 12 12 16 14"/>
              </svg>
            </div>
            <span class="s-lab" style="margin-top: 6px;">En attente</span>
          </div>
        </div>
      </div>
      <div class="ia-pending-notice">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width: 18px; flex-shrink: 0; color: var(--accent-color);">
          <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/>
        </svg>
        <p>Ce profil n'a pas encore été analysé par l'IA. Le diagnostic sera disponible après le traitement automatique du CV.</p>
      </div>
    </template>

    <!-- ANALYZED STATE -->
    <template v-else>
      <div class="ia-score-viz-large">
        <div class="score-ring-large">
          <svg viewBox="0 0 100 100">
            <circle cx="50" cy="50" r="45" fill="none" stroke="var(--r-border)" stroke-width="8"/>
            <circle cx="50" cy="50" r="45" fill="none" stroke="var(--accent)" stroke-width="8"
              stroke-dasharray="283" :stroke-dashoffset="283 - (283 * candidate.overallScore / 100)"
              stroke-linecap="round" transform="rotate(-90 50 50)"/>
          </svg>
          <div class="score-center">
            <span class="s-val">{{ candidate.overallScore }}%</span>
            <span class="s-lab">Matching</span>
          </div>
        </div>
      </div>

      <div class="ia-criteria-stack">
        <div v-for="c in criteria" :key="c.name" class="ia-crit-row">
          <div class="crit-head">
            <span class="crit-name">{{ c.name }}</span>
            <span class="crit-score">{{ c.score }}%</span>
          </div>
          <div class="crit-bar">
            <div class="crit-fill" :style="{ width: c.score + '%', background: 'var(--accent)' }"></div>
          </div>
        </div>
      </div>

      <div class="ia-verdict-box">
        <div class="verdict-header">
           <h5>Verdict IA</h5>
           <span v-if="candidate.aiRecommendation" class="rec-badge" :class="recommendationClass">
              {{ candidate.aiRecommendation }}
           </span>
        </div>
        <p>{{ candidate.aiSummary }}</p>
      </div>

      <!-- STRENGTHS & WEAKNESSES -->
      <div class="ia-pro-cons" v-if="(candidate.strengths && candidate.strengths.length) || (candidate.weaknesses && candidate.weaknesses.length)">
        <div v-if="candidate.strengths && candidate.strengths.length" class="pc-group strengths">
          <h6><CheckCircle :size="14" /> Points Forts</h6>
          <ul>
            <li v-for="s in candidate.strengths" :key="s">{{ s }}</li>
          </ul>
        </div>
        <div v-if="candidate.weaknesses && candidate.weaknesses.length" class="pc-group weaknesses">
          <h6><AlertCircle :size="14" /> Axes d'amélioration</h6>
          <ul>
            <li v-for="w in candidate.weaknesses" :key="w">{{ w }}</li>
          </ul>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { Sparkles, CheckCircle, AlertCircle } from 'lucide-vue-next'

const props = defineProps({
  candidate: Object,
  criteria: Array
})

const recommendationClass = computed(() => {
   const rec = props.candidate.aiRecommendation?.toLowerCase() || ''
   if (rec.includes('hautement') || rec.includes('fortement') || rec === 'recommandé') return 'high'
   if (rec.includes('considérer')) return 'mid'
   return 'low'
})

const isAiAnalyzed = computed(() => {
  // The backend may return default/placeholder criteria (all 0%) and
  // an aiSummary like "Analyse en attente..." before actual AI analysis.
  const summary = (props.candidate.aiSummary || '').trim().toLowerCase()
  const isPendingSummary = !summary || summary.includes('attente') || summary.includes('pending')
  
  // Check if criteria exist AND at least one has a non-zero score
  const hasMeaningfulCriteria = props.criteria && props.criteria.length > 0 
    && props.criteria.some(c => c.score > 0)
  
  // Score > 0 is a definitive sign of analysis
  const hasNonZeroScore = props.candidate.overallScore > 0
  
  return hasNonZeroScore || hasMeaningfulCriteria || !isPendingSummary
})
</script>

<style scoped>
.r-card {
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  border-radius: 20px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.05);
}

.ia-insight-card { 
  padding: 32px; 
  background: var(--card-bg);
  border: 1px solid var(--r-border);
  border-radius: 24px;
  backdrop-filter: blur(10px);
}
.ia-card-head { display: flex; gap: 16px; align-items: center; margin-bottom: 32px; }

.ia-head-text h4 { font-size: 16px; font-weight: 800; color: var(--r-text-main); margin: 0; }
.ia-head-text p { font-size: 12px; color: var(--r-text-sub); margin: 4px 0 0 0; }

.ia-score-viz-large { display: flex; justify-content: center; margin-bottom: 32px; }
.score-ring-large { position: relative; width: 140px; height: 140px; }
.score-center { position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); text-align: center; }
.s-val { display: block; font-size: 28px; font-weight: 800; color: var(--r-text-main); }
.s-lab { font-size: 10px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; }

.ia-criteria-stack { display: flex; flex-direction: column; gap: 16px; margin-bottom: 32px; }
.ia-crit-row { display: flex; flex-direction: column; gap: 6px; }
.crit-head { display: flex; justify-content: space-between; }
.crit-name { font-size: 12px; font-weight: 600; color: var(--r-text-sub); }
.crit-score { font-size: 12px; font-weight: 800; color: var(--r-text-main); }
.crit-bar { height: 6px; background: var(--r-border); border-radius: 100px; overflow: hidden; }
.crit-fill { height: 100%; border-radius: 100px; transition: width 1s ease; }

.ia-verdict-box {
  background: var(--accent-soft); 
  border-radius: 20px; 
  padding: 24px;
  border: 1px solid var(--accent-color);
  box-shadow: 0 8px 25px -10px var(--accent-soft);
  margin-bottom: 24px;
}
.verdict-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 12px; }
.ia-verdict-box h5 { color: var(--accent); font-size: 13px; font-weight: 800; margin: 0; }
.ia-verdict-box p { font-size: 13px; color: var(--r-text-main); line-height: 1.6; margin: 0; }

.rec-badge {
  font-size: 10px; font-weight: 900; padding: 4px 10px; border-radius: 100px; text-transform: uppercase;
}
.rec-badge.high { background: #10b981; color: white; }
.rec-badge.mid { background: #f59e0b; color: white; }
.rec-badge.low { background: #ef4444; color: white; }

.ia-pro-cons { display: flex; flex-direction: column; gap: 20px; }
.pc-group h6 { 
  display: flex; align-items: center; gap: 8px; font-size: 13px; font-weight: 800; margin: 0 0 10px 0;
}
.strengths h6 { color: #10b981; }
.weaknesses h6 { color: #f59e0b; }

.pc-group ul { list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 6px; }
.pc-group li { 
  font-size: 12px; color: var(--r-text-main); padding-left: 14px; position: relative; line-height: 1.4;
}
.pc-group li::before {
  content: ''; position: absolute; left: 0; top: 6px; width: 4px; height: 4px; border-radius: 50%;
}
.strengths li::before { background: #10b981; }
.weaknesses li::before { background: #f59e0b; }

/* PENDING AI ANALYSIS STATE */
.pending-ring svg { animation: pendingRotate 12s linear infinite; }
@keyframes pendingRotate { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

.pending-icon-pulse {
  animation: pendingPulse 2s ease-in-out infinite;
}
@keyframes pendingPulse {
  0%, 100% { opacity: 0.5; transform: scale(0.95); }
  50% { opacity: 1; transform: scale(1.05); }
}

.ia-pending-notice {
  display: flex; align-items: flex-start; gap: 12px;
  padding: 18px 20px; border-radius: 16px;
  background: var(--r-main-bg); border: 1px solid var(--r-border);
  border-left-width: 4px; border-left-color: var(--accent-color);
}
.ia-pending-notice p {
  font-size: 13px; color: var(--r-text-sub); line-height: 1.6; margin: 0;
  font-weight: 500;
}
/* Dark Mode Overrides - Premium Enhancement */
.dark-mode .ia-insight-card {
  background: #0f172a;
  border-color: var(--accent-soft);
}

.dark-mode .ia-head-text h4 {
  color: var(--accent);
}

.dark-mode .s-val {
  color: #ffffff;
}

.dark-mode .crit-score {
  color: #f8fafc;
}

.dark-mode .ia-verdict-box {
  background: var(--accent-soft);
  border-color: var(--accent-glow);
}

.dark-mode .ia-verdict-box h5 {
  color: var(--accent);
}

.dark-mode .ia-verdict-box p {
  color: #e2e8f0;
}

.dark-mode .strengths h6 {
  color: #34d399;
}

.dark-mode .weaknesses h6 {
  color: #fbbf24;
}

.dark-mode .pc-group li {
  color: #cbd5e1;
}
</style>
