<template>
  <Teleport to="body">
    <transition name="modal-fade">
      <div v-if="isOpen" class="modal-overlay" @click.self="close">
        <div class="modal-content premium-glass anim-reveal-scale" @click.stop>
          <!-- Header -->
          <div class="comp-header">
            <div class="header-left">
              <div class="battle-icon">
                <Zap :size="20" />
              </div>
              <div class="header-titles">
                <h2 class="title-v7">Battle Mode : Comparaison Élite</h2>
                <p class="sub-txt">Analyse sémantique croisée par IA</p>
              </div>
            </div>
            <button class="close-btn-v7" @click="close">
              <X :size="20" />
            </button>
          </div>

          <div class="comp-body custom-scroll" v-if="comparisonData">
            <div class="battle-arena">
              <!-- Left Side: First Candidate -->
              <div v-if="comparisonData.candidates[0]" class="arena-candidate left anim-stagger-1">
                <div class="arena-card" :style="{ '--cand-color': getCandidateColor(0, 1) }">
                  <div class="arena-avatar" :style="{ '--avatar-color': getAvatarColor(comparisonData.candidates[0].id) }">
                    {{ comparisonData.candidates[0].initials }}
                  </div>
                  <h3 class="arena-name">{{ comparisonData.candidates[0].name }}</h3>
                  <div class="arena-score">{{ comparisonData.candidates[0].overallScore }}%</div>
                  
                  <div class="arena-stats">
                    <div class="stat-item">
                      <div class="stat-info"><span>Expérience</span><strong>{{ comparisonData.candidates[0].experienceScore }}%</strong></div>
                      <div class="stat-bar"><div class="bar-fill" :style="{ width: comparisonData.candidates[0].experienceScore + '%', background: getCandidateColor(0, 1) }"></div></div>
                    </div>
                    <div class="stat-item">
                      <div class="stat-info"><span>Formation</span><strong>{{ comparisonData.candidates[0].educationScore }}%</strong></div>
                      <div class="stat-bar"><div class="bar-fill" :style="{ width: comparisonData.candidates[0].educationScore + '%', background: getCandidateColor(0, 1) }"></div></div>
                    </div>
                    <div class="stat-item">
                      <div class="stat-info"><span>Compétences</span><strong>{{ comparisonData.candidates[0].skillsScore }}%</strong></div>
                      <div class="stat-bar"><div class="bar-fill" :style="{ width: comparisonData.candidates[0].skillsScore + '%', background: getCandidateColor(0, 1) }"></div></div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Center: Radar Chart -->
              <div class="arena-center anim-stagger-2">
                <div class="radar-container">
                  <svg viewBox="0 0 550 400" class="radar-svg">
                    <!-- Background Circles -->
                    <circle cx="275" cy="200" r="130" class="radar-grid" />
                    <circle cx="275" cy="200" r="85" class="radar-grid" />
                    <circle cx="275" cy="200" r="45" class="radar-grid" />
                    
                    <!-- Axis -->
                    <line v-for="(axis, i) in axes" :key="'axis-'+i" 
                      :x1="275" :y1="200" 
                      :x2="getPoint(i, 1).x" :y2="getPoint(i, 1).y" 
                      class="radar-axis" />
                    
                    <!-- Axis Labels -->
                    <text v-for="(axis, i) in axes" :key="'label-'+i"
                      :x="getPoint(i, 1.35).x" :y="getPoint(i, 1.35).y"
                      class="radar-label" text-anchor="middle">
                      {{ axis.label }}
                    </text>

                    <!-- Data Polygons -->
                    <polygon v-for="(c, idx) in comparisonData.candidates" :key="'poly-'+idx"
                      :points="getPolygonPoints(c)"
                      :class="'poly-' + idx"
                      :style="{ fill: getCandidateColor(idx, 0.15), stroke: getCandidateColor(idx, 1) }"
                      class="radar-poly" />

                    <!-- Vertices Points -->
                    <template v-for="(c, idx) in comparisonData.candidates" :key="'points-'+idx">
                      <circle v-for="(axis, i) in axes" :key="'p-'+idx+'-'+i"
                        :cx="getPoint(i, (c[axis.key] || 0) / 100).x"
                        :cy="getPoint(i, (c[axis.key] || 0) / 100).y"
                        r="4"
                        :fill="getCandidateColor(idx, 1)"
                        class="radar-point" />
                    </template>
                  </svg>
                </div>

                <div class="vs-badge pulse-glow">VS</div>
              </div>

              <!-- Right Side: Second Candidate -->
              <div v-if="comparisonData.candidates[1]" class="arena-candidate right anim-stagger-3">
                <div class="arena-card" :style="{ '--cand-color': getCandidateColor(1, 1) }">
                  <div class="arena-avatar" :style="{ '--avatar-color': getAvatarColor(comparisonData.candidates[1].id) }">
                    {{ comparisonData.candidates[1].initials }}
                  </div>
                  <h3 class="arena-name">{{ comparisonData.candidates[1].name }}</h3>
                  <div class="arena-score">{{ comparisonData.candidates[1].overallScore }}%</div>
                  
                  <div class="arena-stats">
                    <div class="stat-item">
                      <div class="stat-info"><span>Expérience</span><strong>{{ comparisonData.candidates[1].experienceScore }}%</strong></div>
                      <div class="stat-bar"><div class="bar-fill" :style="{ width: comparisonData.candidates[1].experienceScore + '%', background: getCandidateColor(1, 1) }"></div></div>
                    </div>
                    <div class="stat-item">
                      <div class="stat-info"><span>Formation</span><strong>{{ comparisonData.candidates[1].educationScore }}%</strong></div>
                      <div class="stat-bar"><div class="bar-fill" :style="{ width: comparisonData.candidates[1].educationScore + '%', background: getCandidateColor(1, 1) }"></div></div>
                    </div>
                    <div class="stat-item">
                      <div class="stat-info"><span>Compétences</span><strong>{{ comparisonData.candidates[1].skillsScore }}%</strong></div>
                      <div class="stat-bar"><div class="bar-fill" :style="{ width: comparisonData.candidates[1].skillsScore + '%', background: getCandidateColor(1, 1) }"></div></div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Skills Comparison (Horizontal) -->
            <div class="arena-footer">
              <div class="skills-deep-dive">
                <h4 class="section-title">Compétences Communes</h4>
                <div class="common-skills-chips">
                  <span v-for="skill in comparisonData.commonSkills" :key="skill" class="skill-chip">
                    {{ skill }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- Loading State -->
          <div v-else class="comp-loading">
            <Loader2 class="spin" :size="48" />
            <p>Calcul des vecteurs sémantiques...</p>
          </div>
        </div>
      </div>
    </transition>
  </Teleport>
</template>

<script setup>
import { ref, watch } from 'vue';
import { Zap, X, Loader2 } from 'lucide-vue-next';
import api from '@/api/axios';

const props = defineProps({
  isOpen: Boolean,
  candidateIds: Array
});

const emit = defineEmits(['close']);

const comparisonData = ref(null);
const loading = ref(false);

const axes = [
  { label: 'Expérience', key: 'experienceScore' },
  { label: 'Formation', key: 'educationScore' },
  { label: 'Skills', key: 'skillsScore' },
  { label: 'Soft Skills', key: 'overallScore' }, // Placeholder for now
  { label: 'Matching JD', key: 'overallScore' }
];

const fetchComparison = async () => {
  if (!props.candidateIds || props.candidateIds.length === 0) return;
  loading.value = true;
  try {
    const res = await api.get(`/Recruiter/applications/compare?ids=${props.candidateIds.join(',')}`);
    comparisonData.value = res.data;
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};

watch(() => props.isOpen, (newVal) => {
  if (newVal) fetchComparison();
  else comparisonData.value = null;
});

const close = () => emit('close');

const getPoint = (i, factor) => {
  const angle = (Math.PI * 2) / axes.length * i - Math.PI / 2;
  const r = 130 * factor;
  return {
    x: 275 + r * Math.cos(angle),
    y: 200 + r * Math.sin(angle)
  };
};

const getPolygonPoints = (candidate) => {
  return axes.map((axis, i) => {
    const score = (candidate[axis.key] || 0) / 100;
    const p = getPoint(i, score);
    return `${p.x},${p.y}`;
  }).join(' ');
};

const getCandidateColor = (idx, alpha) => {
  const colors = [
    `rgba(79, 70, 229, ${alpha})`, // Indigo
    `rgba(16, 185, 129, ${alpha})`, // Emerald
    `rgba(245, 158, 11, ${alpha})`  // Amber
  ];
  return colors[idx % colors.length];
};

const getAvatarColor = (id) => {
  const colors = ['#0ea5e9', '#6366f1', '#f59e0b', '#ec4899', '#10b981'];
  const seed = id.toString().split('').reduce((acc, char) => acc + char.charCodeAt(0), 0);
  return colors[seed % colors.length];
};

const scoreClass = (s) => {
  if (s >= 85) return 'elite';
  if (s >= 70) return 'high';
  return 'low';
};
</script>

<style scoped>
.modal-overlay {
  position: fixed; inset: 0; z-index: 9999;
  background: rgba(2, 6, 23, 0.85);
  backdrop-filter: blur(15px);
  display: flex; align-items: center; justify-content: center;
  padding: 20px;
  pointer-events: all;
}

:not(.dark-mode) .modal-overlay {
  background: rgba(244, 247, 254, 0.85);
}

/* Imperial Glass Modal */
.modal-content {
  background: var(--r-surface);
  width: 100%; max-width: 1100px; max-height: 90vh;
  border-radius: 30px;
  border: 1px solid var(--r-border);
  box-shadow: var(--r-shadow-hover);
  display: flex; flex-direction: column; overflow: hidden;
  position: relative;
}

.comp-header {
  padding: 20px 30px;
  background: rgba(255, 255, 255, 0.02);
  border-bottom: 1px solid var(--r-border);
  display: flex; justify-content: space-between; align-items: center;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.header-titles {
  display: flex;
  flex-direction: column;
}

.header-titles h2.title-v7 { 
  margin: 0; font-size: 22px; font-weight: 950; color: var(--r-text-main); 
  display: flex; align-items: center; gap: 12px;
}

.header-titles p.sub-txt { margin: 4px 0 0; font-size: 13px; color: var(--r-text-sub); font-weight: 600; }

.battle-icon {
  width: 42px; height: 42px; border-radius: 14px;
  background: linear-gradient(135deg, #FFD700, #FFA500);
  display: flex; align-items: center; justify-content: center; color: #000;
  box-shadow: 0 8px 20px rgba(255, 215, 0, 0.2);
}

.close-btn-v7 {
  width: 36px; height: 36px; border-radius: 12px;
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  color: var(--r-text-sub);
  display: flex; align-items: center; justify-content: center;
  cursor: pointer; transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.close-btn-v7:hover {
  background: rgba(239, 68, 68, 0.1);
  border-color: rgba(239, 68, 68, 0.3);
  color: #ef4444;
  transform: rotate(90deg) scale(1.1);
}

.comp-body { 
  padding: 30px 40px; 
  overflow-y: auto; 
  flex: 1; 
  scrollbar-width: thin;
  scrollbar-color: var(--accent) var(--r-border);
}

.comp-body::-webkit-scrollbar {
  width: 8px;
}

.comp-body::-webkit-scrollbar-track {
  background: var(--r-main-bg);
  border-radius: 10px;
}

.comp-body::-webkit-scrollbar-thumb {
  background: var(--r-border);
  border-radius: 10px;
  border: 2px solid transparent;
  background-clip: content-box;
}

/* BATTLE ARENA LAYOUT */
.battle-arena {
  display: grid;
  grid-template-columns: 280px 1fr 280px;
  gap: 20px;
  align-items: center;
  margin-bottom: 30px;
}

.arena-candidate {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.arena-card {
  background: var(--r-surface);
  border-radius: 32px;
  padding: 40px 30px;
  border: 1px solid var(--r-border);
  border-top: 4px solid var(--cand-color);
  box-shadow: var(--r-shadow);
  text-align: center;
  position: relative;
  transition: transform 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.arena-card::after {
  content: ""; position: absolute; inset: 0;
  border-radius: inherit; padding: 2px;
  background: linear-gradient(135deg, var(--cand-color), transparent 60%);
  -webkit-mask: linear-gradient(#fff 0 0) content-box, linear-gradient(#fff 0 0);
  -webkit-mask-composite: xor; mask-composite: exclude;
  opacity: 0.15;
}

.arena-card:hover {
  transform: translateY(-10px) scale(1.02);
  box-shadow: var(--r-shadow-hover);
  border-color: var(--cand-color);
}

.arena-avatar {
  width: 76px; height: 76px; border-radius: 24px;
  background: var(--avatar-color); color: white;
  display: flex; align-items: center; justify-content: center;
  font-size: 30px; font-weight: 900; margin: 0 auto 15px;
  box-shadow: 0 12px 35px rgba(0,0,0,0.2);
  position: relative; z-index: 2;
}

.vs-badge {
  position: absolute; 
  bottom: -20px;
  left: 50%;
  transform: translateX(-50%);
  background: var(--r-text-main); 
  color: var(--r-surface); 
  padding: 8px 24px; 
  border-radius: 100px;
  font-weight: 950; 
  font-size: 14px; 
  box-shadow: 0 10px 30px rgba(0,0,0,0.2);
  z-index: 10;
  letter-spacing: 2px;
}

.arena-name { font-size: 20px; font-weight: 900; color: var(--r-text-main); margin-bottom: 8px; position: relative; z-index: 2; }

.arena-score { 
  font-size: 42px; font-weight: 950; margin-bottom: 25px;
  background: linear-gradient(to bottom, var(--r-text-main), var(--cand-color));
  -webkit-background-clip: text; -webkit-text-fill-color: transparent;
  filter: drop-shadow(0 0 10px rgba(var(--cand-color-rgb), 0.3));
}

.arena-stats {
  display: flex; flex-direction: column; gap: 15px;
  padding-top: 25px; border-top: 1px solid var(--r-border);
  position: relative; z-index: 2;
}

.stat-item { display: flex; flex-direction: column; gap: 8px; width: 100%; }
.stat-info { 
  display: flex; 
  justify-content: space-between; 
  font-size: 12px; 
  color: var(--r-text-sub); 
  font-weight: 700; 
  margin-bottom: 2px;
}

.stat-info span { letter-spacing: 0.5px; }
.stat-info strong { color: var(--r-text-main); font-weight: 950; }

.stat-bar {
  width: 100%; height: 6px; background: var(--r-main-bg); border-radius: 10px; overflow: hidden;
}
.bar-fill {
  height: 100%; border-radius: inherit;
  box-shadow: 0 0 10px currentColor;
  animation: fillBar 1.5s cubic-bezier(0.65, 0, 0.35, 1) forwards;
}
@keyframes fillBar { from { width: 0; } }

.pulse-glow {
  animation: pulseGlow 2s infinite alternate;
}
@keyframes pulseGlow {
  from { box-shadow: 0 0 10px rgba(128,128,128,0.1); transform: scale(1); }
  to { box-shadow: 0 0 20px rgba(128,128,128,0.3); transform: scale(1.1); }
}

.radar-point {
  filter: drop-shadow(0 0 8px currentColor);
  animation: fadeIn 1s ease forwards;
}
@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }

/* Stagger Animations */
.anim-stagger-1 { animation: slideIn 0.8s cubic-bezier(0.165, 0.84, 0.44, 1) 0.1s both; }
.anim-stagger-2 { animation: slideIn 0.8s cubic-bezier(0.165, 0.84, 0.44, 1) 0.3s both; }
.anim-stagger-3 { animation: slideIn 0.8s cubic-bezier(0.165, 0.84, 0.44, 1) 0.5s both; }

@keyframes slideIn {
  from { opacity: 0; transform: translateY(30px); }
  to { opacity: 1; transform: translateY(0); }
}

.radar-svg { width: 100%; max-width: 420px; }
.radar-grid {
  fill: none;
  stroke: var(--r-border);
  stroke-width: 1;
}

.radar-axis {
  stroke: var(--r-border);
  stroke-width: 1;
  stroke-dasharray: 4,4;
}

.radar-label {
  fill: var(--r-text-sub);
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 1px;
}
.radar-poly { 
  stroke-width: 3.5; 
  transition: all 0.8s cubic-bezier(0.165, 0.84, 0.44, 1); 
  cursor: pointer; 
  filter: drop-shadow(0 0 12px currentColor);
}

.arena-footer { margin-top: 10px; }
.skills-deep-dive { background: var(--r-main-bg); border-radius: 24px; padding: 25px; border: 1px solid var(--r-border); }
.section-title { font-size: 12px; font-weight: 900; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 2px; margin-bottom: 20px; display: flex; align-items: center; gap: 15px; }
.section-title::after { content: ""; flex: 1; height: 1px; background: var(--r-border); }

.common-skills-chips { display: flex; flex-wrap: wrap; gap: 10px; justify-content: center; }
.skill-chip {
  background: var(--r-surface); border: 1px solid var(--r-border);
  padding: 6px 14px; border-radius: 10px; font-size: 12px; font-weight: 800; color: var(--r-text-main);
}

.comp-loading { display: flex; flex-direction: column; align-items: center; gap: 24px; padding: 80px; }
.spin { animation: rotate 2s linear infinite; color: var(--accent); }
@keyframes rotate { from { transform: rotate(0); } to { transform: rotate(360deg); } }

/* Animations */
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.5s cubic-bezier(0.4, 0, 0.2, 1); }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }

.anim-reveal-scale { animation: revealScale 0.6s cubic-bezier(0.165, 0.84, 0.44, 1); }
@keyframes revealScale {
  from { opacity: 0; transform: scale(0.9) translateY(40px); }
  to { opacity: 1; transform: scale(1) translateY(0); }
}

@media (max-width: 1000px) {
  .battle-arena { grid-template-columns: 1fr; }
  .arena-candidate { flex-direction: row; justify-content: center; }
  .arena-card { width: 100%; max-width: 300px; }
}
</style>
