<template>
  <div class="tabs-nav-celestial">
    <div class="tabs-list-wrapper">
      <button 
        v-for="t in tabs" 
        :key="t.id" 
        class="tab-item-pro" 
        :class="{ 'is-active': modelValue === t.id }" 
        @click="$emit('update:modelValue', t.id)"
      >
        <component v-if="t.icon" :is="t.icon" :size="16" class="tab-icon" />
        <span class="tab-label">{{ t.label }}</span>
        
        <div v-if="modelValue === t.id" class="active-glow"></div>
      </button>
    </div>
    <div class="tabs-border-bottom"></div>
  </div>
</template>

<script setup>
defineProps({
  tabs: Array,
  modelValue: String
})

defineEmits(['update:modelValue'])
</script>

<style scoped>
.tabs-nav-celestial {
  position: relative;
  margin-bottom: 32px;
}

.tabs-list-wrapper {
  display: flex;
  gap: 4px;
  overflow-x: auto;
  padding-bottom: 8px;
  width: 100%;
  scrollbar-width: none; /* Firefox */
}

.tabs-list-wrapper::-webkit-scrollbar {
  display: none;
}

.tab-item-pro {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  border: 1px solid transparent;
  background: transparent;
  font-size: 13px;
  font-weight: 700;
  color: #64748b;
  cursor: pointer;
  position: relative;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border-radius: 12px;
  white-space: nowrap;
  flex-shrink: 0;
}

.tab-item-pro:hover {
  background: #f1f5f9;
  color: #1e293b;
}

.tab-item-pro.is-active {
  color: var(--accent);
  background: var(--accent-soft);
}

.tab-icon {
  transition: transform 0.3s;
}

.tab-item-pro.is-active .tab-icon {
  transform: scale(1.2);
}

.active-glow {
  position: absolute;
  bottom: 0;
  left: 20%;
  right: 20%;
  height: 3px;
  background: var(--accent);
  border-radius: 100px 100px 0 0;
  box-shadow: 0 -2px 10px var(--accent-soft);
}

.tabs-border-bottom {
  position: absolute;
  bottom: 0;
  left: 0;
  right: 0;
  height: 1px;
  background: #e2e8f0;
  z-index: -1;
}

.tab-label {
  position: relative;
  z-index: 1;
}

/* Dark Mode Overrides - Premium Enhancement */
.dark-mode .tab-item-pro:hover {
  background: var(--accent-soft);
  color: #ffffff;
}

.dark-mode .tab-item-pro.is-active {
  background: var(--accent-soft);
  color: var(--accent);
  box-shadow: inset 0 0 10px var(--accent-glow);
}

.dark-mode .active-glow {
  background: var(--accent);
  box-shadow: 0 -2px 15px var(--accent);
}

.dark-mode .tabs-border-bottom {
  background: rgba(255, 255, 255, 0.05);
}
</style>

