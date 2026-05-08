<template>
  <div class="report-state-wrapper">
    <!-- LOADING STATE -->
    <div v-if="loading" class="state-content loading">
      <div class="crystal-loader">
        <div class="crystal-outer"></div>
        <div class="crystal-inner"></div>
        <div class="scan-beam"></div>
      </div>
      <h3 class="loading-title">Intelligence Artificielle en cours...</h3>
      <p class="loading-subtitle">Calcul des métriques et extraction des tendances clés.</p>
    </div>

    <!-- EMPTY STATE -->
    <div v-else-if="!selectedOfferId" class="state-content empty anim-reveal-up">
      <div class="insight-visual">
        <div class="core-icon">
          <BarChart3 :size="32" stroke-width="1.5" />
        </div>
      </div>
      <h2 class="insight-title">Analyse des Talents</h2>
      <p class="insight-desc">Veuillez sélectionner une mission pour générer un rapport détaillé de performance et de coaching.</p>
      
      <div class="selection-hint">
        <span class="hint-dot"></span>
        SÉLECTIONNEZ UNE OFFRE DANS LA BARRE SUPÉRIEURE
      </div>
    </div>
  </div>
</template>

<script setup>
import { BarChart3 } from 'lucide-vue-next'

defineProps({
  loading: Boolean,
  selectedOfferId: String
})
</script>

<style scoped>
.report-state-wrapper {
  height: 60vh;
  display: flex;
  align-items: center;
  justify-content: center;
  perspective: 1000px;
}

.state-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

/* CRYSTAL LOADER */
.crystal-loader {
  position: relative;
  width: 120px;
  height: 120px;
  margin-bottom: 40px;
}

.crystal-outer {
  position: absolute;
  inset: 0;
  border: 2px solid var(--accent-color);
  border-radius: 35% 65% 70% 30% / 30% 30% 70% 70%;
  animation: morph 8s ease-in-out infinite;
  opacity: 0.3;
}

.crystal-inner {
  position: absolute;
  inset: 20px;
  background: var(--accent-grad);
  border-radius: 50%;
  filter: blur(20px);
  opacity: 0.2;
  animation: pulse 2s ease-in-out infinite;
}

.scan-beam {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 2px;
  background: var(--accent-color);
  box-shadow: 0 0 15px var(--accent-color);
  animation: beam 2s ease-in-out infinite;
}

@keyframes morph {
  0%, 100% { border-radius: 35% 65% 70% 30% / 30% 30% 70% 70%; }
  50% { border-radius: 65% 35% 30% 70% / 70% 70% 30% 30%; }
}

@keyframes beam {
  0%, 100% { transform: translateY(0); opacity: 0; }
  50% { transform: translateY(120px); opacity: 1; }
}

@keyframes pulse {
  0%, 100% { transform: scale(1); opacity: 0.2; }
  50% { transform: scale(1.2); opacity: 0.4; }
}

.loading-title { font-size: 20px; font-weight: 800; color: var(--text-main); margin-bottom: 8px; }
.loading-subtitle { color: var(--text-muted); font-size: 14px; }

/* INSIGHT VISUAL */
.insight-visual {
  position: relative;
  width: 180px;
  height: 180px;
  margin-bottom: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.orbit-container { position: absolute; inset: 0; }
.orbit {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  border: 1px dashed var(--border-thin);
  border-radius: 50%;
}

.o1 { width: 100%; height: 100%; animation: spin 30s linear infinite; }
.o2 { width: 75%; height: 75%; animation: spin 20s linear reverse infinite; }
.o3 { width: 50%; height: 50%; animation: spin 15s linear infinite; }

.core-icon {
  width: 80px;
  height: 80px;
  background: var(--card-bg);
  border: 1px solid var(--border-thin);
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--accent-color);
  box-shadow: var(--glass-shadow);
  animation: float 4s ease-in-out infinite;
}

.core-icon svg { width: 32px; height: 32px; }

@keyframes spin { from { transform: translate(-50%, -50%) rotate(0deg); } to { transform: translate(-50%, -50%) rotate(360deg); } }
@keyframes float { 0%, 100% { transform: translateY(0); } 50% { transform: translateY(-15px); } }

.insight-title { font-size: 32px; font-weight: 900; color: var(--text-main); margin-bottom: 8px; letter-spacing: -0.5px; }
.insight-desc { color: var(--text-muted); max-width: 500px; line-height: 1.6; font-size: 16px; margin-bottom: 24px; }

.selection-hint {
  display: inline-flex;
  align-items: center;
  gap: 12px;
  padding: 12px 24px;
  background: var(--accent-soft);
  border: 1px solid var(--accent-color);
  opacity: 0.8;
  border-radius: 100px;
  color: var(--accent-color);
  font-size: 11px;
  font-weight: 900;
  letter-spacing: 1.5px;
}

.hint-dot { width: 8px; height: 8px; background: var(--accent-color); border-radius: 50%; box-shadow: 0 0 10px var(--accent-color); animation: blink 1.5s infinite; }
@keyframes blink { 0%, 100% { opacity: 1; transform: scale(1); } 50% { opacity: 0.4; transform: scale(0.8); } }

.anim-reveal-up { animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes revealUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
