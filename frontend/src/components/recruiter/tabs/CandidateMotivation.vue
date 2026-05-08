<template>
  <div class="panel-fade">
    <div class="r-card premium-inner-card">
      <div class="motivation-letter-card">
        <Quote :size="32" class="quote-icon" />
        <div class="letter-inner">
          <p>{{ candidate.coverLetter }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { Quote } from 'lucide-vue-next'

defineProps({
  candidate: Object
})
</script>

<style scoped>
.premium-inner-card {
  background: var(--glass-bg);
  backdrop-filter: blur(25px) saturate(200%);
  border: 1px solid var(--glass-border-color);
  border-radius: 32px;
  padding: 32px;
  box-shadow: var(--premium-shadow);
}

.motivation-letter-card { 
  background: rgba(255,255,255,0.03); 
  border-radius: 32px; 
  border: 1px solid var(--glass-border-color); 
  padding: 48px; 
  position: relative; 
  overflow: hidden;
}
.quote-icon { position: absolute; top: 32px; left: 32px; color: var(--accent); opacity: 0.2; }
.letter-inner { position: relative; z-index: 1; line-height: 2; color: var(--r-text-main); font-size: 16px; margin-top: 20px; }

.panel-fade { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>
