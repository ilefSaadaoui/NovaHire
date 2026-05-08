<template>
  <div class="preview-sidebar">
    <div class="r-card preview-card">
      <div class="mock-browser">
        <div class="browser-header">
          <div class="dots"><span></span><span></span><span></span></div>
        </div>
        <div class="mock-content">
          <div class="mock-hero" :style="{ background: 'var(--accent)' }">
            <div class="mock-logo">NH</div>
          </div>
          <div class="mock-body">
            <div class="mock-badge">{{ (form.department || '').toUpperCase() }}</div>
            <h3>{{ form.title || 'Titre du poste' }}</h3>
            <p class="mock-meta">{{ form.location || 'Localisation' }} · {{ form.type }}</p>
            
            <div class="mock-details">
              <div class="m-item"><strong>Exp:</strong> {{ form.experienceLevel }}</div>
              <div class="m-item" v-if="!form.salaryConfidential && (form.salaryMin || form.salaryMax)">
                <strong>Sal:</strong> {{ form.salaryMin || '?' }} - {{ form.salaryMax || '?' }}K
              </div>
            </div>

            <div class="mock-skills">
              <span v-for="s in form.skills.slice(0,3)" :key="s">{{ s }}</span>
            </div>

            <div class="mock-desc" v-if="form.description" style="white-space: pre-wrap;" v-html="form.description"></div>

            <div class="mock-btn" :style="{ background: 'var(--accent)' }">Postuler</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  form: Object
})
</script>

<style scoped>
.preview-sidebar { width: 380px; }
.preview-card { padding: 0; overflow: hidden; position: sticky; top: 24px; border-radius: 20px; border: 1px solid var(--r-border); }

.mock-browser { background: var(--r-main-bg); border-radius: 20px; overflow: hidden; box-shadow: 0 10px 30px rgba(0,0,0,0.1); }
.browser-header { background: var(--bg-hover); padding: 12px 16px; border-bottom: 1px solid var(--r-border); }
.browser-header .dots { display: flex; gap: 6px; }
.browser-header .dots span { width: 8px; height: 8px; border-radius: 50%; background: var(--r-border); }

.mock-content { padding: 0; }
.mock-hero { height: 80px; display: flex; align-items: center; justify-content: center; color: white; }
.mock-logo { font-size: 24px; font-weight: 900; letter-spacing: -1px; }

.mock-body { padding: 24px; }
.mock-badge { display: inline-block; padding: 4px 10px; border-radius: 6px; background: var(--bg-hover); color: var(--r-text-sub); font-size: 10px; font-weight: 800; margin-bottom: 12px; }
.mock-body h3 { font-size: 18px; font-weight: 800; color: var(--r-text-main); margin: 0 0 8px 0; }
.mock-meta { font-size: 13px; color: var(--r-text-sub); margin-bottom: 20px; }

.mock-details { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 20px; padding-bottom: 20px; border-bottom: 1px solid var(--r-border); }
.m-item { font-size: 12px; color: var(--r-text-main); font-weight: 600; }
.m-item strong { color: var(--r-text-sub); margin-right: 4px; }

.mock-skills { display: flex; flex-wrap: wrap; gap: 6px; margin-bottom: 24px; }
.mock-skills span { padding: 4px 10px; border-radius: 6px; background: var(--bg-hover); border: 1px solid var(--r-border); font-size: 11px; font-weight: 700; color: var(--r-text-main); }

.mock-btn { width: 100%; padding: 12px; border-radius: 12px; text-align: center; color: white; font-weight: 800; font-size: 14px; cursor: pointer; }

.mock-desc {
  font-size: 11px;
  color: var(--r-text-sub);
  margin-bottom: 24px;
  display: -webkit-box;
  -webkit-line-clamp: 4;
  -webkit-box-orient: vertical;
  overflow: hidden;
  line-height: 1.5;
}
</style>
