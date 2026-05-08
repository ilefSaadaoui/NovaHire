<template>
  <div class="modal-overlay" @click="$emit('close')">
    <div class="modal-content premium-modal offer-modal-elite" @click.stop>
      <div class="modal-header-elite">
        <div class="header-icon-box">
          <FileText :size="24" />
        </div>
        <div class="header-text">
          <h3>Générer une Offre d'Embauche</h3>
          <p>L'IA va rédiger une proposition officielle personnalisée.</p>
        </div>
        <button class="close-btn-elite" @click="$emit('close')">
          <X :size="20" />
        </button>
      </div>

      <div class="modal-body-elite">
        <div class="offer-form-grid">
          <div class="input-group-elite">
            <label>Salaire Mensuel Brut</label>
            <div class="input-with-icon">
              <input type="number" v-model.number="form.salary" placeholder="ex: 3500" />
              <span class="currency-tag">DT</span>
            </div>
          </div>

          <div class="input-group-elite">
            <label>Date de début prévue</label>
            <div class="input-with-icon">
              <input type="date" v-model="form.startDate" />
              <Calendar :size="18" />
            </div>
          </div>
        </div>

        <div class="input-group-elite full-width">
          <label>Avantages & Bonus</label>
          <textarea 
            v-model="form.benefits" 
            placeholder="Tickets resto, Assurance groupe, Bonus annuel, Télétravail..."
            rows="3"
          ></textarea>
        </div>

        <div v-if="generatedOffer" class="generated-offer-preview anim-reveal-up">
          <div class="preview-header">
            <span>Brouillon généré par l'IA (Modifiable)</span>
            <button class="btn-copy" @click="copyToClipboard">
              <Copy :size="14" /> Copier
            </button>
          </div>
          <div 
            class="preview-content editable-preview" 
            contenteditable="true"
            @input="onContentInput"
            ref="previewRef"
          ></div>
        </div>
      </div>

      <div class="modal-footer-elite">
        <button class="btn-cancel" @click="$emit('close')">Annuler</button>
        <button 
          class="btn-generate-ai" 
          :disabled="loading || !canGenerate" 
          @click="handleGenerate"
        >
          <Sparkles v-if="!loading" :size="18" />
          <Loader2 v-else :size="18" class="spin" />
          {{ generatedOffer ? 'Régénérer' : 'Générer la proposition' }}
        </button>
        <button 
          v-if="generatedOffer" 
          class="btn-confirm-offer" 
          :disabled="loading"
          @click="$emit('confirm', generatedOffer)"
        >
          Valider & Envoyer
          <Send :size="18" />
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick } from 'vue'
import { FileText, X, Calendar, Sparkles, Loader2, Send, Copy } from 'lucide-vue-next'

const props = defineProps({
  loading: Boolean,
  initialData: Object
})

const emit = defineEmits(['close', 'generate', 'confirm'])

const form = ref({
  salary: '',
  startDate: '',
  benefits: '',
  currency: 'DT'
})

const generatedOffer = ref('')
const previewRef = ref(null)

const canGenerate = computed(() => {
  return form.value.salary && form.value.startDate
})

// Update preview content when generatedOffer changes from AI
watch(generatedOffer, async (newVal) => {
  if (!newVal) return
  await nextTick()
  if (previewRef.value && newVal !== previewRef.value.innerHTML) {
    previewRef.value.innerHTML = newVal
  }
})

const onContentInput = (e) => {
  generatedOffer.value = e.target.innerHTML
}

const handleGenerate = () => {
  emit('generate', form.value, (result) => {
    if (result) {
      generatedOffer.value = result
    }
  })
}

const copyToClipboard = () => {
  navigator.clipboard.writeText(generatedOffer.value)
  // Toast would be good here but we use emit or props for that
}
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.7);
  backdrop-filter: blur(8px);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

.offer-modal-elite {
  width: 100%;
  max-width: 700px;
  background: #fff;
  border-radius: 24px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.modal-header-elite {
  padding: 24px 32px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  align-items: center;
  gap: 20px;
  position: relative;
}

.header-icon-box {
  width: 52px;
  height: 52px;
  background: var(--accent-grad);
  color: var(--accent-contrast, white);
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 8px 16px var(--accent-soft);
}

.header-text h3 {
  font-size: 20px;
  font-weight: 800;
  color: #1e293b;
  margin: 0;
}

.header-text p {
  font-size: 14px;
  color: #64748b;
  margin: 4px 0 0;
}

.close-btn-elite {
  position: absolute;
  top: 24px;
  right: 24px;
  background: #f1f5f9;
  border: none;
  width: 32px;
  height: 32px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #64748b;
  cursor: pointer;
  transition: 0.2s;
}

.close-btn-elite:hover {
  background: #e2e8f0;
  color: #1e293b;
}

.modal-body-elite {
  padding: 32px;
  max-height: 70vh;
  overflow-y: auto;
}

.offer-form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
  margin-bottom: 24px;
}

.input-group-elite {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.input-group-elite.full-width {
  grid-column: span 2;
}

.input-group-elite label {
  font-size: 13px;
  font-weight: 700;
  color: #475569;
  letter-spacing: 0.02em;
}

.input-with-icon {
  position: relative;
  display: flex;
  align-items: center;
}

.input-with-icon input {
  width: 100%;
  padding: 12px 16px;
  background: #f8fafc;
  border: 1.5px solid #e2e8f0;
  border-radius: 12px;
  font-size: 15px;
  color: #1e293b;
  transition: 0.2s;
}

.input-with-icon input:focus {
  border-color: var(--accent);
  background: #fff;
  box-shadow: 0 0 0 4px var(--accent-soft);
  outline: none;
}

.currency-tag {
  position: absolute;
  right: 16px;
  font-weight: 800;
  color: #94a3b8;
  font-size: 13px;
}

.input-with-icon svg {
  position: absolute;
  right: 16px;
  color: #94a3b8;
}

textarea {
  width: 100%;
  padding: 12px 16px;
  background: #f8fafc;
  border: 1.5px solid #e2e8f0;
  border-radius: 12px;
  font-size: 15px;
  font-family: inherit;
  resize: none;
  transition: 0.2s;
}

textarea:focus {
  border-color: var(--accent);
  background: #fff;
  outline: none;
}

.generated-offer-preview {
  margin-top: 32px;
  background: #fdfcf7;
  border: 1px solid #fef3c7;
  border-radius: 16px;
  overflow: hidden;
}

.preview-header {
  padding: 10px 16px;
  background: #fef3c7;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  font-weight: 700;
  color: #92400e;
  text-transform: uppercase;
}

.btn-copy {
  background: white;
  border: 1px solid #f59e0b;
  padding: 4px 8px;
  border-radius: 6px;
  color: #b45309;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 4px;
}

.preview-content {
  padding: 24px;
  font-size: 14px;
  line-height: 1.6;
  color: #334155;
  outline: none;
}

.editable-preview:hover {
  background: #fffefb;
  cursor: text;
}

.editable-preview:focus {
  background: white;
  box-shadow: inset 0 0 0 2px rgba(14, 165, 233, 0.1);
}

.modal-footer-elite {
  padding: 24px 32px;
  background: #f8fafc;
  border-top: 1px solid #e2e8f0;
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

.btn-cancel {
  padding: 12px 24px;
  background: transparent;
  border: 1.5px solid #e2e8f0;
  border-radius: 12px;
  font-weight: 700;
  color: #64748b;
  cursor: pointer;
}

.btn-generate-ai {
  padding: 12px 24px;
  background: #111827;
  color: white;
  border: none;
  border-radius: 12px;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  transition: 0.2s;
}

.btn-generate-ai:hover:not(:disabled) {
  background: #000;
  transform: translateY(-2px);
}

.btn-confirm-offer {
  padding: 12px 24px;
  background: var(--accent);
  color: var(--accent-contrast, white);
  border: none;
  border-radius: 12px;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
}

.spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

@keyframes revealUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.anim-reveal-up {
  animation: revealUp 0.5s ease both;
}
</style>
