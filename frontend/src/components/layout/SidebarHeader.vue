<template>
  <div class="sidebar-header" :class="{ collapsed: isCollapsed }">
    <div class="header-content" v-if="!isCollapsed">
      <button class="brand-trigger" @click="$emit('toggle-collapse')">
        <div class="logo-wrap">
          <img :src="logoUrl" alt="NovaHire" class="brand-logo" />
        </div>
        <div class="badge-role" :style="roleStyle">
          <span class="dot"></span>
          {{ roleLabel }}
        </div>
      </button>
    </div>
    
    <button
      v-else
      class="brand-trigger-mini"
      aria-label="Expand sidebar"
      @click="$emit('toggle-collapse')"
    >
      <svg class="mini-toggle-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
        <path d="M9 18l6-6-6-6" />
      </svg>
    </button>
    
    <div v-if="!isCollapsed" class="header-separator"></div>
  </div>
</template>

<script>
import logoUrl from '@/assets/Logo_NovaHire.png'

export default {
  name: 'SidebarHeader',
  emits: ['toggle-collapse'],
  props: {
    isCollapsed: { type: Boolean, default: false },
    collapsible: { type: Boolean, default: false },
    roleLabel: { type: String, default: 'Recrutement' },
    accentColor: { type: String, default: '#F7C902' }
  },
  data() {
    return { logoUrl }
  },
  computed: {
    roleStyle() {
      return {
        color: 'var(--accent)',
        background: 'var(--accent-soft)'
      }
    }
  }
}
</script>

<style scoped>
.sidebar-header {
  padding: 24px 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: relative;
}

.sidebar-header.collapsed {
  padding: 32px 0;
  justify-content: center;
}

.header-content {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.brand-logo {
  height: 42px;
  width: auto;
  object-fit: contain;
}

.badge-role {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 14px;
  border-radius: 100px;
  font-size: 10px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 1.5px;
  width: fit-content;
}

.badge-role .dot {
  width: 6px;
  height: 6px;
  background: currentColor;
  border-radius: 50%;
}

.brand-trigger {
  background: none;
  border: none;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
  cursor: pointer;
  text-align: left;
  transition: transform 0.2s;
}

.brand-trigger:hover {
  transform: scale(1.02);
}

.brand-trigger-mini {
  width: 36px;
  height: 36px;
  background: none;
  border: none;
  padding: 0;
  cursor: pointer;
  color: var(--text-muted);
  display: grid;
  place-items: center;
  transition: transform 0.2s;
}

.brand-trigger-mini:hover {
  transform: scale(1.1);
  color: var(--accent-color);
}

.mini-toggle-icon {
  width: 18px;
  height: 18px;
}

.toggle-trigger svg {
  width: 16px;
  height: 16px;
  transition: transform 0.3s;
}

.toggle-trigger.rotated svg {
  transform: rotate(180deg);
}

.sidebar-header.collapsed .toggle-trigger {
  position: absolute;
  right: -14px;
  z-index: 10;
}

.header-separator {
  position: absolute;
  bottom: 0;
  left: 20px;
  right: 20px;
  height: 1px;
  background: rgba(0, 0, 0, 0.05);
}

.dark-mode .header-separator {
  background: rgba(255, 255, 255, 0.05);
}
</style>
