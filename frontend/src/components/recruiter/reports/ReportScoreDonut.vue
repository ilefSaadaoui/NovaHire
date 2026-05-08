<template>
  <div class="glass-column anim-reveal-right" style="--delay: 0.1s">
    <div class="column-header">
      <div class="header-with-icon">
        <Sparkles :size="24" class="header-icon" />
        <h3>Potentiel des Talents</h3>
      </div>
      <p>Évaluation objective de l'adéquation profil-poste.</p>
    </div>

      <div class="ai-score-donut">
        <div class="donut-wrapper">
          <svg viewBox="0 0 100 100">
            <circle cx="50" cy="50" r="45" class="d-bg" />
            <circle v-for="(range, i) in scoreRanges" :key="i"
                    cx="50" cy="50" r="45"
                    class="d-segment"
                    :stroke="getRangeColor(range.class)"
                    :stroke-dasharray="getDashArray(range.count)"
                    :stroke-dashoffset="getDashOffset(i)" />
          </svg>
          <div class="donut-center">
            <span class="d-val">{{ totalAnalyses }}</span>
            <span class="d-tag">Analyses</span>
          </div>
        </div>
        
        <div class="range-legend">
          <div v-for="range in scoreRanges" :key="range.label" class="range-item">
            <div class="r-indicator">
              <div class="r-dot" :style="{ background: getRangeColor(range.class), boxShadow: `0 0 8px ${getRangeColor(range.class)}` }"></div>
              <div class="r-line" :style="{ background: getRangeColor(range.class) }"></div>
            </div>
            <div class="r-info">
              <span class="r-label">{{ range.label }}</span>
              <span class="r-count">{{ range.count }} <small>pers.</small></span>
            </div>
          </div>
        </div>
      </div>
  </div>
</template>

<script setup>
import { Sparkles } from 'lucide-vue-next'

const props = defineProps({
  scoreRanges: Array,
  totalAnalyses: Number
})

const getRangeColor = (cls) => {
  if (cls === 'high') return '#10b981'
  if (cls === 'mid') return '#f59e0b'
  return '#ef4444'
}

const getDashArray = (count) => {
  const total = props.totalAnalyses || 1
  const circumference = 2 * Math.PI * 45
  const pct = (count / total)
  return `${pct * circumference} ${circumference}`
}

const getDashOffset = (index) => {
  const circumference = 2 * Math.PI * 45
  let offset = 0
  for (let i = 0; i < index; i++) {
    const count = props.scoreRanges[i].count
    offset += (count / (props.totalAnalyses || 1))
  }
  return -(offset * circumference)
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

.ai-score-donut { display: flex; flex-direction: column; align-items: center; gap: 20px; }
.donut-wrapper { position: relative; width: 120px; height: 120px; }
.d-bg { fill: none; stroke: var(--border-thin); stroke-width: 6; }
.d-segment { fill: none; stroke-width: 8; stroke-linecap: round; transition: all 1s ease; transform: rotate(-90deg); transform-origin: center; }

.donut-center {
  position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);
  text-align: center;
}
.d-val { display: block; font-size: 24px; font-weight: 900; color: var(--text-main); line-height: 1; }
.d-tag { font-size: 11px; font-weight: 800; color: var(--text-muted); text-transform: uppercase; margin-top: 5px; display: block; }

.range-legend { width: 100%; display: flex; flex-direction: column; gap: 8px; }
.range-item { display: flex; align-items: center; gap: 12px; padding: 8px 12px; background: rgba(255,255,255,0.02); border-radius: 12px; transition: all 0.3s ease; border: 1px solid transparent; }
.range-item:hover { background: rgba(255,255,255,0.05); transform: translateX(4px); border-color: var(--border-thin); }

.r-indicator { display: flex; flex-direction: column; align-items: center; gap: 2px; }
.r-dot { width: 6px; height: 6px; border-radius: 50%; }
.r-line { width: 2px; height: 12px; border-radius: 1px; opacity: 0.3; }

.r-info { flex: 1; display: flex; justify-content: space-between; align-items: center; }
.r-label { font-size: 11px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.3px; }
.r-count { font-size: 13px; font-weight: 900; color: var(--text-main); }
.r-count small { font-size: 9px; opacity: 0.5; font-weight: 500; }

.anim-reveal-right { animation: revealRight 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes revealRight { from { opacity: 0; transform: translateX(30px); } to { opacity: 1; transform: translateX(0); } }
</style>
