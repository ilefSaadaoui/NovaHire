<template>
  <div class="chart-container-premium">
    <Line :data="chartData" :options="chartOptions" />
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { Line } from 'vue-chartjs'
import { 
  Chart as ChartJS, 
  Title, Tooltip, Legend, LineElement, 
  PointElement, CategoryScale, LinearScale, Filler 
} from 'chart.js'

ChartJS.register(
  Title, Tooltip, Legend, LineElement, 
  PointElement, CategoryScale, LinearScale, Filler
)

const props = defineProps({
  data: {
    type: Array,
    default: () => [30, 45, 35, 55, 48, 65, 58, 85, 75, 95, 88, 110]
  },
  labels: {
    type: Array,
    default: () => ['Jan', 'Fév', 'Mar', 'Avr', 'Mai', 'Juin', 'Juil', 'Août', 'Sept', 'Oct', 'Nov', 'Déc']
  },
  label: {
    type: String,
    default: 'Candidatures'
  },
  color: {
    type: String,
    default: '#0ea5e9'
  }
})

const chartData = computed(() => ({
  labels: props.labels,
  datasets: [
    {
      label: props.label,
      data: props.data,
      borderColor: props.color,
      borderWidth: 3,
      pointBackgroundColor: props.color,
      pointBorderColor: 'rgba(255, 255, 255, 0.8)',
      pointHoverRadius: 6,
      pointRadius: 4,
      tension: 0.4,
      fill: true,
      backgroundColor: (context) => {
        const ctx = context.chart.ctx;
        const gradient = ctx.createLinearGradient(0, 0, 0, 300);
        gradient.addColorStop(0, `${props.color}44`); // 44 is hex alpha for ~25%
        gradient.addColorStop(1, `${props.color}00`);
        return gradient;
      }
    }
  ]
}))

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false
    },
    tooltip: {
      mode: 'index',
      intersect: false,
      backgroundColor: 'rgba(15, 23, 42, 0.9)',
      titleColor: '#fff',
      bodyColor: '#cbd5e1',
      padding: 12,
      borderRadius: 12,
      displayColors: false,
      callbacks: {
        label: (context) => ` ${context.parsed.y} candidatures`
      }
    }
  },
  scales: {
    y: {
      grid: {
        display: true,
        color: 'rgba(255, 255, 255, 0.05)',
        drawBorder: false
      },
      ticks: {
        color: '#94a3b8',
        font: { size: 11, weight: '600' },
        padding: 8
      }
    },
    x: {
      grid: {
        display: false
      },
      ticks: {
        color: '#94a3b8',
        font: { size: 11, weight: '600' },
        padding: 8
      }
    }
  }
}))
</script>

<style scoped>
.chart-container-premium {
  width: 100%;
  height: 300px;
  padding: 10px;
}
</style>
