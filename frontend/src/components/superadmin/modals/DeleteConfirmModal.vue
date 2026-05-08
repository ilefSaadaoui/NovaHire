<template>
  <div class="modal-overlay" @click.self="$emit('close')">
    <div class="modal-content animate-modal" style="max-width: 400px; text-align: center;">
      <div style="padding: 32px;">
        <div class="delete-icon-wrap">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
        </div>
        <h3 style="font-size: 20px; font-weight: 800; margin: 16px 0 8px 0;">Confirmer suppression</h3>
        <p style="color: var(--text-muted); font-size: 14px; margin-bottom: 24px;">
          Êtes-vous sûr de vouloir supprimer <strong>{{ title }}</strong> ? Cette action est irréversible.
        </p>
        <div style="display: flex; flex-direction: column; gap: 10px;">
          <button @click="$emit('confirm')" class="btn-delete" :disabled="loading">
            {{ loading ? 'Suppression...' : 'Oui, Supprimer' }}
          </button>
          <button @click="$emit('close')" class="btn-secondary" style="border: none;">Annuler</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: String,
  loading: Boolean
})

defineEmits(['close', 'confirm'])
</script>

<style scoped>
.modal-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.6); backdrop-filter: blur(4px); display: flex; align-items: center; justify-content: center; z-index: 1000; padding: 20px; }
.modal-content { background: var(--card-bg); border-radius: 20px; width: 100%; border: 1px solid var(--border-thin); box-shadow: 0 40px 80px rgba(0,0,0,0.5); }

.delete-icon-wrap { width: 64px; height: 64px; border-radius: 50%; background: rgba(239, 68, 68, 0.1); color: #ef4444; display: flex; align-items: center; justify-content: center; margin: 0 auto; }
.delete-icon-wrap svg { width: 32px; }

.btn-delete { width: 100%; padding: 14px; border-radius: 12px; border: none; background: #ef4444; color: white; font-weight: 800; cursor: pointer; transition: 0.3s; }
.btn-delete:hover:not(:disabled) { background: #dc2626; transform: translateY(-2px); }
.btn-delete:disabled { opacity: 0.6; cursor: not-allowed; }

.btn-secondary { width: 100%; padding: 12px; border-radius: 12px; background: transparent; color: var(--text-muted); font-weight: 700; cursor: pointer; }
.btn-secondary:hover { color: var(--text-primary); }

.animate-modal { animation: modalIn 0.3s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes modalIn { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
</style>
