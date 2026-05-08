<template>
  <div class="admin-glass-card anim-reveal-up">
    <div class="section-header-premium">
      <div>
        <h4 class="celestial-title imperial-aura">Départements</h4>
        <p class="section-subtitle">Gérez la structure organisationnelle de votre entreprise</p>
      </div>
      <button class="btn-add-dept" @click="$emit('add')">
        <Plus :size="18" stroke-width="2.5" />
        <span>Nouveau département</span>
      </button>
    </div>

    <!-- Empty State -->
    <div v-if="!departments || departments.length === 0" class="empty-state-dept">
      <div class="empty-icon-wrap">
        <FolderOpen :size="48" stroke-width="1.5" />
      </div>
      <h5 class="empty-title">Aucun département créé</h5>
      <p class="empty-desc">Créez votre premier département pour organiser votre équipe de recrutement.</p>
      <button class="btn-add-dept small" @click="$emit('add')">
        <Plus :size="16" stroke-width="2.5" />
        <span>Créer un département</span>
      </button>
    </div>

    <!-- Department Cards Grid -->
    <div v-else class="dept-grid">
      <div v-for="(d, index) in departments" :key="d.id" class="dept-card" :style="{ '--anim-delay': index * 0.08 + 's' }">
        <div class="dept-card-header">
          <div class="dept-icon-wrap" :style="{ background: getDeptColor(index).bg, color: getDeptColor(index).text }">
            <Layers :size="20" stroke-width="2" />
          </div>
          <div class="dept-actions">
            <button class="dept-action-btn edit" @click="$emit('edit', d)" title="Éditer">
              <Edit3 :size="15" />
            </button>
            <button class="dept-action-btn delete" @click="$emit('delete', d)" title="Supprimer">
              <Trash2 :size="15" />
            </button>
          </div>
        </div>

        <div class="dept-card-body">
          <h5 class="dept-name">{{ d.name }}</h5>
          <p class="dept-description">{{ d.description || 'Aucune description fournie' }}</p>
        </div>

        <div class="dept-card-footer">
          <div class="dept-stat">
            <Users :size="14" />
            <span><b>{{ d.membersCount }}</b> membre{{ d.membersCount !== 1 ? 's' : '' }}</span>
          </div>
          <div class="dept-date">
            <Clock :size="12" />
            <span>{{ formatDate(d.createdAt) }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { Plus, Edit3, Trash2, FolderOpen, Layers, Users, Clock } from 'lucide-vue-next'

defineProps({
  departments: Array
})

defineEmits(['add', 'edit', 'delete'])

const deptColors = [
  { bg: 'rgba(59, 130, 246, 0.1)', text: '#3b82f6' },
  { bg: 'rgba(139, 92, 246, 0.1)', text: '#8b5cf6' },
  { bg: 'rgba(236, 72, 153, 0.1)', text: '#ec4899' },
  { bg: 'rgba(245, 158, 11, 0.1)', text: '#f59e0b' },
  { bg: 'rgba(16, 185, 129, 0.1)', text: '#10b981' },
  { bg: 'rgba(20, 184, 166, 0.1)', text: '#14b8a6' },
]

const getDeptColor = (index) => deptColors[index % deptColors.length]

const formatDate = (dateString) => {
  if (!dateString) return ''
  const d = new Date(dateString)
  return d.toLocaleDateString('fr-FR', { day: 'numeric', month: 'short', year: 'numeric' })
}
</script>

<style scoped>
.admin-glass-card { background: var(--card-bg); border: 1px solid var(--r-border); border-radius: 32px; padding: 32px; }
.section-header-premium { display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px; }
.celestial-title { font-size: 20px; font-weight: 900; color: var(--r-text-main); letter-spacing: -0.5px; }
.section-subtitle { font-size: 13px; color: var(--r-text-sub); margin-top: 4px; font-weight: 600; }

/* ─── Add Button ─── */
.btn-add-dept {
  display: flex; align-items: center; gap: 10px;
  padding: 12px 24px; border-radius: 14px; border: none;
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  color: white; font-weight: 800; font-size: 13px;
  cursor: pointer; transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 6px 20px -4px rgba(14, 165, 233, 0.4), 0 2px 0 #0891b2;
  text-transform: uppercase; letter-spacing: 0.5px;
  position: relative; overflow: hidden;
}
.btn-add-dept::after {
  content: ''; position: absolute; top: 0; left: -100%;
  width: 100%; height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent);
  transition: 0.5s;
}
.btn-add-dept:hover::after { left: 100%; }
.btn-add-dept:hover { transform: translateY(-2px); box-shadow: 0 10px 28px -6px rgba(14, 165, 233, 0.5), 0 1px 0 #0891b2; }
.btn-add-dept:active { transform: translateY(1px); box-shadow: 0 2px 6px rgba(0,0,0,0.15); }
.btn-add-dept.small { font-size: 12px; padding: 10px 20px; border-radius: 12px; }

/* ─── Empty State ─── */
.empty-state-dept {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  padding: 60px 40px; text-align: center;
  background: linear-gradient(180deg, rgba(0,0,0,0.01), rgba(0,0,0,0.03));
  border-radius: 24px; border: 2px dashed var(--r-border);
}
.empty-icon-wrap { 
  width: 80px; height: 80px; border-radius: 24px;
  background: rgba(var(--accent-rgb), 0.08);
  color: var(--accent); display: flex; align-items: center; justify-content: center;
  margin-bottom: 20px;
}
.empty-title { font-size: 16px; font-weight: 900; color: var(--r-text-main); margin-bottom: 8px; }
.empty-desc { font-size: 13px; color: var(--r-text-sub); font-weight: 600; max-width: 360px; margin-bottom: 24px; line-height: 1.6; }

/* ─── Cards Grid ─── */
.dept-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 20px;
}

.dept-card {
  background: var(--card-bg);
  border: 1px solid var(--r-border);
  border-radius: 20px;
  padding: 24px;
  transition: all 0.35s cubic-bezier(0.25, 0.46, 0.45, 0.94);
  animation: cardReveal 0.5s ease both;
  animation-delay: var(--anim-delay);
  position: relative;
}
.dept-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 16px 40px -12px rgba(0, 0, 0, 0.08);
  border-color: transparent;
}

.dept-card-header {
  display: flex; justify-content: space-between; align-items: flex-start;
  margin-bottom: 16px;
}

.dept-icon-wrap {
  width: 44px; height: 44px; border-radius: 14px;
  display: flex; align-items: center; justify-content: center;
  transition: transform 0.3s;
}
.dept-card:hover .dept-icon-wrap { transform: scale(1.1) rotate(-5deg); }

.dept-actions { display: flex; gap: 6px; opacity: 0; transition: opacity 0.3s; }
.dept-card:hover .dept-actions { opacity: 1; }

.dept-action-btn {
  width: 32px; height: 32px; border-radius: 10px; border: none;
  display: flex; align-items: center; justify-content: center;
  cursor: pointer; transition: all 0.25s;
}
.dept-action-btn.edit {
  background: rgba(59, 130, 246, 0.08); color: #3b82f6;
}
.dept-action-btn.edit:hover {
  background: rgba(59, 130, 246, 0.18); transform: translateY(-2px);
  box-shadow: 0 4px 12px -2px rgba(59, 130, 246, 0.25);
}
.dept-action-btn.delete {
  background: rgba(239, 68, 68, 0.06); color: #f87171;
}
.dept-action-btn.delete:hover {
  background: rgba(239, 68, 68, 0.15); color: #ef4444; transform: translateY(-2px);
  box-shadow: 0 4px 12px -2px rgba(239, 68, 68, 0.25);
}

.dept-card-body { margin-bottom: 20px; }

.dept-name {
  font-size: 16px; font-weight: 900; color: var(--r-text-main);
  margin-bottom: 6px; letter-spacing: -0.3px;
}
.dept-description {
  font-size: 12px; color: var(--r-text-sub); font-weight: 600;
  line-height: 1.5; display: -webkit-box; -webkit-line-clamp: 2;
  -webkit-box-orient: vertical; overflow: hidden;
}

.dept-card-footer {
  display: flex; justify-content: space-between; align-items: center;
  padding-top: 16px; border-top: 1px solid var(--r-border);
}

.dept-stat {
  display: flex; align-items: center; gap: 6px;
  font-size: 12px; color: var(--accent); font-weight: 700;
}
.dept-stat b { font-weight: 900; }

.dept-date {
  display: flex; align-items: center; gap: 5px;
  font-size: 11px; color: var(--r-text-sub); font-weight: 600;
}

@keyframes cardReveal {
  from { opacity: 0; transform: translateY(16px) scale(0.97); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.anim-reveal-up { animation: revealUp 0.8s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes revealUp { from { opacity: 0; transform: translateY(30px); } to { opacity: 1; transform: translateY(0); } }
</style>
