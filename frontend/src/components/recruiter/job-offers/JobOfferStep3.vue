<template>
  <div class="step-content">
    <div class="section-header-premium">
      <div class="header-main">
        <h3>Étape 3 : Expérience Candidat & IA</h3>
        <p class="step-subtitle">Configurez le formulaire et les critères de matching intelligent</p>
      </div>
    </div>

    <!-- Section: Configuration du Formulaire -->
    <div class="form-section-premium lux-shadow">
      <div class="section-title-lux">
        <div class="title-badge">
          <span class="num">06</span>
          <div class="icon-wrap-lux">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><polyline points="10 9 9 9 8 9"/></svg>
          </div>
        </div>
        <div class="title-text-lux">
          <h4>Champs du Formulaire</h4>
          <p>Configurez les informations requises</p>
        </div>
      </div>

      <div class="form-config-expert-grid">
        <div v-for="field in formFields" :key="field.id" class="config-card-lux" :class="{ 'is-disabled': !field.enabled, 'is-required': field.required }">
          <div class="card-main-lux">
            <div class="f-icon-lux">
              <svg v-if="field.id === 'fullname'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
              <svg v-else-if="field.id === 'email'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
              <svg v-else-if="field.id === 'phone'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
              <svg v-else-if="field.id === 'resume'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M13 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V9z"/><polyline points="13 2 13 9 20 9"/></svg>
              <svg v-else-if="field.id === 'coverLetter'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
              <svg v-else-if="field.id === 'linkedin'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M16 8a6 6 0 0 1 6 6v7h-4v-7a2 2 0 0 0-2-2 2 2 0 0 0-2 2v7h-4v-7a6 6 0 0 1 6-6z"/><rect x="2" y="9" width="4" height="12"/><circle cx="4" cy="4" r="2"/></svg>
              <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
            </div>
            <div class="field-info-lux">
              <span class="f-name-lux">{{ field.name }}</span>
              <p class="f-desc-lux">{{ field.desc }}</p>
            </div>
          </div>
          
          <div class="field-controls-lux">
            <label class="toggle-lux" title="Afficher ce champ">
              <input 
                type="checkbox" 
                v-model="field.enabled" 
                :disabled="!field.canDisable" 
                @change="!field.enabled ? field.required = false : null"
              >
              <span class="t-slider"></span>
              <span class="t-label">Actif</span>
            </label>
            <div class="sep-v"></div>
            <label class="toggle-lux" :class="{ 'is-locked': !field.enabled }" title="Rendre obligatoire">
              <input 
                type="checkbox" 
                v-model="field.required" 
                :disabled="!field.enabled" 
                @change="field.required ? field.enabled = true : null"
              >
              <span class="t-slider req"></span>
              <span class="t-label">Requis</span>
            </label>
          </div>
        </div>
      </div>
    </div>

    <!-- Section: Matching IA -->
    <div class="form-section-premium mt-40 lux-shadow">
      <div class="section-title-lux">
        <div class="title-badge">
          <span class="num">07</span>
          <div class="icon-wrap-lux">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg>
          </div>
        </div>
        <div class="title-text-lux">
          <h4>Intelligence de Scoring IA</h4>
          <p>Pondération des critères de matching</p>
        </div>
      </div>
      
      <div class="ai-intelligence-card-lux">
        <div class="matching-master-header">
           <div class="total-score-badge" :class="{ 'warning': totalWeight !== 100 }">
              <span class="score-val">{{ totalWeight }}</span>
              <span class="score-unit">%</span>
           </div>
           <div class="score-description">
              <h5>Pondération du Matching</h5>
              <p v-if="totalWeight !== 100" class="err-text">Répartissez les points pour atteindre précisément 100%.</p>
              <p v-else class="success-text">Configuration optimale pour l'algorithme.</p>
           </div>
           <button type="button" class="btn-reset-lux" @click="resetWeights">
              <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="3"><path d="M23 4v6h-6"/><path d="M20.49 15a9 9 0 1 1-2.12-9.36L23 10"/></svg>
              Équilibrer
           </button>
        </div>

        <div class="weights-grid-lux">
          <div v-for="w in weightConfigs" :key="w.id" class="weight-control-lux">
            <div class="wc-header">
              <span class="wc-icon" :style="{ color: w.color }" v-html="w.icon"></span>
              <div class="wc-text">
                <label>{{ w.label }}</label>
                <div class="wc-bar-wrap">
                   <div class="wc-bar-fill" :style="{ width: form[w.key] + '%', background: w.color }"></div>
                </div>
              </div>
              <div class="wc-value" :style="{ color: w.color }">{{ form[w.key] }}%</div>
            </div>
            <input 
              type="range" 
              :value="form[w.key]" 
              @input="$emit('update:form', { ...form, [w.key]: parseInt($event.target.value) })"
              min="0" max="100" class="premium-range-lux"
              :style="{ '--accent-col': w.color }"
            >
          </div>
        </div>
      </div>
    </div>

    <!-- Section: Filtre Automatique IA -->
    <div class="form-section-premium mt-40 lux-shadow">
      <div class="section-title-lux">
        <div class="title-badge">
          <span class="num">⚡</span>
          <div class="icon-wrap-lux">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/></svg>
          </div>
        </div>
        <div class="title-text-lux">
          <h4>Filtre Automatique IA</h4>
          <p>Rejet automatique sous un seuil de score IA (0 = désactivé)</p>
        </div>
      </div>

      <div class="ai-intelligence-card-lux reject-card">
        <div class="wc-header">
          <span class="wc-icon" style="color: #ef4444">
            <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>
          </span>
          <div class="wc-text">
            <label>Seuil de rejet automatique</label>
            <div class="wc-bar-wrap">
              <div class="wc-bar-fill" :style="{ width: form.autoRejectThreshold + '%', background: '#ef4444' }"></div>
            </div>
          </div>
          <div class="wc-value" :style="{ color: form.autoRejectThreshold > 0 ? '#ef4444' : 'var(--r-text-sub)' }">
            {{ form.autoRejectThreshold }}%
          </div>
        </div>
        <input
          type="range"
          :value="form.autoRejectThreshold"
          @input="$emit('update:form', { ...form, autoRejectThreshold: parseInt($event.target.value) })"
          min="0" max="80" step="5"
          class="premium-range-lux reject-range"
        >
        <div v-if="form.autoRejectThreshold > 0" class="auto-reject-hint">
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
          Tout candidat avec un score IA &lt; {{ form.autoRejectThreshold }}% sera rejeté automatiquement.
        </div>
        <div v-else class="auto-reject-hint inactive-hint">
          <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>
          Filtre désactivé — tous les candidats seront acceptés pour analyse.
        </div>
      </div>
    </div>

    <!-- Section: Workflow -->
    <div class="form-section-premium mt-40 lux-shadow">
      <div class="section-title-lux">
        <div class="title-badge">
          <span class="num">08</span>
          <div class="icon-wrap-lux">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>
          </div>
        </div>
        <div class="title-text-lux">
          <h4>Validation & Publication</h4>
          <p>Statut et planification de l'annonce</p>
        </div>
      </div>
      <div class="r-form-row-lux">
        <div class="r-input-group" :class="{ 'has-error': errors.deadline }">
          <label>Date de clôture</label>
          <div class="date-input-lux-wrap">
            <input 
              type="text"
              :value="localDateDisplay"
              @input="handleDateInput"
              @blur="handleDateBlur"
              placeholder="jj-mm-aaaa"
              class="expert-date-lux"
              :class="{ 'date-invalid': dateError }"
              maxlength="10"
              autocomplete="off"
            >
            <input 
              ref="datePickerInput"
              type="date" 
              :value="form.deadline"
              @change="handleDatePickerChange"
              class="date-picker-hidden"
            >
            <button
              type="button"
              @click="openDatePicker"
              class="btn-calendar-lux"
              title="Sélectionner une date"
            >
              <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5">
                <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/>
                <line x1="16" y1="2" x2="16" y2="6"/>
                <line x1="8" y1="2" x2="8" y2="6"/>
                <line x1="3" y1="10" x2="21" y2="10"/>
              </svg>
            </button>
          </div>
          <span v-if="dateError" class="error-msg date-err">{{ dateError }}</span>
          <span v-else-if="errors.deadline" class="error-msg">{{ errors.deadline }}</span>
        </div>
        <div class="r-input-group">
          <label>Visibilité de l'offre</label>
          <div class="status-select-lux-wrap">
            <PremiumSelect 
              :modelValue="form.status"
              @update:modelValue="$emit('update:form', { ...form, status: $event })"
              :options="statusOptions"
              class="expert-select-lux"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref, watch } from 'vue'
import PremiumSelect from '@/components/common/PremiumSelect.vue'

const props = defineProps({
  form: Object,
  errors: Object,
  formFields: Array,
  totalWeight: Number
})

// Date format conversion functions
const formatDateToDisplay = (dateStr) => {
  if (!dateStr) return ''
  const parts = dateStr.split('-')
  if (parts.length === 3 && parts[0].length === 4) {
    const [year, month, day] = parts
    return `${day}-${month}-${year}`
  }
  return dateStr
}

const isValidDate = (day, month, year) => {
  if (year < 1900 || year > 2100) return false
  if (month < 1 || month > 12) return false
  if (day < 1) return false
  const daysInMonth = new Date(year, month, 0).getDate()
  if (day > daysInMonth) return false
  // Date must be in the future
  const inputDate = new Date(year, month - 1, day)
  const today = new Date()
  today.setHours(0, 0, 0, 0)
  if (inputDate < today) return 'future'
  return true
}

const formatDateToISO = (dateStr) => {
  if (!dateStr) return ''
  const parts = dateStr.split('-')
  if (parts.length === 3 && parts[0].length === 2 && parts[1].length === 2 && parts[2].length === 4) {
    const [day, month, year] = parts
    return `${year}-${month}-${day}`
  }
  return ''
}

const emit = defineEmits(['update:form', 'validate'])
const datePickerInput = ref(null)
const localDateDisplay = ref(formatDateToDisplay(props.form.deadline))
const dateError = ref('')

// Sync display if prop changes from outside
watch(() => props.form.deadline, (newVal) => {
  localDateDisplay.value = formatDateToDisplay(newVal)
})

const handleDateInput = (event) => {
  let raw = event.target.value

  // If user is deleting, just allow it without auto-formatting
  if (raw.length < localDateDisplay.value.length) {
    localDateDisplay.value = raw
    event.target.value = raw
    dateError.value = ''
    if (raw.length === 0) emit('update:form', { ...props.form, deadline: '' })
    return
  }

  let digits = raw.replace(/[^\d]/g, '')

  // Smart Masking - Day
  if (digits.length === 1 && parseInt(digits[0]) > 3) digits = '0' + digits
  if (digits.length >= 2) {
    let day = parseInt(digits.slice(0, 2))
    if (day > 31) digits = '31' + digits.slice(2)
    if (day === 0) digits = '01' + digits.slice(2)
  }

  // Smart Masking - Month
  if (digits.length === 3 && parseInt(digits[2]) > 1) digits = digits.slice(0, 2) + '0' + digits.slice(2)
  if (digits.length >= 4) {
    let month = parseInt(digits.slice(2, 4))
    if (month > 12) digits = digits.slice(0, 2) + '12' + digits.slice(4)
    if (month === 0) digits = digits.slice(0, 2) + '01' + digits.slice(4)
  }

  // Re-format with dashes
  let formatted = digits
  if (digits.length > 2) formatted = digits.slice(0, 2) + '-' + digits.slice(2)
  if (digits.length > 4) formatted = digits.slice(0, 2) + '-' + digits.slice(2, 4) + '-' + digits.slice(4, 8)

  localDateDisplay.value = formatted
  event.target.value = formatted
  dateError.value = ''

  // Validate only when fully typed
  if (formatted.length === 10) {
    const parts = formatted.split('-')
    if (parts.length === 3) {
      const [d, m, y] = parts.map(Number)
      const valid = isValidDate(d, m, y)
      if (valid === 'future') {
        dateError.value = 'La date doit être dans le futur'
        return
      } else if (valid !== true) {
        dateError.value = 'Date invalide (ex: 31-12-2025)'
        return
      }
      const isoValue = `${y}-${String(m).padStart(2,'0')}-${String(d).padStart(2,'0')}`
      emit('update:form', { ...props.form, deadline: isoValue })
      emit('validate', 'deadline')
    }
  }
}

const handleDateBlur = () => {
  if (!localDateDisplay.value) return
  if (localDateDisplay.value.length < 10) {
    dateError.value = 'Format attendu : jj-mm-aaaa'
  }
}

const handleDatePickerChange = (event) => {
  const isoValue = event.target.value
  localDateDisplay.value = formatDateToDisplay(isoValue)
  emit('update:form', { ...props.form, deadline: isoValue })
  emit('validate', 'deadline')
}

const openDatePicker = () => {
  if (!datePickerInput.value) return
  if (typeof datePickerInput.value.showPicker === 'function') {
    datePickerInput.value.showPicker()
    return
  }
  datePickerInput.value.focus()
  datePickerInput.value.click()
}

const statusOptions = [
  { 
    label: 'Publier immédiatement', 
    value: 'Published',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="m22 2-7 20-4-9-9-4Z"/><path d="M22 2 11 13"/></svg>'
  },
  { 
    label: 'Garder en Brouillon', 
    value: 'Draft',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"/><polyline points="17 21 17 13 7 13 7 21"/><polyline points="7 3 7 8 15 8"/></svg>'
  }
]

const weightConfigs = [
  { 
    id: 1, key: 'weightExperience', label: 'Expériences Professionnelles', color: '#6366f1',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="2" y="7" width="20" height="14" rx="2" ry="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>'
  },
  { 
    id: 2, key: 'weightEducation', label: 'Formation & Diplômes', color: '#10b981',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M22 10v6M2 10l10-5 10 5-10 5z"/><path d="M6 12v5c3 3 9 3 12 0v-5"/></svg>'
  },
  { 
    id: 3, key: 'weightSkills', label: 'Compétences Techniques', color: '#f59e0b',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/></svg>'
  }
]

const resetWeights = () => {
  emit('update:form', { 
    ...props.form, 
    weightExperience: 33, 
    weightEducation: 33, 
    weightSkills: 34 
  })
}
</script>

<style scoped>
.section-header-premium { margin-bottom: 40px; border-bottom: 1px solid rgba(99, 102, 241, 0.08); padding-bottom: 24px; position: relative; }
.section-header-premium::after { content: ''; position: absolute; bottom: -1px; left: 0; width: 60px; height: 1px; background: var(--accent); opacity: 0.5; }
.section-header-premium h3 { font-size: 26px; font-weight: 950; color: var(--r-text-main); margin: 0; letter-spacing: -0.5px; }

.form-section-premium { 
  background: var(--r-surface); 
  padding: 40px; 
  border-radius: 32px; 
  border: 1px solid var(--r-border);
  transition: 0.4s cubic-bezier(0.2, 0.8, 0.2, 1);
  box-shadow: 
    0 15px 50px -20px rgba(99, 102, 241, 0.08),
    0 0 0 1px rgba(99, 102, 241, 0.05);
}

.section-title-lux { display: flex; align-items: center; gap: 24px; margin-bottom: 40px; }
.title-badge { 
  display: flex; align-items: center; gap: 8px; 
  background: var(--accent-grad); padding: 8px 16px; 
  border-radius: 16px; 
  box-shadow: 0 4px 15px var(--accent-soft);
}
.title-badge .num { 
  font-size: 14px; font-weight: 950; color: white; 
  opacity: 0.8;
}
.icon-wrap-lux { 
  width: 24px; height: 24px; color: white; 
  display: flex; align-items: center; justify-content: center; 
}

.title-text-lux h4 { font-size: 22px; font-weight: 950; color: var(--r-text-main); margin: 0; letter-spacing: -0.02em; }
.title-text-lux p { font-size: 13px; color: var(--r-text-sub); font-weight: 600; margin-top: 4px; }

/* Grid for Form Config */
.form-config-expert-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(320px, 1fr)); gap: 24px; }

.config-card-lux {
  background: var(--r-surface); border: 1.5px solid var(--r-border);
  border-radius: 24px; padding: 24px;
  display: flex; flex-direction: column; gap: 20px;
  transition: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative; overflow: hidden;
}
.config-card-lux:hover { border-color: var(--accent); transform: translateY(-4px); box-shadow: 0 15px 35px -10px rgba(0,0,0,0.1); }
.config-card-lux.is-disabled { opacity: 0.8; filter: grayscale(1); }
.config-card-lux.is-required::before {
  content: "OBLIGATOIRE"; position: absolute; top: 12px; right: -25px; 
  background: var(--accent); color: white; font-size: 9px; font-weight: 900;
  padding: 4px 30px; transform: rotate(45deg);
}

.card-main-lux { display: flex; gap: 16px; align-items: flex-start; }
.f-icon-lux {
  min-width: 48px; height: 48px; border-radius: 14px;
  background: var(--bg-hover); color: var(--accent);
  display: flex; align-items: center; justify-content: center;
}
.f-icon-lux svg { width: 24px; }

.field-info-lux { flex: 1; }
.f-name-lux { font-size: 16px; font-weight: 800; color: var(--r-text-main); display: block; }
.f-desc-lux { font-size: 12px; color: var(--r-text-sub); margin-top: 4px; line-height: 1.5; font-weight: 500; }

.field-controls-lux { display: flex; gap: 16px; align-items: center; padding-top: 16px; border-top: 1px solid var(--r-border); }
.sep-v { width: 1.5px; height: 16px; background: var(--r-border); opacity: 0.5; }

/* Toggle Lux */
.toggle-lux { display: flex; align-items: center; gap: 10px; cursor: pointer; }
.toggle-lux input { display: none; }
.t-slider { width: 36px; height: 20px; background: var(--r-border); border-radius: 10px; position: relative; transition: 0.3s; }
.t-slider::before { content: ''; position: absolute; left: 4px; top: 4px; width: 12px; height: 12px; background: white; border-radius: 50%; transition: 0.3s; }
input:checked + .t-slider { background: #10b981; }
input:checked + .t-slider.req { background: var(--accent); }
input:checked + .t-slider::before { transform: translateX(16px); }
.t-label { font-size: 11px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 0.5px; }

/* AI Intelligence Card */
.ai-intelligence-card-lux {
  background: var(--r-surface);
  border: 1.5px solid var(--r-border);
  border-radius: 32px; padding: 40px;
  box-shadow: 0 20px 60px -20px rgba(0,0,0,0.08);
}

.matching-master-header { display: flex; align-items: center; gap: 24px; margin-bottom: 40px; }
.total-score-badge {
  width: 84px; height: 84px; border-radius: 24px;
  background: var(--accent-grad); color: white;
  display: flex; align-items: center; justify-content: center; gap: 2px;
  box-shadow: 0 15px 30px -5px var(--accent-soft);
  transition: 0.4s;
}
.total-score-badge.warning { background: #ef4444; box-shadow: 0 15px 30px -5px rgba(239, 68, 68, 0.3); animation: shake 0.5s; }
@keyframes shake { 0%, 100% { transform: translateX(0); } 25% { transform: translateX(-5px); } 75% { transform: translateX(5px); } }

.score-val { font-size: 32px; font-weight: 950; }
.score-unit { font-size: 16px; font-weight: 800; margin-top: 8px; opacity: 0.8; }

.score-description h5 { font-size: 18px; font-weight: 900; color: var(--r-text-main); margin: 0; }
.score-description p { font-size: 14px; margin: 4px 0 0 0; font-weight: 600; }
.err-text { color: #ef4444; }
.success-text { color: #10b981; }

.btn-reset-lux {
  margin-left: auto; display: flex; align-items: center; gap: 8px;
  padding: 10px 18px; background: var(--r-surface); border: 1.5px solid var(--r-border);
  border-radius: 12px; font-size: 13px; font-weight: 800; color: var(--r-text-main);
  cursor: pointer; transition: 0.2s;
}
.btn-reset-lux:hover { border-color: var(--accent); color: var(--accent); background: var(--accent-soft); }

.weights-grid-lux { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 32px; }
.weight-control-lux { display: flex; flex-direction: column; gap: 16px; }

.wc-icon { 
  width: 20px; 
  height: 20px; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  margin-top: 2px; 
}
.wc-icon :deep(svg) { width: 100%; height: 100%; }
.wc-text { flex: 1; display: flex; flex-direction: column; gap: 8px; }
.wc-text label { font-size: 13px; font-weight: 800; color: var(--r-text-main); letter-spacing: -0.01em; }
.wc-bar-wrap { height: 4px; background: var(--r-border); border-radius: 2px; overflow: hidden; }
.wc-bar-fill { height: 100%; transition: width 0.6s cubic-bezier(0.2, 0.8, 0.2, 1); }
.wc-value { font-size: 18px; font-weight: 950; min-width: 45px; text-align: right; letter-spacing: -0.02em; }

.premium-range-lux {
  -webkit-appearance: none; width: 100%; height: 8px; border-radius: 4px; background: var(--r-border);
}
.premium-range-lux::-webkit-slider-thumb {
  -webkit-appearance: none; width: 24px; height: 24px; border-radius: 50%;
  background: var(--r-surface); border: 5px solid var(--accent-col); cursor: pointer;
  box-shadow: 0 4px 10px rgba(0,0,0,0.1); transition: 0.2s;
}
.premium-range-lux::-webkit-slider-thumb:hover { transform: scale(1.15); box-shadow: 0 5px 15px rgba(0,0,0,0.15); }

/* Workflow Box Lux */
.r-form-row-lux { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 32px; }
.r-input-group { display: flex; flex-direction: column; gap: 12px; }
.r-input-group label { 
  font-size: 11px; font-weight: 950; color: var(--r-text-sub); 
  text-transform: uppercase; letter-spacing: 1px; 
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}

.date-input-lux-wrap { position: relative; }
.date-icon { 
  position: absolute; right: 20px; top: 50%; transform: translateY(-50%); 
  pointer-events: none; color: var(--accent); opacity: 0.8;
}
.expert-date-lux {
  width: 100%; padding: 16px 50px 16px 20px; border-radius: 18px;
  background: var(--r-main-bg); border: 1.5px solid var(--r-border);
  font-size: 16px; font-weight: 800; color: var(--r-text-main); font-family: inherit;
  transition: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  cursor: pointer;
}
.expert-date-lux:focus {
  border-color: var(--accent);
  background: var(--r-surface);
  box-shadow: 0 10px 25px -5px var(--accent-soft);
  outline: none;
}
.expert-date-lux.date-invalid {
  border-color: #ef4444;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.12);
}
.date-err { color: #ef4444; font-size: 12px; font-weight: 600; margin-top: 6px; display: block; }

.date-picker-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  opacity: 0;
  pointer-events: none;
}

.btn-calendar-lux {
  position: absolute; right: 14px; top: 50%; transform: translateY(-50%);
  width: 32px; height: 32px;
  display: flex; align-items: center; justify-content: center;
  background: var(--accent-soft); border: 1px solid var(--accent);
  border-radius: 10px; cursor: pointer;
  color: var(--accent); transition: 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  padding: 0; font-family: inherit;
}
.btn-calendar-lux:hover {
  background: var(--accent); color: white;
  box-shadow: 0 4px 12px rgba(99, 102, 241, 0.3);
}
.btn-calendar-lux:active {
  transform: translateY(-50%) scale(0.95);
}
.btn-calendar-lux svg {
  width: 18px; height: 18px;
}
.status-select-lux-wrap { position: relative; }
.expert-status-lux {
  width: 100%; border-radius: 18px;
}
.status-select-lux-wrap::after { content: '▼'; position: absolute; right: 20px; top: 50%; transform: translateY(-50%); font-size: 10px; color: var(--accent); pointer-events: none; }

.mt-40 { margin-top: 40px; }

/* Auto-Reject Threshold */
.reject-card { padding: 28px; }
.reject-range { --accent-col: #ef4444; }
.reject-range::-webkit-slider-thumb { background: #ef4444 !important; }

.auto-reject-hint {
  display: flex; align-items: center; gap: 8px;
  margin-top: 16px; padding: 10px 14px;
  background: rgba(239, 68, 68, 0.06);
  border: 1px solid rgba(239, 68, 68, 0.2);
  border-radius: 10px; color: #dc2626;
  font-size: 12px; font-weight: 600;
}
.inactive-hint {
  background: rgba(16, 185, 129, 0.06);
  border-color: rgba(16, 185, 129, 0.2);
  color: #10b981;
}

@media (max-width: 900px) {
  .weights-grid-lux, .r-form-row-lux { grid-template-columns: 1fr; }
}
</style>
