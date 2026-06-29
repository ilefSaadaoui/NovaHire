<template>
  <button
    type="button"
    class="auth-theme-toggle"
    :class="{ 'is-light': !themeStore.isDark }"
    :title="themeStore.isDark ? $t('common.themeLight') : $t('common.themeDark')"
    :aria-label="themeStore.isDark ? $t('common.themeLight') : $t('common.themeDark')"
    @click="themeStore.toggleTheme()"
  >
    <svg
      v-if="themeStore.isDark"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      aria-hidden="true"
    >
      <circle cx="12" cy="12" r="5" />
      <line x1="12" y1="1" x2="12" y2="3" />
      <line x1="12" y1="21" x2="12" y2="23" />
      <line x1="4.22" y1="4.22" x2="5.64" y2="5.64" />
      <line x1="18.36" y1="18.36" x2="19.78" y2="19.78" />
      <line x1="1" y1="12" x2="3" y2="12" />
      <line x1="21" y1="12" x2="23" y2="12" />
      <line x1="4.22" y1="19.07" x2="5.64" y2="17.66" />
      <line x1="18.36" y1="5.64" x2="19.78" y2="4.22" />
    </svg>
    <svg
      v-else
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      aria-hidden="true"
    >
      <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
    </svg>
  </button>
</template>

<script>
import { useThemeStore } from '@/stores/themeStore'

export default {
  name: 'AuthThemeToggle',
  setup() {
    const themeStore = useThemeStore()
    return { themeStore }
  }
}
</script>

<style scoped>
.auth-theme-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  border-radius: 14px;
  border: 1px solid rgba(255, 255, 255, 0.15);
  background: rgba(255, 255, 255, 0.06);
  color: #f8fafc;
  cursor: pointer;
  transition: background 0.25s, border-color 0.25s, transform 0.25s, color 0.25s;
  flex-shrink: 0;
}

.auth-theme-toggle svg {
  width: 20px;
  height: 20px;
}

.auth-theme-toggle:hover {
  transform: translateY(-2px);
  background: rgba(255, 255, 255, 0.12);
  border-color: rgba(0, 167, 225, 0.45);
}

.auth-theme-toggle.is-light {
  border-color: rgba(15, 23, 42, 0.12);
  background: rgba(255, 255, 255, 0.85);
  color: #0f172a;
  box-shadow: 0 4px 16px rgba(15, 23, 42, 0.08);
}

.auth-theme-toggle.is-light:hover {
  border-color: rgba(0, 167, 225, 0.35);
  background: #ffffff;
}
</style>
