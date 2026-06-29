<template>
  <div class="dashboard-layout recruiter-layout" :style="sidebarVars">
    <Sidebar active-item="rapports" :collapsible="true" :collapsed="sidebarCollapsed" @toggle-collapse="sidebarCollapsed = !sidebarCollapsed" />

    <main class="main-content recruiter-main" :class="{ 'ml-collapsed': sidebarCollapsed }">
      
      <!-- PREMIUM DOSSIER HEADER -->
      <header class="dossier-header anim-reveal-down">
         <div class="header-left">
            <button class="back-link-premium" @click="$router.push('/rapports')">
               <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="15 18 9 12 15 6"/></svg>
            </button>
            <div class="dossier-title-box">
               <span class="a-badge-gold">{{ $t('reports.dossierBadge') || 'Confidentiel • Dossier d\'Analyse' }}</span>
               <h1 class="lumina-text">{{ jobTitle || 'Chargement...' }}</h1>
            </div>
         </div>
         <div class="header-actions">
            <button class="btn-premium btn-secondary" @click="window.print()">
               <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M6 9V2h12v7"/><path d="M6 18H4a2 2 0 0 1-2-2v-5a2 2 0 0 1 2-2h16a2 2 0 0 1 2 2v5a2 2 0 0 1-2 2h-2"/><rect x="6" y="14" width="12" height="8"/></svg>
               {{ $t('common.print') || 'Imprimer' }}
            </button>
            <button class="btn-premium btn-accent">
               <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
               {{ $t('common.exportPdf') || 'Exporter PDF' }}
            </button>
         </div>
      </header>

      <div v-if="loading" class="dossier-loading">
         <div class="nebula-spinner"></div>
         <p>{{ $t('reports.generatingDossier') || 'Génération du dossier stratégique...' }}</p>
      </div>

      <div v-else class="page-body dossier-body stagger-reveal">
         <!-- ROW 1: EXECUTIVE SUMMARY -->
         <div class="dossier-row top-row">
            <div class="admin-glass-card executive-summary">
               <div class="summary-head">
                  <div class="sh-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/></svg></div>
                  <h3>{{ $t('reports.strategicSummary') || 'Résumé Stratégique' }}</h3>
               </div>
               <div class="summary-content">
                  <p v-html="$t('reports.summaryText', { jobTitle: jobTitle }) || `L'offre <strong>${jobTitle}</strong> présente une traction exceptionnelle avec un volume de candidatures supérieur à la moyenne du secteur.`"></p>
                  <div class="narrative-kpis">
                     <div class="nk-item">
                        <span class="nk-val">{{ stats.conversionRate }}%</span>
                        <span class="nk-label">{{ $t('reports.conversionRate') || 'Taux de Conversion' }}</span>
                     </div>
                     <div class="nk-item">
                        <span class="nk-val">{{ stats.aiQualityScore }}%</span>
                        <span class="nk-label">{{ $t('reports.matchQuality') || 'Match Qualité IA' }}</span>
                     </div>
                     <div class="nk-item">
                        <span class="nk-val">{{ stats.timeToHire }}j</span>
                        <span class="nk-label">{{ $t('reports.hiringSpeed') || 'Vitesse de Recrutement' }}</span>
                     </div>
                  </div>
                  <div class="ai-logic-voice">
                     <span class="voice-badge">{{ $t('pipeline.iaAnalyzed') || 'ANALYSE IA' }}</span>
                     <p>"{{ $t('reports.aiAnalysisText') || 'L\'annonce est particulièrement bien calibrée. Nous recommandons de doubler les efforts de sourcing sur LinkedIn ce mois-ci pour capter les 5% de profils experts manquants.' }}"</p>
                  </div>
               </div>
            </div>

            <div class="admin-glass-card pipeline-velocity">
               <div class="summary-head">
                  <div class="sh-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg></div>
                  <h3>{{ $t('reports.pipelineVelocity') || 'Vélocité du Pipeline' }}</h3>
               </div>
               <div class="velocity-viz">
                  <div v-for="step in translatedPipelineSteps" :key="step.name" class="v-step">
                     <div class="v-step-head">
                        <span class="vs-name">{{ step.name }}</span>
                        <span class="vs-days">{{ step.days }}{{ $t('reports.period.day') }}</span>
                     </div>
                     <div class="vs-track">
                        <div class="vs-fill" :style="{ width: step.percentage + '%', background: step.color }"></div>
                     </div>
                  </div>
               </div>
            </div>
         </div>

         <!-- ROW 2: SKILLS HEATMAP & TALENT QUALITY -->
         <div class="dossier-row mid-row">
            <div class="admin-glass-card skills-heatmap">
               <div class="summary-head">
                  <h3 class="side-title">{{ $t('reports.skillsHeatmap') || 'Heatmap des Compétences' }}</h3>
               </div>
               <div class="heatmap-grid">
                  <div v-for="skill in skillsData" :key="skill.name" class="skill-heat-item">
                     <div class="sh-label">{{ skill.name }}</div>
                     <div class="sh-bar">
                        <div class="sh-required" :style="{ width: skill.required + '%' }"></div>
                        <div class="sh-actual" :style="{ width: skill.actual + '%' }"></div>
                     </div>
                     <div class="sh-legend">
                        <span>{{ $t('reports.required') || 'Requis' }}</span>
                        <span>{{ $t('reports.present') || 'Présent' }}</span>
                     </div>
                  </div>
               </div>
            </div>

            <div class="admin-glass-card top-talents">
               <div class="summary-head">
                  <h3 class="side-title">{{ $t('reports.topTalents') || 'Top 3 Elite Talents' }}</h3>
               </div>
               <div class="talents-list">
                  <div v-for="(t, i) in topTalents" :key="t.name" class="talent-card-mini">
                     <div class="t-rank">{{ i + 1 }}</div>
                     <div class="t-info">
                        <span class="t-name">{{ t.name }}</span>
                        <span class="t-uni">{{ t.subtitle }}</span>
                     </div>
                     <div class="t-score" :style="{ color: t.score > 90 ? '#10b981' : '#f59e0b' }">
                        {{ t.score }}%
                     </div>
                  </div>
               </div>
               <button class="btn-premium btn-secondary w-full mt-20" @click="$router.push({ path: '/candidatures', query: { jobOfferId: this.id } })">
                  {{ $t('reports.viewAllVivier') || 'Voir tout le vivier' }}
               </button>
            </div>
         </div>

         <!-- ROW 3: SOURCING ROI -->
         <div class="dossier-row bottom-row">
            <div class="admin-glass-card wide sourcing-roi">
               <div class="summary-head">
                  <div class="sh-icon"><svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg></div>
                  <h3 class="side-title">{{ $t('reports.sourcingRoi') || 'Efficacité du Sourcing (ROI)' }}</h3>
               </div>
               <div class="roi-grid">
                  <div v-for="source in sourcingData" :key="source.name" class="roi-card">
                     <div class="rc-label">{{ source.name }}</div>
                     <div class="rc-main">
                        <span class="rc-val">{{ source.candidates }}</span>
                        <span class="rc-unit">{{ $t('dashboard.candidates') }}</span>
                     </div>
                     <div class="rc-footer">
                        <span class="rc-badge" :style="{ background: source.color + '20', color: source.color }">
                           {{ $t('reports.matchQuality') }}: {{ source.quality }}%
                        </span>
                     </div>
                  </div>
               </div>
            </div>
         </div>

         <!-- AUTHENTICITY CERTIFICATE -->
         <div class="dossier-footer-certification">
            <div class="cert-box">
               <div class="cert-stamp">
                  <svg viewBox="0 0 100 100"><circle cx="50" cy="50" r="45" fill="none" stroke="var(--a-accent)" stroke-width="2" stroke-dasharray="2 4"/><path d="M50 20 L60 40 L80 40 L65 55 L70 75 L50 65 L30 75 L35 55 L20 40 L40 40 Z" fill="var(--a-accent)"/></svg>
               </div>
               <div class="cert-text">
                  <p>{{ $t('reports.generatedOn', { date: new Date().toLocaleDateString($i18n.locale === 'ar' ? 'ar-SA' : 'fr-FR') }) || `Généré le ${new Date().toLocaleDateString('fr-FR')} par NovaHire Intelligence` }}</p>
                  <p class="cert-id-text">ID: NH-REPORT-{{ $route.params.id }}-ALPHA</p>
               </div>
            </div>
         </div>
      </div>
    </main>
  </div>
</template>

<script>
import api from '@/api/axios'
import Sidebar from '@/components/layout/Sidebar.vue'


export default {
  name: 'DossierAnalyse',
  components: { Sidebar },
  data() {
    return {
      id: this.$route.params.id,
      sidebarCollapsed: false,
      loading: true,
      jobTitle: '',
      stats: {
        conversionRate: 68,
        aiQualityScore: 84,
        timeToHire: 18
      },
      pipelineSteps: [
         { name: 'Sourcing', days: 4, percentage: 90, color: '#0ea5e9' },
         { name: 'Filtrage IA', days: 2, percentage: 70, color: '#818cf8' },
         { name: 'Entretiens', days: 8, percentage: 50, color: '#fcd34d' },
         { name: 'Hiring', days: 4, percentage: 20, color: '#10b981' }
      ],
      skillsData: [
         { name: 'React/Vue.js', required: 95, actual: 88 },
         { name: 'Architecture System', required: 80, actual: 45 },
         { name: 'UI/UX Design', required: 70, actual: 92 },
         { name: 'Tests Unitaires', required: 85, actual: 30 }
      ],
      topTalents: [
         { name: 'Alice Durand', subtitle: 'Senior Frontend Developer', score: 96 },
         { name: 'Marc Lefebvre', subtitle: 'Fullstack Engineer', score: 92 },
         { name: 'Sarah Bernard', subtitle: 'Vue.js Specialist', score: 89 }
      ],
      sourcingData: [
         { name: 'LinkedIn', candidates: 142, quality: 82, color: '#0077B5' },
         { name: 'Welcome To The Jungle', candidates: 64, quality: 89, color: '#FFD700' },
         { name: 'Site Carrière', candidates: 38, quality: 45, color: '#0ea5e9' }
      ]
    }
  },
  computed: {
    translatedPipelineSteps() {
      const map = {
        'Sourcing': this.$t('pipeline.stages.submitted'),
        'Filtrage IA': this.$t('pipeline.stages.underreview'),
        'Entretiens': this.$t('pipeline.stages.interview'),
        'Hiring': this.$t('pipeline.stages.accepted')
      }
      return this.pipelineSteps.map(s => ({
        ...s,
        name: map[s.name] || s.name
      }))
    }
  },
  async mounted() {
    await this.fetchData()
  },
  methods: {
    async fetchData() {
      this.loading = true
      try {
         const res = await api.get(`/JobOffer/${this.id}`)
         this.jobTitle = res.data.title
         // Simulation d'un délai pour l'effet "Analysis generation"
         setTimeout(() => { this.loading = false }, 1200)
      } catch (err) {
         console.error('Erreur dossier:', err)
         this.loading = false
      }
    }
  }
}
</script>

<style scoped>
@import "@/assets/premium-platform.css";
@import "@/assets/recruiter-theme.css";
@import "@/assets/admin-theme.css";

.dashboard-layout { background: transparent !important; }

.dossier-header {
   display: flex;
   justify-content: space-between;
   align-items: center;
   margin-bottom: 40px;
}

.header-left { display: flex; align-items: center; gap: 24px; }
.dossier-title-box { display: flex; flex-direction: column; gap: 8px; }

.lumina-text {
   font-size: 32px; font-weight: 900; color: var(--r-text-main); margin: 0;
}

.dossier-body { display: flex; flex-direction: column; gap: 32px; }
.dossier-row { display: grid; gap: 32px; }
.top-row { grid-template-columns: 1.5fr 1fr; }
.mid-row { grid-template-columns: 1fr 1fr; }
.bottom-row { grid-template-columns: 1fr; }

/* EXECUTIVE SUMMARY */
.summary-head { display: flex; align-items: center; gap: 16px; margin-bottom: 24px; }
.sh-icon { width: 44px; height: 44px; border-radius: 12px; background: var(--accent-soft); color: var(--accent); display: flex; align-items: center; justify-content: center; }
.summary-head h3 { font-size: 20px; font-weight: 800; color: var(--r-text-main); margin: 0; }

.executive-summary .summary-content p { font-size: 16px; line-height: 1.6; color: var(--r-text-main); }
.narrative-kpis { display: flex; gap: 40px; margin: 32px 0; }
.nk-item { display: flex; flex-direction: column; }
.nk-val { font-size: 28px; font-weight: 900; color: var(--accent); }
.nk-label { font-size: 11px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; }

.ai-logic-voice { background: var(--r-main-bg); padding: 24px; border-radius: 20px; border-left: 4px solid var(--accent); position: relative; }
.voice-badge { position: absolute; top: -10px; left: 24px; background: var(--accent); color: var(--accent-contrast); font-size: 10px; font-weight: 900; padding: 2px 10px; border-radius: 100px; }
.ai-logic-voice p { font-style: italic; color: var(--r-text-main); margin: 0; }

/* VELOCITY */
.velocity-viz { display: flex; flex-direction: column; gap: 20px; }
.v-step-head { display: flex; justify-content: space-between; margin-bottom: 8px; }
.vs-name { font-weight: 700; color: var(--r-text-main); }
.vs-days { font-weight: 900; color: var(--r-text-sub); }
.vs-track { height: 8px; background: var(--r-border); border-radius: 100px; overflow: visible; }
.vs-fill { height: 100%; border-radius: 100px; }

/* HEATMAP */
.heatmap-grid { display: flex; flex-direction: column; gap: 24px; }
.sh-bar { height: 12px; background: var(--r-border); border-radius: 100px; position: relative; margin: 8px 0; }
.sh-required { position: absolute; height: 100%; background: var(--r-text-sub); opacity: 0.1; border-radius: 100px; z-index: 1; }
.sh-actual { position: absolute; height: 100%; background: var(--accent); border-radius: 100px; z-index: 2; box-shadow: 0 0 10px var(--accent-soft); }
.sh-legend { display: flex; justify-content: flex-end; gap: 16px; font-size: 10px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; }

/* TALENTS */
.talents-list { display: flex; flex-direction: column; gap: 12px; }
.talent-card-mini { display: flex; align-items: center; gap: 16px; padding: 16px; background: var(--r-main-bg); border-radius: 16px; border: 1px solid var(--r-border); }
.t-rank { width: 28px; height: 28px; background: var(--accent); color: var(--accent-contrast); border-radius: 8px; display: flex; align-items: center; justify-content: center; font-weight: 900; }
.t-info { flex: 1; }
.t-name { font-weight: 800; color: var(--r-text-main); display: block; }
.t-uni { font-size: 11px; color: var(--r-text-sub); }
.t-score { font-weight: 900; font-size: 16px; }

/* ROI GRID */
.roi-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 24px; }
.roi-card { padding: 24px; background: var(--r-main-bg); border-radius: 20px; border: 1px solid var(--r-border); text-align: center; }
.rc-label { font-size: 11px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; margin-bottom: 12px; }
.rc-val { font-size: 32px; font-weight: 900; color: var(--r-text-main); }
.rc-unit { margin-left: 8px; font-size: 14px; color: var(--r-text-sub); }
.rc-badge { display: inline-block; padding: 4px 12px; border-radius: 100px; font-size: 11px; font-weight: 800; margin-top: 16px; }

/* FOOTER */
.dossier-footer-certification { margin-top: 60px; padding-top: 40px; border-top: 1px solid var(--r-border); display: flex; justify-content: center; opacity: 0.6; }
.cert-box { display: flex; align-items: center; gap: 24px; }
.cert-stamp { width: 60px; height: 60px; }
.cert-id-text { font-family: 'Courier New', monospace; font-size: 11px; letter-spacing: 1px; }

.dossier-loading { height: 60vh; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 24px; }

@media print {
   .sidebar, .back-link-premium, .header-actions { display: none !important; }
   .main-content { margin: 0 !important; padding: 0 !important; }
}

@media (max-width: 1024px) {
   .top-row, .mid-row { grid-template-columns: 1fr; }
   .roi-grid { grid-template-columns: 1fr; }
}
</style>
