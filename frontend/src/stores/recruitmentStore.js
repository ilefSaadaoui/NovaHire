import { defineStore } from 'pinia'
import api from '@/api/axios'

export const useRecruitmentStore = defineStore('recruitment', {
  state: () => ({
    offers: [],
    applications: [],
    selectedOfferId: null,
    loading: false,
    lastFetched: null,
    pipelineCache: {} // Cache per offer ID: { offerId: { applications: [], lastFetched: Date } }
  }),

  getters: {
    getOffers: (state) => state.offers,
    getApplications: (state) => state.applications,
    
    // Get distinct stages from applications for Kanban
    pipelineStages: () => [
      'Identification',
      'Présélection',
      'Entretien',
      'Évaluation',
      'Décision',
      'Offre',
      'Engagé'
    ]
  },

  actions: {
    async fetchOffers(force = false) {
      if (!force && this.offers.length > 0) return
      
      this.loading = true
      try {
        const res = await api.get('/JobOffer', { params: { page: 1, limit: 200 } })
        const raw = res.data.data || []
        const seenIds = new Set()
        this.offers = raw.filter(o => o?.id && !seenIds.has(o.id) && seenIds.add(o.id))
      } catch (err) {
        console.error('Error fetching offers:', err)
      } finally {
        this.loading = false
      }
    },

    async fetchApplications(offerId, force = false) {
      this.selectedOfferId = offerId
      
      // Check cache
      const cache = this.pipelineCache[offerId]
      const isExpired = cache ? (new Date() - cache.lastFetched > 300000) : true // 5 min cache
      
      if (!force && cache && !isExpired) {
        this.applications = cache.applications
        return
      }

      this.loading = true
      try {
        const url = offerId ? `/Recruiter/applications?jobOfferId=${offerId}` : '/Recruiter/applications'
        const res = await api.get(url)
        const apps = res.data || []
        
        this.applications = apps
        
        // Update cache
        if (offerId) {
          this.pipelineCache[offerId] = {
            applications: apps,
            lastFetched: new Date()
          }
        }
      } catch (err) {
        console.error('Error fetching applications:', err)
      } finally {
        this.loading = false
      }
    },

    setApplications(apps) {
      this.applications = apps
      if (this.selectedOfferId) {
        this.pipelineCache[this.selectedOfferId] = {
          applications: apps,
          lastFetched: new Date()
        }
      }
    },

    updateApplicationStage(applicationId, newStage) {
      const app = this.applications.find(a => a.id === applicationId)
      if (app) {
        app.status = newStage
        // Update cache too
        if (this.selectedOfferId && this.pipelineCache[this.selectedOfferId]) {
          this.pipelineCache[this.selectedOfferId].applications = [...this.applications]
        }
      }
    }
  }
})
