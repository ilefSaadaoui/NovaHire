import { defineStore } from 'pinia'
import api from '@/api/axios'
import { useToastStore } from './toastStore'

export const useCompanyStore = defineStore('company', {
  state: () => ({
    stats: {
      activeJobOffers: 0,
      totalApplications: 0,
      aiAnalysesCount: 0,
      plannedInterviews: 0,
      recentOffers: [],
      recentActivities: [],
      monthlyApplications: [],
      teamStats: []
    },
    branding: {
      companyName: '',
      logoUrl: '',
      primaryColor: '#0ea5e9', // Celestial Sky Blue
      secondaryColor: '#fcd34d', // Premium Gold
      description: '',
      industry: '',
      website: ''
    },
    team: [],
    departments: [],
    loading: false
  }),

  actions: {
    async fetchStats() {
      this.loading = true
      try {
        const res = await api.get('/companyadmin/stats')
        this.stats = res.data
      } catch (error) {
        console.error('Error fetching admin stats:', error)
      } finally {
        this.loading = false
      }
    },

    async fetchBranding() {
      try {
        const res = await api.get('/companyadmin/branding')
        this.branding = res.data
        this.applyBrandingToCSS()
      } catch (error) {
        console.error('Error fetching branding:', error)
      }
    },

    async updateBranding(payload) {
      try {
        await api.put('/companyadmin/branding', payload)
        this.branding = { ...this.branding, ...payload }
        this.applyBrandingToCSS()
      } catch (error) {
        useToastStore().show('Erreur lors de la mise à jour', 'error')
      }
    },

    async fetchTeam() {
      try {
        const res = await api.get('/companyadmin/team')
        this.team = res.data.map(u => ({
          ...u,
          fullName: `${u.firstName} ${u.lastName}`,
          initials: (u.firstName[0] + u.lastName[0]).toUpperCase()
        }))
      } catch (error) {
        console.error('Error fetching team:', error)
      }
    },

    async removeTeamMember(id) {
      try {
        await api.delete(`/companyadmin/team/${id}`)
        this.team = this.team.filter(m => m.id !== id)
        useToastStore().show('Membre supprimé', 'success')
      } catch (error) {
        console.error('Error removing member:', error)
      }
    },

    async updateMemberRole(memberId, role) {
      try {
        await api.patch(`/companyadmin/team/${memberId}/role`, { role })
        const index = this.team.findIndex(m => m.id === memberId)
        if (index !== -1) {
          this.team[index].role = role
        }
      } catch (error) {
        console.error('Error updating role:', error)
        throw error
      }
    },
    
    async inviteMember(payload) {
      try {
        const res = await api.post('/recruiter/invite', payload)
        return res.data
      } catch (error) {
        console.error('Error inviting member:', error)
        throw error
      }
    },

    async updateMemberDetails(memberId, payload) {
      try {
        await api.patch(`/companyadmin/team/${memberId}`, payload)
        const index = this.team.findIndex(m => m.id === memberId)
        if (index !== -1) {
          const u = { 
            ...this.team[index], 
            firstName: payload.firstName, 
            lastName: payload.lastName,
            email: payload.email,
            isActive: payload.isActive,
            departmentId: payload.departmentId 
          }
          u.fullName = `${payload.firstName} ${payload.lastName}`
          if (payload.firstName && payload.lastName) {
              u.initials = (payload.firstName[0] + payload.lastName[0]).toUpperCase()
          }
          this.team[index] = u
        }
      } catch (error) {
        console.error('Error updating member details:', error)
        throw error
      }
    },

    async bulkUpdateMembersStatus(memberIds, isActive) {
      this.loading = true
      try {
        // Since there is no bulk endpoint, we process in parallel
        await Promise.all(memberIds.map(id => {
          const m = this.team.find(user => user.id === id)
          if (!m) return Promise.resolve()
          return api.patch(`/companyadmin/team/${id}`, { ...m, isActive })
        }))
        
        // Update local state
        memberIds.forEach(id => {
          const index = this.team.findIndex(m => m.id === id)
          if (index !== -1) {
            this.team[index].isActive = isActive
          }
        })
        useToastStore().show(`${memberIds.length} membre(s) mis à jour`, 'success')
      } catch (error) {
        console.error('Error in bulk update:', error)
        useToastStore().show('Erreur lors de la mise à jour groupée', 'error')
      } finally {
        this.loading = false
      }
    },

    async bulkRemoveMembers(memberIds) {
      this.loading = true
      try {
        await Promise.all(memberIds.map(id => api.delete(`/companyadmin/team/${id}`)))
        this.team = this.team.filter(m => !memberIds.includes(m.id))
        useToastStore().show(`${memberIds.length} membre(s) retirés`, 'success')
      } catch (error) {
        console.error('Error in bulk removal:', error)
        useToastStore().show('Erreur lors de la suppression groupée', 'error')
      } finally {
        this.loading = false
      }
    },

    async resendInvitation(member) {
      try {
        // Calling same invite endpoint re-triggers the token and email
        await api.post('/recruiter/invite', {
          email: member.email,
          firstName: member.firstName,
          lastName: member.lastName,
          role: member.role || 'Recruiter',
          jobTitle: member.jobTitle || '',
          departmentId: member.departmentId || null
        })
        useToastStore().show(`Invitation renvoyée à ${member.email}`, 'success')
      } catch (error) {
        console.error('Error resending invite:', error)
        useToastStore().show('Échec du renvoi de l\'invitation', 'error')
      }
    },



    async fetchDepartments() {
      try {
        const res = await api.get('/department')
        this.departments = res.data
      } catch (error) {
        console.error('Error fetching departments:', error)
      }
    },

    async createDepartment(payload) {
      try {
        const res = await api.post('/department', payload)
        this.departments.push({
          id: res.data.id,
          name: payload.name,
          description: payload.description,
          membersCount: 0,
          createdAt: new Date().toISOString()
        })
        useToastStore().show('Département créé', 'success')
        return res.data
      } catch (error) {
        useToastStore().show(error.response?.data?.message || 'Erreur lors de la création', 'error')
        throw error
      }
    },

    async updateDepartment(id, payload) {
      try {
        await api.put(`/department/${id}`, payload)
        const index = this.departments.findIndex(d => d.id === id)
        if (index !== -1) {
          this.departments[index].name = payload.name
          this.departments[index].description = payload.description
        }
        useToastStore().show('Département mis à jour', 'success')
      } catch (error) {
        useToastStore().show(error.response?.data?.message || 'Erreur lors de la mise à jour', 'error')
        throw error
      }
    },

    async deleteDepartment(id) {
      try {
        await api.delete(`/department/${id}`)
        this.departments = this.departments.filter(d => d.id !== id)
        useToastStore().show('Département supprimé', 'success')
      } catch (error) {
        useToastStore().show(error.response?.data?.message || 'Erreur lors de la suppression', 'error')
        throw error
      }
    },

    applyBrandingToCSS() {
      if (this.branding.primaryColor) {
        document.documentElement.style.setProperty('--brand-primary', this.branding.primaryColor)
        const soft = this.branding.primaryColor + '15'
        document.documentElement.style.setProperty('--brand-primary-soft', soft)
      }
      if (this.branding.secondaryColor) {
        document.documentElement.style.setProperty('--brand-secondary', this.branding.secondaryColor)
      }
    }
  }
})
