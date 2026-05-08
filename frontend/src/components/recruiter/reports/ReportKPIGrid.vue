<template>
  <div class="kpi-grid">
    <div 
      v-for="(kpi, index) in kpis" 
      :key="index" 
      class="kpi-card-premium anim-reveal-up"
      :style="{ animationDelay: `${index * 0.1}s` }"
    >
      <div class="card-glow"></div>
      <div class="card-content">
        <div class="kpi-icon-wrapper" :style="{ color: kpi.color || 'var(--accent-color)' }">
          <component :is="getIcon(kpi.icon)" class="kpi-svg" />
          <div class="icon-pulse" :style="{ background: kpi.color || 'var(--accent-color)' }"></div>
        </div>
        
        <div class="kpi-info">
          <span class="kpi-label">{{ kpi.label }}</span>
          <div class="kpi-value-row">
            <span class="kpi-value">{{ formatValue(kpi.value, kpi.type) }}</span>
            <div v-if="kpi.trend" class="kpi-trend" :class="kpi.trend >= 0 ? 'up' : 'down'">
              {{ kpi.trend >= 0 ? '+' : '' }}{{ kpi.trend }}%
            </div>
          </div>
        </div>
      </div>
      
      <div class="card-progress">
        <div class="progress-bar" :style="{ width: '100%', background: `linear-gradient(90deg, ${kpi.color || 'var(--accent-color)'} 0%, transparent 100%)`, opacity: 0.2 }"></div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { 
  UsersIcon, 
  BriefcaseIcon, 
  CheckCircleIcon, 
  ClockIcon, 
  TrendingUpIcon,
  StarIcon
} from 'lucide-vue-next'

defineProps({
  kpis: { type: Array, default: () => [] }
})

const getIcon = (iconName) => {
  const icons = {
    'users': UsersIcon,
    'briefcase': BriefcaseIcon,
    'check': CheckCircleIcon,
    'clock': ClockIcon,
    'trend': TrendingUpIcon,
    'star': StarIcon
  }
  return icons[iconName?.toLowerCase()] || UsersIcon
}

const formatValue = (val, type) => {
  if (type === 'percentage') return `${val}%`
  if (val >= 1000) return (val / 1000).toFixed(1) + 'k'
  return val
}
</script>

<style scoped>
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  width: 100%;
}

.kpi-card-premium {
  position: relative;
  background: var(--glass-bg);
  backdrop-filter: blur(20px);
  border: 1px solid var(--glass-border-color);
  border-radius: 12px;
  padding: 12px;
  overflow: hidden;
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  display: flex;
  flex-direction: column;
}

.card-glow {
  position: absolute;
  top: -50%;
  left: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(circle at center, var(--accent-soft) 0%, transparent 60%);
  opacity: 0;
  transition: opacity 0.4s ease;
  pointer-events: none;
}

.kpi-card-premium:hover {
  transform: translateY(-8px) scale(1.02);
  border-color: var(--accent-color);
  box-shadow: 0 20px 40px rgba(0,0,0,0.4), 0 0 20px var(--accent-soft);
}

.kpi-card-premium:hover .card-glow { opacity: 0.5; }

.card-content {
  display: flex;
  align-items: center;
  gap: 20px;
  position: relative;
  z-index: 2;
}

.kpi-icon-wrapper {
  position: relative;
  width: 56px;
  height: 56px;
  background: rgba(255,255,255,0.03);
  border: 1px solid rgba(255,255,255,0.08);
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.kpi-svg { width: 24px; height: 24px; position: relative; z-index: 3; }

.icon-pulse {
  position: absolute;
  inset: 0;
  border-radius: inherit;
  background: currentColor;
  opacity: 0;
  transition: opacity 0.3s ease;
}

.kpi-card-premium:hover .icon-pulse {
  opacity: 0.1;
  animation: pulse-ring 2s infinite;
}

@keyframes pulse-ring {
  0% { transform: scale(0.8); opacity: 0.5; }
  100% { transform: scale(1.5); opacity: 0; }
}

.kpi-info { flex: 1; }
.kpi-label { font-size: 13px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.5px; }

.kpi-value-row { display: flex; align-items: baseline; gap: 12px; margin-top: 4px; }
.kpi-value { font-size: 18px; font-weight: 900; color: var(--text-main); letter-spacing: -0.5px; }

.kpi-trend {
  font-size: 11px;
  font-weight: 800;
  padding: 2px 8px;
  border-radius: 100px;
}
.kpi-trend.up { background: rgba(16, 185, 129, 0.1); color: #10b981; }
.kpi-trend.down { background: rgba(239, 68, 68, 0.1); color: #ef4444; }

.card-progress {
  margin-top: 20px;
  height: 3px;
  width: 100%;
  background: rgba(255,255,255,0.05);
  border-radius: 10px;
  overflow: hidden;
}

.progress-bar { height: 100%; transition: width 1s ease; }

.anim-reveal-up { animation: revealUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes revealUp { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
</style>
