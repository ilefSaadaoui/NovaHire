<template>
  <div class="r-card notes-premium">
    <div class="notes-header">
      <MessageSquare :size="18" stroke-width="2.5" />
      <h4>Discussion Collaborative</h4>
    </div>
    <textarea 
      :value="modelValue" 
      @input="$emit('update:modelValue', $event.target.value)"
      placeholder="Ajoutez un commentaire ou une impression sur ce candidat..." 
      rows="2"
    ></textarea>
    <div class="notes-footer">
      <span class="hint">Visible par toute l'équipe de recrutement.</span>
      <button 
        class="btn-premium btn-accent" 
        @click="$emit('post')" 
        :disabled="loading || !modelValue?.trim()"
        style="padding: 10px 20px; border-radius: 12px;"
      >
        <Loader2 v-if="loading" class="spin" :size="16" />
        <span style="font-size: 14px; letter-spacing: 0.5px;">{{ loading ? 'ENVOI...' : 'PUBLIER' }}</span>
      </button>
    </div>
  </div>
</template>

<script setup>
import { MessageSquare, Loader2 } from 'lucide-vue-next'

defineProps({
  modelValue: String,
  loading: Boolean
})

defineEmits(['update:modelValue', 'post'])
</script>

<style scoped>
.r-card {
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  border-radius: 20px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.05);
}

.notes-premium { 
  padding: 24px; 
  display: flex; 
  flex-direction: column; 
  gap: 16px; 
  margin-top: 32px; 
  background: var(--card-bg);
  border: 1px solid var(--r-border);
  border-radius: 24px;
  backdrop-filter: blur(10px);
}
.notes-header { display: flex; align-items: center; gap: 12px; color: var(--r-text-main); }
.notes-header svg { width: 18px; color: var(--accent); }
.notes-header h4 { font-size: 16px; font-weight: 800; margin: 0; }

textarea { 
  width: 100%; 
  padding: 16px; 
  border-radius: 12px; 
  border: 1px solid var(--r-border); 
  background: var(--bg-hover); 
  color: var(--r-text-main); 
  font-family: inherit; 
  font-size: 14px; 
  outline: none; 
  transition: border-color 0.2s; 
  resize: vertical;
}
textarea:focus { border-color: var(--accent); }

.notes-footer { display: flex; justify-content: space-between; align-items: center; }
.notes-footer .hint { font-size: 12px; color: var(--r-text-sub); font-weight: 500; }

.btn-premium.btn-accent {
  background: var(--accent-grad);
  color: white;
  box-shadow: 0 8px 20px -5px var(--accent-soft), 0 3px 0 var(--accent-dark);
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  font-weight: 850;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  text-transform: uppercase;
  position: relative;
  overflow: hidden;
}

.btn-premium.btn-accent::after {
  content: '';
  position: absolute;
  top: 0; left: -100%;
  width: 100%; height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent);
  transition: 0.5s;
}

.btn-premium.btn-accent:hover::after { left: 100%; }

.btn-premium.btn-accent:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 25px -8px var(--accent-soft), 0 1px 0 var(--accent-dark);
}

.btn-premium.btn-accent:active {
  transform: translateY(1px);
  box-shadow: 0 2px 5px rgba(0,0,0,0.2), 0 0 0 var(--accent-dark);
}

.btn-premium.btn-accent:disabled {
  opacity: 0.7;
  cursor: not-allowed;
  transform: none !important;
  box-shadow: none !important;
}
/* Dark Mode Overrides - Premium Enhancement */
.dark-mode .notes-premium {
  background: #0f172a;
  border-color: var(--accent-soft);
}

.dark-mode .notes-header {
  color: var(--accent);
}

.dark-mode .notes-header h4 {
  color: #f8fafc;
}

.dark-mode textarea {
  background: rgba(2, 6, 23, 0.4);
  border-color: rgba(255, 255, 255, 0.1);
  color: #f8fafc;
}

.dark-mode textarea:focus {
  border-color: var(--accent);
  box-shadow: 0 0 10px var(--accent-soft);
}
</style>
