<template>
  <header class="report-premium-header anim-reveal-down">
    <div class="header-main-card">
      <div class="title-section">
        <div class="badge-premium" :class="isAdmin ? 'admin-badge' : 'recruiter-badge'">
          {{ isAdmin ? 'ADMIN INSIGHTS' : 'TALENT ANALYTICS' }}
        </div>
        <h1 class="glitch-title">
          <span class="gradient-text">Stratégie & Talent Intelligence</span>
        </h1>
        <p class="subtitle-premium">Exploitez la puissance des données pour vos décisions stratégiques.</p>
      </div>

      <div class="controls-section">
        <div class="filter-group">
          <span class="filter-label">OFFRE ACTIVE</span>
          <div class="select-container-glass">
            <PremiumSelect 
              :modelValue="selectedOfferId" 
              @update:modelValue="$emit('update:selectedOfferId', $event)"
              :options="offerOptions" 
              placeholder="Toutes les missions"
            />
          </div>
        </div>

        <div class="v-divider"></div>

        <div class="actions-group" v-if="selectedOfferId">
          <button class="btn-premium pdf" @click="$emit('export', 'PDF')" :disabled="isExporting">
            <div class="btn-blur"></div>
            <span class="btn-icon">
              <FileDown :size="18" stroke-width="2.5" />
            </span>
            <span class="btn-label">RAPPORT PDF</span>
          </button>
          
          <button class="btn-premium excel" @click="$emit('export', 'Excel')" :disabled="isExporting">
            <div class="btn-blur"></div>
            <span class="btn-icon">
              <FileSpreadsheet :size="18" stroke-width="2.5" />
            </span>
            <span class="btn-label">EXTRACT DATA</span>
          </button>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup>
import PremiumSelect from '@/components/common/PremiumSelect.vue'
import { FileDown, FileSpreadsheet } from 'lucide-vue-next'
import { computed } from 'vue'
import { useAuthStore } from '@/stores/authStore'

const authStore = useAuthStore()
const isAdmin = computed(() => authStore.isAdmin)

defineProps({
  selectedOfferId: String,
  offerOptions: Array,
  isExporting: Boolean
})

defineEmits(['update:selectedOfferId', 'export'])
</script>

<style scoped>
.report-premium-header {
  margin-bottom: 40px;
  position: relative;
  z-index: 50;
}

.header-main-card {
  background: var(--glass-bg);
  backdrop-filter: blur(25px) saturate(200%);
  border: 1px solid var(--glass-border-color);
  border-radius: 32px;
  padding: 32px 40px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 80px;
  box-shadow: var(--premium-shadow);
}

.badge-premium {
  display: inline-flex;
  padding: 4px 12px;
  border-radius: 100px;
  font-size: 10px;
  font-weight: 900;
  letter-spacing: 1.5px;
  margin-bottom: 12px;
  text-transform: uppercase;
}

.admin-badge { background: rgba(0, 167, 225, 0.15); color: #00A7E1; border: 1px solid rgba(0, 167, 225, 0.2); }
.recruiter-badge { background: rgba(247, 201, 2, 0.15); color: #F7C902; border: 1px solid rgba(247, 201, 2, 0.2); }

.glitch-title { margin: 0; line-height: 1.1; }
.gradient-text {
  font-size: 38px;
  font-weight: 900;
  background: var(--accent-grad);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
  letter-spacing: -1px;
}

.dark-mode .gradient-text {
  background: linear-gradient(135deg, #fff 0%, rgba(255,255,255,0.7) 100%);
  -webkit-background-clip: text;
}

.subtitle-premium {
  margin: 8px 0 0 0;
  color: var(--text-muted);
  font-size: 16px;
  font-weight: 500;
}

.controls-section { display: flex; align-items: center; gap: 60px; }
.filter-group { display: flex; flex-direction: column; gap: 8px; }
.filter-label { font-size: 10px; font-weight: 800; color: var(--text-muted); opacity: 0.6; letter-spacing: 1px; }
.select-container-glass { width: 300px; }

.v-divider { width: 1px; height: 60px; background: var(--border-thin); }

.actions-group { display: flex; gap: 16px; }

.btn-premium {
  position: relative;
  padding: 14px 24px;
  border-radius: 16px;
  border: 1px solid var(--border-thin);
  background: var(--card-bg);
  color: var(--text-main);
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 12px;
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}

.btn-blur {
  position: absolute;
  inset: 0;
  background: var(--accent-grad);
  opacity: 0;
  transition: opacity 0.3s ease;
  z-index: 1;
}

.btn-icon, .btn-label { position: relative; z-index: 2; font-weight: 800; font-size: 12px; letter-spacing: 0.5px; }
.btn-icon svg { width: 18px; height: 18px; }

.btn-premium:hover:not(:disabled) {
  transform: translateY(-4px);
  border-color: var(--accent-color);
  box-shadow: 0 10px 20px var(--accent-soft);
}

.btn-premium:hover .btn-blur { opacity: 0.15; }

.pdf:hover { --accent-color: #ef4444; --accent-soft: rgba(239, 68, 68, 0.2); --accent-grad: linear-gradient(135deg, #ef4444, #991b1b); }
.excel:hover { --accent-color: #10b981; --accent-soft: rgba(16, 185, 129, 0.2); --accent-grad: linear-gradient(135deg, #10b981, #065f46); }

.anim-reveal-down { animation: revealDown 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes revealDown { from { opacity: 0; transform: translateY(-30px); } to { opacity: 1; transform: translateY(0); } }

@media (max-width: 1200px) {
  .header-main-card { flex-direction: column; align-items: flex-start; gap: 32px; }
  .controls-section { width: 100%; justify-content: space-between; }
}
</style>
