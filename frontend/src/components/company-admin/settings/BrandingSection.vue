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

    <div class="branding-layout">
      <!-- Left: Form -->
      <div class="branding-form">
        <!-- Company Name -->
        <div class="form-section">
          <div class="form-section-header">
            <Building2 :size="16" class="form-section-icon" />
            <span>Identité</span>
          </div>
          <div class="input-group">
            <label class="input-label">Nom de l'entreprise</label>
            <div class="input-wrap">
              <input type="text" :value="form.companyName" @input="$emit('update:form', { ...form, companyName: $event.target.value })" class="input-field" placeholder="Votre entreprise">
            </div>
          </div>
          <div class="input-group">
            <label class="input-label">Description</label>
            <div class="input-wrap">
              <textarea :value="form.description" @input="$emit('update:form', { ...form, description: $event.target.value })" class="input-field textarea" rows="3" placeholder="Décrivez votre entreprise en quelques mots..."></textarea>
            </div>
          </div>
        </div>

        <!-- Industry & Website -->
        <div class="form-section">
          <div class="form-section-header">
            <Globe :size="16" class="form-section-icon" />
            <span>Informations</span>
          </div>
          <div class="form-row">
            <div class="input-group flex-1">
              <label class="input-label">Secteur d'activité</label>
              <div class="input-wrap">
                <input type="text" :value="form.industry" @input="$emit('update:form', { ...form, industry: $event.target.value })" class="input-field" placeholder="Ex: Technologie">
              </div>
            </div>
            <div class="input-group flex-1">
              <label class="input-label">Site Web</label>
              <div class="input-wrap">
                <Link :size="15" class="input-icon" />
                <input type="text" :value="form.website" @input="$emit('update:form', { ...form, website: $event.target.value })" class="input-field" placeholder="https://...">
              </div>
            </div>
          </div>
          <div class="form-row">
            <div class="input-group flex-1">
              <label class="input-label">Pays</label>
              <div class="input-wrap">
                <MapPin :size="15" class="input-icon" />
                <input type="text" :value="form.country" @input="$emit('update:form', { ...form, country: $event.target.value })" class="input-field" placeholder="Ex: France">
              </div>
            </div>
            <div class="input-group flex-1">
              <label class="input-label">Téléphone</label>
              <div class="input-wrap">
                <Phone :size="15" class="input-icon" />
                <input type="text" :value="form.contactPhone" @input="$emit('update:form', { ...form, contactPhone: $event.target.value })" class="input-field" placeholder="+33 6...">
              </div>
            </div>
          </div>
        </div>

        <!-- Colors & Logo -->
        <div class="form-section">
          <div class="form-section-header">
            <Palette :size="16" class="form-section-icon" />
            <span>Apparence</span>
          </div>
          <div class="form-row">
            <div class="input-group flex-1">
              <label class="input-label">Couleur principale</label>
              <div class="color-picker-wrap">
                <div class="color-swatch" :style="{ background: form.primaryColor }">
                  <input type="color" :value="form.primaryColor" @input="$emit('update:form', { ...form, primaryColor: $event.target.value })" class="color-input-hidden">
                </div>
                <input type="text" :value="form.primaryColor" @input="$emit('update:form', { ...form, primaryColor: $event.target.value })" class="input-field color-hex">
              </div>
            </div>
            <div class="input-group flex-1">
              <label class="input-label">Couleur secondaire</label>
              <div class="color-picker-wrap">
                <div class="color-swatch" :style="{ background: form.secondaryColor }">
                  <input type="color" :value="form.secondaryColor" @input="$emit('update:form', { ...form, secondaryColor: $event.target.value })" class="color-input-hidden">
                </div>
                <input type="text" :value="form.secondaryColor" @input="$emit('update:form', { ...form, secondaryColor: $event.target.value })" class="input-field color-hex">
              </div>
            </div>
          </div>
          <div class="input-group">
            <label class="input-label">{{ $t('settings.branding.logo') }}</label>
            <div class="input-wrap">
              <Image :size="15" class="input-icon" />
              <input type="text" :value="form.logoUrl" @input="$emit('update:form', { ...form, logoUrl: $event.target.value })" class="input-field" placeholder="https://votre-logo.png">
            </div>
          </div>
        </div>
      </div>

      <!-- Right: Live Preview -->
      <div class="branding-preview">
        <div class="preview-label">
          <Eye :size="14" />
          <span>Aperçu en direct</span>
        </div>
        <div class="preview-container">
          <div class="preview-device">
            <!-- Browser Chrome -->
            <div class="browser-chrome">
              <div class="browser-dots">
                <span class="dot red"></span>
                <span class="dot yellow"></span>
                <span class="dot green"></span>
              </div>
              <div class="browser-address">{{ form.website || 'votre-site.com' }}</div>
            </div>
            <!-- Page Content -->
            <div class="preview-page" :style="{ '--brand': form.primaryColor, '--brand-alt': form.secondaryColor }">
              <div class="preview-nav">
                <img v-if="form.logoUrl" :src="form.logoUrl" class="preview-logo-img">
                <div v-else class="preview-logo-text" :style="{ color: form.primaryColor }">{{ form.companyName || 'Logo' }}</div>
                <div class="preview-nav-links">
                  <span class="nav-dot"></span>
                  <span class="nav-dot"></span>
                  <span class="nav-dot"></span>
                </div>
              </div>
              <div class="preview-hero">
                <div class="preview-badge" :style="{ background: form.primaryColor + '15', color: form.primaryColor }">
                  Nous recrutons
                </div>
                <div class="preview-hero-title">
                  Rejoignez <span :style="{ color: form.primaryColor }">{{ form.companyName || 'l\'équipe' }}</span>
                </div>
                <div class="preview-hero-desc">{{ form.description || 'Découvrez nos opportunités de carrière' }}</div>
                <div class="preview-cta" :style="{ background: form.primaryColor }">
                  Postuler maintenant
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { Save, Loader2, Building2, Globe, Link, MapPin, Phone, Palette, Image, Eye } from 'lucide-vue-next'

defineProps({
  form: Object,
  isSaving: Boolean
})

defineEmits(['save', 'update:form'])
</script>

<style scoped>
.admin-glass-card { background: var(--card-bg); border: 1px solid var(--r-border); border-radius: 32px; padding: 32px; }
.section-header-premium { display: flex; justify-content: space-between; align-items: center; margin-bottom: 36px; }
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

/* ─── Layout ─── */
.branding-layout {
  display: grid; grid-template-columns: 1fr 1fr; gap: 40px; align-items: start;
}

/* ─── Form ─── */
.branding-form { display: flex; flex-direction: column; gap: 28px; }

.form-section {
  background: rgba(0,0,0,0.015); border: 1px solid var(--r-border);
  border-radius: 20px; padding: 24px; display: flex; flex-direction: column; gap: 16px;
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
.color-hex { font-family: 'SF Mono', 'Fira Code', monospace; font-size: 13px; }

/* ─── Preview ─── */
.branding-preview { position: sticky; top: 32px; }

.preview-label {
  display: flex; align-items: center; gap: 8px; justify-content: center;
  font-size: 11px; font-weight: 800; color: var(--r-text-sub);
  text-transform: uppercase; letter-spacing: 1px; margin-bottom: 16px;
}

.preview-container {
  background: linear-gradient(145deg, #f0f4f8, #e2e8f0);
  padding: 32px; border-radius: 24px; border: 1px solid var(--r-border);
}

.preview-device {
  background: white; border-radius: 16px; overflow: hidden;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.15);
  border: 1px solid #e2e8f0;
}

/* Browser Chrome */
.browser-chrome {
  display: flex; align-items: center; gap: 12px;
  padding: 10px 16px; background: #f8fafc; border-bottom: 1px solid #e2e8f0;
}
.browser-dots { display: flex; gap: 6px; }
.dot { width: 10px; height: 10px; border-radius: 50%; }
.dot.red { background: #f87171; }
.dot.yellow { background: #fbbf24; }
.dot.green { background: #34d399; }
.browser-address {
  flex: 1; background: white; padding: 5px 12px; border-radius: 6px;
  font-size: 10px; color: #94a3b8; font-weight: 600;
  border: 1px solid #e2e8f0; text-align: center;
}

/* Page Content */
.preview-page { background: white; min-height: 280px; display: flex; flex-direction: column; }

.preview-nav {
  display: flex; justify-content: space-between; align-items: center;
  padding: 14px 20px; border-bottom: 1px solid #f1f5f9;
}
.preview-logo-img { height: 18px; }
.preview-logo-text { font-weight: 900; font-size: 13px; letter-spacing: -0.3px; }
.preview-nav-links { display: flex; gap: 8px; }
.nav-dot { width: 28px; height: 4px; background: #e2e8f0; border-radius: 4px; }

.preview-hero {
  flex: 1; display: flex; flex-direction: column;
  align-items: center; justify-content: center; gap: 12px;
  padding: 28px 20px; text-align: center;
}

.preview-badge {
  padding: 5px 14px; border-radius: 100px;
  font-size: 9px; font-weight: 800; text-transform: uppercase; letter-spacing: 1px;
}

.preview-hero-title {
  font-size: 18px; font-weight: 900; color: #1e293b; letter-spacing: -0.5px;
  line-height: 1.2;
}

.preview-hero-desc {
  font-size: 10px; color: #94a3b8; font-weight: 600;
  max-width: 240px; line-height: 1.5;
}

.preview-cta {
  padding: 8px 20px; color: white; border-radius: 8px;
  font-weight: 800; font-size: 10px; text-transform: uppercase;
  letter-spacing: 0.5px; margin-top: 4px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.15);
  transition: transform 0.2s;
}

.spin { animation: rotate 2s linear infinite; }
@keyframes rotate { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
.anim-reveal-up { animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes revealUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
