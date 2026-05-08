<template>
  <Transition name="modal-fade">
    <div class="sa-modal-overlay" @click.self="$emit('close')">
      <div class="sa-modal-card anim-scale-up" @click.stop>

        <!-- Header -->
        <div class="sa-modal-header">
          <div class="header-group">
            <div class="header-icon-box">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/>
                <circle cx="8.5" cy="7" r="4"/>
                <line x1="20" y1="8" x2="20" y2="14"/>
                <line x1="17" y1="11" x2="23" y2="11"/>
              </svg>
            </div>
            <div>
              <div class="header-badge">Admin Plateforme</div>
              <h3 class="header-title">{{ editing ? 'Modifier l\'utilisateur' : 'Ajouter un utilisateur' }}</h3>
              <p class="header-sub">{{ editing ? 'Modifiez les informations et les accès' : 'Créez un nouveau compte sur la plateforme' }}</p>
            </div>
          </div>
          <button class="close-btn" @click="$emit('close')">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          </button>
        </div>

        <!-- Form -->
        <form @submit.prevent="$emit('save')" class="sa-modal-form">
          <div class="sa-modal-body">

            <!-- Row 1: Prénom + Nom -->
            <div class="field-row">
              <div class="field-group">
                <label class="field-label">Prénom *</label>
                <div class="field-wrap" :class="{ 'is-active': focusedField === 'firstName' }">
                  <div class="field-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                  </div>
                  <input
                    v-model="userForm.firstName"
                    type="text"
                    required
                    placeholder="ex: Ahmed"
                    class="field-input"
                    @focus="focusedField = 'firstName'"
                    @blur="focusedField = null"
                  />
                </div>
              </div>
              <div class="field-group">
                <label class="field-label">Nom de famille *</label>
                <div class="field-wrap" :class="{ 'is-active': focusedField === 'lastName' }">
                  <div class="field-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                  </div>
                  <input
                    v-model="userForm.lastName"
                    type="text"
                    required
                    placeholder="ex: Mahdi"
                    class="field-input"
                    @focus="focusedField = 'lastName'"
                    @blur="focusedField = null"
                  />
                </div>
              </div>
            </div>

            <!-- Email -->
            <div class="field-group mt-16">
              <label class="field-label">Email *</label>
              <div class="field-wrap" :class="{ 'is-active': focusedField === 'email' }">
                <div class="field-icon">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                </div>
                <input
                  v-model="userForm.email"
                  type="email"
                  required
                  placeholder="ex: ahmed@novahire.com"
                  class="field-input"
                  @focus="focusedField = 'email'"
                  @blur="focusedField = null"
                />
              </div>
            </div>

            <!-- Password (création seulement) -->
            <div class="field-group mt-16" v-if="!editing">
              <label class="field-label">Mot de passe *</label>
              <div class="field-wrap" :class="{ 'is-active': focusedField === 'password' }">
                <div class="field-icon">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                </div>
                <input
                  v-model="userForm.password"
                  :type="showPassword ? 'text' : 'password'"
                  required
                  placeholder="Minimum 8 caractères"
                  class="field-input"
                  @focus="focusedField = 'password'"
                  @blur="focusedField = null"
                />
                <button type="button" class="eye-btn" @click="showPassword = !showPassword">
                  <svg v-if="!showPassword" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                  <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
                </button>
              </div>
            </div>

            <!-- Row 2: Rôle + Entreprise -->
            <div class="field-row mt-16">
              <div class="field-group">
                <label class="field-label">Rôle *</label>
                <div class="field-wrap select-wrap" :class="{ 'is-active': focusedField === 'role' }">
                  <div class="field-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
                  </div>
                  <select
                    v-model="userForm.role"
                    class="field-select"
                    @focus="focusedField = 'role'"
                    @blur="focusedField = null"
                  >
                    <option :value="0">⚡ Admin Plateforme</option>
                    <option :value="1">🏢 CompanyAdmin</option>
                    <option :value="2">👤 Recruiter</option>
                  </select>
                </div>
                <!-- Role description -->
                <div class="role-hint" v-if="userForm.role === 0">Accès total à la plateforme</div>
                <div class="role-hint" v-else-if="userForm.role === 1">Gère son entreprise et son équipe</div>
                <div class="role-hint" v-else-if="userForm.role === 2">Gère les offres et candidatures</div>
              </div>
              <div class="field-group">
                <label class="field-label">Entreprise</label>
                <div class="field-wrap select-wrap" :class="{ 'is-active': focusedField === 'company' }">
                  <div class="field-icon">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6"/></svg>
                  </div>
                  <select
                    v-model="userForm.companyId"
                    class="field-select"
                    @focus="focusedField = 'company'"
                    @blur="focusedField = null"
                  >
                    <option value="">Aucune entreprise</option>
                    <option v-for="c in companies" :key="c.id" :value="c.id">{{ c.name }}</option>
                  </select>
                </div>
              </div>
            </div>

            <!-- Toggle: Statut actif -->
            <div class="toggle-section mt-20">
              <div class="toggle-row" @click="userForm.isActive = !userForm.isActive">
                <div class="toggle-info">
                  <div class="toggle-label-text">Statut du compte</div>
                  <div class="toggle-desc">{{ userForm.isActive ? 'L\'utilisateur peut se connecter' : 'L\'accès est bloqué' }}</div>
                </div>
                <div class="sa-toggle" :class="{ active: userForm.isActive }">
                  <div class="sa-toggle-thumb"></div>
                </div>
              </div>
            </div>

          </div>

          <!-- Footer -->
          <div class="sa-modal-footer">
            <button type="button" class="btn-cancel" @click="$emit('close')">Annuler</button>
            <button type="submit" class="btn-save" :disabled="loading">
              <svg v-if="loading" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" class="spin"><path d="M12 2v4M12 18v4M4.93 4.93l2.83 2.83M16.24 16.24l2.83 2.83M2 12h4M18 12h4M4.93 19.07l2.83-2.83M16.24 7.76l2.83-2.83"/></svg>
              <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 6L9 17l-5-5"/></svg>
              <span>{{ loading ? 'En cours...' : (editing ? 'Enregistrer' : 'Créer l\'utilisateur') }}</span>
            </button>
          </div>
        </form>

      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref } from 'vue'

defineProps({
  editing: Boolean,
  userForm: Object,
  companies: Array,
  loading: Boolean
})

defineEmits(['close', 'save'])

const focusedField = ref(null)
const showPassword = ref(false)
</script>

<style scoped>
/* ── Overlay ── */
.sa-modal-overlay {
  position: fixed;
  inset: 0;
  z-index: 9999;
  background: rgba(15, 23, 42, 0.55);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}

/* ── Modal Card ── */
.sa-modal-card {
  background: var(--card-bg, #ffffff);
  width: 100%;
  max-width: 580px;
  border-radius: 24px;
  border: 1px solid var(--border-thin, rgba(108, 99, 255, 0.12));
  box-shadow:
    0 30px 70px rgba(108, 99, 255, 0.15),
    0 0 0 1px rgba(108, 99, 255, 0.06);
  display: flex;
  flex-direction: column;
  max-height: 90vh;
  overflow: hidden;
}

/* ── Header ── */
.sa-modal-header {
  padding: 24px 28px 20px;
  border-bottom: 1px solid rgba(108, 99, 255, 0.08);
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  background: linear-gradient(135deg, rgba(108,99,255,0.04) 0%, rgba(255,255,255,0) 100%);
  flex-shrink: 0;
}
.header-group { display: flex; align-items: flex-start; gap: 14px; }
.header-icon-box {
  width: 46px; height: 46px; min-width: 46px;
  border-radius: 13px;
  background: linear-gradient(135deg, rgba(108,99,255,0.14), rgba(164,99,255,0.07));
  border: 1px solid rgba(108,99,255,0.18);
  display: flex; align-items: center; justify-content: center;
  color: #6C63FF;
  box-shadow: 0 4px 14px rgba(108,99,255,0.15);
}
.header-icon-box svg { width: 20px; }

.header-badge {
  display: inline-flex;
  padding: 2px 10px; border-radius: 100px;
  background: rgba(108,99,255,0.08);
  border: 1px solid rgba(108,99,255,0.18);
  font-size: 10px; font-weight: 800;
  text-transform: uppercase; letter-spacing: 1.5px;
  color: #6C63FF; margin-bottom: 4px;
}
.header-title {
  font-size: 19px; font-weight: 900;
  background: linear-gradient(135deg, #1e293b 55%, #6C63FF);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent;
  background-clip: text;
  letter-spacing: -0.3px; margin: 0; line-height: 1.2;
}
.header-sub { font-size: 12px; color: var(--text-muted, #94a3b8); margin: 3px 0 0 0; font-weight: 500; }

.close-btn {
  width: 32px; height: 32px; min-width: 32px;
  background: rgba(0,0,0,0.04);
  border: 1px solid rgba(108,99,255,0.1);
  border-radius: 10px;
  color: #94a3b8; cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.25s; margin-top: 2px;
}
.close-btn svg { width: 15px; }
.close-btn:hover { background: rgba(239,68,68,0.08); border-color: rgba(239,68,68,0.25); color: #ef4444; transform: rotate(90deg); }

/* ── Form ── */
.sa-modal-form { display: flex; flex-direction: column; flex: 1; min-height: 0; }
.sa-modal-body { flex: 1; overflow-y: auto; padding: 24px 28px; }
.sa-modal-body::-webkit-scrollbar { width: 4px; }
.sa-modal-body::-webkit-scrollbar-thumb { background: rgba(108,99,255,0.15); border-radius: 10px; }

.field-row { display: flex; gap: 14px; }
.field-group { display: flex; flex-direction: column; gap: 8px; flex: 1; min-width: 0; }
.mt-16 { margin-top: 16px; }
.mt-20 { margin-top: 20px; }

.field-label {
  font-size: 10.5px; font-weight: 800;
  color: var(--text-muted, #64748b);
  text-transform: uppercase; letter-spacing: 0.9px;
}

/* ── Input Wrap ── */
.field-wrap {
  display: flex; align-items: center; gap: 8px;
  background: var(--bg-hover, #f8fafc);
  border: 1.5px solid var(--border-thin, rgba(108,99,255,0.1));
  border-radius: 13px;
  padding: 6px 12px 6px 6px;
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}
.field-wrap.is-active {
  background: var(--card-bg, #ffffff);
  border-color: #6C63FF;
  box-shadow: 0 0 0 3px rgba(108,99,255,0.12), 0 4px 12px -4px rgba(108,99,255,0.18);
  transform: translateY(-1px);
}
.field-wrap.is-active .field-icon {
  background: linear-gradient(135deg, #6C63FF, #A463FF);
  color: white;
  box-shadow: 0 4px 10px -3px rgba(108,99,255,0.5);
}

.field-icon {
  width: 34px; height: 34px; min-width: 34px;
  border-radius: 9px;
  background: rgba(108,99,255,0.07);
  color: #6C63FF;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.25s;
  flex-shrink: 0;
}
.field-icon svg { width: 15px; }

.field-input, .field-select {
  flex: 1; background: transparent; border: none; outline: none;
  color: var(--text-primary, #1e293b); font-weight: 600; font-size: 14px;
  font-family: inherit;
  padding: 7px 0;
  min-width: 0;
}
.field-input::placeholder { color: var(--text-muted, #94a3b8); font-weight: 400; }

.field-select { cursor: pointer; }
.field-select option { background-color: var(--bg-base, white); color: var(--text-primary, #1e293b); }

.select-wrap { padding-right: 8px; }

/* Password eye toggle */
.eye-btn {
  background: none; border: none; cursor: pointer;
  color: #94a3b8; display: flex; padding: 4px;
  flex-shrink: 0; transition: color 0.2s;
}
.eye-btn svg { width: 16px; }
.eye-btn:hover { color: #6C63FF; }

/* Role hint */
.role-hint {
  font-size: 11px; font-weight: 600;
  color: #6C63FF;
  padding: 2px 0 0 2px;
  animation: hintIn 0.2s ease both;
}
@keyframes hintIn { from { opacity: 0; transform: translateY(-4px); } to { opacity: 1; transform: translateY(0); } }

/* ── Toggle ── */
.toggle-section {
  padding: 14px 16px;
  background: rgba(108,99,255,0.035);
  border-radius: 14px;
  border: 1.5px solid rgba(108,99,255,0.08);
}
.toggle-row {
  display: flex; align-items: center; justify-content: space-between;
  cursor: pointer; gap: 16px;
}
.toggle-info { flex: 1; }
.toggle-label-text { font-size: 13px; font-weight: 700; color: var(--text-primary, #1e293b); }
.toggle-desc { font-size: 11px; color: var(--text-muted, #94a3b8); margin-top: 2px; font-weight: 500; }

.sa-toggle {
  width: 40px; height: 22px; min-width: 40px;
  background: #e2e8f0;
  border-radius: 100px; position: relative;
  transition: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
.sa-toggle.active { background: linear-gradient(135deg, #6C63FF, #A463FF); box-shadow: 0 2px 8px rgba(108,99,255,0.3); }
.sa-toggle-thumb {
  position: absolute; top: 3px; left: 3px;
  width: 16px; height: 16px;
  background: white; border-radius: 50%;
  transition: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 1px 4px rgba(0,0,0,0.15);
}
.sa-toggle.active .sa-toggle-thumb { transform: translateX(18px); }

/* ── Footer ── */
.sa-modal-footer {
  padding: 18px 28px 22px;
  border-top: 1px solid var(--border-thin, rgba(108,99,255,0.08));
  display: flex; justify-content: flex-end; gap: 12px;
  background: var(--bg-hover, rgba(248,250,252,0.6));
  flex-shrink: 0;
}
.btn-cancel {
  padding: 10px 20px; border-radius: 11px;
  border: 1.5px solid rgba(108,99,255,0.12);
  background: transparent; color: #64748b;
  font-weight: 700; font-size: 13px;
  cursor: pointer; transition: all 0.2s;
  font-family: inherit;
}
.btn-cancel:hover { background: rgba(108,99,255,0.05); color: #1e293b; border-color: rgba(108,99,255,0.25); transform: translateY(-1px); }

.btn-save {
  display: flex; align-items: center; gap: 8px;
  padding: 10px 22px; border-radius: 11px; border: none;
  background: linear-gradient(135deg, #6C63FF, #A463FF);
  color: white; font-weight: 800; font-size: 13px;
  cursor: pointer; font-family: inherit;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 6px 20px -4px rgba(108,99,255,0.45);
  position: relative; overflow: hidden;
}
.btn-save svg { width: 16px; flex-shrink: 0; }
.btn-save::after {
  content: ''; position: absolute; top: 0; left: -100%;
  width: 100%; height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent);
  transition: 0.5s;
}
.btn-save:hover::after { left: 100%; }
.btn-save:hover { transform: translateY(-2px); box-shadow: 0 10px 28px -6px rgba(108,99,255,0.55); }
.btn-save:active { transform: translateY(1px); }
.btn-save:disabled { opacity: 0.6; cursor: not-allowed; transform: none !important; }

/* Spinner */
.spin { animation: rotate 1.2s linear infinite; }
@keyframes rotate { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

/* ── Animations globales ── */
.anim-scale-up { animation: scaleUp 0.35s cubic-bezier(0.175, 0.885, 0.32, 1.275) both; }
@keyframes scaleUp { from { opacity: 0; transform: scale(0.93); } to { opacity: 1; transform: scale(1); } }

.modal-fade-enter-active, .modal-fade-leave-active { transition: all 0.3s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
