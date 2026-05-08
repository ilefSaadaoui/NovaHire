<template>
  <div class="panel-fade">
    <div class="r-card premium-inner-card">
      <!-- Empty state -->
      <div v-if="!timeline || timeline.length === 0" class="empty-timeline">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="12" cy="12" r="10"/>
          <polyline points="12 6 12 12 16 14"/>
        </svg>
        <p>Aucun historique disponible pour cette candidature.</p>
      </div>

      <!-- Timeline -->
      <div v-else class="history-premium-list">
        <div v-for="(event, idx) in timeline" :key="event.id || idx" class="history-p-item">
          <!-- Icon -->
          <div class="h-icon-container">
            <div class="h-icon-box" :style="{ background: event.color + '18', color: event.color }">
              <!-- submit -->
              <svg v-if="event.icon === 'submit'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M14 2H6a2 2 0 00-2 2v16a2 2 0 002 2h12a2 2 0 002-2V8z"/><polyline points="14 2 14 8 20 8"/>
              </svg>
              <!-- ai -->
              <svg v-else-if="event.icon === 'ai'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M12 2L2 7l10 5 10-5-10-5zM2 17l10 5 10-5M2 12l10 5 10-5"/>
              </svg>
              <!-- stage -->
              <svg v-else-if="event.icon === 'stage'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="9 11 12 14 22 4"/><path d="M21 12v7a2 2 0 01-2 2H5a2 2 0 01-2-2V5a2 2 0 012-2h11"/>
              </svg>
              <!-- interview -->
              <svg v-else-if="event.icon === 'interview'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/>
              </svg>
              <!-- comment -->
              <svg v-else-if="event.icon === 'comment'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z"/>
              </svg>
              <!-- notification -->
              <svg v-else-if="event.icon === 'notification'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M18 8A6 6 0 006 8c0 7-3 9-3 9h18s-3-2-3-9"/><path d="M13.73 21a2 2 0 01-3.46 0"/>
              </svg>
              <!-- default -->
              <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
              </svg>
            </div>
            <!-- Connector line -->
            <div v-if="idx < timeline.length - 1" class="h-connector"></div>
          </div>

          <!-- Content -->
          <div class="h-content">
            <div class="h-header">
              <span class="h-title">{{ event.title }}</span>
              <span class="h-time-badge">{{ event.timeAgo || event.time }}</span>
            </div>
            <p v-if="event.description || event.desc" class="h-desc">{{ event.description || event.desc }}</p>
            <div v-if="event.actor" class="h-actor">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/><circle cx="12" cy="7" r="4"/>
              </svg>
              <span>{{ event.actor }}</span>
            </div>
            <div class="h-timestamp">{{ event.time }}</div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  timeline: Array
})
</script>

<style scoped>
.premium-inner-card {
  background: var(--glass-bg);
  backdrop-filter: blur(25px) saturate(200%);
  border: 1px solid var(--glass-border-color);
  border-radius: 32px;
  padding: 40px;
  box-shadow: var(--premium-shadow);
}

/* Empty State */
.empty-timeline {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  padding: 64px 0;
  color: var(--r-text-sub);
  text-align: center;
}
.empty-timeline svg {
  width: 48px;
  opacity: 0.3;
}
.empty-timeline p {
  font-size: 14px;
  font-weight: 600;
  max-width: 300px;
}

/* Timeline List */
.history-premium-list {
  display: flex;
  flex-direction: column;
  gap: 0;
}

.history-p-item {
  display: flex;
  gap: 20px;
  position: relative;
}

/* Icon Container */
.h-icon-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  flex-shrink: 0;
}

.h-icon-box {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  z-index: 2;
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.history-p-item:hover .h-icon-box {
  transform: scale(1.1);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
}

.h-icon-box svg {
  width: 18px;
  height: 18px;
}

/* Connector line between events */
.h-connector {
  width: 2px;
  flex: 1;
  min-height: 24px;
  background: var(--r-border);
  opacity: 0.4;
  margin: 4px 0;
}

/* Content */
.h-content {
  flex: 1;
  padding-bottom: 28px;
}

.history-p-item:last-child .h-content {
  padding-bottom: 0;
}

.h-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;
  gap: 12px;
}

.h-title {
  font-size: 14px;
  font-weight: 700;
  color: var(--r-text-main);
}

.h-time-badge {
  font-size: 11px;
  font-weight: 700;
  color: var(--r-text-sub);
  background: var(--r-main-bg);
  padding: 3px 10px;
  border-radius: 100px;
  border: 1px solid var(--r-border);
  white-space: nowrap;
  flex-shrink: 0;
}

.h-desc {
  font-size: 13px;
  color: var(--r-text-sub);
  margin: 4px 0 0 0;
  line-height: 1.6;
}

.h-actor {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 8px;
  font-size: 12px;
  font-weight: 600;
  color: var(--accent-color, var(--accent));
}

.h-actor svg {
  width: 14px;
  height: 14px;
}

.h-timestamp {
  font-size: 11px;
  color: var(--r-text-sub);
  opacity: 0.6;
  margin-top: 6px;
  font-weight: 500;
}

/* Fade-in animation */
.panel-fade {
  animation: fadeIn 0.4s ease-out;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
