<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="jobs" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main stagger-reveal" :class="{ 'ml-collapsed': sidebarCollapsed }">
      


      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome" style="display: flex; flex-direction: row !important; align-items: center; gap: 20px;">
          <button class="back-link-premium" @click="$router.push('/jobs')" title="Retour">
            <ChevronLeft :size="24" stroke-width="2.5" />
          </button>
          <div class="header-details-premium" v-if="jobOffer">
            <h1 class="premium-title-themed" style="font-size: 28px; margin-bottom: 4px;">{{ jobOffer.title }}</h1>
            <p style="color: var(--r-text-sub); font-weight: 600; font-size: 14px;">{{ jobOffer.department || 'RH' }} • {{ jobOffer.location }}</p>
          </div>
        </div>
        <div class="r-header-tools" v-if="jobOffer">
          <div class="action-cluster" style="display: flex; gap: 12px; align-items: center;">
            <button class="action-btn action-edit" @click="$router.push(`/jobs/${id}/edit`)">
              <FileEdit :size="18" stroke-width="2.5" />
              <span>Modifier</span>
            </button>
            <button class="action-btn action-danger" @click="handleDelete">
              <Trash2 :size="18" stroke-width="2.5" />
              <span>Supprimer</span>
            </button>
            <button class="action-btn action-main" @click="$router.push(`/applications?jobOfferId=${id}`)">
              <Users :size="18" stroke-width="2.5" />
              <span>Voir Pipeline</span>
            </button>
          </div>
        </div>
      </header>

      <div v-if="isLoading" class="loading-state">
        <div class="spinner"></div>
        <p>Chargement de l'offre...</p>
      </div>

      <div v-else-if="jobOffer" class="page-body">
        <div class="detail-grid-expert">
          <!-- MAIN CONTENT -->
          <div class="detail-main-col">
            <div class="r-card detail-inner-card anim-stagger-up">
              <div class="section-intro-premium">
                <div class="icon-box-themed" style="width: 56px; height: 56px; border-radius: 18px;">
                  <FileText :size="28" stroke-width="2" />
                </div>
                <h3>Description du poste</h3>
              </div>
              
              <div class="offer-description-premium" v-html="formattedDescription"></div>

              <!-- Skills Section -->
              <div class="skills-section-premium mt-40" v-if="jobOffer.skills && jobOffer.skills.length">
                <div class="section-sub-premium">Compétences recherchées</div>
                <div class="skills-chips-premium">
                  <span v-for="skill in jobOffer.skills" :key="skill" class="skill-pill">{{ skill }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- SIDEBAR -->
          <div class="detail-side-col">
            <!-- Information Box -->
            <div class="r-card side-card anim-stagger-up" style="animation-delay: 0.1s">
              <div class="side-head" style="margin-bottom: 24px;">
                <h3 class="side-title">Détails de l'offre</h3>
              </div>
              
              <div class="metadata-list-vertical">
                <div class="meta-item">
                  <span class="m-label">Localisation</span>
                  <span class="m-value"><MapPin :size="18" />{{ jobOffer.location }}</span>
                </div>
                <div class="meta-item">
                  <span class="m-label">Type</span>
                  <span class="m-value"><Briefcase :size="18" />{{ translateEnum('type', jobOffer.type) }}</span>
                </div>
                <div class="meta-item">
                  <span class="m-label">Rémunération</span>
                  <span class="m-value"><Banknote :size="18" />{{ jobOffer.salaryRange || 'À discuter' }}</span>
                </div>
                <div class="meta-item">
                  <span class="m-label">Date Limite</span>
                  <span class="m-value"><Calendar :size="18" />{{ formatDate(jobOffer.deadline) }}</span>
                </div>
                
                <div class="meta-item mt-24" style="border-top: 1px solid var(--r-border); padding-top: 20px;">
                  <span class="m-label">Statut Actuel</span>
                  <div class="status-trigger-premium mt-8" @click="toggleStatusMenu" v-click-outside="closeStatusMenu">
                    <span class="status-pill-premium" :class="jobOffer.status.toLowerCase()">
                      <span class="dot"></span>
                      {{ jobOffer.statusLabel || jobOffer.status }}
                      <ChevronDown :size="14" stroke-width="3" :class="{ rotated: showStatusMenu }" />
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
            </div>

            <!-- Share Box -->
            <div class="r-card side-card anim-stagger-up mt-24" style="animation-delay: 0.2s">
              <div class="share-box-content">
                <div class="share-head">
                  <Link :size="24" stroke-width="2.5" />
                  <div>
                    <h4 style="font-size: 16px; font-weight: 800; color: var(--r-text-main); margin: 0;">Lien public</h4>
                    <p style="font-size: 12px; margin-top: 2px; color: var(--r-text-sub);">Partage externe</p>
                  </div>
                </div>
                <div class="share-input-group mt-16">
                  <input type="text" readonly :value="shareUrl">
                  <button class="btn-copy" @click="copyShareLink">Copier</button>
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
import Sidebar from '@/components/layout/Sidebar.vue'
import { 
  ChevronLeft, FileEdit, Trash2, Users, FileText, 
  MapPin, Briefcase, Banknote, Calendar, ChevronDown, Link 
} from 'lucide-vue-next'

import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'
import { useToastStore } from '@/stores/toastStore'
import { useModalStore } from '@/stores/modalStore'

export default {
  name: 'JobOfferDetail',
  components: { 
    Sidebar,
    ChevronLeft, FileEdit, Trash2, Users, FileText,
    MapPin, Briefcase, Banknote, Calendar, ChevronDown, Link
  },
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
        { id: 'draft', label: 'Brouillon', color: '#94a3b8' },
        { id: 'published', label: 'Publiée', color: '#10b981' },
        { id: 'closed', label: 'Fermée', color: '#f59e0b' },
        { id: 'archived', label: 'Archivée', color: '#64748b' }
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
        useToastStore().show('Erreur lors du chargement de l\'offre', 'error')
        this.$router.push('/jobs')
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
        useToastStore().show('Statut mis à jour avec succès', 'success')
      } catch (err) {
        console.error('Erreur maj statut:', err)
        useToastStore().show('Erreur lors de la mise à jour du statut', 'error')
      }
    },
    copyShareLink() {
      navigator.clipboard.writeText(this.shareUrl).then(() => {
        useToastStore().show('Lien copié dans le presse-papier', 'success')
      })
    },
    translateEnum(type, val) {
      if (!val) return '\u2014'
      const maps = {
        remote: { 'OnSite': 'Sur site', 'Remote': 'Télétravail total', 'Hybrid': 'Hybride' },
        experience: { 'Junior': 'Junior (0-2 ans)', 'Intermediate': 'Intermédiaire (2-5 ans)', 'Senior': 'Senior (5-8 ans)', 'Expert': 'Expert (8+ ans)', 'Graduate': 'Jeune diplômé' },
        type: { 'FullTime': 'CDI', 'PartTime': 'Temps Partiel', 'Contract': 'CDD', 'Internship': 'Stage', 'Freelance': 'Freelance' }
      }
      return maps[type]?.[val] || val
    },
    formatDate(date) {
      if (!date) return 'Aucune'
      return new Date(date).toLocaleDateString('fr-FR', { day: 'numeric', month: 'long', year: 'numeric' })
    },
    async handleDelete() {
      const modalStore = useModalStore()
      const confirmed = await modalStore.confirm({
        title: 'Supprimer cette offre ?',
        message: 'Cette action est irréversible et supprimera toutes les candidatures associées.',
        confirmText: 'Supprimer définitivement',
        type: 'danger'
      })
      if (confirmed) {
        try {
          await api.delete(`/JobOffer/${this.id}`)
          useToastStore().show('Offre supprimée avec succès', 'success')
          this.$router.push('/jobs')
        } catch (err) {
          console.error('Erreur suppression:', err)
          useToastStore().show('Erreur lors de la suppression', 'error')
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
  border: none !important;
  box-shadow: 
    0 15px 50px -20px rgba(99, 102, 241, 0.08),
    0 0 0 1px rgba(99, 102, 241, 0.05),
    inset 0 2px 0 rgba(255, 255, 255, 0.6) !important;
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
  border: none;
  box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.05), inset 0 2px 0 rgba(255, 255, 255, 0.6);
}

.metadata-grid-premium {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 32px;
  padding: 32px;
  background: var(--r-main-bg);
  border-radius: 24px;
  border: none;
  box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.05), inset 0 2px 0 rgba(255, 255, 255, 0.6);
}

.metadata-list-vertical {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.mt-8 { margin-top: 8px; }
.mt-16 { margin-top: 16px; }
.mt-24 { margin-top: 24px; }
.mt-40 { margin-top: 40px; }

.meta-item { display: flex; flex-direction: column; gap: 8px; }
.m-label { font-size: 11px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 1px; }
.m-value { font-size: 15px; font-weight: 700; color: var(--r-text-main); display: flex; align-items: center; gap: 10px; }
.m-value svg { width: 18px; color: var(--accent-color); }

/* STATUS PICKER */
.status-trigger-premium { position: relative; cursor: pointer; }
.status-pill-premium {
  display: flex; align-items: center; gap: 10px; padding: 10px 16px; border-radius: 100px;
  background: var(--r-main-bg); border: none; box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.05); font-size: 13px; font-weight: 800;
  color: var(--r-text-main); transition: 0.3s;
}
.status-pill-premium:hover { box-shadow: 0 0 0 1px var(--accent-color); }
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
  background: var(--r-main-bg); border: none; border-radius: 16px; 
  padding: 8px; box-shadow: 0 15px 40px rgba(0,0,0,0.15), 0 0 0 1px rgba(99, 102, 241, 0.05); z-index: 1000;
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
.skill-pill { padding: 8px 18px; border-radius: 100px; background: var(--r-main-bg); border: none; box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.05); font-size: 13px; font-weight: 700; color: var(--r-text-main); }

/* SHARE AREA */
.share-box-premium {
  padding: 32px; background: var(--r-main-bg); border-radius: 24px;
  border: none; box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.05), inset 0 2px 0 rgba(255, 255, 255, 0.6); display: flex; flex-direction: column; gap: 20px;
}
.share-head { display: flex; align-items: center; gap: 16px; }
.share-head svg { width: 32px; height: 32px; color: var(--accent-color); min-width: 32px; }
.share-head h4 { font-size: 18px; font-weight: 800; color: var(--r-text-main); margin: 0; }
.share-head p { font-size: 13px; color: var(--r-text-sub); margin: 2px 0 0; }

.share-input-group {
  display: flex; gap: 10px; background: var(--r-card-bg); border-radius: 16px; padding: 6px 6px 6px 16px;
  border: none; box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.05);
}
.share-input-group input { flex: 1; background: transparent; border: none; font-size: 13px; color: var(--r-text-main); font-weight: 600; outline: none; }
.btn-copy { background: var(--accent); color: var(--accent-contrast); border: none; padding: 10px 24px; border-radius: 12px; font-weight: 800; cursor: pointer; transition: 0.3s; }

/* SIDEBAR CARD */
.side-card { 
  padding: 32px; 
  border: none !important;
  box-shadow: 
    0 15px 50px -20px rgba(99, 102, 241, 0.08),
    0 0 0 1px rgba(99, 102, 241, 0.05),
    inset 0 2px 0 rgba(255, 255, 255, 0.6) !important;
}
.side-head { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
.side-title { font-size: 18px; font-weight: 800; color: var(--r-text-main); margin: 0; }
.count-pill { padding: 4px 12px; background: var(--accent-soft); color: var(--accent-color); border-radius: 100px; font-weight: 800; font-size: 12px; }

.side-app-item {
  display: flex; align-items: center; gap: 12px; padding: 16px; border-radius: 16px;
  background: var(--r-main-bg); border: none; box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.05); transition: 0.3s; cursor: pointer;
  margin-bottom: 12px;
}
.side-app-item:hover { box-shadow: 0 0 0 1px var(--accent-color); transform: translateX(-4px); }

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

/* ACTION BUTTONS (Elite Theme) */
.action-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  border-radius: 14px;
  font-size: 13.5px;
  font-weight: 800;
  letter-spacing: 0.3px;
  text-transform: uppercase;
  border: none;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.action-edit {
  background: var(--r-main-bg);
  color: var(--r-text-main);
  box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.05);
}
.action-edit:hover {
  background: white;
  transform: translateY(-2px);
  box-shadow: 0 8px 20px -5px rgba(0,0,0,0.05), inset 0 2px 0 rgba(255, 255, 255, 0.6);
}

.action-danger {
  background: rgba(239, 68, 68, 0.05);
  color: #ef4444;
  box-shadow: 0 0 0 1px rgba(239, 68, 68, 0.1);
}
.action-danger:hover {
  background: #ef4444;
  color: white;
  transform: translateY(-2px);
  box-shadow: 0 8px 20px -5px rgba(239, 68, 68, 0.3), inset 0 2px 0 rgba(255, 255, 255, 0.2);
}

.action-main {
  background: var(--accent-grad, linear-gradient(135deg, var(--accent-color), #06b6d4));
  color: white;
  box-shadow: 0 4px 15px var(--accent-soft);
  position: relative;
  overflow: hidden;
}
.action-main::after {
  content: ''; position: absolute; top: 0; left: -100%; width: 50%; height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent);
  transition: 0.5s;
}
.action-main:hover::after { left: 100%; }
.action-main:hover {
  transform: translateY(-2px);
  box-shadow: 0 12px 25px -5px rgba(14, 165, 233, 0.5), inset 0 2px 0 rgba(255, 255, 255, 0.2);
  filter: brightness(1.05);
}

@media (max-width: 1200px) { .detail-grid-expert { grid-template-columns: 1fr; } }
</style>

