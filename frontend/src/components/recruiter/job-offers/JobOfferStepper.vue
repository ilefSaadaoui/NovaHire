<template>
  <div class="r-stepper-container anim-reveal-down" style="animation-delay: 0.1s">
    <div class="stepper-progress-line">
      <div 
        class="progress-fill" 
        :style="{ width: (currentStep / (steps.length - 1)) * 100 + '%' }"
      ></div>
    </div>
    
    <div class="r-stepper-inner">
      <div 
        v-for="(s, i) in steps" 
        :key="i" 
        class="r-step-pill-lux" 
        :class="{ 
          active: currentStep === i, 
          done: currentStep > i,
          upcoming: currentStep < i
        }" 
        @click="$emit('update:currentStep', i)"
      >
        <div class="step-icon-lux">
          <transition name="icon-swap" mode="out-in">
            <svg v-if="currentStep > i" key="check" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3.5">
              <polyline points="20 6 9 17 4 12"/>
            </svg>
            <span v-else key="num">{{ i + 1 }}</span>
          </transition>
          <div class="icon-ring"></div>
        </div>
        <div class="step-text-lux">
          <span class="step-label-lux">{{ s.label }}</span>
          <span class="step-status-lux">{{ getStatusLabel(i) }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const props = defineProps({
  steps: Array,
  currentStep: Number
})

defineEmits(['update:currentStep'])

const getStatusLabel = (i) => {
  if (props.currentStep === i) return 'En cours'
  if (props.currentStep > i) return 'Complété'
  return 'À venir'
}
</script>

<style scoped>
.r-stepper-container {
  position: relative;
  margin-bottom: 48px;
  padding: 0 10px;
}

.r-stepper-inner {
  display: flex;
  justify-content: space-between;
  position: relative;
  z-index: 2;
}

.stepper-progress-line {
  position: absolute;
  top: 26px; /* Center of icon */
  left: 50px;
  right: 50px;
  height: 4px;
  background: var(--r-border);
  border-radius: 4px;
  z-index: 1;
  overflow: hidden;
}

.progress-fill {
  height: 100%;
  background: var(--accent-grad);
  transition: width 0.6s cubic-bezier(0.2, 0.8, 0.2, 1);
  box-shadow: 0 0 10px var(--accent-soft);
}

.r-step-pill-lux {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  transition: 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  min-width: 100px;
}

.step-icon-lux {
  width: 52px;
  height: 52px;
  border-radius: 18px;
  background: var(--r-main-bg);
  border: 2px solid var(--r-border);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  font-weight: 900;
  color: var(--r-text-sub);
  position: relative;
  z-index: 2;
  transition: 0.3s;
}

.icon-ring {
  position: absolute;
  inset: -6px;
  border: 2px solid transparent;
  border-radius: 22px;
  transition: 0.4s;
  opacity: 0;
}

.r-step-pill-lux.active .step-icon-lux {
  background: var(--accent-grad);
  border-color: transparent;
  color: white;
  transform: scale(1.1);
  box-shadow: 0 10px 25px var(--accent-soft);
}

.r-step-pill-lux.active .icon-ring {
  border-color: var(--accent);
  opacity: 0.3;
  transform: scale(1.1);
  animation: pulse-ring 2s infinite;
}

.r-step-pill-lux.done .step-icon-lux {
  background: #10b981;
  border-color: #10b981;
  color: white;
}

.r-step-pill-lux.upcoming .step-icon-lux {
  background: var(--r-main-bg);
  opacity: 0.8;
}

.step-icon-lux svg { width: 22px; height: 22px; }

.step-text-lux {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.step-label-lux {
  font-size: 14px;
  font-weight: 800;
  color: var(--r-text-main);
  transition: 0.3s;
}

.step-status-lux {
  font-size: 11px;
  font-weight: 700;
  color: var(--r-text-sub);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-top: 2px;
}

.r-step-pill-lux.active .step-label-lux { color: var(--accent); }
.r-step-pill-lux.done .step-label-lux { color: #10b981; }

/* Animations */
.icon-swap-enter-active, .icon-swap-leave-active {
  transition: all 0.3s cubic-bezier(0.2, 0.8, 0.2, 1);
}
.icon-swap-enter-from { opacity: 0; transform: scale(0.5) rotate(-45deg); }
.icon-swap-leave-to { opacity: 0; transform: scale(0.5) rotate(45deg); }

@keyframes pulse-ring {
  0% { transform: scale(1.1); opacity: 0.3; }
  50% { transform: scale(1.25); opacity: 0; }
  100% { transform: scale(1.1); opacity: 0.3; }
}

.anim-reveal-down {
  animation: revealDown 0.6s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes revealDown {
  from { opacity: 0; transform: translateY(-20px); }
  to { opacity: 1; transform: translateY(0); }
}

@media (max-width: 600px) {
  .step-status-lux { display: none; }
  .stepper-progress-line { left: 30px; right: 30px; }
  .r-step-pill-lux { min-width: 60px; }
}
</style>
