<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="offres" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main stagger-reveal" :class="{ 'ml-collapsed': sidebarCollapsed }">
      


      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome" style="display: flex; flex-direction: row !important; align-items: center; gap: 20px;">
          <button class="back-link-premium" @click="$router.push('/offres')" title="Retour">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 24px; height: 24px;"><polyline points="15 18 9 12 15 6"/></svg>
          </button>
          <div class="header-details-premium" v-if="jobOffer">
            <h1 class="premium-title-themed" style="font-size: 28px; margin-bottom: 4px;">{{ jobOffer.title }}</h1>
            <p style="color: var(--r-text-sub); font-weight: 600; font-size: 14px;">{{ jobOffer.department || 'RH' }} • {{ jobOffer.location }}</p>
          </div>
        </div>
        <div class="r-header-tools" v-if="jobOffer">
          <div class="action-cluster">
            <button class="btn-premium btn-secondary" @click="$router.push(`/offres/${id}/edit`)">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 18px;"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
              {{ $t('common.edit') || 'Modifier' }}
            </button>
            <button class="btn-premium btn-danger" @click="handleDelete">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 18px;"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/></svg>
              {{ $t('common.delete') || 'Supprimer' }}
            </button>
            <button class="btn-luxury primary" @click="$router.push(`/candidatures?jobOfferId=${id}`)">
              {{ $t('common.pipeline') || 'Pipeline' }}
            </button>
          </div>
        </div>
      </header>

      <div v-if="isLoading" class="loading-state">
        <div class="spinner"></div>
        <p>{{ $t('dashboard.loading') }}</p>
      </div>

      <div v-else-if="jobOffer" class="page-body">
        <div class="detail-grid-expert">
          <!-- MAIN CONTENT -->
          <div class="detail-main-col">
            <div class="r-card detail-inner-card anim-stagger-up">
              <div class="section-intro-premium">
                <div class="icon-box-themed" style="width: 56px; height: 56px; border-radius: 18px;">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><polyline points="14 2 14 8 20 8"/><line x1="16" y1="13" x2="8" y2="13"/><line x1="16" y1="17" x2="8" y2="17"/><polyline points="10 9 9 9 8 9"/></svg>
                </div>
                <h3>{{ $t('createOffer.general.stepTitle') }}</h3>
              </div>
              
              <div class="offer-description-premium" v-html="formattedDescription"></div>

              <div class="metadata-grid-premium mt-40">
                <div class="meta-item">
                  <span class="m-label">{{ $t('createOffer.general.location') }}</span>
                  <span class="m-value"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>{{ jobOffer.location }}</span>
                </div>
                <div class="meta-item">
                  <span class="m-label">{{ $t('createOffer.general.contract') }}</span>
                  <span class="m-value"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>{{ jobOffer.type }}</span>
                </div>
                <div class="meta-item">
                  <span class="m-label">{{ $t('createOffer.general.salary') }}</span>
                  <span class="m-value"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="1" x2="12" y2="23"/><path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>{{ jobOffer.salaryRange || 'À discuter' }}</span>
                </div>
                <div class="meta-item">
                  <span class="m-label">{{ $t('createOffer.publish.deadline') }}</span>
                  <span class="m-value"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>{{ formatDate(jobOffer.deadline) }}</span>
                </div>
                <div class="meta-item">
                  <span class="m-label">{{ $t('createOffer.publish.action') }}</span>
                  <div class="status-trigger-premium" @click="toggleStatusMenu" v-click-outside="closeStatusMenu">
                    <span class="status-pill-premium" :class="jobOffer.status.toLowerCase()">
                      <span class="dot"></span>
                      {{ jobOffer.statusLabel || jobOffer.status }}
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" :class="{ rotated: showStatusMenu }"><polyline points="6 9 12 15 18 9"/></svg>
                    </span>
                    
                    <transition name="pop">
                      <div v-if="showStatusMenu" class="status-dropdown-premium">
                        <div v-for="s in statusOptions" :key="s.id" class="status-opt" :class="{ active: jobOffer.status.toLowerCase() === s.id }" @click.stop="updateStatus(s.id)">
                          <span class="opt-dot" :style="{ background: s.color }"></span>
                          {{ s.label }}
                        </div>
                      </div>
                    </transition>
                  </div>
                </div>
              </div>

              <!-- Skills Section -->
              <div class="skills-section-premium mt-40" v-if="jobOffer.skills && jobOffer.skills.length">
                <div class="section-sub-premium">{{ $t('createOffer.publish.weights.skills') }}</div>
                <div class="skills-chips-premium">
                  <span v-for="skill in jobOffer.skills" :key="skill" class="skill-pill">{{ skill }}</span>
                </div>
              </div>

              <div class="share-box-premium mt-40">
                <div class="share-head">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/></svg>
                  <div>
                    <h4>{{ $t('createOffer.publish.success.shareLink') }}</h4>
                    <p>{{ $t('createOffer.publish.success.subtitle') }}</p>
                  </div>
                </div>
                <div class="share-input-group">
                  <input type="text" readonly :value="shareUrl">
                  <button class="btn-copy" @click="copyShareLink">{{ $t('common.copy') || 'Copier' }}</button>
                </div>
              </div>
            </div>
          </div>

          <!-- SIDEBAR -->
          <div class="detail-side-col">
            <div class="r-card side-card anim-stagger-up" style="animation-delay: 0.2s">
              <div class="side-head">
                <h3 class="side-title">{{ $t('dashboard.candidates') }}</h3>
                <span class="count-pill">{{ applications.length }}</span>
              </div>
              
              <div v-if="isLoadingApps" class="side-loading">
                <div class="spinner sm"></div>
              </div>
              
              <div v-else-if="applications.length === 0" class="side-empty">
                <p>{{ $t('pipeline.empty') }}</p>
              </div>

              <div v-else class="side-app-list">
                <div v-for="app in applications.slice(0, 8)" :key="app.id" class="side-app-item" @click="$router.push(`/candidatures/profil/${app.id}`)">
                  <div class="r-avatar">
                    {{ app.firstName[0] }}{{ app.lastName[0] }}
                  </div>
                  <div class="side-app-info">
                    <div class="side-app-name">{{ app.firstName }} {{ app.lastName }}</div>
                    <div class="side-app-meta">{{ app.date }} · <span class="status-text">{{ app.stage }}</span></div>
                  </div>
                  <div class="side-app-score" v-if="app.score">
                    {{ app.score }}%
                  </div>
                </div>
                
                <button v-if="applications.length > 8" class="btn-premium btn-secondary w-full" @click="$router.push(`/candidatures?jobOfferId=${id}`)">
                  {{ $t('dashboard.pipeline.title') }}
                </button>
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
import Sidebar from '@/components/layout/Sidebar.vue'

import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'
import { useToastStore } from '@/stores/toastStore'
import { useModalStore } from '@/stores/modalStore'

export default {
  name: 'JobOfferDetail',
  components: { Sidebar },
  directives: {
    'click-outside': {
      mounted(el, binding) {
        el.clickOutsideEvent = (event) => {
          if (!(el === event.target || el.contains(event.target))) {
            binding.value(event)
          }
        }
        document.body.addEventListener('click', el.clickOutsideEvent)
      },
      unmounted(el) {
        document.body.removeEventListener('click', el.clickOutsideEvent)
      }
    }
  },
  data() {
    const authStore = useAuthStore()
    const themeStore = useThemeStore()
    const accentColor = authStore.themeColors.accent
    const accentDark = authStore.themeColors.accentDark
    const isCompanyOwner = authStore.isAdmin && !authStore.isSuperAdmin

    return {
      authStore,
      themeStore,
      id: this.$route.params.id,
      sidebarCollapsed: false,
      isLoading: true,
      isLoadingApps: true,
      jobOffer: null,
      applications: [],
      showStatusMenu: false,
      isCompanyOwner,
      accentColor,
      accentDark,
      statusOptions: [
        { id: 'draft', label: this.$t('createOffer.publish.status.draft'), color: '#94a3b8' },
        { id: 'published', label: this.$t('createOffer.publish.status.publish'), color: '#10b981' },
        { id: 'closed', label: this.$t('offers.status.closed'), color: '#f59e0b' },
        { id: 'archived', label: this.$t('offers.status.archived'), color: '#64748b' }
      ]
    }
  },
  computed: {
    sidebarVars() {
      return {}
    },
    formattedDescription() {
      if (!this.jobOffer || !this.jobOffer.description) return ''
      return this.jobOffer.description.replace(/\n/g, '<br>')
    },
    shareUrl() {
      if (!this.jobOffer || !this.jobOffer.shareToken) return ''
      return `${window.location.origin}/shared-job/${this.jobOffer.shareToken}`
    }
  },
  async mounted() {
    await this.fetchData()
  },
  methods: {
    async fetchData() {
      this.isLoading = true
      try {
        const res = await api.get(`/JobOffer/${this.id}`)
        this.jobOffer = res.data
        this.jobOffer.statusLabel = this.getStatusLabel(this.jobOffer.status.toLowerCase())
        await this.fetchApplications()
      } catch (err) {
        console.error('Erreur chargement offre:', err)
        useToastStore().show(this.$t('dashboard.error'), 'error')
        this.$router.push('/offres')
      } finally {
        this.isLoading = false
      }
    },
    async fetchApplications() {
      this.isLoadingApps = true
      try {
        const res = await api.get(`/Recruiter/applications?jobOfferId=${this.id}`)
        this.applications = res.data
      } catch (err) {
        console.error('Erreur chargement candidatures:', err)
      } finally {
        this.isLoadingApps = false
      }
    },
    getStatusLabel(status) {
      const opt = this.statusOptions.find(o => o.id === status)
      return opt ? opt.label : status
    },
    toggleStatusMenu() { this.showStatusMenu = !this.showStatusMenu },
    closeStatusMenu() { this.showStatusMenu = false },
    async updateStatus(newStatus) {
      this.showStatusMenu = false
      if (this.jobOffer.status.toLowerCase() === newStatus) return
      try {
        await api.patch(`/JobOffer/${this.id}/status`, { status: newStatus })
        this.jobOffer.status = newStatus
        this.jobOffer.statusLabel = this.getStatusLabel(newStatus)
        useToastStore().show(this.$t('notifications.updateSuccess'), 'success')
      } catch (err) {
        console.error('Erreur maj statut:', err)
        useToastStore().show(this.$t('notifications.updateError'), 'error')
      }
    },
    copyShareLink() {
      navigator.clipboard.writeText(this.shareUrl).then(() => {
        useToastStore().show(this.$t('notifications.copySuccess'), 'success')
      })
    },
    translateEnum(type, val) {
      if (!val) return '\u2014'
      const maps = {
        remote: { 
          'OnSite': this.$t('createOffer.location.onSite') || 'Sur site', 
          'Remote': this.$t('createOffer.location.remote') || 'Télétravail total', 
          'Hybrid': this.$t('createOffer.location.hybrid') || 'Hybride' 
        },
        experience: { 
          'Junior': this.$t('offers.experience.junior') || 'Junior (1-3 ans)', 
          'Intermediate': this.$t('offers.experience.intermediate') || 'Confirmé (3-5 ans)', 
          'Senior': this.$t('offers.experience.senior') || 'Senior (5-10 ans)', 
          'Expert': this.$t('offers.experience.expert') || 'Expert / Lead (10+ ans)',
          'Graduate': this.$t('offers.experience.beginner') || 'Débutant (0-1 an)'
        }
      }
      return maps[type]?.[val] || val
    },
    formatDate(date) {
      if (!date) return this.$t('common.noResults') || 'Aucune'
      const locale = this.$i18n.locale === 'ar' ? 'ar-SA' : (this.$i18n.locale === 'en' ? 'en-GB' : 'fr-FR')
      return new Date(date).toLocaleDateString(locale, { day: 'numeric', month: 'long', year: 'numeric' })
    },
    async handleDelete() {
      const modalStore = useModalStore()
      const confirmed = await modalStore.confirm({
        title: this.$t('offers.deleteConfirm'),
        message: this.$t('offers.deleteMessage'),
        confirmText: this.$t('common.delete'),
        type: 'danger'
      })
      if (confirmed) {
        try {
          await api.delete(`/JobOffer/${this.id}`)
          useToastStore().show(this.$t('notifications.deleteSuccess'), 'success')
          this.$router.push('/offres')
        } catch (err) {
          console.error('Erreur suppression:', err)
          useToastStore().show(this.$t('notifications.error'), 'error')
        }
      }
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
.back-link-premium {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--r-text-sub);
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  flex-shrink: 0;
}
.back-link-premium:hover {
  background: var(--accent-color);
  color: #fff;
  border-color: var(--accent-color);
  transform: translateX(-4px);
  box-shadow: 0 4px 12px var(--accent-soft);
}
.back-link-premium svg {
  width: 20px;
  height: 20px;
}

.detail-grid-expert {
  display: grid;
  grid-template-columns: 1fr 400px;
  gap: 32px;
  align-items: flex-start;
}

.detail-main-col {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.detail-inner-card {
  padding: 48px;
}

.section-intro-premium {
  display: flex;
  align-items: center;
  gap: 20px;
  margin-bottom: 40px;
}

.i-icon-box {
  width: 56px;
  height: 56px;
  border-radius: 18px;
  background: var(--accent-soft);
  color: var(--accent-color);
  display: flex;
  align-items: center;
  justify-content: center;
}

.i-icon-box svg { width: 28px; }
.section-intro-premium h3 { font-size: 24px; font-weight: 900; color: var(--r-text-main); margin: 0; }

.offer-description-premium {
  font-size: 17px;
  line-height: 1.8;
  color: var(--r-text-main);
  background: var(--r-main-bg);
  padding: 32px;
  border-radius: 24px;
  border: 1px solid var(--r-border);
}

.metadata-grid-premium {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 32px;
  padding: 32px;
  background: var(--r-main-bg);
  border-radius: 24px;
  border: 1px solid var(--r-border);
}

.meta-item { display: flex; flex-direction: column; gap: 8px; }
.m-label { font-size: 11px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 1px; }
.m-value { font-size: 16px; font-weight: 700; color: var(--r-text-main); display: flex; align-items: center; gap: 10px; }
.m-value svg { width: 18px; color: var(--accent-color); }

/* STATUS PICKER */
.status-trigger-premium { position: relative; cursor: pointer; }
.status-pill-premium {
  display: flex; align-items: center; gap: 10px; padding: 10px 16px; border-radius: 100px;
  background: var(--r-main-bg); border: 1px solid var(--r-border); font-size: 13px; font-weight: 800;
  color: var(--r-text-main); transition: 0.3s;
}
.status-pill-premium:hover { border-color: var(--accent-color); }
.status-pill-premium .dot { width: 8px; height: 8px; border-radius: 50%; }
.status-pill-premium.published { color: #10b981; }
.status-pill-premium.published .dot { background: #10b981; box-shadow: 0 0 10px #10b981; }
.status-pill-premium.closed { color: #f59e0b; }
.status-pill-premium.closed .dot { background: #f59e0b; }
.status-pill-premium.draft { color: var(--r-text-sub); }
.status-pill-premium.draft .dot { background: var(--r-text-sub); }
.status-pill-premium svg { width: 14px; transition: 0.3s; }
.status-pill-premium svg.rotated { transform: rotate(180deg); }

.status-dropdown-premium {
  position: absolute; top: calc(100% + 8px); right: 0; left: 0; min-width: 200px;
  background: var(--r-main-bg); border: 1px solid var(--r-border); border-radius: 16px; 
  padding: 8px; box-shadow: 0 15px 40px rgba(0,0,0,0.15); z-index: 1000;
}
.status-opt {
  padding: 12px 14px; border-radius: 12px; font-size: 13px; font-weight: 700;
  display: flex; align-items: center; gap: 10px; transition: 0.2s; color: var(--r-text-main);
  cursor: pointer;
}
.status-opt:hover { background: var(--accent-soft); color: var(--accent); }
.status-opt.active { background: var(--accent-soft); color: var(--accent); }
.opt-dot { width: 6px; height: 6px; border-radius: 50%; display: block; }

/* SKILLS */
.skills-section-premium { padding-top: 32px; border-top: 1px solid var(--r-border); }
.section-sub-premium { font-size: 13px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 1px; margin-bottom: 20px; }
.skills-chips-premium { display: flex; flex-wrap: wrap; gap: 10px; }
.skill-pill { padding: 8px 18px; border-radius: 100px; background: var(--r-main-bg); border: 1px solid var(--r-border); font-size: 13px; font-weight: 700; color: var(--r-text-main); }

/* SHARE AREA */
.share-box-premium {
  padding: 32px; background: var(--r-main-bg); border-radius: 24px;
  border: 1px solid var(--r-border); display: flex; flex-direction: column; gap: 20px;
}
.share-head { display: flex; align-items: center; gap: 16px; }
.share-head svg { width: 32px; height: 32px; color: var(--accent-color); min-width: 32px; }
.share-head h4 { font-size: 18px; font-weight: 800; color: var(--r-text-main); margin: 0; }
.share-head p { font-size: 13px; color: var(--r-text-sub); margin: 2px 0 0; }

.share-input-group {
  display: flex; gap: 10px; background: var(--r-card-bg); border-radius: 16px; padding: 6px 6px 6px 16px;
  border: 1px solid var(--r-border);
}
.share-input-group input { flex: 1; background: transparent; border: none; font-size: 13px; color: var(--r-text-main); font-weight: 600; outline: none; }
.btn-copy { background: var(--accent); color: var(--accent-contrast); border: none; padding: 10px 24px; border-radius: 12px; font-weight: 800; cursor: pointer; transition: 0.3s; }

/* SIDEBAR CARD */
.side-card { padding: 32px; }
.side-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
.side-title { font-size: 18px; font-weight: 800; color: var(--r-text-main); margin: 0; }
.count-pill { padding: 4px 12px; background: var(--accent-soft); color: var(--accent-color); border-radius: 100px; font-weight: 800; font-size: 12px; }

.side-app-item {
  display: flex; align-items: center; gap: 12px; padding: 16px; border-radius: 16px;
  background: var(--r-main-bg); border: 1px solid var(--r-border); transition: 0.3s; cursor: pointer;
  margin-bottom: 12px;
}
.side-app-item:hover { border-color: var(--accent-color); transform: translateX(-4px); }

.side-app-info { flex: 1; }
.side-app-name { font-size: 15px; font-weight: 700; color: var(--r-text-main); }
.side-app-meta { font-size: 12px; color: var(--r-text-sub); margin-top: 2px; }
.side-app-score { font-size: 13px; font-weight: 800; color: var(--accent-color); }

.w-full { width: 100%; }

/* LOADING */
.loading-state { display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 100px 0; gap: 20px; }
.spinner { width: 40px; height: 40px; border: 3px solid var(--r-border); border-top-color: var(--accent-color); border-radius: 50%; animation: spin 1s linear infinite; }
.spinner.sm { width: 20px; height: 20px; border-width: 2px; }
@keyframes spin { to { transform: rotate(360deg); } }

@media (max-width: 1200px) { .detail-grid-expert { grid-template-columns: 1fr; } }
</style>

