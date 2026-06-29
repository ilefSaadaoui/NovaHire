
<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="parametres" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      
      <!-- CELESTIAL TOPBAR -->
      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome">
          <div class="header-with-icon">
            <div class="icon-box-themed">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 26px;"><path d="M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.1a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z"/><circle cx="12" cy="12" r="3"/></svg>
            </div>
            <div>
              <h1 class="premium-title-themed">{{ $t('settings.title') }}</h1>
              <p class="welcome-sub">{{ $t('settings.subtitle') }}</p>
            </div>
          </div>
        </div>
        
        <div class="r-header-tools">
          <div class="admin-identity-tag">
            <div class="tag-glow"></div>
            <span class="tag-dot"></span>
            <span class="tag-text">{{ $t('settings.adminMode') }}</span>
          </div>
        </div>
      </header>

      <div class="page-body stagger-reveal">
        <!-- FLOATING CELESTIAL TABS -->
        <nav class="admin-tabs-floating anim-reveal-up">
          <button v-for="t in tabs" :key="t.id" class="admin-tab-btn" :class="{ active: activeTab === t.id }" @click="activeTab = t.id">
            {{ t.label }}
          </button>
        </nav>

        <div class="tab-content-panel">
          <!-- TAB: TEAM -->
          <div v-if="activeTab === 'team'" class="panel-fade">
            <div class="admin-glass-card anim-reveal-up">
              <div class="section-header-premium">
                <div>
                  <h4 class="celestial-title imperial-aura">{{ $t('settings.team.title') }}</h4>
                  <p class="section-subtitle">{{ $t('settings.team.subtitle') }}</p>
                </div>
                <button class="btn-luxury primary" @click="showInviteModal = true">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width: 14px;"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                  {{ $t('settings.team.invite') }}
                </button>
              </div>
              
              <div class="glass-table-wrap">
                <table class="celestial-table">
                  <thead>
                    <tr>
                      <th>{{ $t('settings.team.member') }}</th>
                      <th>{{ $t('settings.team.roles') }}</th>
                      <th>{{ $t('settings.team.status') }}</th>
                      <th style="text-align: right;">{{ $t('settings.team.actions') }}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="m in companyStore.team" :key="m.id">
                      <td>
                        <div class="user-cell">
                          <div class="r-avatar sm" style="background: var(--accent-soft); color: var(--accent);">{{ m.initials }}</div>
                          <div class="user-meta">
                            <div class="u-name">{{ m.fullName }}</div>
                            <div class="u-email">{{ m.email }}</div>
                          </div>
                        </div>
                      </td>
                      <td>
                        <select v-model="m.role" class="celestial-select-table" @change="updateRole(m)">
                          <option value="CompanyAdmin">{{ $t('roles.admin') }}</option>
                          <option value="Recruiter">{{ $t('roles.recruiter') }}</option>
                        </select>
                      </td>
                      <td>
                        <div class="status-pill-glowing" :class="m.isActive ? 'active' : 'inactive'">
                          <span class="pulse-dot"></span>
                          {{ m.isActive ? $t('dashboard.admin.operational') : $t('dashboard.admin.offline') }}
                        </div>
                      </td>
                      <td style="text-align: right;">
                        <div class="action-group">
                          <button class="tool-btn celestial" @click="editMember(m)" :title="$t('offers.actions.details') || 'Détails'">
                             <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
                          </button>
                          <button class="tool-btn delete" @click="confirmDeleteMember(m)" :title="$t('settings.team.revoke')">
                             <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                          </button>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>

          <!-- TAB: DEPARTMENTS -->
          <div v-if="activeTab === 'departments'" class="panel-fade">
            <div class="admin-glass-card anim-reveal-up">
              <div class="section-header-premium">
                <div>
                  <h4 class="celestial-title imperial-aura">{{ $t('settings.departments.title') }}</h4>
                  <p class="section-subtitle">{{ $t('settings.departments.subtitle') }}</p>
                </div>
                <button class="btn-luxury primary" @click="openAddDeptModal">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width: 14px;"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
                  {{ $t('settings.departments.new') }}
                </button>
              </div>
              
              <div class="glass-table-wrap" style="margin-bottom: 30px;">
                <table class="celestial-table">
                  <thead>
                    <tr>
                      <th>{{ $t('settings.departments.name') }}</th>
                      <th>{{ $t('settings.departments.description') }}</th>
                      <th>{{ $t('settings.departments.members') }}</th>
                      <th>{{ $t('settings.departments.createdAt') }}</th>
                      <th style="text-align: right;">{{ $t('settings.team.actions') }}</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-if="!companyStore.departments || companyStore.departments.length === 0">
                      <td colspan="5" style="text-align: center; color: var(--r-text-sub); padding: 30px;">Aucun département créé.</td>
                    </tr>
                    <tr v-for="d in companyStore.departments" :key="d.id">
                      <td><div class="u-name">{{ d.name }}</div></td>
                      <td><div class="u-email">{{ d.description || '—' }}</div></td>
                      <td>
                        <div class="status-pill-glowing active">
                          {{ d.membersCount }} {{ $t('dashboard.recruiter.peopleCount', { count: '' }).replace(' pers.', '') }}
                        </div>
                      </td>
                      <td><div class="u-email">{{ new Date(d.createdAt).toLocaleDateString('fr-FR') }}</div></td>
                      <td style="text-align: right;">
                        <div class="action-group">
                          <button class="tool-btn celestial" @click="editDepartment(d)" title="Éditer">
                             <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path></svg>
                          </button>
                          <button class="tool-btn delete" @click="confirmDeleteDept(d)" title="Supprimer">
                             <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                          </button>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>

            <!-- Modal Inline pour Départements -->
            <transition name="modal-fade">
              <div v-if="showDeptModal" class="modal-backdrop-lux" style="position:fixed;top:0;left:0;width:100%;height:100%;z-index:999;display:flex;align-items:center;justify-content:center;background:rgba(15,23,42,0.6);backdrop-filter:blur(8px);" @click.self="showDeptModal = false">
                <div class="modal-card-lux" style="max-width: 500px;background:var(--card-bg);padding:32px;border-radius:24px;width:100%;">
                  <h3 class="celestial-title imperial-aura" style="margin-bottom:24px;border-bottom:1px solid var(--r-border);padding-bottom:16px;">{{ isEditingDept ? $t('settings.departments.edit') : $t('settings.departments.new') }}</h3>
                  
                  <div class="lux-input-group" style="margin-top: 20px;">
                    <label class="imperial-label">Nom <span style="color:var(--accent)">*</span></label>
                    <div class="lux-input-wrap">
                      <input v-model="deptForm.name" type="text" class="lux-input-text" :placeholder="$t('settings.departments.placeholders.name')">
                    </div>
                  </div>
                  
                  <div class="lux-input-group" style="margin-top: 20px;">
                    <label class="imperial-label">Description</label>
                    <div class="lux-input-wrap">
                      <textarea v-model="deptForm.description" class="lux-input-text lux-textarea" :placeholder="$t('settings.departments.placeholders.desc')"></textarea>
                    </div>
                  </div>
                  
                  <div class="action-group" style="margin-top: 30px; justify-content: flex-end;">
                    <button class="tool-btn" style="width:auto;padding:0 16px;font-weight:700;" @click="showDeptModal = false">{{ $t('settings.invite.cancel') }}</button>
                    <button class="btn-luxury primary" @click="submitDeptForm" :disabled="deptLoading">
                      <svg v-if="deptLoading" class="spin" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width: 14px;"><path d="M21 12a9 9 0 1 1-6.21-8.58"/></svg>
                      {{ isEditingDept ? $t('settings.departments.update') : $t('settings.departments.create') }}
                    </button>
                  </div>
                </div>
              </div>
            </transition>
          </div>

          <!-- TAB: BRANDING -->
          <div v-if="activeTab === 'branding'" class="panel-fade">
            <div class="admin-glass-card anim-reveal-up">
              <div class="section-header-premium">
                <div>
                  <h4 class="celestial-title imperial-aura">{{ $t('settings.branding.title') }}</h4>
                  <p class="section-subtitle">{{ $t('settings.branding.subtitle') }}</p>
                </div>
                  <svg v-if="isSaving" class="spin" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width: 14px;"><path d="M21 12a9 9 0 1 1-6.21-8.58"/></svg>
                  {{ isSaving ? $t('settings.branding.syncing') : $t('settings.branding.apply') }}
                </button>
              </div>

              <div class="branding-grid-premium">
                <div class="branding-form-lux">
                  <div class="lux-input-group">
                    <label class="imperial-label">{{ $t('settings.branding.companyName') }}</label>
                    <div class="lux-input-wrap">
                      <input type="text" v-model="brandingForm.companyName" class="lux-input-text" :placeholder="$t('settings.branding.companyName')">
                    </div>
                  </div>

                  <div class="lux-input-group">
                    <label class="imperial-label">{{ $t('settings.branding.description') }}</label>
                    <div class="lux-input-wrap">
                      <textarea v-model="brandingForm.description" class="lux-input-text lux-textarea" rows="3"></textarea>
                    </div>
                  </div>

                  <div class="form-grid-lux">
                    <div class="lux-input-group">
                      <label class="imperial-label">{{ $t('settings.branding.industry') }}</label>
                      <div class="lux-input-wrap">
                        <input type="text" v-model="brandingForm.industry" class="lux-input-text">
                      </div>
                    </div>

                    <div class="lux-input-group">
                      <label class="imperial-label">{{ $t('settings.branding.website') }}</label>
                      <div class="lux-input-wrap">
                        <input type="text" v-model="brandingForm.website" placeholder="URL" class="lux-input-text">
                      </div>
                    </div>
                  </div>

                  <div class="form-grid-lux" style="margin-top:20px;">
                    <div class="lux-input-group">
                      <label class="imperial-label">Pays</label>
                      <div class="lux-input-wrap">
                        <input type="text" v-model="brandingForm.country" placeholder="Pays" class="lux-input-text">
                      </div>
                    </div>

                    <div class="lux-input-group">
                      <label class="imperial-label">Téléphone de contact</label>
                      <div class="lux-input-wrap">
                        <input type="text" v-model="brandingForm.contactPhone" placeholder="Numéro" class="lux-input-text">
                      </div>
                    </div>
                  </div>



                  <div class="lux-input-group">
                    <label class="imperial-label">{{ $t('settings.branding.logo') }}</label>
                    <div class="lux-input-wrap">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="input-icon"><path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/></svg>
                      <input type="text" v-model="brandingForm.logoUrl" placeholder="URL" class="lux-input-text">
                    </div>
                  </div>
                </div>

                <div class="branding-preview-lux">
                  <label class="preview-title-lux">{{ $t('settings.branding.preview') }}</label>
                  <div class="mockup-container-lux">
                    <div class="mockup-frame">
                       <div class="mockup-navbar">
                          <img v-if="brandingForm.logoUrl" :src="brandingForm.logoUrl" class="mockup-logo">
                          <div v-else class="mockup-logo-placeholder">LOGO</div>
                       </div>
                       <div class="mockup-hero">
                          <div class="mockup-title">Rejoignez l'Elite</div>
                          <div class="mockup-cta">Postuler maintenant</div>
                       </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- TAB: PLAN -->
          <div v-if="activeTab === 'plan'" class="panel-fade">
            <div class="admin-glass-card anim-reveal-up">
              <div class="section-header-premium">
                <div>
                  <h4 class="celestial-title imperial-aura">Gérer l'abonnement</h4>
                  <p class="section-subtitle">Ajustez votre plan pour débloquer plus de fonctionnalités</p>
                </div>
                <button class="btn-luxury primary" @click="upgradePlan" :disabled="isUpgrading">
                  <svg v-if="isUpgrading" class="spin" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width: 14px;"><path d="M21 12a9 9 0 1 1-6.21-8.58"/></svg>
                  <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width: 16px;"><path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z"/></svg>
                  {{ isUpgrading ? 'Traitement...' : 'Mettre à jour' }}
                </button>
              </div>

              <div class="plans-container">
                <div class="plans-list">
                  <label v-for="plan in subscriptionPlans" :key="plan.code" class="plan-card-h" :class="{ selected: selectedPlan === plan.code }">
                    <input type="radio" v-model="selectedPlan" :value="plan.code" />
                    <div class="plan-icon-h">
                      <svg v-if="plan.code === 'free'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
                      <svg v-else-if="plan.code === 'professional'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>
                      <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 1L15.5 8.5 23 9.5 17.5 15 19 23 12 19 5 23 6.5 15 1 9.5 8.5 8.5z"/></svg>
                    </div>
                    <div class="plan-info-h">
                      <strong>{{ plan.name }}</strong>
                      <span>{{ plan.description }}</span>
                    </div>
                    <div class="plan-price-h">
                      <span v-if="plan.monthlyPrice === 0">GRATUIT</span>
                      <span v-else>{{ plan.monthlyPrice }}€/M</span>
                    </div>
                  </label>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- PREMIUM INVITATION MODAL -->
      <Transition name="celestial-modal">
        <div v-if="showInviteModal" class="luxury-modal-overlay" @click.self="showInviteModal = false">
          <div class="luxury-modal-content anim-scale-up">
            <div class="modal-header-premium">
              <h3 class="celestial-modal-title imperial-aura">{{ $t('settings.invite.title') }}</h3>
              <button class="close-lux-btn" @click="showInviteModal = false">&times;</button>
            </div>
            
            <div class="modal-body-premium">
              <p class="modal-intro">{{ $t('settings.invite.subtitle') }}</p>
              
              <form @submit.prevent="inviteMember" class="lux-form-compact">
                <div class="lux-input-row">
                  <div class="lux-input-group flex-1">
                    <label class="imperial-label">{{ $t('settings.invite.firstName') }}</label>
                    <div class="lux-input-wrap">
                      <input type="text" v-model="inviteForm.firstName" required class="lux-input-text" :placeholder="$t('profile.placeholders.fullName')">
                    </div>
                  </div>
                  <div class="lux-input-group flex-1">
                    <label class="imperial-label">{{ $t('settings.invite.lastName') }}</label>
                    <div class="lux-input-wrap">
                      <input type="text" v-model="inviteForm.lastName" required class="lux-input-text">
                    </div>
                  </div>
                </div>

                <div class="lux-input-group" style="margin-bottom:20px;">
                  <label class="imperial-label">Email Professionnel</label>
                  <div class="lux-input-wrap">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="input-icon"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                    <input type="email" v-model="inviteForm.email" required class="lux-input-text" placeholder="email@entreprise.com">
                  </div>
                </div>

                <div class="lux-input-group" style="margin-bottom:20px;">
                  <label class="imperial-label">Département (Optionnel)</label>
                  <div class="lux-input-wrap" style="padding:0;">
                    <select v-model="inviteForm.departmentId" class="expert-select-glass" style="border:none!important; outline:none; height:100%;">
                      <option :value="null">-- Aucun département --</option>
                      <option v-for="d in companyStore.departments" :value="d.id" :key="d.id">{{ d.name }}</option>
                    </select>
                  </div>
                </div>

                <div class="modal-footer-lux">
                  <button type="button" class="btn-cancel" @click="showInviteModal = false">{{ $t('settings.invite.cancel') }}</button>
                  <button type="submit" class="btn-luxury primary" :disabled="inviteLoading">
                    <svg v-if="inviteLoading" class="spin" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width: 14px;"><path d="M21 12a9 9 0 1 1-6.21-8.58"/></svg>
                    {{ inviteLoading ? $t('settings.invite.sending') : $t('settings.invite.send') }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </Transition>
      
      <!-- EDIT MEMBER MODAL -->
      <Transition name="celestial-modal">
        <div v-if="showEditModal" class="luxury-modal-overlay" @click.self="showEditModal = false">
          <div class="luxury-modal-content anim-scale-up">
            <div class="modal-header-premium">
              <h3 class="celestial-modal-title imperial-aura">{{ $t('settings.editMember.title') }}</h3>
              <button class="close-lux-btn" @click="showEditModal = false">&times;</button>
            </div>
            
            <div class="modal-body-premium">
              <p class="modal-intro">{{ $t('settings.editMember.subtitle') }}</p>
              
              <form @submit.prevent="submitEditMember" class="lux-form-compact">
                <div class="lux-input-row">
                  <div class="lux-input-group flex-1">
                    <label class="imperial-label">{{ $t('settings.invite.firstName') }}</label>
                    <div class="lux-input-wrap">
                      <input type="text" v-model="editForm.firstName" required class="lux-input-text">
                    </div>
                  </div>
                  <div class="lux-input-group flex-1">
                    <label class="imperial-label">{{ $t('settings.invite.lastName') }}</label>
                    <div class="lux-input-wrap">
                      <input type="text" v-model="editForm.lastName" required class="lux-input-text">
                    </div>
                  </div>
                </div>

                <div class="form-grid-lux" style="margin-top:20px;">
                  <div class="lux-input-group">
                    <label class="imperial-label">Email Professionnel</label>
                    <div class="lux-input-wrap">
                      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="input-icon"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                      <input type="email" v-model="editForm.email" required class="lux-input-text">
                    </div>
                  </div>

                  <div class="lux-input-group">
                    <label class="imperial-label">{{ $t('settings.team.status') }}</label>
                    <div class="lux-input-wrap" style="padding:0;">
                      <select v-model="editForm.isActive" class="expert-select-glass" style="border:none!important; outline:none; height:100%;">
                        <option :value="true">{{ $t('dashboard.admin.operational') }}</option>
                        <option :value="false">{{ $t('dashboard.admin.offline') }}</option>
                      </select>
                    </div>
                  </div>
                </div>

                <div class="lux-input-group" style="margin-top:20px;">
                  <label class="imperial-label">{{ $t('settings.invite.department') }}</label>
                  <div class="lux-input-wrap" style="padding:0;">
                    <select v-model="editForm.departmentId" class="expert-select-glass" style="border:none!important; outline:none; height:100%;">
                      <option :value="null">{{ $t('settings.invite.noDept') }}</option>
                      <option v-for="d in companyStore.departments" :value="d.id" :key="d.id">{{ d.name }}</option>
                    </select>
                  </div>
                </div>

                <div class="modal-footer-lux" style="margin-top:25px;">
                  <button type="button" class="btn-cancel" @click="showEditModal = false">{{ $t('settings.invite.cancel') }}</button>
                  <button type="submit" class="btn-luxury primary" :disabled="editLoading">
                    <svg v-if="editLoading" class="spin" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" style="width: 14px;"><path d="M21 12a9 9 0 1 1-6.21-8.58"/></svg>
                    {{ editLoading ? $t('settings.editMember.saving') : $t('settings.editMember.save') }}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </Transition>
    </main>
  </div>
</template>

<script>
import Sidebar from '@/components/layout/Sidebar.vue'

import { useAuthStore } from '@/stores/authStore'
import { useCompanyStore } from '@/stores/companyStore'
import { useToastStore } from '@/stores/toastStore'
import { useModalStore } from '@/stores/modalStore'

export default {
  name: 'Parametres',
  components: { Sidebar },
  setup() {
    return { 
      companyStore: useCompanyStore(),
      toast: useToastStore()
    }
  },
  data() {
    const authStore = useAuthStore()
    return {
      activeTab: 'team',
      sidebarCollapsed: false,
      accentColor: authStore.themeColors.accent,
      isSaving: false,
      showInviteModal: false,
      inviteLoading: false,
      inviteForm: {
        email: '',
        firstName: '',
        lastName: '',
        role: 'Recruiter',
        departmentId: null
      },
      showEditModal: false,
      editLoading: false,
      editForm: {
        id: '',
        firstName: '',
        lastName: '',
        email: '',
        isActive: true,
        departmentId: null
      },
      brandingForm: {
        companyName: '',
        logoUrl: '',
        description: '',
        industry: '',
        website: '',
        country: '',
        contactPhone: ''
      },
      selectedPlan: 'free',
      isUpgrading: false,
      showDeptModal: false,
      isEditingDept: false,
      deptLoading: false,
      deptForm: { id: null, name: '', description: '' }
    }
  },
  computed: {
    tabs() {
      return [
        { id: 'team', label: this.$t('common.team') || 'Équipe' },
        { id: 'departments', label: this.$t('settings.departments.title') },
        { id: 'branding', label: 'Branding' },
        { id: 'plan', label: this.$t('settings.subscription.title') }
      ]
    },
    subscriptionPlans() {
      return [
        { code: 'free', name: this.$t('settings.subscription.plans.free.name'), description: this.$t('settings.subscription.plans.free.desc'), monthlyPrice: 0 },
        { code: 'starter', name: this.$t('settings.subscription.plans.starter.name'), description: this.$t('settings.subscription.plans.starter.desc'), monthlyPrice: 29 },
        { code: 'professional', name: this.$t('settings.subscription.plans.professional.name'), description: this.$t('settings.subscription.plans.professional.desc'), monthlyPrice: 99 },
        { code: 'enterprise', name: this.$t('settings.subscription.plans.enterprise.name'), description: this.$t('settings.subscription.plans.enterprise.desc'), monthlyPrice: 299 }
      ]
    }
  },
  async mounted() {
    await Promise.all([
      this.companyStore.fetchTeam(),
      this.companyStore.fetchDepartments(),
      this.companyStore.fetchBranding()
    ])
    // Sync local form with store
    this.brandingForm = { ...this.companyStore.branding }
  },
  methods: {
    openAddDeptModal() {
      this.isEditingDept = false
      this.deptForm = { id: null, name: '', description: '' }
      this.showDeptModal = true
    },
    editDepartment(dept) {
      this.isEditingDept = true
      this.deptForm = { id: dept.id, name: dept.name, description: dept.description }
      this.showDeptModal = true
    },
    async submitDeptForm() {
      if (!this.deptForm.name) {
        this.toast.show("Le nom du département est requis", "error");
        return;
      }
      this.deptLoading = true;
      try {
        if (this.isEditingDept) {
          await this.companyStore.updateDepartment(this.deptForm.id, this.deptForm);
        } else {
          await this.companyStore.createDepartment(this.deptForm);
        }
        this.showDeptModal = false;
      } catch (error) {
        console.error("Erreur détaillée lors de la soumission :", error);
        this.toast.show(this.$t('notifications.updateError'), "error");
      } finally {
        this.deptLoading = false;
      }
    },
    async confirmDeleteDept(dept) {
      const modalStore = useModalStore()
      const confirmed = await modalStore.confirm({
        title: 'Supprimer ce département ?',
        message: `Voulez-vous vraiment supprimer le département « ${dept.name} » ? Cette action est irréversible.`,
        confirmText: 'Supprimer',
        cancelText: 'Annuler',
        type: 'danger'
      })
      if (confirmed) {
        await this.companyStore.deleteDepartment(dept.id)
      }
    },
    async saveBranding() {
      // Validations additionnelles
      if (this.brandingForm.contactPhone) {
        const phoneRegex = /^\+?[0-9\s\-()]{8,15}$/;
        if (!phoneRegex.test(this.brandingForm.contactPhone)) {
          this.toast.show("Le numéro de téléphone n'est pas valide (8 à 15 chiffres).", "error");
          return;
        }
      }
      
      if (this.brandingForm.website) {
        try {
          new URL(this.brandingForm.website);
        } catch (_) {
          this.toast.show("Le lien du site web n'est pas une URL valide.", "error");
          return;
        }
      }
      
      if (this.brandingForm.logoUrl) {
        try {
          new URL(this.brandingForm.logoUrl);
        } catch (_) {
          this.toast.show("Le lien du logo n'est pas une URL valide.", "error");
          return;
        }
      }

      this.isSaving = true
      await this.companyStore.updateBranding(this.brandingForm)
      this.isSaving = false
    },
    async confirmDeleteMember(member) {
      const modalStore = useModalStore()
      const confirmed = await modalStore.confirm({
        title: 'Retirer ce membre ?',
        message: `Êtes-vous sûr de vouloir retirer ${member.fullName} de l’équipe ?`,
        confirmText: 'Retirer',
        cancelText: 'Annuler',
        type: 'danger'
      })
      if (confirmed) {
        this.companyStore.removeTeamMember(member.id)
      }
    },
    async updateRole(member) {
      try {
        await this.companyStore.updateMemberRole(member.id, member.role)
        this.toast.show(this.$t('notifications.roleUpdated'), 'success')
      } catch (err) {
        this.toast.show('Erreur lors du changement de rôle', 'error')
      }
    },
    editMember(member) {
      this.editForm = {
        id: member.id,
        firstName: member.firstName,
        lastName: member.lastName,
        email: member.email,
        isActive: member.isActive,
        departmentId: member.departmentId || null
      }
      this.showEditModal = true
    },
    async submitEditMember() {
      this.editLoading = true
      try {
        await this.companyStore.updateMemberDetails(this.editForm.id, {
          firstName: this.editForm.firstName,
          lastName: this.editForm.lastName,
          email: this.editForm.email,
          isActive: this.editForm.isActive,
          departmentId: this.editForm.departmentId
        })
        this.showEditModal = false
      } catch (err) {
        // Error handling in store / interceptor
      } finally {
        this.editLoading = false
      }
    },
    async inviteMember() {
      this.inviteLoading = true
      try {
        const res = await this.companyStore.inviteMember(this.inviteForm)
        this.toast.show('Invitation expédiée avec succès ! 🚀', 'success')
        this.showInviteModal = false
        // Reset form
        this.inviteForm = { email: '', firstName: '', lastName: '', role: 'Recruiter', departmentId: null }
        await this.companyStore.fetchTeam()
      } catch (err) {
        // L'erreur est déjà gérée globalement par l'intercepteur axios (axios.js)
      } finally {
        this.inviteLoading = false
      }
    },
    async upgradePlan() {
      if (!this.selectedPlan) return;
      this.isUpgrading = true
      try {
        await this.companyStore.updateSubscriptionPlan(this.selectedPlan)
      } catch (err) {
        // Error is handled in store
      } finally {
        this.isUpgrading = false
      }
    }
  }
}
</script>

<style scoped>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";
@import "@/assets/admin-theme.css";

.dashboard-layout { display: flex; min-height: 100vh; background: transparent !important; }
.main-content { transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1); flex: 1; }
.ml-collapsed { margin-left: var(--sidebar-collapsed); }



/* SECTION HEADERS */
.section-header-premium { display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px; }
.celestial-title { font-size: 20px; font-weight: 900; color: var(--r-text-main); letter-spacing: -0.5px; }
.section-subtitle { font-size: 13px; color: var(--r-text-sub); margin-top: 4px; font-weight: 600; }

/* TABLES */
.glass-table-wrap { 
  border-radius: 20px; 
  border: 1px solid var(--r-border, transparent); 
  overflow: visible; 
  background: var(--card-bg, #ffffff); 
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.03);
}
.celestial-table { width: 100%; border-collapse: collapse; }
.celestial-table th { text-align: left; padding: 20px 24px; font-size: 11px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 1.5px; border-bottom: 1px solid var(--r-border); }
.celestial-table td { padding: 18px 24px; border-bottom: 1px solid var(--r-border); vertical-align: middle; }
.celestial-table tr:last-child td { border-bottom: none; }

.user-cell { display: flex; align-items: center; gap: 14px; }
.u-name { font-weight: 800; font-size: 14px; color: var(--r-text-main); }
.u-email { font-size: 12px; color: var(--r-text-sub); font-weight: 600; }

/* ACTIONS */
.action-group { display: flex; gap: 10px; justify-content: flex-end; }
.tool-btn { width: 36px; height: 36px; border-radius: 10px; border: none; background: rgba(0,0,0,0.03); color: var(--r-text-sub); cursor: pointer; transition: 0.3s; display: flex; align-items: center; justify-content: center; }
.tool-btn:hover { background: var(--accent-soft); color: var(--accent); transform: translateY(-2px); box-shadow: 0 5px 15px rgba(0,0,0,0.04); }
.tool-btn.delete:hover { background: rgba(239, 68, 68, 0.1); color: #ef4444; box-shadow: 0 5px 15px rgba(239, 68, 68, 0.1); }

/* BRANDING FORM LUX */
.branding-grid-premium { display: grid; grid-template-columns: 1fr 1fr; gap: 60px; align-items: start; }
.branding-form-lux { display: flex; flex-direction: column; gap: 24px; }
.form-grid-lux { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.form-grid-lux.color-row { margin-top: 8px; }

.lux-textarea { min-height: 80px; padding: 12px 0; resize: vertical; }

.lux-input-group label { display: block; font-size: 12px; font-weight: 900; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 1px; margin-bottom: 12px; }
.lux-color-box { display: flex; align-items: center; gap: 16px; background: var(--r-surface); padding: 8px 16px; border-radius: 16px; border: 1px solid var(--r-border); transition: 0.3s; }
.lux-color-box:focus-within { border-color: var(--accent); box-shadow: 0 0 0 4px var(--accent-soft); }

.color-preview-circle { width: 32px; height: 32px; border-radius: 50%; border: 24px solid rgba(255,255,255,0.1); cursor: pointer; box-shadow: 0 4px 10px rgba(0,0,0,0.1); }
.hex-picker-hidden { position: absolute; opacity: 0; width: 32px; height: 32px; cursor: pointer; }

.lux-input-text { flex: 1; background: transparent; border: none; outline: none; color: var(--r-text-main); font-weight: 700; font-size: 14px; }
.lux-input-wrap { display: flex; align-items: center; gap: 12px; background: rgba(255, 255, 255, 0.03); padding: 14px 20px; border-radius: 16px; border: 1px solid var(--r-border); transition: 0.3s; }
.lux-input-wrap:focus-within { border-color: var(--accent); box-shadow: 0 0 0 4px var(--accent-soft); }
.input-icon { width: 18px; color: var(--r-text-sub); }

.celestial-select-table {
  background: rgba(255, 255, 255, 0.05);
  border: 1px solid var(--r-border);
  color: var(--r-text-main);
  padding: 6px 12px;
  border-radius: 8px;
  font-weight: 700;
  outline: none;
  cursor: pointer;
}

.preview-title-lux {
  display: block;
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
  color: var(--r-text-sub);
  margin-bottom: 20px;
  text-align: center;
}

/* MOCKUP LUX */
.mockup-container-lux { background: var(--r-main-bg); padding: 40px; border-radius: 32px; border: 1px solid var(--r-border); position: relative; overflow: visible; }
.mockup-frame { background: #f8fafc; width: 100%; border-radius: 16px; height: 360px; border: 1px solid #e2e8f0; display: flex; flex-direction: column; overflow: visible; box-shadow: 0 30px 60px -20px rgba(0,0,0,0.2); }

.mockup-navbar { height: 60px; padding: 0 24px; display: flex; align-items: center; background: white; border-bottom: 1px solid #f1f5f9; }
.mockup-logo { height: 28px; width: auto; }
.mockup-logo-placeholder { font-weight: 900; color: var(--brand-color); font-size: 16px; }

.mockup-hero { flex: 1; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 24px; padding: 40px; background: linear-gradient(135deg, #fff 0%, #f1f5f9 100%); }
.mockup-title { font-size: 28px; font-weight: 900; color: #1e293b; text-align: center; letter-spacing: -1px; }
.mockup-cta { padding: 14px 28px; background: var(--brand-color); color: white; border-radius: 12px; font-weight: 800; font-size: 14px; box-shadow: 0 10px 20px -5px var(--accent-color); transition: 0.3s; }

/* PLANS VERTICAL LIST */
.plans-container { padding: 24px 0; max-width: 800px; margin: 0 auto; }
.plans-list { display: flex; flex-direction: column; gap: 16px; }
.plan-card-h {
  position: relative; display: flex; align-items: center; gap: 20px;
  padding: 24px; background: var(--r-surface);
  border: 1px solid var(--r-border); border-radius: 20px;
  cursor: pointer; transition: 0.3s;
}
.plan-card-h:hover { 
  background: rgba(255,255,255,0.04); border-color: rgba(255,255,255,0.15); 
  transform: translateX(4px);
}
.plan-card-h.selected {
  background: var(--accent-soft); border-color: var(--accent);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
  transform: translateX(4px);
}
.plan-card-h.selected:hover { border-color: var(--accent); transform: translateX(6px); }
.plan-card-h input { 
  position: absolute; 
  opacity: 0; 
  width: 0; 
  height: 0; 
}

.plan-icon-h { 
  width: 56px; height: 56px; border-radius: 16px; background: rgba(0,0,0,0.05);
  display: flex; align-items: center; justify-content: center; flex-shrink: 0;
  transition: 0.3s; border: 1px solid var(--r-border);
}
.plan-icon-h svg { width: 24px; color: var(--r-text-sub); transition: 0.3s; }
.plan-card-h.selected .plan-icon-h { background: var(--accent); border-color: transparent; }
.plan-card-h.selected .plan-icon-h svg { color: var(--r-main-bg); }

.plan-info-h { flex: 1; }
.plan-info-h strong { display: block; font-size: 18px; font-weight: 900; margin-bottom: 4px; color: var(--r-text-main); transition: 0.3s; letter-spacing: 0.5px; }
.plan-info-h span { display: block; font-size: 14px; font-weight: 600; color: var(--r-text-sub); line-height: 1.4; transition: 0.3s; }

.plan-price-h { font-size: 20px; font-weight: 900; color: var(--accent); letter-spacing: -0.5px; white-space: nowrap; }
.plan-card-h.selected .plan-price-h { color: var(--r-text-main); }


.spin { animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }

@media (max-width: 1200px) { .branding-grid-premium { grid-template-columns: 1fr; } }
</style>

<style>
/* Fix for button text visibility in light theme */
body:not(.dark-mode) .dashboard-layout .btn-luxury,
body:not(.dark-mode) .dashboard-layout .btn-cancel {
  color: #1e293b !important;
}
</style>
