<template>
  <div class="step-content">
    <div class="section-header-premium">
      <div class="header-main">
        <h3>Étape 1 : Identité & Logistique</h3>
        <p class="step-subtitle">Définissez les bases de votre future annonce</p>
      </div>
    </div>

    <div class="form-section-premium lux-shadow">
      <div class="section-title-lux">
        <div class="title-badge">
          <span class="num">01</span>
          <div class="icon-wrap-lux">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
          </div>
        </div>
        <div class="title-text-lux">
          <h4>Identification du Poste</h4>
          <p>Détails essentiels de l'annonce</p>
        </div>
      </div>
      
      <div class="r-input-group full" :class="{ 'has-error': errors.title }">
        <label>Titre de l'annonce</label>
        <div class="input-lux-wrap">
          <input 
            type="text" 
            :value="form.title" 
            @input="$emit('update:form', { ...form, title: $event.target.value }); $emit('validate', 'title')" 
            placeholder="Saisir l'intitulé exact de l'offre"
            class="expert-input-lux"
          >
          <div class="input-glow"></div>
        </div>
        <span v-if="errors.title" class="error-msg">{{ errors.title }}</span>
      </div>

      <div class="r-form-grid-3">
        <div class="r-input-group">
          <label>Département</label>
          <div class="select-lux-wrap">
            <PremiumSelect 
              :modelValue="form.department"
              @update:modelValue="$emit('update:form', { ...form, department: $event })"
              :options="departmentOptions"
              class="expert-select-lux"
            />
          </div>
        </div>

        <div class="r-input-group">
          <label>Type de contrat</label>
          <div class="select-lux-wrap">
            <PremiumSelect 
              :modelValue="form.type"
              @update:modelValue="$emit('update:form', { ...form, type: $event })"
              :options="typeOptions"
              class="expert-select-lux"
            />
          </div>
        </div>

        <div class="r-input-group">
          <label>Séniorité requise</label>
          <div class="select-lux-wrap">
            <PremiumSelect 
              :modelValue="form.experienceLevel"
              @update:modelValue="$emit('update:form', { ...form, experienceLevel: $event })"
              :options="experienceOptions"
              class="expert-select-lux"
            />
          </div>
        </div>
      </div>

      <div class="r-input-group mt-20" v-if="form.department === 'other'">
        <label>Précisez le département</label>
        <div class="input-lux-wrap">
          <input 
            type="text" 
            :value="form.otherDepartment" 
            @input="$emit('update:form', { ...form, otherDepartment: $event.target.value })" 
            placeholder="Saisir le nom du département"
            class="expert-input-lux"
          >
          <div class="input-glow"></div>
        </div>
      </div>
    </div>

    <!-- Section: Localisation -->
    <div class="form-section-premium mt-32 lux-shadow">
      <div class="section-title-lux">
        <div class="title-badge">
          <span class="num">02</span>
          <div class="icon-wrap-lux">
            <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"/><circle cx="12" cy="10" r="3"/></svg>
          </div>
        </div>
        <div class="title-text-lux">
          <h4>Présence & Localisation</h4>
          <p>Lieu de travail et politique de présence</p>
        </div>
      </div>
      <div class="r-form-row">
        <div class="r-input-group" :class="{ 'has-error': errors.location }">
          <label>Ville / Région</label>
          <div class="input-lux-wrap">
            <input 
              type="text" 
              :value="form.location" 
              @input="$emit('update:form', { ...form, location: $event.target.value }); $emit('validate', 'location')" 
              placeholder="Saisir la ville ou zone géographique"
              class="expert-input-lux"
            >
            <div class="input-glow"></div>
          </div>
          <span v-if="errors.location" class="error-msg">{{ errors.location }}</span>
        </div>
        <div class="r-input-group">
          <label>Politique de Télétravail</label>
          <div class="select-lux-wrap">
            <PremiumSelect 
              :modelValue="form.remotePolicy"
              @update:modelValue="$emit('update:form', { ...form, remotePolicy: $event })"
              :options="remoteOptions"
              class="expert-select-lux"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import PremiumSelect from '@/components/common/PremiumSelect.vue'

const props = defineProps({
  form: Object,
  errors: Object,
  departmentOptions: Array,
  typeOptions: Array,
  remoteOptions: Array
})

const experienceOptions = [
  {
    label: 'Toute (Ouvert à tous)', value: 'all',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>'
  },
  {
    label: 'Débutant (0 - 1 an)', value: 'beginner',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 19V6M5 13l7 7 7-7"/></svg>'
  },
  { 
    label: 'Junior (1 - 3 ans)', value: 'junior',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/><polyline points="17 6 23 6 23 12"/></svg>'
  },
  { 
    label: 'Confirmé (3 - 5 ans)', value: 'intermediate',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polygon points="12 15 17 21 7 21 12 15"/><path d="M12 15V3"/><path d="M7 3h10"/></svg>'
  },
  { 
    label: 'Senior (5 - 10 ans)', value: 'senior',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="m2 4 3 12h14l3-12-6 7-4-7-4 7-6-7z"/><path d="M5 20h14"/></svg>'
  },
  { 
    label: 'Expert / Lead (10+ ans)', value: 'expert',
    icon: '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M6 3h12l4 6-10 13L2 9l4-6z"/><path d="M11 3 8 9l10 13"/><path d="M13 3l3 6L6 22"/></svg>'
  }
]

defineEmits(['update:form', 'validate'])
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
.form-section-premium:hover {
  border-color: var(--accent-soft);
  box-shadow: 0 20px 50px -20px rgba(0,0,0,0.08);
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

.r-form-grid-3 { 
  display: grid; 
  grid-template-columns: repeat(3, minmax(0, 1fr)); 
  gap: 32px; 
}
.r-form-row { 
  display: grid; 
  grid-template-columns: repeat(2, minmax(0, 1fr)); 
  gap: 32px; 
}
.mt-20 { margin-top: 20px; }
.mt-32 { margin-top: 32px; }

.r-input-group { display: flex; flex-direction: column; gap: 10px; margin-bottom: 24px; }
.r-input-group.full { grid-column: 1 / -1; }
.r-input-group label { 
  font-size: 11px; font-weight: 950; color: var(--r-text-sub); 
  text-transform: uppercase; letter-spacing: 1px; 
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}

.input-lux-wrap { position: relative; }
.input-lux-wrap input {
  width: 100%; padding: 14px 18px; border-radius: 14px; 
  background: var(--r-main-bg); border: 1.5px solid var(--r-border);
  color: var(--r-text-main); font-size: 15px; font-weight: 600;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-sizing: border-box;
}

.input-glow {
  position: absolute; inset: 0; border-radius: 14px;
  pointer-events: none; transition: 0.3s;
  box-shadow: 0 0 0 0 var(--accent-soft);
}

.input-lux-wrap input:focus {
  border-color: var(--accent);
  outline: none;
  background: var(--r-surface);
}
.input-lux-wrap input:focus + .input-glow {
  box-shadow: 0 0 20px -5px var(--accent-soft);
}

.expert-select-lux { 
  width: 100%; 
  min-height: 52px; 
  border-radius: 14px; 
}

.expert-select-native-lux {
  width: 100%; padding: 14px 40px 14px 18px; border-radius: 14px;
  background: var(--r-main-bg); border: 1.5px solid var(--r-border);
  color: var(--r-text-main); font-weight: 700; font-size: 15px;
  appearance: none; cursor: pointer; transition: 0.3s;
}
.expert-select-native-lux:focus {
  border-color: var(--accent);
  background: var(--r-surface);
  box-shadow: 0 10px 20px -5px var(--accent-soft);
  outline: none;
}

.select-lux-wrap { position: relative; }
.select-lux-wrap::after {
  content: '▼'; position: absolute; right: 16px; top: 50%; transform: translateY(-50%);
  font-size: 10px; color: var(--accent); pointer-events: none;
}

.error-msg { font-size: 11px; color: #ef4444; font-weight: 800; margin-top: 4px; }
.has-error input { border-color: #ef4444 !important; }

@media (max-width: 800px) {
  .r-form-grid-3, .r-form-row { grid-template-columns: 1fr; }
}
</style>
