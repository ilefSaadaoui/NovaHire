<template>
  <div class="identity-card-premium anim-reveal-up">
    <div class="card-glow-bg"></div>
    
    <div class="identity-content">
      <div class="identity-top">
        <div class="avatar-premium-container">
          <div class="avatar-glow-ring"></div>
          <div class="avatar-main" :style="avatarStyle">
            {{ candidate.initials }}
          </div>
        </div>
        
        <div class="identity-names">
          <h3 class="full-name-pro">{{ formattedName }}</h3>
          <div class="status-indicator">
            <span class="pulse-dot"></span>
            {{ currentStatusLabel }}
          </div>
        </div>
      </div>

      <div class="contact-grid-premium">
        <div class="contact-item-pro" @mouseenter="hovered = 'mail'" @mouseleave="hovered = null">
          <div class="icon-box" :class="{ 'glow': hovered === 'mail' }">
            <Mail :size="16" />
          </div>
          <div class="contact-info">
            <span class="contact-label">Email</span>
            <span class="contact-value">{{ candidate.email }}</span>
          </div>
        </div>

        <div class="contact-item-pro" @mouseenter="hovered = 'phone'" @mouseleave="hovered = null">
          <div class="icon-box" :class="{ 'glow': hovered === 'phone' }">
            <Phone :size="16" />
          </div>
          <div class="contact-info">
            <span class="contact-label">Téléphone</span>
            <span class="contact-value">{{ candidate.phone }}</span>
          </div>
        </div>
      </div>

      <div class="pipeline-section-pro">
        <div class="section-header-compact">
          <Workflow :size="12" />
          <span>Étape du Pipeline</span>
        </div>

        <div
          class="pipeline-dropdown-wrapper"
          ref="dropdownRef"
          :class="{ 'is-open': isOpen }"
        >
          <button
            type="button"
            class="pipeline-select-trigger"
            :class="currentStage"
            @click="isOpen = !isOpen"
          >
            <div class="status-marker" :class="currentStage"></div>
            <span class="trigger-label">{{ currentLabel }}</span>
            <ChevronDown class="chevron-pro" :size="14" />
          </button>

          <transition name="dropdown-slide">
            <ul v-if="isOpen" class="pipeline-menu-pro">
              <li
                v-for="opt in options"
                :key="opt.value"
                class="menu-option-pro"
                :class="[opt.value, { 'is-active': opt.value === currentStage }]"
                @click="selectOption(opt.value)"
              >
                <div class="status-marker" :class="opt.value"></div>
                <span>{{ opt.label }}</span>
                <Check v-if="opt.value === currentStage" class="check-icon-pro" :size="14" />
              </li>
            </ul>
          </transition>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { Mail, Phone, ChevronDown, Check, Workflow } from 'lucide-vue-next'

const props = defineProps({
  candidate: Object,
  currentStage: String,
  avatarStyle: Object
})

const emit = defineEmits(['update-stage'])

const hovered = ref(null)
const isOpen = ref(false)
const dropdownRef = ref(null)

const options = [
  { value: 'submitted', label: 'Nouvelle candidature' },
  { value: 'underreview', label: "En cours d'examen" },
  { value: 'shortlisted', label: 'Présélectionné' },
  { value: 'interview', label: 'Entretien prévu' },
  { value: 'interviewed', label: 'Entretien réalisé' },
  { value: 'offersent', label: 'Offre envoyée' },
  { value: 'accepted', label: 'Accepté (Recruté)' },
  { value: 'rejected', label: 'Refusé' }
]

const formattedName = computed(() => {
  if (!props.candidate?.fullName) return '...'
  return props.candidate.fullName
    .toLowerCase()
    .split(' ')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join(' ')
})

const currentLabel = computed(() => {
  const match = options.find(o => o.value === props.currentStage)
  return match ? match.label : 'Sélectionner...'
})

const currentStatusLabel = computed(() => {
  if (props.currentStage === 'offersent') return 'Offre en attente'
  if (props.currentStage === 'accepted') return 'Candidat recruté'
  if (props.currentStage === 'interview') return 'Entretien à venir'
  return 'Profil Actif'
})

const selectOption = (value) => {
  emit('update-stage', value)
  isOpen.value = false
}

const handleClickOutside = (e) => {
  if (dropdownRef.value && !dropdownRef.value.contains(e.target)) {
    isOpen.value = false
  }
}

onMounted(() => document.addEventListener('mousedown', handleClickOutside))
onBeforeUnmount(() => document.removeEventListener('mousedown', handleClickOutside))
</script>

<style scoped>
.identity-card-premium {
  position: relative;
  background: white;
  border-radius: 28px;
  padding: 32px;
  border: 1px solid rgba(226, 232, 240, 0.8);
  box-shadow: 0 10px 30px -10px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

.card-glow-bg {
  position: absolute;
  top: -50px;
  right: -50px;
  width: 150px;
  height: 150px;
  background: var(--accent-soft);
  filter: blur(60px);
  border-radius: 50%;
  opacity: 0.5;
  pointer-events: none;
}

.identity-content {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.identity-top {
  display: flex;
  align-items: center;
  gap: 20px;
}

.avatar-premium-container {
  position: relative;
  width: 72px;
  height: 72px;
}

.avatar-glow-ring {
  position: absolute;
  inset: -4px;
  border-radius: 24px;
  background: var(--accent-grad);
  opacity: 0.15;
  filter: blur(4px);
}

.avatar-main {
  position: relative;
  width: 100%;
  height: 100%;
  border-radius: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 26px;
  font-weight: 900;
  color: white;
  box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
}

.full-name-pro {
  font-size: 20px;
  font-weight: 900;
  color: #0f172a;
  margin: 0 0 4px 0;
  letter-spacing: -0.01em;
}

.status-indicator {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 12px;
  font-weight: 700;
  color: #64748b;
  text-transform: uppercase;
}

.pulse-dot {
  width: 8px;
  height: 8px;
  background: #10b981;
  border-radius: 50%;
  box-shadow: 0 0 0 rgba(16, 185, 129, 0.4);
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0.4); }
  70% { box-shadow: 0 0 0 10px rgba(16, 185, 129, 0); }
  100% { box-shadow: 0 0 0 0 rgba(16, 185, 129, 0); }
}

.contact-grid-premium {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.contact-item-pro {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 12px;
  border-radius: 16px;
  transition: all 0.3s;
  cursor: pointer;
}

.contact-item-pro:hover {
  background: #f8fafc;
}

.icon-box {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #64748b;
  transition: all 0.3s;
}

.icon-box.glow {
  background: var(--accent);
  color: white;
  box-shadow: 0 4px 12px var(--accent-soft);
}

.contact-info {
  display: flex;
  flex-direction: column;
}

.contact-label {
  font-size: 11px;
  font-weight: 800;
  color: #94a3b8;
  text-transform: uppercase;
}

.contact-value {
  font-size: 14px;
  font-weight: 600;
  color: #334155;
}

.pipeline-section-pro {
  border-top: 1px solid #f1f5f9;
  padding-top: 24px;
}

.section-header-compact {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 11px;
  font-weight: 900;
  color: #94a3b8;
  text-transform: uppercase;
  margin-bottom: 12px;
}

.pipeline-dropdown-wrapper {
  position: relative;
}

.pipeline-select-trigger {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 14px 16px;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 16px;
  cursor: pointer;
  transition: all 0.3s;
}

.pipeline-select-trigger:hover {
  background: white;
  border-color: var(--accent);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.05);
}

.status-marker {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--status-col, #94a3b8);
  box-shadow: 0 0 8px var(--status-col, #94a3b8);
}

.status-marker.submitted { --status-col: #fbbf24; }
.status-marker.underreview { --status-col: #f59e0b; }
.status-marker.shortlisted { --status-col: #6366f1; }
.status-marker.interview { --status-col: #818cf8; }
.status-marker.offersent { --status-col: #3b82f6; }
.status-marker.accepted { --status-col: #10b981; }
.status-marker.rejected { --status-col: #f43f5e; }

.trigger-label {
  flex: 1;
  text-align: left;
  font-size: 14px;
  font-weight: 700;
  color: #1e293b;
}

.chevron-pro {
  color: #94a3b8;
  transition: transform 0.3s;
}

.is-open .chevron-pro {
  transform: rotate(180deg);
  color: var(--accent);
}

.pipeline-menu-pro {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  right: 0;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 18px;
  padding: 8px;
  box-shadow: 0 15px 35px rgba(0, 0, 0, 0.1);
  z-index: 100;
  list-style: none;
}

.menu-option-pro {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 14px;
  border-radius: 12px;
  font-size: 14px;
  font-weight: 600;
  color: #475569;
  cursor: pointer;
  transition: all 0.2s;
}

.menu-option-pro:hover {
  background: #f1f5f9;
  color: #0f172a;
}

.menu-option-pro.is-active {
  background: var(--accent-soft);
  color: var(--accent);
}

.check-icon-pro {
  margin-left: auto;
}

.dropdown-slide-enter-active, .dropdown-slide-leave-active {
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.dropdown-slide-enter-from, .dropdown-slide-leave-to {
  opacity: 0;
  transform: translateY(-10px) scale(0.95);
}

.anim-reveal-up {
  animation: revealUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes revealUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Dark Mode Overrides - Premium Enhancement */
.dark-mode .identity-card-premium {
  background: #020617; /* Deeper OLED Black/Navy */
  border-color: var(--accent-soft);
  box-shadow: 0 10px 40px rgba(0, 0, 0, 0.6);
}

.dark-mode .full-name-pro {
  color: #ffffff;
}

.dark-mode .contact-item-pro:hover {
  background: var(--accent-soft);
}

.dark-mode .icon-box {
  background: rgba(255, 255, 255, 0.03);
  color: #64748b;
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.dark-mode .icon-box.glow {
  background: var(--accent);
  color: white;
  box-shadow: 0 0 20px var(--accent-glow);
}

.dark-mode .contact-value {
  color: #e2e8f0;
}

.dark-mode .pipeline-section-pro {
  border-color: rgba(255, 255, 255, 0.05);
}

.dark-mode .pipeline-select-trigger {
  background: rgba(2, 6, 23, 0.4);
  border-color: var(--accent-soft);
}

.dark-mode .pipeline-select-trigger:hover {
  background: rgba(2, 6, 23, 0.6);
  border-color: var(--accent);
  box-shadow: 0 0 15px var(--accent-glow);
}

.dark-mode .trigger-label {
  color: #ffffff;
}

.dark-mode .pipeline-menu-pro {
  background: #0f172a;
  border-color: var(--accent-glow);
  box-shadow: 0 15px 50px rgba(0, 0, 0, 0.8);
}

.dark-mode .menu-option-pro {
  color: #94a3b8;
}

.dark-mode .menu-option-pro:hover {
  background: var(--accent-soft);
  color: #ffffff;
}

.dark-mode .menu-option-pro.is-active {
  background: var(--accent-soft);
  color: var(--accent);
}
</style>

