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
                <svg v-if="modalStore.type === 'danger'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
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
              <button class="btn-modal-confirm" @click="modalStore.handleConfirm">
                <span class="btn-text">{{ modalStore.confirmText }}</span>
                <div class="btn-glow"></div>
              </button>
            </div>

            <!-- Subtle border laser effect or decorative elements -->
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
  setTimeout(() => {
    isShaking.value = false
  }, 500)
}
</script>

<style scoped>
.premium-modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(13, 24, 33, 0.9);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10000;
  padding: 20px;
}

.modal-window-wrapper {
  perspective: 1000px;
  width: 100%;
  max-width: 440px;
}

.premium-modal-card {
  position: relative;
  background: rgba(13, 24, 33, 0.95);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 32px;
  padding: 40px;
  box-shadow: 
    0 25px 50px -12px rgba(0, 0, 0, 0.2),
    inset 0 0 0 1px rgba(255, 255, 255, 0.2);
  display: flex;
  flex-direction: column;
  gap: 24px;
  overflow: hidden;
}

.modal-glow {
  position: absolute;
  top: -50%;
  left: -50%;
  width: 200%;
  height: 200%;
  background: radial-gradient(circle at center, var(--accent-soft, rgba(0, 167, 225, 0.1)) 0%, transparent 70%);
  pointer-events: none;
  opacity: 0.5;
}

.modal-header {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
  text-align: center;
}

.modal-icon-box {
  width: 64px;
  height: 64px;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--bg-main);
  box-shadow: 0 10px 20px rgba(0,0,0,0.05);
  position: relative;
}

.danger .modal-icon-box { color: #ef4444; border: 1px solid rgba(239, 68, 68, 0.1); }
.info .modal-icon-box { color: var(--accent-color); border: 1px solid var(--accent-soft); }

.modal-title {
  font-size: 22px;
  font-weight: 800;
  color: var(--text-main);
  letter-spacing: -0.5px;
}

.modal-body {
  text-align: center;
}

.modal-body p {
  font-size: 15px;
  color: var(--text-muted);
  line-height: 1.6;
  font-weight: 500;
}

.modal-footer {
  display: grid;
  grid-template-columns: 1fr 1.2fr;
  gap: 16px;
  margin-top: 8px;
}

.btn-modal-cancel {
  padding: 14px 24px;
  border-radius: 16px;
  border: 1.5px solid var(--border-thin);
  background: transparent;
  color: var(--text-muted);
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-modal-cancel:hover {
  background: rgba(0,0,0,0.03);
  color: var(--text-main);
  border-color: rgba(0,0,0,0.1);
}

.btn-modal-confirm {
  position: relative;
  padding: 14px 24px;
  border-radius: 16px;
  border: none;
  background: var(--accent-color);
  color: white;
  font-size: 14px;
  font-weight: 800;
  cursor: pointer;
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.danger .btn-modal-confirm { background: #ef4444; }

.btn-modal-confirm:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 20px -5px var(--accent-soft);
}

.btn-modal-confirm:active {
  transform: translateY(1px) scale(0.98);
}

/* Animations */
.modal-fade-enter-active {
  transition: all 0.4s ease-out;
}
.modal-fade-leave-active {
  transition: all 0.3s ease-in;
}

.modal-fade-enter-from {
  opacity: 0;
}
.modal-fade-leave-to {
  opacity: 0;
}

.modal-fade-enter-active .modal-window-wrapper {
  animation: modal-in 0.5s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.modal-fade-leave-active .modal-window-wrapper {
  animation: modal-out 0.3s ease-in forwards;
}

@keyframes modal-in {
  0% { transform: scale(0.8) translateY(40px); opacity: 0; }
  100% { transform: scale(1) translateY(0); opacity: 1; }
}

@keyframes modal-out {
  0% { transform: scale(1) translateY(0); opacity: 1; }
  100% { transform: scale(0.9) translateY(20px); opacity: 0; }
}

.shake-anim {
  animation: shake 0.5s cubic-bezier(.36,.07,.19,.97) both;
}

@keyframes shake {
  10%, 90% { transform: translate3d(-1px, 0, 0); }
  20%, 80% { transform: translate3d(2px, 0, 0); }
  30%, 50%, 70% { transform: translate3d(-4px, 0, 0); }
  40%, 60% { transform: translate3d(4px, 0, 0); }
}

/* Dark mode overrides */
.dark-mode .premium-modal-card {
  background: rgba(15, 23, 42, 0.9);
  border-color: rgba(255, 255, 255, 0.08);
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.6);
}

.dark-mode .btn-modal-cancel:hover { background: rgba(255,255,255,0.05); }

/* Corner decorations */
.modal-corner {
  position: absolute;
  width: 20px;
  height: 20px;
  border: 2px solid var(--accent-soft);
  opacity: 0.3;
}
.c-tl { top: 20px; left: 20px; border-right: none; border-bottom: none; border-top-left-radius: 8px; }
.c-tr { top: 20px; right: 20px; border-left: none; border-bottom: none; border-top-right-radius: 8px; }
</style>
