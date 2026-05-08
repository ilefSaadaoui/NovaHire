<template>
  <div class="sparkline-container" :style="{ height: height + 'px', width: width }">
    <svg :viewBox="`0 0 ${points.length * 10} 40`" preserveAspectRatio="none" class="sparkline-svg">
      <!-- Gradient Fill -->
      <defs>
        <linearGradient :id="gradientId" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" :stop-color="color" stop-opacity="0.4" />
          <stop offset="100%" :stop-color="color" stop-opacity="0" />
        </linearGradient>
      </defs>

      <!-- Area -->
      <path
        :d="areaPath"
        :fill="`url(#${gradientId})`"
        class="sparkline-area"
      />

      <!-- Line -->
      <path
        :d="linePath"
        :stroke="color"
        stroke-width="2.5"
        fill="none"
        stroke-linecap="round"
        stroke-linejoin="round"
        class="sparkline-path"
      />
    </svg>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  points: {
    type: Array,
    required: true
  },
  color: {
    type: String,
    default: '#0ea5e9'
  },
  width: {
    type: String,
    default: '100%'
  },
  height: {
    type: Number,
    default: 40
  }
})

const gradientId = computed(() => `sparkline-grad-${Math.random().toString(36).substr(2, 9)}`)

const normalizedPoints = computed(() => {
  if (!props.points.length) return []
  const min = Math.min(...props.points)
  const max = Math.max(...props.points)
  const range = max - min || 1
  
  // Normalize to 0-30 range (leaving 5px padding top/bottom in a 40px height)
  return props.points.map(p => 35 - ((p - min) / range) * 30)
})

const linePath = computed(() => {
  if (!normalizedPoints.value.length) return ''
  return normalizedPoints.value.reduce((path, p, i) => {
    return path + (i === 0 ? `M 0 ${p}` : ` L ${i * 10} ${p}`)
  }, '')
})

const areaPath = computed(() => {
  if (!normalizedPoints.value.length) return ''
  const line = linePath.value
  const lastX = (normalizedPoints.value.length - 1) * 10
  return `${line} L ${lastX} 40 L 0 40 Z`
})
</script>

<style scoped>
.sparkline-container {
  display: flex;
  align-items: center;
  overflow: hidden;
}

.sparkline-svg {
  width: 100%;
  height: 100%;
}

.sparkline-path {
  stroke-dasharray: 1000;
  stroke-dashoffset: 1000;
  animation: drawLine 2s ease-out forwards;
}

.sparkline-area {
  opacity: 0;
  animation: fadeIn 1s ease-out 1s forwards;
}

@keyframes drawLine {
  to { stroke-dashoffset: 0; }
}

@keyframes fadeIn {
  to { opacity: 1; }
}
</style>
