<template>
  <div class="sa-tab-container logs-wrap animate-fade-in">
    <div class="sa-tab-header">
      <div>
        <h2 class="sa-tab-title">Journaux d'Activité Système</h2>
        <p class="sa-tab-desc">Historique complet des actions effectuées par les administrateurs et les utilisateurs.</p>
      </div>

    </div>
    <div class="table-card-full">
      <!-- Header -->
      <div class="card-header">
        <div class="header-left">
          <div class="header-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
          </div>
          <div>
            <span class="card-title">Journaux d'Activité Système</span>
            <div class="card-sub">{{ adminStore.logs.length }} entrées enregistrées</div>
          </div>
        </div>
        <div class="header-right">
          <!-- Filter -->
          <select v-model="filterAction" class="filter-select">
            <option value="">Toutes les actions</option>
            <option value="CREATE">Création</option>
            <option value="UPDATE">Mise à jour</option>
            <option value="DELETE">Suppression</option>
            <option value="LOGIN">Connexion</option>
          </select>
          <button 
            class="refresh-btn" 
            :class="{ 'is-refreshing': adminStore.loading }"
            :disabled="adminStore.loading"
            @click="adminStore.fetchLogs()"
          >
            <svg 
              viewBox="0 0 24 24" 
              fill="none" 
              stroke="currentColor" 
              stroke-width="2.5"
              :class="{ 'spin-icon': adminStore.loading }"
            >
              <polyline points="23 4 23 10 17 10"/><polyline points="1 20 1 14 7 14"/><path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"/></svg>
            {{ adminStore.loading ? 'Actualisation...' : 'Actualiser' }}
          </button>
        </div>
      </div>

      <!-- Table -->
      <div class="table-scroll">
        <table class="modern-table">
          <thead>
            <tr>
              <th>Date &amp; Heure</th>
              <th>Action</th>
              <th>Type d'entité</th>
              <th>Utilisateur</th>
              <th>Détails</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="(l, idx) in filteredLogs"
              :key="l.id"
              class="log-row"
              :style="{ animationDelay: (idx * 0.03) + 's' }"
            >
              <td class="td-timestamp">
                <div class="ts-main">{{ formatDate(l.timestamp) }}</div>
                <div class="ts-time">{{ formatTime(l.timestamp) }}</div>
              </td>
              <td>
                <span class="action-badge" :style="{ background: getBg(l.action), color: getColor(l.action) }">
                  <span class="action-dot" :style="{ background: getColor(l.action) }"></span>
                  {{ getLabel(l.action) }}
                </span>
              </td>
              <td>
                <span class="entity-chip">{{ l.entityType || 'Système' }}</span>
              </td>
              <td class="td-user">{{ l.userEmail || l.userId || 'Auto' }}</td>
              <td class="td-details">{{ l.details || l.entityId || '—' }}</td>
            </tr>
          </tbody>
        </table>
        <div v-if="!filteredLogs.length" class="empty-state">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
          <p>Aucun log enregistré</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useAdminStore } from '@/stores/adminStore'

const adminStore = useAdminStore()

const filterAction = ref('')

const filteredLogs = computed(() => {
  if (!filterAction.value) return adminStore.logs
  return adminStore.logs.filter(l => l.action?.toUpperCase() === filterAction.value)
})

const formatDate = (ts) => {
  if (!ts) return '—'
  return new Date(ts).toLocaleDateString('fr-FR', { day: '2-digit', month: 'short', year: 'numeric' })
}

const formatTime = (ts) => {
  if (!ts) return ''
  return new Date(ts).toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

const ACTION_MAP = {
  CREATE:   { label: 'Création',     color: '#10b981', bg: 'rgba(16,185,129,0.1)' },
  UPDATE:   { label: 'Mise à jour',  color: '#6C63FF', bg: 'rgba(108,99,255,0.1)' },
  DELETE:   { label: 'Suppression',  color: '#ef4444', bg: 'rgba(239,68,68,0.1)'  },
  LOGIN:    { label: 'Connexion',    color: '#f59e0b', bg: 'rgba(245,158,11,0.1)' },
  REGISTER: { label: 'Inscription',  color: '#8b5cf6', bg: 'rgba(139,92,246,0.1)' },
}
const getColor = (a) => ACTION_MAP[a?.toUpperCase()]?.color || '#94a3b8'
const getBg    = (a) => ACTION_MAP[a?.toUpperCase()]?.bg    || 'rgba(148,163,184,0.1)'
const getLabel = (a) => ACTION_MAP[a?.toUpperCase()]?.label || (a || 'Inconnu')
</script>

<style scoped>
.logs-wrap { animation: fadeIn 0.5s cubic-bezier(0.16, 1, 0.3, 1) both; }

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

.filter-select {
  padding: 8px 12px;
  background: var(--bg-hover);
  border: 1px solid var(--border-thin);
  border-radius: 10px;
  color: var(--text-primary);
  font-family: inherit;
  font-size: 13px;
  cursor: pointer;
  transition: 0.2s;
  outline: none;
}
.filter-select:focus { border-color: #6C63FF; box-shadow: 0 0 0 3px rgba(108,99,255,0.1); }

.refresh-btn {
  display: flex; align-items: center; gap: 8px;
  padding: 8px 18px;
  border-radius: 10px; border: none;
  background: linear-gradient(135deg, #6C63FF, #A463FF);
  color: white; font-weight: 700; font-size: 13px;
  cursor: pointer; transition: 0.25s;
  box-shadow: 0 4px 12px rgba(108,99,255,0.3);
}
.refresh-btn svg { width: 14px; }
.refresh-btn:hover { transform: translateY(-2px); box-shadow: 0 6px 20px rgba(108,99,255,0.4); }
.refresh-btn:disabled { opacity: 0.7; cursor: not-allowed; transform: none; }

.spin-icon {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.table-scroll { overflow-x: auto; }
.modern-table { width: 100%; border-collapse: collapse; min-width: 900px; }

.modern-table thead tr {
  background: linear-gradient(to right, rgba(108,99,255,0.04), transparent);
}
.modern-table th {
  text-align: left;
  padding: 12px 20px;
  font-size: 10px; font-weight: 800;
  color: var(--text-muted);
  text-transform: uppercase; letter-spacing: 0.8px;
  border-bottom: 1px solid var(--border-thin);
}

.modern-table td {
  padding: 13px 20px;
  border-bottom: 1px solid var(--border-thin);
  font-size: 13px; color: var(--text-primary);
}

.log-row {
  transition: background 0.2s;
  animation: rowReveal 0.4s cubic-bezier(0.16, 1, 0.3, 1) both;
}
.log-row:hover { background: rgba(108,99,255,0.025); }
.log-row:last-child td { border-bottom: none; }

@keyframes rowReveal {
  from { opacity: 0; transform: translateX(-6px); }
  to   { opacity: 1; transform: translateX(0); }
}

/* Timestamp */
.td-timestamp { white-space: nowrap; }
.ts-main { font-size: 13px; font-weight: 700; color: var(--text-primary); }
.ts-time { font-size: 11px; font-weight: 600; color: var(--text-muted); margin-top: 2px; font-variant-numeric: tabular-nums; }

/* Action badge */
.action-badge {
  display: inline-flex; align-items: center; gap: 6px;
  padding: 4px 12px; border-radius: 8px;
  font-size: 11px; font-weight: 800;
  text-transform: uppercase; letter-spacing: 0.4px;
  white-space: nowrap;
}
.action-dot { width: 5px; height: 5px; border-radius: 50%; flex-shrink: 0; }

/* Entity chip */
.entity-chip {
  padding: 3px 10px;
  border-radius: 6px;
  font-size: 11px; font-weight: 700;
  background: rgba(108,99,255,0.06);
  color: #6C63FF;
  border: 1px solid rgba(108,99,255,0.12);
}

.td-user { font-size: 12px; color: var(--text-muted); font-weight: 600; }
.td-details {
  font-size: 12px; color: var(--text-muted);
  max-width: 260px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.empty-state {
  padding: 60px 24px; text-align: center; color: var(--text-muted);
}
.empty-state svg { width: 40px; margin-bottom: 12px; opacity: 0.4; }
.empty-state p { font-size: 14px; margin: 0; }

@keyframes fadeIn { from { opacity: 0; transform: translateY(8px); } to { opacity: 1; transform: translateY(0); } }
</style>
