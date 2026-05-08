<template>
  <div class="notification-container" v-click-outside="closePanel">
    <!-- Notification Bell -->
    <button class="notif-bell-btn" @click="togglePanel" :class="{ 'has-unread': unreadCount > 0 }">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path><path d="M13.73 21a2 2 0 0 1-3.46 0"></path></svg>
      <span v-if="unreadCount > 0" class="badge-count">{{ unreadCount > 9 ? '9+' : unreadCount }}</span>
    </button>

    <!-- Dropdown Panel -->
    <transition name="notif-slide">
      <div v-if="isOpen" class="notif-panel">
        <!-- Header -->
        <div class="notif-panel-header">
          <div class="header-left">
            <h3>Notifications</h3>
            <span v-if="unreadCount > 0" class="unread-chip">{{ unreadCount }} nouvelle{{ unreadCount > 1 ? 's' : '' }}</span>
          </div>
          <button v-if="unreadCount > 0" class="mark-all-btn" @click="markAllAsRead">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><polyline points="20 6 9 17 4 12"></polyline></svg>
            Tout lire
          </button>
        </div>

        <!-- Notification List -->
        <div class="notif-scroll">
          <!-- Empty State -->
          <div v-if="notifications.length === 0" class="notif-empty">
            <div class="empty-icon-wrap">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9"></path><path d="M13.73 21a2 2 0 0 1-3.46 0"></path></svg>
            </div>
            <p class="empty-title">Tout est à jour</p>
            <p class="empty-sub">Aucune notification pour le moment.</p>
          </div>

          <!-- Items -->
          <div 
            v-for="(notif, index) in notifications" 
            :key="notif.id" 
            class="notif-item"
            :class="{ 'is-unread': !notif.read }"
            :style="{ '--delay': index * 0.05 + 's' }"
            @click="handleAction(notif)"
          >
            <div class="notif-icon-box" :class="'type-' + notif.type">
              <svg v-if="notif.type === 'application'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="8.5" cy="7" r="4"/><path d="M20 8v6"/><path d="M23 11h-6"/></svg>
              <svg v-else-if="notif.type === 'interview'" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="3" y="4" width="18" height="18" rx="2" ry="2"/><line x1="16" y1="2" x2="16" y2="6"/><line x1="8" y1="2" x2="8" y2="6"/><line x1="3" y1="10" x2="21" y2="10"/></svg>
              <svg v-else viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>
            </div>
            <div class="notif-body">
              <p class="notif-msg" v-html="notif.message"></p>
              <span class="notif-timestamp">{{ notif.time }}</span>
            </div>
            <div v-if="!notif.read" class="unread-indicator"></div>
          </div>
        </div>

        <!-- Footer -->
        <div class="notif-panel-footer">
          <button class="see-all-btn">
            Voir tout
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polyline points="9 18 15 12 9 6"></polyline></svg>
          </button>
        </div>
      </div>
    </transition>
  </div>
</template>

<script>
export default {
  name: 'NotificationBell',
  data() {
    return {
      isOpen: false,
      notifications: []
    }
  },
  computed: {
    unreadCount() {
      return this.notifications.filter(n => !n.read).length
    }
  },
  methods: {
    togglePanel() {
      this.isOpen = !this.isOpen
    },
    closePanel() {
      this.isOpen = false
    },
    markAllAsRead() {
      this.notifications.forEach(n => n.read = true)
    },
    handleAction(notif) {
      notif.read = true
      if (notif.link) {
        this.$router.push(notif.link)
        this.closePanel()
      }
    },
    // Call this from parent to push a notification
    addNotification(notif) {
      this.notifications.unshift({
        id: Date.now(),
        read: false,
        ...notif
      })
    }
  },
  directives: {
    'click-outside': {
      mounted(el, binding) {
        el.clickOutsideEvent = (event) => {
          if (!(el === event.target || el.contains(event.target))) {
            binding.value(event)
          }
        }
        document.body.addEventListener('click', el.clickOutsideEvent)
      },
      unmounted(el) {
        document.body.removeEventListener('click', el.clickOutsideEvent)
      }
    }
  }
}
</script>

<style scoped>
/* ─── Container ─── */
.notification-container {
  position: relative;
  z-index: 1000;
}

/* ─── Bell Button ─── */
.notif-bell-btn {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: var(--r-main-bg, rgba(255,255,255,0.05));
  border: 1px solid var(--r-border, rgba(255,255,255,0.1));
  color: var(--r-text-sub, #94a3b8);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
}
.notif-bell-btn svg { width: 20px; height: 20px; }
.notif-bell-btn:hover {
  color: var(--r-text-main, #fff);
  border-color: var(--accent-color, #fbbf24);
  box-shadow: 0 0 15px rgba(251, 191, 36, 0.1);
  background: rgba(255,255,255,0.08);
}
.notif-bell-btn.has-unread { color: var(--accent-color, #fbbf24); }

/* ─── Badge ─── */
.badge-count {
  position: absolute;
  top: -5px;
  right: -5px;
  background: #ef4444;
  color: #fff;
  font-size: 10px;
  font-weight: 800;
  min-width: 18px;
  height: 18px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 4px;
  border: 2px solid var(--card-bg, #111827);
  box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.7);
  animation: pulse-badge 2s infinite;
}

@keyframes pulse-badge {
  0% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(239, 68, 68, 0.7); }
  70% { transform: scale(1); box-shadow: 0 0 0 6px rgba(239, 68, 68, 0); }
  100% { transform: scale(0.95); box-shadow: 0 0 0 0 rgba(239, 68, 68, 0); }
}

/* ─── Panel ─── */
.notif-panel {
  position: absolute;
  top: calc(100% + 12px);
  right: 0;
  width: 380px;
  max-height: 520px;
  border-radius: 16px;
  z-index: 1000;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  transform-origin: top right;
  background: var(--card-bg, #111827);
  border: 1px solid var(--border-thin, rgba(255,255,255,0.08));
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.4);
}

/* ─── Header ─── */
.notif-panel-header {
  padding: 18px 20px 14px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid var(--border-thin, rgba(255,255,255,0.06));
}
.header-left { display: flex; align-items: center; gap: 10px; }
.notif-panel-header h3 {
  margin: 0;
  font-size: 16px;
  font-weight: 800;
  color: var(--r-text-main, #fff);
  letter-spacing: -0.3px;
}
.unread-chip {
  background: rgba(239, 68, 68, 0.12);
  color: #ef4444;
  font-size: 11px;
  font-weight: 700;
  padding: 3px 8px;
  border-radius: 20px;
}

.mark-all-btn {
  background: none;
  border: none;
  color: var(--accent-color, #fbbf24);
  font-size: 12px;
  font-weight: 700;
  cursor: pointer;
  padding: 6px 10px;
  border-radius: 8px;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  gap: 4px;
}
.mark-all-btn svg { width: 14px; height: 14px; }
.mark-all-btn:hover { background: rgba(251, 191, 36, 0.1); }

/* ─── Scroll Area ─── */
.notif-scroll {
  flex: 1;
  overflow-y: auto;
  max-height: 380px;
}
.notif-scroll::-webkit-scrollbar { width: 3px; }
.notif-scroll::-webkit-scrollbar-track { background: transparent; }
.notif-scroll::-webkit-scrollbar-thumb { background: rgba(255, 255, 255, 0.08); border-radius: 4px; }

/* ─── Empty State ─── */
.notif-empty {
  padding: 48px 24px;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
}
.empty-icon-wrap {
  width: 56px;
  height: 56px;
  border-radius: 16px;
  background: rgba(255, 255, 255, 0.04);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 16px;
}
.empty-icon-wrap svg { width: 28px; height: 28px; color: var(--r-text-sub, #64748b); }
.empty-title {
  font-size: 15px;
  font-weight: 700;
  color: var(--r-text-main, #fff);
  margin: 0 0 6px;
}
.empty-sub {
  font-size: 13px;
  color: var(--r-text-sub, #64748b);
  margin: 0;
}

/* ─── Notification Item ─── */
.notif-item {
  display: flex;
  align-items: flex-start;
  gap: 14px;
  padding: 14px 20px;
  cursor: pointer;
  transition: background 0.2s;
  position: relative;
  animation: notifIn 0.25s ease both;
  animation-delay: var(--delay, 0s);
}
.notif-item + .notif-item {
  border-top: 1px solid var(--border-thin, rgba(255,255,255,0.04));
}
.notif-item:hover {
  background: rgba(255, 255, 255, 0.03);
}
.notif-item.is-unread {
  background: rgba(251, 191, 36, 0.03);
}

@keyframes notifIn {
  from { opacity: 0; transform: translateX(8px); }
  to { opacity: 1; transform: translateX(0); }
}

/* ─── Icon Box ─── */
.notif-icon-box {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.notif-icon-box svg { width: 18px; height: 18px; }
.notif-icon-box.type-application { background: rgba(16, 185, 129, 0.12); color: #10b981; }
.notif-icon-box.type-interview { background: rgba(99, 102, 241, 0.12); color: #6366f1; }
.notif-icon-box.type-system { background: rgba(244, 114, 182, 0.12); color: #f472b6; }

/* ─── Body ─── */
.notif-body { flex: 1; min-width: 0; }
.notif-msg {
  margin: 0 0 4px;
  font-size: 13px;
  line-height: 1.5;
  color: var(--r-text-main, #e2e8f0);
}
.notif-msg :deep(strong) { font-weight: 700; }
.notif-timestamp {
  font-size: 11px;
  font-weight: 600;
  color: var(--r-text-sub, #64748b);
}

/* ─── Unread Dot ─── */
.unread-indicator {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--accent-color, #fbbf24);
  flex-shrink: 0;
  margin-top: 8px;
  box-shadow: 0 0 6px rgba(251, 191, 36, 0.4);
}

/* ─── Footer ─── */
.notif-panel-footer {
  padding: 12px 20px;
  border-top: 1px solid var(--border-thin, rgba(255,255,255,0.06));
}
.see-all-btn {
  width: 100%;
  background: transparent;
  border: none;
  color: var(--r-text-sub, #64748b);
  font-size: 13px;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 8px 0;
  border-radius: 8px;
}
.see-all-btn svg { width: 14px; height: 14px; transition: transform 0.2s; }
.see-all-btn:hover {
  color: var(--r-text-main, #fff);
}
.see-all-btn:hover svg { transform: translateX(3px); }

/* ─── LIGHT MODE ─── */
.root-light .notif-bell-btn {
  background: rgba(0, 0, 0, 0.03);
  border-color: rgba(0, 0, 0, 0.08);
  color: #64748b;
}
.root-light .notif-bell-btn:hover {
  background: rgba(0, 0, 0, 0.06);
  color: #1e293b;
  border-color: #d4a017;
}
.root-light .badge-count { border-color: var(--card-bg, #fff); }

.root-light .notif-panel {
  background: var(--card-bg, #ffffff);
  border: 1px solid rgba(0, 0, 0, 0.06);
  box-shadow: 0 20px 50px rgba(0, 0, 0, 0.1);
}
.root-light .notif-panel-header { border-bottom-color: rgba(0, 0, 0, 0.05); }
.root-light .notif-panel-header h3 { color: #0f172a; }
.root-light .mark-all-btn { color: #d4a017; }
.root-light .mark-all-btn:hover { background: rgba(212, 160, 23, 0.06); }

.root-light .notif-item + .notif-item { border-top-color: rgba(0, 0, 0, 0.04); }
.root-light .notif-item:hover { background: rgba(0, 0, 0, 0.02); }
.root-light .notif-item.is-unread { background: rgba(212, 160, 23, 0.03); } 

.root-light .notif-msg { color: #1e293b; }
.root-light .notif-timestamp { color: #94a3b8; }

.root-light .empty-icon-wrap { background: rgba(0, 0, 0, 0.04); }
.root-light .empty-title { color: #0f172a; }
.root-light .empty-sub { color: #94a3b8; }

.root-light .notif-panel-footer { border-top-color: rgba(0, 0, 0, 0.05); }
.root-light .see-all-btn { color: #64748b; }
.root-light .see-all-btn:hover { color: #0f172a; }

.root-light .notif-scroll::-webkit-scrollbar-thumb { background: rgba(0, 0, 0, 0.08); }
.root-light .unread-indicator { background: #d4a017; box-shadow: 0 0 6px rgba(212, 160, 23, 0.3); }

/* ─── Transition ─── */
.notif-slide-enter-active,
.notif-slide-leave-active {
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
}
.notif-slide-enter-from,
.notif-slide-leave-to {
  opacity: 0;
  transform: scale(0.96) translateY(-8px);
}
</style>
