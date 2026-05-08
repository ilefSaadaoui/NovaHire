<template>
  <div class="sa-tab-container tab-wrap animate-fade-in">
    <div class="sa-tab-header">
      <div>
        <h2 class="sa-tab-title">Gestion des Entreprises</h2>
        <p class="sa-tab-desc">Visualisez, modifiez et gérez l'ensemble des entreprises inscrites sur la plateforme.</p>
      </div>

    </div>
    <div class="table-card-full">
      <!-- Header -->
      <div class="card-header">
        <div class="header-left">
          <div class="header-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6"/></svg>
          </div>
          <div>
            <span class="card-title">Gestion des Entreprises</span>
            <div class="card-sub">{{ filteredCompanies.length }} / {{ adminStore.companies.length }} entreprises</div>
          </div>
        </div>
        <div class="header-right">
          <div class="search-wrap">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="search-icon"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
            <input type="text" v-model="searchQuery" placeholder="Rechercher une entreprise..." class="search-input" />
          </div>
          <button class="primary-btn" @click="$emit('add-company')">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
            Ajouter
          </button>
        </div>
      </div>

      <!-- Table -->
      <div class="table-scroll">
        <table class="modern-table">
          <thead>
            <tr>
              <th style="width: 35%">
                <div class="th-content">Entreprise <svg viewBox="0 0 24 24" class="sort-icon"><path d="M7 15l5 5 5-5M7 9l5-5 5 5"/></svg></div>
              </th>
              <th style="width: 25%">Contact</th>
              <th style="width: 15%">Industrie</th>
              <th style="width: 15%">Statut</th>
              <th class="th-right" style="width: 10%">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(c, idx) in filteredCompanies"
              :key="c.id"
              class="table-row"
              :style="{ animationDelay: (idx * 0.04) + 's' }"
            >
              <td>
                <div class="entity-cell">
                  <UserAvatar
                    :src="c.logoUrl || c.avatarUrl"
                    :name="c.name"
                    :size="38"
                    radius="10px"
                    :gradient="accentGrad || 'linear-gradient(135deg, #6C63FF, #A463FF)'"
                  />
                  <div>
                    <div class="cell-name">{{ c.name }}</div>
                    <div class="cell-sub">ID: {{ c.id.substring(0,8) }}…</div>
                  </div>
                </div>
              </td>
              <td class="td-muted">{{ c.contactEmail || '—' }}</td>
              <td>
                <span class="industry-badge" v-if="c.industry">{{ c.industry }}</span>
                <span v-else class="td-muted">—</span>
              </td>
              <td>
                <span class="status-badge" :class="c.isActive ? 'ok' : 'down'">
                  <span class="status-dot"></span>
                  {{ c.isActive ? 'Actif' : 'Inactif' }}
                </span>
              </td>
              <td class="th-right">
                <div class="action-group">
                  <button class="action-btn" title="Modifier" @click="$emit('edit-company', c)">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                  </button>
                  <button class="action-btn action-btn--delete" title="Supprimer" @click="$emit('delete-company', c)">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="!filteredCompanies.length" class="empty-state">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M3 21h18M5 21V7l7-4 7 4v14M9 21v-6h6v6"/></svg>
          <p>{{ searchQuery ? 'Aucun résultat pour votre recherche' : 'Aucune entreprise enregistrée' }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineOptions({ name: 'CompaniesTab' })
import { ref, computed } from 'vue'
import { useAdminStore } from '@/stores/adminStore'
import UserAvatar from '@/components/shared/UserAvatar.vue'

const adminStore = useAdminStore()
const searchQuery = ref('')

defineProps({
  accentGrad: String
})

defineEmits(['add-company', 'edit-company', 'delete-company'])

const filteredCompanies = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  if (!q) return adminStore.companies
  return adminStore.companies.filter(c =>
    c.name?.toLowerCase().includes(q) ||
    c.contactEmail?.toLowerCase().includes(q) ||
    c.industry?.toLowerCase().includes(q)
  )
})
</script>

<style scoped>
.tab-wrap { display: flex; flex-direction: column; gap: 0; }

.table-card-full {
  background: var(--card-bg);
  border-radius: 20px;
  border: 1px solid var(--border-thin);
  overflow: hidden;
  box-shadow: 0 4px 24px rgba(108, 99, 255, 0.07), 0 1px 4px rgba(0,0,0,0.04);
}

.card-header {
  padding: 20px 28px;
  border-bottom: 1px solid rgba(108, 99, 255, 0.08);
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  background: linear-gradient(135deg, rgba(108,99,255,0.03) 0%, rgba(255,255,255,0) 100%);
}

.header-left { display: flex; align-items: center; gap: 14px; }

.header-icon {
  width: 40px; height: 40px;
  border-radius: 12px;
  background: linear-gradient(135deg, rgba(108,99,255,0.12), rgba(164,99,255,0.06));
  border: 1px solid rgba(108,99,255,0.15);
  display: flex; align-items: center; justify-content: center;
  color: #6C63FF;
}
.header-icon svg { width: 18px; }

.card-title { font-size: 16px; font-weight: 800; color: var(--text-primary); display: block; }
.card-sub { font-size: 11px; font-weight: 600; color: var(--text-muted); margin-top: 2px; }

.header-right { display: flex; align-items: center; gap: 12px; }

.search-wrap { position: relative; display: flex; align-items: center; }
.search-icon { position: absolute; left: 10px; width: 15px; color: var(--text-muted); pointer-events: none; }
.search-input {
  padding: 8px 12px 8px 32px;
  background: var(--bg-hover);
  border: 1px solid var(--border-thin);
  border-radius: 10px;
  color: var(--text-primary);
  font-family: inherit;
  font-size: 13px;
  width: 220px;
  transition: all 0.2s;
}
.search-input:focus {
  outline: none;
  border-color: #6C63FF;
  box-shadow: 0 0 0 3px rgba(108,99,255,0.1);
  background: var(--card-bg);
}

.primary-btn {
  display: flex; align-items: center; gap: 6px;
  padding: 8px 18px;
  border-radius: 10px; border: none;
  background: linear-gradient(135deg, #6C63FF, #A463FF);
  color: white; font-weight: 700; font-size: 13px;
  cursor: pointer; transition: 0.25s; white-space: nowrap;
  box-shadow: 0 4px 12px rgba(108,99,255,0.3);
}
.primary-btn svg { width: 14px; }
.primary-btn:hover { transform: translateY(-2px); box-shadow: 0 6px 20px rgba(108,99,255,0.4); }

.table-scroll { overflow-x: auto; }
.modern-table { width: 100%; border-collapse: collapse; min-width: 700px; }

.modern-table thead tr {
  background: linear-gradient(to right, rgba(108,99,255,0.04), rgba(255,255,255,0));
}
.modern-table th {
  text-align: left;
  padding: 14px 20px;
  font-size: 11px; font-weight: 700;
  color: var(--text-muted);
  text-transform: uppercase; letter-spacing: 0.8px;
  border-bottom: 2px solid var(--border-thin);
  background: rgba(108, 99, 255, 0.015);
  position: sticky; top: 0; z-index: 10;
}
.th-content { display: flex; align-items: center; gap: 6px; }
.sort-icon { width: 12px; height: 12px; opacity: 0.3; transition: 0.2s; }
.modern-table th:hover .sort-icon { opacity: 0.8; color: var(--accent-color); }
.th-right { text-align: right !important; }

.modern-table td {
  padding: 14px 20px;
  border-bottom: 1px solid var(--border-thin);
  color: var(--text-primary); font-size: 13.5px;
  transition: 0.2s;
  vertical-align: middle;
}
.td-muted { color: var(--text-muted) !important; font-size: 13px !important; }

.table-row {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  animation: rowReveal 0.4s cubic-bezier(0.16, 1, 0.3, 1) both;
  position: relative;
}
.table-row:hover {
  background: rgba(108, 99, 255, 0.04);
  transform: scale(1.002) translateX(4px);
}
.table-row:last-child td { border-bottom: none; }

@keyframes rowReveal {
  from { opacity: 0; transform: translateX(-8px); }
  to   { opacity: 1; transform: translateX(0); }
}

.entity-cell { display: flex; align-items: center; gap: 12px; }
.entity-avatar {
  width: 38px; height: 38px;
  border-radius: 10px;
  display: flex; align-items: center; justify-content: center;
  color: white; font-weight: 800; font-size: 13px; flex-shrink: 0;
}
.cell-name { font-size: 13px; font-weight: 700; color: var(--text-primary); }
.cell-sub { font-size: 11px; color: var(--text-muted); margin-top: 1px; }

.industry-badge {
  padding: 4px 12px;
  border-radius: 8px;
  font-size: 11px; font-weight: 800;
  background: rgba(108, 99, 255, 0.08);
  color: #6C63FF;
  border: 1px solid rgba(108,99,255,0.2);
  text-transform: uppercase;
  letter-spacing: 0.3px;
  white-space: nowrap;
  backdrop-filter: blur(4px);
}

.status-badge {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 4px 12px; border-radius: 100px;
  font-size: 11px; font-weight: 800;
  text-transform: uppercase; letter-spacing: 0.5px;
  border: 1px solid transparent;
}
.status-badge.ok { background: rgba(16,185,129,0.08); color: #10b981; border-color: rgba(16,185,129,0.2); }
.status-badge.down { background: rgba(239,68,68,0.08); color: #ef4444; border-color: rgba(239,68,68,0.2); }
.status-dot { width: 6px; height: 6px; border-radius: 50%; background: currentColor; box-shadow: 0 0 8px currentColor; }

.action-group { display: flex; align-items: center; justify-content: flex-end; gap: 4px; }
.action-btn {
  width: 32px; height: 32px; border-radius: 9px; border: none;
  background: transparent; color: #94a3b8; cursor: pointer;
  display: inline-flex; align-items: center; justify-content: center;
  transition: 0.15s;
}
.action-btn:hover { background: rgba(108,99,255,0.08); color: #6C63FF; }
.action-btn--delete:hover { background: rgba(239,68,68,0.08); color: #ef4444; }
.action-btn svg { width: 16px; }

.empty-state {
  padding: 60px 24px; text-align: center; color: var(--text-muted);
}
.empty-state svg { width: 40px; margin-bottom: 12px; opacity: 0.4; }
.empty-state p { font-size: 14px; margin: 0; }

.animate-fade-in { animation: fadeIn 0.5s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
</style>
