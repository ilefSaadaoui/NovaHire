<template>
  <Teleport to="body">
    <Transition name="modal-fade">
      <div v-if="modalStore.isOpen" class="premium-modal-overlay" @click.self="handleOutsideClick">
        <div class="modal-window-wrapper" :class="{ 'shake-anim': isShaking }">
          <div class="premium-modal-card" :class="[modalStore.type]">
            <!-- Decorative Glow Background -->
            <div class="modal-glow"></div>
            
            <div class="modal-header">
              <div class="modal-icon-box">
                <!-- Danger: red triangle -->
                <svg v-if="modalStore.type === 'danger'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
                <!-- Warning: orange alert bell -->
                <svg v-else-if="modalStore.type === 'warning'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 0 1-3.46 0"/></svg>
                <!-- Default: info circle -->
                <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
              </div>
              <h3 class="modal-title">{{ modalStore.title }}</h3>
            </div>

            <div class="modal-body">
              <p>{{ modalStore.message }}</p>
            </div>

            <div class="modal-footer">
              <button class="btn-modal-cancel" @click="modalStore.handleCancel">
                {{ modalStore.cancelText }}
              </button>
              <button class="btn-modal-confirm" :class="modalStore.type" @click="modalStore.handleConfirm">
                <span class="btn-text">{{ modalStore.confirmText }}</span>
                <div class="btn-glow"></div>
              </button>
            </div>

            <div class="modal-corner c-tl"></div>
            <div class="modal-corner c-tr"></div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup>
import { ref } from 'vue'
import { useModalStore } from '@/stores/modalStore'

const modalStore = useModalStore()
const isShaking = ref(false)

const handleOutsideClick = () => {
  isShaking.value = true
  setTimeout(() => { isShaking.value = false }, 500)
}
</script>

<style scoped>
.premium-modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10000;
  padding: 20px;
  backdrop-filter: blur(4px);
  pointer-events: auto !important;
}

.modal-window-wrapper {
  perspective: 1000px;
  width: 100%;
  max-width: 440px;
}

/* ── Light Mode card ── */
.premium-modal-card {
  position: relative;
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 28px;
  padding: 40px;
  box-shadow: 0 25px 60px -12px rgba(0, 0, 0, 0.15);
  display: flex;
  flex-direction: column;
  gap: 24px;
  overflow: hidden;
}

/* ── Dark Mode card (Moved to unscoped block) ── */

.modal-glow {
  position: absolute;
  top: -50%;
  left: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(circle at center, var(--accent-soft, rgba(0, 167, 225, 0.08)) 0%, transparent 70%);
  pointer-events: none;
  opacity: 0.6;
}

.modal-header {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
  text-align: center;
}

/* ── Icon box ── */
.modal-icon-box {
  width: 68px;
  height: 68px;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  background: #f1f5f9;
}

.modal-icon-box svg { width: 28px; height: 28px; }

/* Danger → red */
.danger .modal-icon-box {
  color: #ef4444;
  background: rgba(239, 68, 68, 0.1);
  border: 1px solid rgba(239, 68, 68, 0.2);
}

/* Warning → orange */
.warning .modal-icon-box {
  color: #f59e0b;
  background: rgba(245, 158, 11, 0.1);
  border: 1px solid rgba(245, 158, 11, 0.2);
}

/* Info → accent blue */
.info .modal-icon-box {
  color: var(--accent-color, #0ea5e9);
  background: rgba(14, 165, 233, 0.1);
  border: 1px solid rgba(14, 165, 233, 0.2);
}

/* ── Title ── */
.modal-title {
  font-size: 20px;
  font-weight: 800;
  color: #0f172a;
  letter-spacing: -0.5px;
  line-height: 1.3;
}

/* ── Body ── */
.modal-body { text-align: center; }

.modal-body p {
  font-size: 15px;
  color: #475569;
  line-height: 1.6;
  margin: 0;
}

/* ── Footer ── */
.modal-footer {
  display: grid;
  grid-template-columns: 1fr 1.2fr;
  gap: 12px;
  margin-top: 4px;
}

.btn-modal-cancel {
  padding: 13px 20px;
  border-radius: 14px;
  border: 1.5px solid #e2e8f0;
  background: transparent;
  color: #64748b;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-modal-cancel:hover {
  background: #f8fafc;
  color: #1e293b;
  border-color: #cbd5e1;
}

.btn-modal-confirm {
  position: relative;
  padding: 13px 20px;
  border-radius: 14px;
  border: none;
  background: var(--accent-color, #0ea5e9);
  color: white;
  font-size: 14px;
  font-weight: 800;
  cursor: pointer;
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.btn-modal-confirm.danger  { background: #ef4444; }
.btn-modal-confirm.warning { background: linear-gradient(135deg, #f59e0b, #d97706); }

.btn-modal-confirm:hover {
  transform: translateY(-2px);
  filter: brightness(1.1);
  box-shadow: 0 8px 20px -5px rgba(0,0,0,0.25);
}

.btn-modal-confirm:active { transform: translateY(1px) scale(0.98); }

/* ── Corner decorations ── */
.modal-corner {
  position: absolute;
  width: 16px;
  height: 16px;
  border: 2px solid rgba(var(--accent-rgb, 0, 167, 225), 0.2);
  opacity: 0.5;
}
.c-tl { top: 18px; left: 18px; border-right: none; border-bottom: none; border-top-left-radius: 6px; }
.c-tr { top: 18px; right: 18px; border-left: none; border-bottom: none; border-top-right-radius: 6px; }

/* ── Animations ── */
.modal-fade-enter-active { transition: all 0.35s ease-out; }
.modal-fade-leave-active  { transition: all 0.25s ease-in; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }

.modal-fade-enter-active .modal-window-wrapper {
  animation: modal-in 0.45s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.modal-fade-leave-active .modal-window-wrapper {
  animation: modal-out 0.25s ease-in forwards;
}

@keyframes modal-in {
  0%   { transform: scale(0.85) translateY(30px); opacity: 0; }
  100% { transform: scale(1) translateY(0); opacity: 1; }
}
@keyframes modal-out {
  0%   { transform: scale(1) translateY(0); opacity: 1; }
  100% { transform: scale(0.92) translateY(16px); opacity: 0; }
}

.shake-anim {
  animation: shake 0.45s cubic-bezier(.36,.07,.19,.97) both;
}
@keyframes shake {
  10%, 90% { transform: translate3d(-2px, 0, 0); }
  20%, 80% { transform: translate3d(3px, 0, 0); }
  30%, 50%, 70% { transform: translate3d(-4px, 0, 0); }
  40%, 60% { transform: translate3d(4px, 0, 0); }
}
</style>

<style>
/* Unscoped Dark Mode Styles for Teleported Modal */
body.dark-mode .premium-modal-overlay {
  pointer-events: auto !important;
}

body.dark-mode .premium-modal-card {
  background: #0f172a;
  border-color: rgba(255, 255, 255, 0.08);
  box-shadow: 0 25px 60px -12px rgba(0, 0, 0, 0.6);
  pointer-events: auto !important;
}

body.dark-mode .modal-icon-box {
  background: rgba(255, 255, 255, 0.06);
}

body.dark-mode .modal-title {
  color: #f8fafc;
}

body.dark-mode .modal-body p {
  color: #94a3b8;
}

body.dark-mode .btn-modal-cancel,
body.dark-mode .btn-modal-confirm {
  pointer-events: auto !important;
}

body.dark-mode .btn-modal-cancel {
  background: rgba(255, 255, 255, 0.05);
  color: #e2e8f0;
  box-shadow: inset 0 0 0 1px rgba(0, 0, 0, 0.1);
}

body.dark-mode .btn-modal-cancel:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
}
</style>
