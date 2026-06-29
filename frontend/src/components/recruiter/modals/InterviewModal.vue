<template>
  <Teleport to="body">
    <div class="interview-modal-overlay" @click.self="$emit('close')">
      <div class="modal-content premium-modal" style="max-width: 600px; width: 90%; max-height: 90vh;">
        <div class="modal-header">
          <div class="header-title">
            <div class="icon-box-themed" style="width: 44px; height: 44px; border-radius: 14px;">
              <CalendarIcon :size="22" stroke-width="2.5"/>
            </div>
            <div>
              <h3>Planifier un Entretien</h3>
              <p>Définissez les détails et prévenez le candidat</p>
            </div>
          </div>
          <button class="close-btn" @click="$emit('close')">
            <X :size="24" stroke-width="2.5" />
          </button>
        </div>

        <div class="modal-body">
          
          <!-- Configuration Card -->
          <div class="r-card setup-card mb-24">
            <div class="grid-2-col">
              <div class="form-group">
                <label><CalendarIcon :size="14" /> Date prévue</label>
                <div class="input-wrap">
                  <input
                    type="date"
                    v-model="localForm.date"
                    @input="updateField('date', localForm.date)"
                  />
                </div>
              </div>
              <div class="form-group">
                <label><Clock :size="14" /> Heure prévue</label>
                <div class="input-wrap">
                  <input
                    type="time"
                    v-model="localForm.time"
                    @input="updateField('time', localForm.time)"
                  />
                </div>
              </div>
            </div>
            <div class="form-group mt-16">
              <label><Video :size="14" /> Format de l'entretien</label>
              <div class="input-wrap custom-select-wrapper">
                <select v-model="localForm.type" @change="handleTypeChange($event.target.value)" style="width:100%; padding:12px; border:1px solid var(--r-border); border-radius:8px; background: var(--r-main-bg); color: var(--r-text-main); outline:none;">
                  <option value="visio">Visio / Télétravail</option>
                  <option value="phone">Appel Téléphonique</option>
                  <option value="onsite">En Personne (Bureau)</option>
                </select>
              </div>
            </div>
          </div>

          <!-- Email Preview Window -->
          <div class="email-preview-window">
            <div class="ep-header">
              <div class="ep-dots"><span class="dot d1"></span><span class="dot d2"></span><span class="dot d3"></span></div>
              <span class="ep-title"><Mail :size="14" /> E-mail envoyé au candidat</span>
            </div>
            <div class="ep-body">
              <div class="form-group ep-subject" style="flex-direction: row;">
                <label style="white-space: nowrap;">Objet :</label>
                <input type="text" :value="form?.subject" @input="updateField('subject', $event.target.value)" class="invisible-input" />
              </div>
              <div class="form-group ep-message">
                <textarea :value="form?.message" @input="updateField('message', $event.target.value)" rows="5" class="invisible-input"></textarea>
              </div>
            </div>
          </div>

        </div>

        <div class="modal-footer">
          <button class="action-btn action-cancel" @click="$emit('close')">Annuler</button>
          <button class="action-btn action-main" @click="$emit('save')" :disabled="isSubmitDisabled">
            <Send :size="16" stroke-width="2.5" v-if="!loading" />
            <span style="text-transform: uppercase; letter-spacing: 1px;">{{ loading ? 'ENVOI...' : 'ENVOYER L\'INVITATION' }}</span>
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
import { reactive, watch, computed } from 'vue'
import { Calendar as CalendarIcon, Clock, Video, Mail, Send, X } from 'lucide-vue-next'

const props = defineProps({
  form: {
    type: Object,
    required: true
  },
  loading: Boolean
})

const emit = defineEmits(['close', 'save', 'generate-template', 'update:form'])

const formatDateForInput = (value) => {
  if (!value) return ''
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}$/.test(value)) {
    return value
  }
  let date = value
  if (!(date instanceof Date)) {
    date = parseDateInput(value)
  }
  if (!date) return ''
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

const formatTimeForInput = (value) => {
  if (!value) return ''
  const match = String(value).match(/^(\d{1,2}):(\d{1,2})$/)
  if (!match) return ''
  const hours = String(Number(match[1])).padStart(2, '0')
  const minutes = String(Number(match[2])).padStart(2, '0')
  return `${hours}:${minutes}`
}

const localForm = reactive({
  date: formatDateForInput(props.form?.date),
  time: formatTimeForInput(props.form?.time),
  type: props.form?.type || 'visio',
  subject: props.form?.subject || '',
  message: props.form?.message || ''
})

watch(
  () => props.form?.subject,
  (next) => {
    if (next !== undefined && next !== localForm.subject) {
      localForm.subject = next || ''
    }
  }
)

watch(
  () => props.form?.message,
  (next) => {
    if (next !== undefined && next !== localForm.message) {
      localForm.message = next || ''
    }
  }
)

const updateField = (field, value) => {
  localForm[field] = value
  emit('update:form', { ...localForm })
  if (field === 'date' || field === 'time') {
    emit('generate-template')
  }
}

function parseDateInput(value) {
  if (!value) return null
  const normalized = value.trim()
  const frMatch = normalized.match(/^(\d{1,2})[\/\-.](\d{1,2})[\/\-.](\d{2,4})$/)
  if (frMatch) {
    const day = Number(frMatch[1])
    const month = Number(frMatch[2])
    let year = Number(frMatch[3])
    if (year < 100) year += 2000
    const date = new Date(year, month - 1, day)
    if (date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day) {
      return date
    }
  }
  const isoMatch = normalized.match(/^(\d{4})-(\d{2})-(\d{2})$/)
  if (isoMatch) {
    const year = Number(isoMatch[1])
    const month = Number(isoMatch[2])
    const day = Number(isoMatch[3])
    const date = new Date(year, month - 1, day)
    if (date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day) {
      return date
    }
  }
  const parsed = new Date(normalized)
  return isNaN(parsed.getTime()) ? null : parsed
}

const formatLocalizedDate = (value) => {
  const date = typeof value === 'string' ? parseDateInput(value) : value
  if (!date) return value
  return date.toLocaleDateString('fr-FR', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
}

const formatPayloadDate = (value) => {
  const date = parseDateInput(value)
  return date ? date.toISOString().split('T')[0] : value
}

const handleTypeChange = (val) => {
  updateField('type', val)
  emit('generate-template')
}

const isSubmitDisabled = computed(() => !localForm.date || !localForm.time || props.loading)
</script>

<style>
/* Unscoped so Teleport-moved element always receives these styles,
   and !important beats any stacking/pointer-events interference. */
.interview-modal-overlay {
  position: fixed !important;
  inset: 0 !important;
  min-height: 100vh !important;
  background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 99999 !important;
  padding: 20px;
  overflow-y: auto;
  pointer-events: auto !important;
}
.interview-modal-overlay * { pointer-events: auto; }
</style>

<style scoped>
.modal-content, .modal-content * { pointer-events: auto !important; }
.modal-content { background: var(--card-bg, #ffffff); border-radius: 24px; border: 1px solid rgba(255,255,255,0.8); box-shadow: 0 40px 100px -20px rgba(0,0,0,0.15), inset 0 2px 0 rgba(255,255,255,0.7); overflow: hidden; display: flex; flex-direction: column; position: relative; z-index: 1; }

.modal-header { padding: 24px 32px 16px; display: flex; justify-content: space-between; align-items: flex-start; }
.header-title { display: flex; gap: 16px; align-items: center; }
.header-title h3 { margin: 0 0 4px 0; font-size: 20px; font-weight: 850; color: var(--r-text-main); letter-spacing: -0.5px; }
.header-title p { margin: 0; font-size: 13px; color: var(--r-text-sub); }
.close-btn { background: var(--bg-hover); border: none; color: var(--r-text-sub); width: 36px; height: 36px; border-radius: 12px; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: 0.2s; }
.close-btn:hover { background: rgba(239, 68, 68, 0.1); color: #ef4444; }

.modal-body { padding: 0 32px 24px; max-height: calc(90vh - 160px); overflow-y: auto; }

.grid-2-col { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
.mb-24 { margin-bottom: 20px; }
.mt-16 { margin-top: 12px; }

/* Setup Card Style */
.setup-card {
  background: rgba(14, 165, 233, 0.03) !important;
  border: 1px solid rgba(14, 165, 233, 0.3) !important;
  border-radius: 16px !important;
  padding: 24px !important;
  box-shadow: inset 0 2px 10px rgba(255, 255, 255, 0.5) !important;
}

.form-group { display: flex; flex-direction: column; gap: 8px; }
.form-group label { font-weight: 800; font-size: 11px; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 1px; display: flex; align-items: center; gap: 6px; }

input, textarea {
  width: 100%; padding: 12px 16px; border: 1px solid var(--r-border); border-radius: 12px; background: transparent; color: var(--r-text-main); font-family: inherit; font-size: 14px; font-weight: 500; outline: none; transition: all 0.2s;
  box-shadow: inset 0 2px 4px rgba(0,0,0,0.01);
}
input:focus, textarea:focus { border-color: var(--accent); box-shadow: 0 0 0 4px var(--accent-soft); background: white; }

/* Dark mode: keep inputs readable (don't force white bg on focus) */
.dark-mode input, .dark-mode textarea {
  background: rgba(255, 255, 255, 0.04);
  border-color: rgba(255, 255, 255, 0.12);
  color: var(--r-text-main);
}
.dark-mode input::placeholder, .dark-mode textarea::placeholder {
  color: rgba(255, 255, 255, 0.35);
}
.dark-mode input:focus, .dark-mode textarea:focus {
  background: rgba(255, 255, 255, 0.08);
  border-color: var(--accent);
}
.dark-mode input[type="date"]::-webkit-calendar-picker-indicator,
.dark-mode input[type="time"]::-webkit-calendar-picker-indicator {
  filter: invert(1) opacity(0.6);
}
.dark-mode .custom-select-wrapper :deep(.premium-select-display:hover),
.dark-mode .custom-select-wrapper :deep(.premium-select-wrapper.is-open .premium-select-display) {
  background: rgba(255, 255, 255, 0.06);
}

/* PremiumSelect Override */
.custom-select-wrapper :deep(.premium-select-display) {
  padding: 10px 16px;
  border-radius: 12px;
  font-size: 14px;
  background: transparent;
  border: 1px solid var(--r-border);
  box-shadow: inset 0 2px 4px rgba(0,0,0,0.01);
}
.custom-select-wrapper :deep(.premium-select-display:hover),
.custom-select-wrapper :deep(.premium-select-wrapper.is-open .premium-select-display) {
  background: white; border-color: var(--accent); box-shadow: 0 0 0 4px var(--accent-soft);
}

/* Email Preview Setup */
.email-preview-window {
  border-radius: 14px; border: 1px solid var(--r-border); overflow: hidden; background: #fafafa;
  box-shadow: 0 10px 30px rgba(0,0,0,0.02), inset 0 2px 0 rgba(255,255,255,1);
}
.ep-header {
  padding: 10px 16px; background: #f0f0f0; border-bottom: 1px solid var(--r-border); display: flex; align-items: center; gap: 12px;
}
.ep-dots { display: flex; gap: 6px; }
.dot { width: 10px; height: 10px; border-radius: 50%; }
.d1 { background: #ff5f56; } .d2 { background: #ffbd2e; } .d3 { background: #27c93f; }
.ep-title { font-size: 11px; font-weight: 800; color: #666; text-transform: uppercase; display: flex; align-items: center; gap: 6px; letter-spacing: 0.5px; }

.ep-body { display: flex; flex-direction: column; }
.ep-subject { display: flex; flex-direction: row; align-items: center; padding: 8px 16px; border-bottom: 1px solid var(--r-border); background: white; gap: 12px; }
.ep-subject label { margin: 0; color: #888; white-space: nowrap; }
.ep-message { padding: 0; background: white; }

.invisible-input {
  border: none !important; border-radius: 0 !important; box-shadow: none !important; padding: 0 !important; background: transparent !important;
}
.invisible-input:focus { box-shadow: none !important; }
textarea.invisible-input { padding: 16px !important; resize: vertical; min-height: 120px; line-height: 1.5; color: #333; }
.ep-subject .invisible-input { font-weight: 700; color: #111; }

.modal-footer { padding: 20px 32px; border-top: 1px solid var(--r-border); display: flex; justify-content: flex-end; gap: 12px; background: #f8fafc; border-radius: 0 0 24px 24px; flex-shrink: 0; }

/* Elite Action Buttons */
.action-btn { display: flex; align-items: center; gap: 8px; padding: 12px 24px; border-radius: 14px; font-size: 14px; font-weight: 800; border: none; cursor: pointer; transition: 0.3s; }
.action-cancel { background: white; color: var(--r-text-main); border: 1px solid var(--r-border); box-shadow: 0 2px 4px rgba(0,0,0,0.02); }
.action-cancel:hover { background: var(--bg-hover); }

.action-main {
  background: var(--accent-grad);
  color: var(--accent-contrast, white); 
  box-shadow: 0 10px 25px -5px var(--accent-soft), 0 3px 0 var(--accent-dark), inset 0 2px 0 rgba(255, 255, 255, 0.2); 
  position: relative; 
  overflow: hidden;
  border-radius: 100px;
  padding: 14px 32px;
  font-weight: 900;
  letter-spacing: 0.5px;
}
.action-main:disabled { opacity: 0.5; cursor: not-allowed; box-shadow: 0 3px 0 var(--accent-dark); filter: none; }
.action-main:not(:disabled):hover { transform: translateY(-2px); box-shadow: 0 15px 30px -5px var(--accent-soft), 0 3px 0 var(--accent-dark), inset 0 2px 0 rgba(255, 255, 255, 0.3); filter: brightness(1.05); }
.action-main:not(:disabled):active { transform: translateY(1px); box-shadow: 0 0 0 transparent, 0 1px 0 var(--accent-dark); }

.premium-modal { animation: modalIn 0.4s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes modalIn { from { opacity: 0; transform: translateY(30px) scale(0.98); } to { opacity: 1; transform: translateY(0) scale(1); } }

/* Scrollbar tweaks for textarea */
textarea::-webkit-scrollbar { width: 6px; }
textarea::-webkit-scrollbar-thumb { background: #ddd; border-radius: 10px; }

/* Dark mode overrides */
.dark-mode .modal-content {
  background: var(--card-bg, #1e293b);
  border: 1px solid rgba(255, 255, 255, 0.08);
  box-shadow: 0 40px 100px -20px rgba(0, 0, 0, 0.6), inset 0 2px 0 rgba(255, 255, 255, 0.05);
}
.dark-mode .setup-card {
  background: var(--accent-soft) !important;
  border-color: var(--accent-glow) !important;
  box-shadow: inset 0 2px 10px rgba(255, 255, 255, 0.03) !important;
}
.dark-mode .email-preview-window {
  background: rgba(255, 255, 255, 0.02);
  border-color: rgba(255, 255, 255, 0.1);
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2), inset 0 2px 0 rgba(255, 255, 255, 0.03);
}
.dark-mode .ep-header {
  background: rgba(255, 255, 255, 0.04);
  border-bottom-color: rgba(255, 255, 255, 0.08);
}
.dark-mode .ep-title {
  color: rgba(255, 255, 255, 0.6);
}
.dark-mode .ep-subject {
  background: rgba(255, 255, 255, 0.02);
  border-bottom-color: rgba(255, 255, 255, 0.08);
}
.dark-mode .ep-subject label {
  color: rgba(255, 255, 255, 0.5);
}
.dark-mode .ep-message {
  background: rgba(255, 255, 255, 0.02);
}
.dark-mode .ep-subject .invisible-input {
  color: var(--r-text-main);
}
.dark-mode textarea.invisible-input {
  color: var(--r-text-main);
}
.dark-mode .invisible-input {
  background: transparent !important;
}
.dark-mode .modal-footer {
  background: rgba(255, 255, 255, 0.02);
  border-top-color: rgba(255, 255, 255, 0.08);
}
.dark-mode .action-cancel {
  background: rgba(255, 255, 255, 0.04);
  color: var(--r-text-main);
  border-color: rgba(255, 255, 255, 0.12);
  box-shadow: none;
}
.dark-mode .action-cancel:hover {
  background: rgba(255, 255, 255, 0.08);
}
.dark-mode textarea::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.15);
}
.dark-mode .custom-select-wrapper :deep(.premium-select-display:hover),
.dark-mode .custom-select-wrapper :deep(.premium-select-wrapper.is-open .premium-select-display) {
  background: rgba(255, 255, 255, 0.06);
}
</style>
