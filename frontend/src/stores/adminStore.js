import { defineStore } from 'pinia';
import adminService from '@/services/adminService';

export const useAdminStore = defineStore('admin', {
    state: () => ({
        loading: false,
        error: null,
        success: null,

        // Summary
        summary: {
            totalUsers: 0,
            totalCandidates: 0,
            totalCompanies: 0,
            totalJobOffers: 0,
            totalJobApplications: 0,
            activeUsers: 0,
            activeCompanies: 0,
            roleDistribution: {
                SuperAdmin: 0,
                CompanyAdmin: 0,
                Recruiter: 0,
                Candidate: 0
            }
        },
        health: {
            api: { status: 'Stable', message: 'Online' },
            database: { status: 'Stable', message: 'Optimale' },
            storage: { status: 'Stable', message: '98% Libre' },
            ai: { status: 'Stable', message: 'Actif' }
        },

        // Data arrays
        users: [],
        usersTotal: 0,
        companies: [],
        companiesTotal: 0,
        jobApplications: [],
        logs: [],
        contactMessages: [],
        currentUser: null,
        selectedCandidate: null,

        // Filtering
        usersPageNumber: 1,
        usersPageSize: 10,
        companiesPageNumber: 1,
        companiesPageSize: 10
    }),

    getters: {
        // New Getters for Monitoring
        roleDistribution(state) {
            return state.summary.roleDistribution;
        },

        activeCompaniesCount(state) {
            return state.summary.activeCompanies;
        },

        activeUsersCount(state) {
            return state.summary.activeUsers;
        }
    },

    actions: {
        async fetchAll() {
            this.loading = true;
            try {
                // Fetch all fundamental admin data at once
                await Promise.allSettled([
                    this.fetchSummary(),
                    this.fetchCompanies(1, 50),
                    this.fetchUsers(null, null, null, 1, 50),
                    this.fetchJobApplications(1, 100),
                    this.fetchLogs(50),
                    this.fetchContactMessages(),
                    this.fetchCurrentProfile(),
                    this.fetchHealth()
                ]);
            } finally {
                this.loading = false;
            }
        },

        async fetchCandidateWithApplications(candidateId) {
            this.loading = true;
            this.error = null;
            try {
                const response = await adminService.getCandidateById(candidateId);
                // Backend returns candidate fields + applications at root level
                const { applications, ...candidate } = response.data;
                this.selectedCandidate = candidate;
                return { candidate, applications: applications || [] };
            } catch (error) {
                this.error = error.response?.data?.message || 'Error fetching candidate details';
                return null;
            } finally {
                this.loading = false;
            }
        },

        async fetchSummary() {
            try {
                const response = await adminService.getSummary();
                this.summary = response.data;
                this.error = null;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error fetching summary';
            }
        },

        async fetchCompanies(pageNumber = 1, pageSize = 50) {
            this.loading = true;
            this.error = null;
            try {
                const response = await adminService.getCompanies(pageNumber, pageSize);
                const companiesData = response.data.data || response.data;
                this.companies = Array.isArray(companiesData) ? companiesData : [companiesData];
                this.companiesTotal = response.data.total || this.companies.length;
                this.companiesPageNumber = pageNumber;
                this.companiesPageSize = pageSize;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error fetching companies';
            } finally {
                this.loading = false;
            }
        },

        async fetchUsers(role = null, companyId = null, search = null, pageNumber = 1, pageSize = 50) {
            this.loading = true;
            this.error = null;
            try {
                const response = await adminService.getUsers(pageNumber, pageSize, role, companyId, search);
                this.users = response.data.data || response.data;
                this.usersTotal = response.data.total || this.users.length;
                this.usersPageNumber = pageNumber;
                this.usersPageSize = pageSize;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error fetching users';
            } finally {
                this.loading = false;
            }
        },


        async fetchJobApplications(pageNumber = 1, pageSize = 200) {
            this.loading = true;
            this.error = null;
            try {
                const response = await adminService.getJobApplications(pageNumber, pageSize);
                this.jobApplications = response.data?.data || response.data || [];
            } catch (error) {
                this.error = error.response?.data?.message || 'Error fetching job applications';
                this.jobApplications = [];
            } finally {
                this.loading = false;
            }
        },

        async fetchCandidateById(candidateId) {
            this.loading = true;
            this.error = null;
            try {
                const response = await adminService.getCandidateById(candidateId);
                this.selectedCandidate = response.data;
                return response.data;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error fetching candidate details';
                return null;
            } finally {
                this.loading = false;
            }
        },

        async updateCandidate(candidateId, candidateData) {
            this.loading = true;
            this.error = null;
            this.success = null;
            try {
                await adminService.updateCandidate(candidateId, candidateData);
                this.success = 'Candidate updated successfully';
                await this.fetchJobApplications();
                return true;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error updating candidate';
                return false;
            } finally {
                this.loading = false;
            }
        },

        async deleteCandidateApplication(candidateId) {
            this.loading = true;
            this.error = null;
            this.success = null;
            try {
                await adminService.deleteCandidate(candidateId);
                this.success = 'Candidate deleted successfully';
                await this.fetchJobApplications();
                await this.fetchSummary();
                return true;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error deleting candidate';
                return false;
            } finally {
                this.loading = false;
            }
        },

        async fetchLogs(limit = 100) {
            this.loading = true;
            try {
                const response = await adminService.getLogs(limit);
                this.logs = Array.isArray(response.data) ? response.data : (response.data ? [response.data] : []);
            } catch (error) {
                // Silently fail for logs - don't set global error
                console.error('Failed to fetch logs:', error.message);
                this.logs = [];
            } finally {
                this.loading = false;
            }
        },

        async fetchContactMessages() {
            try {
                const response = await adminService.getContactMessages();
                this.contactMessages = response.data || [];
            } catch (error) {
                console.error('Failed to fetch contact messages:', error.message);
                this.contactMessages = [];
            }
        },

        // User CRUD operations
        async createUser(userData) {
            this.loading = true;
            this.error = null;
            this.success = null;
            try {
                await adminService.createUser(userData);
                this.success = 'User created successfully';
                await this.fetchUsers();
                await this.fetchSummary();
                return true;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error creating user';
                return false;
            } finally {
                this.loading = false;
            }
        },

        async updateUser(userId, userData) {
            this.loading = true;
            this.error = null;
            this.success = null;
            try {
                await adminService.updateUser(userId, userData);
                this.success = 'User updated successfully';
                await this.fetchUsers();
                return true;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error updating user';
                return false;
            } finally {
                this.loading = false;
            }
        },

        async deleteUser(userId) {
            this.loading = true;
            this.error = null;
            this.success = null;
            try {
                await adminService.deleteUser(userId);
                this.success = 'User deleted successfully';
                await this.fetchUsers();
                await this.fetchSummary();
                return true;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error deleting user';
                return false;
            } finally {
                this.loading = false;
            }
        },

        // Company CRUD operations
        async createCompany(companyData) {
            this.loading = true;
            this.error = null;
            this.success = null;
            try {
                await adminService.createCompany(companyData);
                this.success = 'Company created successfully';
                await this.fetchCompanies();
                await this.fetchSummary();
                return true;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error creating company';
                return false;
            } finally {
                this.loading = false;
            }
        },

        async updateCompany(companyId, companyData) {
            this.loading = true;
            this.error = null;
            this.success = null;
            try {
                await adminService.updateCompany(companyId, companyData);
                this.success = 'Company updated successfully';
                await this.fetchCompanies();
                await this.fetchSummary();
                return true;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error updating company';
                return false;
            } finally {
                this.loading = false;
            }
        },

        async deleteCompany(companyId) {
            this.loading = true;
            this.error = null;
            this.success = null;
            try {
                await adminService.deleteCompany(companyId);
                this.success = 'Company deleted successfully';
                await this.fetchCompanies();
                await this.fetchSummary();
                return true;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error deleting company';
                return false;
            } finally {
                this.loading = false;
            }
        },


        // Profile operations
        async fetchCurrentProfile() {
            this.loading = true;
            this.error = null;
            try {
                const response = await adminService.getCurrentProfile();
                this.currentUser = response.data;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error fetching profile';
            } finally {
                this.loading = false;
            }
        },

        async saveProfileUpdates(data) {
            this.loading = true;
            this.error = null;
            this.success = null;
            try {
                await adminService.updateProfile(data);
                this.success = 'Profile updated successfully';
                await this.fetchCurrentProfile();
                return true;
            } catch (error) {
                this.error = error.response?.data?.message || 'Error updating profile';
                return false;
            } finally {
                this.loading = false;
            }
        },

        async fetchHealth() {
            try {
                const response = await adminService.getHealth();
                this.health = response.data;
            } catch (error) {
                console.error('Failed to fetch system health:', error.message);
            }
        },

        // Clear messages
        clearMessages() {
            this.error = null;
            this.success = null;
        }
    }
});
