<template>
  <div class="step-content">
    <div class="section-header-premium">
      <div class="header-main">
        <h3>Étape 2 : Profil & Contenu</h3>
        <p class="step-subtitle">Décrivez les missions et identifiez les talents recherchés</p>
      </div>
    </div>

    <!-- Section: Missions & Description (Rich Text) -->
    <div class="form-section-premium lux-shadow">
      <div class="section-title-lux">
        <div class="title-badge">
          <span class="num">03</span>
          <div class="icon-wrap-lux">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
          </div>
        </div>
        <div class="title-text-lux">
          <h4>Missions & Description</h4>
          <p>Détaillez les responsabilités du poste</p>
        </div>
      </div>
      
      <div class="rich-editor-lux" :class="{ 'has-error': errors.description }">
        <div class="editor-toolbar-lux">
          <div class="tool-group">
            <button type="button" @click="execCommand('bold')" title="Gras" class="tool-btn-lux">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M6 4h8a4 4 0 0 1 4 4 4 4 0 0 1-4 4H6z"/><path d="M6 12h9a4 4 0 0 1 4 4 4 4 0 0 1-4 4H6z"/></svg>
            </button>
            <button type="button" @click="execCommand('italic')" title="Italique" class="tool-btn-lux">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="19" y1="4" x2="10" y2="4"/><line x1="14" y1="20" x2="5" y2="20"/><line x1="15" y1="4" x2="9" y2="20"/></svg>
            </button>
          </div>
          <div class="tool-sep"></div>
          <div class="tool-group">
            <button type="button" @click="execCommand('insertUnorderedList')" title="Liste à puces" class="tool-btn-lux">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/></svg>
            </button>
            <button type="button" @click="execCommand('insertOrderedList')" title="Liste numérotée" class="tool-btn-lux">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="10" y1="6" x2="21" y2="6"/><line x1="10" y1="12" x2="21" y2="12"/><line x1="10" y1="18" x2="21" y2="18"/><path d="M4 6h1v4"/><path d="M4 10h2"/><path d="M6 18H4c0-1 2-2 2-3s-1-1.5-2-1"/></svg>
            </button>
          </div>
          <div class="tool-sep"></div>
          <div class="tool-group">
            <button type="button" @click="showAiPanel = !showAiPanel" title="Générer avec IA" class="tool-btn-lux ai-magic-btn" :class="{ 'is-active': showAiPanel }">
              <svg viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/></svg>
              <span class="btn-text">IA</span>
            </button>
          </div>
        </div>

        <div v-if="showAiPanel" class="ai-generator-panel anim-reveal-down">
          <div class="ai-input-wrapper">
             <input 
               type="text" 
               v-model="aiKeywords" 
               placeholder="Mots-clés (ex: React, Node, 5 ans exp...)"
               @keyup.enter="triggerAiGeneration"
             >
             <button class="ai-gen-submit" @click="triggerAiGeneration" :disabled="isGenerating">
                <span v-if="isGenerating" class="spinner-mini"></span>
                <span v-else>Générer</span>
             </button>
          </div>
          <p class="ai-hint">L'IA rédigera une description basée sur le titre du poste et vos mots-clés.</p>
        </div>
        
        <div 
          class="editor-body-lux"
          contenteditable="true"
          ref="editorRef"
          @input="onEditorInput"
          @blur="$emit('validate', 'description')"
        ></div>
        
        <div class="editor-footer-lux">
          <span class="char-count">
            <svg viewBox="0 0 24 24" width="12" height="12" fill="none" stroke="currentColor" stroke-width="3"><path d="M4 7V4h16v3M9 20h6M12 4v16"/></svg>
            {{ form.description?.length || 0 }} caractères
          </span>
          <span v-if="errors.description" class="error-msg-mini">{{ errors.description }}</span>
        </div>
      </div>
    </div>

    <!-- Section: Compétences -->
    <div class="form-section-premium mt-32 lux-shadow">
      <div class="section-title-lux">
        <div class="title-badge">
          <span class="num">04</span>
          <div class="icon-wrap-lux">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><path d="m12 3 1.912 5.886h6.19l-5.007 3.638L17.007 18.41 12 14.773l-5.007 3.637 1.912-5.886-5.007-3.638h6.19z"/></svg>
          </div>
        </div>
        <div class="title-text-lux">
          <h4>Expertise & Compétences</h4>
          <p>Savoir-faire et technologies requises</p>
        </div>
      </div>
      <div class="r-skills-box-lux">
        <div class="tags-wall-lux">
          <transition-group name="tag-pop">
            <span v-for="(skill, idx) in form.skills" :key="skill" class="skill-tag-lux">
              <span class="skill-name">{{ skill }}</span>
              <button class="kill-tag-btn" @click="$emit('remove-skill', idx)">
                <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" stroke-width="3"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </span>
          </transition-group>
          
          <div class="skill-input-box-lux">
            <input 
              type="text" 
              :value="newSkill"
              @input="$emit('update:newSkill', $event.target.value)"
              placeholder="Ajouter une compétence..." 
              @keydown.enter.prevent="$emit('add-skill')"
            >
            <div class="input-focus-line"></div>
            <kbd class="key-badge" v-if="newSkill">Enter</kbd>
          </div>
        </div>
      </div>
      <p class="field-hint-lux">Appuyez sur Entrée pour valider chaque mot-clé.</p>
    </div>

    <!-- Section: Rémunération -->
    <div class="form-section-premium mt-32 lux-shadow">
      <div class="section-title-lux">
        <div class="title-badge">
          <span class="num">05</span>
          <div class="icon-wrap-lux">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
          </div>
        </div>
        <div class="title-text-lux">
          <h4>Budget & Rémunération</h4>
          <p>Conditions financières du poste</p>
        </div>
      </div>
      
      <div class="salary-luxury-card">
         <div class="salary-header-lux">
            <div class="salary-label-group">
               <span class="main-label">Fourchette de salaire</span>
               <span class="sub-label">Montant annuel brut en euros</span>
            </div>
            <div class="salary-range-display" v-if="form.salaryMin || form.salaryMax">
               {{ form.salaryMin || '0' }}k - {{ form.salaryMax || '?' }}k €
            </div>
         </div>

         <div class="salary-grid-lux">
            <div class="sal-input-group">
               <label>Minimum</label>
               <div class="sal-input-wrap">
                  <input type="number" :value="form.salaryMin" @input="$emit('update:form', { ...form, salaryMin: $event.target.value })" placeholder="Montant annuel min">
                  <span class="cur">€</span>
               </div>
            </div>
            <div class="sal-sep-lux"><div class="dots"></div></div>
            <div class="sal-input-group">
               <label>Maximum</label>
               <div class="sal-input-wrap">
                  <input type="number" :value="form.salaryMax" @input="$emit('update:form', { ...form, salaryMax: $event.target.value })" placeholder="Montant annuel max">
                  <span class="cur">€</span>
               </div>
            </div>
         </div>

         <div class="salary-actions-lux">
            <label class="premium-toggle-lux">
               <input type="checkbox" :checked="form.salaryConfidential" @change="$emit('update:form', { ...form, salaryConfidential: $event.target.checked })">
               <span class="toggle-track"></span>
               <span class="toggle-text">Masquer le salaire sur l'offre publique</span>
            </label>
         </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'

const props = defineProps({
  form: Object,
  errors: Object,
  newSkill: String,
  isGenerating: Boolean
})

const emit = defineEmits(['update:form', 'validate', 'add-skill', 'remove-skill', 'update:newSkill', 'generate-ai-description'])

const editorRef = ref(null)
const showAiPanel = ref(false)
const aiKeywords = ref('')

const triggerAiGeneration = () => {
  emit('generate-ai-description', aiKeywords.value)
}

onMounted(() => {
  if (editorRef.value && props.form.description) {
    editorRef.value.innerHTML = props.form.description
  }
})

const onEditorInput = () => {
  emit('update:form', { ...props.form, description: editorRef.value.innerHTML })
}

watch(() => props.form.description, (newVal) => {
  if (editorRef.value && editorRef.value.innerHTML !== newVal) {
    editorRef.value.innerHTML = newVal || ''
  }
})

const execCommand = (command, value = null) => {
  document.execCommand(command, false, value)
  onEditorInput()
  editorRef.value.focus()
}
</script>

<style scoped>
.section-header-premium { margin-bottom: 40px; border-bottom: 1px solid rgba(99, 102, 241, 0.08); padding-bottom: 24px; position: relative; }
.section-header-premium::after { content: ''; position: absolute; bottom: -1px; left: 0; width: 60px; height: 1px; background: var(--accent); opacity: 0.5; }
.section-header-premium h3 { font-size: 26px; font-weight: 950; color: var(--r-text-main); margin: 0; letter-spacing: -0.5px; }

.form-section-premium { 
  background: var(--r-surface); 
  padding: 40px; 
  border-radius: 32px; 
  border: 1px solid var(--r-border);
  transition: 0.4s cubic-bezier(0.2, 0.8, 0.2, 1);
  box-shadow: 
    0 15px 50px -20px rgba(99, 102, 241, 0.08),
    0 0 0 1px rgba(99, 102, 241, 0.05);
}

.section-title-lux { display: flex; align-items: center; gap: 24px; margin-bottom: 40px; }
.title-badge { 
  display: flex; align-items: center; gap: 8px; 
  background: var(--accent-grad); padding: 8px 16px; 
  border-radius: 16px; 
  box-shadow: 0 4px 15px var(--accent-soft);
}
.title-badge .num { 
  font-size: 14px; font-weight: 950; color: white; 
  opacity: 0.8;
}
.icon-wrap-lux { 
  width: 24px; height: 24px; color: white; 
  display: flex; align-items: center; justify-content: center; 
}

.title-text-lux h4 { font-size: 22px; font-weight: 950; color: var(--r-text-main); margin: 0; letter-spacing: -0.02em; }
.title-text-lux p { font-size: 13px; color: var(--r-text-sub); font-weight: 600; margin-top: 4px; }

/* Rich Editor Lux */
.rich-editor-lux {
  background: var(--r-main-bg);
  border: 1.5px solid var(--r-border);
  border-radius: 24px;
  overflow: hidden;
  transition: 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
.rich-editor-lux:focus-within {
  border-color: var(--accent);
  box-shadow: 0 20px 40px -15px var(--accent-soft);
  transform: translateY(-2px);
}
.rich-editor-lux.has-error { border-color: #ef4444; }

.editor-toolbar-lux {
  padding: 12px 20px;
  background: rgba(0,0,0,0.02);
  border-bottom: 1px solid var(--r-border);
  display: flex;
  gap: 12px;
  align-items: center;
}
.tool-group { display: flex; gap: 6px; }
.tool-btn-lux {
  background: transparent; border: 1px solid transparent;
  width: 36px; height: 36px; border-radius: 10px;
  color: var(--r-text-sub); cursor: pointer;
  display: flex; align-items: center; justify-content: center;
  transition: 0.2s;
}
.tool-btn-lux:hover { background: var(--r-surface); color: var(--accent); border-color: var(--r-border); transform: scale(1.05); }
.tool-sep { width: 1.5px; height: 20px; background: var(--r-border); opacity: 0.5; }

.editor-body-lux {
  padding: 24px;
  min-height: 280px;
  max-height: 500px;
  overflow-y: auto;
  color: var(--r-text-main);
  font-size: 15px;
  line-height: 1.7;
  outline: none;
}
.editor-body-lux h3 {
  margin: 1.8em 0 0.8em;
  font-size: 1.4em;
  font-weight: 900;
  color: var(--accent);
  text-transform: uppercase;
  letter-spacing: 0.5px;
}
.editor-body-lux ul, .editor-body-lux ol {
  margin: 0.8em 0;
  padding-left: 1.8em;
}
.editor-body-lux ul {
  list-style-type: square;
}
.editor-body-lux ol {
  list-style-type: decimal;
}
.editor-body-lux li {
  margin-bottom: 0.3em;
}
.editor-body-lux p {
  margin-bottom: 1em;
}
.editor-body-lux:empty::before {
  content: "Décrivez les missions détaillées, l'environnement de travail...";
  color: var(--r-text-sub); opacity: 0.5;
}

.editor-footer-lux {
  padding: 10px 20px; border-top: 1px solid var(--r-border);
  display: flex; justify-content: space-between; align-items: center;
  background: rgba(0,0,0,0.01);
}
.char-count { font-size: 11px; font-weight: 800; color: var(--r-text-sub); display: flex; align-items: center; gap: 6px; }
.error-msg-mini { font-size: 11px; color: #ef4444; font-weight: 800; }

/* Skills Wall Lux */
.r-skills-box-lux {
  background: var(--r-main-bg);
  border: 1.5px solid var(--r-border);
  border-radius: 20px;
  padding: 20px;
}
.tags-wall-lux { display: flex; flex-wrap: wrap; gap: 10px; align-items: center; }

.skill-tag-lux {
  display: inline-flex; align-items: center; gap: 8px;
  padding: 8px 16px; background: var(--accent-grad);
  color: white; border-radius: 14px; font-size: 13px; font-weight: 800;
  box-shadow: 0 4px 15px var(--accent-soft);
  animation: tagEntrance 0.3s ease-out;
}
@keyframes tagEntrance { from { opacity: 0; transform: scale(0.8) translateY(10px); } }

.kill-tag-btn { background: rgba(255,255,255,0.2); border: none; width: 22px; height: 22px; border-radius: 50%; color: white; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: 0.2s; }
.kill-tag-btn:hover { background: rgba(255,255,255,0.4); transform: rotate(90deg); }

.skill-input-box-lux { position: relative; flex: 1; min-width: 250px; }
.skill-input-box-lux input {
  width: 100%; border: none !important; background: transparent !important;
  padding: 10px !important; font-size: 15px !important; font-weight: 600 !important;
  color: var(--r-text-main) !important; outline: none !important;
}
.input-focus-line { position: absolute; bottom: 0; left: 0; right: 0; height: 2px; background: var(--accent); transform: scaleX(0); transition: 0.3s; }
.skill-input-box-lux input:focus + .input-focus-line { transform: scaleX(1); }
.key-badge { position: absolute; right: 0; padding: 4px 8px; font-size: 10px; background: var(--bg-hover); border-radius: 6px; border: 1px solid var(--r-border); font-weight: 800; }

.field-hint-lux { font-size: 12px; color: var(--r-text-sub); margin-top: 12px; font-weight: 600; }

/* Salary Luxury Card */
.salary-luxury-card {
  background: var(--r-surface); border: 1.5px solid var(--r-border);
  border-radius: 28px; padding: 32px;
  box-shadow: 0 10px 40px -10px rgba(0,0,0,0.05);
}
.salary-header-lux { display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px; }
.salary-label-group { display: flex; flex-direction: column; gap: 4px; }
.main-label { font-size: 16px; font-weight: 800; color: var(--r-text-main); }
.sub-label { font-size: 12px; color: var(--r-text-sub); font-weight: 600; }
.salary-range-display { background: var(--accent-grad); color: white; padding: 8px 16px; border-radius: 12px; font-weight: 900; font-size: 14px; box-shadow: 0 5px 15px var(--accent-soft); }

.salary-grid-lux { display: grid; grid-template-columns: 1fr auto 1fr; gap: 24px; align-items: center; margin-bottom: 24px; }
.sal-input-group label { font-size: 12px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; margin-bottom: 10px; display: block; }
.sal-input-wrap { position: relative; }
.sal-input-wrap input {
  width: 100%; padding: 16px 45px 16px 20px; border-radius: 18px;
  background: var(--bg-hover); border: 1.5px solid var(--r-border);
  font-size: 18px; font-weight: 900; color: var(--r-text-main); transition: 0.3s;
}
.sal-input-wrap input:focus { border-color: var(--accent); background: var(--r-surface); box-shadow: 0 10px 20px -5px var(--accent-soft); }
.cur { position: absolute; right: 20px; top: 50%; transform: translateY(-50%); font-weight: 950; color: var(--accent); }

.sal-sep-lux .dots { width: 12px; height: 3px; background: var(--r-border); border-radius: 2px; }

.salary-actions-lux { border-top: 1px dashed var(--r-border); pt: 20px; margin-top: 24px; padding-top: 24px; }
.premium-toggle-lux { display: flex; align-items: center; gap: 14px; cursor: pointer; }
.premium-toggle-lux input { display: none; }
.toggle-track { width: 44px; height: 24px; background: var(--r-border); border-radius: 20px; position: relative; transition: 0.3s; }
.toggle-track::before { content: ''; position: absolute; left: 4px; top: 4px; width: 16px; height: 16px; background: white; border-radius: 50%; transition: 0.3s; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }
input:checked + .toggle-track { background: var(--accent); }
input:checked + .toggle-track::before { transform: translateX(20px); }
.toggle-text { font-size: 13px; font-weight: 700; color: var(--r-text-sub); }

.mt-32 { margin-top: 32px; }

/* AI Magic Button */
.ai-magic-btn {
  width: auto !important;
  padding: 0 12px;
  gap: 8px;
  background: var(--accent-soft) !important;
  border: 1.5px solid var(--accent) !important;
  color: var(--accent) !important;
  font-weight: 900;
}
.ai-magic-btn.is-active {
  background: var(--accent-grad) !important;
  color: white !important;
}
.ai-magic-btn .btn-text { font-size: 11px; }

.ai-generator-panel {
  background: rgba(99, 102, 241, 0.03);
  padding: 16px 20px;
  border-bottom: 1px solid var(--r-border);
}
.ai-input-wrapper {
  display: flex;
  gap: 12px;
}
.ai-input-wrapper input {
  flex: 1;
  background: var(--r-surface);
  border: 1.5px solid var(--r-border);
  border-radius: 12px;
  padding: 10px 16px;
  font-size: 14px;
  color: var(--r-text-main);
  outline: none;
  transition: 0.3s;
}
.ai-input-wrapper input:focus { border-color: var(--accent); }

.ai-gen-submit {
  background: var(--accent-grad);
  color: white;
  border: none;
  border-radius: 12px;
  padding: 0 20px;
  font-weight: 800;
  cursor: pointer;
  transition: 0.3s;
  display: flex;
  align-items: center;
  gap: 8px;
}
.ai-gen-submit:disabled { opacity: 0.6; cursor: not-allowed; }
.ai-gen-submit:hover:not(:disabled) { transform: scale(1.02); filter: brightness(1.1); }

.ai-hint {
  font-size: 11px;
  color: var(--r-text-sub);
  margin-top: 8px;
  font-weight: 600;
}

.anim-reveal-down {
  animation: revealDown 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
@keyframes revealDown {
  from { opacity: 0; transform: translateY(-10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
