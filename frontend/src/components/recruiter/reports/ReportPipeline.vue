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
          <span class="p-name">{{ formatStepName(step.name) }}</span>
          <span class="p-count">{{ step.count }}</span>
        </div>
        <div class="p-bar-track">
          <div class="p-bar-fill" :style="{ width: calculateWidth(step.count), background: getStepColor(step.name), '--delay': (idx * 0.05) + 's' }">
            <div class="p-shine"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { TrendingUp } from 'lucide-vue-next'

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
    'Rejected': 'Résilié',
    'Accepted': 'Intégration'
  }
  return map[name] || name
}

const getStepColor = (name) => {
  const map = {
    'Submitted': 'var(--r-text-sub)',
    'UnderReview': '#f59e0b',
    'Shortlisted': '#0ea5e9',
    'Interview': 'var(--accent)',
    'Interviewed': '#a855f7',
    'Rejected': '#ef4444',
    'Accepted': '#10b981'
  }
  return map[name] || 'var(--r-text-sub)'
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
  border-radius: 32px;
  padding: 32px;
  box-shadow: var(--premium-shadow);
}

.header-with-icon { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; }
.header-icon { color: var(--accent); }
.column-header h3 { font-size: 20px; font-weight: 900; margin: 0; color: var(--text-main); }
.column-header p { font-size: 14px; color: var(--text-muted); margin: 0 0 30px 0; font-weight: 500; }

.pipeline-visual-v2 { display: flex; flex-direction: column; gap: 20px; }
.p-row { display: flex; flex-direction: column; gap: 10px; }
.p-info { display: flex; justify-content: space-between; align-items: center; }
.p-name { font-size: 13px; font-weight: 700; color: var(--text-muted); }
.p-count { font-size: 15px; font-weight: 900; color: var(--text-main); }

.p-bar-track { height: 14px; background: var(--border-thin); border-radius: 7px; overflow: visible; position: relative; }
.p-bar-fill {
  height: 100%; border-radius: 7px; position: relative;
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
