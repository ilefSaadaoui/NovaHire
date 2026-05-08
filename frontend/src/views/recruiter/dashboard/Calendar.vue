<template>
  <div class="dashboard-layout recruiter-layout app-admin-glass-theme" :class="{ 'dark-mode': isDarkMode }">
    <Sidebar active-item="calendar" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      <!-- CELESTIAL BACKGROUND -->
      <div class="celestial-bg">
        <div class="c-orb orb-1"></div>
        <div class="c-orb orb-2"></div>
      </div>

      <div class="content-wrapper">
        <header class="calendar-header anim-reveal-down">
          <div class="header-info">
            <div class="icon-box-themed">
              <CalendarIcon :size="28" stroke-width="2.5" />
            </div>
            <div>
              <h1 class="premium-title-themed animated-gradient-text">{{ $t('common.calendar') }}</h1>
              <p class="subtitle">{{ $t('calendar.subtitle') }}</p>
            </div>
          </div>
          
          <div class="header-actions-pro">
            <div class="view-filters">
              <button class="filter-chip" :class="{ active: activeFilter === 'all' }" @click="activeFilter = 'all'">Tous</button>
              <button class="filter-chip" :class="{ active: activeFilter === 'visio' }" @click="activeFilter = 'visio'">Visio</button>
              <button class="filter-chip" :class="{ active: activeFilter === 'onsite' }" @click="activeFilter = 'onsite'">Physique</button>
            </div>
            
            <div class="navigation-controls">
              <button class="btn-today-nav" @click="goToToday">Aujourd'hui</button>
              <div class="nav-arrows-pro">
                <button class="btn-nav-arrow" @click="prevMonth"><ChevronLeft :size="18" /></button>
                <h2 class="current-month-display">{{ currentMonthName }} {{ currentYear }}</h2>
                <button class="btn-nav-arrow" @click="nextMonth"><ChevronRight :size="18" /></button>
              </div>
            </div>
          </div>
        </header>

        <div v-if="loading" class="loading-container-pro">
          <div class="nebula-spinner-pro"></div>
          <p>Chargement du planning céleste...</p>
        </div>

        <div v-else class="calendar-grid-container-pro anim-reveal-up">
          <div class="calendar-card-pro">
            <div class="days-header-pro">
              <div v-for="day in weekDays" :key="day" class="day-label-pro">{{ day }}</div>
            </div>

            <div class="days-grid-pro">
              <div 
                v-for="(day, idx) in calendarDays" 
                :key="idx" 
                class="day-cell-pro"
                :class="{ 
                  'other-month': !day.isCurrentMonth, 
                  'today-cell': day.isToday,
                  'weekend-cell': day.date.getDay() === 0 || day.date.getDay() === 6
                }"
              >
                <div class="day-header-box">
                  <span class="day-number-pro">{{ day.date.getDate() }}</span>
                  <span v-if="day.isToday" class="today-marker"></span>
                </div>
                
                <div class="events-stack">
                  <div 
                    v-for="event in filteredEvents(day.events)" 
                    :key="event.id" 
                    class="event-card-mini"
                    :class="event.Type"
                    @click="showEventDetails(event)"
                  >
                    <div class="event-accent" :style="{ background: event.Color }"></div>
                    <span class="event-time-mini">{{ event.time }}</span>
                    <span class="event-name-mini">{{ event.CandidateName }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- SIDE PANEL: UPCOMING -->
          <div class="side-upcoming-panel">
            <div class="panel-header-pro">
              <h3 class="panel-title-pro">
                <Clock :size="18" />
                {{ $t('calendar.upcoming') }}
              </h3>
              <div class="upcoming-badge">{{ upcomingEvents.length }}</div>
            </div>
            
            <div class="upcoming-scroll-area premium-scroll">
              <div v-for="event in upcomingEvents" :key="event.id" class="upcoming-item-card" :class="event.Type">
                <div class="item-date-square">
                  <span class="item-day">{{ event.dayNumber }}</span>
                  <span class="item-month">{{ event.monthName }}</span>
                </div>
                <div class="item-details">
                  <div class="item-job-title">{{ event.JobTitle }}</div>
                  <div class="item-candidate-name">{{ event.CandidateName }}</div>
                  <div class="item-footer-meta">
                    <span class="item-time-stamp"><Clock :size="12" /> {{ event.time }}</span>
                    <span class="item-type-pill" :style="{ background: event.Color + '15', color: event.Color }">
                      {{ event.Type }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- MODAL DÉTAILS ENTRETIEN -->
      <div v-if="selectedEvent" class="modal-overlay-pro" @click="selectedEvent = null">
        <div class="modal-content-pro premium-glass anim-scale-up" @click.stop>
          <div class="modal-header-pro">
            <div class="header-main-pro">
              <div class="event-type-icon" :style="{ background: selectedEvent.Color }">
                <Video v-if="selectedEvent.Type === 'visio'" :size="20" />
                <Clock v-else :size="20" />
              </div>
              <div>
                <h3>Détails de l'entretien</h3>
                <p>{{ selectedEvent.CandidateName }} — {{ selectedEvent.JobTitle }}</p>
              </div>
            </div>
            <button class="btn-close-pro" @click="selectedEvent = null"><X :size="20" /></button>
          </div>

          <div class="modal-body-pro">
            <div class="info-grid-pro">
              <div class="info-item-pro">
                <label><CalendarIcon :size="14" /> Date & Heure</label>
                <p>{{ formatDate(selectedEvent.date) }} à {{ selectedEvent.time }}</p>
              </div>
              <div class="info-item-pro">
                <label><Info :size="14" /> Type</label>
                <p class="type-badge-pro" :style="{ background: selectedEvent.Color + '15', color: selectedEvent.Color }">
                  {{ (selectedEvent.Type || '').toUpperCase() }}
                </p>
              </div>
            </div>

            <div class="form-group-pro">
              <label><Plus :size="14" /> Lien ou Lieu</label>
              <input v-model="editForm.LocationOrLink" placeholder="Lien Meet/Zoom ou adresse physique..." />
            </div>

            <div class="form-group-pro">
              <label><MessageSquare :size="14" /> Message / Informations générales</label>
              <textarea v-model="editForm.Message" rows="5" placeholder="Ajoutez des informations générales ici..."></textarea>
            </div>
          </div>

          <div class="modal-footer-pro">
            <button class="btn-secondary-pro" @click="selectedEvent = null">Annuler</button>
            <button class="btn-primary-pro" @click="updateInterview" :disabled="updating">
              <Save v-if="!updating" :size="16" />
              <Loader2 v-else :size="16" class="spin" />
              {{ updating ? 'Enregistrement...' : 'Enregistrer les modifications' }}
            </button>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import Sidebar from '@/components/layout/Sidebar.vue'
import api from '@/api/axios'
import { useAuthStore } from '@/stores/authStore'
import { 
  Calendar as CalendarIcon, ChevronLeft, ChevronRight, 
  Plus, Clock, User, Info, Video, X, Save, MessageSquare, Loader2 
} from 'lucide-vue-next'
import { useToastStore } from '@/stores/toastStore'

const authStore = useAuthStore()
const toastStore = useToastStore()
const sidebarCollapsed = ref(false)
const currentDate = ref(new Date())
const interviews = ref([])
const loading = ref(true)
const updating = ref(false)
const activeFilter = ref('all')

const selectedEvent = ref(null)
const editForm = ref({ LocationOrLink: '', Message: '' })

const isDarkMode = computed(() => authStore.user?.theme === 'dark' || document.body.classList.contains('dark-mode'))

const filteredEvents = (dayEvents) => {
  if (activeFilter.value === 'all') return dayEvents
  return dayEvents.filter(e => e.Type === activeFilter.value)
}

const goToToday = () => {
  currentDate.value = new Date()
}

const formatDate = (date) => {
  return date.toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long' })
}

const fetchInterviews = async () => {
  try {
    loading.value = true
    const response = await api.get('/recruiter/interviews')
    interviews.value = response.data.map(i => {
      const type = i.type || i.Type || 'visio'
      const date = new Date(i.scheduledAt || i.ScheduledAt)
      return {
        ...i,
        id: i.id || i.Id,
        type: type,
        Type: type, // Support both for safety
        color: i.color || i.Color || '#64748b',
        Color: i.color || i.Color || '#64748b',
        date: date,
        title: i.jobTitle || i.JobTitle,
        candidate: i.candidateName || i.CandidateName,
        CandidateName: i.candidateName || i.CandidateName,
        JobTitle: i.jobTitle || i.JobTitle,
        time: date.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
      }
    })
  } catch (error) {
    console.error('Error fetching interviews:', error)
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  fetchInterviews()
})

const weekDays = ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim']

const currentMonthName = computed(() => {
  return currentDate.value.toLocaleString('fr-FR', { month: 'long' }).replace(/^\w/, c => c.toUpperCase())
})

const currentYear = computed(() => currentDate.value.getFullYear())

const calendarDays = computed(() => {
  const year = currentDate.value.getFullYear()
  const month = currentDate.value.getMonth()
  
  const firstDayOfMonth = new Date(year, month, 1)
  const lastDayOfMonth = new Date(year, month + 1, 0)
  
  let startDay = firstDayOfMonth.getDay() - 1
  if (startDay === -1) startDay = 6 
  
  const days = []
  
  const prevMonthLastDay = new Date(year, month, 0).getDate()
  for (let i = startDay - 1; i >= 0; i--) {
    days.push({
      date: new Date(year, month - 1, prevMonthLastDay - i),
      isCurrentMonth: false,
      isToday: false,
      events: []
    })
  }
  
  const today = new Date()
  for (let i = 1; i <= lastDayOfMonth.getDate(); i++) {
    const date = new Date(year, month, i)
    const dayEvents = interviews.value.filter(e => 
      e.date.getDate() === date.getDate() && 
      e.date.getMonth() === date.getMonth() && 
      e.date.getFullYear() === date.getFullYear()
    )
    
    days.push({
      date,
      isCurrentMonth: true,
      isToday: date.toDateString() === today.toDateString(),
      events: dayEvents
    })
  }
  
  const remainingCells = 42 - days.length
  for (let i = 1; i <= remainingCells; i++) {
    days.push({
      date: new Date(year, month + 1, i),
      isCurrentMonth: false,
      isToday: false,
      events: []
    })
  }
  
  return days
})

const upcomingEvents = computed(() => {
  return interviews.value
    .filter(e => e.date >= new Date())
    .sort((a, b) => a.date - b.date)
    .map(e => ({
      ...e,
      dayNumber: e.date.getDate(),
      monthName: e.date.toLocaleString('fr-FR', { month: 'short' }).toUpperCase().replace('.', ''),
      time: e.date.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })
    }))
})

const prevMonth = () => {
  currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() - 1, 1)
}

const nextMonth = () => {
  currentDate.value = new Date(currentDate.value.getFullYear(), currentDate.value.getMonth() + 1, 1)
}

const showEventDetails = (event) => {
  selectedEvent.value = event
  editForm.value = {
    LocationOrLink: event.LocationOrLink || event.locationOrLink || '',
    Message: event.Message || event.message || ''
  }
}

const updateInterview = async () => {
  if (!selectedEvent.value) return
  updating.value = true
  try {
    await api.patch(`/recruiter/interviews/${selectedEvent.value.Id || selectedEvent.value.id}`, {
      LocationOrLink: editForm.value.LocationOrLink,
      Message: editForm.value.Message
    })
    
    // Update local state
    const idx = interviews.value.findIndex(i => (i.Id || i.id) === (selectedEvent.value.Id || selectedEvent.value.id))
    if (idx !== -1) {
      interviews.value[idx].LocationOrLink = editForm.value.LocationOrLink
      interviews.value[idx].Message = editForm.value.Message
    }
    
    toastStore.success('Entretien mis à jour avec succès')
    selectedEvent.value = null
  } catch (error) {
    console.error('Error updating interview:', error)
    toastStore.error('Erreur lors de la mise à jour')
  } finally {
    updating.value = false
  }
}
</script>

<style scoped>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";

.main-content {
  flex: 1;
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  padding: 30px;
  position: relative;
  overflow-x: hidden;
  margin-left: 320px; 
}

:deep(.sidebar) {
  z-index: 9999 !important;
  pointer-events: auto !important;
}

.dark-mode .main-content { background: #0f172a; }
.ml-collapsed { margin-left: 100px !important; }

.content-wrapper {
  position: relative;
  z-index: 1;
  max-width: 1600px;
  margin: 0 auto;
}

/* HEADER PRO */
.calendar-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 40px;
}

.header-info { display: flex; align-items: center; gap: 24px; }
.premium-title-themed { font-size: 32px; font-weight: 900; margin: 0; letter-spacing: -1px; }
.subtitle { color: #64748b; font-size: 15px; margin-top: 4px; }

.header-actions-pro { display: flex; align-items: center; gap: 24px; }

.view-filters {
  display: flex;
  background: rgba(255, 255, 255, 0.5);
  padding: 4px;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.3);
  backdrop-filter: blur(10px);
}

.dark-mode .view-filters { background: rgba(30, 41, 59, 0.4); border-color: rgba(255, 255, 255, 0.05); }

.filter-chip {
  padding: 6px 16px;
  border-radius: 9px;
  font-size: 13px;
  font-weight: 700;
  color: #64748b;
  border: none;
  background: transparent;
  cursor: pointer;
  transition: all 0.3s ease;
}

.filter-chip.active { background: white; color: var(--primary); box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05); }
.dark-mode .filter-chip.active { background: var(--primary); color: white; }

.navigation-controls { display: flex; align-items: center; gap: 16px; }

.btn-today-nav {
  padding: 8px 16px;
  border-radius: 10px;
  font-size: 13px;
  font-weight: 700;
  background: rgba(255, 255, 255, 0.8);
  border: 1px solid rgba(255, 255, 255, 0.5);
  cursor: pointer;
  transition: all 0.2s;
}

.dark-mode .btn-today-nav { background: rgba(30, 41, 59, 0.6); color: #f8fafc; border-color: rgba(255, 255, 255, 0.1); }

.nav-arrows-pro {
  display: flex;
  align-items: center;
  gap: 12px;
  background: rgba(255, 255, 255, 0.5);
  padding: 4px 12px;
  border-radius: 12px;
  border: 1px solid rgba(255, 255, 255, 0.3);
}

.dark-mode .nav-arrows-pro { background: rgba(30, 41, 59, 0.4); border-color: rgba(255, 255, 255, 0.05); }

.btn-nav-arrow { background: transparent; border: none; color: #64748b; cursor: pointer; padding: 4px; border-radius: 6px; transition: all 0.2s; }
.btn-nav-arrow:hover { background: rgba(14, 165, 233, 0.1); color: var(--primary); }

.current-month-display { font-size: 16px; font-weight: 800; min-width: 140px; text-align: center; }

.btn-celestial-action {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 24px;
  border-radius: 14px;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  border: none;
  background: var(--primary);
  color: white;
  box-shadow: 0 10px 25px rgba(14, 165, 233, 0.3);
  transition: all 0.3s;
}

.btn-celestial-action:hover { transform: translateY(-2px); box-shadow: 0 15px 35px rgba(14, 165, 233, 0.4); }

/* GRID PRO */
.calendar-grid-container-pro { display: grid; grid-template-columns: 1fr 380px; gap: 30px; }

.calendar-card-pro {
  background: rgba(255, 255, 255, 0.7);
  border-radius: 32px;
  border: 1px solid rgba(255, 255, 255, 0.4);
  box-shadow: 0 25px 60px rgba(0, 0, 0, 0.05);
  overflow: hidden;
  backdrop-filter: blur(20px);
}

.dark-mode .calendar-card-pro { background: rgba(30, 41, 59, 0.4); border-color: rgba(255, 255, 255, 0.08); }

.days-header-pro {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  background: rgba(255, 255, 255, 0.3);
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
}

.day-label-pro { padding: 20px; text-align: center; font-size: 11px; font-weight: 900; color: #94a3b8; text-transform: uppercase; letter-spacing: 2px; }

.days-grid-pro { display: grid; grid-template-columns: repeat(7, 1fr); grid-auto-rows: minmax(140px, 1fr); }

.day-cell-pro { border-right: 1px solid rgba(0, 0, 0, 0.04); border-bottom: 1px solid rgba(0, 0, 0, 0.04); padding: 16px; transition: all 0.3s ease; position: relative; }
.dark-mode .day-cell-pro { border-color: rgba(255, 255, 255, 0.03); }
.day-cell-pro:hover { background: rgba(14, 165, 233, 0.02); }

.day-header-box { display: flex; align-items: center; gap: 8px; margin-bottom: 12px; }
.day-number-pro { font-size: 15px; font-weight: 800; color: #64748b; }
.dark-mode .day-number-pro { color: #94a3b8; }

.today-cell .day-number-pro { color: var(--primary); font-weight: 900; font-size: 18px; }
.today-marker { width: 6px; height: 6px; background: var(--primary); border-radius: 50%; box-shadow: 0 0 10px var(--primary); }

.weekend-cell { background: rgba(0, 0, 0, 0.01); }
.dark-mode .weekend-cell { background: rgba(255, 255, 255, 0.01); }

/* EVENT CARDS MINI */
.event-card-mini {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.8);
  border: 1px solid rgba(0, 0, 0, 0.05);
  margin-bottom: 6px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.dark-mode .event-card-mini { background: rgba(30, 41, 59, 0.6); border-color: rgba(255, 255, 255, 0.05); }

.event-card-mini:hover { transform: translateY(-3px) scale(1.02); box-shadow: 0 10px 20px rgba(0, 0, 0, 0.08); }

.event-accent { width: 4px; height: 16px; border-radius: 2px; }
.event-time-mini { font-size: 10px; font-weight: 900; color: #94a3b8; }
.event-name-mini { font-size: 12px; font-weight: 700; color: #1e293b; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.dark-mode .event-name-mini { color: #f1f5f9; }

/* SIDE PANEL PRO */
.side-upcoming-panel {
  background: rgba(255, 255, 255, 0.7);
  border-radius: 32px;
  padding: 30px;
  border: 1px solid rgba(255, 255, 255, 0.4);
  box-shadow: 0 25px 60px rgba(0, 0, 0, 0.05);
  display: flex;
  flex-direction: column;
  backdrop-filter: blur(20px);
}

.dark-mode .side-upcoming-panel { background: rgba(30, 41, 59, 0.4); border-color: rgba(255, 255, 255, 0.08); }

.panel-header-pro { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
.panel-title-pro { font-size: 18px; font-weight: 900; display: flex; align-items: center; gap: 10px; }
.upcoming-badge { padding: 4px 10px; background: rgba(14, 165, 233, 0.1); color: var(--primary); border-radius: 20px; font-size: 12px; font-weight: 900; }

.upcoming-item-card {
  display: flex;
  gap: 16px;
  padding: 20px;
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.5);
  border: 1px solid transparent;
  margin-bottom: 16px;
  transition: all 0.3s ease;
}

.dark-mode .upcoming-item-card { background: rgba(15, 23, 42, 0.3); }

.upcoming-item-card:hover { transform: translateX(8px); background: white; border-color: var(--primary); box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05); }

.item-date-square {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 54px;
  height: 54px;
  background: white;
  border-radius: 14px;
  box-shadow: 0 8px 20px rgba(0, 0, 0, 0.06);
  flex-shrink: 0;
}

.item-day { font-size: 20px; font-weight: 900; color: #1e293b; line-height: 1; }
.item-month { font-size: 10px; font-weight: 800; color: #94a3b8; text-transform: uppercase; }

.item-job-title { font-size: 14px; font-weight: 800; color: #1e293b; margin-bottom: 2px; }
.dark-mode .item-job-title { color: #f1f5f9; }
.item-candidate-name { font-size: 13px; font-weight: 500; color: #64748b; margin-bottom: 10px; }

.item-footer-meta { display: flex; align-items: center; gap: 12px; }
.item-time-stamp { font-size: 11px; font-weight: 700; color: #94a3b8; display: flex; align-items: center; gap: 4px; }
.item-type-pill { font-size: 10px; font-weight: 900; padding: 4px 8px; border-radius: 6px; text-transform: uppercase; letter-spacing: 0.5px; }

/* UTILS PRO */
.loading-container-pro { display: flex; flex-direction: column; align-items: center; justify-content: center; min-height: 400px; gap: 20px; color: #64748b; font-weight: 600; }
.nebula-spinner-pro { width: 50px; height: 50px; border: 4px solid rgba(14, 165, 233, 0.1); border-top-color: var(--primary); border-radius: 50%; animation: spin 1s linear infinite; }

@keyframes spin { to { transform: rotate(360deg); } }

/* CELESTIAL BG */
.celestial-bg { position: absolute; inset: 0; z-index: 0; overflow: hidden; pointer-events: none; opacity: 0.3; }
.dark-mode .celestial-bg { opacity: 0.6; }
.c-orb { position: absolute; border-radius: 50%; filter: blur(120px); opacity: 0.15; animation: float 20s infinite alternate; }
.orb-1 { width: 600px; height: 600px; background: var(--primary); top: -200px; right: -100px; }
.orb-2 { width: 500px; height: 500px; background: #8b5cf6; bottom: -100px; left: -100px; }

@keyframes float { 0% { transform: translate(0, 0) scale(1); } 100% { transform: translate(100px, 50px) scale(1.1); } }

.icon-box-themed {
  width: 56px;
  height: 56px;
  background: rgba(14, 165, 233, 0.1);
  color: var(--primary);
  border-radius: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* MODAL PRO STYLES */
.modal-overlay-pro {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(8px);
  z-index: 10000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.modal-content-pro {
  background: rgba(255, 255, 255, 0.9);
  width: 100%;
  max-width: 600px;
  border-radius: 32px;
  border: 1px solid rgba(255, 255, 255, 0.5);
  box-shadow: 0 40px 100px rgba(0, 0, 0, 0.15);
  overflow: hidden;
}

.dark-mode .modal-content-pro {
  background: rgba(30, 41, 59, 0.8);
  border-color: rgba(255, 255, 255, 0.08);
}

.modal-header-pro {
  padding: 30px;
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  border-bottom: 1px solid rgba(0, 0, 0, 0.05);
}

.dark-mode .modal-header-pro { border-color: rgba(255, 255, 255, 0.05); }

.header-main-pro { display: flex; gap: 20px; align-items: center; }
.event-type-icon { width: 44px; height: 44px; border-radius: 12px; display: flex; align-items: center; justify-content: center; color: white; }

.header-main-pro h3 { margin: 0; font-size: 20px; font-weight: 900; }
.header-main-pro p { margin: 4px 0 0; font-size: 14px; color: #64748b; }

.btn-close-pro { background: transparent; border: none; color: #64748b; cursor: pointer; padding: 4px; }

.modal-body-pro { padding: 30px; display: flex; flex-direction: column; gap: 24px; }

.info-grid-pro { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.info-item-pro label { display: flex; align-items: center; gap: 6px; font-size: 11px; font-weight: 900; color: #94a3b8; text-transform: uppercase; margin-bottom: 8px; }
.info-item-pro p { margin: 0; font-size: 15px; font-weight: 700; }

.type-badge-pro { display: inline-block; padding: 4px 12px; border-radius: 8px; font-size: 11px; font-weight: 900; }

.form-group-pro { display: flex; flex-direction: column; gap: 10px; }
.form-group-pro label { display: flex; align-items: center; gap: 6px; font-size: 11px; font-weight: 900; color: #94a3b8; text-transform: uppercase; }

.form-group-pro input, .form-group-pro textarea {
  width: 100%;
  padding: 14px 18px;
  border-radius: 16px;
  border: 1px solid rgba(0, 0, 0, 0.1);
  background: white;
  font-size: 14px;
  font-weight: 600;
  outline: none;
  transition: all 0.3s;
}

.dark-mode .form-group-pro input, .dark-mode .form-group-pro textarea {
  background: rgba(15, 23, 42, 0.5);
  border-color: rgba(255, 255, 255, 0.1);
  color: white;
}

.form-group-pro input:focus, .form-group-pro textarea:focus { border-color: var(--primary); box-shadow: 0 0 0 4px rgba(14, 165, 233, 0.1); }

.modal-footer-pro { padding: 20px 30px; background: rgba(0, 0, 0, 0.02); display: flex; justify-content: flex-end; gap: 16px; }
.dark-mode .modal-footer-pro { background: rgba(255, 255, 255, 0.02); }

.btn-secondary-pro {
  padding: 12px 24px;
  border-radius: 14px;
  border: 1px solid rgba(0, 0, 0, 0.1);
  background: white;
  font-weight: 700;
  cursor: pointer;
}

.dark-mode .btn-secondary-pro { background: rgba(30, 41, 59, 0.6); color: white; border-color: rgba(255, 255, 255, 0.1); }

.btn-primary-pro {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 24px;
  border-radius: 14px;
  border: none;
  background: var(--primary);
  color: white;
  font-weight: 700;
  cursor: pointer;
  box-shadow: 0 10px 20px rgba(14, 165, 233, 0.2);
}

.anim-scale-up { animation: scaleUp 0.3s cubic-bezier(0.34, 1.56, 0.64, 1); }
@keyframes scaleUp { from { transform: scale(0.9); opacity: 0; } to { transform: scale(1); opacity: 1; } }

.spin { animation: spin 1s linear infinite; }
</style>
