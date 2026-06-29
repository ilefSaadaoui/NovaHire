<template>
  <div class="glass-column full-width anim-reveal-up" style="--delay: 0.3s">
    <div class="column-header">
      <div class="header-with-icon">
        <Briefcase :size="24" class="header-icon" />
        <h3>Meilleures Offres</h3>
      </div>
      <p>Les offres d'emploi générant le plus de candidatures.</p>
    </div>
    
    <div class="top-offers-list">
      <div v-for="(offer, idx) in topOffers" :key="idx" class="offer-item">
        <div class="offer-rank">#{{ idx + 1 }}</div>
        <div class="offer-title">{{ offer.title }}</div>
        <div class="offer-count">{{ offer.count }}</div>
      </div>
      <div v-if="!topOffers?.length" class="empty-mini">
        Aucune donnée disponible.
      </div>
    </div>
  </div>
</template>

<script setup>
import { Briefcase } from 'lucide-vue-next'

defineProps({
  topOffers: Array
})
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

.full-width { grid-column: span 2; }

.header-with-icon { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; }
.header-icon { color: var(--accent); }
.column-header h3 { font-size: 20px; font-weight: 900; margin: 0; color: var(--text-main); }
.column-header p { font-size: 14px; color: var(--text-muted); margin: 0 0 20px 0; font-weight: 500; }

.top-offers-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.offer-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 12px 16px;
  background: rgba(255,255,255,0.03);
  border-radius: 12px;
  border: 1px solid var(--border-thin);
  transition: all 0.3s ease;
}

.offer-item:hover {
  background: rgba(var(--accent-rgb), 0.05);
  border-color: var(--accent-color);
  transform: translateX(5px);
}

.offer-rank {
  font-weight: 900;
  color: var(--accent);
  background: var(--accent-soft);
  padding: 4px 8px;
  border-radius: 6px;
  font-size: 12px;
}

.offer-title {
  flex: 1;
  font-weight: 700;
  color: var(--text-main);
  font-size: 14px;
}

.offer-count {
  font-size: 13px;
  font-weight: 600;
  color: var(--text-muted);
}

.empty-mini { color: var(--text-muted); opacity: 0.5; font-size: 14px; font-weight: 600; padding: 20px 0; width: 100%; text-align: center; }

.anim-reveal-up { animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; animation-delay: var(--delay, 0s); }
@keyframes revealUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
