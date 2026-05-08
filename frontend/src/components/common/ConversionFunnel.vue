<template>
  <div class="funnel-container">
    <div v-for="(stage, index) in stages" :key="index" class="funnel-stage" :style="{ width: getWidth(index) + '%' }">
      <div class="stage-content" :style="{ background: stage.color }">
        <span class="stage-label">{{ stage.label }}</span>
        <span class="stage-value">{{ stage.value }}</span>
      </div>
      <div v-if="index < stages.length - 1" class="stage-connector">
        <div class="connector-line"></div>
        <div class="drop-rate">{{ getDropRate(index) }}% drop</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  data: {
    type: Object,
    default: () => ({
      applications: 450,
      interviews: 120,
      offers: 45,
      hired: 18
    })
  }
})

const stages = computed(() => [
  { label: 'Candidatures', value: props.data.applications, color: 'rgba(14, 165, 233, 0.8)' },
  { label: 'Entretiens', value: props.data.interviews, color: 'rgba(99, 102, 241, 0.8)' },
  { label: 'Offres', value: props.data.offers, color: 'rgba(139, 92, 246, 0.8)' },
  { label: 'Recrutés', value: props.data.hired, color: 'rgba(16, 185, 129, 0.8)' }
])

const getWidth = (index) => {
  const max = 100
  const min = 40
  return max - (index * (max - min) / (stages.value.length - 1))
}

const getDropRate = (index) => {
  const current = stages.value[index].value
  const next = stages.value[index + 1].value
  return Math.round(((current - next) / current) * 100)
}
</script>

<style scoped>
.funnel-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
  padding: 10px 0;
}

.funnel-stage {
  position: relative;
  transition: all 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.stage-content {
  height: 44px;
  border-radius: 12px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0 20px;
  color: white;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(5px);
}

.stage-label { font-size: 11px; font-weight: 800; text-transform: uppercase; letter-spacing: 0.5px; opacity: 0.9; }
.stage-value { font-size: 16px; font-weight: 900; }

.stage-connector {
  height: 24px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  position: relative;
}

.connector-line {
  width: 2px;
  height: 100%;
  background: linear-gradient(to bottom, rgba(255, 255, 255, 0.2), transparent);
}

.drop-rate {
  position: absolute;
  right: 10px;
  font-size: 9px;
  font-weight: 800;
  color: #ef4444;
  background: rgba(239, 68, 68, 0.1);
  padding: 2px 6px;
  border-radius: 99px;
}
</style>
