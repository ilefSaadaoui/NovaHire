<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="profile" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

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
          <div class="premium-status-elite">
            <span class="elite-pulse"></span>
            {{ $t('profile.verified') || 'Compte Vérifié' }}
          </div>
        </div>
      </header>

      <div class="page-body">
        <div class="profile-layout-grid">
          <!-- LEFT SIDE: IDENTITY -->
          <div class="layout-side anim-reveal-left">
            <div class="glass-panel-elite identity-card-elite">
              <div class="elite-card-header"></div>
              
              <div class="avatar-wrapper-elite">
                <div class="avatar-halo-ring">
                  <img v-if="avatarPreview || profile.avatar" :src="avatarPreview || profile.avatar" alt="User" class="avatar-elite-main">
                  <div v-else class="initials-avatar-elite">
                    {{ userInitials }}
                  </div>
                  
                  <label for="avatar-picker" class="avatar-edit-btn-elite">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z"/><circle cx="12" cy="13" r="4"/></svg>
                  </label>
                  <input id="avatar-picker" type="file" hidden @change="handleAvatarUpload">
                </div>
              </div>

              <div class="identity-info-elite">
                <h2 class="elite-user-name">{{ profile.fullName }}</h2>
                <div class="elite-role-tag-container">
                  <span class="elite-role-tag-gold">{{ profile.roleLabel }}</span>
                </div>
                <p v-if="!isCompanyOwner" class="elite-user-dept">{{ profile.department }}</p>

                <div class="elite-metric-grid">
                  <div class="metric-glass-item">
                    <div class="metric-icon-gold">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
                    </div>
                    <div class="metric-details">
                      <span class="metric-val">{{ profile.memberSinceShort }}</span>
                      <span class="metric-lab">{{ $t('profile.stats.tenure') }}</span>
                    </div>
                  </div>
                  <div class="metric-glass-item">
                    <div class="metric-icon-gold anim-pulse-live">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
                    </div>
                    <div class="metric-details">
                      <span class="metric-val">{{ activityScore }}%</span>
                      <span class="metric-lab">{{ $t('profile.stats.activity') }}</span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="elite-contact-strip">
                <div class="contact-pill-glass">
                  <svg class="pill-icon-gold" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                  <span>{{ profile.email }}</span>
                </div>
                <div class="contact-pill-glass">
                  <svg class="pill-icon-gold" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
                  <span>{{ profile.phone ? `${profile.countryCode} ${profile.phone}` : $t('profile.placeholders.notSet') }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- RIGHT SIDE: CONFIGURATION -->
          <div class="layout-main anim-reveal-right">
            <div class="glass-panel-elite settings-main-panel">
              <div class="elite-tabs-navigation">
                <button 
                  v-for="t in tabs" 
                  :key="t.id" 
                  class="elite-tab-btn" 
                  :class="{ active: activeTab === t.id }" 
                  @click="activeTab = t.id"
                >
                  {{ t.label }}
                  <div v-if="activeTab === t.id" class="elite-tab-indicator"></div>
                </button>
              </div>

              <div class="settings-viewport">
                <!-- TAB: GENERAL INFO -->
                <div v-if="activeTab === 'info'" class="tab-panel-elite">
                  <div class="settings-elite-section">
                    <h4 class="settings-elite-title">{{ $t('profile.personalInfo') }}</h4>
                    <div class="elite-form-grid vertical-stack">
                        <!-- ROW 1: NAME & EMAIL -->
                        <div class="elite-input-wrapper" :class="{ 'has-error': errors.fullName }">
                          <label>{{ $t('profile.fullName') }}</label>
                          <div class="input-with-icon">
                            <svg class="field-icon icon-accent" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
                            <input v-model="profile.fullName" type="text" :placeholder="$t('profile.placeholders.fullName')" @input="validateField('fullName')">
                          </div>
                          <span v-if="errors.fullName" class="error-gold">{{ errors.fullName }}</span>
                        </div>

                        <div class="elite-input-wrapper">
                          <label>{{ $t('profile.email') }}</label>
                          <div class="input-with-icon">
                            <svg class="field-icon icon-accent" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                            <input v-model="profile.email" type="email" disabled class="elite-input-disabled">
                          </div>
                        </div>

                        <!-- ROW 2: PHONE & LANGUAGE -->
                        <div class="elite-input-wrapper" :class="{ 'has-error': errors.phone }">
                          <label>{{ $t('profile.phone') }}</label>
                          <div class="phone-input-group">
                            <CountrySelector 
                              v-model="profile.countryCode" 
                              v-model:selectedIso="profile.selectedIso" 
                            />
                            <div class="input-with-icon flex-grow">
                              <svg class="field-icon icon-accent" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
                              <input v-model="profile.phone" type="tel" :placeholder="$t('profile.placeholders.phone')" @input="validateField('phone')">
                            </div>
                          </div>
                          <span v-if="errors.phone" class="error-gold">{{ errors.phone }}</span>
                        </div>

                        <div class="elite-input-wrapper">
                          <label>{{ $t('common.language') }}</label>
                          <PremiumSelect v-model="profile.language" :options="languageOptions" @change="changeLanguage" />
                        </div>

                        <!-- ROW 3: DEPARTMENT (Hidden for CompanyAdmin) -->
                        <div v-if="!isCompanyOwner" class="elite-input-wrapper full-width-sm mt-8">
                          <label>{{ $t('profile.department') }}</label>
                          <div class="department-assigned-badge">
                            <svg class="dept-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2L2 7l10 5 10-5-10-5z"/><path d="M2 17l10 5 10-5"/><path d="M2 12l10 5 10-5"/></svg>
                            <span>{{ profile.department || 'Non Assigné' }}</span>
                            <div class="locked-tooltip">Assigné par la Direction</div>
                          </div>
                        </div>
                    </div>
                  </div>



                  <div class="elite-panel-footer">
                    <button class="glass-btn-secondary" @click="resetInfo">
                      {{ $t('common.reset') }}
                    </button>
                    <button class="glass-btn-gold reflective-elite" @click="saveInfo" :disabled="isSaving">
                      <svg v-if="isSaving" class="spinner-gold" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none"></circle></svg>
                      {{ isSaving ? $t('profile.updating') : $t('common.save') }}
                    </button>
                  </div>
                </div>

                <!-- TAB: SECURITY -->
                <div v-if="activeTab === 'security'" class="tab-panel-elite">
                  <div class="settings-elite-section">
                    <h4 class="settings-elite-title">{{ $t('profile.security.title') }}</h4>
                    <p class="section-hint-gold">{{ $t('profile.security.subtitle') }}</p>
                    
                    <div class="elite-form-grid vertical-stack">
                      <div class="elite-input-wrapper">
                        <label>{{ $t('profile.security.current') }}</label>
                        <div class="input-with-icon">
                          <svg class="field-icon icon-accent" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                          <input v-model="security.currentPassword" :type="showPasswords.current ? 'text' : 'password'" :placeholder="$t('profile.placeholders.currentPassword')">
                          <button class="eye-toggle" @click="showPasswords.current = !showPasswords.current">
                            <svg v-if="!showPasswords.current" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                            <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
                          </button>
                        </div>
                      </div>
                      
                      <div class="elite-input-wrapper" :class="{ 'has-error': errors.newPassword }">
                        <label>{{ $t('profile.security.new') }}</label>
                        <div class="input-with-icon">
                          <svg class="field-icon icon-accent" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                          <input v-model="security.newPassword" :type="showPasswords.new ? 'text' : 'password'" :placeholder="$t('profile.placeholders.newPassword')" @input="validateField('newPassword')">
                          <button class="eye-toggle" @click="showPasswords.new = !showPasswords.new">
                            <svg v-if="!showPasswords.new" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                            <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
                          </button>
                        </div>
                        <span v-if="errors.newPassword" class="error-gold">{{ errors.newPassword }}</span>
                      </div>

                      <div class="elite-input-wrapper" :class="{ 'has-error': errors.confirmPassword }">
                        <label>{{ $t('profile.security.confirm') }}</label>
                        <div class="input-with-icon">
                          <svg class="field-icon icon-accent" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                          <input v-model="security.confirmPassword" :type="showPasswords.confirm ? 'text' : 'password'" :placeholder="$t('profile.placeholders.confirmPassword')" @input="validateField('confirmPassword')">
                          <button class="eye-toggle" @click="showPasswords.confirm = !showPasswords.confirm">
                            <svg v-if="!showPasswords.confirm" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                            <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
                          </button>
                        </div>
                        <span v-if="errors.confirmPassword" class="error-gold">{{ errors.confirmPassword }}</span>
                      </div>
                    </div>
                  </div>

                  <div class="elite-panel-footer">
                    <button class="glass-btn-secondary" @click="resetSecurity">
                      {{ $t('profile.security.cancel') }}
                    </button>
                    <button class="glass-btn-gold reflective-elite" @click="saveSecurity" :disabled="isSaving">
                      {{ $t('profile.security.changeBtn') }}
                    </button>
                  </div>
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
import CountrySelector from '@/components/common/CountrySelector.vue'
import countriesData from '@/assets/data/countries.json'

export default {
  name: 'Profile',
  components: { Sidebar, PremiumSelect, CountrySelector },
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
      showPasswords: { current: false, new: false, confirm: false },
      profile: {
        fullName: '',
        email: '',
        phone: '',
        countryCode: '+216',
        selectedIso: 'tn',
        roleLabel: '',
        department: '',
        departmentId: null,
        memberSince: '',
        memberSinceShort: '',
        language: langStore.currentLang,
        avatar: null 
      },
      activityScore: 98, // Mocked high activity score
      departments: [], // Dynamic departments from API
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
    userInitials() {
      if (!this.profile.fullName) return 'RN'
      return this.profile.fullName.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
    },
    departmentOptions() {
      // Map API departments to PremiumSelect format
      return this.departments.map(d => ({
        value: d.id,
        label: d.name
      }))
    },
    languageOptions() {
      return [
        { value: 'fr', label: 'Français (France)' },
        { value: 'en', label: 'English (UK)' },
        { value: 'ar', label: 'العربية (TN)' }
      ]
    },
    sidebarVars() {
      return {}
    }
  },
  async mounted() {
    await Promise.all([
      this.fetchProfile(),
      this.fetchDepartments()
    ])
  },
  methods: {
    async fetchDepartments() {
      try {
        const res = await api.get('/department')
        this.departments = res.data
      } catch (e) {
        console.error('Failed to fetch departments:', e)
      }
    },
    async fetchProfile() {
      try {
        const res = await api.get('/profile')
        const data = res.data
        
        let processedPhone = data.phoneNumber || ''
        let countryCode = '+216'
        let iso = 'tn'
        let phoneNum = processedPhone

        // Parse dial code if exists (e.g. "+216 12345678")
        if (processedPhone.startsWith('+')) {
          const parts = processedPhone.split(' ')
          if (parts.length >= 2) {
            countryCode = parts[0]
            phoneNum = parts.slice(1).join(' ')
            // Find ISO from data
            const found = countriesData.find(c => c.code === countryCode)
            if (found) iso = found.iso
          }
        }

        this.profile = {
          ...this.profile,
          firstName: data.firstName,
          lastName: data.lastName,
          fullName: `${data.firstName} ${data.lastName}`,
          email: data.email,
          phone: phoneNum,
          countryCode: countryCode,
          selectedIso: iso,
          departmentId: data.departmentId,
          department: data.department || 'Non Classé',
          roleLabel: data.jobTitle || (data.role === 1 ? this.$t('roles.admin') : this.$t('roles.recruiter')),
          memberSince: new Date(data.createdAt).toLocaleDateString(this.langStore.currentLang, { month: 'long', year: 'numeric' }),
          memberSinceShort: new Date(data.createdAt).getFullYear().toString()
        }
        
        if (data.avatarUrl) {
          const apiBase = api.defaults.baseURL || 'http://localhost:5000/api'
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
      this.avatarPreview = URL.createObjectURL(file)
      try {
        const formData = new FormData()
        formData.append('file', file)
        const res = await api.post('/profile/avatar', formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        })
        const apiBase = api.defaults.baseURL || 'http://localhost:5000/api'
        this.authStore.fetchUser()
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
        const names = this.profile.fullName.trim().split(' ')
        const firstName = names[0]
        const lastName = names.slice(1).join(' ') || ''
        await api.put('/profile', { 
          firstName, 
          lastName, 
          phoneNumber: `${this.profile.countryCode} ${this.profile.phone}`,
          departmentId: this.profile.departmentId // Now sending the real ID
        })
        
        // Update display department name
        const dep = this.departments.find(d => d.id === this.profile.departmentId)
        if (dep) this.profile.department = dep.name

        this.initialProfile = JSON.parse(JSON.stringify(this.profile))
        this.authStore.fetchUser()
        this.showToast(this.$t('notifications.profileUpdate'), 'success')
      } catch (err) {
        console.error(err)
      } finally {
        this.isSaving = false
      }
    },
    validateField(field) {
      const val = field.includes('Password') ? this.security[field] : this.profile[field]
      const nameRegex = /^[A-Za-zÀ-ÖØ-öø-ÿ\s-]+$/
      const phoneRegex = /^[0-9\s-]{8,15}$/ // Simplified as dial code is separate
      if (!val && field !== 'confirmPassword') {
        this.errors[field] = 'Champ requis'
        return
      }
      switch (field) {
        case 'fullName':
          if (!nameRegex.test(val)) this.errors.fullName = 'Lettres uniquement'
          else this.errors.fullName = ''
          break
        case 'phone':
          if (!phoneRegex.test(val)) this.errors.phone = 'Numéro invalide'
          else this.errors.phone = ''
          break
        case 'newPassword':
          if (val.length < 8) this.errors.newPassword = '8 caractères min'
          else if (!/[A-Z]/.test(val)) this.errors.newPassword = 'Majuscule requise'
          else if (!/[0-9]/.test(val)) this.errors.newPassword = 'Chiffre requis'
          else this.errors.newPassword = ''
          break
        case 'confirmPassword':
          if (val !== this.security.newPassword) this.errors.confirmPassword = 'Non identique'
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
      }
    },
    changeLanguage() { this.langStore.setLanguage(this.profile.language) },
    showToast(msg, type = 'info') {
      const toastStore = useToastStore()
      toastStore.show(msg, type)
    }
  }
}
</script>

<style scoped>
/* IMPERIAL GLASS ELITE: PROFILE SYSTEM */

.dashboard-layout { 
  background: transparent !important; 
  min-height: 100vh;
}

.page-body {
  padding: 24px 0;
}

.profile-layout-grid {
  display: grid;
  grid-template-columns: 380px 1fr;
  gap: 32px;
  align-items: flex-start;
}

/* TOPBAR CUSTOMS */
.premium-status-elite {
  display: flex;
  align-items: center;
  gap: 12px;
  background: var(--glass-bg);
  backdrop-filter: blur(12px);
  padding: 10px 20px;
  border-radius: 100px;
  border: 1px solid var(--accent-soft);
  color: var(--accent-color);
  font-weight: 800;
  font-size: 13px;
  box-shadow: 0 4px 20px var(--shadow-sm);
}

.elite-pulse {
  width: 10px;
  height: 10px;
  background: var(--accent-color);
  border-radius: 50%;
  box-shadow: 0 0 12px var(--accent-color);
  animation: pulse-elite 2s infinite;
}

@keyframes pulse-elite {
  0% { transform: scale(0.9); opacity: 0.8; }
  50% { transform: scale(1.1); opacity: 1; }
  100% { transform: scale(0.9); opacity: 0.8; }
}

/* IDENTITY CARD ELITE */
.identity-card-elite {
  padding: 0;
  overflow: visible;
  position: relative;
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.identity-card-elite:hover {
  transform: translateY(-10px);
  box-shadow: 0 40px 80px -15px var(--accent-soft);
}

.elite-card-header {
  height: 140px;
  border-radius: 28px 28px 0 0;
  background: linear-gradient(135deg, var(--accent-color), #3b82f6),
              radial-gradient(circle at 0% 0%, rgba(255,255,255,0.2) 0%, transparent 50%),
              radial-gradient(circle at 100% 100%, rgba(0,0,0,0.1) 0%, transparent 50%);
  position: relative;
  overflow: hidden;
}

.elite-card-header::after {
  content: '';
  position: absolute;
  inset: 0;
  background: url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter)'/%3E%3C/svg%3E");
  opacity: 0.05;
  mix-blend-mode: overlay;
}

.avatar-wrapper-elite {
  margin-top: -70px;
  display: flex;
  justify-content: center;
  position: relative;
  z-index: 5;
}

.avatar-halo-ring {
  position: relative;
  padding: 6px;
  background: rgba(255, 255, 255, 0.9);
  border-radius: 50%;
  box-shadow: 0 15px 45px rgba(0,0,0,0.1);
  border: 1px solid var(--r-border);
  display: flex;
  align-items: center;
  justify-content: center;
  backdrop-filter: blur(10px);
}

.avatar-elite-main {
  width: 140px;
  height: 140px;
  border-radius: 50%;
  object-fit: cover;
  border: 3px solid #fff;
  box-shadow: 0 8px 20px rgba(0,0,0,0.08);
  display: block;
}

.initials-avatar-elite {
  width: 140px;
  height: 140px;
  border-radius: 50%;
  background: var(--accent-grad);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 54px; /* Increased from 48px */
  font-weight: 900;
  color: #fff;
  text-shadow: 0 4px 12px rgba(0,0,0,0.2);
  border: 4px solid var(--card-bg);
}

.avatar-edit-btn-elite {
  position: absolute;
  bottom: 8px;
  right: 8px;
  width: 38px;
  height: 38px;
  background: var(--accent-color);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: #fff;
  border: 3px solid #fff;
  box-shadow: 0 6px 15px rgba(0,0,0,0.15);
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  z-index: 10;
}

.avatar-edit-btn-elite svg {
  width: 18px;
  height: 18px;
}

.avatar-edit-btn-elite:hover {
  transform: scale(1.15) rotate(15deg);
  background: var(--accent-dark);
}

.identity-info-elite {
  padding: 32px;
  text-align: center;
}

.elite-user-name {
  font-size: 28px;
  font-weight: 800;
  color: var(--r-text-main);
  margin-bottom: 8px;
  letter-spacing: -0.5px;
}

.elite-role-tag-container {
  margin-bottom: 12px;
}

.elite-role-tag-gold {
  display: inline-block;
  padding: 5px 16px;
  border-radius: 100px;
  background: var(--accent-soft);
  color: var(--accent-color);
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 1.5px;
}

.elite-user-dept {
  font-size: 16px;
  font-weight: 600;
  color: var(--r-text-sub);
  margin-bottom: 32px;
}

/* METRIC GRID */
.elite-metric-grid {
  display: flex;
  justify-content: space-around;
  padding: 16px;
  background: var(--r-main-bg);
  backdrop-filter: none;
  border-radius: 24px;
  border: 1px solid var(--r-border);
  margin: 0 16px;
}

.metric-glass-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
}

.metric-icon-gold {
  width: 38px;
  height: 38px;
  background: var(--accent-soft);
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--accent-color);
  transition: all 0.3s;
}

.anim-pulse-live {
  animation: pulse-live 2s infinite ease-in-out;
}

@keyframes pulse-live {
  0% { transform: scale(1); opacity: 0.8; }
  50% { transform: scale(1.1); opacity: 1; filter: drop-shadow(0 0 5px var(--accent-soft)); }
  100% { transform: scale(1); opacity: 0.8; }
}

.metric-details {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.metric-val {
  font-size: 18px;
  font-weight: 800;
  color: var(--r-text-main);
}

.metric-lab {
  font-size: 10px;
  font-weight: 700;
  color: var(--r-text-sub);
  text-transform: uppercase;
  letter-spacing: 1px;
}

.elite-contact-strip {
  padding: 0 32px 32px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.contact-pill-glass {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background: var(--glass-bg);
  border-radius: 16px;
  border: 1px solid var(--r-border);
  color: var(--r-text-sub);
  font-size: 14px;
  font-weight: 600;
  transition: all 0.2s;
}

.contact-pill-glass:hover {
  background: var(--card-bg);
  border-color: var(--accent-color);
  color: var(--r-text-main);
  transform: translateX(5px);
}

.pill-icon-gold {
  width: 18px;
  color: var(--accent-color);
}

/* SETTINGS PANEL ELITE */
.settings-main-panel {
  padding: 0;
  overflow: hidden;
}

.elite-tabs-navigation {
  display: flex;
  gap: 40px;
  padding: 0 32px;
  border-bottom: 1px solid var(--r-border);
}

.elite-tab-btn {
  padding: 24px 0;
  background: transparent;
  border: none;
  font-size: 15px;
  font-weight: 700;
  color: var(--r-text-sub);
  cursor: pointer;
  position: relative;
  transition: all 0.3s;
}

.elite-tab-btn.active {
  color: var(--r-text-main);
}

.elite-tab-indicator {
  position: absolute;
  bottom: -1px;
  left: 0;
  right: 0;
  height: 3px;
  background: var(--accent-grad);
  border-radius: 3px;
  box-shadow: 0 0 10px var(--accent-soft);
}

.settings-viewport {
  padding: 32px;
}

.settings-elite-section {
  margin-bottom: 40px;
}

.settings-elite-title {
  font-size: 20px;
  font-weight: 800;
  color: var(--r-text-main);
  margin-bottom: 24px;
  letter-spacing: -0.5px;
}

.elite-form-grid {
  display: grid;
  grid-template-columns: 1.5fr 0.5fr;
  gap: 32px;
}

.vertical-stack {
  grid-template-columns: 1fr !important;
  max-width: 550px;
}

.department-assigned-badge {
  display: flex;
  align-items: center;
  gap: 12px;
  background: var(--accent-soft);
  border: 1px dashed var(--accent-color);
  padding: 14px 20px;
  border-radius: 16px;
  color: var(--r-text-main);
  font-weight: 800;
  font-size: 14px;
  position: relative;
  cursor: not-allowed;
  transition: all 0.3s ease;
}

.department-assigned-badge:hover {
  background: rgba(0,0,0,0.02);
}

.department-assigned-badge .dept-icon {
  width: 20px;
  color: var(--accent-color);
}

.locked-tooltip {
  position: absolute;
  top: -35px;
  right: 10px;
  background: var(--r-text-main);
  color: var(--card-bg);
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 11px;
  font-weight: 800;
  opacity: 0;
  transform: translateY(10px);
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  pointer-events: none;
  box-shadow: 0 4px 15px rgba(0,0,0,0.1);
}

.locked-tooltip::after {
  content: '';
  position: absolute;
  bottom: -4px;
  right: 20px;
  width: 8px;
  height: 8px;
  background: var(--r-text-main);
  transform: rotate(45deg);
}

.department-assigned-badge:hover .locked-tooltip {
  opacity: 1;
  transform: translateY(0);
}

.elite-input-wrapper {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.elite-input-wrapper label {
  font-size: 13px;
  font-weight: 700;
  color: var(--r-text-sub);
  margin-left: 4px;
}

/* INPUT WITH ICON ENHANCEMENT */
.input-with-icon {
  position: relative;
  display: flex;
  align-items: center;
}

.field-icon {
  position: absolute;
  left: 16px;
  width: 18px;
  height: 18px;
  color: var(--r-text-sub);
  opacity: 0.6;
  pointer-events: none;
  transition: all 0.3s;
}

/* VIBRANT ICON COLOR */
.field-icon.icon-accent {
  color: var(--accent-color);
  opacity: 0.8;
}

.input-with-icon input {
  width: 100%;
  padding: 14px 18px 14px 44px !important; 
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  border-radius: 16px;
  color: var(--r-text-main);
  font-size: 15px;
  font-weight: 500;
  transition: all 0.3s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.input-with-icon input:focus {
  outline: none;
  border-color: var(--accent-color);
  background: var(--card-bg);
  box-shadow: 0 10px 25px -5px var(--accent-soft);
}

.input-with-icon input:focus + .field-icon {
  color: var(--accent-color);
  opacity: 1;
  filter: drop-shadow(0 0 5px var(--accent-soft));
}

/* PHONE INPUT GROUP */
.phone-input-group {
  display: flex;
  gap: 12px;
  align-items: stretch;
}

.flex-grow { flex-grow: 1; }

.eye-toggle {
  position: absolute;
  right: 14px;
  background: transparent;
  border: none;
  color: var(--r-text-sub);
  cursor: pointer;
  padding: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.3s;
}

.eye-toggle:hover {
  color: var(--accent-color);
  transform: scale(1.1);
}

.eye-toggle svg {
  width: 20px;
}

.elite-input-disabled {
  background: rgba(0,0,0,0.03) !important;
  color: var(--r-text-sub);
  border-style: dashed !important;
  cursor: not-allowed;
}

.elite-panel-footer {
  display: flex;
  justify-content: flex-end;
  gap: 20px;
  margin-top: 40px;
  padding-top: 32px;
  border-top: 1px solid var(--r-border);
}

/* ERROR STYLING */
.error-gold {
  color: #ef4444;
  font-size: 11px;
  font-weight: 700;
  margin: 4px 0 0 4px;
}

.section-hint-gold {
  font-size: 14px;
  color: var(--r-text-sub);
  margin-bottom: 32px;
  font-weight: 500;
}

.spacer-row {
  grid-column: span 2;
  height: 0;
}

/* BUTTONS */
.glass-btn-gold {
  position: relative;
  padding: 14px 32px;
  background: var(--accent-grad);
  border: none;
  border-radius: 16px;
  color: #fff;
  font-weight: 800;
  font-size: 15px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 10px;
  transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 10px 30px var(--accent-soft);
  overflow: hidden;
}

.glass-btn-gold:hover:not(:disabled) {
  transform: translateY(-4px) scale(1.02);
  box-shadow: 0 20px 40px var(--accent-soft);
}

.glass-btn-gold:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.glass-btn-secondary {
  padding: 14px 32px;
  background: var(--glass-bg);
  border: 1px solid var(--r-border);
  border-radius: 16px;
  color: var(--r-text-main);
  font-weight: 700;
  font-size: 15px;
  cursor: pointer;
  transition: all 0.3s;
}

.glass-btn-secondary:hover:not(:disabled) {
  background: var(--card-bg);
  border-color: var(--accent-color);
  box-shadow: 0 8px 20px var(--shadow-sm);
}

.spinner-gold {
  width: 18px;
  height: 18px;
  animation: rotate-elite 2s linear infinite;
}

@keyframes rotate-elite {
  100% { transform: rotate(360deg); }
}

/* REVEAL ANIMATIONS */
.anim-reveal-left {
  animation: reveal-left 0.8s cubic-bezier(0.165, 0.84, 0.44, 1);
}

.anim-reveal-right {
  animation: reveal-right 0.8s cubic-bezier(0.165, 0.84, 0.44, 1);
}

@keyframes reveal-left {
  0% { transform: translateX(-30px); opacity: 0; }
  100% { transform: translateX(0); opacity: 1; }
}

@keyframes reveal-right {
  0% { transform: translateX(30px); opacity: 0; }
  100% { transform: translateX(0); opacity: 1; }
}

@media (max-width: 1100px) {
  .profile-layout-grid { grid-template-columns: 1fr; }
  .elite-form-grid { grid-template-columns: 1fr; }
}
</style>
