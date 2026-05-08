<template>
  <div class="public-quiz-page" :style="pageTheme">
    <!-- Animated Background Elements -->
    <div class="animated-bg">
      <div class="bg-blob bg-blob-1"></div>
      <div class="bg-blob bg-blob-2"></div>
      <div class="bg-blob bg-blob-3"></div>
      <div class="glass-overlay"></div>
    </div>

    <!-- Proctoring Warning Overlay with Enhanced Animation -->
    <transition name="warning-modal">
      <div v-if="showWarning" class="proctor-warning-overlay">
        <div class="warning-card premium-card anim-reveal-up">
          <div class="warning-icon">
            <i class="fas fa-exclamation-triangle"></i>
          </div>
          <h3>Oups ! Vous avez quitté la page</h3>
          <p>Pour garantir l'équité du test, merci de rester sur cet onglet.<br><span style="color: #94a3b8; font-size: 0.9em;">Les recruteurs sont informés en cas de sorties répétées.</span></p>
          <p class="warning-count">Avertissement ({{ warningCount }})</p>
          <button class="celestial-btn-nav primary" style="width: 100%; justify-content: center;" @click="showWarning = false">
            <span>Retourner au test</span>
            <i class="fas fa-arrow-right"></i>
          </button>
        </div>
      </div>
    </transition>

    <div class="quiz-container" :class="{ 'is-result-phase': phase === 'result', 'is-active-phase': phase === 'active' }">
      <!-- Error State with Better Visual Feedback -->
      <transition name="fade" mode="out-in">
        <div v-if="errorMsg && !loading" class="quiz-card premium-card anim-reveal-up" style="text-align:center;">
          <div class="error-icon">
            <i class="fas fa-exclamation-triangle"></i>
          </div>
          <h2 class="quiz-title" style="font-size: 2rem; margin-bottom:12px;">Quiz indisponible</h2>
          <p class="error-message">{{ errorMsg }}</p>
          <button class="btn-start-quiz" @click="$router.push('/')" style="width: auto; padding: 16px 40px; margin: 0 auto;">
            <i class="fas fa-home"></i> Retour à l'accueil
          </button>
        </div>

        <!-- Enhanced Loading State -->
        <div v-else-if="loading" class="loading-wrap">
          <div class="ai-loader">
            <div class="loader-ring"></div>
            <div class="loader-ring"></div>
            <div class="loader-ring"></div>
          </div>
          <p>Préparation de votre session d'évaluation...</p>
          <span class="loader-subtle">Sécurisation du test en cours</span>
        </div>

        <!-- Phase 1: Welcome with Enhanced Design -->
        <div v-else-if="phase === 'welcome'" class="quiz-card premium-card anim-reveal-up">
          <div class="company-badge-premium anim-stagger-1" v-if="offer?.company">
            <img v-if="offer.companyLogo" :src="offer.companyLogo" class="mini-logo" />
            <span>{{ offer.company }}</span>
          </div>
          
          <h1 class="quiz-title anim-stagger-2">Test de Présélection</h1>
          <p class="quiz-subtitle-premium anim-stagger-3">Poste : <strong>{{ offer?.title }}</strong></p>
          
          <div class="info-grid anim-stagger-4">
            <div class="info-card primary-info">
              <div class="info-icon blue-icon glow-pulse">
                <i class="fas fa-hourglass-half"></i>
              </div>
              <div class="info-text">
                <span class="info-label">Temps imparti</span>
                <span class="info-val">{{ quiz?.timeLimitMinutes || 10 }} min</span>
              </div>
            </div>
            <div class="info-card secondary-info">
              <div class="info-icon violet-icon glow-pulse">
                <i class="fas fa-brain"></i>
              </div>
              <div class="info-text">
                <span class="info-label">Questions</span>
                <span class="info-val">{{ quiz?.questions?.length || 0 }}</span>
              </div>
            </div>
          </div>

          <div class="warning-box anim-stagger-5">
            <div class="warning-icon-wrapper"><i class="fas fa-shield-alt"></i></div>
            <p>Ce test est <strong>sécurisé</strong>. Évitez de changer d'onglet ou de minimiser la fenêtre pour éviter l'invalidation de votre session.</p>
          </div>

          <button class="btn-start-quiz anim-stagger-6" @click="startQuiz">
            <span>{{ sessionRestored ? 'Reprendre le test' : 'Démarrer l\'évaluation' }}</span>
            <i class="fas fa-arrow-right"></i>
          </button>
        </div>

        <!-- Phase 2: Active Questions with Page Transitions -->
        <div v-else-if="phase === 'active'" class="quiz-active-wrap">
          <div class="celestial-header">
            <div class="progress-bar-wrap-celestial">
              <div class="progress-fill-celestial" :style="{ width: progressPercent + '%' }">
                <div class="progress-glow"></div>
              </div>
            </div>
            <div class="quiz-meta-bar">
              <span class="q-counter">
                <i class="fas fa-layer-group"></i>
                Page {{ currentPage + 1 }} sur {{ totalPages }}
                <span class="answered-badge">{{ answeredCount }}/{{ quiz?.questions?.length || 0 }} répondues</span>
              </span>
              <div class="timer-celestial" :class="{ 'timer-critical': timeLeft < 60 }">
                <i class="fas fa-stopwatch timer-icon"></i>
                <span>{{ formatTime(timeLeft) }}</span>
              </div>
            </div>
          </div>

          <!-- Page Transition Wrapper -->
          <transition name="page-transition" mode="out-in">
            <div :key="currentPage" class="questions-grid">
              <div v-for="(q, idx) in currentPageQuestions" :key="pageOffset + idx" 
                   class="celestial-q-card anim-scale-up"
                   :style="{ animationDelay: (idx * 0.08) + 's' }">
                <div class="q-header-row">
                  <div class="q-badge-group">
                    <span class="q-number">Question {{ pageOffset + idx + 1 }}</span>
                    <span class="q-type-tag" :class="q.type?.toLowerCase()">
                      <i :class="q.type === 'Technical' ? 'fas fa-code' : 'fas fa-users'"></i>
                      {{ q.type === 'Technical' ? 'Technique' : 'Soft Skill' }}
                    </span>
                  </div>
                  <div class="q-status" v-if="answers[pageOffset + idx] !== -1">
                    <i class="fas fa-check-circle"></i>
                    <span>Répondue</span>
                  </div>
                </div>
                <h3 class="q-text-light">{{ q.text }}</h3>
                <div class="options-stack">
                  <div v-for="(opt, oIdx) in q.options" :key="oIdx"
                       class="celestial-option"
                       :class="{ 'is-selected': answers[pageOffset + idx] === oIdx }"
                       tabindex="0"
                       @click="selectOptionAt(pageOffset + idx, oIdx)"
                       @keydown.enter.prevent="selectOptionAt(pageOffset + idx, oIdx)"
                       @keydown.space.prevent="selectOptionAt(pageOffset + idx, oIdx)">
                    <div class="opt-letter-light">{{ String.fromCharCode(65 + oIdx) }}</div>
                    <span class="opt-text">{{ opt.replace(/^[A-Z]\)\s*/, '') }}</span>
                    <div class="option-check" v-if="answers[pageOffset + idx] === oIdx">
                      <i class="fas fa-check"></i>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </transition>

          <div class="quiz-footer-nav">
            <button class="celestial-btn-nav" :disabled="currentPage === 0" @click="currentPage--">
              <i class="fas fa-arrow-left"></i> Précédent
            </button>
            <div class="page-dots" role="navigation" aria-label="Pagination">
              <span v-for="p in totalPages" :key="p" 
                    class="dot-celestial" :class="{ active: p - 1 === currentPage }"
                    tabindex="0"
                    role="button"
                    :aria-label="'Aller à la page ' + p"
                    @click="currentPage = p - 1"
                    @keydown.enter.prevent="currentPage = p - 1"
                    @keydown.space.prevent="currentPage = p - 1"></span>
            </div>
            <button v-if="currentPage < totalPages - 1" class="celestial-btn-nav primary" @click="currentPage++">
              Suivant <i class="fas fa-arrow-right"></i>
            </button>
            <button v-else class="celestial-btn-nav primary" @click="submitQuiz" :disabled="submitting">
              <i class="fas" :class="submitting ? 'fa-spinner fa-spin' : 'fa-paper-plane'"></i>
              {{ submitting ? 'Envoi...' : 'Terminer le test' }}
            </button>
          </div>
        </div>

        <!-- Phase 3: Dashboard Centralisé Candidat -->
        <div v-else-if="phase === 'result'" class="quiz-card result-dashboard-celestial premium-card anim-reveal-up">
          
          <div class="dashboard-header candidate-header">
            <div class="success-icon-celestial candidate-success">
              <i class="fas fa-check-circle"></i>
            </div>
            <h2 class="celestial-title">Évaluation Terminée !</h2>
            <p class="candidate-subtitle">Félicitations, vous êtes arrivé(e) au bout de l'évaluation.</p>
          </div>
          
          <div class="dashboard-body candidate-body">
            <div class="info-box-celestial primary-box">
              <div class="box-icon"><i class="fas fa-paper-plane"></i></div>
              <div class="box-text">
                <strong>Résultats transmis</strong>
                <span>Vos réponses ont été envoyées en toute sécurité à l'équipe de recrutement de <strong>{{ offer?.company || 'notre partenaire' }}</strong>.</span>
              </div>
            </div>

            <div class="info-box-celestial secondary-box">
              <div class="box-icon"><i class="fas fa-clock"></i></div>
              <div class="box-text">
                <strong>Prochaines étapes</strong>
                <span>Le recruteur analysera votre profil et vous contactera très prochainement pour la suite du processus.</span>
              </div>
            </div>
          </div>
          
          <div class="dashboard-footer candidate-footer">
            <button class="btn-premium-action" @click="$router.push('/')">
              <i class="fas fa-home"></i> <span>Retourner à l'accueil</span>
            </button>
          </div>
        </div>
      </transition>
    </div>
  </div>
</template>

<script>
import api from '@/api/axios';

export default {
  name: 'PublicQuiz',
  data() {
    return {
      loading: true,
      phase: 'welcome',
      offer: null,
      quiz: null,
      currentPage: 0,
      questionsPerPage: 4,
      answers: [],
      timeLeft: 0,
      timerInterval: null,
      submitting: false,
      finalScore: 0,
      quizReviews: [],
      applicationId: null,
      errorMsg: null,
      warningCount: 0,
      showWarning: false,
      isWindowFocused: true,
      sessionRestored: false
    };
  },
  computed: {
    pageTheme() {
      if (!this.offer) return {};
      return {
        '--primary': this.offer.pageColor || '#00A7E1',
        '--secondary': this.offer.pageSecondaryColor || '#fcd34d'
      };
    },
    totalPages() {
      if (!this.quiz?.questions) return 1;
      return Math.ceil(this.quiz.questions.length / this.questionsPerPage);
    },
    pageOffset() {
      return this.currentPage * this.questionsPerPage;
    },
    currentPageQuestions() {
      if (!this.quiz?.questions) return [];
      return this.quiz.questions.slice(this.pageOffset, this.pageOffset + this.questionsPerPage);
    },
    progressPercent() {
      if (!this.quiz?.questions) return 0;
      return ((this.currentPage + 1) / this.totalPages) * 100;
    },
    answeredCount() {
      return this.answers.filter(a => a !== -1).length;
    },
    timerRingDash() {
      const circumference = 2 * Math.PI * 14;
      const progress = this.timeLeft / ((this.quiz?.timeLimitMinutes || 10) * 60);
      return `${circumference * progress} ${circumference}`;
    }
  },
  async mounted() {
    const token = this.$route.params.token;
    this.applicationId = this.$route.query.appId || localStorage.getItem('last_application_id');
    await this.fetchOfferAndQuiz(token);
    this.setupProctoring();
    this.restoreSession();
  },
  methods: {
    async fetchOfferAndQuiz(token) {
      try {
        this.loading = true;
        const offerRes = await api.get(`/public/offers/${token}`);
        this.offer = offerRes.data;

        const quizRes = await api.get(`/Quiz/job-offer/${this.offer.id}`);
        this.quiz = quizRes.data;
        this.timeLeft = (this.quiz.timeLimitMinutes || 10) * 60;
        this.answers = new Array(this.quiz.questions.length).fill(-1);
      } catch (err) {
        console.error("Erreur chargement quiz:", err);
        if (err.response?.status === 404) {
          this.errorMsg = "Ce quiz n'est plus accessible ou l'offre n'est pas publiée.";
        } else {
          this.errorMsg = "Erreur de connexion au serveur. Vérifiez votre connexion.";
        }
      } finally {
        this.loading = false;
      }
    },
    setupProctoring() {
      window.addEventListener('blur', () => {
        if (this.phase === 'active') {
          this.isWindowFocused = false;
          this.warningCount++;
          this.showWarning = true;
          this.saveSession();
        }
      });
      window.addEventListener('focus', () => {
        this.isWindowFocused = true;
      });
    },
    restoreSession() {
      const saved = localStorage.getItem(`quiz_session_${this.applicationId}`);
      if (saved) {
        try {
          const data = JSON.parse(saved);
          if (data.quizId === this.quiz?.id) {
            this.answers = data.answers;
            this.timeLeft = data.timeLeft;
            this.currentPage = data.currentPage || 0;
            this.warningCount = data.warningCount || 0;
            this.sessionRestored = true;
          }
        } catch (e) {
          console.error("Erreur restauration session:", e);
        }
      }
    },
    saveSession() {
      if (!this.applicationId || !this.quiz) return;
      const data = {
        quizId: this.quiz.id,
        answers: this.answers,
        timeLeft: this.timeLeft,
        currentPage: this.currentPage,
        warningCount: this.warningCount,
        timestamp: Date.now()
      };
      localStorage.setItem(`quiz_session_${this.applicationId}`, JSON.stringify(data));
    },
    clearSession() {
      localStorage.removeItem(`quiz_session_${this.applicationId}`);
    },
    startQuiz() {
      this.phase = 'active';
      this.startTimer();
    },
    startTimer() {
      if (this.timerInterval) clearInterval(this.timerInterval);
      this.timerInterval = setInterval(() => {
        if (this.timeLeft > 0) {
          this.timeLeft--;
          if (this.timeLeft % 5 === 0) this.saveSession();
        } else {
          this.submitQuiz();
        }
      }, 1000);
    },
    formatTime(seconds) {
      const m = Math.floor(seconds / 60);
      const s = seconds % 60;
      return `${m}:${s < 10 ? '0' : ''}${s}`;
    },
    selectOptionAt(questionIdx, optionIdx) {
      this.answers[questionIdx] = optionIdx;
      this.answers = [...this.answers];
      this.saveSession();
    },
    scrollToQuestion(questionIndex) {
      // Feature: Scroll to the question in active mode (if needed)
      const page = Math.floor(questionIndex / this.questionsPerPage);
      if (this.phase === 'active') {
        this.currentPage = page;
      }
    },
    async submitQuiz() {
      if (this.submitting) return;
      clearInterval(this.timerInterval);
      
      try {
        this.submitting = true;
        
        if (!this.applicationId) {
          this.finalScore = 0;
          this.phase = 'result';
          this.clearSession();
          return;
        }

        const response = await api.post(`/Quiz/submit`, {
          jobApplicationId: this.applicationId,
          quizId: this.quiz.id,
          answers: this.answers,
          metadata: JSON.stringify({ warnings: this.warningCount })
        });

        this.finalScore = response.data.score;
        this.quizReviews = response.data.reviews || [];
        this.phase = 'result';
        this.clearSession();
      } catch (err) {
        console.error("Erreur soumission quiz:", err);
        this.errorMsg = err.response?.data?.message || "Une erreur est survenue lors de l'enregistrement de votre test.";
      } finally {
        this.submitting = false;
      }
    }
  },
  beforeUnmount() {
    if (this.timerInterval) clearInterval(this.timerInterval);
    this.saveSession();
  }
};
</script>

<style scoped>
/* Variables & Base */
.public-quiz-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  position: relative;
  overflow-x: hidden;
}

/* Animated Background with Blobs */
.animated-bg {
  position: fixed;
  inset: 0;
  z-index: 0;
  overflow: hidden;
}

.bg-blob {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  opacity: 0.4;
  animation: floatBlob 20s infinite ease-in-out;
}

.bg-blob-1 {
  width: 500px;
  height: 500px;
  background: radial-gradient(circle, rgba(14, 165, 233, 0.4) 0%, rgba(14, 165, 233, 0) 70%);
  top: -150px;
  left: -150px;
  animation-delay: 0s;
}

.bg-blob-2 {
  width: 600px;
  height: 600px;
  background: radial-gradient(circle, rgba(99, 102, 241, 0.3) 0%, rgba(99, 102, 241, 0) 70%);
  bottom: -200px;
  right: -200px;
  animation-delay: 5s;
}

.bg-blob-3 {
  width: 400px;
  height: 400px;
  background: radial-gradient(circle, rgba(245, 158, 11, 0.25) 0%, rgba(245, 158, 11, 0) 70%); /* Yellow */
  top: 40%;
  left: 30%;
  animation-delay: 10s;
}

.glass-overlay {
  position: absolute;
  inset: 0;
  background: rgba(248, 250, 252, 0.7);
  backdrop-filter: blur(2px);
}

@keyframes floatBlob {
  0%, 100% { transform: translate(0, 0) scale(1); }
  33% { transform: translate(30px, -30px) scale(1.1); }
  66% { transform: translate(-20px, 20px) scale(0.9); }
}

/* Container */
.quiz-container {
  width: 100%;
  max-width: 650px;
  position: relative;
  z-index: 10;
  transition: max-width 0.5s cubic-bezier(0.4, 0, 0.2, 1);
}

.quiz-container.is-active-phase {
  max-width: 900px;
}

.quiz-container.is-result-phase {
  max-width: 600px;
}

/* Premium Card */
.premium-card {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(20px);
  border-radius: 32px;
  padding: 40px;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.15), 0 0 0 1px rgba(255, 255, 255, 0.5);
  transition: all 0.3s ease;
}

/* Animations */
.anim-reveal-up {
  animation: revealUp 0.7s cubic-bezier(0.2, 0, 0.2, 1) both;
}

.anim-scale-up {
  animation: scaleUp 0.5s cubic-bezier(0.2, 0, 0.2, 1) both;
}

@keyframes revealUp {
  from { opacity: 0; transform: translateY(30px); }
  to { opacity: 1; transform: translateY(0); }
}

@keyframes scaleUp {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

/* Page Transition */
.page-transition-enter-active,
.page-transition-leave-active {
  transition: all 0.4s ease;
}

.page-transition-enter-from {
  opacity: 0;
  transform: translateX(20px);
}

.page-transition-leave-to {
  opacity: 0;
  transform: translateX(-20px);
}

/* Warning Modal Animation */
.warning-modal-enter-active,
.warning-modal-leave-active {
  transition: all 0.3s ease;
}

.warning-modal-enter-from,
.warning-modal-leave-to {
  opacity: 0;
  backdrop-filter: blur(0);
}

.warning-modal-enter-from .warning-card,
.warning-modal-leave-to .warning-card {
  transform: scale(0.9);
}

/* Proctoring Warning Overlay */
.proctor-warning-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.8);
  backdrop-filter: blur(12px);
  z-index: 10000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
}

.warning-card {
  max-width: 420px;
  width: 100%;
  background: #ffffff;
  border-radius: 28px;
  padding: 32px;
  text-align: center;
  box-shadow: 0 30px 60px -12px rgba(0, 0, 0, 0.3);
  animation: shake 0.5s ease;
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-5px); }
  75% { transform: translateX(5px); }
}

.warning-icon {
  width: 64px;
  height: 64px;
  background: linear-gradient(135deg, #fef2f2, #fee2e2);
  color: #ef4444;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  margin: 0 auto 20px;
  animation: pulseWarning 1s infinite;
}

@keyframes pulseWarning {
  0%, 100% { box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.4); }
  50% { box-shadow: 0 0 0 15px rgba(239, 68, 68, 0); }
}

.warning-count {
  font-weight: 700;
  color: #ef4444;
  font-size: 1rem;
  margin: 16px 0;
  background: #fef2f2;
  padding: 6px 16px;
  border-radius: 100px;
  display: inline-block;
}

/* Loading State */
.loading-wrap {
  text-align: center;
  padding: 60px 40px;
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(20px);
  border-radius: 32px;
}

.ai-loader {
  position: relative;
  width: 80px;
  height: 80px;
  margin: 0 auto 24px;
}

.loader-ring {
  position: absolute;
  width: 100%;
  height: 100%;
  border: 3px solid transparent;
  border-radius: 50%;
  animation: spin 1.5s cubic-bezier(0.68, -0.55, 0.265, 1.55) infinite;
}

.loader-ring:nth-child(1) {
  border-top-color: #0ea5e9;
  animation-delay: 0s;
}

.loader-ring:nth-child(2) {
  border-right-color: #6366f1;
  animation-delay: 0.2s;
}

.loader-ring:nth-child(3) {
  border-bottom-color: #0ea5e9;
  animation-delay: 0.4s;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.loader-subtle {
  display: block;
  margin-top: 12px;
  font-size: 0.8rem;
  color: #94a3b8;
  font-weight: 500;
}

/* Error State */
.error-icon {
  width: 80px;
  height: 80px;
  background: #fef2f2;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 3rem;
  color: #f59e0b;
  margin: 0 auto 24px;
  animation: bounceError 0.6s ease;
}

@keyframes bounceError {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-10px); }
}

.error-message {
  color: #64748b;
  font-size: 1.1rem;
  line-height: 1.6;
  margin-bottom: 32px;
}

/* Welcome Elements Premium */
.company-badge-premium {
  display: flex;
  width: fit-content;
  margin: 0 auto 24px auto;
  align-items: center;
  gap: 10px;
  background: #ffffff;
  padding: 8px 24px;
  border-radius: 100px;
  border: 1px solid #e2e8f0;
  font-weight: 700;
  color: #0f172a;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.mini-logo {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  object-fit: cover;
}

.quiz-title {
  font-family: 'Sora', 'Inter', sans-serif;
  font-size: 2.6rem;
  font-weight: 800;
  margin: 0 0 12px 0;
  color: #0f172a;
  letter-spacing: -0.03em;
}

.quiz-subtitle-premium {
  font-size: 1.2rem;
  margin-bottom: 40px;
  color: #475569;
}

.quiz-subtitle-premium strong {
  background: linear-gradient(135deg, #0ea5e9, #8b5cf6);
  background-clip: text;
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  font-weight: 800;
}

.info-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  margin-bottom: 32px;
}

.info-card {
  padding: 24px;
  border-radius: 24px;
  display: flex;
  align-items: center;
  gap: 20px;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  text-align: left;
}

.info-card.primary-info {
  background: #f0f9ff;
  border: 1px solid #bae6fd;
}

.info-card.secondary-info {
  background: #faf5ff;
  border: 1px solid #e9d5ff;
}

.info-card:hover {
  transform: translateY(-4px);
}

.primary-info:hover { box-shadow: 0 10px 25px -5px rgba(14, 165, 233, 0.2); }
.secondary-info:hover { box-shadow: 0 10px 25px -5px rgba(139, 92, 246, 0.2); }

.info-icon {
  width: 56px;
  height: 56px;
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.5rem;
}

.info-icon.blue-icon {
  background: #0ea5e9;
  color: white;
  box-shadow: 0 4px 15px rgba(14, 165, 233, 0.3);
}

.info-icon.violet-icon {
  background: #8b5cf6;
  color: white;
  box-shadow: 0 4px 15px rgba(139, 92, 246, 0.3);
}

.glow-pulse {
  animation: pulse-glow 2s infinite;
}

@keyframes pulse-glow {
  0% { transform: scale(1); }
  50% { transform: scale(1.05); }
  100% { transform: scale(1); }
}

.info-text {
  display: flex;
  flex-direction: column;
}

.info-label {
  font-size: 0.85rem;
  color: #64748b;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin-bottom: 4px;
}

.info-val {
  font-weight: 800;
  font-size: 1.6rem;
  color: #0f172a;
}

.warning-box {
  background: #fefce8;
  border: 1px solid #fef08a;
  padding: 20px 24px;
  border-radius: 20px;
  display: flex;
  gap: 20px;
  margin-bottom: 40px;
  align-items: flex-start;
  text-align: left;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.warning-icon-wrapper {
  background: #f59e0b;
  color: white;
  width: 40px;
  height: 40px;
  min-width: 40px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.2rem;
  box-shadow: 0 4px 10px rgba(245, 158, 11, 0.3);
}

.warning-box p {
  color: #475569;
  font-size: 0.95rem;
  line-height: 1.5;
  margin: 0;
}

/* Stagger Animations */
.anim-stagger-1 { animation: slideUpFade 0.6s 0.1s both cubic-bezier(0.16, 1, 0.3, 1); }
.anim-stagger-2 { animation: slideUpFade 0.6s 0.15s both cubic-bezier(0.16, 1, 0.3, 1); }
.anim-stagger-3 { animation: slideUpFade 0.6s 0.2s both cubic-bezier(0.16, 1, 0.3, 1); }
.anim-stagger-4 { animation: slideUpFade 0.6s 0.25s both cubic-bezier(0.16, 1, 0.3, 1); }
.anim-stagger-5 { animation: slideUpFade 0.6s 0.3s both cubic-bezier(0.16, 1, 0.3, 1); }
.anim-stagger-6 { animation: slideUpFade 0.6s 0.35s both cubic-bezier(0.16, 1, 0.3, 1); }

@keyframes slideUpFade {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.btn-start-quiz {
  width: 100%;
  padding: 18px;
  background: linear-gradient(135deg, #0ea5e9 0%, #6366f1 100%);
  color: white;
  border: none;
  border-radius: 20px;
  font-weight: 800;
  font-size: 1.1rem;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  position: relative;
  overflow: hidden;
}

.btn-start-quiz::before {
  content: '';
  position: absolute;
  top: 50%;
  left: 50%;
  width: 0;
  height: 0;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.3);
  transform: translate(-50%, -50%);
  transition: width 0.6s, height 0.6s;
}

.btn-start-quiz:hover::before {
  width: 300px;
  height: 300px;
}

.btn-start-quiz:hover {
  transform: translateY(-2px);
  box-shadow: 0 15px 35px -10px rgba(14, 165, 233, 0.4);
}

/* Active Quiz Header */
.celestial-header {
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  padding: 24px 32px;
  border-radius: 28px;
  margin-bottom: 32px;
  box-shadow: 
    0 20px 40px -15px rgba(15, 23, 42, 0.1),
    inset 0 0 0 1px rgba(255, 255, 255, 0.6);
  position: sticky;
  top: 20px;
  z-index: 100;
}

.progress-bar-wrap-celestial {
  height: 8px;
  background: #f1f5f9;
  border-radius: 100px;
  overflow: hidden;
}

.progress-fill-celestial {
  height: 100%;
  background: linear-gradient(90deg, #0ea5e9, #6366f1);
  border-radius: 100px;
  position: relative;
  transition: width 0.5s cubic-bezier(0.4, 0, 0.2, 1);
}

.progress-glow {
  position: absolute;
  right: 0;
  top: 0;
  bottom: 0;
  width: 20px;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.5));
  animation: shimmer 1.5s infinite;
}

@keyframes shimmer {
  0% { transform: translateX(-100%); }
  100% { transform: translateX(200%); }
}

.quiz-meta-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 16px;
}

.q-counter {
  font-size: 0.9rem;
  color: #64748b;
  font-weight: 600;
  display: flex;
  align-items: center;
  gap: 12px;
}

.answered-badge {
  background: #e0f2fe;
  padding: 4px 12px;
  border-radius: 100px;
  color: #0ea5e9;
  font-size: 0.8rem;
}

.timer-celestial {
  display: flex;
  align-items: center;
  gap: 12px;
  font-weight: 800;
  font-size: 1.25rem;
  color: #0f172a;
  background: #ffffff;
  padding: 8px 24px 8px 12px;
  border-radius: 50px;
  font-variant-numeric: tabular-nums;
  transition: all 0.3s;
  border: 2px solid #f1f5f9;
  box-shadow: 0 4px 10px rgba(15, 23, 42, 0.04);
}

.timer-critical {
  color: #ef4444;
  background: #fef2f2;
  border-color: #fecaca;
  animation: pulseCritical 1.5s infinite;
  box-shadow: 0 4px 15px rgba(239, 68, 68, 0.2);
}

@keyframes pulseCritical {
  0%, 100% { transform: scale(1); box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.4); }
  50% { transform: scale(1.02); box-shadow: 0 0 0 10px rgba(239, 68, 68, 0); }
}

.timer-icon {
  font-size: 1.4rem;
  color: #0ea5e9;
  animation: pulseIcon 2s ease-in-out infinite;
}

@keyframes pulseIcon {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.1); }
}

.timer-critical .timer-icon {
  color: #ef4444;
  animation: tickTock 0.5s ease-in-out infinite alternate;
}

@keyframes tickTock {
  0% { transform: rotate(-15deg) scale(1.1); }
  100% { transform: rotate(15deg) scale(1.1); }
}

/* Question Cards */
.questions-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 24px;
  margin-bottom: 32px;
}

.celestial-q-card {
  background: linear-gradient(145deg, #ffffff, #f8fafc);
  border: 1px solid rgba(255, 255, 255, 0.8);
  border-radius: 28px;
  padding: 32px;
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  box-shadow: 
    0 10px 25px -5px rgba(15, 23, 42, 0.05),
    inset 0 0 0 1px rgba(255, 255, 255, 1);
  position: relative;
  overflow: hidden;
}

.celestial-q-card::before {
  content: '';
  position: absolute;
  top: 0; left: 0; right: 0; height: 4px;
  background: linear-gradient(90deg, #0ea5e9, #8b5cf6);
  opacity: 0;
  transition: opacity 0.4s;
}

.celestial-q-card:hover {
  box-shadow: 0 25px 50px -12px rgba(15, 23, 42, 0.12);
  transform: translateY(-4px) scale(1.01);
}
.celestial-q-card:hover::before {
  opacity: 1;
}

.q-header-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.q-badge-group {
  display: flex;
  align-items: center;
  gap: 12px;
}

.q-number {
  font-weight: 800;
  font-size: 0.8rem;
  color: #0284c7; /* Blue */
  background: #e0f2fe;
  border: 1px solid #bae6fd;
  padding: 6px 14px;
  border-radius: 12px;
}

.q-type-tag {
  padding: 6px 14px;
  border-radius: 12px;
  font-size: 0.7rem;
  font-weight: 800;
  display: flex;
  align-items: center;
  gap: 6px;
}

.q-type-tag.technical {
  background: #fef3c7; /* Light Yellow */
  color: #d97706; /* Yellow/Amber text */
  border: 1px solid #fde68a;
}

.q-type-tag.softskill {
  background: #f3e8ff; /* Light Violet */
  color: #7c3aed; /* Violet text */
  border: 1px solid #e9d5ff;
}

.q-status {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.75rem;
  color: #10b981;
  background: #f0fdf4;
  padding: 6px 12px;
  border-radius: 20px;
}

.q-text-light {
  font-family: 'Sora', sans-serif;
  font-size: 1.15rem;
  font-weight: 700;
  line-height: 1.6;
  margin-bottom: 24px;
  color: #0f172a;
}

.options-stack {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.celestial-option {
  background: #ffffff;
  border: 2px solid transparent;
  padding: 14px 20px;
  border-radius: 20px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  display: flex;
  align-items: center;
  gap: 16px;
  position: relative;
  box-shadow: 0 4px 15px rgba(15, 23, 42, 0.04);
}

.celestial-option:focus-visible {
  outline: 3px solid rgba(139, 92, 246, 0.5);
  outline-offset: 2px;
}

.celestial-option:hover,
.celestial-option:focus-visible {
  background: #f8fafc;
  border-color: rgba(14, 165, 233, 0.3);
  transform: translateX(6px);
  box-shadow: 0 8px 25px rgba(15, 23, 42, 0.08);
}

.celestial-option.is-selected {
  background: linear-gradient(135deg, rgba(240, 249, 255, 0.8) 0%, rgba(243, 232, 255, 0.8) 100%);
  border-color: #8b5cf6;
  box-shadow: 0 10px 30px -10px rgba(139, 92, 246, 0.3);
}

.opt-letter-light {
  width: 36px;
  height: 36px;
  border-radius: 12px;
  background: #f1f5f9;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  color: #64748b;
  border: none;
  transition: all 0.3s;
  font-family: 'Sora', sans-serif;
}

.celestial-option:hover .opt-letter-light {
  background: #e0f2fe;
  color: #0ea5e9;
}

.is-selected .opt-letter-light {
  background: linear-gradient(135deg, #0ea5e9, #8b5cf6); /* Blue to Violet */
  color: white;
  border-color: transparent;
  transform: scale(1.05);
  box-shadow: 0 2px 8px rgba(139, 92, 246, 0.3);
}

.opt-text {
  font-size: 1rem;
  color: #475569;
  font-weight: 500;
  flex: 1;
}

.is-selected .opt-text {
  color: #0f172a;
  font-weight: 700;
}

.option-check {
  width: 24px;
  height: 24px;
  background: #f59e0b; /* Yellow for the checkmark! */
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 0.8rem;
  animation: checkPop 0.3s cubic-bezier(0.68, -0.55, 0.265, 1.55);
  box-shadow: 0 2px 8px rgba(245, 158, 11, 0.4);
}

@keyframes checkPop {
  0% { transform: scale(0); }
  80% { transform: scale(1.2); }
  100% { transform: scale(1); }
}

/* Footer Navigation */
.quiz-footer-nav {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 20px;
}

.celestial-btn-nav {
  padding: 14px 28px;
  border-radius: 18px;
  font-weight: 800;
  font-size: 0.95rem;
  cursor: pointer;
  transition: all 0.3s;
  display: flex;
  align-items: center;
  gap: 10px;
  border: 2px solid #f1f5f9;
  background: white;
  color: #64748b;
}

.celestial-btn-nav:hover:not(:disabled) {
  border-color: #0ea5e9;
  color: #0ea5e9;
  transform: translateY(-2px);
}

.celestial-btn-nav:active:not(:disabled) {
  transform: translateY(0) scale(0.98);
}

.celestial-btn-nav:focus-visible {
  outline: 3px solid rgba(14, 165, 233, 0.5);
  outline-offset: 2px;
}

.celestial-btn-nav.primary {
  background: linear-gradient(135deg, #0ea5e9, #6366f1);
  color: white;
  border: none;
  box-shadow: 0 5px 15px -5px rgba(14, 165, 233, 0.3);
}

.celestial-btn-nav.primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 25px -5px rgba(14, 165, 233, 0.4);
}

.celestial-btn-nav.primary:active {
  transform: translateY(0) scale(0.98);
}

.page-dots {
  display: flex;
  gap: 8px;
}

.dot-celestial {
  width: 8px;
  height: 8px;
  border-radius: 100px;
  background: #cbd5e1;
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  cursor: pointer;
}

.dot-celestial:focus-visible {
  outline: 2px solid #0ea5e9;
  outline-offset: 2px;
}

.dot-celestial.active {
  width: 28px;
  background: linear-gradient(135deg, #0ea5e9, #6366f1);
}

.result-dashboard-celestial {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  padding: 32px 24px;
  background: linear-gradient(180deg, rgba(248, 250, 252, 0.95) 0%, rgba(255, 255, 255, 0.95) 100%);
}

.success-icon-celestial {
  width: 60px;
  height: 60px;
  background: linear-gradient(135deg, #e0f2fe, #bae6fd);
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.8rem;
  color: #0ea5e9;
  margin: 0 auto 16px;
  animation: scaleIn 0.5s cubic-bezier(0.2, 0, 0.2, 1);
}

.success-icon-celestial.warning {
  background: linear-gradient(135deg, #fef3c7, #fde68a);
  color: #f59e0b;
}

@keyframes scaleIn {
  from { transform: scale(0); opacity: 0; }
  to { transform: scale(1); opacity: 1; }
}

.celestial-title {
  font-size: 1.6rem;
  font-weight: 800;
  color: #0f172a;
  margin-bottom: 16px;
}

.candidate-header {
  margin-bottom: 32px;
}

.candidate-subtitle {
  color: #64748b;
  font-size: 1rem;
  font-weight: 500;
  margin-top: 8px;
}

.candidate-success {
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, #0ea5e9, #8b5cf6);
  color: white;
  font-size: 2.5rem;
  margin: 0 auto 24px;
  box-shadow: 0 10px 25px -5px rgba(139, 92, 246, 0.3);
}

.candidate-body {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin-bottom: 40px;
  width: 100%;
}

.info-box-celestial {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  padding: 24px;
  border-radius: 20px;
  text-align: left;
  transition: transform 0.3s ease;
}

.info-box-celestial:hover {
  transform: translateY(-2px);
}

.primary-box {
  background: #f0f9ff;
  border: 1px solid #bae6fd;
}

.primary-box .box-icon {
  background: #0ea5e9;
  color: white;
}

.secondary-box {
  background: #fefce8;
  border: 1px solid #fef08a;
}

.secondary-box .box-icon {
  background: #f59e0b;
  color: white;
}

.box-icon {
  width: 48px;
  height: 48px;
  min-width: 48px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.3rem;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.box-text {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.box-text strong {
  color: #0f172a;
  font-size: 1.1rem;
  font-weight: 800;
}

.box-text span {
  color: #475569;
  font-size: 0.95rem;
  line-height: 1.5;
}

.candidate-footer {
  width: 100%;
  display: flex;
  justify-content: center;
  margin-top: 10px;
}

/* Bouton Premium */
.btn-premium-action {
  width: 100%;
  padding: 14px 24px;
  background: linear-gradient(135deg, #0ea5e9, #8b5cf6);
  color: white;
  border: none;
  border-radius: 16px;
  font-weight: 700;
  font-size: 1.05rem;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  box-shadow: 0 8px 20px -6px rgba(139, 92, 246, 0.4);
  max-width: 280px;
}

.btn-premium-action:hover {
  transform: translateY(-3px);
  box-shadow: 0 12px 25px -6px rgba(139, 92, 246, 0.6);
}

.btn-premium-action:active {
  transform: translateY(0) scale(0.98);
}

/* Responsive */
@media (max-width: 900px) {
  .questions-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .quiz-container.is-result-phase {
    max-width: 650px;
  }
  .result-dashboard-celestial {
    padding: 32px 24px;
  }
  
  .info-box-celestial {
    flex-direction: column;
    text-align: center;
    align-items: center;
  }
  
  .box-icon {
    margin: 0 auto;
  }
  
  .quiz-title {
    font-size: 1.8rem;
  }
  
  .celestial-q-card {
    padding: 24px;
  }
  
  .celestial-header {
    padding: 20px;
  }
  
  .quiz-footer-nav {
    flex-wrap: wrap;
  }
  
  .celestial-btn-nav {
    padding: 12px 20px;
  }
}
</style>