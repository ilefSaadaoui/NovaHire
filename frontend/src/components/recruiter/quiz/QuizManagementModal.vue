<template>
  <Teleport to="body">
    <div v-if="show" class="modal-overlay" @click.self="$emit('close')">
      <div class="modal-content premium-card anim-reveal-up" :class="userRoleClass">
        <!-- Header -->
        <div class="modal-header">
          <div class="header-main-group">
            <div class="role-indicator role-admin">
              <i class="fas fa-shield-check"></i>
              <span>ADMINISTRATEUR</span>
            </div>
            <div class="title-group">
              <div>
              <h2 style="color: #0EA5E9 !important; font-weight: 800; letter-spacing: -0.5px;">Gestion du Quiz IA</h2>
                <p class="subtitle">{{ jobOffer?.title }} — NovaHire</p>
              </div>
            </div>
          </div>
          <button class="close-btn" @click="$emit('close')">
            <i class="fas fa-times"></i>
          </button>
        </div>

        <!-- Body -->
        <div class="modal-body">
          <!-- No manual email input anymore -->

        <div v-if="loading" class="loading-state">
          <div class="loader-ai"></div>
          <p>L'IA analyse le poste et génère les questions...</p>
        </div>

        <div v-else-if="!quiz" class="config-view">
          <div class="config-header">
            <h3>Personnaliser le Quiz</h3>
            <p>Configurez les paramètres pour que l'IA génère une évaluation sur mesure.</p>
          </div>

            <div class="config-form">
              <div class="form-group">
                <label>Difficulté</label>
                <select v-model="config.difficulty" class="premium-select">
                  <option value="Junior">Débutant (Junior)</option>
                  <option value="Mid">Intermédiaire (Mid)</option>
                  <option value="Senior">Avancé (Senior)</option>
                  <option value="Expert">Expert</option>
                </select>
              </div>
              <div class="form-group">
                <label>Nombre de questions</label>
                <select v-model="config.numQuestions" class="premium-select">
                  <option :value="4">4 questions (Express)</option>
                  <option :value="10">10 questions (Standard)</option>
                  <option :value="20">20 questions (Complet)</option>
                </select>
              </div>

              <div class="form-group full-width">
                <label>Sujets spécifiques (optionnel)</label>
                <input type="text" v-model="config.topicsRaw" 
                       placeholder="Ex: React Hooks, SQL complexe, Management..."
                       class="premium-input" />
                <p class="input-hint">Séparez les sujets par des virgules.</p>
              </div>

              <div class="form-group full-width">
                <label>Langue du Quiz</label>
                <div class="lang-selector">
                  <button v-for="lang in ['fr', 'en']" :key="lang"
                          :class="['lang-btn', { active: config.language === lang }]"
                          @click="config.language = lang">
                    {{ lang.toUpperCase() }}
                  </button>
                </div>
              </div>
            </div>
        </div>

        <div v-else class="quiz-view">
          <div class="quiz-toolbar">
            <div class="quiz-meta">
              <div class="meta-item">
                <i class="fas fa-list-check"></i>
                <span>{{ quiz.questions.length }} Questions</span>
              </div>
              <div class="meta-item">
                <i class="fas fa-clock"></i>
                <span>{{ quiz.timeLimitMinutes }} Minutes</span>
              </div>
            </div>
            <div class="btn-group">
              <button class="btn-action primary-themed" @click="handleInvitationClick" v-if="!isEditing" :disabled="invitingCandidate">
                <i class="fas" :class="invitingCandidate ? 'fa-spinner fa-spin' : 'fa-paper-plane'"></i>
                {{ invitingCandidate ? 'Envoi...' : 'Inviter' }}
              </button>
              <button class="btn-action" @click="copyQuizLink" v-if="!isEditing">
                <i class="fas fa-link"></i>
                {{ copied ? 'Copié !' : 'Lien' }}
              </button>
              <button class="btn-action" @click="generateQuiz" v-if="!isEditing" :disabled="loading">
                <i class="fas fa-sync" :class="{ 'fa-spin': loading }"></i>
                Régénérer
              </button>
              <button class="btn-action" @click="isEditing = !isEditing" :class="{ 'active': isEditing }">
                <i class="fas" :class="isEditing ? 'fa-times' : 'fa-edit'"></i>
                {{ isEditing ? 'Annuler' : 'Éditer' }}
              </button>
              <button v-if="!isEditing" class="btn-action danger" @click="deleteQuiz">
                <i class="fas fa-trash"></i>
              </button>
            </div>
          </div>

          <div class="questions-scroll" :class="{ 'edit-mode': isEditing }">
            <div v-for="(q, idx) in quiz.questions" :key="idx" class="question-card">
              <!-- Mode Lecture -->
              <template v-if="!isEditing">
                <div class="q-badge" :class="q.type.toLowerCase()">{{ q.type === 'Technical' ? 'Technique' : 'Soft Skill' }}</div>
                <h4 class="q-title"><span>{{ idx + 1 }}.</span> {{ q.text }}</h4>
                <div class="options-grid">
                  <div v-for="(opt, oIdx) in q.options" :key="oIdx" 
                       :class="['opt-item', { 'is-correct': oIdx === q.correctAnswerIndex }]">
                    <div class="opt-marker"></div>
                    {{ opt }}
                  </div>
                </div>
                <div class="explanation">
                  <i class="fas fa-info-circle"></i>
                  <span>{{ q.explanation }}</span>
                </div>
              </template>

              <!-- Mode Édition -->
              <template v-else>
                <div class="edit-q-header">
                  <span class="q-num">Question {{ idx + 1 }}</span>
                  <select v-model="q.type" class="edit-select">
                    <option value="Technical">Technical</option>
                    <option value="SoftSkill">SoftSkill</option>
                  </select>
                  <button class="btn-remove-q" @click="removeQuestion(idx)" title="Supprimer la question"><i class="fas fa-trash"></i></button>
                </div>
                
                <input type="text" v-model="q.text" class="edit-input q-text-input" placeholder="Intitulé de la question" />
                
                <div class="edit-options-list">
                  <div v-for="(opt, oIdx) in q.options" :key="oIdx" class="edit-opt-row">
                    <input type="radio" :name="'correct_'+idx" :value="oIdx" v-model="q.correctAnswerIndex" title="Réponse correcte" />
                    <input type="text" v-model="q.options[oIdx]" class="edit-input" placeholder="Option" />
                  </div>
                </div>
                
                <input type="text" v-model="q.explanation" class="edit-input q-exp-input" placeholder="Explication (affichée après réponse)" />
              </template>
            </div>
            
            <div v-if="isEditing" class="add-q-container">
              <button class="btn-add-q" @click="addQuestion">
                <i class="fas fa-plus"></i> Ajouter une question
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- STICKY FOOTER -->
      <div class="modal-footer-sticky" v-if="!loading">
        <template v-if="!quiz">
          <div class="footer-summary">
            <span class="footer-config-badge">
              <i class="fas fa-sliders-h"></i>
              {{ config.numQuestions }} Q. · {{ config.difficulty }} · {{ config.language.toUpperCase() }}
            </span>
          </div>
          <button class="btn-generate-final" @click="generateQuiz" :disabled="loading">
            <i class="fas fa-wand-magic-sparkles"></i>
            Générer le Quiz IA
          </button>
        </template>
        <template v-else-if="isEditing">
          <button class="btn-action" @click="isEditing = false">
            <i class="fas fa-times"></i> Annuler
          </button>
          <button class="btn-action success" @click="saveQuiz" :disabled="saving">
            <i class="fas fa-save"></i>
            {{ saving ? 'Enregistrement...' : 'Enregistrer les modifications' }}
          </button>
        </template>
        <template v-else>
          <div class="footer-summary">
            <span class="quiz-ready-badge">
              <i class="fas fa-check-circle"></i> Quiz IA prêt et configuré
            </span>
          </div>
        </template>
      </div> <!-- modal-footer-sticky -->
    </div> <!-- modal-content -->
  </div> <!-- modal-overlay -->
</Teleport>
</template>

<script>
import axios from '@/api/axios';

export default {
  name: 'QuizManagementModal',
  props: {
    show: Boolean,
    jobOffer: Object,
    applicationId: String
  },
  emits: ['close'],
  data() {
    return {
      quiz: null,
      loading: false,
      copied: false,
      isEditing: false,
      saving: false,
      invitingCandidate: false,
      originalQuizJson: null,
      userRole: 'CompanyAdmin', // Force Admin for your session
      config: {
        numQuestions: 10,
        language: 'fr',
        difficulty: 'Mid',
        topicsRaw: ''
      }
    };
  },
  computed: {
    userRoleClass() {
      const role = this.userRole?.toLowerCase() || 'recruiter';
      return `role-${role}`;
    },
    userRoleLabel() {
      if (this.userRole === 'Admin' || this.userRole === 'CompanyAdmin') return 'Administrateur';
      if (this.userRole === 'SuperAdmin') return 'Super Admin';
      return 'Recruteur';
    },
    userRoleIcon() {
      if (this.userRole?.includes('Admin')) return 'fas fa-shield-halved';
      return 'fas fa-user-tie';
    }
  },
  watch: {
    show(newVal) {
      if (newVal) {
        document.body.classList.add('quiz-open-focus');
        this.detectRole();
        if (this.jobOffer) {
          this.fetchQuiz();
        }
      } else {
        document.body.classList.remove('quiz-open-focus');
      }
    }
  },
  methods: {
    detectRole() {
      this.userRole = 'CompanyAdmin'; // FORCED FOR TEST
      console.log("ROLE FORCÉ À ADMIN POUR TEST");
    },
    async fetchQuiz() {
      try {
        this.loading = true;
        const response = await axios.get(`/quiz/job-offer/${this.jobOffer.id}`);
        this.quiz = response.data;
        this.originalQuizJson = JSON.stringify(this.quiz);
      } catch (err) {
        this.quiz = null;
      } finally {
        this.loading = false;
      }
    },
    async generateQuiz() {
      try {
        this.loading = true;
        const topics = this.config.topicsRaw 
          ? this.config.topicsRaw.split(',').map(t => t.trim()).filter(t => t)
          : [];

        const response = await axios.post('/quiz/generate', {
          jobOfferId: this.jobOffer.id,
          numQuestions: this.config.numQuestions,
          language: this.config.language,
          difficulty: this.config.difficulty,
          topics: topics
        });
        this.quiz = response.data;
      } catch (err) {
        console.error("Erreur génération quiz:", err.response?.data || err);
        const errorMsg = err.response?.data?.message || err.message;
        this.showError('Erreur de génération: ' + errorMsg);
      } finally {
        this.loading = false;
      }
    },
    async regenerateQuiz() {
      if (confirm('Régénérer un nouveau quiz ? Le quiz actuel sera remplacé.')) {
        this.quiz = null;
      }
    },
    async showError(msg) {
      try {
        const { useToastStore } = await import('@/stores/toastStore');
        useToastStore().show(msg, 'error');
      } catch { alert(msg); }
    },
    copyQuizLink() {
      if (!this.jobOffer?.shareToken) {
        if (window.showToast) window.showToast("L'offre doit être publiée pour partager le quiz.", 'warning');
        return;
      }
      const url = `${window.location.origin}/public/quiz/${this.jobOffer.shareToken}?appId=${this.applicationId || ''}`;
      navigator.clipboard.writeText(url);
      this.copied = true;
      setTimeout(() => this.copied = false, 2000);
      if (window.showToast) window.showToast("Lien copié ! Vous pouvez l'envoyer au candidat.", 'success');
    },
    handleInvitationClick() {
      if (this.applicationId) {
        this.sendInvitation();
      } else {
        if (confirm(`Voulez-vous envoyer ce test de présélection à TOUS les candidats de l'offre ?`)) {
          this.sendInvitationToAll();
        }
      }
    },
    async sendInvitation() {
      if (!this.applicationId) return;

      this.invitingCandidate = true;
      try {
        await axios.post(`/recruiter/applications/${this.applicationId}/send-quiz-invitation`);
        if (window.showToast) window.showToast("Invitation envoyée avec succès !", 'success');
      } catch (err) {
        console.error("Erreur envoi invitation:", err);
        if (window.showToast) window.showToast("Erreur lors de l'envoi de l'invitation.", 'error');
      } finally {
        this.invitingCandidate = false;
      }
    },
    async sendInvitationToAll() {
      this.invitingCandidate = true;
      try {
        const res = await axios.post(`/recruiter/job-offers/${this.jobOffer.id}/send-quiz-to-all`);
        if (window.showToast) window.showToast(res.data.message || "Invitations envoyées avec succès !", 'success');
      } catch (err) {
        console.error("Erreur envoi invitations groupées:", err);
        if (window.showToast) window.showToast("Erreur lors de l'envoi des invitations.", 'error');
      } finally {
        this.invitingCandidate = false;
      }
    },
    deleteQuiz() {
       if(confirm("Voulez-vous vraiment supprimer ce quiz ?")) {
           this.quiz = null;
       }
    },
    async saveQuiz() {
      try {
        this.saving = true;
        await axios.put(`/quiz/${this.quiz.id}`, this.quiz);
        this.originalQuizJson = JSON.stringify(this.quiz);
        this.isEditing = false;
      } catch (err) {
        console.error("Erreur sauvegarde quiz:", err);
        alert("Erreur lors de la sauvegarde.");
      } finally {
        this.saving = false;
      }
    },
    addQuestion() {
      this.quiz.questions.push({
        text: '',
        type: 'Technical',
        options: ['', '', '', ''],
        correctAnswerIndex: 0,
        explanation: ''
      });
    },
    removeQuestion(idx) {
      this.quiz.questions.splice(idx, 1);
    }
  }
};
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0; left: 0; width: 100vw; height: 100vh;
  background: rgba(15, 23, 42, 0.75);
  backdrop-filter: blur(12px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2147483647;
  padding: 20px;
}
</style>

<style>
/* Global style to force hide sidebar when quiz modal is open */
body.quiz-open-focus aside, 
body.quiz-open-focus .sidebar,
body.quiz-open-focus .app-sidebar,
body.quiz-open-focus .nav-container {
  display: none !important;
  opacity: 0 !important;
  visibility: hidden !important;
}
</style>

<style scoped>

.modal-content {
  width: 95%;
  max-width: 1200px;
  height: 90vh; /* Increased to use more space */
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  border-radius: 30px;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.6);
}

/* Role Based Accents */
.modal-content.role-admin, .modal-content.role-companyadmin { border-top: 5px solid #00A7E1; }
.modal-content.role-superadmin { border-top: 5px solid #8b5cf6; }
.modal-content.role-recruiter { border-top: 5px solid #FFD700; }

.modal-header {
  padding: 24px 32px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--r-border);
}

.header-main-group {
  display: flex;
  align-items: center;
  gap: 32px;
}

.role-indicator {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 18px;
  border-radius: 100px;
  font-size: 0.8rem;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.8px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.05);
}

.role-indicator.role-admin, .role-indicator.role-companyadmin { background: rgba(14, 165, 233, 0.15); color: #0EA5E9; border: 1px solid rgba(14, 165, 233, 0.2); }
.role-indicator.role-recruiter { background: rgba(255, 215, 0, 0.15); color: #FFD700; border: 1px solid rgba(255, 215, 0, 0.2); }
.role-indicator.role-superadmin { background: rgba(139, 92, 246, 0.15); color: #8b5cf6; border: 1px solid rgba(139, 92, 246, 0.2); }

.title-group {
  display: flex;
  align-items: center;
  gap: 16px;
}

.icon-box-new {
  width: 58px;
  height: 58px;
  background: rgba(14, 165, 233, 0.05);
  border-radius: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #0EA5E9;
  font-size: 1.8rem;
  transition: 0.3s;
}

.role-recruiter .icon-box-new { 
  background: rgba(255, 215, 0, 0.05);
  color: #FFD700;
}
.icon-box-new.role-superadmin { 
  background: rgba(139, 92, 246, 0.1); 
  border-color: #8b5cf6; 
  color: #8b5cf6; 
}

.modal-header h2 { margin: 0; font-size: 1.25rem; color: var(--r-text-main); }
.subtitle { margin: 0; font-size: 0.9rem; color: var(--r-text-sub); }

.close-btn {
  background: none; border: none; color: var(--r-text-sub);
  font-size: 1.2rem; cursor: pointer; transition: 0.2s;
}
.close-btn:hover { color: var(--r-text-main); transform: rotate(90deg); }

.modal-body {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.modal-body::-webkit-scrollbar { width: 6px; }
.modal-body::-webkit-scrollbar-thumb { background: var(--r-border); border-radius: 10px; }

/* Config View Grid */
.config-view {
  max-width: 900px;
  margin: 0 auto;
  width: 100%;
}

.config-header {
  text-align: center;
  margin-bottom: 40px;
}
.config-header h3 { margin: 0; font-size: 1.5rem; font-weight: 800; color: var(--r-text-main); }
.config-header p { margin: 10px 0 0 0; color: var(--r-text-sub); font-size: 1rem; }

.config-form {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 32px;
  margin-top: 20px;
}

.form-group.full-width {
  grid-column: span 2;
}

.empty-state {
  text-align: center;
  padding: 40px;
}

.ai-circle-small {
  width: 70px; height: 70px;
  margin: 0 auto 20px;
  background: rgba(14, 165, 233, 0.08);
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  font-size: 1.8rem; color: #0EA5E9;
  border: 2px dashed #0EA5E9;
  animation: spin-slow 12s linear infinite;
}

.role-recruiter .ai-circle-small {
  background: rgba(255, 215, 0, 0.08);
  color: #FFD700;
  border-color: #FFD700;
}
@keyframes spin-slow { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }



.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-group label {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--r-text-sub);
}

.premium-select, .premium-input {
  background: var(--r-main-bg);
  border: 1px solid var(--r-border);
  color: var(--r-text-main);
  padding: 10px 14px;
  border-radius: 10px;
  font-size: 0.9rem;
  transition: 0.2s;
}

.premium-select:focus, .premium-input:focus {
  border-color: #FFD700;
  outline: none;
}

.input-hint {
  font-size: 0.75rem;
  color: var(--r-text-sub);
  margin: 0;
  font-style: italic;
}

.lang-selector {
  display: flex;
  gap: 8px;
}

.lang-btn {
  flex: 1;
  padding: 8px;
  background: rgba(128, 128, 128, 0.1);
  border: 1px solid var(--r-border);
  color: var(--r-text-sub);
  border-radius: 8px;
  cursor: pointer;
  font-weight: 600;
  transition: 0.2s;
}

.lang-btn.active {
  background: rgba(14, 165, 233, 0.1) !important;
  border-color: #0EA5E9 !important;
  color: #0EA5E9 !important;
}

.role-recruiter .lang-btn.active {
  background: rgba(255, 215, 0, 0.1) !important;
  border-color: #FFD700 !important;
  color: #FFD700 !important;
}

/* Sticky Footer */
.modal-footer-sticky {
  padding: 16px 24px;
  border-top: 1px solid var(--r-border);
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
  flex-shrink: 0;
  background: var(--r-main-bg);
  border-radius: 0 0 24px 24px;
}

.footer-summary {
  flex: 1;
}

.footer-config-badge {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: rgba(14, 165, 233, 0.1) !important;
  border: 1px solid rgba(14, 165, 233, 0.3) !important;
  border-radius: 10px;
  font-size: 0.85rem;
  font-weight: 700;
  color: #0EA5E9 !important;
}

.role-recruiter .footer-config-badge {
  background: rgba(255, 215, 0, 0.08) !important;
  border-color: rgba(255, 215, 0, 0.2) !important;
  color: #FFB300 !important;
}

.btn-generate-final {
  padding: 16px 50px;
  background: linear-gradient(to bottom, #38BDF8, #0EA5E9) !important; /* Pure Sky Blue Gradient */
  color: #fff !important;
  border: none;
  border-radius: 16px;
  font-weight: 800;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  transition: 0.4s cubic-bezier(0.4, 0, 0.2, 1);
  font-size: 1.1rem;
  box-shadow: 0 8px 25px rgba(14, 165, 233, 0.3) !important;
}

/* Specific override for Recruiter */
.role-recruiter .btn-generate-final { 
  background: linear-gradient(135deg, #FFD700, #FFA500) !important; 
  color: #000 !important;
  box-shadow: 0 8px 25px rgba(255, 215, 0, 0.3) !important;
}

.btn-generate-final:hover:not(:disabled) {
  transform: translateY(-4px) scale(1.02);
  box-shadow: 0 12px 30px rgba(14, 165, 233, 0.4);
}

.btn-generate-final:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.loading-sub {
  font-size: 0.8rem;
  color: var(--r-text-faint);
  margin-top: 4px;
}

/* Loading AI */
.loader-ai {
  width: 50px; height: 50px;
  border: 3px solid rgba(255, 215, 0, 0.1);
  border-top-color: #FFD700;
  border-radius: 50%;
  animation: spin 1s infinite linear;
  margin: 0 auto 20px;
}

@keyframes spin { to { transform: rotate(360deg); } }

/* Quiz View */
.quiz-view {
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
}

.quiz-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px 25px;
  background: rgba(128, 128, 128, 0.03);
  border-radius: 16px;
  margin-bottom: 25px;
  border: 1px solid var(--r-border);
}

.quiz-meta { display: flex; gap: 25px; }
.meta-item { 
  display: flex; 
  align-items: center; 
  gap: 10px; 
  color: var(--r-text-main); 
  font-size: 0.95rem; 
  font-weight: 600;
}
.meta-item i { color: #0EA5E9; font-size: 1.1rem; }

.btn-group { display: flex; gap: 8px; }
.btn-action {
  background: rgba(128,128,128,0.05); border: 1px solid var(--r-border);
  color: var(--r-text-main); padding: 8px 16px; border-radius: 8px; cursor: pointer;
  display: flex; align-items: center; gap: 8px; font-size: 0.85rem;
  transition: 0.2s;
}
.btn-action:hover { background: rgba(128,128,128,0.1); }
.btn-action.danger:hover { color: #ff4444; border-color: #ff4444; }

/* Questions List */
.questions-scroll {
  overflow-y: auto;
  flex: 1;
  padding-right: 8px;
}

.questions-scroll::-webkit-scrollbar { width: 6px; }
.questions-scroll::-webkit-scrollbar-thumb { background: var(--r-border); border-radius: 10px; }

.questions-list {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 20px;
  padding: 10px 0;
}

.question-item {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  padding: 24px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  flex-direction: column;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.question-item:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 20px -5px rgba(0, 0, 0, 0.1);
  border-color: #0EA5E9;
}

.q-badge {
  display: inline-flex;
  padding: 5px 12px;
  border-radius: 8px;
  font-size: 0.75rem;
  text-transform: uppercase;
  font-weight: 800;
  margin-bottom: 15px;
  letter-spacing: 0.5px;
  width: fit-content;
}
.q-badge.technical { background: rgba(14, 165, 233, 0.1); color: #0EA5E9; }
.q-badge.softskill { background: rgba(16, 185, 129, 0.1); color: #10b981; }

.q-title { 
  margin: 0 0 20px 0; 
  font-size: 1.1rem; 
  color: #1e293b; 
  line-height: 1.5; 
  font-weight: 700;
}
.q-title span { color: #0EA5E9; margin-right: 12px; font-weight: 900; }
.role-recruiter .q-title span { color: #FFD700; }

.options-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 10px;
  margin-bottom: 16px;
}

.opt-item {
  background: #f8fafc;
  padding: 12px 16px;
  border-radius: 12px;
  font-size: 0.95rem;
  color: #475569;
  display: flex;
  align-items: center;
  gap: 12px;
  border: 1px solid #e2e8f0;
  transition: 0.2s;
}

.is-correct {
  background: rgba(16, 185, 129, 0.05);
  border-color: #10b981;
  color: #059669;
  font-weight: 600;
}

.opt-marker {
  width: 10px; height: 10px;
  border-radius: 50%;
  background: #cbd5e1;
}
.is-correct .opt-marker { background: #10b981; box-shadow: 0 0 12px rgba(16, 185, 129, 0.4); }

.explanation {
  background: rgba(14, 165, 233, 0.03);
  border-left: 3px solid #0EA5E9;
  padding: 10px 14px;
  font-size: 0.85rem;
  color: #0EA5E9;
  display: flex;
  gap: 10px;
  line-height: 1.4;
}

.role-recruiter .explanation {
  background: rgba(255, 215, 0, 0.03);
  border-left: 3px solid #FFD700;
  color: #FFB300;
}

.email-input-bar {
  display: flex;
  gap: 12px;
  background: #f0f9ff;
  padding: 15px 25px;
  border-radius: 16px;
  margin-bottom: 20px;
  border: 1px solid #bae6fd;
  align-items: center;
  animation: slideDown 0.3s ease-out;
}

.email-input-bar input {
  flex: 1;
  background: #fff;
  border: 1px solid #cbd5e1;
  padding: 10px 16px;
  border-radius: 10px;
  font-size: 0.95rem;
  outline: none;
}

.btn-send-manual {
  background: #0EA5E9;
  color: #fff;
  border: none;
  padding: 10px 20px;
  border-radius: 10px;
  font-weight: 700;
  cursor: pointer;
}

.btn-cancel-manual {
  background: transparent;
  color: #64748b;
  border: none;
  cursor: pointer;
  padding: 5px;
}

@keyframes slideDown {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}
.edit-q-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 12px;
}
.q-num { font-weight: 800; font-size: 0.9rem; color: #FFD700; }
.edit-select {
  background: var(--r-main-bg); border: 1px solid var(--r-border);
  color: var(--r-text-main); border-radius: 6px; padding: 4px 8px; font-size: 0.8rem;
}
.btn-remove-q {
  margin-left: auto; background: none; border: none; color: #ff4444; cursor: pointer;
  opacity: 0.6; transition: 0.2s;
}
.btn-remove-q:hover { opacity: 1; transform: scale(1.1); }

.edit-input {
  width: 100%; background: var(--r-main-bg); border: 1px solid var(--r-border);
  color: var(--r-text-main); border-radius: 8px; padding: 10px 12px; font-size: 0.9rem;
  transition: 0.2s; font-family: inherit;
}
.edit-input:focus { outline: none; border-color: #FFD700; background: rgba(128, 128, 128, 0.05); }

.q-text-input { font-weight: 700; font-size: 1rem; margin-bottom: 16px; }

.edit-options-list {
  display: flex; flex-direction: column; gap: 8px; margin-bottom: 16px;
}
.edit-opt-row {
  display: flex; align-items: center; gap: 12px;
}
.edit-opt-row input[type="radio"] {
  accent-color: #A5D6A7; width: 16px; height: 16px; cursor: pointer;
}

.q-exp-input {
  border-left: 3px solid #FFD700; background: rgba(255, 215, 0, 0.02);
}

.add-q-container {
  display: flex; justify-content: center; margin-top: 10px; padding-bottom: 20px;
}
.btn-add-q {
  background: rgba(128, 128, 128, 0.05); border: 1px dashed var(--r-border);
  color: var(--r-text-main); padding: 12px 24px; border-radius: 12px; font-weight: 600; cursor: pointer;
  transition: 0.2s; display: flex; align-items: center; gap: 8px;
}
.btn-add-q:hover { background: rgba(128, 128, 128, 0.1); border-color: #FFD700; color: #FFD700; }
.btn-action.success { background: rgba(165, 214, 167, 0.1); color: #A5D6A7; border-color: #A5D6A7; }
.btn-action.success:hover { background: rgba(165, 214, 167, 0.2); }
.btn-action.active { background: rgba(128, 128, 128, 0.2); border-color: var(--r-text-main); }
</style>
