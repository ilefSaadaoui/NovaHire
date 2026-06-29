<template>
  <div class="toast-container" role="region" aria-label="Notifications">
    <transition-group name="toast" tag="div" class="toast-stack">
      <div
        v-for="toast in toastStore.toasts"
        :key="toast.id"
        class="premium-toast"
        :class="[toast.type]"
        :role="toast.type === 'error' ? 'alert' : 'status'"
        :aria-live="toast.type === 'error' ? 'assertive' : 'polite'"
        @click="toastStore.remove(toast.id)"
      >
        <!-- Barre colorée gauche via classe CSS -->

        <!-- Icône correcte selon le type -->
        <div class="toast-icon">
          <CheckCircle   v-if="toast.type === 'success'" :size="20" stroke-width="2.5" />
          <XCircle       v-else-if="toast.type === 'error'"   :size="20" stroke-width="2.5" />
          <AlertTriangle v-else-if="toast.type === 'warning'" :size="20" stroke-width="2.5" />
          <Info          v-else                                :size="20" stroke-width="2.5" />
        </div>

        <!-- Message -->
        <div class="toast-content">
          <span class="toast-label">{{ typeLabel(toast.type) }}</span>
          <span class="toast-msg">{{ toast.message }}</span>
        </div>

        <!-- Bouton fermeture — @click.stop évite le double remove -->
        <button
          class="toast-close"
          :aria-label="'Fermer la notification'"
          @click.stop="toastStore.remove(toast.id)"
        >
          <X :size="14" stroke-width="3" />
        </button>

        <!-- Barre de progression du timer -->
        <div
          v-if="toast.duration > 0"
          class="toast-progress"
          :style="{ animationDuration: toast.duration + 'ms' }"
        ></div>
      </div>
    </transition-group>
  </div>
</template>

<script setup>
import { CheckCircle, XCircle, AlertTriangle, Info, X } from 'lucide-vue-next'
import { useToastStore } from '@/stores/toastStore'

const toastStore = useToastStore()

function typeLabel(type) {
  const labels = {
    success: 'Succès',
    error:   'Erreur',
    warning: 'Attention',
    info:    'Information'
  }
  return labels[type] || 'Information'
}
</script>

<style scoped>
/* ─── Container ─── */
.toast-container {
  position: fixed;
  top: 28px;
  right: 28px;
  z-index: 9999;
  pointer-events: none;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 10px;
}

.toast-stack {
  display: flex;
  flex-direction: column;
  gap: 10px;
  align-items: flex-end;
}

/* ─── Toast card ─── */
.premium-toast {
  pointer-events: auto;
  min-width: 320px;
  max-width: 460px;
  background: var(--glass-bg, rgba(255, 255, 255, 0.7));
  backdrop-filter: blur(25px) saturate(200%);
  -webkit-backdrop-filter: blur(25px) saturate(200%);
  border: 1px solid var(--glass-border-color, rgba(255, 255, 255, 0.5));
  border-radius: 20px;
  padding: 16px 20px;
  display: flex;
  align-items: flex-start;
  gap: 14px;
  box-shadow: var(--premium-shadow, 0 8px 32px rgba(31, 38, 135, 0.07));
  cursor: pointer;
  transition: transform 0.4s cubic-bezier(0.16, 1, 0.3, 1), box-shadow 0.4s ease, border-color 0.4s ease;
  position: relative;
  overflow: hidden;
}

.premium-toast:hover {
  transform: translateX(-4px) translateY(-2px);
  border-color: var(--accent-color);
  box-shadow: 0 20px 40px rgba(0,0,0,0.1), 0 0 20px var(--accent-soft);
}

/* ─── Barre colorée gauche (par type) ─── */
.premium-toast::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  border-radius: 18px 0 0 18px;
}

.success::before { background: #10b981; }
.error::before   { background: #ef4444; }
.warning::before { background: #f59e0b; }
.info::before    { background: #3b82f6; }

/* ─── Icône ─── */
.toast-icon {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  margin-top: 1px;
}

.success .toast-icon { background: rgba(16, 185, 129, 0.12); color: #10b981; }
.error   .toast-icon { background: rgba(239, 68,  68,  0.12); color: #ef4444; }
.warning .toast-icon { background: rgba(245, 158, 11,  0.12); color: #f59e0b; }
.info    .toast-icon { background: rgba(59,  130, 246, 0.12); color: #3b82f6; }

/* ─── Contenu ─── */
.toast-content {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.toast-label {
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  line-height: 1;
}

.success .toast-label { color: #10b981; }
.error   .toast-label { color: #ef4444; }
.warning .toast-label { color: #f59e0b; }
.info    .toast-label { color: #3b82f6; }

.toast-msg {
  font-size: 13.5px;
  font-weight: 600;
  color: var(--text-main, #1e293b);
  line-height: 1.45;
  word-break: break-word;
}

/* ─── Bouton fermeture ─── */
.toast-close {
  background: transparent;
  border: none;
  color: var(--text-muted, #94a3b8);
  width: 26px;
  height: 26px;
  border-radius: 7px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: background 0.2s, color 0.2s;
  flex-shrink: 0;
  margin-top: 2px;
}

.toast-close:hover {
  background: rgba(var(--accent-rgb, 0,0,0), 0.07);
  color: var(--text-main, #1e293b);
}

/* ─── Barre de progression ─── */
.toast-progress {
  position: absolute;
  bottom: 0;
  left: 0;
  height: 3px;
  width: 100%;
  border-radius: 0 0 18px 18px;
  animation: progress-shrink linear forwards;
  transform-origin: left;
}

.success .toast-progress { background: #10b981; }
.error   .toast-progress { background: #ef4444; }
.warning .toast-progress { background: #f59e0b; }
.info    .toast-progress { background: #3b82f6; }

@keyframes progress-shrink {
  from { transform: scaleX(1); }
  to   { transform: scaleX(0); }
}

/* ─── Animations entrée / sortie ─── */
.toast-enter-active {
  animation: toast-in 0.45s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}
.toast-leave-active {
  animation: toast-out 0.35s cubic-bezier(0.4, 0, 1, 1) forwards;
  position: absolute;
}
.toast-move {
  transition: transform 0.4s ease;
}

@keyframes toast-in {
  0%   { opacity: 0; transform: translateX(110%) scale(0.92); }
  100% { opacity: 1; transform: translateX(0)    scale(1);    }
}

@keyframes toast-out {
  0%   { opacity: 1; transform: translateX(0)   scale(1);    }
  100% { opacity: 0; transform: translateX(60%) scale(0.88); }
}

/* ─── Dark mode ─── */
:global(.dark-mode) .premium-toast {
  background: var(--glass-bg, rgba(15, 23, 42, 0.5));
  border-color: var(--glass-border-color, rgba(255, 255, 255, 0.08));
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.4);
}

:global(.dark-mode) .premium-toast:hover {
  border-color: var(--accent-color);
  box-shadow: 0 20px 40px rgba(0,0,0,0.6), 0 0 20px var(--accent-soft);
}

:global(.dark-mode) .toast-msg {
  color: var(--text-main, #e2e8f0);
}

:global(.dark-mode) .toast-close {
  color: var(--text-muted, #64748b);
}

:global(.dark-mode) .toast-close:hover {
  background: rgba(255, 255, 255, 0.08);
  color: var(--text-main, #e2e8f0);
}

/* ─── Responsive mobile ─── */
@media (max-width: 480px) {
  .toast-container {
    top: 16px;
    right: 12px;
    left: 12px;
    align-items: stretch;
  }
  .premium-toast {
    min-width: unset;
    max-width: unset;
    width: 100%;
  }
}
</style>
