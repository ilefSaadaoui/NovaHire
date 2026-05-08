<template>
  <div class="glass-column full-width anim-reveal-up" style="--delay: 0.2s">
    <div class="column-header">
      <div class="header-with-icon">
        <BrainCircuit :size="24" class="header-icon" />
        <h3>Cartographie des Compétences</h3>
      </div>
      <p>Analyse approfondie des expertises détectées dans votre vivier.</p>
    </div>
    
    <div class="skills-galaxy">
      <div v-for="skill in topSkills" :key="skill.name" 
           class="g-skill-tag" 
           :style="{ '--size': (skill.count / maxSkillCount), '--color': 'var(--accent)' }">
        <span class="g-name">{{ skill.name }}</span>
        <span class="g-cnt" :style="{ background: 'var(--accent-soft)', color: 'var(--accent)' }">{{ skill.count }}</span>
      </div>
      <div v-if="!topSkills?.length" class="empty-mini">
        En attente d'analyses pour cette offre...
      </div>
    </div>
  </div>
</template>

<script setup>
import { BrainCircuit } from 'lucide-vue-next'

defineProps({
  topSkills: Array,
  maxSkillCount: Number
})
</script>

<style scoped>
.glass-column {
  background: var(--glass-bg);
  backdrop-filter: blur(25px) saturate(200%);
  border: 1px solid var(--glass-border-color);
  border-radius: 32px;
  padding: 32px;
  box-shadow: var(--premium-shadow);
}

.full-width { grid-column: span 2; }

.header-with-icon { display: flex; align-items: center; gap: 12px; margin-bottom: 8px; }
.header-icon { color: var(--accent); }
.column-header h3 { font-size: 20px; font-weight: 900; margin: 0; color: var(--text-main); }
.column-header p { font-size: 14px; color: var(--text-muted); margin: 0 0 30px 0; font-weight: 500; }

.skills-galaxy { display: flex; flex-wrap: wrap; gap: 15px; }
.g-skill-tag {
  background: rgba(255,255,255,0.03);
  backdrop-filter: blur(5px);
  border: 1px solid var(--border-thin);
  padding: 12px 20px;
  border-radius: 18px;
  display: flex;
  align-items: center;
  gap: 12px;
  transition: all 0.3s ease;
  transform: scale(calc(var(--size) * 0.3 + 0.8));
  opacity: calc(var(--size) * 0.5 + 0.5);
}

.g-skill-tag:hover {
  background: var(--accent);
  color: white;
  transform: translateY(-5px) scale(calc(var(--size) * 0.4 + 0.9));
  box-shadow: 0 10px 30px var(--accent-soft);
  border-color: var(--color);
}

.g-name { font-weight: 800; font-size: 16px; color: var(--text-main); }
.g-skill-tag:hover .g-name { color: white; }
.g-cnt { font-size: 11px; font-weight: 900; background: var(--card-bg); color: var(--accent-color); padding: 2px 8px; border-radius: 6px; }

.empty-mini { color: var(--text-muted); opacity: 0.5; font-size: 14px; font-weight: 600; padding: 40px 0; width: 100%; text-align: center; }

.anim-reveal-up { animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; animation-delay: var(--delay, 0s); }
@keyframes revealUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
