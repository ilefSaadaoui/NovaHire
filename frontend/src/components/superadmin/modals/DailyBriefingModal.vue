<template>
  <Transition name="briefing-fade">
    <div v-if="show" class="briefing-overlay" @click.self="close">
      <div class="briefing-card platinum-glass" :class="roleTheme">
        <!-- Close Button -->
        <button class="close-btn" @click="close">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
        </button>

        <div class="briefing-header">
          <div class="header-icon-wrap">
            <svg v-if="role === 'superadmin'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="header-icon"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg>
            <svg v-else-if="role === 'companyadmin'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="header-icon"><path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6"/></svg>
            <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="header-icon"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z"/></svg>
          </div>
          <div class="header-text">
            <h2>{{ greeting }}, {{ authStore.user?.firstName || 'Admin' }}</h2>
            <p>{{ roleSubtitle }}</p>
          </div>
        </div>

        <div class="briefing-body">
          <div v-for="(task, i) in currentTasks" :key="i" 
               class="task-item" 
               :class="{ 'has-work': task.count > 0 }" 
               @click="handleTaskClick(task)">
            <div class="task-icon" :style="{ background: task.bgColor, color: task.color }">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" v-html="task.icon"></svg>
            </div>
            <div class="task-info">
              <span class="task-title">{{ task.title }}</span>
              <span class="task-status" v-if="task.count > 0">{{ task.count }} {{ task.unit }} {{ task.alertMsg }}</span>
              <span class="task-status ok" v-else>{{ task.okMsg }}</span>
            </div>
            <div class="task-arrow">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="9 18 15 12 9 6"/></svg>
            </div>
          </div>
        </div>

        <div class="briefing-footer">
          <button class="action-btn" @click="close">
            {{ actionBtnLabel }}
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="5" y1="12" x2="19" y2="12"/><polyline points="12 5 19 12 12 19"/></svg>
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { computed } from 'vue'
import { useAdminStore } from '@/stores/adminStore'
import { useAuthStore } from '@/stores/authStore'
import { useCompanyStore } from '@/stores/companyStore'
import { useRecruitmentStore } from '@/stores/recruitmentStore'

const props = defineProps({
  show: Boolean
})

const emit = defineEmits(['close', 'navigate'])

const adminStore = useAdminStore()
const authStore = useAuthStore()
const companyStore = useCompanyStore()
const recruitmentStore = useRecruitmentStore()

const role = computed(() => authStore.userRole?.toLowerCase() || 'recruiter')

const greeting = computed(() => {
  const hour = new Date().getHours()
  if (hour >= 5 && hour < 12) return 'Bonjour'
  if (hour >= 12 && hour < 18) return 'Bon après-midi'
  return 'Bonsoir'
})

const roleTheme = computed(() => {
  if (role.value === 'superadmin') return 'theme-violet'
  if (role.value === 'companyadmin') return 'theme-blue'
  return 'theme-gold'
})

const roleSubtitle = computed(() => {
  if (role.value === 'superadmin') return "Voici votre plan d'action pour aujourd'hui."
  if (role.value === 'companyadmin') return "Suivi de la performance de votre entreprise."
  return "Vos priorités de recrutement aujourd'hui."
})

const actionBtnLabel = computed(() => {
  if (role.value === 'superadmin') return "Accéder au cockpit"
  return "Aller au Dashboard"
})

const currentTasks = computed(() => {
  if (role.value === 'superadmin') {
    return [
      {
        id: 'approbations',
        title: 'Validations en attente',
        count: (adminStore.companies || []).filter(c => c.status === 0 || c.status === 'Pending').length,
        unit: 'entreprise(s)',
        alertMsg: 'attendent votre accord.',
        okMsg: 'Toutes les entreprises sont à jour.',
        icon: '<path d="M22 11.08V12a10 10 0 1 1-5.93-9.14M22 4 12 14.01l-3-3"/>',
        color: '#10b981', bgColor: 'rgba(16, 185, 129, 0.1)'
      },
      {
        id: 'support',
        title: 'Messages Support',
        count: (adminStore.contactMessages || []).filter(m => m.status === 0 || m.status === 'New').length,
        unit: 'nouveau(x) message(s)',
        alertMsg: 'non traité(s).',
        okMsg: 'Boîte de réception vide. Bon travail !',
        icon: '<path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>',
        color: '#6C63FF', bgColor: 'rgba(108, 99, 255, 0.1)'
      }
    ]
  }

  if (role.value === 'companyadmin') {
    return [
      {
        id: 'candidats',
        title: 'Nouveaux Candidats',
        count: companyStore.stats.totalApplications || 0,
        unit: 'postulations',
        alertMsg: 'reçues récemment.',
        okMsg: 'Pas de nouveaux candidats aujourd\'hui.',
        icon: '<path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2M9 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8z"/>',
        color: '#0ea5e9', bgColor: 'rgba(14, 165, 233, 0.1)'
      },
      {
        id: 'entretiens',
        title: 'Entretiens prévus',
        count: companyStore.stats.plannedInterviews || 0,
        unit: 'session(s)',
        alertMsg: 'organisées pour aujourd\'hui.',
        okMsg: 'Aucun entretien au programme.',
        icon: '<rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/>',
        color: '#f59e0b', bgColor: 'rgba(245, 158, 11, 0.1)'
      }
    ]
  }

  // Recruiter Default
  return [
    {
      id: 'evaluation',
      title: 'À évaluer',
      count: recruitmentStore.applications.filter(a => a.status === 'Identification' || a.status === 'New').length,
      unit: 'candidat(s)',
      alertMsg: 'attendent votre revue.',
      okMsg: 'Tous vos candidats ont été traités.',
      icon: '<circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/>',
      color: '#10b981', bgColor: 'rgba(16, 185, 129, 0.1)'
    },
    {
      id: 'offres',
      title: 'Offres actives',
      count: recruitmentStore.offers.filter(o => o.isActive).length,
      unit: 'offre(s)',
      alertMsg: 'sont actuellement en ligne.',
      okMsg: 'Aucune offre active pour le moment.',
      icon: '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/>',
      color: '#818cf8', bgColor: 'rgba(129, 140, 248, 0.1)'
    }
  ]
})

const close = () => emit('close')

const handleTaskClick = (task) => {
  if (role.value === 'superadmin') {
    emit('navigate', task.id)
  } else {
    // For other roles, we can emit a general navigate or let them close
    emit('navigate', 'dashboard')
  }
  close()
}
</script>

<style scoped>
.briefing-overlay {
  position: fixed;
  inset: 0;
  background: var(--overlay-bg, rgba(13, 24, 33, 0.7));
  backdrop-filter: blur(12px);
  z-index: 2000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  --overlay-bg: rgba(15, 23, 42, 0.4);
  --card-bg: rgba(255, 255, 255, 0.95);
  --card-border: rgba(0, 0, 0, 0.08);
  --text-title: #0f172a;
  --text-desc: #64748b;
  --item-bg: rgba(0, 0, 0, 0.03);
  --item-border: rgba(0, 0, 0, 0.05);
  --item-hover: rgba(0, 0, 0, 0.06);
  --close-bg: rgba(0, 0, 0, 0.05);
  --close-text: #64748b;
}

:global(body.dark-mode) .briefing-overlay {
  --overlay-bg: rgba(2, 6, 23, 0.7);
  --card-bg: rgba(15, 23, 42, 0.9);
  --card-border: rgba(255, 255, 255, 0.1);
  --text-title: #ffffff;
  --text-desc: #94a3b8;
  --item-bg: rgba(255, 255, 255, 0.03);
  --item-border: rgba(255, 255, 255, 0.05);
  --item-hover: rgba(255, 255, 255, 0.06);
  --close-bg: rgba(255, 255, 255, 0.05);
  --close-text: #94a3b8;
}

/* Themes per Role */
.theme-violet { --role-color: #6C63FF; --role-color-rgb: 108, 99, 255; --role-gradient: linear-gradient(135deg, #6C63FF, #00D9FF); }
.theme-blue { --role-color: #00A7E1; --role-color-rgb: 0, 167, 225; --role-gradient: linear-gradient(135deg, #00A7E1, #00D1FF); }
.theme-gold { --role-color: #f59e0b; --role-color-rgb: 245, 158, 11; --role-gradient: linear-gradient(135deg, #f59e0b, #fbbf24); }

.theme-violet .header-icon-wrap { background: var(--role-gradient); }
.theme-violet .action-btn { background: linear-gradient(135deg, #6C63FF, #A463FF); }

.theme-blue .header-icon-wrap { background: var(--role-gradient); }
.theme-blue .action-btn { background: linear-gradient(135deg, #00A7E1, #0086B5); }
.theme-blue .task-item.has-work { border-left-color: var(--role-color); }

.theme-gold .header-icon-wrap { background: var(--role-gradient); }
.theme-gold .action-btn { background: linear-gradient(135deg, #f59e0b, #d97706); }
.theme-gold .task-item.has-work { border-left-color: var(--role-color); }

@keyframes cardScale {
  from { opacity: 0; transform: scale(0.9) translateY(20px); }
  to { opacity: 1; transform: scale(1) translateY(0); }
}

.briefing-card {
  width: min(520px, 100%);
  background: var(--card-bg);
  border: 2px solid var(--role-color);
  border-radius: 28px;
  padding: 40px;
  position: relative;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.1), 0 0 25px rgba(var(--role-color-rgb), 0.2);
  animation: cardScale 0.5s cubic-bezier(0.16, 1, 0.3, 1) both;
}

.close-btn {
  position: absolute;
  top: 24px; right: 24px;
  background: var(--close-bg);
  border: none; color: var(--close-text);
  width: 36px; height: 36px; border-radius: 12px;
  cursor: pointer; transition: 0.2s;
  display: flex; align-items: center; justify-content: center;
}
.close-btn:hover { background: rgba(239, 68, 68, 0.15); color: #ef4444; transform: rotate(90deg); }
.close-btn svg { width: 18px; }

.briefing-header {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 32px;
}

.header-icon-wrap {
  width: 56px; height: 56px;
  border-radius: 16px;
  display: flex; align-items: center; justify-content: center;
  color: white;
  box-shadow: 0 10px 20px rgba(0, 0, 0, 0.2);
}
.header-icon { width: 28px; }

.header-text h2 {
  font-size: 24px; font-weight: 800; color: var(--text-title); margin: 0;
}
.header-text p {
  font-size: 14px; color: var(--text-desc); margin: 4px 0 0 0;
}

.briefing-body {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.task-item {
  background: var(--item-bg);
  border: 1px solid var(--item-border);
  border-radius: 20px;
  padding: 20px;
  display: flex;
  align-items: center;
  gap: 16px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.task-item:hover {
  background: var(--item-hover);
  transform: translateX(6px);
}

.task-item.has-work {
  border-left: 4px solid #6C63FF;
}

.task-icon {
  width: 44px; height: 44px;
  border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}
.task-icon svg { width: 20px; }

.task-info { flex: 1; }
.task-title { font-size: 15px; font-weight: 700; color: var(--text-title); display: block; }
.task-status { font-size: 12px; color: #ef4444; font-weight: 600; margin-top: 2px; display: block; }
.task-status.ok { color: #10b981; }

.task-arrow { color: #475569; opacity: 0.5; transition: 0.3s; }
.task-item:hover .task-arrow { color: var(--text-title); opacity: 1; transform: translateX(4px); }
.task-arrow svg { width: 18px; }

.briefing-footer {
  margin-top: 40px;
}

.action-btn {
  width: 100%;
  padding: 16px;
  border: none; border-radius: 18px;
  color: white; font-weight: 800; font-size: 15px;
  cursor: pointer; transition: 0.3s;
  display: flex; align-items: center; justify-content: center; gap: 10px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
}
.action-btn:hover {
  transform: translateY(-2px);
  box-shadow: 0 15px 30px rgba(0, 0, 0, 0.15);
}
.action-btn svg { width: 18px; }

/* Transitions */
.briefing-fade-enter-active, .briefing-fade-leave-active { transition: opacity 0.5s ease; }
.briefing-fade-enter-from, .briefing-fade-leave-to { opacity: 0; }
</style>
