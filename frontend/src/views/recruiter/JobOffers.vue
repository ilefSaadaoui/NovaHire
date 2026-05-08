<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="offres" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      
      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome">
          <div class="header-with-icon" style="flex-direction: row; align-items: center; gap: 16px;">
            <div class="icon-box-themed" style="width: 52px; height: 52px; border-radius: 16px;">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 26px;"><rect x="2" y="7" width="20" height="14" rx="2" ry="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
            </div>
            <div>
              <h1 class="premium-title-themed" style="font-size: 28px;">{{ $t('offers.title') }}</h1>
              <p class="welcome-sub" style="margin-top: 4px;">{{ $t('offers.subtitle', { count: filteredOffers.length }) }}</p>
            </div>
          </div>
        </div>
        
        <div class="r-header-tools">

          <button 
            class="btn-luxury primary anim-reveal-right" 
            style="animation-delay: 0.2s"
            @click="$router.push('/offres/nouvelle')"
          >
            <div class="btn-glow"></div>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
            {{ $t('offers.new') }}
          </button>
        </div>
      </header>

      <div class="page-body">
        <!-- Filters Bar -->
        <div class="r-card filter-card anim-reveal-down">
          <div class="r-search">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            <input type="text" v-model="search" :placeholder="$t('offers.searchPlaceholder')" />
          </div>
          
          <div class="filter-group">
            <PremiumSelect 
              v-model="filterStatus" 
              :options="statusOptions" 
              :placeholder="$t('offers.allStatus')"
              class="expert-filter"
            />
            <PremiumSelect 
              v-model="filterType" 
              :options="typeOptions" 
              :placeholder="$t('offers.allTypes')"
              class="expert-filter"
            />
          </div>

          <div class="view-switch">
            <button :class="{ active: viewMode === 'table' }" @click="viewMode = 'table'" :title="$t('offers.viewList')">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/></svg>
            </button>
            <button :class="{ active: viewMode === 'cards' }" @click="viewMode = 'cards'" :title="$t('offers.viewCards')">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/></svg>
            </button>
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
        <div v-else-if="viewMode === 'table'" class="glass-table-container anim-reveal-up">
          <table class="celestial-table">
            <thead>
              <tr>
                <th>{{ $t('offers.table.position') }}</th>
                <th>{{ $t('offers.table.location') }}</th>
                <th>{{ $t('offers.table.contract') }}</th>
                <th>{{ $t('offers.table.candidates') }}</th>
                <th>{{ $t('offers.table.date') }}</th>
                <th>{{ $t('offers.table.status') }}</th>
                <th style="text-align: right;">{{ $t('offers.table.actions') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(offer, index) in filteredOffers" 
                  :key="offer.id" 
                  class="anim-stagger-ui" 
                  :style="{ animationDelay: `${0.1 + (index * 0.05)}s` }">
                <td>
                  <div class="offer-cell">
                    <div class="offer-icon">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
                    </div>
                    <div class="offer-meta clickable-offer" @click="$router.push(`/offres/${offer.id}`)">
                      <div class="offer-title">{{ offer.title }}</div>
                      <div class="offer-dept">{{ offer.department || '—' }}</div>
                    </div>
                  </div>
                </td>
                <td><span class="glass-meta">{{ offer.location }}</span></td>
                <td><span class="glass-pill-type">{{ offer.type }}</span></td>
                <td>
                  <div class="stats-mini-visual">
                    <div class="val"><strong>{{ offer.applications || 0 }}</strong></div>
                    <div class="progress-track-lux">
                      <div class="progress-fill-lux" :style="{ width: Math.min((offer.applications || 0) * 10, 100) + '%' }"></div>
                    </div>
                  </div>
                </td>
                <td><span class="glass-meta">{{ offer.date }}</span></td>
                <td>
                  <div class="status-pill-glowing" :class="offer.status">
                    <span class="pulse-dot"></span>
                    {{ offer.statusLabel }}
                  </div>
                </td>
                <td>
                  <div class="action-grid-lux">
                    <div class="dropdown-expert" v-click-outside="() => activeMenuId === offer.id && (activeMenuId = null)">
                      <button class="glass-btn-sm" @click="activeMenuId = activeMenuId === offer.id ? null : offer.id" title="Plus d'actions">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="5" r="1"/><circle cx="12" cy="12" r="1"/><circle cx="12" cy="19" r="1"/></svg>
                      </button>
                      
                        <transition name="pop-fast">
                          <div v-if="activeMenuId === offer.id" class="dropdown-menu-lux right">
                            <button class="dropdown-item" @click="$router.push(`/offres/${offer.id}`)">
                              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                              Détails
                            </button>
                            <button class="dropdown-item" @click="$router.push(`/offres/${offer.id}/edit`)">
                              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                              Modifier
                            </button>
                            <button v-if="offer.status === 'draft'" class="dropdown-item gold-action" @click="publishOffer(offer.id)">
                              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 3v12"/><path d="M7 10l5 5 5-5"/><path d="M5 21h14"/></svg>
                              Publier
                            </button>
                            <button v-if="offer.status === 'active'" class="dropdown-item gold-action" @click="copyLink(offer)">
                             <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"/><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/></svg>
                             Copier le lien
                            </button>
                          <div class="dropdown-sep"></div>
                          <button class="dropdown-item danger" @click="deleteOffer(offer.id)">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/></svg>
                            Supprimer
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
        <div v-else class="celestial-grid anim-reveal-up" style="gap: 24px; display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));">
          <div v-for="(offer, index) in filteredOffers" 
               :key="offer.id" 
               class="r-card offer-card anim-stagger-ui" 
               :style="{ animationDelay: `${0.1 + (index * 0.1)}s` }"
               @click="$router.push(`/offres/${offer.id}`)">
            
            <!-- Card Top Row: icon + status badge + action menu -->
            <div class="offer-card-top">
              <div class="r-item-icon offer-icon-wrapper">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width: 28px;"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>
              </div>
              <div class="offer-card-badges">
                <div class="status-pill-glowing small" :class="offer.status">
                  <span class="pulse-dot"></span>
                  {{ offer.statusLabel }}
                </div>
                <div class="dropdown-expert" v-click-outside="() => activeMenuId === offer.id && (activeMenuId = null)">
                  <button class="glass-btn-sm" @click.stop="activeMenuId = activeMenuId === offer.id ? null : offer.id">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="5" r="1"/><circle cx="12" cy="12" r="1"/><circle cx="12" cy="19" r="1"/></svg>
                  </button>
                  <transition name="pop-fast">
                    <div v-if="activeMenuId === offer.id" class="dropdown-menu-lux right">
                      <button class="dropdown-item" @click.stop="$router.push(`/offres/${offer.id}`)">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                        Détails
                      </button>
                      <button class="dropdown-item" @click.stop="$router.push(`/offres/${offer.id}/edit`)">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                        Modifier
                      </button>
                      <button v-if="offer.status === 'draft'" class="dropdown-item gold-action" @click.stop="publishOffer(offer.id)">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 3v12"/><path d="M7 10l5 5 5-5"/><path d="M5 21h14"/></svg>
                        Publier
                      </button>
                      <button v-if="offer.status === 'active'" class="dropdown-item gold-action" @click.stop="copyLink(offer)">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"/><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/></svg>
                        Copier le lien
                      </button>
                      <div class="dropdown-sep"></div>
                      <button class="dropdown-item danger" @click.stop="deleteOffer(offer.id)">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6M14 11v6"/><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/></svg>
                        Supprimer
                      </button>
                    </div>
                  </transition>
                </div>
              </div>
            </div>

            <!-- Body -->
            <div class="r-item-body" style="margin-top: 20px; flex: 1;">
              <h3 class="r-card-title" style="font-size: 20px; font-weight: 800; color: var(--r-text-main); margin-bottom: 4px; line-height: 1.3;">{{ offer.title }}</h3>
              <p style="font-size: 13px; font-weight: 600; color: var(--r-text-sub); margin-bottom: 14px;">{{ offer.department || '—' }} • {{ offer.type }}</p>
              <div style="display: flex; align-items: center; gap: 8px; font-size: 13px; color: var(--r-text-sub); font-weight: 500;">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 16px; flex-shrink: 0;"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
                <span>{{ offer.location }}</span>
              </div>
            </div>

            <!-- Footer -->
            <div class="offer-card-footer">
              <div style="display: flex; flex-direction: column; gap: 2px;">
                <strong style="font-size: 28px; font-weight: 900; color: var(--r-text-main); line-height: 1;">{{ offer.applications || 0 }}</strong>
                <span style="font-size: 11px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 0.5px;">Candidats</span>
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

import PremiumSelect from '@/components/common/PremiumSelect.vue'
import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'
import { useToastStore } from '@/stores/toastStore'
import { useModalStore } from '@/stores/modalStore'

export default {
  name: 'JobOffers',
  components: { Sidebar,  PremiumSelect },
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
        { value: 'CDI', label: this.$t('createOffer.general.types.cdi') },
        { value: 'CDD', label: this.$t('createOffer.general.types.cdd') },
        { value: 'Stage', label: this.$t('createOffer.general.types.stage') },
        { value: 'Freelance', label: this.$t('createOffer.general.types.freelance') }
      ]
    },
    filteredOffers() {
      return this.offers.filter(o => {
        const matchSearch = !this.search || o.title.toLowerCase().includes(this.search.toLowerCase()) || (o.location || '').toLowerCase().includes(this.search.toLowerCase())
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
          const raw = `${o.status || o.visibility || ''}`.toLowerCase()
          let status = 'draft'
          if (raw === 'published' || raw === 'active' || raw === '1') status = 'active'
          else if (raw === 'closed' || raw === '2') status = 'closed'
          
          return {
            id: o.id,
            title: o.title,
            department: o.department || '—',
            location: o.location || 'N/A',
            type: o.type || 'FullTime',
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
        title: this.$t('offers.deleteConfirmTitle'),
        message: this.$t('offers.deleteConfirmSub'),
        confirmText: this.$t('common.delete'),
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
        useToastStore().show(this.$t('offers.published'), 'success')
      } catch (err) {
        console.error('Erreur publication:', err)
        useToastStore().show(this.$t('notifications.updateError'), 'error')
      }
    },
    copyLink(offer) {
      if (!offer.shareToken) {
        useToastStore().show(this.$t('notifications.updateError'), 'warning')
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
.dashboard-layout { background: transparent !important; }

/* COMPONENT-SPECIFIC STYLES */
.filter-card {
  position: relative;
  z-index: 10;
  flex-direction: row;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  margin-bottom: 24px;
  overflow: visible;
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-left: auto;
  margin-right: 24px;
}

.expert-filter {
  width: 180px;
}

.view-switch {
  display: flex;
  background: var(--r-main-bg);
  padding: 4px;
  border-radius: 14px;
  border: 1px solid var(--r-border);
}

.view-switch button {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  border: none;
  background: transparent;
  color: var(--r-text-sub);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.3s;
}

.view-switch button.active {
  background: var(--accent);
  color: var(--accent-contrast);
  box-shadow: 0 4px 12px var(--accent-soft);
}

/* Glass Table Adjustments */
.glass-table-container {
  background: var(--card-bg, #ffffff);
  border-radius: 20px;
  border: 1px solid var(--r-border, transparent);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.03);
}

.celestial-table {
  width: 100%;
  border-collapse: collapse;
}

.celestial-table th {
  padding: 16px 24px;
  text-align: left;
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
  color: var(--r-text-sub);
  letter-spacing: 1px;
  border-bottom: 2px solid var(--r-border);
  background: transparent;
}

.celestial-table td {
  padding: 16px 24px;
  border-bottom: 1px solid var(--r-border);
  vertical-align: middle;
}

.celestial-table tbody tr {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
}

.celestial-table tbody tr:hover {
  background: rgba(255, 255, 255, 0.03);
  transform: translateX(4px);
  box-shadow: -4px 0 0 0 var(--accent), 0 10px 25px -10px rgba(0, 0, 0, 0.3);
  z-index: 10;
}

.celestial-table tbody tr:last-child td {
  border-bottom: none;
}

/* Dropdown */
.dropdown-expert { position: relative; }
.dropdown-expert .glass-btn-sm {
  width: 32px; height: 32px; border-radius: 8px; border: 1px solid var(--r-border);
  background: var(--r-main-bg); color: var(--r-text-sub);
  display: flex; align-items: center; justify-content: center; cursor: pointer; transition: 0.2s;
}
.dropdown-expert .glass-btn-sm:hover { color: var(--r-text-main); border-color: var(--accent-color); }

.dropdown-menu-lux {
  position: absolute; top: calc(100% + 4px); right: 0;
  background: var(--r-card-bg); border: 1px solid var(--r-border);
  border-radius: 12px; box-shadow: var(--r-shadow-hover);
  padding: 8px; z-index: 50; min-width: 180px;
}
.dropdown-menu-lux.left { right: auto; left: 0; }

.dropdown-item {
  width: 100%; padding: 10px 12px; border: none; background: none;
  border-radius: 8px; display: flex; align-items: center; gap: 10px;
  font-size: 13px; font-weight: 600; color: var(--r-text-main);
  cursor: pointer; transition: 0.2s;
}
.dropdown-item:hover { background: var(--r-main-bg); color: var(--accent-color); }
.dropdown-item.gold-action {
  background: var(--accent-soft, rgba(251,191,36,0.12));
  color: var(--accent-color, #fbbf24);
  font-weight: 700;
}
.dropdown-item.gold-action:hover {
  background: var(--accent-color, #fbbf24);
  color: #fff;
}
.dropdown-item.danger:hover { color: #ef4444; background: rgba(239, 68, 68, 0.1); }
.dropdown-item svg { width: 14px; color: var(--r-text-sub); }
.dropdown-item.gold-action svg { color: inherit; }
.dropdown-item:hover svg { color: inherit; }
.dropdown-sep { height: 1px; background: var(--r-border); margin: 6px 0; }

.offer-cell { display: flex; align-items: center; gap: 16px; }
.offer-icon {
  width: 44px; height: 44px; border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  color: var(--accent-color); background: var(--accent-soft);
}
.offer-title { font-size: 15px; font-weight: 800; color: var(--r-text-main); }
.offer-dept { font-size: 12px; font-weight: 600; color: var(--r-text-sub); }

.card-actions { display: flex; align-items: center; gap: 12px; }

/* Offer Card (Grid View) */
.offer-card {
  padding: 24px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  gap: 0;
  transition: all 0.4s cubic-bezier(0.2, 0.8, 0.2, 1);
  border: 1px solid var(--r-border, transparent);
  background: var(--card-bg, #ffffff);
  border-radius: 20px;
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.03);
}

.offer-card:hover { 
  transform: translateY(-5px);
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.06);
  z-index: 15;
}

.offer-card-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
}

.offer-icon-wrapper {
  width: 54px; height: 54px; border-radius: 14px;
  background: var(--accent-soft); color: var(--accent-color);
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}

.offer-card-badges {
  display: flex;
  align-items: center;
  gap: 10px;
}

.offer-card-footer {
  margin-top: 24px;
  padding-top: 20px;
  border-top: 1px solid var(--r-border);
}

@media (max-width: 1024px) {
  .filter-card { flex-direction: column; align-items: stretch; gap: 16px; }
  .filter-group { margin: 0; justify-content: space-between; }
  .r-search { max-width: none; }
}
</style>
