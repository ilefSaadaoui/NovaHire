<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content modal-wide animate-modal">
      <div class="modal-header">
        <div>
          <h2>Détails Candidat</h2>
          <p class="modal-subtitle">Visualiser et modifier les colonnes du candidat</p>
        </div>
        <button class="modal-close" @click="$emit('close')">×</button>
      </div>

      <form @submit.prevent="$emit('save')" class="modal-form" v-if="!loading">
        <div class="form-section-title">Informations principales</div>
        <div class="form-row">
          <div class="form-group">
            <label>Prénom</label>
            <input v-model="form.firstName" type="text" required />
          </div>
          <div class="form-group">
            <label>Nom</label>
            <input v-model="form.lastName" type="text" required />
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label>Email</label>
            <input v-model="form.email" type="email" required />
          </div>
          <div class="form-group">
            <label>Téléphone</label>
            <input v-model="form.phoneNumber" type="text" />
          </div>
        </div>

        <div class="form-section-title">Liens</div>
        <div class="form-group">
          <label>LinkedIn URL</label>
          <input v-model="form.linkedInUrl" type="url" />
        </div>
        <div class="form-group">
          <label>Portfolio URL</label>
          <input v-model="form.portfolioUrl" type="url" />
        </div>
        <div class="form-group">
          <label>Main CV URL</label>
          <input v-model="form.mainCVUrl" type="url" />
        </div>

        <div class="form-section-title">Métadonnées</div>
        <div class="candidate-db-meta">
          <div><span>ID</span><strong>{{ form.id || '-' }}</strong></div>
          <div><span>Créé le</span><strong>{{ form.createdAt ? formatDateTime(form.createdAt) : '-' }}</strong></div>
          <div><span>Mis à jour</span><strong>{{ form.updatedAt ? formatDateTime(form.updatedAt) : '-' }}</strong></div>
        </div>

        <div class="form-section-title">Candidatures liées</div>
        <div class="candidate-linked-apps" v-if="applications.length">
          <div v-for="linked in applications" :key="linked.id" class="candidate-linked-row">
            <div>{{ linked.jobTitle || 'Offre non définie' }}</div>
            <div class="plan-tag">{{ mapStatusToStage(linked.status).label }}</div>
            <div>{{ linked.appliedAt ? formatDateTime(linked.appliedAt) : '-' }}</div>
          </div>
        </div>
        <div v-else class="candidate-empty">Aucune candidature liée.</div>

        <div class="modal-actions">
          <button type="button" @click="$emit('close')" class="btn-secondary">Fermer</button>
          <button type="submit" class="btn-primary" :disabled="adminLoading">{{ adminLoading ? 'Enregistrement...' : 'Enregistrer' }}</button>
        </div>
      </form>

      <div v-else style="padding: 28px; color: var(--text-muted); text-align: center;">Chargement du candidat...</div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  form: Object,
  applications: Array,
  loading: Boolean,
  adminLoading: Boolean
})

defineEmits(['close', 'save'])

const formatDateTime = (timestamp) => {
  if (!timestamp) return '-'
  const date = new Date(timestamp)
  return date.toLocaleString('fr-FR')
}

const mapStatusToStage = (status) => {
  const map = {
    0: { id: 'new', label: 'Nouveau' },
    1: { id: 'screening', label: 'Préqualification' },
    2: { id: 'screening', label: 'Préqualification' },
    3: { id: 'interview', label: 'Entretien' },
    4: { id: 'rejected', label: 'Rejeté' },
    5: { id: 'offer', label: 'Offre' },
    6: { id: 'interview', label: 'Entretien' }
  }
  return map[status] || { id: 'new', label: 'Nouveau' }
}
</script>

<style scoped>
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.6); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; z-index: 1000; padding: 20px; }
.modal-content { background: var(--card-bg); border-radius: 20px; width: 100%; max-width: 700px; max-height: 90vh; overflow-y: auto; border: 1px solid var(--border-thin); box-shadow: 0 40px 80px rgba(0,0,0,0.5); }
.modal-wide { max-width: 800px; }

.modal-header { padding: 24px 32px; border-bottom: 1px solid var(--border-thin); display: flex; justify-content: space-between; align-items: center; }
.modal-header h2 { margin: 0; font-size: 20px; font-weight: 800; color: var(--text-primary); }
.modal-subtitle { margin: 4px 0 0 0; font-size: 13px; color: var(--text-muted); }
.modal-close { background: none; border: none; font-size: 28px; color: var(--text-muted); cursor: pointer; }

.modal-form { padding: 32px; }
.form-section-title { font-size: 11px; font-weight: 800; color: var(--accent-color); text-transform: uppercase; letter-spacing: 1px; margin: 24px 0 16px 0; border-bottom: 1px solid var(--border-thin); padding-bottom: 8px; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 16px; }
.form-group { display: flex; flex-direction: column; gap: 8px; margin-bottom: 16px; }
.form-group label { font-size: 12px; font-weight: 700; color: var(--text-muted); }

input, select, textarea { background: var(--bg-base); border: 1px solid var(--border-thin); border-radius: 8px; padding: 10px; color: var(--text-primary); font-family: inherit; font-size: 14px; }
input:focus { border-color: var(--accent-color); outline: none; }

.candidate-db-meta { display: grid; grid-template-columns: 1fr; gap: 8px; }
.candidate-db-meta div { display: flex; justify-content: space-between; gap: 12px; font-size: 12px; border-bottom: 1px dashed var(--border-thin); padding-bottom: 8px; }
.candidate-db-meta span { color: var(--text-muted); }
.candidate-db-meta strong { color: var(--text-primary); }

.candidate-linked-apps { display: flex; flex-direction: column; gap: 8px; }
.candidate-linked-row { display: grid; grid-template-columns: 1fr auto auto; gap: 12px; align-items: center; background: var(--bg-base); border: 1px solid var(--border-thin); border-radius: 100px; padding: 12px; font-size: 13px; }
.plan-tag { padding: 4px 10px; background: var(--bg-hover); border-radius: 8px; font-size: 11px; font-weight: 800; color: var(--text-muted); }

.modal-actions { display: flex; justify-content: flex-end; gap: 16px; margin-top: 32px; }
.btn-secondary { padding: 10px 20px; border-radius: 8px; border: 1px solid var(--border-thin); background: transparent; color: var(--text-primary); font-weight: 700; cursor: pointer; }
.btn-primary { padding: 10px 20px; border-radius: 8px; border: none; background: var(--accent-grad); color: white; font-weight: 800; cursor: pointer; }

.animate-modal { animation: modalIn 0.3s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes modalIn { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }

.candidate-empty { padding: 16px; background: var(--bg-base); border-radius: 10px; border: 1px dashed var(--border-thin); text-align: center; color: var(--text-muted); font-size: 13px; }
</style>
