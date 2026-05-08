<template>
  <div class="glass-column anim-reveal-left">
    <div class="column-header">
      <div class="header-with-icon">
        <TrendingUp :size="24" class="header-icon" />
        <h3>Dynamique du Vivier</h3>
      </div>
      <p>Suivi en temps réel de l'évolution de vos candidats.</p>
    </div>
    
    <div class="pipeline-visual-v2">
      <div v-for="(step, idx) in pipelineData" :key="step.name" class="p-row">
        <div class="p-info">
          <div class="p-name-with-icon">
            <component :is="getStepIcon(step.name)" :size="14" class="step-mini-icon" :style="{ color: getStepColor(step.name) }" />
            <span class="p-name">{{ formatStepName(step.name) }}</span>
          </div>
          <span class="p-count">{{ step.count }}</span>
        </div>
        <div class="p-bar-track">
          <div class="p-bar-fill" :style="{ 
            width: calculateWidth(step.count), 
            background: `linear-gradient(90deg, ${getStepColor(step.name)} 0%, ${getStepColor(step.name)}88 100%)`, 
            '--delay': (idx * 0.05) + 's',
            boxShadow: `0 0 10px ${getStepColor(step.name)}44`
          }">
            <div class="p-shine"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { 
  TrendingUp, 
  UserPlus, 
  Search, 
  UserCheck, 
  Mic2, 
  XCircle, 
  CheckCircle2, 
  ClipboardCheck,
  Send
} from 'lucide-vue-next'

const props = defineProps({
  pipelineData: Array
})

const formatStepName = (name) => {
  const map = {
    'Submitted': 'Nouv. Candidats',
    'UnderReview': 'En Analyse',
    'Shortlisted': 'Shortlisté',
    'Interview': 'Entretien Hub',
    'Interviewed': 'Post-Entretien',
    'Rejected': 'Refusé',
    'Accepted': 'Recruté',
    'OfferSent': 'Offre Envoyée'
  }
  return map[name] || name
}

const getStepIcon = (name) => {
  const map = {
    'Submitted': UserPlus,
    'UnderReview': Search,
    'Shortlisted': UserCheck,
    'Interview': Mic2,
    'Interviewed': ClipboardCheck,
    'Rejected': XCircle,
    'Accepted': CheckCircle2,
    'OfferSent': Send
  }
  return map[name] || UserPlus
}

const getStepColor = (name) => {
  const map = {
    'Submitted': '#94a3b8',
    'UnderReview': '#f59e0b',
    'Shortlisted': '#0ea5e9',
    'Interview': '#6366f1',
    'Interviewed': '#a855f7',
    'Rejected': '#ef4444',
    'Accepted': '#10b981',
    'OfferSent': '#ec4899'
  }
  return map[name] || '#94a3b8'
}

const calculateWidth = (count) => {
  if (!props.pipelineData || !props.pipelineData.length) return '0%'
  const max = Math.max(...props.pipelineData.map(p => p.count), 1)
  return `${(count / max) * 100}%`
}
</script>

<style scoped>
.glass-column {
  background: var(--glass-bg);
  backdrop-filter: blur(25px) saturate(200%);
  border: 1px solid var(--glass-border-color);
  border-radius: 16px;
  padding: 16px;
  box-shadow: var(--premium-shadow);
}

.header-with-icon { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; }
.header-icon { color: var(--accent); }
.column-header h3 { font-size: 20px; font-weight: 900; margin: 0; color: var(--text-main); }
.column-header p { font-size: 14px; color: var(--text-muted); margin: 0 0 30px 0; font-weight: 500; }

.pipeline-visual-v2 { display: flex; flex-direction: column; gap: 20px; }
.p-row { display: flex; flex-direction: column; gap: 6px; }
.p-info { display: flex; justify-content: space-between; align-items: center; }
.p-name-with-icon { display: flex; align-items: center; gap: 8px; }
.step-mini-icon { opacity: 0.8; }
.p-name { font-size: 12px; font-weight: 700; color: var(--text-muted); letter-spacing: 0.2px; }
.p-count { font-size: 14px; font-weight: 900; color: var(--text-main); }

.p-bar-track { height: 6px; background: var(--border-thin); border-radius: 3px; overflow: visible; position: relative; }
.p-bar-fill {
  height: 100%; border-radius: 3px; position: relative;
  transition: width 1.2s cubic-bezier(0.16, 1, 0.3, 1);
  animation: grow 1s ease forwards;
  animation-delay: var(--delay);
}

.p-shine {
  position: absolute; top: 0; left: 0; right: 0; bottom: 0;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.1), transparent);
  animation: moveShine 3s infinite linear;
}

@keyframes grow { from { width: 0; } }
@keyframes moveShine { 0% { transform: translateX(-100%); } 100% { transform: translateX(100%); } }

.anim-reveal-left { animation: revealLeft 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes revealLeft { from { opacity: 0; transform: translateX(-30px); } to { opacity: 1; transform: translateX(0); } }
</style>
