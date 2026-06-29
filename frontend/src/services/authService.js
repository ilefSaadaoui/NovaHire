/**
 * Authentication Service
 * Handles all API calls related to authentication
 */

// Always use the relative /api path so requests go through the Vite dev proxy
// (configured in vite.config.js → target: https://localhost:7075)
// Using an absolute URL like http://localhost:5000 bypasses the proxy and causes CORS errors.
const API_BASE_URL =
  (typeof import.meta !== 'undefined' && import.meta.env && import.meta.env.VITE_API_URL) ||
  '/api'

const DEFAULT_REQUEST_TIMEOUT_MS = 10000
const REGISTER_COMPANY_TIMEOUT_MS = 60000

const fetchWithTimeout = async (resource, options = {}) => {
  const { timeout = DEFAULT_REQUEST_TIMEOUT_MS } = options;
  
  const controller = new AbortController();
  const id = setTimeout(() => controller.abort(), timeout);
  
  try {
    const response = await fetch(resource, {
      ...options,
      signal: controller.signal
    });
    return response;
  } catch (error) {
    if (error.name === 'AbortError') {
      throw new Error('La requête a expiré. Veuillez vérifier votre connexion ou réessayer plus tard.');
    }
    throw error;
  } finally {
    clearTimeout(id);
  }
};

const readErrorMessage = async (response, fallbackMessage) => {
  if (response.ok) {
    return ''
  }

  let message = fallbackMessage
  try {
    const payload = await response.json()
    
    // Extract precise validation errors if they exist (ASP.NET Core style)
    if (payload?.errors && typeof payload.errors === 'object') {
      const errorMessages = []
      for (const [field, messages] of Object.entries(payload.errors)) {
        if (Array.isArray(messages)) {
          errorMessages.push(`${field}: ${messages.join(', ')}`)
        }
      }
      if (errorMessages.length > 0) {
        return errorMessages.join(' | ')
      }
    }
    
    message = payload?.message || payload?.title || message
  } catch (_) {
    // keep fallback message
  }

  return message
}

const authService = {
  /**
   * Register a new company and its admin
   * @param {Object} registrationData - Complete registration data
   * @returns {Promise} Response from API
   */
  registerCompany(registrationData) {
    return fetchWithTimeout(`${API_BASE_URL}/auth/register-company`, {
      timeout: REGISTER_COMPANY_TIMEOUT_MS,
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        companyName: registrationData.companyName,
        industry: registrationData.industry,
        subscriptionPlan: registrationData.subscriptionPlan,
        employeesRange: registrationData.employeesRange,
        adminFirstName: registrationData.adminFirstName,
        adminLastName: registrationData.adminLastName,
        adminEmail: registrationData.adminEmail,
        adminPassword: registrationData.adminPassword,
        recruiterEmail: registrationData.recruiterEmail
      })
    }).then(async response => {
      if (!response.ok) {
        const error = await readErrorMessage(response, 'Company registration failed')
        throw new Error(error)
      }
      return response.json()
    }).then(data => {
      const token = data.token || data.accessToken
      if (token) {
        localStorage.setItem('authToken', token)
        if (data.role) {
          localStorage.setItem('userRole', data.role.toLowerCase())
        }
      }
      return data
    })
  },

  /**
   * Login user (Unified)
   * @param {string} email - User email
   * @param {string} password - User password
   * @returns {Promise} Response from API with token and role
   */
  async login(email, password, rememberMe = false) {
    const response = await fetchWithTimeout(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ email: email.trim().toLowerCase(), password: password.trim(), rememberMe })
    })

    if (!response.ok) {
      const message = await readErrorMessage(
        response,
        response.status === 401
          ? 'Identifiants incorrects. Veuillez réessayer.'
          : 'Connexion impossible. Veuillez réessayer.'
      )
      throw new Error(message)
    }

    const data = await response.json()
    const token = data.token || data.accessToken
    if (token) {
      const storage = rememberMe ? localStorage : sessionStorage
      const secondaryStorage = rememberMe ? sessionStorage : localStorage

      secondaryStorage.removeItem('authToken')
      secondaryStorage.removeItem('refreshToken')
      secondaryStorage.removeItem('tokenExpiration')
      secondaryStorage.removeItem('userRole')
      secondaryStorage.removeItem('companyName')

      storage.setItem('authToken', token)
      if (data.role) {
        storage.setItem('userRole', data.role.toLowerCase())
      }
      if (data.companyName) {
        storage.setItem('companyName', data.companyName)
      }
      if (data.tokenExpiration) {
        storage.setItem('tokenExpiration', data.tokenExpiration)
      }
      if (data.refreshToken) {
        storage.setItem('refreshToken', data.refreshToken)
      }
    }
    return data
  },

  /**
   * Login SuperAdmin user with MFA code
   * @param {Object} payload - Login payload
   * @param {string} payload.email - SuperAdmin email
   * @param {string} payload.password - SuperAdmin password
   * @param {string} payload.otpCode - 2FA code (frontend validation)
   * @param {boolean} payload.rememberMe - Persist session across browser restart
   * @returns {Promise<Object>} API auth response
   */
  async loginSuperAdmin(payload) {
    const response = await fetchWithTimeout(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        email: payload.email,
        password: payload.password,
        rememberMe: !!payload.rememberMe
      })
    })

    if (!response.ok) {
      let serverMessage = 'SuperAdmin login failed'
      try {
        const errorPayload = await response.json()
        serverMessage = errorPayload?.message || errorPayload?.title || serverMessage
      } catch (_) {
        // ignore json parse errors and keep fallback message
      }
      throw new Error(serverMessage)
    }

    const data = await response.json()

    const storage = payload.rememberMe ? localStorage : sessionStorage
    const secondaryStorage = payload.rememberMe ? sessionStorage : localStorage

    secondaryStorage.removeItem('authToken')
    secondaryStorage.removeItem('refreshToken')
    secondaryStorage.removeItem('tokenExpiration')
    secondaryStorage.removeItem('userRole')

    storage.setItem('userRole', 'superadmin')
    storage.setItem('superAdminEmail', payload.email)
    if (data.accessToken || data.token) {
      storage.setItem('authToken', data.accessToken || data.token)
    }
    if (data.refreshToken) {
      storage.setItem('refreshToken', data.refreshToken)
    }
    if (data.tokenExpiration) {
      storage.setItem('tokenExpiration', data.tokenExpiration)
    }

    return data
  },

  getAuthHeaders() {
    const token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken')
    const headers = {
      'Content-Type': 'application/json'
    }

    if (token) {
      headers.Authorization = `Bearer ${token}`
    }

    return headers
  },

  async listSuperAdmins() {
    const response = await fetch(`${API_BASE_URL}/admin/users?pageNumber=1&pageSize=300`, {
      method: 'GET',
      headers: this.getAuthHeaders()
    })

    if (!response.ok) {
      const message = await readErrorMessage(response, 'Failed to fetch super admins')
      throw new Error(message)
    }

    const payload = await response.json()
    const collection = Array.isArray(payload) ? payload : Array.isArray(payload?.data) ? payload.data : []

    const normalizeRole = (value) => {
      if (typeof value === 'number') {
        return value === 0 ? 'superadmin' : ''
      }
      return String(value || '').toLowerCase()
    }

    return collection.filter((user) => normalizeRole(user?.userRole ?? user?.role) === 'superadmin')
  },

  async getSuperAdminProfile() {
    const superAdmins = await this.listSuperAdmins()
    const targetEmail =
      (localStorage.getItem('superAdminEmail') || sessionStorage.getItem('superAdminEmail') || '').toLowerCase()

    if (!superAdmins.length) {
      return null
    }

    if (!targetEmail) {
      return superAdmins[0]
    }

    return (
      superAdmins.find((item) => String(item?.email || '').toLowerCase() === targetEmail) || superAdmins[0]
    )
  },

  async updateSuperAdminProfile(profileData) {
    const response = await fetch(`${API_BASE_URL}/admin/users/${profileData.id}`, {
      method: 'PUT',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({
        firstName: profileData.firstName,
        lastName: profileData.lastName,
        email: profileData.email,
        role: 0,
        companyId: null,
        isActive: true
      })
    })

    if (!response.ok) {
      const message = await readErrorMessage(response, 'Failed to update super admin profile')
      throw new Error(message)
    }

    return response.json()
  },

  async createSuperAdmin(profileData) {
    const response = await fetch(`${API_BASE_URL}/admin/users`, {
      method: 'POST',
      headers: this.getAuthHeaders(),
      body: JSON.stringify({
        firstName: profileData.firstName,
        lastName: profileData.lastName,
        email: profileData.email,
        password: profileData.password,
        role: 0,
        companyId: null,
        isActive: true
      })
    })

    if (!response.ok) {
      const message = await readErrorMessage(response, 'Failed to create super admin')
      throw new Error(message)
    }

    return response.json()
  },

  async changeSuperAdminPassword(payload) {
    const attempts = [
      {
        url: `${API_BASE_URL}/auth/change-password`,
        body: {
          currentPassword: payload.currentPassword,
          newPassword: payload.newPassword,
          confirmPassword: payload.confirmPassword
        }
      },
      {
        url: `${API_BASE_URL}/auth/change-password/superadmin`,
        body: {
          currentPassword: payload.currentPassword,
          newPassword: payload.newPassword,
          confirmPassword: payload.confirmPassword
        }
      },
      {
        url: `${API_BASE_URL}/admin/users/${payload.userId}/password`,
        body: {
          currentPassword: payload.currentPassword,
          newPassword: payload.newPassword,
          password: payload.newPassword
        }
      }
    ]

    let lastMessage = 'Failed to change super admin password'

    for (const attempt of attempts) {
      try {
        const response = await fetch(attempt.url, {
          method: 'POST',
          headers: this.getAuthHeaders(),
          body: JSON.stringify(attempt.body)
        })

        if (response.ok) {
          try {
            return await response.json()
          } catch (_) {
            return { success: true }
          }
        }

        lastMessage = await readErrorMessage(response, lastMessage)
      } catch (_) {
        // try next endpoint
      }
    }

    throw new Error(lastMessage)
  },

  /**
   * Check if email is available
   * @param {string} email - Email to check
   * @returns {Promise} True if available, false otherwise
   */
  checkEmailAvailability(email) {
    return fetch(`${API_BASE_URL}/auth/check-email?email=${encodeURIComponent(email)}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json'
      }
    }).then(response => response.json())
  },

  /**
   * Logout user
   */
  logout() {
    localStorage.removeItem('authToken')
    localStorage.removeItem('userRole')
    localStorage.removeItem('refreshToken')
    localStorage.removeItem('tokenExpiration')
    sessionStorage.removeItem('authToken')
    sessionStorage.removeItem('userRole')
    sessionStorage.removeItem('refreshToken')
    sessionStorage.removeItem('tokenExpiration')
    localStorage.removeItem('superAdminEmail')
    sessionStorage.removeItem('superAdminEmail')
  },

  /**
   * Get stored auth token
   * @returns {string} Auth token or null
   */
  getToken() {
    return localStorage.getItem('authToken')
  },

  /**
   * Get stored user role
   * @returns {string} User role ('candidate' or 'company') or null
   */
  getUserRole() {
    return localStorage.getItem('userRole')
  },

  /**
   * Check if user is authenticated
   * @returns {boolean} True if authenticated
   */
  isAuthenticated() {
    return !!this.getToken()
  },

  /**
   * Request password reset
   * @param {string} email - User email
   * @returns {Promise} Response from API
   */
  async requestPasswordReset(email) {
    const response = await fetch(`${API_BASE_URL}/auth/forgot-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ email: email.trim().toLowerCase() })
    })

    if (!response.ok) {
      const message = await readErrorMessage(response, 'Échec de la demande de réinitialisation')
      throw new Error(message)
    }
    return response.json()
  },

  /**
   * Reset password with token
   * @param {string} email - User email
   * @param {string} token - Reset token
   * @param {string} newPassword - New password
   * @param {string} confirmPassword - Password confirmation
   * @returns {Promise} Response from API
   */
  async resetPassword(email, token, newPassword, confirmPassword) {
    const response = await fetch(`${API_BASE_URL}/auth/reset-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        email: email.trim().toLowerCase(),
        token,
        newPassword,
        confirmPassword
      })
    })

    if (!response.ok) {
      const message = await readErrorMessage(response, 'Échec de la réinitialisation du mot de passe')
      throw new Error(message)
    }
    return response.json()
  },

  /**
   * Verify email token
   * @param {string} token - Email verification token
   * @returns {Promise} Response from API
   */
  verifyEmail(token) {
    return fetch(`${API_BASE_URL}/auth/verify-email`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ token })
    }).then(response => {
      if (!response.ok) {
        throw new Error('Email verification failed')
      }
      return response.json()
    })
  }
}

export default authService
