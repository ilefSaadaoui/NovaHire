<template>
  <div class="sidebar-footer" :class="{ collapsed: isCollapsed }">
    <div class="theme-switch">
      <button 
        class="theme-btn" 
        :class="{ active: !themeStore.isDark }" 
        @click="themeStore.toggleTheme()"
        :title="$t('common.themeLight')"
        :style="{ color: !themeStore.isDark ? '#1e293b' : 'var(--text-muted)' }"
      >
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="5"/><line x1="12" y1="1" x2="12" y2="3"/><line x1="12" y1="21" x2="12" y2="23"/><line x1="4.22" y1="4.22" x2="5.64" y2="5.64"/><line x1="18.36" y1="18.36" x2="19.78" y2="19.78"/><line x1="1" y1="12" x2="3" y2="12"/><line x1="21" y1="12" x2="23" y2="12"/><line x1="4.22" y1="19.07" x2="5.64" y2="17.66"/><line x1="18.36" y1="5.64" x2="19.78" y2="4.22"/></svg>
      </button>
      <button 
        class="theme-btn" 
        :class="{ active: themeStore.isDark }"
        @click="themeStore.toggleTheme()"
        :title="$t('common.themeDark')"
        :style="{ color: themeStore.isDark ? '#ffffff' : 'var(--text-muted)' }"
      >
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/></svg>
      </button>
    </div>

    <div class="user-block">
      <div class="user-avatar" :style="avatarStyle">
        <img v-if="authStore.user?.avatarUrl && !imageError" :src="authStore.user.avatarUrl" alt="Avatar" class="avatar-img-sidebar" @error="imageError = true">
        <template v-else>
          {{ initials }}
        </template>
      </div>
      <div class="user-info" v-if="!isCollapsed">
        <span class="u-name">{{ userName }}</span>
        <span class="u-role">{{ userRoleLabel }}</span>
      </div>
      <button class="logout-btn" @click="$emit('logout')" :title="$t('common.logout')">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
      </button>
    </div>
  </div>
</template>

<script>
import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'

export default {
  name: 'SidebarFooter',
  emits: ['logout'],
  props: {
    isCollapsed: { type: Boolean, default: false },
    accentColor: { type: String, default: '#F7C902' },
    accentDark: { type: String, default: '#D4A50E' }
  },
  setup() {
    const authStore = useAuthStore()
    const themeStore = useThemeStore()
    return { authStore, themeStore }
  },
  data() {
    return {
      imageError: false
    }
  },
  async mounted() {
    if (!this.authStore.user && this.authStore.token) {
      await this.authStore.fetchUser()
    }
  },
  watch: {
    'authStore.user.avatarUrl'() {
      this.imageError = false
    }
  },
  computed: {
    userName() {
      const u = this.authStore.user
      return u?.firstName ? `${u.firstName} ${u.lastName}` : this.$t('common.userPlaceholder')
    },
    initials() {
      const u = this.authStore.user
      return u?.firstName ? (u.firstName[0] + u.lastName[0]).toUpperCase() : 'NH'
    },
    userRoleLabel() {
      const role = (this.authStore.userRole || '').toLowerCase()
      return ['company', 'admin', 'companyadmin'].includes(role) ? this.$t('roles.admin') : this.$t('roles.recruiter')
    },
    avatarStyle() {
      return {
        background: 'var(--accent-grad)',
        boxShadow: '0 4px 12px var(--accent-soft)'
      }
    }
  }
}
</script>

<style scoped>
.sidebar-footer {
  padding: 20px 16px;
  display: flex;
  flex-direction: column;
  gap: 20px;
  border-top: 1px solid var(--border-thin);
}

.sidebar-footer.collapsed {
  padding: 24px 0;
  align-items: center;
}

.theme-switch {
  display: flex;
  background: var(--border-thin);
  padding: 4px;
  border-radius: 100px;
  margin-bottom: 8px;
}

.theme-btn {
  flex: 1;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 100px;
  border: none;
  background: transparent;
  color: var(--text-muted);
  cursor: pointer;
  transition: 0.3s;
}

.theme-btn svg { 
  width: 16px; 
  height: 16px; 
  color: inherit !important;
  stroke: currentColor !important;
}

.theme-btn.active {
  background: var(--card-bg);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
}

/* Force dark icon in light mode */
:root:not(.dark-mode) .theme-btn.active {
  color: #1e293b !important;
}

/* Force white icon in dark mode */
.dark-mode .theme-btn.active {
  color: #ffffff !important;
}

.user-block {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 4px;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: 13px;
  color: white;
  flex-shrink: 0;
  overflow: hidden;
}

.avatar-img-sidebar {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.user-info {
  display: flex;
  flex-direction: column;
  min-width: 0;
  flex: 1;
}

.u-name {
  font-size: 13px;
  font-weight: 700;
  color: var(--text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.u-role {
  font-size: 11px;
  color: var(--text-muted);
}

.logout-btn {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  background: transparent;
  color: var(--danger);
  cursor: pointer;
  opacity: 0.6;
  transition: 0.2s;
}

.logout-btn:hover { opacity: 1; background: rgba(239, 68, 68, 0.1); }

.sidebar-footer.collapsed .theme-switch { 
  flex-direction: column;
  width: 40px;
}

.sidebar-footer.collapsed .user-info,
.sidebar-footer.collapsed .logout-btn {
  display: none;
}
</style>
