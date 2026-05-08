<template>
  <div class="toast-container">
    <transition-group name="toast" tag="div" class="toast-stack">
      <div 
        v-for="toast in toastStore.toasts" 
        :key="toast.id" 
        class="premium-toast" 
        :class="[toast.type]"
        @click="toastStore.remove(toast.id)"
      >
        <div class="toast-icon">
          <CheckCircle v-if="toast.type === 'success'" :size="20" stroke-width="3" />
          <AlertCircle v-else-if="toast.type === 'error'" :size="20" stroke-width="3" />
          <AlertCircle v-else-if="toast.type === 'warning'" :size="20" stroke-width="3" />
          <Info v-else :size="20" stroke-width="3" />
        </div>
        <div class="toast-content">
          <span class="toast-msg">{{ toast.message }}</span>
        </div>
        <button class="toast-close">
          <X :size="14" stroke-width="3" />
        </button>
      </div>
    </transition-group>
  </div>
</template>

<script setup>
import { CheckCircle, AlertCircle, Info, X } from 'lucide-vue-next'
import { useToastStore } from '@/stores/toastStore'
const toastStore = useToastStore()
</script>

<style scoped>
.toast-container {
  position: fixed;
  top: 32px;
  right: 32px;
  z-index: 9999;
  pointer-events: none;
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: 12px;
}

.toast-stack {
  display: flex;
  flex-direction: column;
  gap: 12px;
  align-items: flex-end;
}

.premium-toast {
  pointer-events: auto;
  min-width: 320px;
  max-width: 450px;
  background: var(--glass-bg-base);
  backdrop-filter: blur(20px) saturate(180%);
  -webkit-backdrop-filter: blur(20px) saturate(180%);
  border: 1px solid var(--border-thin);
  border-radius: 20px;
  padding: 18px 24px;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: var(--glass-shadow);
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  position: relative;
  overflow: hidden;
}

.premium-toast::before {
  content: '';
  position: absolute;
  left: 0;
  top: 0;
  bottom: 0;
  width: 4px;
  background: var(--accent-color);
}

.premium-toast:hover {
  transform: translateX(-6px) translateY(-2px);
  background: var(--glass-bg-hover);
  box-shadow: var(--glass-shadow), 0 20px 50px rgba(0,0,0,0.1);
}

.toast-icon {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.success .toast-icon { background: rgba(16, 185, 129, 0.12); color: #10b981; }
.error .toast-icon { background: rgba(239, 68, 68, 0.12); color: #ef4444; }
.warning .toast-icon { background: rgba(245, 158, 11, 0.12); color: #f59e0b; }
.info .toast-icon { background: rgba(0, 167, 225, 0.12); color: #00A7E1; }

/* Role-based branding override for info/success if desirable */
.info.premium-toast::before { background: var(--accent-color); }

.toast-icon svg { width: 20px; height: 20px; }

.toast-content { flex: 1; min-width: 0; }
.toast-msg {
  font-size: 14px;
  font-weight: 700;
  color: var(--text-main);
  line-height: 1.4;
}

.toast-close {
  background: transparent;
  border: none;
  color: var(--text-muted);
  width: 28px;
  height: 28px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
  opacity: 0.5;
}

.toast-close:hover {
  background: rgba(0,0,0,0.05);
  opacity: 1;
}

.toast-close svg { width: 14px; height: 14px; }

/* Animations */
.toast-enter-active {
  animation: toast-in 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}
.toast-leave-active {
  animation: toast-out 0.4s cubic-bezier(0.4, 0, 1, 1) forwards;
  position: absolute;
}
.toast-move {
  transition: transform 0.4s ease;
}

@keyframes toast-in {
  0% { opacity: 0; transform: translateX(100%) scale(0.9); }
  100% { opacity: 1; transform: translateX(0) scale(1); }
}

@keyframes toast-out {
  0% { opacity: 1; transform: translateX(0) scale(1); }
  100% { opacity: 0; transform: translateX(50%) scale(0.8); }
}

.dark-mode .premium-toast {
  background: var(--glass-bg-base);
  border-color: var(--border-glass);
}
 
.dark-mode .toast-close:hover { background: rgba(255,255,255,0.08); }
</style>
