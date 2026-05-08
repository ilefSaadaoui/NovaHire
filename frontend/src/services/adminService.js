import axios from 'axios';

// Ensure the base URL matches your backend environment setting.
// Backend runs on port 5000 (see launchSettings.json).
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

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
        return axios.get(`${API_URL}/admin/summary`, getAuthHeaders());
    },

    // ---- Users Endpoints ----
    getUsers(pageNumber = 1, pageSize = 50, role = null, companyId = null, search = null) {
        let qs = `?pageNumber=${pageNumber}&pageSize=${pageSize}`;
        if (role != null) qs += `&role=${role}`;
        if (companyId) qs += `&companyId=${companyId}`;
        if (search) qs += `&search=${search}`;

        return axios.get(`${API_URL}/admin/users${qs}`, getAuthHeaders());
    },
    getUserById(id) {
        return axios.get(`${API_URL}/admin/users/${id}`, getAuthHeaders());
    },
    createUser(data) {
        return axios.post(`${API_URL}/admin/users`, data, getAuthHeaders());
    },
    updateUser(id, data) {
        return axios.put(`${API_URL}/admin/users/${id}`, data, getAuthHeaders());
    },
    deleteUser(id) {
        return axios.delete(`${API_URL}/admin/users/${id}`, getAuthHeaders());
    },

    // ---- Companies Endpoints ----
    getCompanies(pageNumber = 1, pageSize = 50) {
        return axios.get(`${API_URL}/admin/companies?pageNumber=${pageNumber}&pageSize=${pageSize}`, getAuthHeaders());
    },
    getCompanyById(id) {
        return axios.get(`${API_URL}/admin/companies/${id}`, getAuthHeaders());
    },
    createCompany(data) {
        return axios.post(`${API_URL}/admin/companies`, data, getAuthHeaders());
    },
    updateCompany(id, data) {
        return axios.put(`${API_URL}/admin/companies/${id}`, data, getAuthHeaders());
    },
    deleteCompany(id) {
        return axios.delete(`${API_URL}/admin/companies/${id}`, getAuthHeaders());
    },
    getPendingCompanies() {
        return axios.get(`${API_URL}/admin/companies/pending`, getAuthHeaders());
    },
    approveCompany(id) {
        return axios.post(`${API_URL}/admin/companies/${id}/approve`, {}, getAuthHeaders());
    },
    rejectCompany(id) {
        return axios.post(`${API_URL}/admin/companies/${id}/reject`, {}, getAuthHeaders());
    },

    // ---- Job Applications Endpoints ----
    getJobApplications(pageNumber = 1, pageSize = 200) {
        return axios.get(`${API_URL}/admin/jobapplications?pageNumber=${pageNumber}&pageSize=${pageSize}`, getAuthHeaders());
    },

    // ---- Candidates Endpoints ----
    getCandidateById(id) {
        return axios.get(`${API_URL}/admin/candidates/${id}`, getAuthHeaders());
    },
    updateCandidate(id, data) {
        return axios.put(`${API_URL}/admin/candidates/${id}`, data, getAuthHeaders());
    },
    deleteCandidate(id) {
        return axios.delete(`${API_URL}/admin/candidates/${id}`, getAuthHeaders());
    },

    // ---- Activity Logs Endpoints ----
    getLogs(limit = 100) {
        return axios.get(`${API_URL}/admin/logs?limit=${limit}`, getAuthHeaders());
    },

    // ---- Contact Messages Endpoints ----
    getContactMessages() {
        return axios.get(`${API_URL}/admin/contact-messages`, getAuthHeaders());
    },
    updateContactMessageStatus(id, status) {
        return axios.put(`${API_URL}/admin/contact-messages/${id}/status`, { status }, getAuthHeaders());
    },

    // ---- Profile Endpoints ----
    getCurrentProfile() {
        return axios.get(`${API_URL}/admin/profile`, getAuthHeaders());
    },
    updateProfile(data) {
        return axios.put(`${API_URL}/admin/profile`, data, getAuthHeaders());
    },
    getHealth() {
        return axios.get(`${API_URL}/admin/health`, getAuthHeaders());
    }
};

export default adminService;
