<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="profil" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      
      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome">
          <div class="header-with-icon">
            <div class="icon-box-themed" style="width: 52px; height: 52px; border-radius: 18px;">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 26px;"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            </div>
            <div>
              <h1 class="premium-title-themed">{{ $t('profile.title') }}</h1>
              <p class="welcome-sub">{{ $t('profile.subtitle') }}</p>
            </div>
          </div>
        </div>
        
        <div class="r-header-tools">
          <div class="profile-status-badge">
            <span class="pulse-dot"></span>
            {{ $t('profile.accountVerified') || 'Compte Vérifié' }}
          </div>
        </div>
      </header>

      <div class="page-body">
        <div class="profile-layout-grid">
          <!-- LEFT SIDE: IDENTITY CARD -->
          <div class="layout-side">
            <div class="r-card identity-premium admin-glass-card" style="overflow: visible; padding: 0;">
              <div class="card-cover halo-gold" style="background: var(--accent-grad);"></div>
              <div class="avatar-container">
                <div class="avatar-ring imperial-aura">
                  <img :src="avatarPreview || profile.avatar" alt="User" class="main-avatar">
                  <label for="avatar-picker" class="edit-trigger">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/><circle cx="12" cy="13" r="4"/></svg>
                  </label>
                  <input id="avatar-picker" type="file" hidden @change="handleAvatarUpload">
                </div>
              </div>

              <div class="identity-content">
                <h2 class="u-full-name imperial-aura">{{ profile.fullName }}</h2>
                <div style="margin-bottom: 12px;"><span class="u-badge">{{ profile.roleLabel }}</span></div>
                <p class="u-dept">{{ profile.departmentName || '—' }}</p>

                <div class="u-stats">
                  <div class="u-stat-item">
                    <div class="s-icon-mini">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
                    </div>
                    <span class="val">{{ profile.memberSinceShort }}</span>
                    <span class="lab">{{ $t('profile.stats.tenure') }}</span>
                  </div>
                  <div class="u-divider"></div>
                  <div class="u-stat-item">
                    <div class="s-icon-mini">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
                    </div>
                    <span class="val">98%</span>
                    <span class="lab">{{ $t('profile.stats.activity') }}</span>
                  </div>
                </div>
              </div>

              <div class="contact-list">
                <div class="contact-item" style="color: var(--r-text-sub);">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" :style="{ color: accentColor }"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                  <span>{{ profile.email }}</span>
                </div>
                <div class="contact-item" style="color: var(--r-text-sub);">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" :style="{ color: accentColor }"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
                  <span>{{ profile.phone || $t('profile.placeholders.notSet') }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- RIGHT SIDE: CONFIGURATION -->
          <div class="layout-main">
            <div class="premium-tabs-minimal" style="border-bottom-color: var(--r-border);">
              <button v-for="t in tabs" :key="t.id" class="minimal-tab" :class="{ active: activeTab === t.id }" @click="activeTab = t.id" :style="{ color: activeTab === t.id ? 'var(--r-text-main)' : 'var(--r-text-sub)' }">
                {{ t.label }}
                <div v-if="activeTab === t.id" style="position: absolute; bottom: -1px; left: 0; right: 0; height: 2px; border-radius: 2px; background: var(--accent-color);"></div>
              </button>
            </div>

            <div class="settings-panel">
              <!-- TAB: GENERAL INFO -->
              <div v-if="activeTab === 'info'" class="panel-fade">
                <div class="admin-glass-card panel-section" style="padding: 24px;">
                  <h4 style="font-size: 18px; font-weight: 800; color: #1e293b; margin-bottom: 24px;">{{ $t('profile.personalInfo') }}</h4>
                  <div class="form-grid-premium">
                    <div class="premium-input-group" :class="{ 'has-error': errors.fullName }">
                      <label style="color: var(--r-text-main);">{{ $t('profile.fullName') }}</label>
                      <input v-model="profile.fullName" type="text" :placeholder="$t('profile.placeholders.fullName')" @input="validateField('fullName')" style="background: var(--r-main-bg); color: var(--r-text-main); border-color: var(--r-border);">
                      <span v-if="errors.fullName" class="input-error-msg-mini">{{ errors.fullName }}</span>
                    </div>
                    <div class="premium-input-group">
                      <label style="color: var(--r-text-main);">{{ $t('profile.email') }}</label>
                      <input v-model="profile.email" type="email" disabled class="disabled-input" style="background: rgba(0,0,0,0.02) !important; color: var(--r-text-sub); border-color: var(--r-border);">
                    </div>
                    <div class="premium-input-group" :class="{ 'has-error': errors.phone }">
                      <label style="color: var(--r-text-main);">{{ $t('profile.phone') }}</label>
                      <input v-model="profile.phone" type="tel" :placeholder="$t('profile.placeholders.phone')" @input="validateField('phone')" style="background: var(--r-main-bg); color: var(--r-text-main); border-color: var(--r-border);">
                      <span v-if="errors.phone" class="input-error-msg-mini">{{ errors.phone }}</span>
                    </div>
                    <div class="premium-input-group">
                      <label style="color: var(--r-text-main);">{{ $t('profile.department') }}</label>
                      <!-- Admins can change their department -->
                      <PremiumSelect
                        v-if="isCompanyOwner"
                        v-model="profile.department"
                        :options="departmentOptions"
                        class="premium-select"
                        style="background: var(--r-main-bg); color: var(--r-text-main); border-color: var(--r-border); padding: 0;"
                      />
                      <!-- Recruiters: read-only display -->
                      <div
                        v-else
                        class="dept-readonly"
                        style="padding: 12px 16px; border-radius: 12px; border: 1px solid var(--r-border); background: rgba(0,0,0,0.02); color: var(--r-text-sub); font-size: 14px; font-weight: 500; display: flex; align-items: center; gap: 10px;"
                      >
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width:16px; flex-shrink:0; color: var(--accent-color);">
                          <path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/>
                        </svg>
                        <span>{{ profile.departmentName || '—' }}</span>
                      </div>
                    </div>
                  </div>
                </div>

                <div class="admin-glass-card panel-section" style="padding: 24px; margin-top: 24px;">
                  <h4 style="font-size: 18px; font-weight: 800; color: #1e293b; margin-bottom: 24px;">{{ $t('profile.interfaceSettings') }}</h4>
                  <div class="form-grid-premium">
                    <div class="premium-input-group">
                      <label style="color: var(--r-text-main);">{{ $t('common.language') }}</label>
                      <PremiumSelect 
                        v-model="profile.language" 
                        :options="languageOptions"
                        @change="changeLanguage" 
                        class="premium-select" 
                        style="background: var(--r-main-bg); color: var(--r-text-main); border-color: var(--r-border); padding: 0;"
                      />
                    </div>
                  </div>
                </div>

                <div class="form-footer" style="margin-top: 24px;">
                  <button class="btn-premium" style="background: var(--r-main-bg); color: var(--r-text-main); border: 1px solid var(--r-border);" @click="resetInfo">{{ $t('common.reset') }}</button>
                  <button class="btn-luxury primary" @click="saveInfo" :disabled="isSaving">
                    <svg v-if="isSaving" class="spinner" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle></svg>
                    {{ isSaving ? $t('profile.updating') : $t('common.save') }}
                  </button>
                </div>
              </div>

              <!-- TAB: SECURITY -->
              <div v-if="activeTab === 'security'" class="panel-fade">
                <div class="admin-glass-card panel-section" style="padding: 24px;">
                  <h4 style="font-size: 18px; font-weight: 800; color: #1e293b; margin-bottom: 8px;">{{ $t('profile.security.title') }}</h4>
                  <p class="section-hint" style="color: var(--r-text-sub); margin-top: 0;">{{ $t('profile.security.subtitle') }}</p>
                  
                  <div class="security-form">
                    <div class="premium-input-group">
                      <label style="color: var(--r-text-main);">{{ $t('profile.security.current') }}</label>
                      <input v-model="security.currentPassword" type="password" style="background: var(--r-main-bg); color: var(--r-text-main); border-color: var(--r-border);">
                    </div>
                    <div class="form-row-premium" style="display: grid; grid-template-columns: 1fr 1fr; gap: 24px; margin-top: 24px;">
                      <div class="premium-input-group" :class="{ 'has-error': errors.newPassword }">
                        <label>{{ $t('profile.security.new') }}</label>
                        <input v-model="security.newPassword" type="password" @input="validateField('newPassword')">
                        <span v-if="errors.newPassword" class="input-error-msg-mini">{{ errors.newPassword }}</span>
                      </div>
                      <div class="premium-input-group" :class="{ 'has-error': errors.confirmPassword }">
                        <label>{{ $t('profile.security.confirm') }}</label>
                        <input v-model="security.confirmPassword" type="password" @input="validateField('confirmPassword')">
                        <span v-if="errors.confirmPassword" class="input-error-msg-mini">{{ errors.confirmPassword }}</span>
                      </div>
                    </div>
                  </div>
                </div>

                <div class="form-footer">
                  <button class="btn-premium btn-secondary" @click="resetSecurity">{{ $t('profile.security.cancel') }}</button>
                  <button class="btn-luxury primary" @click="saveSecurity" :disabled="isSaving">
                    {{ $t('profile.security.changeBtn') }}
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>

<script>
import api from '@/api/axios'
import { useAuthStore } from '@/stores/authStore'
import { useLangStore } from '@/stores/langStore'
import { useToastStore } from '@/stores/toastStore'
import Sidebar from '@/components/layout/Sidebar.vue'

import PremiumSelect from '@/components/common/PremiumSelect.vue'

export default {
  name: 'Profile',
  components: { Sidebar,  PremiumSelect },
  data() {
    const authStore = useAuthStore()
    const langStore = useLangStore()
    const accentColor = authStore.themeColors.accent
    const accentDark = authStore.themeColors.accentDark
    const isCompanyOwner = authStore.isAdmin && !authStore.isSuperAdmin

    return {
      sidebarCollapsed: false,
      activeTab: 'info',
      avatarPreview: '',
      isSaving: false,
      isCompanyOwner,
      langStore,
      authStore,
      accentColor,
      accentDark,
      departments: [],
      profile: {
        fullName: '',
        email: '',
        phone: '',
        roleLabel: '',
        department: null,   // stores departmentId (Guid)
        memberSince: '',
        memberSinceShort: '',
        language: langStore.currentLang,
        avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=200&q=80'
      },
      errors: {
        fullName: '',
        phone: '',
        newPassword: '',
        confirmPassword: ''
      },
      security: { currentPassword: '', newPassword: '', confirmPassword: '' }
    }
  },
  computed: {
    tabs() {
      return [
        { id: 'info', label: this.$t('profile.info') },
        { id: 'security', label: this.$t('profile.securityTab') }
      ]
    },
    departmentOptions() {
      return this.departments.map(d => ({ value: d.id, label: d.name }))
    },
    languageOptions() {
      return [
        { value: 'fr', label: 'Français (France)' },
        { value: 'en', label: 'English (UK)' }
      ]
    },
    sidebarVars() {
      return {}
    }
  },
  async mounted() {
    const calls = [this.fetchProfile()]
    if (this.isCompanyOwner) calls.push(this.fetchDepartments())
    await Promise.all(calls)

  },
  methods: {
    async fetchDepartments() {
      try {
        const res = await api.get('/department')
        this.departments = res.data || []
      } catch (e) {
        console.error('Failed to fetch departments:', e)
        this.departments = []
      }
    },
    async fetchProfile() {
      try {
        const res = await api.get('/profile')
        const data = res.data
        this.profile = {
            ...this.profile,
            firstName: data.firstName,
            lastName: data.lastName,
            fullName: `${data.firstName} ${data.lastName}`,
            email: data.email,
            phone: data.phoneNumber || '',
            department: data.departmentId || null,
            departmentName: data.departmentName || '',
            roleLabel: data.role === 1 ? this.$t('roles.admin') : this.$t('roles.recruiter'),
            memberSince: new Date(data.createdAt).toLocaleDateString(this.langStore.currentLang, { month: 'long', year: 'numeric' }),
            memberSinceShort: new Date(data.createdAt).getFullYear().toString()
        }
        
        if (data.avatarUrl) {
            const apiBase = api.defaults.baseURL || '/api'
            this.profile.avatar = data.avatarUrl.startsWith('http') 
              ? data.avatarUrl 
              : `${apiBase.replace('/api', '')}${data.avatarUrl}`
        }

        this.initialProfile = JSON.parse(JSON.stringify(this.profile))
      } catch (e) { console.error(e) }
    },
    async handleAvatarUpload(e) {
      const file = e.target.files[0]
      if (!file) return

      // Preview localement
      this.avatarPreview = URL.createObjectURL(file)

      try {
        const formData = new FormData()
        formData.append('file', file)

        const res = await api.post('/profile/avatar', formData, {
          headers: { 
            'Content-Type': 'multipart/form-data'
          }
        })
        
        const apiBase = api.defaults.baseURL || '/api'
        this.profile.avatar = res.data.avatarUrl.startsWith('http')
          ? res.data.avatarUrl
          : `${apiBase.replace('/api', '')}${res.data.avatarUrl}`
        
        this.showToast(this.$t('notifications.avatarUpdate'), 'success')
      } catch (err) {
        console.error(err)
        this.showToast(this.$t('notifications.avatarError'), 'error')
      }
    },
    async saveInfo() {
      this.validateField('fullName')
      this.validateField('phone')
      if (this.errors.fullName || this.errors.phone) return

      this.isSaving = true
      try {
        // Séparer le nom complet en Prénom et Nom pour le backend
        const names = this.profile.fullName.trim().split(' ')
        const firstName = names[0]
        const lastName = names.slice(1).join(' ') || ''

        await api.put('/profile', {
          firstName,
          lastName,
          phoneNumber: this.profile.phone,
          departmentId: this.profile.department || null
        })

        this.initialProfile = JSON.parse(JSON.stringify(this.profile))
        this.showToast(this.$t('notifications.profileUpdate'), 'success')
      } catch (err) {
        console.error(err)
        // Toast est géré par l'intercepteur
      } finally {
        this.isSaving = false
      }
    },
    validateField(field) {
      const val = field.includes('Password') ? this.security[field] : this.profile[field]
      const nameRegex = /^[A-Za-zÀ-ÖØ-öø-ÿ\s-]+$/
      const phoneRegex = /^[+0-9\s-]{8,15}$/

      if (!val && field !== 'confirmPassword') {
        this.errors[field] = this.$t('profile.validation.required') || 'Champ requis'
        return
      }

      switch (field) {
        case 'fullName':
          if (!nameRegex.test(val)) this.errors.fullName = this.$t('profile.validation.lettersOnly') || 'Lettres uniquement'
          else this.errors.fullName = ''
          break
        case 'phone':
          if (!phoneRegex.test(val)) this.errors.phone = this.$t('profile.validation.invalidPhone') || 'Numéro invalide'
          else this.errors.phone = ''
          break
        case 'newPassword':
          if (val.length < 8) this.errors.newPassword = this.$t('profile.validation.minChar') || '8 caractères min'
          else if (!/[A-Z]/.test(val)) this.errors.newPassword = this.$t('profile.validation.upperCaseRequired') || 'Majuscule requise'
          else if (!/[0-9]/.test(val)) this.errors.newPassword = this.$t('profile.validation.digitRequired') || 'Chiffre requis'
          else this.errors.newPassword = ''
          break
        case 'confirmPassword':
          if (val !== this.security.newPassword) this.errors.confirmPassword = this.$t('profile.validation.notIdentical') || 'Non identique'
          else this.errors.confirmPassword = ''
          break
      }
    },
    resetInfo() { this.profile = JSON.parse(JSON.stringify(this.initialProfile)) },
    resetSecurity() { this.security = { currentPassword: '', newPassword: '', confirmPassword: '' } },
    async saveSecurity() { 
      this.validateField('newPassword')
      this.validateField('confirmPassword')
      if (this.errors.newPassword || this.errors.confirmPassword) return
      
      try {
        await api.post('/profile/change-password', {
          currentPassword: this.security.currentPassword,
          newPassword: this.security.newPassword,
          confirmPassword: this.security.confirmPassword
        })

        this.showToast(this.$t('notifications.passwordUpdate'), 'success') 
        this.resetSecurity()
      } catch (err) {
        console.error(err)
        // Toast est géré par l'intercepteur
      }
    },
    changeLanguage() {
      this.langStore.setLanguage(this.profile.language)
    },
    showToast(msg, type = 'info') {
      const toastStore = useToastStore()
      toastStore.show(msg, type)
    }
  }
}
</script>

<style>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";
</style>

<style scoped>
.dashboard-layout { background: transparent !important; }

/* COMPONENT-SPECIFIC STYLES */
.profile-layout-grid {
  display: grid;
  grid-template-columns: 380px 1fr;
  gap: 32px;
  align-items: flex-start;
}

/* COMPONENT-SPECIFIC STYLES OVERRIDE */
.icon-box-themed {
  box-shadow: 0 12px 28px var(--accent-soft);
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.profile-status-badge {
  display: flex;
  align-items: center;
  gap: 10px;
  background: #fff;
  color: var(--accent-color);
  padding: 10px 20px;
  border-radius: 100px;
  font-weight: 800;
  font-size: 13px;
  box-shadow: 0 4px 15px rgba(0,0,0,0.05);
  border: 1px solid var(--accent-soft);
}

.profile-status-badge .pulse-dot {
  background: var(--accent-color);
  width: 10px;
  height: 10px;
  border-radius: 50%;
  box-shadow: 0 0 10px var(--accent-color);
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(var(--accent-rgb), 0.7); }
  70% { transform: scale(1); box-shadow: 0 0 0 10px rgba(var(--accent-rgb), 0); }
  100% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(var(--accent-rgb), 0); }
}

/* IDENTITY CARD */
.identity-premium {
  padding: 0;
  overflow: visible;
  transition: transform 0.3s ease, box-shadow 0.3s ease;
}

.identity-premium:hover {
  transform: translateY(-5px);
  box-shadow: 0 30px 60px -12px rgba(0,0,0,0.15);
}

.card-cover {
  height: 140px;
  background: linear-gradient(135deg, var(--accent-color) 0%, var(--accent-dark) 100%);
  opacity: 1;
  border-radius: 28px 28px 0 0;
  position: relative;
  overflow: hidden;
}

.card-cover::after {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0; bottom: 0;
  background: url("data:image/svg+xml,%3Csvg width='100' height='100' viewBox='0 0 100 100' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M11 18c3.866 0 7-3.134 7-7s-3.134-7-7-7-7 3.134-7 7 3.134 7 7 7zm48 25c3.866 0 7-3.134 7-7s-3.134-7-7-7-7 3.134-7 7 3.134 7 7 7zm-43-7c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zm63 31c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zM34 90c1.657 0 3-1.343 3-3s-1.343-3-3-3-3 1.343-3 3 1.343 3 3 3zm56-76c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zM12 86c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zm66-3c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zm-46-43c1.105 0 2-.895 2-2s-.895-2-2-2-2 .895-2 2 .895 2 2 2zm20-27c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm58 48c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-6-33c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-33-6c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-5 67c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-13-10c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm58 28c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-6-27c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-33-20c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm5-76c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zM32 35c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm60 70c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm16-41c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-99-4s.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm72-32c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-37 13c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm60 59c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm11 11c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-75 14c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm13 0c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm19-3c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm33-31c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zM67 105c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-13 8c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm23-10c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-7 5c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm15 3c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-32-5c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-5-31c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm12-1c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm8-13c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-20 2c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-24 16c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zM0 37c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm13-7c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm45 15c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm65 15c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-42 6c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm42-26c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-8 47c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-40 4c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm10-33c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm50 20c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-15-48c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm15 14c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-83 6c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm69 5c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-2 29c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm13-17c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-28-12c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-15-12c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-10 15c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm24-11c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-32 3c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-9 35c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm65-1c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-10-23c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-10 44c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-13 36c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-33-31c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-6-24c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zM25 75c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm42 16c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-56-14c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm50 20c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm15-48c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm15 14c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-83 6c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm69 5c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-2 29c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm13-17c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-28-12c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-15-12c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-10 15c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm24-11c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-32 3c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-9 35c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm65-1c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-10-23c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-10 44c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-13 36c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-33-31c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1zm-6-24c.552 0 1-.448 1-1s-.448-1-1-1-1 .448-1 1 .448 1 1 1z' fill='%23ffffff' fill-opacity='0.1' fill-rule='evenodd'/%3E%3C/svg%3E");
}

.avatar-container {
  margin-top: -75px;
  display: flex;
  justify-content: center;
  position: relative;
  z-index: 2;
}

.avatar-ring {
  position: relative;
  padding: 8px;
  background: #fff;
  border-radius: 40px;
  box-shadow: 0 15px 45px rgba(0,0,0,0.12);
}

.main-avatar {
  width: 140px;
  height: 140px;
  border-radius: 34px;
  object-fit: cover;
  border: 4px solid #fff;
}

.edit-trigger {
  position: absolute;
  bottom: 5px;
  right: 5px;
  width: 42px;
  height: 42px;
  background: var(--accent-color);
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: var(--accent-contrast, #fff);
  box-shadow: 0 8px 20px var(--accent-soft);
  border: 2px solid #fff;
  transition: all 0.3s;
}

.edit-trigger:hover {
  transform: scale(1.1) rotate(5deg);
  background: var(--accent-dark);
}

.identity-content {
  padding: 32px;
  text-align: center;
}

.u-full-name {
  font-size: 28px;
  font-weight: 800;
  color: var(--r-text-main);
  margin-bottom: 8px;
  letter-spacing: -0.5px;
}

.u-badge {
  display: inline-block;
  padding: 6px 16px;
  border-radius: 100px;
  background: var(--accent-soft);
  color: var(--accent-color);
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 1px;
  margin-bottom: 16px;
}

.u-dept {
  color: var(--r-text-sub);
  font-size: 15px;
  font-weight: 600;
  margin-bottom: 32px;
}

.u-stats {
  display: flex;
  justify-content: space-around;
  padding: 24px 0;
  background: var(--r-main-bg);
  border-radius: 20px;
  margin-bottom: 24px;
}

.u-stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
}

.u-stat-item .val {
  font-size: 18px;
  font-weight: 800;
  color: var(--r-text-main);
}

.u-stat-item .lab {
  font-size: 10px;
  font-weight: 700;
  color: var(--r-text-sub);
  text-transform: uppercase;
}

.u-divider {
  width: 1px;
  background: var(--r-border);
}

.contact-list {
  padding: 0 32px 32px;
  display: flex; flex-direction: column;
  gap: 16px;
}

.contact-item {
  display: flex;
  align-items: center;
  gap: 12px;
  color: var(--r-text-sub);
  font-size: 14px;
  font-weight: 500;
}

.contact-item svg {
  width: 18px;
  color: var(--accent-color);
}

/* TABS */
.premium-tabs-minimal {
  display: flex;
  gap: 32px;
  margin-bottom: 24px;
  border-bottom: 1px solid var(--r-border);
}

.minimal-tab {
  padding: 12px 0;
  border: none;
  background: transparent;
  font-size: 15px;
  font-weight: 700;
  color: var(--r-text-sub);
  cursor: pointer;
  position: relative;
  transition: all 0.3s;
}

.minimal-tab.active {
  color: var(--r-text-main);
}

.minimal-tab.active::after {
  content: '';
  position: absolute;
  bottom: -1px;
  left: 0;
  right: 0;
  height: 2px;
  background: var(--accent-color);
  border-radius: 2px;
}

/* FORM */
.form-grid-premium {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 24px;
}

.premium-input-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.premium-input-group label {
  font-size: 13px;
  font-weight: 700;
  color: var(--r-text-main);
}

.premium-input-group input, .premium-select {
  padding: 12px 16px;
  border-radius: 12px;
  border: 1px solid var(--r-border);
  background: var(--r-main-bg);
  color: var(--r-text-main);
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s;
}

.premium-input-group input:focus {
  border-color: var(--accent-color);
  box-shadow: 0 0 0 4px var(--accent-soft);
  outline: none;
  background: #fff;
}

.form-footer {
  margin-top: 32px;
  display: flex;
  justify-content: flex-end;
  gap: 16px;
}

.btn-secondary {
  background: var(--r-main-bg);
  color: var(--r-text-main);
  border: 1px solid var(--r-border) !important;
}

.input-error-msg-mini {
  color: #ef4444;
  font-size: 11px;
  font-weight: 600;
  margin-top: 4px;
}

@media (max-width: 1100px) {
  .profile-layout-grid { grid-template-columns: 1fr; }
}
</style>


