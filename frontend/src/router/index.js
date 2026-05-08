import { createRouter, createWebHistory } from 'vue-router'

// Auth / Public
import Welcome from '../views/public/Welcome.vue'
import Login from '../views/public/Login.vue'
import Register from '../views/public/Register.vue'
import RegisterCompany from '../views/public/RegisterCompany.vue'
import ForgotPassword from '../views/public/ForgotPassword.vue'
import ResetPassword from '../views/public/ResetPassword.vue'

// Plateforme recruteur
import AdminDashboard from '../views/company-admin/AdminDashboard.vue'
import RecruiterDashboard from '../views/recruiter/dashboard/RecruiterDashboard.vue'
import JobOffers from '../views/recruiter/jobs/JobOffers.vue'
import CreateJobOffer from '../views/recruiter/jobs/CreateJobOffer.vue'
import JobOfferDetail from '../views/recruiter/jobs/JobOfferDetail.vue'
import Applications from '../views/recruiter/candidates/Applications.vue'
import CandidateProfile from '../views/recruiter/candidates/CandidateProfile.vue'
import Reports from '../views/recruiter/dashboard/Reports.vue'
import Settings from '../views/company-admin/Settings.vue'
import Profile from '../views/recruiter/account/Profile.vue'
import Calendar from '../views/recruiter/dashboard/Calendar.vue'
import SharedJob from '../views/public/SharedJob.vue'
import SuperAdminLogin from '../views/superadmin/SuperAdminLogin.vue'
import SuperAdminDashboard from '../views/superadmin/SuperAdminDashboard.vue'

const routes = [
  // ── Routes publiques ──
  { path: '/', name: 'Welcome', component: Welcome },
  { path: '/connexion', name: 'Login', component: Login, alias: '/login' },
  { path: '/mot-de-passe-oublie', name: 'ForgotPassword', component: ForgotPassword },
  { path: '/reinitialiser-mot-de-passe', name: 'ResetPassword', component: ResetPassword },
  { path: '/inscription', redirect: '/inscription/entreprise' },
  { path: '/inscription/entreprise', name: 'RegisterCompany', component: RegisterCompany },

  { path: '/shared-job/:token', name: 'SharedJob', component: SharedJob },
  { path: '/public/quiz/:token', name: 'PublicQuiz', component: () => import('../views/public/PublicQuiz.vue') },


  // ── Plateforme recruteur (protégée) ──
  { 
    path: '/dashboard', 
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        redirect: to => {
          const role = (localStorage.getItem('userRole') || sessionStorage.getItem('userRole') || '').toLowerCase();
          if (['companyadmin', 'admin', 'company'].includes(role)) return '/dashboard/companyadmin';
          return '/dashboard/recruiter';
        }
      },
      {
        path: 'companyadmin',
        name: 'AdminDashboard',
        component: AdminDashboard,
        meta: { requiresAdmin: true }
      },
      {
        path: 'recruiter',
        name: 'RecruiterDashboard',
        component: RecruiterDashboard
      }
    ]
  },
  { path: '/jobs', name: 'JobOffers', component: JobOffers, meta: { requiresAuth: true } },
  { path: '/jobs/new', name: 'CreateJobOffer', component: CreateJobOffer, meta: { requiresAuth: true } },
  { path: '/jobs/:id', name: 'JobOfferDetail', component: JobOfferDetail, meta: { requiresAuth: true } },
  { path: '/jobs/:id/edit', name: 'EditJobOffer', component: CreateJobOffer, meta: { requiresAuth: true } },
  { path: '/applications', name: 'Applications', component: Applications, meta: { requiresAuth: true } },
  { path: '/applications/profile/:id', name: 'CandidateProfile', component: CandidateProfile, meta: { requiresAuth: true } },
  { path: '/profile', name: 'Profile', component: Profile, meta: { requiresAuth: true } },
  { path: '/reports', name: 'Reports', component: Reports, meta: { requiresAuth: true } },
  { path: '/settings', name: 'Settings', component: Settings, meta: { requiresAuth: true, requiresAdmin: true } },
  { path: '/reports/analysis/:id', name: 'JobAnalysis', component: () => import('../views/recruiter/candidates/JobAnalysis.vue'), meta: { requiresAuth: true } },
  { path: '/calendar', name: 'Calendar', component: Calendar, meta: { requiresAuth: true } },

  // ── SuperAdmin (protégée) ──
  { path: '/superadmin/connexion', name: 'SuperAdminLogin', component: SuperAdminLogin },
  { path: '/superadmin/dashboard', name: 'SuperAdminDashboard', component: SuperAdminDashboard, meta: { requiresAuth: true, requiresSuperAdmin: true } },
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// ── Guard global d'authentification ──
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('authToken') || sessionStorage.getItem('authToken')
  const userRole = (localStorage.getItem('userRole') || sessionStorage.getItem('userRole') || '').toLowerCase()

  // Routes nécessitant SuperAdmin
  if (to.meta.requiresSuperAdmin) {
    if (userRole !== 'superadmin' || !token) {
      next('/superadmin/connexion')
      return
    }
  }

  // Routes nécessitant une authentification
  if (to.meta.requiresAuth) {
    if (!token) {
      next('/connexion')
      return
    }
  }

  // Routes nécessitant admin (Company ou SuperAdmin)
  if (to.meta.requiresAdmin) {
    if (!['company', 'admin', 'companyadmin', 'superadmin'].includes(userRole)) {
      next('/dashboard')
      return
    }
  }

  // Rediriger SuperAdmin déjà connecté qui va sur la page de login
  if (to.name === 'SuperAdminLogin' && userRole === 'superadmin' && token) {
    next('/superadmin/dashboard')
    return
  }

  next()
})

export default router

