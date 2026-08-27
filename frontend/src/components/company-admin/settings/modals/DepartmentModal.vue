<template>
  <Transition name="celestial-modal">
    <div v-if="show" class="luxury-modal-overlay" @click.self="$emit('close')">
      <div class="luxury-modal-content anim-scale-up" @click.stop>
        
        <!-- ── Header ── -->
        <div class="modal-header-premium">
          <div class="modal-title-group">
            <div class="icon-box-themed">
              <Building :size="22" stroke-width="2.5" />
            </div>
            <div class="modal-title-text">
              <div class="modal-badge">{{ isEditing ? 'Édition' : 'Organisation' }}</div>
              <h3 class="celestial-modal-title">{{ isEditing ? 'Éditer le département' : 'Nouveau département' }}</h3>
              <p class="modal-intro">Définissez l'organisation de votre entreprise pour structurer vos offres.</p>
            </div>
          </div>
          <button class="close-lux-btn" @click="$emit('close')">
            <X :size="18" stroke-width="2.5" />
          </button>
        </div>
        
        <!-- ── Form ── -->
        <form @submit.prevent="$emit('submit')" class="modal-form-wrapper">
          <div class="modal-body-premium">
            
            <!-- Nom du département -->
            <div class="lux-input-group">
              <label class="imperial-label">Nom du département <span class="required-star">*</span></label>
              <div class="lux-input-wrap">
                <div class="input-icon-box"><Building :size="15" stroke-width="2.5" /></div>
                <input
                  :value="form.name"
                  @input="$emit('update:form', { ...form, name: $event.target.value })"
                  type="text"
                  required
                  class="lux-input-text"
                  placeholder="Ex: Ressources Humaines, Tech, Marketing..."
                >
              </div>
            </div>
            
            <!-- Description -->
            <div class="lux-input-group mt-16">
              <label class="imperial-label">Description (Optionnel)</label>
              <div class="lux-input-wrap textarea-wrap">
                <div class="input-icon-box textarea-icon"><AlignLeft :size="15" stroke-width="2.5" /></div>
                <textarea
                  :value="form.description"
                  @input="$emit('update:form', { ...form, description: $event.target.value })"
                  class="lux-input-text lux-textarea"
                  placeholder="Brève description du rôle ou des missions de ce département..."
                ></textarea>
              </div>
            </div>
            
          </div>

          <!-- ── Footer ── -->
          <div class="modal-footer-lux">
            <button type="button" class="btn-cancel" @click="$emit('close')">Annuler</button>
            <button type="submit" class="btn-premium btn-accent" :disabled="loading">
              <Loader2 v-if="loading" class="spin" :size="16" />
              <Save v-else-if="isEditing" :size="16" stroke-width="3" />
              <Plus v-else :size="16" stroke-width="3" />
              <span>{{ isEditing ? 'METTRE À JOUR' : 'CRÉER' }}</span>
            </button>
          </div>
        </form>

      </div>
    </div>
  </Transition>
</template>

<script setup>
import { Save, Loader2, Plus, Building, AlignLeft, X } from 'lucide-vue-next'

defineProps({
  show: Boolean,
  isEditing: Boolean,
  form: Object,
  loading: Boolean
})

defineEmits(['close', 'submit', 'update:form'])
</script>

<style scoped>
/* ── Overlay ── */
.luxury-modal-overlay {
  position: fixed !important;
  top: 0 !important; left: 0 !important;
  width: 100vw !important; height: 100vh !important;
  z-index: 99999 !important;
  background: rgba(15, 23, 42, 0.65);
  backdrop-filter: blur(12px);
  -webkit-backdrop-filter: blur(12px);
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  padding: 20px;
  box-sizing: border-box;
}

/* ── Content Card ── */
.luxury-modal-content {
  background: var(--card-bg, #ffffff);
  width: 100%;
  max-width: 520px;
  border-radius: 28px;
  padding: 28px 32px 24px;
  border: 1px solid var(--r-border, rgba(0, 0, 0, 0.08));
  box-shadow: 0 30px 70px rgba(0, 0, 0, 0.25), 0 0 0 1px rgba(14, 165, 233, 0.07);
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
  max-height: 90vh;
  position: relative;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

:global(.dark-mode) .luxury-modal-content,
:global(.theme-dark) .luxury-modal-content {
  background: #0f172a !important;
  border-color: rgba(255, 255, 255, 0.1) !important;
  box-shadow: 0 30px 70px rgba(0, 0, 0, 0.6), 0 0 0 1px rgba(14, 165, 233, 0.15) !important;
}

/* ── Header ── */
.modal-header-premium {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 24px;
  padding-bottom: 20px;
  border-bottom: 1px solid var(--r-border, #f1f5f9);
}

:global(.dark-mode) .modal-header-premium,
:global(.theme-dark) .modal-header-premium {
  border-bottom-color: rgba(255, 255, 255, 0.08) !important;
}

.modal-title-group {
  display: flex;
  align-items: flex-start;
  gap: 14px;
}

.icon-box-themed {
  width: 48px;
  height: 48px;
  min-width: 48px;
  border-radius: 14px;
  background: linear-gradient(135deg, rgba(14, 165, 233, 0.15), rgba(6, 182, 212, 0.08));
  color: var(--accent, #0ea5e9);
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid rgba(14, 165, 233, 0.2);
  box-shadow: 0 4px 14px rgba(14, 165, 233, 0.18);
}

.modal-title-text {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.modal-badge {
  display: inline-flex;
  align-items: center;
  padding: 2px 10px;
  border-radius: 100px;
  background: linear-gradient(135deg, rgba(14, 165, 233, 0.12), rgba(6, 182, 212, 0.08));
  border: 1px solid rgba(14, 165, 233, 0.2);
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 1.5px;
  color: var(--accent, #0ea5e9);
  width: fit-content;
  margin-bottom: 2px;
}

.celestial-modal-title {
  font-size: 20px;
  font-weight: 900;
  color: var(--r-text-main, #0f172a);
  letter-spacing: -0.5px;
  margin: 0;
  line-height: 1.2;
}

:global(.dark-mode) .celestial-modal-title,
:global(.theme-dark) .celestial-modal-title {
  color: #f8fafc !important;
}

.modal-intro {
  font-size: 12.5px;
  color: var(--r-text-sub, #64748b);
  margin: 0;
  font-weight: 500;
}

:global(.dark-mode) .modal-intro,
:global(.theme-dark) .modal-intro {
  color: #94a3b8 !important;
}

.close-lux-btn {
  background: rgba(0, 0, 0, 0.04);
  border: 1px solid var(--r-border, #e2e8f0);
  width: 32px;
  height: 32px;
  min-width: 32px;
  border-radius: 10px;
  color: var(--r-text-sub, #64748b);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.25s;
  flex-shrink: 0;
}

:global(.dark-mode) .close-lux-btn,
:global(.theme-dark) .close-lux-btn {
  background: rgba(255, 255, 255, 0.06) !important;
  border-color: rgba(255, 255, 255, 0.1) !important;
  color: #94a3b8 !important;
}

.close-lux-btn:hover {
  background: rgba(239, 68, 68, 0.08);
  border-color: rgba(239, 68, 68, 0.3);
  color: #ef4444;
  transform: rotate(90deg);
}

:global(.dark-mode) .close-lux-btn:hover,
:global(.theme-dark) .close-lux-btn:hover {
  background: rgba(239, 68, 68, 0.15) !important;
  color: #ef4444 !important;
}

/* ── Form Inputs ── */
.modal-form-wrapper {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.modal-body-premium {
  display: flex;
  flex-direction: column;
}

.lux-input-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: 100%;
}

.mt-16 {
  margin-top: 16px;
}

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

.required-star {
  color: var(--accent, #0ea5e9);
}

.lux-input-wrap {
  display: flex;
  align-items: center;
  gap: 10px;
  background: rgba(0, 0, 0, 0.02);
  padding: 6px 14px 6px 6px;
  border-radius: 14px;
  border: 1px solid var(--r-border, #e2e8f0);
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
  box-sizing: border-box;
  width: 100%;
}

:global(.dark-mode) .lux-input-wrap,
:global(.theme-dark) .lux-input-wrap {
  background: rgba(255, 255, 255, 0.03) !important;
  border-color: rgba(255, 255, 255, 0.1) !important;
}

.lux-input-wrap:focus-within {
  background: var(--card-bg, #ffffff);
  border-color: var(--accent, #0ea5e9) !important;
  box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.15), 0 6px 14px -4px rgba(14, 165, 233, 0.15);
  transform: translateY(-1px);
}

:global(.dark-mode) .lux-input-wrap:focus-within,
:global(.theme-dark) .lux-input-wrap:focus-within {
  background: rgba(15, 23, 42, 0.95) !important;
  border-color: #0ea5e9 !important;
  box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.25) !important;
}

.textarea-wrap {
  align-items: flex-start;
  padding-top: 8px;
}

.input-icon-box {
  width: 34px;
  height: 34px;
  border-radius: 10px;
  background: rgba(14, 165, 233, 0.08);
  color: var(--accent, #0ea5e9);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.textarea-icon {
  margin-top: 2px;
}

.lux-input-wrap:focus-within .input-icon-box {
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  color: white;
  box-shadow: 0 4px 10px -3px rgba(14, 165, 233, 0.5);
}

.lux-input-text {
  flex: 1;
  background: transparent !important;
  background-color: transparent !important;
  border: none !important;
  outline: none !important;
  color: var(--r-text-main, #0f172a) !important;
  font-weight: 600;
  font-size: 14px;
  min-width: 0;
  padding: 8px 0;
}

:global(.dark-mode) .lux-input-text,
:global(.theme-dark) .lux-input-text {
  color: #f8fafc !important;
  background: transparent !important;
  background-color: transparent !important;
}

.lux-input-text::placeholder {
  color: #94a3b8 !important;
  font-weight: 400;
}

.lux-textarea {
  min-height: 75px;
  resize: vertical;
  padding-top: 4px;
  line-height: 1.5;
  font-family: inherit;
}

/* ── Footer ── */
.modal-footer-lux {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 12px;
  margin-top: 28px;
  padding-top: 20px;
  border-top: 1px solid var(--r-border, #f1f5f9);
}

:global(.dark-mode) .modal-footer-lux,
:global(.theme-dark) .modal-footer-lux {
  border-top-color: rgba(255, 255, 255, 0.08) !important;
}

.btn-cancel {
  background: transparent;
  border: 1px solid var(--r-border, #cbd5e1);
  padding: 10px 22px;
  border-radius: 12px;
  font-weight: 700;
  font-size: 13px;
  cursor: pointer;
  color: var(--r-text-sub, #475569);
  transition: all 0.2s;
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
  height: 42px;
  padding: 0 24px;
  border-radius: 12px;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  font-size: 13px;
  position: relative;
  overflow: hidden;
}

.btn-premium.btn-accent:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 28px -6px rgba(14, 165, 233, 0.5), 0 1px 0 #0891b2;
}

.btn-premium.btn-accent:active {
  transform: translateY(1px);
}

.btn-premium.btn-accent:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
  box-shadow: none;
}

.spin { animation: rotate 2s linear infinite; }
@keyframes rotate { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
.anim-scale-up { animation: scaleUp 0.35s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes scaleUp { from { opacity: 0; transform: scale(0.96) translateY(8px); } to { opacity: 1; transform: scale(1) translateY(0); } }
</style>
