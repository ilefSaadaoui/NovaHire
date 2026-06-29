<template>
  <div class="admin-glass-card anim-reveal-up">
    <div class="section-header-premium">
      <div>
        <h4 class="celestial-title imperial-aura">{{ $t('settings.branding.title') }}</h4>
        <p class="section-subtitle">{{ $t('settings.branding.subtitle') }}</p>
      </div>
      <button class="btn-save-branding" @click="$emit('save')" :disabled="isSaving">
        <Loader2 v-if="isSaving" class="spin" :size="16" />
        <Save v-else :size="16" stroke-width="2.5" />
        <span>{{ isSaving ? 'Synchronisation...' : 'Appliquer' }}</span>
      </button>
    </div>

    <div class="branding-form-centered">
      <!-- Identité -->
      <div class="form-section">
        <div class="form-section-header">
          <Building2 :size="16" class="form-section-icon" />
          <span>Identité</span>
        </div>

        <!-- Nom de l'entreprise -->
        <div class="input-group">
          <label class="input-label">Nom de l'entreprise</label>
          <div class="input-wrap">
            <input type="text" :value="form.companyName" @input="$emit('update:form', { ...form, companyName: $event.target.value })" class="input-field" placeholder="Votre entreprise">
          </div>
        </div>

        <!-- Description -->
        <div class="input-group">
          <label class="input-label">Description</label>
          <div class="input-wrap">
            <textarea :value="form.description" @input="$emit('update:form', { ...form, description: $event.target.value })" class="input-field textarea" rows="3" placeholder="Décrivez votre entreprise en quelques mots..."></textarea>
          </div>
        </div>

        <!-- Logo Upload -->
        <div class="input-group">
          <label class="input-label">{{ $t('settings.branding.logo') }}</label>

          <!-- État : logo existant -->
          <div v-if="form.logoUrl && !logoPreview" class="logo-preview-wrap">
            <img :src="form.logoUrl" class="logo-preview-img" alt="Logo actuel" />
            <div class="logo-preview-meta">
              <span class="logo-preview-name">Logo actuel</span>
              <button class="logo-remove-btn" @click="removeLogo" :disabled="logoUploading" title="Supprimer le logo">
                <X :size="14" />
              </button>
            </div>
          </div>

          <!-- État : nouveau logo uploadé (aperçu local) -->
          <div v-else-if="logoPreview" class="logo-preview-wrap">
            <img :src="logoPreview" class="logo-preview-img" alt="Nouveau logo" />
            <div class="logo-preview-meta">
              <span class="logo-preview-name logo-uploading-label">
                <Loader2 v-if="logoUploading" class="spin-sm" :size="12" />
                {{ logoUploading ? 'Envoi en cours...' : 'Nouveau logo' }}
              </span>
              <button v-if="!logoUploading" class="logo-remove-btn" @click="cancelLogoPreview" title="Annuler">
                <X :size="14" />
              </button>
            </div>
          </div>

          <!-- Zone de drop / sélection -->
          <div
            v-else
            class="logo-drop-zone"
            :class="{ 'logo-drop-active': logoDragOver, 'logo-drop-error': logoUploadError }"
            @click="triggerLogoInput"
            @dragover.prevent="logoDragOver = true"
            @dragleave.prevent="logoDragOver = false"
            @drop.prevent="handleLogoDrop"
          >
            <div class="logo-drop-icon-wrap">
              <Upload :size="22" class="logo-drop-icon" />
            </div>
            <p class="logo-drop-text">
              <span class="logo-drop-link">Cliquez pour importer</span> ou glissez votre logo ici
            </p>
            <p class="logo-drop-hint">PNG, JPG, WebP — max 2 Mo</p>
            <input
              ref="logoFileInput"
              type="file"
              accept="image/png,image/jpeg,image/webp"
              hidden
              @change="handleLogoFile"
            />
          </div>

          <!-- Message d'erreur -->
          <p v-if="logoUploadError" class="logo-error-msg">
            <AlertCircle :size="12" /> {{ logoUploadError }}
          </p>
        </div>
      </div>

      <!-- Informations -->
      <div class="form-section">
        <div class="form-section-header">
          <Globe :size="16" class="form-section-icon" />
          <span>Informations</span>
        </div>
        <div class="form-row">
          <div class="input-group flex-1">
            <label class="input-label">Secteur d'activité</label>
            <div class="input-wrap">
              <input type="text" :value="form.industry" @input="$emit('update:form', { ...form, industry: $event.target.value })" class="input-field" placeholder="Secteur">
            </div>
          </div>
          <div class="input-group flex-1">
            <label class="input-label">Site Web</label>
            <div class="input-wrap">
              <Link :size="15" class="input-icon" />
              <input type="text" :value="form.website" @input="$emit('update:form', { ...form, website: $event.target.value })" class="input-field" placeholder="URL">
            </div>
          </div>
        </div>
        <div class="form-row">
          <div class="input-group flex-1">
            <label class="input-label">Pays</label>
            <div class="input-wrap">
              <MapPin :size="15" class="input-icon" />
              <input type="text" :value="form.country" @input="$emit('update:form', { ...form, country: $event.target.value })" class="input-field" placeholder="Pays">
            </div>
          </div>
          <div class="input-group flex-1">
            <label class="input-label">Téléphone</label>
            <div class="input-wrap">
              <Phone :size="15" class="input-icon" />
              <input type="text" :value="form.contactPhone" @input="$emit('update:form', { ...form, contactPhone: $event.target.value })" class="input-field" placeholder="Numéro">
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { Save, Loader2, Building2, Globe, Link, MapPin, Phone, X, Upload, AlertCircle } from 'lucide-vue-next'
import api from '@/api/axios'

const props = defineProps({
  form: Object,
  isSaving: Boolean
})

const emit = defineEmits(['save', 'update:form'])

// ── Logo upload state ──────────────────────────────────────────────
const logoFileInput = ref(null)
const logoPreview   = ref('')
const logoUploading = ref(false)
const logoUploadError = ref('')
const logoDragOver  = ref(false)

const triggerLogoInput = () => {
  if (logoFileInput.value) logoFileInput.value.click()
}

const handleLogoDrop = (e) => {
  logoDragOver.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) processLogoFile(file)
}

const handleLogoFile = (e) => {
  const file = e.target?.files?.[0]
  if (file) processLogoFile(file)
}

const processLogoFile = async (file) => {
  logoUploadError.value = ''

  const allowed = ['image/png', 'image/jpeg', 'image/webp']
  if (!allowed.includes(file.type)) {
    logoUploadError.value = 'Format non supporté. Utilisez PNG, JPG ou WebP.'
    return
  }
  if (file.size > 2 * 1024 * 1024) {
    logoUploadError.value = 'Fichier trop volumineux. Maximum 2 Mo.'
    return
  }

  // Aperçu local instantané
  logoPreview.value = URL.createObjectURL(file)
  logoUploading.value = true

  try {
    const formData = new FormData()
    formData.append('file', file)

    const res = await api.post('/companyadmin/branding/logo', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })

    // Mettre à jour le form avec l'URL Cloudinary
    emit('update:form', { ...props.form, logoUrl: res.data.logoUrl })
    logoPreview.value = ''
  } catch (err) {
    logoUploadError.value = err?.response?.data?.message || 'Erreur lors de l\'envoi. Réessayez.'
    logoPreview.value = ''
  } finally {
    logoUploading.value = false
    if (logoFileInput.value) logoFileInput.value.value = ''
  }
}

const removeLogo = () => {
  emit('update:form', { ...props.form, logoUrl: '' })
  logoPreview.value = ''
  logoUploadError.value = ''
}

const cancelLogoPreview = () => {
  logoPreview.value = ''
  logoUploadError.value = ''
  if (logoFileInput.value) logoFileInput.value.value = ''
}
</script>

<style scoped>
.admin-glass-card {
  background: var(--card-bg);
  border: 1px solid var(--r-border);
  border-radius: 32px;
  padding: 32px;
}
.section-header-premium {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 36px;
}
.celestial-title { font-size: 20px; font-weight: 900; color: var(--r-text-main); letter-spacing: -0.5px; }
.section-subtitle { font-size: 13px; color: var(--r-text-sub); margin-top: 4px; font-weight: 600; }

/* ─── Save Button ─── */
.btn-save-branding {
  display: flex; align-items: center; gap: 10px;
  padding: 12px 24px; border-radius: 14px; border: none;
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  color: white; font-weight: 800; font-size: 13px;
  cursor: pointer; transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 6px 20px -4px rgba(14, 165, 233, 0.4), 0 2px 0 #0891b2;
  text-transform: uppercase; letter-spacing: 0.5px;
  position: relative; overflow: hidden;
}
.btn-save-branding::after {
  content: ''; position: absolute; top: 0; left: -100%;
  width: 100%; height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent);
  transition: 0.5s;
}
.btn-save-branding:hover::after { left: 100%; }
.btn-save-branding:hover { transform: translateY(-2px); box-shadow: 0 10px 28px -6px rgba(14, 165, 233, 0.5), 0 1px 0 #0891b2; }
.btn-save-branding:active { transform: translateY(1px); box-shadow: 0 2px 6px rgba(0,0,0,0.15); }
.btn-save-branding:disabled { opacity: 0.7; cursor: not-allowed; transform: none !important; box-shadow: none !important; }

/* ─── Centred layout ─── */
.branding-form-centered {
  display: flex;
  flex-direction: column;
  gap: 28px;
  max-width: 780px;
  margin: 0 auto;
}

/* ─── Sections ─── */
.form-section {
  background: rgba(0,0,0,0.015);
  border: 1px solid var(--r-border);
  border-radius: 20px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.form-section-header {
  display: flex; align-items: center; gap: 8px;
  font-size: 13px; font-weight: 900; color: var(--accent);
  text-transform: uppercase; letter-spacing: 0.8px;
  padding-bottom: 12px; border-bottom: 1px solid var(--r-border);
}
.form-section-icon { opacity: 0.85; }

.form-row { display: flex; gap: 16px; }
.flex-1 { flex: 1; }

.input-group { display: flex; flex-direction: column; gap: 6px; }
.input-label {
  font-size: 11px; font-weight: 800; color: var(--r-text-sub);
  text-transform: uppercase; letter-spacing: 0.8px;
}

.input-wrap {
  display: flex; align-items: center; gap: 10px;
  background: var(--card-bg); padding: 12px 16px; border-radius: 12px;
  border: 1px solid var(--r-border); transition: all 0.25s;
}
.input-wrap:focus-within {
  border-color: var(--accent); box-shadow: 0 0 0 3px rgba(var(--accent-rgb), 0.08);
}
.input-field {
  flex: 1; background: transparent; border: none; outline: none;
  color: var(--r-text-main); font-weight: 700; font-size: 13px;
  font-family: inherit;
}
.textarea { min-height: 70px; resize: vertical; }
.input-icon { color: var(--r-text-sub); opacity: 0.6; flex-shrink: 0; }

/* ─── Color Picker ─── */
.color-picker-wrap {
  display: flex; align-items: center; gap: 12px;
  background: var(--card-bg); padding: 10px 16px; border-radius: 12px;
  border: 1px solid var(--r-border); transition: all 0.25s;
}
.color-picker-wrap:focus-within {
  border-color: var(--accent); box-shadow: 0 0 0 3px rgba(var(--accent-rgb), 0.08);
}
.color-swatch {
  width: 36px; height: 36px; border-radius: 10px; cursor: pointer;
  border: 2px solid rgba(0,0,0,0.08); position: relative;
  box-shadow: 0 4px 10px rgba(0,0,0,0.1); transition: transform 0.2s;
}
.color-swatch:hover { transform: scale(1.1); }
.color-input-hidden {
  position: absolute; top: 0; left: 0; width: 100%; height: 100%;
  opacity: 0; cursor: pointer;
}
.color-hex { font-family: 'SF Mono', 'Fira Code', monospace; font-size: 13px; color: var(--r-text-main); font-weight: 700; }

/* ─── Logo upload zone ─── */
.logo-drop-zone {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  gap: 8px; padding: 28px 20px;
  border: 2px dashed var(--r-border); border-radius: 16px;
  cursor: pointer; transition: all 0.25s;
  background: rgba(0,0,0,0.01);
  text-align: center;
}
.logo-drop-zone:hover,
.logo-drop-active {
  border-color: var(--accent);
  background: rgba(var(--accent-rgb), 0.04);
}
.logo-drop-error {
  border-color: #ef4444;
  background: rgba(239, 68, 68, 0.04);
}
.logo-drop-icon-wrap {
  width: 48px; height: 48px; border-radius: 14px;
  background: rgba(var(--accent-rgb), 0.08);
  display: flex; align-items: center; justify-content: center;
  transition: transform 0.2s;
}
.logo-drop-zone:hover .logo-drop-icon-wrap,
.logo-drop-active .logo-drop-icon-wrap { transform: scale(1.1); }
.logo-drop-icon { color: var(--accent); }
.logo-drop-text {
  font-size: 13px; font-weight: 600; color: var(--r-text-sub);
  margin: 0;
}
.logo-drop-link {
  color: var(--accent); font-weight: 800; text-decoration: underline;
  text-underline-offset: 3px;
}
.logo-drop-hint {
  font-size: 11px; color: var(--r-text-sub); opacity: 0.7; margin: 0;
}

/* ─── Logo preview ─── */
.logo-preview-wrap {
  display: flex; align-items: center; gap: 16px;
  padding: 14px 18px;
  background: rgba(0,0,0,0.015);
  border: 1px solid var(--r-border);
  border-radius: 16px;
}
.logo-preview-img {
  height: 52px; max-width: 140px;
  object-fit: contain; border-radius: 10px;
  border: 1px solid var(--r-border);
  padding: 6px; background: white;
}
.logo-preview-meta {
  display: flex; flex: 1; align-items: center; justify-content: space-between; gap: 12px;
}
.logo-preview-name {
  font-size: 13px; font-weight: 700; color: var(--r-text-main);
  display: flex; align-items: center; gap: 6px;
}
.logo-uploading-label { color: var(--accent); }
.logo-remove-btn {
  display: flex; align-items: center; justify-content: center;
  width: 30px; height: 30px; border-radius: 8px;
  border: 1px solid var(--r-border);
  background: rgba(239, 68, 68, 0.06); color: #ef4444;
  cursor: pointer; transition: all 0.2s; flex-shrink: 0;
}
.logo-remove-btn:hover { background: rgba(239, 68, 68, 0.12); transform: scale(1.1); }
.logo-remove-btn:disabled { opacity: 0.4; cursor: not-allowed; transform: none; }

/* ─── Error message ─── */
.logo-error-msg {
  display: flex; align-items: center; gap: 6px;
  font-size: 12px; font-weight: 600; color: #ef4444;
  margin: 0; padding: 8px 12px;
  background: rgba(239, 68, 68, 0.08);
  border-radius: 8px;
}

/* ─── Animations ─── */
.spin { animation: rotate 2s linear infinite; }
.spin-sm { animation: rotate 1.5s linear infinite; }
@keyframes rotate { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
.anim-reveal-up { animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes revealUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }

/* ─── Responsive ─── */
@media (max-width: 640px) {
  .form-row { flex-direction: column; }
}
</style>
