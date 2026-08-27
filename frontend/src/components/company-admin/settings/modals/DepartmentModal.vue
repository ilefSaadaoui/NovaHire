<template>
  <Transition name="celestial-modal">
    <div v-if="show" class="luxury-modal-overlay" @click.self="$emit('close')">
      <div class="luxury-modal-content anim-scale-up">
        <div class="modal-header-premium">
          <h3 class="celestial-modal-title imperial-aura">{{ isEditing ? 'Éditer le département' : 'Nouveau département' }}</h3>
          <button class="close-lux-btn" @click="$emit('close')">&times;</button>
        </div>
        
        <div class="modal-body-premium">
          <p class="modal-intro">Définissez l'organisation de votre entreprise pour structurer vos offres.</p>
          
          <div class="lux-input-group mt-20">
            <label class="imperial-label">Nom <span style="color:var(--accent)">*</span></label>
            <div class="lux-input-wrap">
              <input :value="form.name" @input="$emit('update:form', { ...form, name: $event.target.value })" type="text" class="lux-input-text" placeholder="Ex: Ressources Humaines">
            </div>
          </div>
          
          <div class="lux-input-group mt-20">
            <label class="imperial-label">Description</label>
            <div class="lux-input-wrap">
              <textarea :value="form.description" @input="$emit('update:form', { ...form, description: $event.target.value })" class="lux-input-text lux-textarea" placeholder="Optionnel..."></textarea>
            </div>
          </div>
          
          <div class="modal-footer-lux">
            <button class="btn-cancel" @click="$emit('close')">Annuler</button>
            <button class="btn-premium btn-accent" @click="$emit('submit')" :disabled="loading" style="padding: 12px 24px;">
              <Loader2 v-if="loading" class="spin" :size="16" />
              <Save v-else-if="isEditing" :size="16" stroke-width="3" />
              <Plus v-else :size="16" stroke-width="3" />
              <span style="font-size: 14px; letter-spacing: 0.5px;">{{ isEditing ? 'METTRE À JOUR' : 'CRÉER' }}</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { Save, Loader2, Plus } from 'lucide-vue-next'

defineProps({
  show: Boolean,
  isEditing: Boolean,
  form: Object,
  loading: Boolean
})

defineEmits(['close', 'submit', 'update:form'])
</script>

<style scoped>
.luxury-modal-overlay {
  position: fixed;
  top: 0; left: 0;
  width: 100%; height: 100%;
  z-index: 2000;
  background: rgba(15, 23, 42, 0.65);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.luxury-modal-content {
  background: var(--card-bg, #ffffff);
  width: 100%;
  max-width: 480px;
  border-radius: 24px;
  padding: 32px;
  position: relative;
  border: 1px solid var(--r-border, rgba(0,0,0,0.06));
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  transition: all 0.3s ease;
}

:global(.dark-mode) .luxury-modal-content,
:global(.theme-dark) .luxury-modal-content {
  background: #0f172a !important;
  border-color: rgba(255, 255, 255, 0.1) !important;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.6), 0 0 0 1px rgba(14, 165, 233, 0.15) !important;
}

.modal-header-premium {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.celestial-modal-title {
  font-size: 24px;
  font-weight: 900;
  color: var(--r-text-main, #0f172a);
  letter-spacing: -0.5px;
  margin: 0;
}

:global(.dark-mode) .celestial-modal-title,
:global(.theme-dark) .celestial-modal-title {
  color: #f8fafc !important;
}

.close-lux-btn {
  background: rgba(0,0,0,0.04);
  border: 1px solid var(--r-border, #e2e8f0);
  width: 36px; height: 36px;
  border-radius: 50%;
  font-size: 20px;
  color: var(--r-text-sub, #64748b);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.3s;
  line-height: 1;
}

:global(.dark-mode) .close-lux-btn,
:global(.theme-dark) .close-lux-btn {
  background: rgba(255, 255, 255, 0.06) !important;
  border-color: rgba(255, 255, 255, 0.1) !important;
  color: #94a3b8 !important;
}

.close-lux-btn:hover {
  background: #e2e8f0;
  color: #0f172a;
  transform: rotate(90deg);
}

:global(.dark-mode) .close-lux-btn:hover,
:global(.theme-dark) .close-lux-btn:hover {
  background: rgba(239, 68, 68, 0.15) !important;
  color: #ef4444 !important;
}

.modal-intro {
  font-size: 14px;
  color: var(--r-text-sub, #64748b);
  margin-bottom: 28px;
  font-weight: 500;
  line-height: 1.5;
}

:global(.dark-mode) .modal-intro,
:global(.theme-dark) .modal-intro {
  color: #94a3b8 !important;
}

.lux-input-group { display: flex; flex-direction: column; gap: 8px; }
.mt-20 { margin-top: 20px; }

.imperial-label {
  font-size: 11px;
  font-weight: 800;
  color: var(--r-text-sub, #475569);
  text-transform: uppercase;
  letter-spacing: 1px;
}

:global(.dark-mode) .imperial-label,
:global(.theme-dark) .imperial-label {
  color: #94a3b8 !important;
}

.lux-input-wrap {
  display: flex;
  align-items: center;
  gap: 12px;
  background: var(--r-input-bg, #f8fafc);
  padding: 14px 16px;
  border-radius: 14px;
  border: 1px solid var(--r-border, #e2e8f0);
  transition: 0.3s;
}

:global(.dark-mode) .lux-input-wrap,
:global(.theme-dark) .lux-input-wrap {
  background: rgba(15, 23, 42, 0.7) !important;
  border-color: rgba(255, 255, 255, 0.12) !important;
}

.lux-input-wrap:focus-within {
  border-color: #0ea5e9;
  background: #fff;
  box-shadow: 0 0 0 4px rgba(14, 165, 233, 0.12);
}

:global(.dark-mode) .lux-input-wrap:focus-within,
:global(.theme-dark) .lux-input-wrap:focus-within {
  background: rgba(15, 23, 42, 0.95) !important;
  border-color: #0ea5e9 !important;
  box-shadow: 0 0 0 4px rgba(14, 165, 233, 0.25) !important;
}

.lux-input-text {
  flex: 1;
  background: transparent;
  border: none;
  outline: none;
  color: var(--r-text-main, #0f172a);
  font-weight: 600;
  font-size: 14px;
}

:global(.dark-mode) .lux-input-text,
:global(.theme-dark) .lux-input-text {
  color: #f8fafc !important;
}

.lux-input-text::placeholder { color: #94a3b8; font-weight: 500; }
.lux-textarea { min-height: 80px; resize: vertical; padding-top: 4px; }

.modal-footer-lux {
  margin-top: 32px;
  padding-top: 20px;
  border-top: 1px solid var(--r-border, #f1f5f9);
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

:global(.dark-mode) .modal-footer-lux,
:global(.theme-dark) .modal-footer-lux {
  border-color: rgba(255, 255, 255, 0.08) !important;
}

.btn-cancel {
  background: var(--card-bg, white);
  border: 1px solid var(--r-border, #cbd5e1);
  padding: 0 20px;
  height: 44px;
  border-radius: 12px;
  font-weight: 700;
  cursor: pointer;
  color: var(--r-text-main, #475569);
  transition: 0.3s;
}

:global(.dark-mode) .btn-cancel,
:global(.theme-dark) .btn-cancel {
  background: rgba(255, 255, 255, 0.05) !important;
  border-color: rgba(255, 255, 255, 0.12) !important;
  color: #94a3b8 !important;
}

.btn-cancel:hover {
  background: #f8fafc;
  color: #0f172a;
  border-color: #94a3b8;
}

:global(.dark-mode) .btn-cancel:hover,
:global(.theme-dark) .btn-cancel:hover {
  background: rgba(255, 255, 255, 0.1) !important;
  color: #ffffff !important;
  border-color: rgba(255, 255, 255, 0.25) !important;
}

.btn-premium.btn-accent {
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  color: white !important;
  box-shadow: 0 6px 20px -4px rgba(14, 165, 233, 0.4), 0 2px 0 #0891b2;
  border: none;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  font-weight: 800;
  cursor: pointer;
  height: 44px;
  padding: 0 24px;
  border-radius: 14px;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  font-size: 13px;
  position: relative;
  overflow: hidden;
}

.btn-premium.btn-accent svg {
  color: white !important;
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
  box-shadow: 0 10px 28px -6px rgba(14, 165, 233, 0.5), 0 1px 0 #0891b2;
}

.btn-premium.btn-accent:active {
  transform: translateY(1px);
  box-shadow: 0 2px 6px rgba(0,0,0,0.15);
}

.btn-premium.btn-accent:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.spin { animation: rotate 2s linear infinite; }
@keyframes rotate { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
.anim-scale-up { animation: scaleUp 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275) both; }
@keyframes scaleUp { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }
</style>
