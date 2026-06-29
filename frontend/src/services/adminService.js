import api from '@/api/axios';

// The centralized axios instance has baseURL: '/api' (see @/api/axios.js).
// All paths here must be RELATIVE to that base (e.g., '/admin/summary')
// and must NOT include '/api' again to avoid the double-prefix bug (/api/api/...).

const getAuthHeaders = () => {
    // Token is stored as 'authToken', not 'token'.
    // It may be in sessionStorage (default) or localStorage (rememberMe=true).
    const token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken');
    return {
        headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    };
};

export const adminService = {
    // ---- Dashboard General Metrics ----
    getSummary() {
        return api.get('/admin/summary', getAuthHeaders());
    },

    // ---- Users Endpoints ----
    getUsers(pageNumber = 1, pageSize = 50, role = null, companyId = null, search = null) {
        let qs = `?pageNumber=${pageNumber}&pageSize=${pageSize}`;
        if (role != null) qs += `&role=${role}`;
        if (companyId) qs += `&companyId=${companyId}`;
        if (search) qs += `&search=${search}`;

        return api.get(`/admin/users${qs}`, getAuthHeaders());
    },
    getUserById(id) {
        return api.get(`/admin/users/${id}`, getAuthHeaders());
    },
    createUser(data) {
        return api.post('/admin/users', data, getAuthHeaders());
    },
    updateUser(id, data) {
        return api.put(`/admin/users/${id}`, data, getAuthHeaders());
    },
    deleteUser(id) {
        return api.delete(`/admin/users/${id}`, getAuthHeaders());
    },

    // ---- Companies Endpoints ----
    getCompanies(pageNumber = 1, pageSize = 50) {
        return api.get(`/admin/companies?pageNumber=${pageNumber}&pageSize=${pageSize}`, getAuthHeaders());
    },
    getCompanyById(id) {
        return api.get(`/admin/companies/${id}`, getAuthHeaders());
    },
    createCompany(data) {
        return api.post('/admin/companies', data, getAuthHeaders());
    },
    updateCompany(id, data) {
        return api.put(`/admin/companies/${id}`, data, getAuthHeaders());
    },
    deleteCompany(id) {
        return api.delete(`/admin/companies/${id}`, getAuthHeaders());
    },
    getPendingCompanies() {
        return api.get('/admin/companies/pending', getAuthHeaders());
    },
    approveCompany(id) {
        return api.post(`/admin/companies/${id}/approve`, {}, getAuthHeaders());
    },
    rejectCompany(id) {
        return api.post(`/admin/companies/${id}/reject`, {}, getAuthHeaders());
    },

    // ---- Job Applications Endpoints ----
    getJobApplications(pageNumber = 1, pageSize = 200) {
        return api.get(`/admin/jobapplications?pageNumber=${pageNumber}&pageSize=${pageSize}`, getAuthHeaders());
    },

    // ---- Candidates Endpoints ----
    getCandidateById(id) {
        return api.get(`/admin/candidates/${id}`, getAuthHeaders());
    },
    updateCandidate(id, data) {
        return api.put(`/admin/candidates/${id}`, data, getAuthHeaders());
    },
    deleteCandidate(id) {
        return api.delete(`/admin/candidates/${id}`, getAuthHeaders());
    },

    // ---- Activity Logs Endpoints ----
    getLogs(limit = 100) {
        return api.get(`/admin/logs?limit=${limit}`, getAuthHeaders());
    },

    // ---- Contact Messages Endpoints ----
    getContactMessages() {
        return api.get('/admin/contact-messages', getAuthHeaders());
    },
    updateContactMessageStatus(id, status) {
        return api.put(`/admin/contact-messages/${id}/status`, { status }, getAuthHeaders());
    },

    // ---- Profile Endpoints ----
    getCurrentProfile() {
        return api.get('/admin/profile', getAuthHeaders());
    },
    updateProfile(data) {
        return api.put('/admin/profile', data, getAuthHeaders());
    },
    getHealth() {
        return api.get('/admin/health', getAuthHeaders());
    }
};

export default adminService;
