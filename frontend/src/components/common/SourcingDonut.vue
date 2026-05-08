<template>
  <div class="donut-container">
    <Doughnut :data="chartData" :options="chartOptions" />
    <div class="donut-center">
      <span class="total-label">Top Source</span>
      <span class="top-source-val">{{ topSource }}</span>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { Doughnut } from 'vue-chartjs'
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from 'chart.js'

ChartJS.register(ArcElement, Tooltip, Legend)

const props = defineProps({
  data: {
    type: Object,
    default: () => ({
      'LinkedIn': 45,
      'Indeed': 25,
      'Direct': 15,
      'Referral': 10,
      'Others': 5
    })
  }
})

const chartData = computed(() => ({
  labels: Object.keys(props.data),
  datasets: [
    {
      data: Object.values(props.data),
      backgroundColor: [
        '#00A7E1', // LinkedIn Blue
        '#6C63FF', // Indeed/Violet
        '#10b981', // Direct/Emerald
        '#f59e0b', // Referral/Gold
        '#64748b'  // Others/Slate
      ],
      borderWidth: 0,
      hoverOffset: 10,
      cutout: '75%'
    }
  ]
}))

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'right',
      labels: {
        color: '#94a3b8',
        usePointStyle: true,
        pointStyle: 'circle',
        font: { size: 10, weight: '700' },
        padding: 15
      }
    },
    tooltip: {
      backgroundColor: 'rgba(15, 23, 42, 0.9)',
      padding: 12,
      borderRadius: 12,
      titleFont: { size: 12, weight: 'bold' }
    }
  }
}

const topSource = computed(() => {
  const sources = Object.entries(props.data)
  return sources.sort((a, b) => b[1] - a[1])[0][0]
})
</script>

<style scoped>
.donut-container {
  width: 100%;
  height: 200px;
  position: relative;
  display: flex;
  justify-content: center;
  align-items: center;
}

.donut-center {
  position: absolute;
  top: 50%;
  left: 36%; /* Adjusted because legend is on the right */
  transform: translate(-50%, -50%);
  display: flex;
  flex-direction: column;
  align-items: center;
  pointer-events: none;
}

.total-label { font-size: 10px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; }
.top-source-val { font-size: 14px; font-weight: 900; color: var(--r-text-main); }
</style>
