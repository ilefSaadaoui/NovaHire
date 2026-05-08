<template>
  <header class="sa-topbar" @animationend.once="undefined">
    <!-- Left: Greeting -->
    <div class="top-left">
      <div class="greeting-shell">
        <div class="greeting-icon">
          <svg v-if="now.getHours() < 12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="sun-icon"><path d="M12 2v2M12 20v2M4.93 4.93l1.41 1.41M17.66 17.66l1.41 1.41M2 12h2M20 12h2M6.34 17.66l-1.41 1.41M19.07 4.93l-1.41 1.41"/><circle cx="12" cy="12" r="4"/></svg>
          <svg v-else-if="now.getHours() < 18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="sun-icon"><circle cx="12" cy="12" r="5"/><path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42"/></svg>
          <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="moon-icon"><path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"/></svg>
        </div>
        <div>
          <h1 class="greeting-text">{{ greeting }}, <span class="greeting-name">{{ userName }}</span></h1>
          <p class="greeting-sub">Tableau de bord Admin Plateforme · NovaHire Elite</p>
        </div>
      </div>
    </div>

    <!-- Right: Date & Live Time -->
    <div class="top-right">
      <div class="datetime-card">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="dt-icon"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
        <div class="dt-info">
          <span class="dt-date">{{ currentDate }}</span>
          <span class="dt-time">{{ currentTime }}</span>
        </div>
      </div>

      <div class="v-divider"></div>

      <div class="sys-status">
        <div class="status-pill online">
          <span class="p-dot"></span>
          <span>API</span>
        </div>
        <div class="status-pill ai">
          <span class="p-dot"></span>
          <span>AI</span>
        </div>
      </div>

      <div class="v-divider"></div>

      <div class="user-avatar-topbar">
        <UserAvatar
          :src="authStore.user?.avatarUrl"
          :name="authStore.user?.firstName + ' ' + authStore.user?.lastName"
          :size="38"
          radius="12px"
        />
      </div>

      <div class="v-divider"></div>

      <button class="logout-btn-sa" @click="$emit('logout')" title="Déconnexion">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
        <span>Quitter</span>
      </button>
    </div>
  </header>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import UserAvatar from '@/components/shared/UserAvatar.vue'

const authStore = useAuthStore()
defineEmits(['logout'])

const now = ref(new Date())
let timer = null

onMounted(() => {
  timer = setInterval(() => { now.value = new Date() }, 1000)
})
onUnmounted(() => clearInterval(timer))

const userName = computed(() => authStore.user?.firstName || 'Administrateur')

const greeting = computed(() => {
  const h = now.value.getHours()
  if (h < 12) return 'Bonjour'
  if (h < 18) return 'Bon après-midi'
  return 'Bonsoir'
})

const currentDate = computed(() =>
  now.value.toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long' })
)

const currentTime = computed(() =>
  now.value.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
)
</script>

<style scoped>
.sa-topbar {
  height: 72px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 32px;
  background: rgba(255, 255, 255, 0.55);
  backdrop-filter: blur(24px) saturate(180%);
  -webkit-backdrop-filter: blur(24px) saturate(180%);
  border: 1px solid rgba(255, 255, 255, 0.7);
  border-bottom: 1px solid rgba(108, 99, 255, 0.1);
  border-radius: 28px;
  margin: 16px 24px 20px 0;
  z-index: 1000;
  box-shadow: 
    0 10px 40px -10px rgba(108, 99, 255, 0.12),
    0 4px 12px -4px rgba(108, 99, 255, 0.05);
  animation: topbarDrop 0.8s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes topbarDrop {
  from { opacity: 0; transform: translateY(-20px); }
  to   { opacity: 1; transform: translateY(0); }
}

.theme-dark .sa-topbar {
  background: rgba(15, 23, 42, 0.65);
  border: 1px solid rgba(255, 255, 255, 0.05);
  border-bottom: 1px solid rgba(108, 99, 255, 0.15);
}

/* --- Left: Greeting --- */
.top-left { display: flex; align-items: center; }
.greeting-shell { display: flex; align-items: center; gap: 14px; }

.greeting-icon {
  width: 48px; height: 48px;
  border-radius: 16px;
  background: white;
  border: 1px solid rgba(108, 99, 255, 0.15);
  display: flex; align-items: center; justify-content: center;
  box-shadow: 0 4px 15px rgba(108, 99, 255, 0.08);
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  cursor: pointer;
}

.theme-dark .greeting-icon {
  background: rgba(30, 41, 59, 0.8);
  border-color: rgba(108, 99, 255, 0.2);
}

.greeting-icon:hover {
  transform: scale(1.1) rotate(8deg);
  border-color: #6C63FF;
  box-shadow: 0 8px 25px rgba(108, 99, 255, 0.2);
}

.greeting-icon svg { width: 22px; transition: 0.3s; }
.greeting-icon .sun-icon { color: #f59e0b; animation: sunSpin 12s linear infinite; }
.greeting-icon .moon-icon { color: #818cf8; animation: moonPulse 2s ease-in-out infinite; }

@keyframes sunSpin { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
@keyframes moonPulse { 
  0%, 100% { transform: scale(1); filter: drop-shadow(0 0 0 rgba(129, 140, 248, 0)); }
  50% { transform: scale(1.1); filter: drop-shadow(0 0 8px rgba(129, 140, 248, 0.4)); }
}

@keyframes iconBounce {
  from { transform: scale(0.6) rotate(-15deg); opacity: 0; }
  to   { transform: scale(1) rotate(0deg); opacity: 1; }
}

.greeting-text {
  font-size: 17px;
  font-weight: 900;
  margin: 0;
  color: var(--text-primary);
  letter-spacing: -0.4px;
  animation: textReveal 0.5s cubic-bezier(0.16, 1, 0.3, 1) 0.25s both;
}

@keyframes textReveal {
  from { opacity: 0; transform: translateX(-10px); }
  to   { opacity: 1; transform: translateX(0); }
}

.greeting-name {
  background: linear-gradient(135deg, #6C63FF, #A463FF);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.greeting-sub {
  font-size: 11px;
  font-weight: 600;
  color: var(--text-muted);
  margin: 2px 0 0 0;
  letter-spacing: 0.2px;
}

/* --- Right: DateTime + Status --- */
.top-right { display: flex; align-items: center; gap: 16px; }

.datetime-card {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 6px 14px;
  background: rgba(108, 99, 255, 0.04);
  border: 1px solid rgba(108, 99, 255, 0.1);
  border-radius: 12px;
  animation: dtReveal 0.5s cubic-bezier(0.16, 1, 0.3, 1) 0.35s both;
}

@keyframes dtReveal {
  from { opacity: 0; transform: translateX(10px); }
  to   { opacity: 1; transform: translateX(0); }
}

.dt-icon { width: 15px; color: #6C63FF; flex-shrink: 0; }

.dt-info { display: flex; flex-direction: column; gap: 1px; }
.dt-date {
  font-size: 12px;
  font-weight: 700;
  color: var(--text-primary);
  text-transform: capitalize;
  white-space: nowrap;
}
.dt-time {
  font-size: 11px;
  font-weight: 800;
  color: #6C63FF;
  letter-spacing: 0.5px;
  font-variant-numeric: tabular-nums;
}

.v-divider { width: 1px; height: 28px; background: rgba(108, 99, 255, 0.1); margin: 0 4px; }

.sys-status { display: flex; gap: 8px; }
.status-pill {
  display: flex; align-items: center; gap: 6px;
  font-size: 10px; font-weight: 800;
  color: var(--text-muted);
  padding: 4px 10px;
  border-radius: 100px;
  background: rgba(108, 99, 255, 0.05);
  border: 1px solid transparent;
  transition: all 0.3s;
}
.status-pill:hover { border-color: rgba(108, 99, 255, 0.2); transform: translateY(-1px); }

.p-dot { 
  width: 6px; height: 6px; border-radius: 50%; 
  animation: p-dot-pulse 2s infinite;
}

@keyframes p-dot-pulse {
  0% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.4); opacity: 0.5; }
  100% { transform: scale(1); opacity: 1; }
}

.status-pill.online .p-dot { background: #10b981; box-shadow: 0 0 8px rgba(16, 185, 129, 0.6); }
.status-pill.ai .p-dot { background: #A463FF; box-shadow: 0 0 8px rgba(164, 99, 255, 0.6); }

.user-avatar-topbar {
  display: flex;
  align-items: center;
  padding: 2px;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.3);
  border: 1px solid rgba(108, 99, 255, 0.1);
  transition: all 0.3s;
  cursor: pointer;
}
.theme-dark .user-avatar-topbar {
  background: rgba(255, 255, 255, 0.05);
}
.user-avatar-topbar:hover {
  transform: scale(1.05);
  border-color: var(--accent-color);
  box-shadow: 0 4px 12px rgba(108, 99, 255, 0.15);
}

.logout-btn-sa {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: rgba(239, 68, 68, 0.08);
  border: 1px solid rgba(239, 68, 68, 0.15);
  border-radius: 12px;
  color: #ef4444;
  font-size: 12px;
  font-weight: 800;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.logout-btn-sa:hover {
  background: #ef4444;
  color: white;
  transform: translateY(-1px);
  box-shadow: 0 4px 12px rgba(239, 68, 68, 0.25);
}

.logout-btn-sa svg {
  width: 16px;
}
</style>