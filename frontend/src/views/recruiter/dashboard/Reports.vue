<template>
  <div class="dashboard-layout recruiter-layout">
    <Sidebar active-item="reports" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      <!-- CELESTIAL BACKGROUND -->
      <div class="nebula-background">
        <div class="nebula-spot blue" v-if="isAdmin"></div>
        <div class="nebula-spot gold" v-else></div>
        <div class="glass-overlay"></div>
      </div>

      <div class="content-wrapper scroll-container-premium">
        <!-- TOP BAR -->
        <ReportHeader 
          v-model:selectedOfferId="selectedOfferId"
          :offerOptions="offerOptions"
          :isExporting="isExporting"
          @export="handleExport"
        />

        <!-- LOADING & EMPTY STATES -->
        <ReportEmptyState 
          v-if="loading || !selectedOfferId"
          :loading="loading"
          :selectedOfferId="selectedOfferId"
        />

        <!-- MAIN DASHBOARD -->
        <div v-else class="dashboard-render anim-reveal-up">
          <!-- KPI GRID -->
          <ReportKPIGrid :kpis="kpis" />

          <div class="dashboard-main-columns">
            <!-- PIPELINE VISUAL -->
            <ReportPipeline :pipelineData="pipelineData" />

            <div class="side-analytics">
              <!-- AI SCORING DONUT -->
              <ReportScoreDonut 
                :scoreRanges="scoreRanges" 
                :totalAnalyses="totalAnalyses" 
              />

              <!-- TOP OFFERS -->
              <ReportTopOffers 
                :topOffers="stats.topOffers" 
              />
            </div>
          </div>
        </div>
      </div>
      <ToastContainer />
    </main>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from 'vue'
import Sidebar from '@/components/layout/Sidebar.vue'
import ToastContainer from '@/components/common/ToastContainer.vue'

// Report Components
import ReportHeader from '@/components/recruiter/reports/ReportHeader.vue'
import ReportKPIGrid from '@/components/recruiter/reports/ReportKPIGrid.vue'
import ReportEmptyState from '@/components/recruiter/reports/ReportEmptyState.vue'
import ReportPipeline from '@/components/recruiter/reports/ReportPipeline.vue'
import ReportScoreDonut from '@/components/recruiter/reports/ReportScoreDonut.vue'
import ReportTopOffers from '@/components/recruiter/reports/ReportTopOffers.vue'

import { useAuthStore } from '@/stores/authStore'
import { useToastStore } from '@/stores/toastStore'
import axios from '@/api/axios'

const authStore = useAuthStore()
const toastStore = useToastStore()

const isAdmin = computed(() => authStore.isAdmin)

const sidebarCollapsed = ref(false)
const loading = ref(false)
const isExporting = ref(false)
const selectedOfferId = ref('')
const offers = ref([])
const stats = ref({
  kpis: [],
  pipelineData: [],
  scoreRanges: [],
  topSkills: [],
  topOffers: []
})

const offerOptions = computed(() => {
  return offers.value.map(o => ({ label: o.title, value: o.id }))
})

const kpis = computed(() => stats.value.kpis || [])
const pipelineData = computed(() => stats.value.pipelineData || [])
const scoreRanges = computed(() => stats.value.scoreRanges || [])
const totalAnalyses = computed(() => scoreRanges.value.reduce((acc, curr) => acc + curr.count, 0))

const fetchOffers = async () => {
  try {
    const res = await axios.get('/joboffer')
    offers.value = (res.data.data || []).filter(o => o.status === 1 || o.status === 'Published')
  } catch (err) {
    console.error('Fetch offers error:', err)
  }
}

const fetchAnalytics = async () => {
  if (!selectedOfferId.value) return
  loading.value = true
  try {
    const res = await axios.get(`/recruiter/analytics?jobOfferId=${selectedOfferId.value}`)
    stats.value = res.data
  } catch (err) {
    console.error('Fetch analytics error:', err)
    toastStore.show("Échec de la synchronisation des données", "error")
  } finally {
    loading.value = false
  }
}

watch(selectedOfferId, () => {
  fetchAnalytics()
})

const handleExport = async (format) => {
  if (!selectedOfferId.value) return
  isExporting.value = true
  toastStore.show(`Génération du rapport ${format}...`, "info")
  
  try {
    const endpoint = format === 'PDF' ? '/recruiter/export/pdf' : '/recruiter/export/excel'
    const extension = format === 'PDF' ? 'pdf' : 'xlsx'
    
    const response = await axios.get(`${endpoint}?jobOfferId=${selectedOfferId.value}`, {
      responseType: 'blob'
    })
    
    const blob = new Blob([response.data])
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    
    const offerTitle = offers.value.find(o => o.id === selectedOfferId.value)?.title || 'Report'
    link.setAttribute('download', `Rapport_Premium_${offerTitle.replace(/\s+/g, '_')}.${extension}`)
    
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    
    toastStore.show(`Rapport ${format} prêt`, "success")
  } catch (err) {
    console.error('Export error:', err)
    toastStore.show(`Erreur lors de l'export ${format}`, "error")
  } finally {
    isExporting.value = false
  }
}

onMounted(() => {
  fetchOffers()
})
</script>

<style scoped>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";
@import "@/assets/admin-theme.css";

.main-content {
  flex: 1;
  padding: 40px;
  position: relative;
  z-index: 5;
  min-height: 100vh;
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}

.content-wrapper {
  position: relative;
  z-index: 10;
  max-width: 1600px;
  margin: 0 auto;
}

.dashboard-render {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.dashboard-main-columns {
  display: grid;
  grid-template-columns: 8fr 4fr;
  gap: 32px;
}

.side-analytics {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

@media (max-width: 1400px) {
  .dashboard-main-columns { grid-template-columns: 1fr; }
}

@media (max-width: 768px) {
  .main-content { padding: 20px; }
}
</style>
