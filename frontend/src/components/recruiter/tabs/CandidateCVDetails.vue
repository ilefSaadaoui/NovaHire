<template>
  <div class="panel-fade-celestial">
    <div class="cv-details-premium">

      <!-- Edit Toggle -->
      <div class="edit-toolbar-celestial">
        <button v-if="!editing" class="pill-btn secondary" @click="startEditing">
          <Pencil :size="14" />
          <span>Modifier</span>
        </button>
        <template v-else>
          <button class="pill-btn confirm" @click="saveChanges" :disabled="saving">
            <Check :size="14" />
            <span>{{ saving ? 'Enregistrement...' : 'Confirmer' }}</span>
          </button>
          <button class="pill-btn cancel" @click="cancelEditing">
            <XIcon :size="14" />
            <span>Annuler</span>
          </button>
        </template>
      </div>

      <!-- ═══ EXPERIENCES ═══ -->
      <section class="cv-section">
        <div class="section-header-pro">
          <div class="header-main">
            <div class="icon-circle"><Briefcase :size="18" /></div>
            <h3>Expériences Professionnelles</h3>
          </div>
          <button v-if="editing" class="add-btn-minimal" @click="addExperience">
            <Plus :size="14" /> Ajouter une expérience
          </button>
        </div>

        <div class="timeline-container-pro">
          <div v-if="displayExperiences.length === 0" class="empty-state-pro">
            <p>Aucune expérience professionnelle extraite.</p>
          </div>

          <!-- VIEW MODE -->
          <template v-if="!editing">
            <div v-for="(exp, idx) in displayExperiences" :key="exp.id" class="timeline-item-pro">
              <div class="timeline-node">
                <div class="node-dot"></div>
                <div v-if="idx < displayExperiences.length - 1" class="node-line"></div>
              </div>
              
              <div class="exp-card-pro">
                <div class="exp-card-header">
                  <div class="company-brand">
                    <div class="brand-initials">{{ exp.companyInitial }}</div>
                    <div class="brand-info">
                      <h4 class="role-title">{{ exp.role }}</h4>
                      <p class="company-name">{{ exp.company }}</p>
                    </div>
                  </div>
                  <div class="period-badge">{{ exp.period }}</div>
                </div>
                
                <p v-if="exp.description" class="exp-description-pro">{{ exp.description }}</p>
                
                <div v-if="exp.duration" class="duration-tag">
                  <Clock :size="12" /> {{ exp.duration }}
                </div>
              </div>
            </div>
          </template>

          <!-- EDIT MODE -->
          <template v-else>
            <div v-for="(exp, idx) in editForm.experiences" :key="idx" class="edit-card-pro">
              <div class="edit-card-header">
                <span class="entry-index">#{{ idx + 1 }}</span>
                <button class="btn-delete-pro" @click="editForm.experiences.splice(idx, 1)">
                  <Trash2 :size="14" />
                </button>
              </div>
              
              <div class="form-grid">
                <div class="form-field">
                  <label>Intitulé du poste</label>
                  <input v-model="exp.role" placeholder="Ex: Lead Developer" />
                </div>
                <div class="form-field">
                  <label>Entreprise</label>
                  <input v-model="exp.company" placeholder="Ex: Tech Corp" />
                </div>
                <div class="form-field">
                  <label>Début</label>
                  <input type="month" v-model="exp.startDate" />
                </div>
                <div class="form-field">
                  <label>Fin</label>
                  <input type="month" v-model="exp.endDate" :disabled="exp.isCurrent" />
                </div>
              </div>
              
              <div class="form-field-full">
                <label class="check-container">
                  <input type="checkbox" v-model="exp.isCurrent" />
                  <span class="checkmark"></span>
                  <span class="check-label">Poste actuel</span>
                </label>
              </div>

              <div class="form-field-full">
                <label>Description des missions</label>
                <textarea v-model="exp.description" rows="3" placeholder="Qu'avez-vous accompli ?"></textarea>
              </div>
            </div>
          </template>
        </div>
      </section>

      <!-- ═══ EDUCATION ═══ -->
      <section class="cv-section">
        <div class="section-header-pro">
          <div class="header-main">
            <div class="icon-circle"><GraduationCap :size="18" /></div>
            <h3>Formations & Diplômes</h3>
          </div>
          <button v-if="editing" class="add-btn-minimal" @click="addEducation">
            <Plus :size="14" /> Ajouter un diplôme
          </button>
        </div>

        <div class="edu-grid-pro">
          <!-- VIEW MODE -->
          <template v-if="!editing">
            <div v-for="edu in displayEducation" :key="edu.id" class="edu-card-pro">
              <div class="edu-icon-box">
                <Award :size="20" />
              </div>
              <div class="edu-details">
                <h5>{{ edu.degree }}</h5>
                <p class="school-name">{{ edu.school }}</p>
                <p class="edu-year">{{ edu.year }}</p>
              </div>
            </div>
          </template>

          <!-- EDIT MODE -->
          <template v-else>
            <div v-for="(edu, idx) in editForm.education" :key="idx" class="edit-card-pro compact">
              <div class="edit-card-header">
                <span class="entry-index">#{{ idx + 1 }}</span>
                <button class="btn-delete-pro" @click="editForm.education.splice(idx, 1)">
                  <Trash2 :size="14" />
                </button>
              </div>
              <div class="form-grid-3">
                <div class="form-field">
                  <label>Diplôme</label>
                  <input v-model="edu.degree" />
                </div>
                <div class="form-field">
                  <label>École / Université</label>
                  <input v-model="edu.school" />
                </div>
                <div class="form-field">
                  <label>Année</label>
                  <input v-model="edu.year" />
                </div>
              </div>
            </div>
          </template>
        </div>
      </section>

      <!-- ═══ SKILLS ═══ -->
      <section class="cv-section last">
        <div class="section-header-pro">
          <div class="header-main">
            <div class="icon-circle"><Trophy :size="18" /></div>
            <h3>Compétences & Expertise</h3>
          </div>
        </div>

        <!-- VIEW MODE -->
        <div v-if="!editing" class="skills-view-pro">
          <div v-for="group in displaySkillGroups" :key="group.name" class="skill-category">
            <label class="category-label">{{ group.name }}</label>
            <div class="skill-pills-row">
              <span v-for="s in group.skills" :key="s" class="skill-pill-premium">{{ s }}</span>
            </div>
          </div>
        </div>

        <!-- EDIT MODE -->
        <div v-else class="skills-edit-pro">
          <div class="skill-pills-row editable">
            <span v-for="(s, idx) in editForm.skills" :key="idx" class="skill-pill-premium is-editable">
              {{ s }}
              <button class="btn-remove-pill" @click="editForm.skills.splice(idx, 1)"><XIcon :size="10" /></button>
            </span>
          </div>
          <div class="skill-input-bar">
            <input
              v-model="newSkill"
              placeholder="Ajouter une compétence..."
              @keydown.enter.prevent="addSkill"
            />
            <button class="btn-add-skill" @click="addSkill" :disabled="!newSkill.trim()">
              <Plus :size="14" />
            </button>
          </div>
        </div>
      </section>

    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { Briefcase, GraduationCap, Trophy, Pencil, Check, X as XIcon, Plus, Trash2, Clock, Award } from 'lucide-vue-next'

const props = defineProps({
  candidate: Object,
  onSaveExtractedData: Function
})

const editing = ref(false)
const saving = ref(false)
const newSkill = ref('')

const editForm = ref({
  experiences: [],
  education: [],
  skills: []
})

// Display data
const displayExperiences = computed(() => props.candidate?.experiences || [])
const displayEducation = computed(() => props.candidate?.education || [])
const displaySkillGroups = computed(() => props.candidate?.skillGroups || [])

function startEditing() {
  editForm.value.experiences = (props.candidate?.experiences || []).map(exp => ({
    role: exp.role || '',
    company: exp.company || '',
    startDate: periodToMonth(exp.period, 'start'),
    endDate: periodToMonth(exp.period, 'end'),
    isCurrent: (exp.period || '').toLowerCase().includes('pr') || (exp.duration || '').toLowerCase().includes('pr'),
    description: exp.description || ''
  }))
  editForm.value.education = (props.candidate?.education || []).map(edu => ({
    degree: edu.degree || '',
    school: edu.school || '',
    year: edu.year || ''
  }))
  const allSkills = []
  for (const group of (props.candidate?.skillGroups || [])) {
    for (const s of (group.skills || [])) {
      if (!allSkills.includes(s)) allSkills.push(s)
    }
  }
  editForm.value.skills = allSkills
  editing.value = true
}

function cancelEditing() {
  editing.value = false
}

function addExperience() {
  editForm.value.experiences.push({
    role: '', company: '', startDate: '', endDate: '', isCurrent: false, description: ''
  })
}

function addEducation() {
  editForm.value.education.push({ degree: '', school: '', year: '' })
}

function addSkill() {
  const s = newSkill.value.trim()
  if (s && !editForm.value.skills.includes(s)) {
    editForm.value.skills.push(s)
  }
  newSkill.value = ''
}

async function saveChanges() {
  if (!props.onSaveExtractedData) return
  saving.value = true
  try {
    await props.onSaveExtractedData({
      experiences: editForm.value.experiences,
      education: editForm.value.education,
      skills: editForm.value.skills
    })
    editing.value = false
  } catch {
    // Error handled by parent
  } finally {
    saving.value = false
  }
}

function periodToMonth(period, part) {
  if (!period) return ''
  const parts = period.split(/\s*-\s*/)
  const raw = part === 'start' ? (parts[0] || '') : (parts[1] || '')
  const cleaned = raw.trim()
  if (!cleaned || cleaned.toLowerCase().includes('pr') || cleaned.toLowerCase().includes('n/a')) return ''
  const mmyyyy = cleaned.match(/^(\d{2})\/(\d{4})$/)
  if (mmyyyy) return `${mmyyyy[2]}-${mmyyyy[1]}`
  const yyyy = cleaned.match(/^(\d{4})$/)
  if (yyyy) return `${yyyy[1]}-01`
  return ''
}
</script>

<style scoped>
.cv-details-premium {
  position: relative;
}

/* ── Toolbar ── */
.edit-toolbar-celestial {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-bottom: 32px;
}

.pill-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  border-radius: 100px;
  font-size: 13px;
  font-weight: 800;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: 1px solid transparent;
}

.pill-btn.secondary {
  background: white;
  border-color: #e2e8f0;
  color: #64748b;
}

.pill-btn.secondary:hover {
  border-color: var(--accent);
  color: var(--accent);
  transform: translateY(-2px);
}

.pill-btn.confirm {
  background: var(--accent);
  color: white;
  box-shadow: 0 4px 12px var(--accent-soft);
}

.pill-btn.confirm:hover {
  filter: brightness(1.1);
  transform: translateY(-2px);
}

.pill-btn.cancel {
  background: #fff1f2;
  color: #e11d48;
}

/* ── Sections ── */
.cv-section {
  margin-bottom: 56px;
}

.cv-section.last {
  margin-bottom: 0;
}

.section-header-pro {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 32px;
}

.header-main {
  display: flex;
  align-items: center;
  gap: 16px;
}

.icon-circle {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: var(--accent-soft);
  color: var(--accent);
  display: flex;
  align-items: center;
  justify-content: center;
}

.header-main h3 {
  font-size: 20px;
  font-weight: 900;
  color: #1e293b;
  margin: 0;
}

.add-btn-minimal {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 800;
  color: var(--accent);
  background: transparent;
  border: 1px dashed var(--accent);
  padding: 8px 16px;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.3s;
}

.add-btn-minimal:hover {
  background: var(--accent);
  color: white;
}

/* ── Timeline ── */
.timeline-container-pro {
  display: flex;
  flex-direction: column;
}

.timeline-item-pro {
  display: flex;
  gap: 24px;
}

.timeline-node {
  display: flex;
  flex-direction: column;
  align-items: center;
  width: 20px;
}

.node-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: white;
  border: 3px solid var(--accent);
  box-shadow: 0 0 10px var(--accent-soft);
  z-index: 2;
  margin-top: 24px;
}

.node-line {
  flex: 1;
  width: 2px;
  background: #e2e8f0;
  margin-top: -12px;
  margin-bottom: -24px;
}

.exp-card-pro {
  flex: 1;
  padding: 24px;
  background: #f8fafc;
  border-radius: 20px;
  margin-bottom: 24px;
  border: 1px solid transparent;
  transition: all 0.3s;
}

.exp-card-pro:hover {
  background: white;
  border-color: #e2e8f0;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.04);
}

.exp-card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 16px;
}

.company-brand {
  display: flex;
  gap: 16px;
  align-items: center;
}

.brand-initials {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: white;
  border: 1px solid #e2e8f0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 900;
  color: #1e293b;
  font-size: 18px;
}

.role-title {
  font-size: 16px;
  font-weight: 800;
  color: #0f172a;
  margin: 0;
}

.company-name {
  font-size: 14px;
  font-weight: 700;
  color: var(--accent);
  margin: 2px 0 0 0;
}

.period-badge {
  padding: 4px 12px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 100px;
  font-size: 12px;
  font-weight: 800;
  color: #64748b;
}

.exp-description-pro {
  font-size: 14px;
  line-height: 1.6;
  color: #475569;
  margin: 0 0 16px 0;
}

.duration-tag {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  font-weight: 700;
  color: #94a3b8;
  background: #f1f5f9;
  padding: 4px 10px;
  border-radius: 8px;
}

/* ── Education ── */
.edu-grid-pro {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.edu-card-pro {
  display: flex;
  gap: 16px;
  padding: 20px;
  background: white;
  border: 1px solid #f1f5f9;
  border-radius: 20px;
  transition: all 0.3s;
}

.edu-card-pro:hover {
  border-color: var(--accent-soft);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.03);
  transform: translateY(-2px);
}

.edu-icon-box {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: #f8fafc;
  color: #cbd5e1;
  display: flex;
  align-items: center;
  justify-content: center;
}

.edu-details h5 {
  font-size: 15px;
  font-weight: 800;
  color: #1e293b;
  margin: 0;
}

.school-name {
  font-size: 13px;
  font-weight: 600;
  color: #64748b;
  margin: 4px 0;
}

.edu-year {
  font-size: 12px;
  font-weight: 800;
  color: var(--accent);
  margin: 0;
}

/* ── Skills ── */
.skills-view-pro {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.category-label {
  display: block;
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
  color: #94a3b8;
  letter-spacing: 1px;
  margin-bottom: 12px;
}

.skill-pills-row {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.skill-pill-premium {
  padding: 8px 18px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 100px;
  font-size: 13px;
  font-weight: 700;
  color: #475569;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);
  transition: all 0.2s;
}

.skill-pill-premium:hover {
  border-color: var(--accent);
  color: var(--accent);
  transform: scale(1.05);
}

/* ── Edit Forms ── */
.edit-card-pro {
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 24px;
  padding: 24px;
  margin-bottom: 20px;
}

.edit-card-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 20px;
}

.entry-index {
  font-size: 12px;
  font-weight: 900;
  color: var(--accent);
}

.btn-delete-pro {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: #fff1f2;
  color: #e11d48;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-bottom: 16px;
}

.form-grid-3 {
  display: grid;
  grid-template-columns: 1fr 1fr 100px;
  gap: 16px;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-field label {
  font-size: 12px;
  font-weight: 800;
  color: #64748b;
}

.form-field input, .form-field textarea, .skill-input-bar input {
  padding: 12px 16px;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  background: #f8fafc;
  font-size: 14px;
  font-weight: 600;
  color: #1e293b;
  outline: none;
  transition: all 0.3s;
}

.form-field input:focus, .form-field textarea:focus {
  border-color: var(--accent);
  background: white;
  box-shadow: 0 0 0 4px var(--accent-soft);
}

.form-field-full {
  margin-top: 16px;
}

.check-container {
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  user-select: none;
}

.check-label {
  font-size: 13px;
  font-weight: 700;
  color: #1e293b;
}

.skill-input-bar {
  display: flex;
  gap: 12px;
  margin-top: 20px;
}

.skill-input-bar input {
  flex: 1;
}

.btn-add-skill {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: var(--accent);
  color: white;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.panel-fade-celestial {
  animation: revealUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes revealUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Dark Mode Overrides - Premium Enhancement */
.dark-mode .cv-section {
  border-color: rgba(255, 255, 255, 0.05);
}

.dark-mode .header-main h3 {
  color: #ffffff;
}

.dark-mode .icon-circle {
  background: var(--accent-soft);
  color: var(--accent);
  border-color: var(--accent-glow);
  box-shadow: 0 0 15px var(--accent-soft);
}

/* Specific colors per section in Dark Mode - Kept for variety if desired, but can be forced to accent */
.dark-mode .cv-section:nth-of-type(2) .icon-circle {
  background: rgba(16, 185, 129, 0.1);
  color: #34d399;
  border-color: rgba(16, 185, 129, 0.2);
  box-shadow: 0 0 15px rgba(16, 185, 129, 0.1);
}

.dark-mode .cv-section:nth-of-type(3) .icon-circle {
  background: rgba(245, 158, 11, 0.1);
  color: #fbbf24;
  border-color: rgba(245, 158, 11, 0.2);
  box-shadow: 0 0 15px rgba(245, 158, 11, 0.1);
}

.dark-mode .pill-btn.secondary {
  background: rgba(255, 255, 255, 0.05);
  border-color: rgba(255, 255, 255, 0.1);
  color: #94a3b8;
}

.dark-mode .pill-btn.secondary:hover {
  background: rgba(255, 255, 255, 0.1);
  color: #f8fafc;
}

.dark-mode .exp-card-pro {
  background: rgba(255, 255, 255, 0.02);
  border-color: rgba(255, 255, 255, 0.05);
}

.dark-mode .exp-card-pro:hover {
  background: rgba(255, 255, 255, 0.04);
  border-color: rgba(255, 255, 255, 0.1);
}

.dark-mode .brand-initials {
  background: rgba(255, 255, 255, 0.05);
  border-color: rgba(255, 255, 255, 0.1);
  color: #f8fafc;
}

.dark-mode .role-title {
  color: #f8fafc;
}

.dark-mode .company-name {
  color: var(--accent);
}

.dark-mode .period-badge {
  background: rgba(255, 255, 255, 0.05);
  border-color: rgba(255, 255, 255, 0.1);
  color: #94a3b8;
}

.dark-mode .exp-description-pro {
  color: #94a3b8;
}

.dark-mode .duration-tag {
  background: rgba(255, 255, 255, 0.05);
  color: #64748b;
}

.dark-mode .edu-card-pro {
  background: rgba(255, 255, 255, 0.02);
  border-color: rgba(255, 255, 255, 0.05);
}

.dark-mode .edu-card-pro:hover {
  background: rgba(255, 255, 255, 0.04);
  border-color: var(--accent-soft);
}

.dark-mode .edu-icon-box {
  background: rgba(255, 255, 255, 0.05);
  color: #475569;
}

.dark-mode .edu-details h5 {
  color: #f8fafc;
}

.dark-mode .school-name {
  color: #94a3b8;
}

.dark-mode .skill-pill-premium {
  background: rgba(255, 255, 255, 0.03);
  border-color: rgba(255, 255, 255, 0.08);
  color: #cbd5e1;
}

.dark-mode .skill-pill-premium:hover {
  border-color: var(--accent);
  color: var(--accent);
  box-shadow: 0 0 15px var(--accent-glow);
}

.dark-mode .edit-card-pro {
  background: rgba(255, 255, 255, 0.02);
  border-color: rgba(255, 255, 255, 0.1);
}

.dark-mode .form-field label {
  color: #64748b;
}

.dark-mode .form-field input, 
.dark-mode .form-field textarea, 
.dark-mode .skill-input-bar input {
  background: rgba(2, 6, 23, 0.4);
  border-color: rgba(255, 255, 255, 0.1);
  color: #f8fafc;
}

.dark-mode .form-field input:focus, 
.dark-mode .form-field textarea:focus {
  background: rgba(2, 6, 23, 0.6);
  border-color: var(--accent);
  box-shadow: 0 0 10px var(--accent-glow);
}

.dark-mode .check-label {
  color: #cbd5e1;
}

@media (max-width: 768px) {
  .form-grid { grid-template-columns: 1fr; }
  .edu-grid-pro { grid-template-columns: 1fr; }
}
</style>

