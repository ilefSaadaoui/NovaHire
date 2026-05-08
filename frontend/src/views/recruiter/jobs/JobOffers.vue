<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="jobs" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      
      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome">
          <div class="header-with-icon">
            <div class="icon-box-themed page-icon-glow">
              <Briefcase :size="24" stroke-width="2.5" />
            </div>
            <div>
              <h1 class="premium-title-themed">{{ $t('offers.title') }}</h1>
              <p class="welcome-sub">{{ $t('offers.subtitle', { count: filteredOffers.length }) }}</p>
            </div>
          </div>
        </div>
        
        <div class="r-header-tools">
          <button 
            class="btn-premium btn-accent-glow anim-reveal-right" 
            @click="$router.push('/jobs/new')"
          >
            <div class="btn-glow-layer"></div>
            <Plus :size="18" stroke-width="3" />
            <span>{{ $t('offers.new') }}</span>
          </button>
        </div>
      </header>

      <div class="page-body">
        <!-- Filters Bar -->
        <div class="r-card filter-toolbar-premium anim-reveal-down">
          <div class="search-box-premium">
            <Search :size="18" class="search-icon" />
            <input type="text" v-model="search" :placeholder="$t('offers.searchPlaceholder')" />
          </div>
          
          <div class="filter-actions-right">
            <div class="filter-group-premium">
              <PremiumSelect 
                v-model="filterStatus" 
                :options="statusOptions" 
                :placeholder="$t('offers.allStatus')"
                class="expert-filter-lux"
              />
              <PremiumSelect 
                v-model="filterType" 
                :options="typeOptions" 
                :placeholder="$t('offers.allTypes')"
                class="expert-filter-lux"
              />
            </div>

            <div class="view-mode-divider"></div>

            <div class="view-mode-selector">
              <button :class="{ active: viewMode === 'table' }" @click="viewMode = 'table'">
                <List :size="18" />
              </button>
              <button :class="{ active: viewMode === 'cards' }" @click="viewMode = 'cards'">
                <LayoutGrid :size="18" />
              </button>
            </div>
          </div>
        </div>

        <div v-if="isLoading" class="dashboard-loading">
          <div class="spinner"></div>
          <p>{{ $t('offers.loading') }}</p>
        </div>

        <div v-else-if="filteredOffers.length === 0" class="r-card anim-reveal-up" style="align-items: center; justify-content: center; padding: 60px 20px; text-align: center;">
          <div style="background: var(--r-main-bg); width: 64px; height: 64px; border-radius: 20px; display: flex; align-items: center; justify-content: center; color: var(--r-text-sub); margin-bottom: 24px; border: 1px solid var(--r-border);">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width: 32px;"><path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
          </div>
          <h3 style="font-size: 18px; font-weight: 800; color: var(--r-text-main); margin-bottom: 8px;">{{ $t('offers.noResult') }}</h3>
          <p style="font-size: 14px; font-weight: 500; color: var(--r-text-sub);">{{ $t('offers.noResultSub') }}</p>
        </div>

        <!-- Table View -->
        <div v-else-if="viewMode === 'table'" class="glass-table-container-lux anim-reveal-up">
          <table class="celestial-table-lux">
            <thead>
              <tr>
                <th class="col-pos">{{ $t('offers.table.position') }}</th>
                <th class="col-loc">{{ $t('offers.table.location') }}</th>
                <th class="col-type">{{ $t('offers.table.contract') }}</th>
                <th class="col-stats">{{ $t('offers.table.candidates') }}</th>
                <th class="col-date">{{ $t('offers.table.date') }}</th>
                <th class="col-status">{{ $t('offers.table.status') }}</th>
                <th class="col-actions"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(offer, index) in filteredOffers" 
                  :key="offer.id" 
                  class="table-row-premium anim-stagger-ui" 
                  :style="{ animationDelay: `${0.1 + (index * 0.05)}s` }">
                <td>
                  <div class="offer-info-cell" @click="$router.push(`/jobs/${offer.id}`)">
                    <div class="offer-icon-mini">
                      <Briefcase :size="14" />
                    </div>
                    <div class="offer-text-block">
                      <div class="offer-primary-title">{{ offer.title }}</div>
                      <div class="offer-secondary-dept">{{ offer.department || '—' }}</div>
                    </div>
                  </div>
                </td>
                <td>
                  <div class="location-pill">
                    <MapPin :size="12" />
                    <span>{{ offer.location }}</span>
                  </div>
                </td>
                <td>
                  <span class="type-badge-lux">{{ offer.type }}</span>
                </td>
                <td>
                  <div class="candidates-mini-dashboard">
                    <div class="count-box">
                      <Users :size="12" />
                      <strong>{{ offer.applications || 0 }}</strong>
                    </div>
                    <div class="mini-progress-lux">
                      <div class="fill" :style="{ width: Math.min((offer.applications || 0) * 10, 100) + '%' }"></div>
                    </div>
                  </div>
                </td>
                <td>
                  <span class="date-text-lux">{{ offer.date }}</span>
                </td>
                <td>
                  <div class="status-indicator-lux" :class="offer.status">
                    <span class="dot"></span>
                    <span class="label">{{ offer.statusLabel }}</span>
                  </div>
                </td>
                <td style="text-align: right;">
                  <div class="actions-wrapper-lux">
                    <div class="dropdown-expert" v-click-outside="() => activeMenuId === offer.id && (activeMenuId = null)">
                      <button class="btn-action-trigger" @click="activeMenuId = activeMenuId === offer.id ? null : offer.id">
                        <MoreVertical :size="18" />
                      </button>
                      
                      <transition name="pop-lux">
                        <div v-if="activeMenuId === offer.id" class="dropdown-menu-premium">
                          <button class="menu-item" @click="$router.push(`/jobs/${offer.id}`)">
                            <Eye :size="14" />
                            <span>Détails</span>
                          </button>
                          <button class="menu-item" @click="$router.push(`/jobs/${offer.id}/edit`)">
                            <FileEdit :size="14" />
                            <span>Modifier</span>
                          </button>
                          <button v-if="offer.status === 'draft'" class="menu-item publish-btn" @click="publishOffer(offer.id)">
                            <Plus :size="14" />
                            <span>Publier</span>
                          </button>
                          <button v-if="offer.status === 'active'" class="menu-item link-btn" @click="copyLink(offer)">
                            <Copy :size="14" />
                            <span>Copier le lien</span>
                          </button>
                          <div class="menu-divider"></div>
                          <button class="menu-item danger-btn" @click="deleteOffer(offer.id)">
                            <Trash2 :size="14" />
                            <span>Supprimer</span>
                          </button>
                        </div>
                      </transition>
                    </div>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Cards View -->
        <div v-else class="celestial-grid anim-reveal-up">
          <div v-for="(offer, index) in filteredOffers" 
               :key="offer.id" 
               class="offer-card-lux anim-stagger-ui" 
               :style="{ animationDelay: `${0.1 + (index * 0.1)}s` }"
               @click="$router.push(`/jobs/${offer.id}`)">
            
            <div class="card-header-lux">
              <div class="card-icon-lux">
                <Briefcase :size="24" />
              </div>
              <div class="card-status-lux">
                <div class="status-indicator-lux small" :class="offer.status">
                  <span class="dot"></span>
                  {{ offer.statusLabel }}
                </div>
              </div>
              <div class="card-actions-lux">
                <div class="dropdown-expert" v-click-outside="() => activeMenuId === offer.id && (activeMenuId = null)">
                  <button class="btn-mini-action" @click.stop="activeMenuId = activeMenuId === offer.id ? null : offer.id">
                    <MoreVertical :size="16" />
                  </button>
                  
                  <transition name="pop-lux">
                    <div v-if="activeMenuId === offer.id" class="dropdown-menu-premium" @click.stop>
                      <button class="menu-item" @click="$router.push(`/jobs/${offer.id}`)">
                        <Eye :size="14" />
                        <span>Détails</span>
                      </button>
                      <button class="menu-item" @click="$router.push(`/jobs/${offer.id}/edit`)">
                        <FileEdit :size="14" />
                        <span>Modifier</span>
                      </button>
                      <button v-if="offer.status === 'draft'" class="menu-item publish-btn" @click="publishOffer(offer.id)">
                        <Plus :size="14" />
                        <span>Publier</span>
                      </button>
                      <button v-if="offer.status === 'active'" class="menu-item" @click="copyLink(offer)">
                        <Copy :size="14" />
                        <span>Copier lien</span>
                      </button>
                      <div class="menu-divider"></div>
                      <button class="menu-item danger-btn" @click="deleteOffer(offer.id)">
                        <Trash2 :size="14" />
                        <span>Supprimer</span>
                      </button>
                    </div>
                  </transition>
                </div>
              </div>
            </div>

            <div class="card-body-lux">
              <h3 class="card-title-lux">{{ offer.title }}</h3>
              <div class="card-meta-lux">
                <span class="dept">{{ offer.department || '—' }}</span>
                <span class="sep">•</span>
                <span class="type">{{ offer.type }}</span>
              </div>
              <div class="card-location-lux">
                <MapPin :size="14" />
                <span>{{ offer.location }}</span>
              </div>
            </div>

            <div class="card-footer-lux">
              <div class="card-stats-lux">
                <div class="stat-group">
                  <span class="stat-val">{{ offer.applications || 0 }}</span>
                  <span class="stat-label">Candidats</span>
                </div>
                <div class="stat-progress-lux">
                  <div class="progress-bar-lux">
                    <div class="fill" :style="{ width: Math.min((offer.applications || 0) * 10, 100) + '%' }"></div>
                  </div>
                </div>
              </div>
              <div class="card-date-lux">
                {{ offer.date }}
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
  Briefcase, Plus, Search, LayoutGrid, List, 
  MoreVertical, Eye, FileEdit, Copy, Trash2, 
  MapPin, Users 
} from 'lucide-vue-next'

import PremiumSelect from '@/components/common/PremiumSelect.vue'
import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'
import { useToastStore } from '@/stores/toastStore'
import { useModalStore } from '@/stores/modalStore'

export default {
  name: 'JobOffers',
  components: { 
    Sidebar, PremiumSelect,
    Briefcase, Plus, Search, LayoutGrid, List,
    MoreVertical, Eye, FileEdit, Copy, Trash2,
    MapPin, Users
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
    const storedRole = (authStore.userRole || '').toLowerCase()
    const isCompanyOwner = authStore.isAdmin && !authStore.isSuperAdmin

    return {
      authStore,
      themeStore,
      isLoading: true,
      sidebarCollapsed: false,
      isCompanyOwner,
      search: '',
      filterStatus: '',
      filterType: '',
      viewMode: 'table',
      showCreateModal: false,
      isSaving: false,
      activeMenuId: null,
      offers: []
    }
  },
  computed: {
    statusOptions() {
      return [
        { value: '', label: this.$t('offers.allStatus') },
        { value: 'active', label: this.$t('offers.status.active') },
        { value: 'draft', label: this.$t('offers.status.draft') },
        { value: 'closed', label: this.$t('offers.status.closed') }
      ]
    },
    typeOptions() {
      return [
        { value: '', label: this.$t('offers.allTypes') },
        { value: 'CDI', label: 'CDI / Full-Time' },
        { value: 'CDD', label: 'CDD / Contract' },
        { value: 'Stage', label: 'Stage / Internship' },
        { value: 'Alternance', label: 'Alternance' },
        { value: 'Freelance', label: 'Freelance' }
      ]
    },
    filteredOffers() {
      return this.offers.filter(o => {
        const s = (this.search || '').toLowerCase().trim()
        const matchSearch = !s || 
          o.title.toLowerCase().includes(s) || 
          (o.location || '').toLowerCase().includes(s) ||
          (o.department || '').toLowerCase().includes(s) ||
          (o.type || '').toLowerCase().includes(s)

        const matchStatus = !this.filterStatus || o.status === this.filterStatus
        const matchType = !this.filterType || o.type === this.filterType
        
        return matchSearch && matchStatus && matchType
      })
    },
    sidebarVars() {
      // Returns empty by default as Sidebar now consumes global variables
      return {}
    }
  },
  async mounted() {
    await this.fetchOffers()
  },
  methods: {
    async fetchOffers() {
      this.isLoading = true
      try {
        const res = await api.get('/JobOffer')
        // Gérer la nouvelle structure paginée: res.data = { data: [...], total, page, limit }
        const data = res.data.data || []
        this.totalOffers = res.data.total || data.length
        
        this.offers = data.map(o => {
          // Normalize status
          const rawStatus = `${o.status || o.visibility || ''}`.toLowerCase()
          let status = 'draft'
          if (rawStatus === 'published' || rawStatus === 'active' || rawStatus === '1') status = 'active'
          else if (rawStatus === 'closed' || rawStatus === '2') status = 'closed'
          
          // Normalize type (Map backend/creation values to human labels)
          let type = o.type || 'CDI'
          if (type === 'FullTime') type = 'CDI'
          else if (type === 'Contract') type = 'CDD'
          else if (type === 'Internship') type = 'Stage'
          else if (type === 'Apprenticeship') type = 'Alternance'

          return {
            id: o.id,
            title: o.title,
            department: o.department || '—',
            location: o.location || 'N/A',
            type: type,
            applications: o.applicationsCount || 0,
            date: o.createdAt ? new Date(o.createdAt).toLocaleDateString('fr-FR') : '—',
            status: status,
            statusLabel: this.getStatusLabel(status),
            shareToken: o.shareToken
          }
        })
      } catch (err) {
        console.error('Erreur chargement offres:', err)
      } finally {
        this.isLoading = false
      }
    },
    getStatusLabel(status) {
      const map = {
        active: this.$t('offers.status.active'),
        paused: this.$t('offers.status.paused'),
        closed: this.$t('offers.status.closed'),
        draft: this.$t('offers.status.draft')
      }
      return map[status] || status
    },
    createShareToken() {
      if (window.crypto && typeof window.crypto.randomUUID === 'function') {
        return window.crypto.randomUUID().replace(/-/g, '')
      }
      return `${Date.now()}${Math.random().toString(16).slice(2)}`
    },
    getSharePayload(offer) {
      return {
        id: offer.id,
        title: offer.title,
        department: offer.department,
        location: offer.location,
        date: offer.date,
        type: offer.type,
        workMode: offer.location === 'Remote' ? 'Remote' : 'Hybride',
        salary: offer.type === 'Stage' ? 'Indemnité de stage' : 'À discuter',
        experience: offer.type === 'Stage' ? 'Sans expérience' : '1-2 ans',
        pageColor: this.accentColor,
        welcomeMsg: 'Rejoignez notre équipe et contribuez à des projets à fort impact.',
        visibility: 'link',
        deadline: '',
        company: 'NovaHire',
        description: `Nous recherchons un(e) ${offer.title} expérimenté(e) pour rejoindre notre équipe.`,
        benefits: ['Environnement dynamique', 'Technologies modernes', 'Formation continue'],
        skills: [offer.type, offer.department, 'Communication'],
        candidatureFields: [
          { id: 1, name: 'Prénom & Nom', type: 'Texte', enabled: true, required: true },
          { id: 2, name: 'Email', type: 'Email', enabled: true, required: true },
          { id: 3, name: 'Téléphone', type: 'Téléphone', enabled: true, required: false },
          { id: 4, name: 'LinkedIn', type: 'URL', enabled: true, required: false },
          { id: 5, name: 'Lettre de motivation', type: 'Texte long', enabled: true, required: false }
        ],
        requiredDocs: [
          { id: 1, name: 'CV (PDF obligatoire)', required: true },
          { id: 2, name: 'Lettre de motivation', required: false }
        ]
      }
    },
    async deleteOffer(id) {
      const modalStore = useModalStore()
      const confirmed = await modalStore.confirm({
        title: 'Supprimer cette offre ?',
        message: 'Cette action supprimera également toutes les candidatures associées. Voulez-vous continuer ?',
        confirmText: 'Supprimer',
        type: 'danger'
      })

      if (confirmed) {
        try {
          await api.delete(`/JobOffer/${id}`)
          this.offers = this.offers.filter(o => o.id !== id)
          const toastStore = useToastStore()
          toastStore.show(this.$t('offers.deleted'), 'success')
        } catch (err) {
          console.error('Erreur suppression:', err)
          // Toast déjà géré par l'intercepteur Axios
        }
      }
    },
    async publishOffer(id) {
      try {
        await api.patch(`/JobOffer/${id}/status`, { status: 'Published' })
        this.offers = this.offers.map(offer =>
          offer.id === id
            ? {
                ...offer,
                status: 'active',
                statusLabel: this.getStatusLabel('active')
              }
            : offer
        )
        this.activeMenuId = null
        useToastStore().show('Offre publiée avec succès !', 'success')
      } catch (err) {
        console.error('Erreur publication:', err)
        useToastStore().show('Erreur lors de la publication de l\'offre.', 'error')
      }
    },
    copyLink(offer) {
      if (!offer.shareToken) {
        useToastStore().show('Lien non disponible (Vérifiez le statut)', 'warning')
        return
      }
      const link = `${window.location.origin}/shared-job/${offer.shareToken}`
      
      navigator.clipboard.writeText(link).then(() => {
        useToastStore().show('Lien copié avec succès !', 'success')
      }).catch(err => {
        console.error('Erreur copie:', err)
      })
    }
  }
}
</script>

<style>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";
</style>

<style scoped>
/* Premium Header */
.icon-box-themed.page-icon-glow {
  width: 52px;
  height: 52px;
  border-radius: 16px;
  background: var(--accent-soft);
  color: var(--accent-color);
  box-shadow: 0 8px 25px var(--accent-soft);
  position: relative;
}

.premium-title-themed {
  font-size: 26px;
  font-weight: 900;
  letter-spacing: -0.5px;
  margin-bottom: 2px;
}

.btn-accent-glow {
  background: var(--accent-grad);
  color: white !important;
  padding: 12px 24px;
  border-radius: 14px;
  font-weight: 800;
  box-shadow: 0 10px 25px var(--accent-soft);
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.btn-accent-glow:hover {
  transform: translateY(-3px) scale(1.02);
  box-shadow: 0 15px 35px var(--accent-soft);
}

/* Filter Toolbar Premium */
.filter-toolbar-premium {
  background: var(--r-surface);
  border: 1px solid var(--r-border);
  padding: 8px 16px;
  border-radius: 18px;
  display: flex !important;
  flex-direction: row !important;
  flex-wrap: nowrap !important;
  align-items: center !important;
  justify-content: space-between !important;
  margin-bottom: 24px;
  box-shadow: var(--r-shadow);
  gap: 16px;
  height: 64px;
  overflow: visible !important;
  z-index: 100;
}

.search-box-premium {
  position: relative;
  display: flex;
  align-items: center;
  flex: 0 0 320px; /* Fix width to prevent weird stretching */
}

.filter-actions-right {
  display: flex;
  align-items: center;
  gap: 16px;
  overflow: visible !important;
}

.search-box-premium .search-icon {
  position: absolute;
  left: 16px;
  color: var(--r-text-sub);
  pointer-events: none;
}

.search-box-premium input {
  width: 100%;
  background: var(--r-main-bg);
  border: 1px solid transparent;
  padding: 10px 16px 10px 44px;
  border-radius: 14px;
  font-size: 14px;
  font-weight: 600;
  color: var(--r-text-main);
  transition: all 0.3s;
}

.search-box-premium input:focus {
  background: var(--r-surface);
  border-color: var(--accent-color);
  box-shadow: 0 0 0 4px var(--accent-soft);
  outline: none;
}

.filter-group-premium {
  display: flex;
  gap: 12px;
  overflow: visible !important;
}

.expert-filter-lux {
  position: relative;
  z-index: 1000;
}

.expert-filter-lux :deep(.premium-select-display) {
  height: 40px !important;
  min-width: 160px;
  background: var(--r-main-bg);
  border-radius: 12px;
  border: 1px solid var(--r-border) !important;
  padding-top: 0 !important;
  padding-bottom: 0 !important;
}

.view-mode-divider {
  width: 1px;
  height: 24px;
  background: var(--r-border);
}

.view-mode-selector {
  display: flex;
  background: var(--r-main-bg);
  padding: 3px;
  border-radius: 10px;
  gap: 3px;
}

.view-mode-selector button {
  width: 34px;
  height: 34px;
  border-radius: 8px;
  border: none;
  background: transparent;
  color: var(--r-text-sub);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.3s;
}

.view-mode-selector button.active {
  background: var(--r-surface);
  color: var(--accent-color);
  box-shadow: 0 4px 10px rgba(0,0,0,0.05);
}

/* Celestial Table Lux */
.glass-table-container-lux {
  background: var(--r-surface);
  border-radius: 24px;
  border: 1px solid var(--r-border);
  box-shadow: var(--r-shadow);
  position: relative;
  min-height: 200px;
  z-index: 1;
}

.celestial-table-lux {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
}

.celestial-table-lux th {
  padding: 20px 24px;
  text-align: left;
  font-size: 11px;
  font-weight: 850;
  color: var(--r-text-sub);
  text-transform: uppercase;
  letter-spacing: 1.5px;
  background: rgba(0, 0, 0, 0.01);
  border-bottom: 1px solid var(--r-border);
}

.celestial-table-lux th:first-child { border-top-left-radius: 24px; }
.celestial-table-lux th:last-child { border-top-right-radius: 24px; }
.table-row-premium:last-child td:first-child { border-bottom-left-radius: 24px; }
.table-row-premium:last-child td:last-child { border-bottom-right-radius: 24px; }

.table-row-premium {
  transition: all 0.3s ease;
  border-bottom: 1px solid var(--r-border);
}

.table-row-premium:hover {
  background: rgba(var(--accent-rgb), 0.02);
}

.table-row-premium td {
  padding: 16px 24px;
  vertical-align: middle;
}

.offer-info-cell {
  display: flex;
  align-items: center;
  gap: 16px;
  cursor: pointer;
}

.offer-icon-mini {
  width: 32px;
  height: 32px;
  border-radius: 10px;
  background: var(--accent-soft);
  color: var(--accent-color);
  display: flex;
  align-items: center;
  justify-content: center;
}

.offer-primary-title {
  font-size: 15px;
  font-weight: 800;
  color: var(--r-text-main);
  margin-bottom: 2px;
}

.offer-secondary-dept {
  font-size: 12px;
  font-weight: 600;
  color: var(--r-text-sub);
}

.location-pill {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 4px 10px;
  background: var(--r-main-bg);
  border-radius: 8px;
  font-size: 12px;
  font-weight: 600;
  color: var(--r-text-sub);
}

.type-badge-lux {
  font-size: 12px;
  font-weight: 750;
  color: var(--accent-color);
  padding: 4px 12px;
  background: var(--accent-soft);
  border-radius: 100px;
}

.celestial-table-lux th.col-stats {
  text-align: center;
}

.candidates-mini-dashboard {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 6px;
  width: 100%;
}

.count-box {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  font-size: 13px;
  color: var(--r-text-main);
}

.mini-progress-lux {
  height: 4px;
  width: 80px;
  background: var(--r-main-bg);
  border-radius: 10px;
  overflow: hidden;
}

.mini-progress-lux .fill {
  height: 100%;
  background: var(--accent-grad);
  border-radius: 10px;
}

.date-text-lux {
  font-size: 13px;
  font-weight: 600;
  color: var(--r-text-sub);
}

/* Status Indicator Lux */
.status-indicator-lux {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 6px 14px;
  border-radius: 100px;
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-indicator-lux.active {
  background: rgba(16, 185, 129, 0.1);
  color: #10b981;
}

.status-indicator-lux.draft {
  background: rgba(245, 158, 11, 0.1);
  color: #f59e0b;
}

.status-indicator-lux.closed {
  background: rgba(148, 163, 184, 0.1);
  color: #94a3b8;
}

.status-indicator-lux .dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: currentColor;
}

.status-indicator-lux.active .dot {
  box-shadow: 0 0 10px #10b981;
  animation: pulse-green 2s infinite;
}

@keyframes pulse-green {
  0% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.5); opacity: 0.5; }
  100% { transform: scale(1); opacity: 1; }
}

.btn-action-trigger {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  border: 1px solid var(--r-border);
  background: var(--r-main-bg);
  color: var(--r-text-sub);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.3s;
}

.btn-action-trigger:hover {
  background: var(--accent-soft);
  color: var(--accent-color);
  border-color: var(--accent-color);
}

.dropdown-expert {
  position: relative;
  display: inline-block;
}

/* Dropdown Premium */
.dropdown-menu-premium {
  position: absolute;
  top: 100%;
  right: 0;
  background: var(--r-surface);
  border: 1px solid var(--r-border);
  border-radius: 16px;
  padding: 8px;
  min-width: 180px;
  box-shadow: 0 15px 40px rgba(0,0,0,0.15);
  z-index: 100;
}

.menu-item {
  width: 100%;
  padding: 10px 14px;
  border: none;
  background: transparent;
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 13px;
  font-weight: 700;
  color: var(--r-text-main);
  border-radius: 10px;
  cursor: pointer;
  transition: 0.2s;
}

.menu-item:hover {
  background: var(--r-main-bg);
  color: var(--accent-color);
  transform: translateX(4px);
}

.menu-item.danger-btn {
  color: #ef4444;
}

.menu-item.danger-btn:hover {
  background: rgba(239, 68, 68, 0.1);
}

.menu-divider {
  height: 1px;
  background: var(--r-border);
  margin: 6px 0;
}

/* Cards View Lux */
.offer-card-lux {
  padding: 24px;
  border-radius: 24px;
  background: var(--r-surface);
  border: 1px solid var(--r-border);
  box-shadow: var(--r-shadow);
  transition: all 0.4s cubic-bezier(0.165, 0.84, 0.44, 1);
  cursor: pointer;
  display: flex;
  flex-direction: column;
}

.offer-card-lux:hover {
  transform: translateY(-10px);
  box-shadow: 0 25px 50px -12px rgba(0,0,0,0.1);
  border-color: var(--accent-color);
}

.celestial-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
}

.card-header-lux {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
}

.card-icon-lux {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  background: var(--accent-soft);
  color: var(--accent-color);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.card-actions-lux {
  margin-left: auto;
  position: relative;
}

.btn-mini-action {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  border: 1px solid var(--r-border);
  background: var(--r-main-bg);
  color: var(--r-text-sub);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.2s;
}

.btn-mini-action:hover {
  background: var(--accent-soft);
  color: var(--accent-color);
}

.card-body-lux {
  flex: 1;
}

.card-title-lux {
  font-size: 19px;
  font-weight: 850;
  color: var(--r-text-main);
  margin-bottom: 8px;
  line-height: 1.3;
}

.card-meta-lux {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 700;
  color: var(--r-text-sub);
  margin-bottom: 12px;
}

.card-location-lux {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
  color: var(--r-text-sub);
}

.card-footer-lux {
  margin-top: 24px;
  padding-top: 20px;
  border-top: 1px solid var(--r-border);
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
}

.card-stats-lux {
  display: flex;
  flex-direction: column;
  gap: 8px;
  flex: 1;
}

.stat-group {
  display: flex;
  align-items: baseline;
  gap: 6px;
}

.stat-val {
  font-size: 24px;
  font-weight: 900;
  color: var(--r-text-main);
}

.stat-label {
  font-size: 11px;
  font-weight: 800;
  color: var(--r-text-sub);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.stat-progress-lux {
  max-width: 140px;
}

.progress-bar-lux {
  height: 4px;
  background: var(--r-main-bg);
  border-radius: 10px;
  overflow: hidden;
}

.progress-bar-lux .fill {
  height: 100%;
  background: var(--accent-grad);
  border-radius: 10px;
}

.card-date-lux {
  font-size: 12px;
  font-weight: 700;
  color: var(--r-text-sub);
  opacity: 0.8;
}

/* Transitions */
.pop-lux-enter-active, .pop-lux-leave-active {
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
.pop-lux-enter-from, .pop-lux-leave-to {
  opacity: 0;
  transform: translateY(-10px) scale(0.95);
}

@media (max-width: 768px) {
  .filter-toolbar-premium {
    flex-direction: column !important;
    align-items: stretch !important;
    height: auto;
    gap: 12px;
  }
  .search-box-premium {
    flex: 1 1 auto;
    width: 100%;
  }
}
</style>
