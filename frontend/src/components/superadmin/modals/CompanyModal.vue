<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content modal-wide animate-modal">
      <div class="modal-header">
        <div class="header-main">
          <div class="header-icon-box">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6"/></svg>
          </div>
          <div>
            <h2>{{ editing ? 'Modifier Entreprise' : 'Ajouter Entreprise' }}</h2>
            <p class="modal-subtitle">Cockpit de gestion de l'entité professionnelle</p>
          </div>
        </div>
        <button class="modal-close" @click="$emit('close')">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
        </button>
      </div>
      <form @submit.prevent="$emit('save')" class="modal-form">

        <!-- Section: Identity -->
        <div class="form-section">
          <div class="form-section-header">
            <div class="section-icon identity"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg></div>
            <span>Identité & Validation</span>
          </div>
          <div class="form-group">
            <label>Nom de l'Entreprise *</label>
            <div class="input-with-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6"/></svg>
              <input v-model="companyForm.name" type="text" required placeholder="Nom de l'entreprise" />
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label>Industrie / Secteur</label>
              <div class="input-with-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 7h-9m3 3H7m5 5H3m18 5h-4M5 20h1a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1H1a1 1 0 0 1-1-1v-2a1 1 0 0 1 1-1h1m12-15h1a1 1 0 0 1 1 1v2a1 1 0 0 1-1 1h-1a1 1 0 0 1-1-1V6a1 1 0 0 1 1-1z"/></svg>
                <input v-model="companyForm.industry" type="text" placeholder="Secteur d'activité" />
              </div>
            </div>
            <div class="form-group">
              <label>Site Web Officiel</label>
              <div class="input-with-icon">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="2" y1="12" x2="22" y2="12"/><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/></svg>
                <input v-model="companyForm.website" type="url" placeholder="https://" />
              </div>
            </div>
          </div>
        </div>

        <!-- Section: Contact -->
        <div class="form-section">
          <div class="form-section-header">
            <div class="section-icon contact"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg></div>
            <span>Point de Contact</span>
          </div>
          <div class="form-group">
            <label>Adresse Email de Contact</label>
            <div class="input-with-icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
              <input v-model="companyForm.contactEmail" type="email" placeholder="Adresse email officielle" />
            </div>
          </div>
        </div>

        <!-- Section: Status Control -->
        <div class="status-panel">
          <div class="status-info">
            <div class="status-icon" :class="{ active: companyForm.isActive }">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5"/></svg>
            </div>
            <div>
              <div class="status-label">Autorisation Plateforme</div>
              <div class="status-desc">{{ companyForm.isActive ? 'Compte actif et autorisé.' : 'Compte suspendu / En attente.' }}</div>
            </div>
          </div>
          <div class="elite-toggle" :class="{ active: companyForm.isActive }" @click="companyForm.isActive = !companyForm.isActive">
            <div class="toggle-track"></div>
            <div class="toggle-thumb"></div>
          </div>
        </div>

        <div class="modal-actions">
          <button type="button" @click="$emit('close')" class="btn-secondary">Annuler</button>
          <button type="submit" class="btn-primary" :disabled="loading">
            <span v-if="loading">Mise à jour...</span>
            <span v-else>{{ editing ? 'Enregistrer les modifications' : 'Créer l\'entité' }}</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
defineProps({
  editing: Boolean,
  companyForm: Object,
  loading: Boolean
})

defineEmits(['close', 'save'])
</script>

<style scoped>
.modal-overlay { 
  position: fixed; inset: 0; 
  background: rgba(10, 11, 26, 0.85); 
  backdrop-filter: blur(12px); 
  display: flex; align-items: center; justify-content: center; 
  z-index: 2000; padding: 20px; 
}
.modal-content { 
  background: var(--card-bg); 
  border-radius: 28px; 
  width: 100%; max-width: 650px; 
  max-height: 85vh; 
  display: flex; flex-direction: column;
  border: 1px solid rgba(108, 99, 255, 0.2); 
  box-shadow: 0 40px 100px rgba(0,0,0,0.6); 
  overflow: hidden;
}
.modal-wide { max-width: 750px; }

.modal-header { 
  padding: 24px 32px; 
  border-bottom: 1px solid var(--border-thin); 
  display: flex; justify-content: space-between; align-items: center; 
  background: rgba(255,255,255,0.02);
}
.header-main { display: flex; align-items: center; gap: 18px; }
.header-icon-box {
  width: 46px; height: 46px;
  background: linear-gradient(135deg, rgba(108,99,255,0.15), rgba(164,99,255,0.08));
  border: 1px solid rgba(108,99,255,0.2);
  border-radius: 14px;
  display: flex; align-items: center; justify-content: center;
  color: #6C63FF;
}
.header-icon-box svg { width: 22px; }

.modal-header h2 { margin: 0; font-size: 20px; font-weight: 900; color: var(--text-primary); letter-spacing: -0.5px; }
.modal-subtitle { margin: 2px 0 0 0; font-size: 13px; color: var(--text-muted); font-weight: 500; }
.modal-close { 
  width: 36px; height: 36px; border-radius: 10px;
  background: rgba(255,255,255,0.03); border: 1px solid var(--border-thin); 
  color: var(--text-muted); cursor: pointer; 
  display: flex; align-items: center; justify-content: center; transition: 0.2s;
}
.modal-close:hover { background: rgba(239, 68, 68, 0.1); color: #ef4444; border-color: rgba(239, 68, 68, 0.2); }
.modal-close svg { width: 18px; }

.modal-form { 
  padding: 32px; 
  overflow-y: auto;
  scrollbar-width: thin;
  scrollbar-color: rgba(108,99,255,0.2) transparent;
}

.form-section { margin-bottom: 32px; }
.form-section-header { 
  display: flex; align-items: center; gap: 10px;
  margin-bottom: 20px;
}
.form-section-header span { font-size: 13px; font-weight: 800; color: var(--text-primary); text-transform: uppercase; letter-spacing: 0.8px; }
.section-icon { width: 28px; height: 28px; border-radius: 8px; display: flex; align-items: center; justify-content: center; }
.section-icon svg { width: 16px; }

.section-icon.identity { background: rgba(108,99,255,0.1); color: #6C63FF; }
.section-icon.contact  { background: rgba(14,165,233,0.1); color: #0ea5e9; }
.section-icon.location { background: rgba(16,185,129,0.1); color: #10b981; }
.section-icon.branding { background: rgba(245,158,11,0.1); color: #f59e0b; }

.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; margin-bottom: 20px; }
.form-row-triple { display: grid; grid-template-columns: 2fr 1fr 1fr; gap: 16px; margin-bottom: 20px; }
.form-group { display: flex; flex-direction: column; gap: 8px; }
.form-group label { font-size: 12px; font-weight: 700; color: var(--text-muted); padding-left: 4px; }

.input-with-icon { position: relative; display: flex; align-items: center; }
.input-with-icon svg { position: absolute; left: 14px; width: 16px; color: #6C63FF; pointer-events: none; }
.input-with-icon input, .input-with-icon select { padding-left: 42px; width: 100%; }

input, select, textarea { 
  background: var(--bg-base); 
  border: 1px solid var(--border-thin); 
  border-radius: 12px; 
  padding: 12px 14px; 
  color: var(--text-primary); 
  font-family: inherit; font-size: 14px; transition: 0.3s;
  width: 100%;
}
input:focus, select:focus, textarea:focus { border-color: var(--accent-color); outline: none; box-shadow: 0 0 0 4px rgba(108,99,255,0.1); background: var(--bg-hover); }

/* Custom Color Control */
.color-control { 
  display: flex; align-items: center; gap: 12px;
  background: var(--bg-base); border: 1px solid var(--border-thin);
  border-radius: 12px; padding: 6px 12px 6px 6px;
}
.color-preview {
  width: 34px; height: 34px; border-radius: 8px;
  position: relative; overflow: hidden; border: 1px solid rgba(255,255,255,0.1);
}
.color-preview input {
  position: absolute; inset: -10px; cursor: pointer;
  width: 200%; height: 200%;
}
.color-code { 
  border: none !important; background: transparent !important; 
  padding: 0 !important; font-family: monospace; font-weight: 700;
  text-transform: uppercase; color: var(--text-primary) !important;
}

/* Status Panel */
.status-panel {
  background: rgba(108, 99, 255, 0.04);
  border: 1px solid rgba(108, 99, 255, 0.1);
  border-radius: 20px; padding: 20px;
  display: flex; align-items: center; justify-content: space-between;
  margin-top: 10px;
}
.status-info { display: flex; align-items: center; gap: 16px; }
.status-icon {
  width: 40px; height: 40px; border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  background: rgba(239, 68, 68, 0.1); color: #ef4444; transition: 0.3s;
}
.status-icon.active { background: rgba(16, 185, 129, 0.1); color: #10b981; }
.status-icon svg { width: 20px; }
.status-label { font-size: 14px; font-weight: 800; color: var(--text-primary); }
.status-desc { font-size: 12px; color: var(--text-muted); }

/* Elite Toggle */
.elite-toggle {
  width: 50px; height: 26px; position: relative; cursor: pointer;
}
.toggle-track {
  position: absolute; inset: 0; border-radius: 100px;
  background: rgba(255,255,255,0.05); border: 1px solid var(--border-thin);
  transition: 0.3s;
}
.elite-toggle.active .toggle-track { background: #10b981; border-color: #10b981; }
.toggle-thumb {
  position: absolute; top: 4px; left: 4px; width: 18px; height: 18px;
  background: white; border-radius: 50%; transition: 0.3s cubic-bezier(0.18, 0.89, 0.35, 1.15);
  box-shadow: 0 2px 4px rgba(0,0,0,0.2);
}
.elite-toggle.active .toggle-thumb { transform: translateX(24px); }

.modal-actions { 
  display: flex; justify-content: flex-end; gap: 16px; 
  padding: 24px 32px; border-top: 1px solid var(--border-thin); 
  background: rgba(255,255,255,0.02);
}
.btn-secondary { 
  padding: 12px 24px; border-radius: 14px; border: 1px solid var(--border-thin); 
  background: transparent; color: var(--text-primary); font-weight: 700; cursor: pointer; transition: 0.2s; 
}
.btn-secondary:hover { background: rgba(255,255,255,0.05); transform: translateY(-1px); }

.btn-primary { 
  padding: 12px 32px; border-radius: 14px; border: none; 
  background: linear-gradient(135deg, #6C63FF, #A463FF); 
  color: white; font-weight: 800; cursor: pointer; transition: 0.3s; 
  box-shadow: 0 8px 20px rgba(108,99,255,0.3); 
}
.btn-primary:hover:not(:disabled) { transform: translateY(-2px); box-shadow: 0 12px 28px rgba(108,99,255,0.4); }
.btn-primary:disabled { opacity: 0.6; cursor: not-allowed; }

.animate-modal { animation: modalIn 0.5s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes modalIn { 
  from { opacity: 0; transform: translateY(40px) scale(0.96); } 
  to { opacity: 1; transform: translateY(0) scale(1); } 
}
</style>
