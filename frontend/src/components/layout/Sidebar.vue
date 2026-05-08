<template>
  <aside class="sidebar" :class="{ collapsed: isCollapsed }" :style="sidebarVars">
    <SidebarHeader
      :is-collapsed="isCollapsed"
      :show-brand-name="!isCollapsed"
      :show-brand-role="!isCollapsed"
      :role-label="roleLabel"
      :accent-color="accentColor"
      :collapsible="collapsible"
      @toggle-collapse="$emit('toggle-collapse')"
    />

    <nav class="sidebar-nav">
      <router-link :to="dashboardLink" class="nav-item" :class="{ active: activeItem === 'dashboard' }">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
        <span v-if="!isCollapsed">{{ $t('common.dashboard') }}</span>
      </router-link>
      
      <router-link to="/jobs" class="nav-item" :class="{ active: activeItem === 'jobs' }">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="2" y="7" width="20" height="14" rx="2" ry="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
        <span v-if="!isCollapsed">{{ $t('common.offers') }}</span>
      </router-link>

      <router-link to="/applications" class="nav-item" :class="{ active: activeItem === 'pipeline' }">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
        <span v-if="!isCollapsed">{{ $t('common.pipeline') }}</span>
      </router-link>
      
      <router-link to="/calendar" class="nav-item" :class="{ active: activeItem === 'calendar' }">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
        <span v-if="!isCollapsed">{{ $t('common.calendar') }}</span>
      </router-link>


      <router-link v-if="!authStore.isSuperAdmin" to="/reports" class="nav-item" :class="{ active: activeItem === 'reports' }">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21.21 15.89A10 10 0 1 1 8 2.83"/><path d="M22 12A10 10 0 0 0 12 2v10z"/></svg>
        <span v-if="!isCollapsed">{{ $t('common.reports') }}</span>
      </router-link>

      <router-link v-if="isCompanyOwner" to="/settings" class="nav-item" :class="{ active: activeItem === 'settings' }">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.1a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z"/><circle cx="12" cy="12" r="3"/></svg>
        <span v-if="!isCollapsed">{{ $t('common.settings') }}</span>
      </router-link>

      <router-link to="/profile" class="nav-item" :class="{ active: activeItem === 'profile' }">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 21a8 8 0 1 0-16 0"/><circle cx="12" cy="7" r="4"/></svg>
        <span v-if="!isCollapsed">{{ $t('common.profile') }}</span>
      </router-link>
    </nav>

    <SidebarFooter
      :is-collapsed="isCollapsed"
      :accent-color="accentColor"
      :accent-dark="accentDark"
      @logout="logout"
    />
  </aside>
</template>

<script>
import SidebarHeader from '@/components/layout/SidebarHeader.vue'
import SidebarFooter from '@/components/layout/SidebarFooter.vue'
import { useAuthStore } from '@/stores/authStore'

export default {
  name: 'Sidebar',
  components: {
    SidebarHeader,
    SidebarFooter
  },
  emits: ['toggle-collapse'],
  props: {
    activeItem: { type: String, default: 'dashboard' },
    collapsible: { type: Boolean, default: false },
    collapsed: { type: Boolean, default: false }
  },
  setup() {
    const authStore = useAuthStore()
    return { authStore }
  },
  computed: {
    isCollapsed() {
      return this.collapsible && this.collapsed
    },
    isCompanyOwner() {
      return this.authStore.isAdmin && !this.authStore.isSuperAdmin
    },
    roleLabel() {
      if (this.authStore.isSuperAdmin) return 'SuperAdmin'
      return this.isCompanyOwner ? this.$t('roles.admin') : this.$t('roles.recruiter')
    },
    accentColor() {
      return this.authStore.themeColors.accent
    },
    accentDark() {
      return this.authStore.themeColors.accentDark
    },
    sidebarVars() {
      return {}
    },
    dashboardLink() {
      return this.isCompanyOwner ? '/dashboard/companyadmin' : '/dashboard/recruiter'
    }
  },
  methods: {
    logout() {
      this.authStore.logout()
      this.$router.push('/connexion')
    }
  }
}
</script>

<style scoped>
.sidebar {
  width: var(--sidebar-width); 
  background: var(--r-surface, var(--sidebar-bg)); 
  display: flex; 
  flex-direction: column; 
  position: fixed; 
  top: var(--sidebar-gap); 
  left: var(--sidebar-gap); 
  bottom: var(--sidebar-gap);
  z-index: 100; 
  transition: all 0.5s cubic-bezier(0.2, 0.8, 0.2, 1);
  border-radius: var(--radius-xl);
  border: var(--glass-border);
  box-shadow: var(--glass-shadow);
  backdrop-filter: var(--glass-blur);
}

.sidebar.collapsed { width: var(--sidebar-collapsed); }

.sidebar-nav { flex: 1; padding: 24px 16px; display: flex; flex-direction: column; gap: 4px; }

.nav-item {
  display: flex; 
  align-items: center; 
  gap: 16px; 
  padding: 12px 20px; 
  border-radius: var(--radius-md);
  color: var(--text-muted); 
  text-decoration: none; 
  font-size: 14px; 
  font-weight: 600;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1); 
  white-space: nowrap;
}

.nav-item svg { width: 22px; height: 22px; flex-shrink: 0; opacity: 0.6; transition: 0.3s; }

.nav-item:hover { 
  background: rgba(var(--accent-rgb), 0.05); 
  color: var(--accent-color); 
}
.nav-item:hover svg { opacity: 1; }

.nav-item.active {
  background: var(--accent-color);
  color: var(--accent-contrast, white); 
  box-shadow: 0 8px 20px var(--accent-soft);
  transform: translateX(4px);
}

.nav-item.active::before {
  content: '';
  position: absolute;
  left: -8px;
  top: 50%;
  transform: translateY(-50%);
  width: 4px;
  height: 20px;
  background: var(--accent-color);
  border-radius: 4px;
  box-shadow: 0 0 10px var(--accent-color);
}

.sidebar.collapsed .nav-item { padding: 12px; justify-content: center; width: 48px; margin: 0 auto; }
.sidebar.collapsed .nav-item span { display: none; }
</style>

