<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="candidatures" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main stagger-reveal" :class="{ 'ml-collapsed': sidebarCollapsed }">
      
      <header class="r-topbar anim-reveal-down">
        <div class="r-welcome" style="display: flex; flex-direction: row !important; align-items: center; gap: 20px;">
          <button class="back-link-premium" @click="$router.push('/candidatures')" title="Retour">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 24px; height: 24px;"><polyline points="15 18 9 12 15 6"/></svg>
          </button>
          <div class="header-details-premium">
            <h1 class="premium-title-themed" style="font-size: 28px; margin-bottom: 4px;">{{ candidate.fullName }}</h1>
            <div class="cand-meta-row" style="font-size: 14px; color: var(--r-text-sub); font-weight: 600; display: flex; align-items: center; gap: 8px;">
              <span class="cand-job-ref">Offre: {{ candidate.jobTitle }}</span>
              <span class="dot-sep" style="width: 4px; height: 4px; border-radius: 50%; background: var(--r-text-sub);"></span>
              <span class="cand-location">{{ candidate.location }}</span>
            </div>
          </div>
        </div>
        <div class="r-header-tools">
          <div class="action-cluster">
            <button class="btn-premium btn-secondary" @click="viewCV">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 18px;"><path d="M2 12s3-7 10-7 10 7 10 7-3 7-10 7-10-7-10-7Z"/><circle cx="12" cy="12" r="3"/></svg>
              {{ $t('profile.viewCV') || 'Voir le CV' }}
            </button>

            <button class="btn-premium btn-secondary" @click="downloadPDF" :disabled="isExporting">
              <svg v-if="!isExporting" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 18px;"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
              <div v-else class="spinner-xs"></div>
              {{ isExporting ? $t('common.exporting') || 'Exportation...' : $t('profile.downloadPDF') || 'Rapport PDF' }}
            </button>

            <button class="btn-luxury primary" @click="scheduleInterview">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" style="width: 18px;"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
              {{ $t('dashboard.recruiter.plannedInterviews') }}
            </button>
          </div>
        </div>
      </header>

      <div class="page-body">
        <div class="profile-grid-container">
          
          <!-- LEFT SIDE: CANDIDATE IDENTITY & IA -->
          <div class="side-col">
            <div class="r-card cand-id-card">
              <div class="id-header">
                <div class="cand-avatar-hero" :style="avatarStyle">{{ candidate.initials }}</div>
                <div class="id-main">
                  <h3>{{ candidate.fullName }}</h3>
                  <span class="cand-badge">{{ $t('pipeline.analyzedRecently') || 'Analysé récemment' }}</span>
                </div>
              </div>
              <div class="contact-stack">
                <div class="c-item">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"/><polyline points="22,6 12,13 2,6"/></svg>
                  <span>{{ candidate.email }}</span>
                </div>
                <div class="c-item">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72 12.84 12.84 0 0 0 .7 2.81 2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45 12.84 12.84 0 0 0 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
                  <span>{{ candidate.phone }}</span>
                </div>
              </div>
              <div class="status-update-row">
                <label>{{ $t('pipeline.table.stage') }}</label>
                <div class="select-wrapper">
                  <select v-model="currentStage" class="r-select">
                    <option value="new">{{ $t('pipeline.stages.submitted') }}</option>
                    <option value="analyzed">{{ $t('pipeline.stages.underreview') }}</option>
                    <option value="preselected">{{ $t('pipeline.stages.shortlisted') }}</option>
                    <option value="interview">{{ $t('pipeline.stages.interview') }}</option>
                    <option value="offer">{{ $t('pipeline.stages.interviewed') }}</option>
                    <option value="rejected">{{ $t('pipeline.stages.rejected') }}</option>
                  </select>
                  <svg class="select-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="6 9 12 15 18 9"/></svg>
                </div>
              </div>
            </div>

            <div class="r-card ia-insight-card">
              <div class="ia-card-head">
                <div class="icon-box-themed" style="width: 44px; height: 44px; border-radius: 12px;">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg>
                </div>
                <div class="ia-head-text">
                  <h4>{{ $t('pipeline.iaDiagnostic') || 'Diagnostic IA' }}</h4>
                  <p v-if="isAiAnalyzed">{{ $t('pipeline.iaDiagnosticSub', { count: criteria.length || 14 }) }}</p>
                  <p v-else>{{ $t('pipeline.iaPending') || 'Analyse en attente' }}</p>
                </div>
              </div>

              <!-- PENDING ANALYSIS STATE -->
              <template v-if="!isAiAnalyzed">
                <div class="ia-score-viz-large">
                  <div class="score-ring-large pending-ring">
                    <svg viewBox="0 0 100 100">
                      <circle cx="50" cy="50" r="45" fill="none" stroke="var(--r-border)" stroke-width="8" stroke-dasharray="8 6" opacity="0.5"/>
                    </svg>
                    <div class="score-center">
                      <div class="pending-icon-pulse">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width: 28px; color: var(--r-text-sub);">
                          <circle cx="12" cy="12" r="10"/>
                          <polyline points="12 6 12 12 16 14"/>
                        </svg>
                      </div>
                      <span class="s-lab" style="margin-top: 6px;">{{ $t('dashboard.loading') }}</span>
                    </div>
                  </div>
                </div>
                <div class="ia-pending-notice">
                  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width: 18px; flex-shrink: 0; color: var(--accent-color);">
                    <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/>
                  </svg>
                  <p>{{ $t('pipeline.iaPendingSub') || "Ce profil n'a pas encore été analysé par l'IA. Le diagnostic sera disponible après le traitement automatique du CV." }}</p>
                </div>
              </template>

              <!-- ANALYZED STATE -->
              <template v-else>
                <div class="ia-score-viz-large">
                  <div class="score-ring-large">
                    <svg viewBox="0 0 100 100">
                      <circle cx="50" cy="50" r="45" fill="none" stroke="var(--r-border)" stroke-width="8"/>
                      <circle cx="50" cy="50" r="45" fill="none" stroke="var(--accent)" stroke-width="8"
                        stroke-dasharray="283" :stroke-dashoffset="283 - (283 * candidate.overallScore / 100)"
                        stroke-linecap="round" transform="rotate(-90 50 50)"/>
                    </svg>
                    <div class="score-center">
                      <span class="s-val">{{ candidate.overallScore }}%</span>
                      <span class="s-lab">Matching</span>
                    </div>
                  </div>
                </div>

                <div class="ia-criteria-stack">
                  <div v-for="c in criteria" :key="c.name" class="ia-crit-row">
                    <div class="crit-head">
                      <span class="crit-name">{{ c.name }}</span>
                      <span class="crit-score">{{ c.score }}%</span>
                    </div>
                    <div class="crit-bar">
                      <div class="crit-fill" :style="{ width: c.score + '%', background: 'var(--accent)' }"></div>
                    </div>
                  </div>
                </div>

                <!-- SKILLS GAP ANALYSIS (New Section) -->
                <div class="skills-gap-analysis">
                  <h5 class="gap-title">{{ $t('profile.skillsGapTitle') || 'Analyse de l\'Écart (Skills Gap)' }}</h5>
                  <div class="gap-grid">
                    <div v-for="item in skillsGap" :key="item.name" class="gap-item" :class="item.status">
                      <div class="gap-status-icon">
                        <svg v-if="item.status === 'match'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg>
                        <svg v-else-if="item.status === 'missing'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                        <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M12 2L15.09 8.26L22 9.27L17 14.14L18.18 21.02L12 17.77L5.82 21.02L7 14.14L2 9.27L8.91 8.26L12 2Z"/></svg>
                      </div>
                      <span class="gap-name">{{ item.name }}</span>
                    </div>
                  </div>
                </div>

                <div class="ia-verdict-box">
                  <h5>{{ $t('pipeline.iaVerdict') || 'Verdict IA' }}</h5>
                  <p>{{ candidate.aiSummary }}</p>
                </div>
              </template>
            </div>
          </div>

          <!-- RIGHT SIDE: DETAILS & TIMELINE -->
          <div class="main-col">
            <div class="premium-tabs-minimal">
              <button v-for="t in tabs" :key="t.id" class="minimal-tab" :class="{ active: activeTab === t.id }" @click="activeTab = t.id">
                {{ t.label }}
                <div v-if="activeTab === t.id" class="tab-indicator"></div>
              </button>
            </div>

            <div class="tab-content-panel">
              <!-- CV CONTENT -->
              <div v-if="activeTab === 'cv'" class="panel-fade">
                <div class="r-card premium-inner-card">
                  <section class="details-section">
                    <h4 class="section-title">{{ $t('pipeline.candidate.expTitle') || 'Expériences Professionnelles' }}</h4>
                  <div class="exp-premium-timeline">
                    <div v-for="exp in candidate.experiences" :key="exp.id" class="exp-p-item">
                      <div class="exp-p-logo">
                        <span>{{ exp.companyInitial }}</span>
                      </div>
                      <div class="exp-p-body">
                        <div class="exp-p-head">
                          <h5 class="exp-p-title">{{ exp.role }}</h5>
                          <span class="exp-p-period">{{ exp.period }}</span>
                        </div>
                        <div class="exp-p-company">{{ exp.company }} · {{ exp.duration }}</div>
                        <p v-if="exp.description && exp.description.trim()" class="exp-p-desc">{{ exp.description }}</p>
                        <div v-if="exp.tags" class="exp-p-tags">
                          <span v-for="tag in exp.tags" :key="tag" class="p-tag">{{ tag }}</span>
                        </div>
                      </div>
                    </div>
                  </div>
                </section>

                <section class="details-section">
                  <h4 class="section-title">{{ $t('pipeline.candidate.eduTitle') || 'Formations & Diplômes' }}</h4>
                  <div class="edu-p-grid">
                    <div v-for="edu in candidate.education" :key="edu.id" class="edu-p-card">
                      <div class="edu-p-icon">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 10v6M2 10l10-5 10 5-10 5z"/><path d="M6 12v5c3 3 9 3 12 0v-5"/></svg>
                      </div>
                      <div class="edu-p-info">
                        <h6>{{ edu.degree }}</h6>
                        <p>{{ edu.school }} · {{ edu.year }}</p>
                      </div>
                    </div>
                  </div>
                </section>

                <section class="details-section">
                  <h4 class="section-title">{{ $t('pipeline.candidate.skillsTitle') || 'Compétences & Expertise' }}</h4>
                  <div class="skills-p-layout">
                    <div v-for="group in candidate.skillGroups" :key="group.name" class="skill-p-group">
                      <label>{{ group.name }}</label>
                      <div class="skill-p-row">
                        <span v-for="s in group.skills" :key="s" class="skill-p-pill">{{ s }}</span>
                      </div>
                    </div>
                  </div>
                </section>
              </div>
              </div>

              <!-- MOTIVATION -->
              <div v-if="activeTab === 'lettre'" class="panel-fade">
                <div class="r-card premium-inner-card">
                  <div class="motivation-letter-card">
                    <div class="letter-inner">
                      <p>{{ candidate.coverLetter }}</p>
                    </div>
                  </div>
                </div>
              </div>

              <!-- TIMELINE -->
              <div v-if="activeTab === 'timeline'" class="panel-fade">
                <div class="r-card premium-inner-card">
                  <div v-if="!timeline || timeline.length === 0" class="empty-timeline-inline" style="display: flex; flex-direction: column; align-items: center; gap: 16px; padding: 64px 0; color: var(--r-text-sub); text-align: center;">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width: 48px; opacity: 0.3;">
                      <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
                    </svg>
                    <p style="font-size: 14px; font-weight: 600; max-width: 300px;">{{ $t('pipeline.emptyHistory') || 'Aucun historique disponible pour cette candidature.' }}</p>
                  </div>
                  <div v-else class="history-premium-list">
                    <div v-for="event in timeline" :key="event.id" class="history-p-item">
                      <div class="h-dot" :style="{ background: event.color }"></div>
                      <div class="h-content">
                        <div class="h-header">
                          <span class="h-title">{{ event.title }}</span>
                          <span class="h-time" style="font-size: 11px; font-weight: 700; background: var(--r-main-bg); padding: 3px 10px; border-radius: 100px; border: 1px solid var(--r-border);">{{ event.timeAgo || event.time }}</span>
                        </div>
                        <p v-if="event.description || event.desc" class="h-desc">{{ event.description || event.desc }}</p>
                        <div v-if="event.actor" style="display: flex; align-items: center; gap: 6px; margin-top: 8px; font-size: 12px; font-weight: 600; color: var(--accent-color);">
                          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" style="width: 14px;">
                            <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/><circle cx="12" cy="7" r="4"/>
                          </svg>
                          <span>{{ event.actor }}</span>
                        </div>
                        <div style="font-size: 11px; color: var(--r-text-sub); opacity: 0.6; margin-top: 6px; font-weight: 500;">{{ event.time }}</div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              <!-- AI INTERVIEW GUIDE -->
              <div v-if="activeTab === 'interview_guide'" class="panel-fade">
                <div class="interview-guide-container">
                  <div class="guide-header">
                    <div class="guide-badge">AI POWERED</div>
                    <h3>{{ $t('profile.guideTitle') || 'Guide d\'Entretien Stratégique' }}</h3>
                    <p>{{ $t('profile.guideSub') || 'Questions personnalisées générées par l\'IA pour valider les compétences et le potentiel du candidat.' }}</p>
                  </div>

                  <div v-if="!candidate.interviewQuestions || candidate.interviewQuestions.length === 0" class="empty-guide">
                    <div class="empty-icon">🤖</div>
                    <p>{{ $t('profile.guidePending') || 'Relancez l\'analyse IA pour générer votre guide d\'entretien personnalisé.' }}</p>
                  </div>

                  <div v-else class="questions-grid">
                    <div v-for="(q, idx) in candidate.interviewQuestions" :key="idx" class="q-card anim-reveal-up" :style="{ '--delay': (idx * 0.1) + 's' }">
                      <div class="q-header">
                        <span class="q-category">{{ q.category }}</span>
                        <span class="q-number">#{{ idx + 1 }}</span>
                      </div>
                      <p class="q-text">{{ q.question }}</p>
                      <div class="q-footer">
                        <div class="q-purpose">
                          <span class="p-label">Objectif :</span> {{ q.purpose }}
                        </div>
                      </div>
                    </div>
                  </div>

                  <div class="guide-pro-tips">
                    <h5>💡 {{ $t('profile.proTips') || 'Conseils de Recrutement' }}</h5>
                    <ul>
                      <li>{{ $t('profile.tip1') || 'Utilisez la méthode STAR (Situation, Tâche, Action, Résultat) pour creuser les réponses.' }}</li>
                      <li>{{ $t('profile.tip2') || 'Faites attention aux soft skills et à l\'adéquation culturelle mentionnée dans l\'analyse.' }}</li>
                    </ul>
                  </div>
                </div>
              </div>

              <!-- COMMENTS / DISCUSSION -->
              <div v-if="activeTab === 'comments'" class="panel-fade">
                <div class="r-card premium-inner-card discussion-thread">
                  <div v-if="candidate.comments && candidate.comments.length > 0" class="comment-list">
                    <div v-for="comment in candidate.comments" :key="comment.id" class="comment-item">
                      <div class="comment-avatar" :style="{ background: accentColor }">{{ comment.authorName[0] }}</div>
                      <div class="comment-content-box">
                        <div class="comment-meta">
                          <span class="comment-author">{{ comment.authorName }}</span>
                          <span class="comment-time">{{ comment.timeAgo }}</span>
                        </div>
                        <p class="comment-text">{{ comment.content }}</p>
                      </div>
                    </div>
                  </div>
                  <div v-else class="empty-discussion">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
                    <p>{{ $t('pipeline.emptyDiscussion') || 'Aucun commentaire pour le moment. Soyez le premier à noter ce candidat.' }}</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- RECRUITER NOTES BOX (NOW COLLABORATIVE COMMENT INPUT) -->
            <div class="r-card notes-premium">
              <div class="notes-header">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 11.5a8.38 8.38 0 0 1-.9 3.8 8.5 8.5 0 0 1-7.6 4.7 8.38 8.38 0 0 1-3.8-.9L3 21l1.9-5.7a8.38 8.38 0 0 1-.9-3.8 8.5 8.5 0 0 1 4.7-7.6 8.38 8.38 0 0 1 3.8-.9h.5a8.48 8.48 0 0 1 8 8v.5z"/></svg>
                <h4>{{ $t('pipeline.collaborativeTitle') || 'Discussion Collaborative' }}</h4>
              </div>
              <textarea v-model="recruiterNotes" :placeholder="$t('pipeline.modal.notePlaceholder')" rows="2"></textarea>
              <div class="notes-footer">
                <span class="hint">{{ $t('pipeline.collaborativeHint') || 'Visible par toute l\'équipe de recrutement.' }}</span>
                <button class="btn-luxury primary" @click="postComment" :disabled="isSavingNote || !recruiterNotes.trim()">
                  {{ isSavingNote ? $t('common.sending') : $t('common.publish') || 'Publier' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- INTERVIEW SCHEDULING MODAL -->
    <div v-if="showInterviewModal" class="modal-overlay" @click.self="showInterviewModal = false">
      <div class="modal-content premium-modal" style="max-width: 620px; width: 90%;">
        <div class="modal-header">
          <h3>{{ $t('dashboard.recruiter.plannedInterviews') }}</h3>
          <button class="close-btn" @click="showInterviewModal = false">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
          </button>
        </div>
        <div class="modal-body">
          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 20px;">
            <div class="form-group">
              <label style="display: block; margin-bottom: 8px; font-weight: 600; font-size: 13px; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 0.5px;">{{ $t('pipeline.table.date') }}</label>
              <input type="date" v-model.lazy="interviewForm.date" style="width: 100%; padding: 12px; border: 1px solid var(--r-border); border-radius: 8px; background: var(--r-main-bg); color: var(--r-text-main);" />
            </div>
            <div class="form-group">
              <label style="display: block; margin-bottom: 8px; font-weight: 600; font-size: 13px; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 0.5px;">{{ $t('reports.period.day') }}</label>
              <input type="time" v-model.lazy="interviewForm.time" style="width: 100%; padding: 12px; border: 1px solid var(--r-border); border-radius: 8px; background: var(--r-main-bg); color: var(--r-text-main);" />
            </div>
          </div>
          <div class="form-group" style="margin-bottom: 20px;">
            <label style="display: block; margin-bottom: 8px; font-weight: 600; font-size: 13px; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 0.5px;">{{ $t('pipeline.modal.interviewType') || 'Type d\'Entretien' }}</label>
            <select v-model="interviewForm.type" @change="generateEmailTemplate" style="width: 100%; padding: 12px; border: 1px solid var(--r-border); border-radius: 8px; background: var(--r-main-bg); color: var(--r-text-main); outline: none;">
              <option value="visio" style="background: var(--r-main-bg); color: var(--r-text-main);">{{ $t('pipeline.modal.types.visio') || 'Visio / Télétravail' }}</option>
              <option value="phone" style="background: var(--r-main-bg); color: var(--r-text-main);">{{ $t('pipeline.modal.types.phone') || 'Appel Téléphonique' }}</option>
              <option value="onsite" style="background: var(--r-main-bg); color: var(--r-text-main);">{{ $t('pipeline.modal.types.onsite') || 'En Personne (Bureau)' }}</option>
            </select>
          </div>
          <div style="border-top: 1px solid var(--r-border); padding-top: 20px; margin-top: 8px;">
            <p style="font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px; color: var(--accent-color); margin-bottom: 16px;">{{ $t('pipeline.modal.emailHint') || 'E-mail envoyé au candidat' }}</p>
            <div class="form-group" style="margin-bottom: 16px;">
              <label style="display: block; margin-bottom: 8px; font-weight: 600; font-size: 13px; color: var(--r-text-sub);">{{ $t('pipeline.modal.subject') || 'Objet' }}</label>
              <input type="text" v-model="interviewForm.subject" style="width: 100%; padding: 12px; border: 1px solid var(--r-border); border-radius: 8px; background: var(--r-main-bg); color: var(--r-text-main);" />
            </div>
            <div class="form-group">
              <label style="display: block; margin-bottom: 8px; font-weight: 600; font-size: 13px; color: var(--r-text-sub);">{{ $t('pipeline.modal.message') || 'Corps du message' }}</label>
              <textarea v-model="interviewForm.message" rows="8" style="width: 100%; padding: 12px; border: 1px solid var(--r-border); border-radius: 8px; background: var(--r-main-bg); color: var(--r-text-main); font-family: inherit; font-size: 14px; line-height: 1.6;"></textarea>
            </div>
          </div>
        </div>
        <div class="modal-footer">
            <button class="btn-premium btn-secondary" @click="showInterviewModal = false">{{ $t('profile.security.cancel') }}</button>
            <button class="btn-luxury primary" @click="confirmScheduleInterview" :disabled="!interviewForm.date || !interviewForm.time || sendingInterview">
              {{ sendingInterview ? $t('common.sending') : $t('pipeline.sendInvitation') || "Envoyer l'Invitation" }}
            </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '@/api/axios'
import Sidebar from '@/components/layout/Sidebar.vue'

import { useAuthStore } from '@/stores/authStore'
import { useToastStore } from '@/stores/toastStore'
import { useModalStore } from '@/stores/modalStore'
import { useRecruitmentStore } from '@/stores/recruitmentStore'

export default {
  name: 'CandidateProfile',
  components: { Sidebar },
  data() {
    const authStore = useAuthStore()
    const recruitmentStore = useRecruitmentStore()
    const storedRole = (authStore.userRole || '').toLowerCase()
    const isCompanyOwner = storedRole === 'company' || storedRole === 'admin'
    const accentColor = authStore.themeColors?.accent || (isCompanyOwner ? '#00A7E1' : '#F7C902')
    const accentDark = authStore.themeColors?.accentDark || (isCompanyOwner ? '#0077B6' : '#D4A50E')

    return {
      authStore,
      sidebarCollapsed: false,
      isCompanyOwner,
      accentColor,
      accentDark,
      currentStage: 'submitted',
      activeTab: 'cv',
      recruiterNotes: '',
      tabs: [
        { id: 'cv', label: this.$t('pipeline.candidate.cvTab') || 'Extraits du CV' },
        { id: 'lettre', label: this.$t('pipeline.candidate.motivationTab') || 'Motivation' },
        { id: 'timeline', label: this.$t('pipeline.candidate.historyTab') || 'Historique' },
        { id: 'interview_guide', label: (this.$t('profile.interviewGuideTab') || 'Guide d\'Entretien') + ' ✨' },
        { id: 'comments', label: this.$t('pipeline.candidate.commentsTab') || 'Commentaires' }
      ],
      criteria: [],
      candidate: {
        fullName: 'Chargement...',
        initials: '..',
        jobTitle: '',
        location: '',
        email: '',
        phone: '',
        overallScore: 0,
        aiSummary: '',
        experiences: [],
        education: [],
        skillGroups: [],
        coverLetter: '',
        comments: [],
        interviewQuestions: []
      },
      timeline: [],
      showInterviewModal: false,
      sendingInterview: false,
      isSavingNote: false, // For comment
      isExporting: false,
      recruitmentStore,
      interviewForm: {
        date: '',
        time: '',
        type: 'visio',
        subject: '',
        message: ''
      }
    }
  },
  mounted() {
    this.fetchCandidate()
  },
  watch: {
    currentStage(newVal, oldVal) {
      if (oldVal !== newVal) {
        this.updateStage(newVal)
      }
    }
  },
  computed: {
    sidebarVars() {
      return {}
    },
    avatarStyle() {
      return { background: `linear-gradient(135deg, ${this.accentColor}, ${this.accentDark})` }
    },
    isAiAnalyzed() {
      // The backend may return default/placeholder criteria (all 0%) and
      // an aiSummary like "Analyse en attente..." before actual AI analysis.
      // We detect this as NOT analyzed.
      const summary = (this.candidate.aiSummary || '').trim().toLowerCase()
      const isPendingSummary = !summary || summary.includes('attente') || summary.includes('pending')
      
      // Check if criteria exist AND at least one has a non-zero score
      const hasMeaningfulCriteria = this.criteria && this.criteria.length > 0 
        && this.criteria.some(c => c.score > 0)
      
      // Score > 0 is a definitive sign of analysis
      const hasNonZeroScore = this.candidate.overallScore > 0
      
      return hasNonZeroScore || hasMeaningfulCriteria || !isPendingSummary
    },
    skillsGap() {
      if (!this.candidate.requiredSkills || !this.candidate.skillGroups) return []
      
      const identified = this.candidate.skillGroups.flatMap(g => g.skills || []).map(s => s.toLowerCase().trim())
      const required = this.candidate.requiredSkills.map(s => s.toLowerCase().trim())
      
      const gap = []
      
      // Match & Missing
      this.candidate.requiredSkills.forEach(req => {
        const reqLower = req.toLowerCase().trim()
        const isMatch = identified.some(id => id.includes(reqLower) || reqLower.includes(id))
        gap.push({ name: req, status: isMatch ? 'match' : 'missing', type: 'required' })
      })
      
      // Bonus (Identified but not required)
      this.candidate.skillGroups.flatMap(g => g.skills || []).forEach(id => {
        const idLower = id.toLowerCase().trim()
        const isRequired = required.some(req => idLower.includes(req) || req.includes(idLower))
        if (!isRequired) {
          gap.push({ name: id, status: 'bonus', type: 'identified' })
        }
      })
      
      return gap
    }
  },
  methods: {
    async fetchCandidate() {
      const id = this.$route.params.id
      if (!id) return
      
      try {
        const res = await api.get(`/recruiter/applications/${id}`)
        const data = res.data
        this.candidate = data
        this.criteria = data.criteria
        this.currentStage = data.stage || 'submitted'
        
        // Use timeline from API if available, otherwise fallback
        if (data.timeline && data.timeline.length > 0) {
          this.timeline = data.timeline
        } else {
          this.timeline = [
            { id: 1, title: this.$t('pipeline.received') || 'Candidature Reçue', time: 'Récemment', color: '#94a3b8', desc: this.$t('pipeline.receivedSub') || 'Candidature enregistrée dans le système.', icon: 'submit' }
          ]
        }
      } catch (err) {
        console.error('Erreur chargement candidat:', err)
        this.showToast(this.$t('notifications.error'), 'error')
      }
    },
    async updateStage(stage) {
      const id = this.$route.params.id
      try {
        const res = await api.patch(`/recruiter/applications/${id}/stage`, { stage })
        
        // Update Store Cache if needed (though store fetch might be better)
        this.recruitmentStore.updateApplicationStage(id, stage)
        
        this.showToast(this.$t('notifications.statusUpdated'), 'success')
      } catch (err) {
        console.error('Erreur mise à jour statut:', err)
      }
    },
    async postComment() {
      if (!this.recruiterNotes.trim()) return
      const id = this.$route.params.id
      this.isSavingNote = true
      try {
        const res = await api.post(`/recruiter/applications/${id}/comments`, { content: this.recruiterNotes })
        this.candidate.comments.unshift(res.data)
        this.recruiterNotes = ''
        this.showToast(this.$t('notifications.commentAdded') || 'Commentaire ajouté', 'success')
      } catch (err) {
        console.error('Erreur ajout commentaire:', err)
        this.showToast(this.$t('notifications.updateError'), 'error')
      } finally {
        this.isSavingNote = false
      }
    },
    saveNotes() { this.postComment() },
    async rejectCandidate() { 
      const modalStore = useModalStore()
      const confirmed = await modalStore.confirm({
        title: this.$t('pipeline.actions.reject') + ' ?',
        message: this.$t('pipeline.confirmReject') || 'Cette action placera le candidat dans la liste des refusés.',
        confirmText: this.$t('common.confirm') || 'Confirmer',
        type: 'danger'
      })
      if (confirmed) {
        this.currentStage = 'rejected'
        await this.updateStage('rejected')
      }
    },
    scheduleInterview() { 
      this.interviewForm = { date: '', time: '', type: 'visio', subject: '', message: '' }
      this.showInterviewModal = true
      this.$nextTick(() => this.generateEmailTemplate())
    },
    generateEmailTemplate() {
      const name = this.candidate?.fullName || 'Candidat'
      const job = this.candidate?.jobTitle || 'le poste'
      const typeLabels = { 
        visio: this.$t('pipeline.interview.types.visio') || 'en visioconférence', 
        phone: this.$t('pipeline.interview.types.phone') || 'par téléphone', 
        onsite: this.$t('pipeline.interview.types.onsite') || 'dans nos locaux' 
      }
      const typeLabel = typeLabels[this.interviewForm.type] || typeLabels.visio

      this.interviewForm.subject = this.$t('pipeline.interview.email.subject', { job })

      let details = ''
      if (this.interviewForm.type === 'visio') {
        details = this.$t('pipeline.interview.email.visioDetails')
      } else if (this.interviewForm.type === 'phone') {
        details = this.$t('pipeline.interview.email.phoneDetails')
      } else {
        details = this.$t('pipeline.interview.email.onsiteDetails')
      }

      this.interviewForm.message = this.$t('pipeline.interview.email.body', { name, job, type: typeLabel, details })
    },
    async confirmScheduleInterview() {
      this.sendingInterview = true
      const id = this.$route.params.id
      try {
        await api.post(`/recruiter/applications/${id}/interviews`, {
          date: this.interviewForm.date,
          time: this.interviewForm.time,
          type: this.interviewForm.type,
          subject: this.interviewForm.subject,
          message: this.interviewForm.message
        })
        // Add to local timeline
        this.timeline.unshift({
          id: Date.now(),
          title: this.$t('pipeline.timeline.interviewScheduled'),
          time: `${this.interviewForm.date} \u00e0 ${this.interviewForm.time}`,
          color: this.accentColor,
          desc: this.interviewForm.type === 'visio' ? this.$t('pipeline.timeline.visioSub') : this.$t('pipeline.timeline.interviewScheduledSub')
        })
        this.showInterviewModal = false
        this.currentStage = 'interview'
        this.showToast(this.$t('notifications.interviewSent') || "Invitation d'entretien envoyée avec succès !", 'success')
      } catch (err) {
        console.error('Erreur planification entretien:', err)
        this.showToast(this.$t('notifications.error'), 'error')
      } finally {
        this.sendingInterview = false
      }
    },
    viewCV() { 
      if (this.candidate && this.candidate.resumeUrl) {
        if (this.candidate.resumeUrl.includes('placeholder')) {
          this.showToast(this.$t('notifications.testCV') || 'Test candidate. No real CV.', 'warning')
        } else {
          window.open(this.candidate.resumeUrl, '_blank')
        }
      } else {
        this.showToast(this.$t('notifications.noCV') || 'No CV available.', 'warning')
      }
    },
    showToast(m, type = 'success') { 
      const toastStore = useToastStore()
      toastStore.show(m, type) 
    },
    async downloadPDF() {
      if (this.isExporting) return
      this.isExporting = true
      
      const id = this.$route.params.id
      try {
        const response = await api.get(`/recruiter/applications/${id}/export-pdf`, {
          responseType: 'blob'
        })
        
        const url = window.URL.createObjectURL(new Blob([response.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `Profil_${this.candidate.fullName.replace(/\s+/g, '_')}.pdf`)
        document.body.appendChild(link)
        link.click()
        document.body.removeChild(link)
        
        this.showToast("Rapport PDF généré avec succès !")
      } catch (err) {
        console.error('Erreur export PDF:', err)
        this.showToast("Erreur lors de la génération du PDF", "error")
      } finally {
        this.isExporting = false
      }
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
.profile-grid-container {
  display: grid;
  grid-template-columns: 360px 1fr;
  gap: 32px;
  align-items: flex-start;
}

.side-col {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

/* LEFT SIDE */
.cand-id-card {
  padding: 32px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.cand-avatar-hero {
  width: 64px;
  height: 64px;
  border-radius: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 24px;
  font-weight: 800;
  color: #fff;
  flex-shrink: 0;
  box-shadow: 0 8px 20px rgba(0,0,0,0.1);
}

.id-header {
  display: flex;
  gap: 16px;
  align-items: center;
}

.id-main h3 {
  font-size: 18px;
  font-weight: 800;
  color: var(--r-text-main);
  margin-bottom: 4px;
}

.cand-badge {
  font-size: 11px;
  font-weight: 700;
  color: var(--r-text-sub);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.contact-stack {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.c-item {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 14px;
  color: var(--r-text-sub);
  font-weight: 500;
}

.c-item svg {
  width: 16px;
  color: var(--accent);
}

.status-update-row {
  border-top: 1px solid var(--r-border);
  padding-top: 20px;
  display: flex;
  flex-direction: column; gap: 8px;
}

.status-update-row label {
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
  color: var(--r-text-sub);
}

.select-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.select-wrapper .r-select {
  width: 100%;
  appearance: none;
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  padding: 12px 36px 12px 14px;
  border-radius: 12px;
  font-size: 13px;
  font-weight: 700;
  color: var(--r-text-main);
  outline: none;
  cursor: pointer;
  transition: all 0.2s;
}

.select-wrapper .r-select:hover {
  border-color: var(--accent);
}

.select-arrow {
  position: absolute;
  right: 14px;
  width: 16px;
  height: 16px;
  color: var(--r-text-sub);
  pointer-events: none;
}

/* IA INSIGHT CARD */
.ia-insight-card { padding: 32px; }
.ia-card-head { display: flex; gap: 16px; align-items: center; margin-bottom: 32px; }
.ia-icon-box {
  width: 44px; height: 44px; border-radius: 12px;
  background: var(--accent-soft); color: var(--accent-color);
  display: flex; align-items: center; justify-content: center;
}
.ia-head-text h4 { font-size: 16px; font-weight: 800; color: var(--r-text-main); }
.ia-head-text p { font-size: 12px; color: var(--r-text-sub); }

.ia-score-viz-large { display: flex; justify-content: center; margin-bottom: 32px; }
.score-ring-large { position: relative; width: 140px; height: 140px; }
.score-center { position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); text-align: center; }
.s-val { display: block; font-size: 28px; font-weight: 800; color: var(--r-text-main); }
.s-lab { font-size: 10px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; }

.ia-criteria-stack { display: flex; flex-direction: column; gap: 16px; margin-bottom: 32px; }
.ia-crit-row { display: flex; flex-direction: column; gap: 6px; }
.crit-head { display: flex; justify-content: space-between; }
.crit-name { font-size: 12px; font-weight: 600; color: var(--r-text-sub); }
.crit-score { font-size: 12px; font-weight: 800; color: var(--r-text-main); }
.crit-bar { height: 6px; background: var(--r-border); border-radius: 100px; overflow: visible; }
.crit-fill { height: 100%; border-radius: 100px; transition: width 1s ease; }

.ia-verdict-box {
  background: var(--r-main-bg); border-radius: 16px; padding: 20px;
  border: 1px solid var(--r-border); border-left-width: 4px; border-left-color: var(--accent);
}
.ia-verdict-box h5 { color: var(--accent); font-size: 13px; font-weight: 800; margin-bottom: 8px; }
.ia-verdict-box p { font-size: 13px; color: var(--r-text-main); line-height: 1.6; opacity: 0.9; }

/* SKILLS GAP STYLES */
.skills-gap-analysis {
  margin-bottom: 32px;
  padding: 20px;
  background: var(--r-card-bg);
  border: 1px solid var(--r-border);
  border-radius: 20px;
}

.gap-title {
  font-size: 13px;
  font-weight: 800;
  color: var(--r-text-sub);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 16px;
}

.gap-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.gap-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  border-radius: 10px;
  font-size: 12px;
  font-weight: 700;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: 1px solid transparent;
}

.gap-item.match {
  background: rgba(16, 185, 129, 0.1);
  color: #10b981;
  border-color: rgba(16, 185, 129, 0.2);
}

.gap-item.missing {
  background: rgba(239, 68, 68, 0.1);
  color: #ef4444;
  border-color: rgba(239, 68, 68, 0.2);
  opacity: 0.8;
}

.gap-item.bonus {
  background: rgba(139, 92, 246, 0.1);
  color: #8b5cf6;
  border-color: rgba(139, 92, 246, 0.2);
}

.gap-status-icon {
  width: 14px;
  height: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.gap-status-icon svg {
  width: 100%;
  height: 100%;
}

.gap-item:hover {
  transform: translateY(-2px);
  filter: brightness(1.1);
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}

/* INTERVIEW GUIDE STYLES */
.interview-guide-container {
  padding: 8px;
}

.guide-header {
  margin-bottom: 32px;
}

.guide-badge {
  display: inline-block;
  font-size: 10px;
  font-weight: 800;
  padding: 4px 10px;
  background: var(--accent-grad);
  color: white;
  border-radius: 6px;
  letter-spacing: 1px;
  margin-bottom: 12px;
}

.guide-header h3 {
  font-size: 24px;
  font-weight: 900;
  color: var(--r-text-main);
  margin-bottom: 8px;
}

.guide-header p {
  color: var(--r-text-sub);
  font-size: 14px;
  max-width: 600px;
}

.questions-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  margin-bottom: 40px;
}

.q-card {
  background: var(--r-card-bg);
  border: 1px solid var(--r-border);
  border-radius: 20px;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 0 4px 20px rgba(0,0,0,0.02);
}

.q-card:hover {
  transform: translateY(-5px);
  border-color: var(--accent-color);
  box-shadow: 0 12px 30px rgba(0,0,0,0.08);
}

.q-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.q-category {
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
  color: var(--accent-color);
  background: var(--accent-soft);
  padding: 4px 10px;
  border-radius: 8px;
}

.q-number {
  font-size: 12px;
  font-weight: 800;
  color: var(--r-text-faint);
}

.q-text {
  font-size: 15px;
  font-weight: 700;
  color: var(--r-text-main);
  line-height: 1.6;
  flex: 1;
}

.q-footer {
  padding-top: 16px;
  border-top: 1px solid var(--r-border);
}

.q-purpose {
  font-size: 13px;
  color: var(--r-text-sub);
  font-style: italic;
}

.p-label {
  font-weight: 800;
  color: var(--r-text-main);
  font-style: normal;
}

.guide-pro-tips {
  background: var(--r-main-bg);
  border: 1px dashed var(--r-border);
  border-radius: 20px;
  padding: 24px;
}

.guide-pro-tips h5 {
  font-size: 16px;
  font-weight: 800;
  margin-bottom: 16px;
  color: var(--r-text-main);
}

.guide-pro-tips ul {
  list-style: none;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.guide-pro-tips li {
  font-size: 14px;
  color: var(--r-text-sub);
  position: relative;
  padding-left: 24px;
}

.guide-pro-tips li::before {
  content: '→';
  position: absolute;
  left: 0;
  color: var(--accent-color);
  font-weight: 900;
}

.empty-guide {
  padding: 80px 0;
  text-align: center;
  background: var(--r-card-bg);
  border: 1px dashed var(--r-border);
  border-radius: 32px;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
  filter: grayscale(1) opacity(0.5);
}

.empty-guide p {
  color: var(--r-text-sub);
  font-weight: 600;
}

/* PENDING AI ANALYSIS STATE */
.pending-ring svg { animation: pendingRotate 12s linear infinite; }
@keyframes pendingRotate { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }

.pending-icon-pulse {
  animation: pendingPulse 2s ease-in-out infinite;
}
@keyframes pendingPulse {
  0%, 100% { opacity: 0.5; transform: scale(0.95); }
  50% { opacity: 1; transform: scale(1.05); }
}

.ia-pending-notice {
  display: flex; align-items: flex-start; gap: 12px;
  padding: 18px 20px; border-radius: 16px;
  background: var(--r-main-bg); border: 1px solid var(--r-border);
  border-left-width: 4px; border-left-color: var(--accent-color);
}
.ia-pending-notice p {
  font-size: 13px; color: var(--r-text-sub); line-height: 1.6; margin: 0;
  font-weight: 500;
}

/* TABS */
.premium-tabs-minimal {
  display: flex;
  gap: 32px;
  margin-bottom: 24px;
  border-bottom: 1px solid var(--r-border);
}

.minimal-tab {
  padding: 12px 16px;
  border: none;
  background: transparent;
  font-size: 15px;
  font-weight: 700;
  color: var(--r-text-sub);
  cursor: pointer;
  position: relative;
  transition: all 0.3s;
}

.minimal-tab.active {
  color: var(--r-text-main);
}

.minimal-tab .tab-indicator {
  position: absolute;
  bottom: -1px;
  left: 0;
  right: 0;
  height: 2px;
  background: var(--accent-color);
  border-radius: 2px;
}

.tab-content-panel { min-height: 400px; margin-bottom: 32px; }

.details-section { margin-bottom: 40px; }
.section-title { font-size: 18px; font-weight: 800; color: var(--r-text-main); margin-bottom: 24px; display: flex; align-items: center; gap: 12px; }
.section-title::after { content: ''; height: 2px; flex: 1; background: var(--r-border); }

/* EXP TIMELINE */
.exp-premium-timeline { display: flex; flex-direction: column; gap: 32px; }
.exp-p-item { display: flex; gap: 20px; }
.exp-p-logo { width: 44px; height: 44px; border-radius: 12px; background: var(--accent-dark); color: white; display: flex; align-items: center; justify-content: center; font-weight: 800; flex-shrink: 0; }
.exp-p-body { flex: 1; display: flex; flex-direction: column; gap: 4px; }
.exp-p-head { display: flex; justify-content: space-between; align-items: center; }
.exp-p-title { font-size: 16px; font-weight: 800; color: var(--r-text-main); }
.exp-p-period { font-size: 12px; font-weight: 700; color: var(--r-text-sub); }
.exp-p-company { font-size: 13px; font-weight: 700; color: var(--accent-color); }
.exp-p-desc { margin-top: 8px; font-size: 14px; color: var(--r-text-main); line-height: 1.6; opacity: 0.8; }
.exp-p-tags { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; }
.p-tag { font-size: 11px; font-weight: 700; padding: 4px 10px; background: var(--r-main-bg); border-radius: 6px; color: var(--r-text-sub); border: 1px solid var(--r-border); }

/* EDU GRID */
.edu-p-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
.edu-p-card { display: flex; gap: 16px; align-items: center; padding: 16px; border-radius: 16px; border: 1px solid var(--r-border); }
.edu-p-icon { width: 40px; height: 40px; border-radius: 10px; background: var(--accent-soft); color: var(--accent-color); display: flex; align-items: center; justify-content: center; }
.edu-p-info h6 { font-size: 14px; font-weight: 700; color: var(--r-text-main); }
.edu-p-info p { font-size: 12px; color: var(--r-text-sub); }

/* SKILLS */
.skills-p-layout { display: grid; gap: 20px; }
.skill-p-group label { display: block; font-size: 11px; font-weight: 800; text-transform: uppercase; color: var(--r-text-sub); margin-bottom: 12px; }
.skill-p-row { display: flex; flex-wrap: wrap; gap: 10px; }
.skill-p-pill { padding: 6px 14px; background: var(--r-main-bg); border: 1.5px solid var(--r-border); border-radius: 100px; font-size: 13px; font-weight: 700; color: var(--r-text-main); }

/* MOTIVATION */
.motivation-letter-card { background: var(--r-main-bg); border-radius: 20px; border: 1px solid var(--r-border); padding: 32px; position: relative; }
.letter-inner { position: relative; z-index: 1; line-height: 1.8; color: var(--r-text-main); font-size: 15px; }

/* HISTORY */
.history-premium-list { display: flex; flex-direction: column; gap: 24px; }
.history-p-item { display: flex; gap: 16px; position: relative; }
.history-p-item::before { content: ''; position: absolute; left: 4px; top: 20px; bottom: -20px; width: 1px; background: var(--r-border); }
.history-p-item:last-child::before { display: none; }
.h-dot { width: 10px; height: 10px; border-radius: 50%; margin-top: 6px; position: relative; z-index: 1; }
.h-header { display: flex; justify-content: space-between; margin-bottom: 4px; }
.h-title { font-size: 14px; font-weight: 700; color: var(--r-text-main); }
.h-time { font-size: 12px; color: var(--r-text-sub); }
.h-desc { font-size: 13px; color: var(--r-text-sub); }

/* NOTES */
.notes-premium { padding: 24px; display: flex; flex-direction: column; gap: 16px; }
.notes-header { display: flex; align-items: center; gap: 12px; color: var(--r-text-main); }
.notes-header svg { width: 18px; color: var(--accent-color); }
.notes-header h4 { font-size: 16px; font-weight: 800; }
.notes-premium textarea { width: 100%; padding: 16px; border-radius: 12px; border: 1px solid var(--r-border); background: var(--r-main-bg); color: var(--r-text-main); font-family: inherit; font-size: 14px; outline: none; transition: border-color 0.2s; }
.notes-premium textarea:focus { border-color: var(--accent-color); }
.notes-footer { display: flex; justify-content: space-between; align-items: center; }
.notes-footer .hint { font-size: 12px; color: var(--r-text-sub); font-weight: 500; }

@media (max-width: 1100px) { .profile-grid-container { grid-template-columns: 1fr; } }

/* DISCUSSION THREAD */
.discussion-thread { padding: 32px; }
.comment-list { display: flex; flex-direction: column; gap: 24px; }
.comment-item { display: flex; gap: 16px; }
.comment-avatar {
  width: 40px; height: 40px; border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  color: white; font-weight: 800; font-size: 16px; flex-shrink: 0;
}
.comment-content-box { flex: 1; background: var(--r-main-bg); border-radius: 16px; padding: 16px; border: 1px solid var(--r-border); }
.comment-meta { display: flex; justify-content: space-between; margin-bottom: 6px; }
.comment-author { font-size: 13px; font-weight: 800; color: var(--r-text-main); }
.comment-time { font-size: 11px; font-weight: 600; color: var(--r-text-sub); }
.comment-text { font-size: 14px; color: var(--r-text-main); line-height: 1.6; opacity: 0.9; }

.empty-discussion {
  display: flex; flex-direction: column; align-items: center; gap: 16px;
  padding: 64px 0; color: var(--r-text-sub); text-align: center;
}
.empty-discussion svg { width: 48px; opacity: 0.3; }
.empty-discussion p { font-size: 14px; font-weight: 600; max-width: 300px; }

/* INTERVIEW MODAL */
.modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  animation: fadeIn 0.2s ease-out;
}

.modal-content.premium-modal {
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  border-radius: 20px;
  box-shadow: 0 20px 40px rgba(0,0,0,0.2);
  display: flex;
  flex-direction: column;
  overflow: visible;
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-header {
  padding: 24px 32px;
  border-bottom: 1px solid var(--r-border);
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: var(--r-card-bg);
}

.modal-header h3 {
  font-size: 18px;
  font-weight: 800;
  color: var(--r-text-main);
  margin: 0;
}

.close-btn {
  background: transparent;
  border: none;
  cursor: pointer;
  color: var(--r-text-sub);
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  width: 32px; height: 32px;
  transition: all 0.2s;
}

.close-btn:hover {
  background: var(--r-border);
  color: var(--r-text-main);
}

.close-btn svg {
  width: 20px; height: 20px;
}

.modal-body {
  padding: 32px;
  overflow-y: auto;
  max-height: 70vh;
}

.modal-footer {
  padding: 20px 32px;
  border-top: 1px solid var(--r-border);
  display: flex;
  justify-content: flex-end;
  gap: 16px;
  background: var(--r-card-bg);
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px) scale(0.98); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

</style>

