<template>
  <header class="candidate-header-celestial anim-reveal-down">
    <div class="header-glass-backdrop"></div>
    
    <div class="header-main-content">
      <div class="candidate-info-block">
        <button class="back-pill" @click="$emit('back')" title="Retour">
          <ArrowLeft :size="20" stroke-width="3" />
        </button>
        
        <div class="title-meta-stack">
          <h1 class="candidate-name-display">{{ formattedName }}</h1>
          <div class="meta-badge-row">
            <div class="meta-badge offer-badge">
              <Briefcase :size="14" />
              {{ candidate.jobTitle }}
            </div>
            <div v-if="candidate.quizScore !== null && candidate.quizScore !== undefined" class="meta-badge quiz-badge" title="Résultat du Quiz IA">
              <Zap :size="14" />
              Quiz : {{ candidate.quizScore }}%
            </div>
            <div class="meta-badge location-badge">
              <MapPin :size="14" />
              {{ candidate.location }}
            </div>
          </div>
        </div>
      </div>

      <div class="header-actions-premium">
        <div class="action-glass-bar">
          <button class="action-btn secondary" @click="$emit('view-cv')" title="Voir le CV">
            <Eye :size="18" />
            <span>CV</span>
          </button>

          <button 
            class="action-btn ai-special" 
            :class="{ 'is-analyzing': analyzing }" 
            :disabled="analyzing" 
            @click="$emit('reanalyze')"
          >
            <Loader2 v-if="analyzing" :size="18" class="spin-icon" />
            <Sparkles v-else :size="18" />
            <span>{{ analyzing ? 'Analyse...' : 'Réanalyser' }}</span>
          </button>

          <button class="action-btn accent" @click="$emit('schedule-interview')">
            <Calendar :size="18" />
            <span>Entretien</span>
          </button>

          <button class="action-btn dark" @click="$emit('generate-offer')">
            <FileText :size="18" />
            <span>Offre</span>
          </button>
        </div>
      </div>
    </div>
  </header>
</template>

<script setup>
import { computed } from 'vue'
import { ArrowLeft, Eye, Calendar, Sparkles, Loader2, FileText, Briefcase, MapPin } from 'lucide-vue-next'

const props = defineProps({
  candidate: Object,
  accentColor: String,
  accentDark: String,
  analyzing: { type: Boolean, default: false }
})

const emit = defineEmits(['back', 'view-cv', 'schedule-interview', 'reanalyze', 'generate-offer'])

const formattedName = computed(() => {
  if (!props.candidate?.fullName) return '...'
  return props.candidate.fullName
    .toLowerCase()
    .split(' ')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ')
})
</script>

<style scoped>
.candidate-header-celestial {
  position: relative;
  margin-bottom: 40px;
  border-radius: 24px;
  overflow: hidden;
  padding: 32px;
  background: linear-gradient(135deg, rgba(255, 255, 255, 0.9), rgba(255, 255, 255, 0.6));
  border: 1px solid rgba(255, 255, 255, 0.8);
  box-shadow: 0 10px 30px -10px rgba(0, 0, 0, 0.05);
  color: #1e293b; /* Base text color for light mode */
}

.anim-reveal-down {
  animation: revealDown 0.8s cubic-bezier(0.16, 1, 0.3, 1) both;
}

.header-glass-backdrop {
  position: absolute;
  inset: 0;
  backdrop-filter: blur(12px);
  z-index: 0;
  background: rgba(255, 255, 255, 0.1);
}

.header-main-content {
  position: relative;
  z-index: 1;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 24px;
}

.candidate-info-block {
  display: flex;
  align-items: center;
  gap: 24px;
}

.back-pill {
  width: 48px;
  height: 48px;
  border-radius: 16px;
  background: white;
  border: 1px solid #e2e8f0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #64748b; /* Explicit Slate */
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.03);
}

.back-pill svg {
  color: inherit; /* Ensure icon uses parent color */
}

.back-pill:hover {
  background: #f8fafc;
  transform: translateX(-4px);
  color: var(--accent);
  border-color: var(--accent);
}

.candidate-name-display {
  font-size: 32px;
  font-weight: 900;
  color: #0f172a; /* Explicit Dark Navy */
  margin: 0 0 8px 0;
  letter-spacing: -0.02em;
}

.meta-badge-row {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.meta-badge {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 100px;
  font-size: 13px;
  font-weight: 700;
  color: #475569; /* Explicit Slate */
}

.meta-badge svg {
  color: inherit;
}

.offer-badge {
  color: var(--accent);
  border-color: var(--accent-soft);
  background: var(--accent-soft);
}

.quiz-badge {
  color: #8b5cf6;
  border-color: rgba(139, 92, 246, 0.2);
  background: rgba(139, 92, 246, 0.05);
}

.header-actions-premium {
  display: flex;
  align-items: center;
}

.action-glass-bar {
  display: flex;
  background: rgba(255, 255, 255, 0.6);
  backdrop-filter: blur(8px);
  padding: 6px;
  border-radius: 18px;
  border: 1px solid white;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.05);
  gap: 6px;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 20px;
  border-radius: 14px;
  font-size: 14px;
  font-weight: 800;
  border: none;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.action-btn.secondary {
  background: white;
  color: #475569;
  border: 1px solid #e2e8f0;
}

.action-btn.secondary:hover {
  background: #f8fafc;
  transform: translateY(-2px);
}

.action-btn.accent {
  background: var(--accent-grad);
  color: white;
  box-shadow: 0 4px 15px var(--accent-soft);
}

.action-btn.accent:hover {
  transform: translateY(-2px) scale(1.02);
  box-shadow: 0 8px 25px var(--accent-soft);
}

.action-btn.ai-special {
  background: linear-gradient(135deg, #8b5cf6, #6366f1);
  color: white;
  box-shadow: 0 4px 15px rgba(139, 92, 246, 0.2);
}

.action-btn.ai-special:hover {
  transform: translateY(-2px) scale(1.02);
  box-shadow: 0 8px 25px rgba(139, 92, 246, 0.3);
}

.action-btn.dark {
  background: #1e293b;
  color: white;
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
}

.action-btn.dark:hover {
  background: #0f172a;
  transform: translateY(-2px);
}

/* Dark Mode Overrides - Premium Enhancement */
.dark-mode .candidate-header-celestial {
  background: linear-gradient(135deg, rgba(15, 23, 42, 0.9), rgba(30, 41, 59, 0.7));
  border-color: var(--accent-soft);
  box-shadow: 0 10px 40px -10px rgba(0, 0, 0, 0.5), 0 0 20px var(--accent-soft);
}

.dark-mode .candidate-name-display {
  color: #ffffff;
  text-shadow: 0 0 20px rgba(255, 255, 255, 0.1);
}

.dark-mode .back-pill {
  background: rgba(255, 255, 255, 0.03);
  border-color: rgba(255, 255, 255, 0.1);
  color: #94a3b8;
}

.dark-mode .back-pill:hover {
  background: var(--accent-soft);
  color: var(--accent);
  border-color: var(--accent);
  box-shadow: 0 0 15px var(--accent-glow);
}

.dark-mode .meta-badge {
  background: var(--accent-soft);
  border-color: var(--accent-glow);
  color: var(--accent);
}

.dark-mode .action-glass-bar {
  background: rgba(2, 6, 23, 0.6);
  border-color: rgba(255, 255, 255, 0.08);
}

.dark-mode .action-btn.secondary {
  background: rgba(255, 255, 255, 0.03);
  color: #cbd5e1;
  border-color: rgba(255, 255, 255, 0.1);
}

.dark-mode .action-btn.secondary:hover {
  background: rgba(255, 255, 255, 0.08);
  border-color: rgba(255, 255, 255, 0.2);
}

.spin-icon {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@keyframes revealDown {
  from { opacity: 0; transform: translateY(-20px); }
  to { opacity: 1; transform: translateY(0); }
}

@media (max-width: 900px) {
  .header-main-content {
    flex-direction: column;
    align-items: flex-start;
  }
  .action-glass-bar {
    width: 100%;
    justify-content: space-between;
  }
  .action-btn span {
    display: none;
  }
}
</style>
