<template>
  <div class="sa-tab-container">
    <div class="sa-tab-header">
      <div>
        <h2 class="sa-tab-title">Support Hub</h2>
        <p class="sa-tab-desc">Gérez les demandes d'assistance et les messages de prospects.</p>
      </div>

    </div>

    <div v-if="loading" class="sa-loading-state">
      <div class="sa-spinner"></div>
      <p>Chargement du Support Hub...</p>
    </div>

    <div v-else-if="messages.length === 0" class="sa-empty-state glass-panel animate-fade-in">
      <div class="empty-icon-wrap">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0a2 2 0 012 2v4a2 2 0 01-2 2H4a2 2 0 01-2-2v-4a2 2 0 012-2m16 0h-4m-8 0H4m8 0v4"/></svg>
      </div>
      <h3>Boîte de réception vide</h3>
    </div>

    <div v-else class="support-list">
      <div 
        v-for="msg in messages" 
        :key="msg.id" 
        class="message-item glass-panel hov-lift animate-slide-up"
        :class="{ 'status-new': msg.status === 0 }"
      >
        <div class="msg-main">
          <div class="msg-header-row">
            <div class="msg-user">
              <div class="user-avatar-mini">{{ msg.fullName.charAt(0) }}</div>
              <div class="user-meta">
                <h4>{{ msg.fullName }}</h4>
                <span class="user-sub">{{ msg.company }} • {{ msg.email }}</span>
              </div>
            </div>
            
            <div class="msg-actions">
              <button 
                v-if="msg.status === 0"
                @click="updateStatus(msg, 1)" 
                class="action-btn"
                title="Prendre en charge"
              >
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>
              </button>
              <button 
                v-if="msg.status !== 2"
                @click="updateStatus(msg, 2)" 
                class="action-btn success"
                title="Marquer comme résolu"
              >
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="20 6 9 17 4 12"/></svg>
              </button>
              <button 
                @click="updateStatus(msg, 3)" 
                class="action-btn danger"
                title="Archiver"
              >
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
              </button>
            </div>
          </div>

          <div class="msg-content">
            <p>"{{ msg.message }}"</p>
          </div>

          <div class="msg-footer-row">
            <span class="msg-date">{{ formatDate(msg.createdAt) }}</span>
            <div class="msg-status-badge" :class="getStatusClass(msg.status)">
              {{ getStatusLabel(msg.status) }}
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import adminService from '@/services/adminService'

const messages = ref([])
const loading = ref(true)

const newMessagesCount = computed(() => messages.value.filter(m => m.status === 0).length)

const fetchMessages = async () => {
  loading.value = true
  try {
    const res = await adminService.getContactMessages()
    messages.value = res.data
  } catch (err) {
    console.error('Failed to fetch contact messages', err)
  } finally {
    loading.value = false
  }
}

const updateStatus = async (msg, newStatus) => {
  try {
    await adminService.updateContactMessageStatus(msg.id, newStatus)
    msg.status = newStatus
    // Optionnel: rafraichir si ARCHIVÉ pour le sortir de la liste principale
    if (newStatus === 3) {
      messages.value = messages.value.filter(m => m.id !== msg.id)
    }
  } catch (err) {
    alert('Erreur lors de la mise à jour du statut')
  }
}

const formatDate = (dateStr) => {
  return new Date(dateStr).toLocaleDateString('fr-FR', {
    day: 'numeric',
    month: 'short',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const getStatusLabel = (status) => {
  const labels = ['Nouveau', 'En cours', 'Résolu', 'Archivé']
  return labels[status] || 'Inconnu'
}

const getStatusClass = (status) => {
  const classes = ['stat-new', 'stat-progress', 'stat-resolved', 'stat-archived']
  return classes[status] || ''
}

onMounted(fetchMessages)
</script>

<style scoped>
.sa-tab-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 32px; }

.header-stat-card { 
  padding: 16px 32px; 
  display: flex; 
  flex-direction: column; 
  align-items: center; 
  min-width: 140px; 
}
.stat-value { font-size: 32px; font-weight: 800; color: #f59e0b; line-height: 1; }
.stat-label { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: 1.5px; color: var(--text-muted); margin-top: 4px; }

.support-list { display: flex; flex-direction: column; gap: 16px; }

.message-item {
  display: flex; 
  flex-direction: column;
  padding: 24px; 
  gap: 20px; 
  border-left: 4px solid transparent;
  transition: all 0.3s ease;
}

.message-item.status-new { 
  border-left-color: var(--accent-color); 
  background: linear-gradient(to right, var(--accent-soft), transparent);
}

.msg-main {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.msg-header-row {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
}

.msg-user { display: flex; align-items: center; gap: 14px; }
.user-avatar-mini {
  width: 44px; height: 44px; border-radius: 12px; 
  background: var(--accent-grad);
  display: flex; align-items: center; justify-content: center; 
  font-weight: 800; color: white;
  box-shadow: 0 4px 12px var(--accent-soft);
}

.user-meta h4 { font-size: 16px; font-weight: 700; margin: 0; color: var(--text-primary); }
.user-sub { font-size: 12px; color: var(--text-muted); display: block; margin-top: 2px; }

.msg-content {
  background: rgba(108, 99, 255, 0.03);
  padding: 16px;
  border-radius: 12px;
  border: 1px solid var(--border-thin);
}

.msg-content p { 
  font-size: 15px; 
  color: var(--text-primary); 
  margin: 0; 
  line-height: 1.6; 
}

.msg-footer-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 4px;
}

.msg-date { font-size: 12px; color: var(--text-muted); font-weight: 500; }

.msg-status-badge {
  padding: 6px 12px; border-radius: 100px; font-size: 10px; font-weight: 800; text-transform: uppercase; letter-spacing: 0.8px;
}
.stat-new { background: rgba(0, 167, 225, 0.1); color: #00A7E1; }
.stat-progress { background: rgba(245, 158, 11, 0.1); color: #f59e0b; }
.stat-resolved { background: rgba(16, 185, 129, 0.1); color: #10b981; }
.stat-archived { background: var(--bg-hover); color: var(--text-muted); }

.msg-actions { display: flex; gap: 10px; }
.action-btn {
  width: 40px; height: 40px; border-radius: 12px; border: 1px solid var(--border-thin);
  background: var(--card-bg); color: var(--text-primary); cursor: pointer; transition: 0.3s cubic-bezier(0.16, 1, 0.3, 1);
  display: flex; align-items: center; justify-content: center;
}
.action-btn:hover { background: var(--bg-hover); transform: translateY(-3px); box-shadow: 0 4px 12px rgba(0,0,0,0.05); }
.action-btn.success:hover { color: #10b981; border-color: #10b981; background: rgba(16, 185, 129, 0.05); }
.action-btn.danger:hover { color: #ef4444; border-color: #ef4444; background: rgba(239, 68, 68, 0.05); }
.action-btn svg { width: 18px; }

.sa-empty-state {
  padding: 60px;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
}

.empty-icon-wrap {
  width: 80px; height: 80px;
  background: var(--accent-soft);
  color: var(--accent-color);
  border-radius: 24px;
  display: flex; align-items: center; justify-content: center;
  margin-bottom: 8px;
}
.empty-icon-wrap svg { width: 40px; height: 40px; }

@media (max-width: 768px) {
  .msg-header-row { flex-direction: column; gap: 16px; }
  .msg-actions { width: 100%; justify-content: flex-end; }
}
</style>
