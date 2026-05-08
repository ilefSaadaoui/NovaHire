<template>
  <div class="sa-tab-container animate-fade-in">
    <div class="sa-tab-header">
      <div>
        <h2 class="sa-tab-title">Mon Profil</h2>
        <p class="sa-tab-desc">Gérez vos informations personnelles et les paramètres de sécurité de votre compte.</p>
      </div>

    </div>
    <div v-if="adminStore.error" class="sa-error-banner animate-fade-in">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
      <span>{{ adminStore.error }}</span>
      <button @click="adminStore.clearMessages" class="close-error">×</button>
    </div>

    <div class="profile-grid">
      <!-- LEFT: Profile Settings -->
      <div class="profile-main-column">
        <form @submit.prevent="handleSaveProfile" class="profile-sections-wrapper">
          
          <!-- SECTION 1: Identity -->
          <div class="premium-card section-card">
            <div class="section-header">
              <div class="section-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg></div>
              <h4>Identité de l'Administrateur</h4>
            </div>
            
            <div class="profile-form-grid">
              <div class="field-group">
                <label>Prénom</label>
                <div class="input-wrap">
                  <input v-model="profileForm.firstName" class="modern-input" type="text" placeholder="Prénom" />
                </div>
              </div>
              <div class="field-group">
                <label>Nom de famille</label>
                <div class="input-wrap">
                  <input v-model="profileForm.lastName" class="modern-input" type="text" placeholder="Nom" />
                </div>
              </div>
              <div class="field-group full-width">
                <label>Email de Connexion</label>
                <div class="input-wrap">
                  <input v-model="profileForm.email" class="modern-input" type="email" placeholder="admin@example.com" />
                </div>
              </div>
            </div>
          </div>

          <!-- SECTION 2: Security -->
          <div class="premium-card section-card">
            <div class="section-header">
              <div class="section-icon security"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg></div>
              <h4>Sécurité</h4>
            </div>
            
            <div class="field-group full-width">
              <label>Changer le mot de passe</label>
              <div class="input-wrap">
                <input v-model="profileForm.password" class="modern-input" type="password" placeholder="Laisser vide pour ne pas modifier" />
                <svg class="field-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 2l-2 2m-7.61 7.61a5.5 5.5 0 1 1-7.778 7.778 5.5 5.5 0 0 1 7.777-7.777zm0 0L15.5 7.5m0 0l3 3L22 7l-3-3y-1.5z"/></svg>
              </div>
              <p class="field-hint">Utilisez un mot de passe complexe pour protéger l'accès à la plateforme.</p>
            </div>
          </div>

          <button type="submit" class="save-profile-btn" :disabled="adminStore.loading">
            <span v-if="adminStore.loading" class="btn-spinner"></span>
            <span v-else>Enregistrer les modifications</span>
          </button>
        </form>
      </div>

      <!-- RIGHT: Identity Card -->
      <div class="profile-side-column">
        <div class="identity-card glass-panel">
          <div class="card-shimmer"></div>
          
          <div class="identity-header">
            <div class="avatar-executive-wrap">
              <UserAvatar
                :src="profileForm.avatarUrl"
                :name="profileForm.firstName + ' ' + profileForm.lastName"
                :size="100"
                radius="24px"
                :gradient="accentGrad || 'linear-gradient(135deg, #6C63FF, #A463FF)'"
              />
              <button class="avatar-edit-trigger" @click="triggerAvatarFileInput" title="Changer la photo">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/><circle cx="12" cy="13" r="4"/></svg>
              </button>
            </div>
            
            <div class="identity-title">
              <span class="role-badge">PLATFORME MASTER</span>
              <h3>{{ profileForm.firstName }} {{ profileForm.lastName }}</h3>
              <p>{{ profileForm.email }}</p>
            </div>
          </div>

          <div class="identity-stats">
            <div class="stat-item">
              <span class="stat-val">{{ adminStore.summary.totalUsers || 0 }}</span>
              <span class="stat-lab">Utilisateurs</span>
            </div>
            <div class="stat-sep"></div>
            <div class="stat-item">
              <span class="stat-val">{{ adminStore.summary.totalCompanies || 0 }}</span>
              <span class="stat-lab">Entreprises</span>
            </div>
          </div>

          <div class="account-details-list">
            <div class="detail-row">
              <span class="detail-label">ID Système</span>
              <span class="detail-value">#{{ adminStore.currentUser?.id?.substring(0, 8) || '---' }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Membre depuis</span>
              <span class="detail-value">{{ adminStore.currentUser?.createdAt ? new Date(adminStore.currentUser.createdAt).toLocaleDateString() : '---' }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">Dernière IP</span>
              <span class="detail-value">Vérifié</span>
            </div>
          </div>

          <div class="avatar-drop-hint" 
               @click="triggerAvatarFileInput"
               @dragover.prevent="avatarDragOver = true" 
               @dragleave.prevent="avatarDragOver = false" 
               @drop.prevent="handleAvatarDrop"
               :class="{ 'active': avatarDragOver }">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="17 8 12 3 7 8"/><line x1="12" y1="3" x2="12" y2="15"/></svg>
            <span>{{ avatarUploading ? 'Envoi...' : 'Déposer une photo' }}</span>
            <input ref="avatarFileInput" type="file" accept="image/png,image/jpeg,image/webp" hidden @change="handleAvatarFile" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import api from '@/api/axios'
import { useAdminStore } from '@/stores/adminStore'
import { useAuthStore } from '@/stores/authStore'
import UserAvatar from '@/components/shared/UserAvatar.vue'

const adminStore = useAdminStore()
const authStore = useAuthStore()

defineProps({
  accentGrad: String
})

const profileForm = ref({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  phoneNumber: '',
  avatarUrl: '',
  isActive: true,
  emailConfirmed: false,
  mustChangePassword: false
})

const getRoleName = (role) => {
  const roles = { 0: 'Admin Plateforme', 1: 'CompanyAdmin', 2: 'Recruiter' }
  const idx = parseInt(role)
  return roles[idx] ?? (role || 'Inconnu')
}

const formatDateTime = (timestamp) => {
  if (!timestamp) return '-'
  const date = new Date(timestamp)
  return date.toLocaleString('fr-FR')
}

const toggleProfileField = (field) => {
  profileForm.value[field] = !profileForm.value[field]
}

const handleSaveProfile = async () => {
  const payload = { ...profileForm.value }
  const ok = await adminStore.saveProfileUpdates(payload)
  if (ok) {
    profileForm.value.password = ''
    await loadProfile()
    // Sync with authStore for topbar update
    await authStore.fetchUser()
  }
}

const loadProfile = async () => {
  await adminStore.fetchCurrentProfile()
  if (!adminStore.currentUser) return
  const u = adminStore.currentUser
  profileForm.value = {
    firstName: u.firstName || '',
    lastName: u.lastName || '',
    email: u.email || '',
    password: '',
    phoneNumber: u.phoneNumber || '',
    avatarUrl: u.avatarUrl || '',
    isActive: !!u.isActive,
    emailConfirmed: !!u.emailConfirmed,
    mustChangePassword: !!u.mustChangePassword
  }
}

const avatarDragOver = ref(false)
const avatarUploading = ref(false)
const avatarUploadError = ref('')
const avatarFileInput = ref(null)

const triggerAvatarFileInput = () => {
  if (avatarFileInput.value) avatarFileInput.value.click()
}

const handleAvatarDrop = (e) => {
  avatarDragOver.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) processAvatarFile(file)
}

const handleAvatarFile = (e) => {
  const file = e.target?.files?.[0]
  if (file) processAvatarFile(file)
}

const processAvatarFile = async (file) => {
  avatarUploadError.value = ''
  
  // Validate file type
  const allowedTypes = ['image/png', 'image/jpeg', 'image/webp']
  if (!allowedTypes.includes(file.type)) {
    avatarUploadError.value = 'Format non support\u00e9. Utilisez PNG, JPG ou WebP.'
    return
  }
  
  // Validate file size (2MB max)
  if (file.size > 2 * 1024 * 1024) {
    avatarUploadError.value = 'Fichier trop volumineux. Maximum 2 Mo.'
    return
  }

  avatarUploading.value = true
  
  try {
    const formData = new FormData()
    formData.append('file', file)
    
    const res = await api.post('/profile/avatar', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    
    // Update the avatarUrl in the form
    const apiBase = api.defaults.baseURL || 'http://localhost:5000/api'
    const avatarUrl = res.data.avatarUrl?.startsWith('http')
      ? res.data.avatarUrl
      : `${apiBase.replace('/api', '')}${res.data.avatarUrl}`
    
    profileForm.value.avatarUrl = avatarUrl
    
    // Also save the profile to persist the avatar URL
    await handleSaveProfile()
    
  } catch (err) {
    console.error('Avatar upload error:', err)
    avatarUploadError.value = 'Erreur lors de l\'envoi. R\u00e9essayez.'
  } finally {
    avatarUploading.value = false
    // Reset file input
    if (avatarFileInput.value) avatarFileInput.value.value = ''
  }
}

onMounted(() => {
  loadProfile()
})
</script>

<style scoped>
/* Error Banner */
.sa-error-banner {
  background: rgba(239, 68, 68, 0.1); border: 1px solid rgba(239, 68, 68, 0.2);
  color: #ef4444; padding: 12px 20px; border-radius: 12px; margin-bottom: 24px;
  display: flex; align-items: center; gap: 12px; font-size: 14px; font-weight: 600;
}
.sa-error-banner svg { width: 18px; }
.close-error { margin-left: auto; background: none; border: none; color: currentColor; font-size: 20px; cursor: pointer; opacity: 0.6; }
.close-error:hover { opacity: 1; }

/* Executive Layout */
.profile-grid { display: grid; grid-template-columns: 1fr 340px; gap: 32px; align-items: start; }

.profile-sections-wrapper { display: flex; flex-direction: column; gap: 24px; }

.section-card { padding: 28px; position: relative; overflow: hidden; }

.section-header { display: flex; align-items: center; gap: 14px; margin-bottom: 24px; }
.section-icon { 
  width: 38px; height: 38px; border-radius: 12px; background: rgba(108, 99, 255, 0.1); 
  display: flex; align-items: center; justify-content: center; color: var(--accent-color);
}
.section-icon.security { background: rgba(239, 68, 68, 0.1); color: #ef4444; }
.section-icon.status { background: rgba(16, 185, 129, 0.1); color: #10b981; }
.section-icon svg { width: 18px; }
.section-header h4 { font-size: 16px; font-weight: 800; margin: 0; color: var(--text-primary); letter-spacing: -0.3px; }

/* Form Fields */
.profile-form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.field-group { display: flex; flex-direction: column; gap: 8px; }
.field-group.full-width { grid-column: 1 / -1; }
.field-group label { font-size: 11px; font-weight: 800; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.5px; }

.input-wrap { position: relative; }
.modern-input {
  width: 100%; padding: 12px 14px; border-radius: 12px;
  background: var(--bg-base); border: 1px solid var(--border-thin);
  color: var(--text-primary); font-family: inherit; font-size: 14px;
  transition: all 0.3s;
}

/* Fix browser autofill background */
.modern-input:-webkit-autofill,
.modern-input:-webkit-autofill:hover, 
.modern-input:-webkit-autofill:focus {
  -webkit-text-fill-color: var(--text-primary);
  -webkit-box-shadow: 0 0 0px 1000px var(--bg-base) inset;
  transition: background-color 5000s ease-in-out 0s;
}

.modern-input:focus { 
  outline: none; border-color: var(--accent-color); 
  background: var(--bg-base); box-shadow: 0 0 0 4px rgba(108, 99, 255, 0.15);
}
.field-icon { position: absolute; right: 12px; top: 50%; transform: translateY(-50%); width: 16px; color: var(--text-muted); opacity: 0.5; }

/* Premium Toggles */
.toggle-grid { display: grid; grid-template-columns: 1fr; gap: 16px; margin-top: 4px; }
.premium-toggle { 
  display: flex; align-items: center; gap: 14px; 
  padding: 12px; border-radius: 12px; background: rgba(108, 99, 255, 0.03); 
  cursor: pointer; transition: 0.2s;
}
.premium-toggle:hover { background: rgba(108, 99, 255, 0.06); }

.toggle-control { 
  width: 44px; height: 24px; background: var(--border-thin); border-radius: 100px; 
  position: relative; transition: 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275); 
}
.toggle-control.active { background: #10b981; }
.toggle-knob { 
  position: absolute; top: 3px; left: 3px; width: 18px; height: 18px; 
  background: white; border-radius: 50%; transition: 0.4s; 
}
.toggle-control.active .toggle-knob { transform: translateX(20px); }

.toggle-info { display: flex; flex-direction: column; }
.toggle-title { font-size: 13px; font-weight: 700; color: var(--text-primary); }
.toggle-desc { font-size: 11px; color: var(--text-muted); }

/* Identity Card */
.identity-card { 
  padding: 32px; border-radius: 24px; position: relative; overflow: hidden;
  display: flex; flex-direction: column; gap: 28px;
}
.card-shimmer {
  position: absolute; inset: 0; background: linear-gradient(135deg, rgba(108, 99, 255, 0.05) 0%, transparent 100%);
  pointer-events: none;
}

.identity-header { display: flex; flex-direction: column; align-items: center; text-align: center; gap: 16px; }
.avatar-executive-wrap { position: relative; }
.avatar-edit-trigger {
  position: absolute; bottom: 0; right: 0; 
  width: 32px; height: 32px; border-radius: 10px; border: 3px solid white;
  background: var(--accent-color); color: white; display: flex; align-items: center; justify-content: center;
  cursor: pointer; transition: 0.3s;
}
.avatar-edit-trigger:hover { transform: scale(1.1) rotate(5deg); }
.avatar-edit-trigger svg { width: 14px; }

.role-badge { 
  display: inline-block; padding: 4px 10px; border-radius: 100px;
  background: rgba(108, 99, 255, 0.1); color: var(--accent-color);
  font-size: 9px; font-weight: 900; letter-spacing: 1px; margin-bottom: 8px;
}
.identity-title h3 { font-size: 20px; font-weight: 800; margin: 0; color: var(--text-primary); }
.identity-title p { font-size: 12px; color: var(--text-muted); margin: 4px 0 0; }

.identity-stats { display: flex; justify-content: center; align-items: center; gap: 20px; }
.stat-item { display: flex; flex-direction: column; align-items: center; }
.stat-val { font-size: 18px; font-weight: 800; color: var(--text-primary); }
.stat-lab { font-size: 10px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; }
.stat-sep { width: 1px; height: 24px; background: var(--border-thin); }

.account-details-list { display: flex; flex-direction: column; gap: 12px; }
.detail-row { display: flex; justify-content: space-between; align-items: center; padding: 10px 0; border-bottom: 1px dashed var(--border-thin); }
.detail-label { font-size: 12px; color: var(--text-muted); font-weight: 500; }
.detail-value { font-size: 12px; color: var(--text-primary); font-weight: 700; }

.avatar-drop-hint {
  padding: 14px; border: 2px dashed var(--border-thin); border-radius: 16px;
  display: flex; align-items: center; justify-content: center; gap: 10px;
  font-size: 12px; font-weight: 700; color: var(--text-muted);
  cursor: pointer; transition: 0.3s;
}
.avatar-drop-hint:hover, .avatar-drop-hint.active { 
  border-color: var(--accent-color); background: var(--accent-soft); color: var(--accent-color); 
}
.avatar-drop-hint svg { width: 16px; }

.save-profile-btn {
  width: 100%; padding: 16px; border-radius: 14px; border: none;
  background: var(--accent-grad); color: white; font-size: 15px; font-weight: 800;
  cursor: pointer; transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 10px 25px -5px rgba(108, 99, 255, 0.4);
}
.save-profile-btn:hover:not(:disabled) { transform: translateY(-3px); box-shadow: 0 15px 35px -5px rgba(108, 99, 255, 0.6); }
.save-profile-btn:disabled { opacity: 0.7; cursor: not-allowed; }

.btn-spinner { width: 18px; height: 18px; border: 2px solid rgba(255,255,255,0.3); border-top-color: white; border-radius: 50%; animation: spin 0.8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

.animate-fade-in { animation: fadeIn 0.6s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
</style>
