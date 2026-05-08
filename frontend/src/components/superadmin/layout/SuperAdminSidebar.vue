<template>
  <aside class="sa-sidebar">
    <div class="sa-brand-card">
      <div class="sa-brand-header">
        <img :src="logoUrl" alt="NovaHire" class="sa-logo-img">
      </div>
      
      <div class="sa-admin-badge">
        <span class="badge-dot"></span>
        <span class="badge-text">Admin Plateforme</span>
      </div>
    </div>

    <nav class="sa-nav">
      <button
        v-for="item in navItems"
        :key="item.id"
        class="nav-btn"
        :class="{ active: activeSection === item.id }"
        @click="$emit('update:activeSection', item.id)"
      >
        <div class="active-indicator"></div>
        <div class="nav-icon-wrap">
          <svg v-if="item.id === 'supervision'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/></svg>
          <svg v-else-if="item.id === 'entreprises'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 21h18"/><path d="M5 21V7l7-4 7 4v14"/><path d="M9 21v-6h6v6"/></svg>
          <svg v-else-if="item.id === 'approbations'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M9 11l3 3L22 4"/><path d="M21 12v7a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11"/></svg>
          <svg v-else-if="item.id === 'support'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
          <svg v-else-if="item.id === 'utilisateurs'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
          <svg v-else-if="item.id === 'profile'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 21a8 8 0 1 0-16 0"/><circle cx="12" cy="7" r="4"/></svg>
          <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
        </div>
        <span>{{ item.label }}</span>
      </button>
    </nav>

    <div class="sa-footer">
      <div class="theme-switcher-sa">
        <div class="switcher-track">
          <div class="switcher-thumb" :class="{ 'is-dark': darkMode }"></div>
          <button @click="!darkMode ? null : $emit('toggleDarkMode')" class="switcher-option" :class="{ active: !darkMode }">
             <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="5"/><line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="12" x2="12" y2="23"/><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/><line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/></svg>
          </button>
          <button @click="darkMode ? null : $emit('toggleDarkMode')" class="switcher-option" :class="{ active: darkMode }">
             <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/></svg>
          </button>
        </div>
      </div>
    </div>
  </aside>
</template>

<script setup>
import { computed } from 'vue'
import logoUrl from '@/assets/Logo_NovaHire.png'
import { useAuthStore } from '@/stores/authStore'
import UserAvatar from '@/components/shared/UserAvatar.vue'

const authStore = useAuthStore()

defineProps({
  activeSection: String,
  navItems: Array,
  accentColor: String,
  accentGrad: String,
  darkMode: Boolean
})

const userName = computed(() => authStore.user?.firstName || 'Admin')
const userFullName = computed(() => `${authStore.user?.firstName || ''} ${authStore.user?.lastName || ''}`.trim())
const userAvatarUrl = computed(() => authStore.user?.avatarUrl || '')

defineEmits(['update:activeSection', 'logout', 'toggleDarkMode'])
</script>

<style scoped>
.sa-sidebar {
  width: 280px;
  position: fixed;
  top: 24px;
  left: 24px;
  bottom: 24px;
  background: var(--sidebar-bg);
  backdrop-filter: blur(28px);
  -webkit-backdrop-filter: blur(28px);
  border: 1px solid rgba(108, 99, 255, 0.15);
  border-radius: 28px;
  display: flex;
  flex-direction: column;
  padding: 24px;
  z-index: 1000;
  box-shadow:
    0 8px 32px rgba(108, 99, 255, 0.12),
    0 2px 8px rgba(108, 99, 255, 0.06),
    inset 0 1px 0 rgba(255, 255, 255, 0.9);
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  animation: sidebarReveal 0.7s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes sidebarReveal {
  from {
    opacity: 0;
    transform: translateX(-24px);
    box-shadow: 0 0 0 rgba(108, 99, 255, 0);
  }
  to {
    opacity: 1;
    transform: translateX(0);
    box-shadow:
      0 8px 32px rgba(108, 99, 255, 0.12),
      0 2px 8px rgba(108, 99, 255, 0.06),
      inset 0 1px 0 rgba(255, 255, 255, 0.9);
  }
}

.sa-brand-card {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 16px;
  margin-bottom: 24px;
}

.sa-brand-header {
  display: flex;
  align-items: center;
  padding: 4px 0;
}

.sa-logo-img {
  height: 42px;
  width: auto;
  object-fit: contain;
}

.sa-admin-badge {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 16px;
  background: #e0f2fe; /* Very light blue */
  border-radius: 100px;
  width: fit-content;
  margin-top: 4px;
}

.theme-dark .sa-admin-badge {
  background: rgba(14, 165, 233, 0.15);
}

.badge-dot {
  width: 8px;
  height: 8px;
  background: #0ea5e9;
  border-radius: 50%;
  box-shadow: 0 0 8px rgba(14, 165, 233, 0.5);
  animation: pulse-status 2s infinite;
}

.badge-text {
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
  color: #0369a1; /* Darker blue */
  letter-spacing: 0.5px;
}

.theme-dark .badge-text {
  color: #7dd3fc;
}

@keyframes pulse-status {
  0% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.3); opacity: 0.7; }
  100% { transform: scale(1); opacity: 1; }
}

.sa-nav { display: flex; flex-direction: column; gap: 6px; flex: 1; margin-top: 10px; }

.nav-btn {
  position: relative;
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 14px 20px;
  border-radius: 16px;
  border: none;
  background: transparent;
  color: var(--text-muted);
  font-weight: 700;
  cursor: pointer;
  transition: all 0.35s cubic-bezier(0.16, 1, 0.3, 1);
  width: 100%;
  overflow: hidden;
}

.nav-btn::before {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, rgba(108, 99, 255, 0.08), rgba(164, 99, 255, 0.04));
  border-radius: 16px;
  opacity: 0;
  transition: opacity 0.3s;
}

.nav-btn:hover {
  background: rgba(108, 99, 255, 0.07);
  color: var(--accent-color);
  transform: translateX(3px);
  box-shadow: 0 4px 16px rgba(108, 99, 255, 0.1);
}
.nav-btn:hover::before { opacity: 1; }

.nav-btn.active {
  background: linear-gradient(135deg, rgba(108, 99, 255, 0.12), rgba(164, 99, 255, 0.08));
  color: var(--accent-color);
  box-shadow: 0 4px 20px rgba(108, 99, 255, 0.15), inset 0 1px 0 rgba(255,255,255,0.6);
  border: 1px solid rgba(108, 99, 255, 0.15);
}

.active-indicator {
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%) translateX(-10px);
  width: 4px;
  height: 24px;
  background: linear-gradient(180deg, #6C63FF, #A463FF);
  border-radius: 0 4px 4px 0;
  box-shadow: 2px 0 16px rgba(108, 99, 255, 0.5);
  opacity: 0;
  transition: 0.35s cubic-bezier(0.16, 1, 0.3, 1);
}

.nav-btn.active .active-indicator { opacity: 1; transform: translateY(-50%) translateX(0); }

.nav-icon-wrap { 
  width: 22px; 
  height: 22px; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  opacity: 0.6; 
  transition: all 0.3s;
}
.nav-btn:hover .nav-icon-wrap { transform: scale(1.1); opacity: 1; color: white; }
.nav-btn.active .nav-icon-wrap { opacity: 1; color: var(--accent-color); }
.nav-icon-wrap svg { width: 20px; }

.sa-footer { margin-top: auto; display: flex; flex-direction: column; gap: 20px; }

.theme-switcher-sa {
  background: var(--bg-hover);
  border: 1px solid var(--border-thin);
  border-radius: 100px;
  padding: 4px;
}
.switcher-track { display: flex; position: relative; height: 36px; }
.switcher-thumb {
  position: absolute; width: calc(50% - 2px); height: 100%;
  background: var(--card-bg); border: 1px solid var(--border-thin);
  border-radius: 100px; transition: 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}
.switcher-thumb.is-dark { 
  transform: translateX(100%); 
  background: var(--accent-color);
  box-shadow: 0 0 15px var(--accent-color);
}
.switcher-option { flex: 1; border: none; background: transparent; cursor: pointer; display: flex; align-items: center; justify-content: center; z-index: 2; }
.switcher-option svg { width: 16px; color: var(--text-muted); }
.switcher-option.active svg { color: var(--text-primary); }
.theme-dark .switcher-option.active svg { color: white; }

.user-chip-sa {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: var(--bg-hover);
  border: 1px solid var(--border-thin);
  border-radius: 20px;
}

.user-avatar-sa {
  width: 40px; height: 40px; border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  color: white; font-weight: 800; font-size: 14px;
  box-shadow: 0 4px 16px rgba(108, 99, 255, 0.35);
  animation: avatarPulse 3s ease-in-out infinite;
}

@keyframes avatarPulse {
  0%, 100% { box-shadow: 0 4px 16px rgba(108, 99, 255, 0.35); }
  50%       { box-shadow: 0 4px 24px rgba(108, 99, 255, 0.55); }
}

.user-info-sa { flex: 1; display: flex; flex-direction: column; min-width: 0; }
.user-name-sa { font-size: 13px; font-weight: 700; color: var(--text-primary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.user-role-sa { font-size: 10px; color: var(--text-muted); font-weight: 600; }

.mini-logout {
  width: 32px; height: 32px; border-radius: 10px;
  border: none; background: rgba(239, 68, 68, 0.1);
  color: #ef4444; cursor: pointer; transition: 0.3s;
  display: flex; align-items: center; justify-content: center;
}
.mini-logout:hover { background: #ef4444; color: white; }
.mini-logout svg { width: 16px; }
</style>