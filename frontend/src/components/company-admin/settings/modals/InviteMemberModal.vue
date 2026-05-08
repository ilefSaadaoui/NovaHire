<template>
  <Transition name="celestial-modal">
    <div v-if="show" class="luxury-modal-overlay" @click.self="$emit('close')">
      <div class="luxury-modal-content anim-scale-up" @click.stop>

        <!-- ── Header ── -->
        <div class="modal-header-premium">
          <div class="modal-title-group">
            <div class="icon-box-themed">
              <UserPlus :size="22" stroke-width="2.5" />
            </div>
            <div class="modal-title-text">
              <div class="modal-badge">NovaHire</div>
              <h3 class="celestial-modal-title">Inviter un Recruteur</h3>
              <p class="modal-intro">Ajoutez un nouveau membre à votre équipe</p>
            </div>
          </div>
          <button class="close-lux-btn" @click="$emit('close')">
            <X :size="18" stroke-width="2.5" />
          </button>
        </div>

        <!-- ── Form ── -->
        <form @submit.prevent="$emit('submit')" class="modal-form-wrapper">
          <div class="modal-body-premium">

            <!-- Row 1: Prénom + Nom -->
            <div class="lux-input-row">
              <div class="lux-input-group flex-1">
                <label class="imperial-label">Prénom</label>
                <div class="lux-input-wrap">
                  <div class="input-icon-box"><User :size="15" stroke-width="2.5" /></div>
                  <input
                    type="text"
                    :value="form.firstName"
                    @input="$emit('update:form', { ...form, firstName: $event.target.value })"
                    required
                    class="lux-input-text"
                    placeholder="Saisir le prénom"
                  >
                </div>
              </div>
              <div class="lux-input-group flex-1">
                <label class="imperial-label">Nom</label>
                <div class="lux-input-wrap">
                  <div class="input-icon-box"><User :size="15" stroke-width="2.5" /></div>
                  <input
                    type="text"
                    :value="form.lastName"
                    @input="$emit('update:form', { ...form, lastName: $event.target.value })"
                    required
                    class="lux-input-text"
                    placeholder="Saisir le nom"
                  >
                </div>
              </div>
            </div>

            <!-- Row 2: Email + Poste -->
            <div class="lux-input-row mt-16">
              <div class="lux-input-group flex-1">
                <label class="imperial-label">Email Professionnel</label>
                <div class="lux-input-wrap">
                  <div class="input-icon-box"><Mail :size="15" stroke-width="2.5" /></div>
                  <input
                    type="email"
                    :value="form.email"
                    @input="$emit('update:form', { ...form, email: $event.target.value })"
                    required
                    class="lux-input-text"
                    placeholder="Saisir l'adresse email"
                  >
                </div>
              </div>
              <div class="lux-input-group flex-1">
                <label class="imperial-label">Poste (Optionnel)</label>
                <div class="lux-input-wrap">
                  <div class="input-icon-box"><Briefcase :size="15" stroke-width="2.5" /></div>
                  <input
                    type="text"
                    :value="form.jobTitle"
                    @input="$emit('update:form', { ...form, jobTitle: $event.target.value })"
                    class="lux-input-text"
                    placeholder="Saisir le titre du poste"
                  >
                </div>
              </div>
            </div>

            <!-- Row 3: Département -->
            <div class="lux-input-group mt-16">
              <label class="imperial-label">Département (Optionnel)</label>
              <div class="lux-input-wrap custom-select-wrap">
                <div class="input-icon-box"><Building :size="15" stroke-width="2.5" /></div>
                <div style="flex: 1; min-width: 0;">
                  <PremiumSelect
                    :modelValue="form.departmentId"
                    @update:modelValue="$emit('update:form', { ...form, departmentId: $event })"
                    :options="deptOptions"
                  />
                </div>
              </div>
            </div>

          </div>

          <!-- ── Footer ── -->
          <div class="modal-footer-lux">
            <button type="button" class="btn-cancel" @click="$emit('close')">Annuler</button>
            <button type="submit" class="btn-invite-member" :disabled="loading">
              <Loader2 v-if="loading" class="spin" :size="18" />
              <Plus v-else :size="18" stroke-width="2.5" />
              <span>{{ loading ? 'INVITATION...' : 'INVITER UN MEMBRE' }}</span>
            </button>
          </div>
        </form>

      </div><!-- /.luxury-modal-content -->
    </div><!-- /.luxury-modal-overlay -->
  </Transition>
</template>

<script setup>
import { computed } from 'vue'
import { Mail, Loader2, Briefcase, User, UserPlus, X, Plus, Building } from 'lucide-vue-next'
import PremiumSelect from '@/components/common/PremiumSelect.vue'

const props = defineProps({
  show: Boolean,
  form: Object,
  departments: Array,
  loading: Boolean
})

defineEmits(['close', 'submit', 'update:form'])

const deptOptions = computed(() => {
  return [
    { value: null, label: 'Sélectionner un département...' },
    ...(props.departments || []).map(d => ({ value: d.id, label: d.name }))
  ]
})
</script>

<style scoped>
/* ── Overlay ── CRITICAL: pointer-events auto forces click capture in all modes */
.luxury-modal-overlay {
  position: fixed !important;
  top: 0 !important; left: 0 !important;
  width: 100vw !important; height: 100vh !important;
  z-index: 99999 !important;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(8px);
  -webkit-backdrop-filter: blur(8px);
  display: flex !important;
  align-items: center !important;
  justify-content: center !important;
  padding: 20px;
  box-sizing: border-box;
  pointer-events: auto !important;
  isolation: isolate;
}

/* ── Modal Card ── */
.luxury-modal-content {
  background: var(--card-bg);
  width: 100%;
  max-width: 580px;
  border-radius: 28px;
  padding: 28px 32px 24px;
  border: 1px solid var(--r-border);
  box-shadow: 0 30px 70px rgba(0,0,0,0.25), 0 0 0 1px rgba(14,165,233,0.07);
  display: flex;
  flex-direction: column;
  box-sizing: border-box;
  max-height: 90vh;
  pointer-events: auto !important;
  position: relative;
  z-index: 1;
}

/* ── Header ── */
.modal-header-premium {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 28px;
  padding-bottom: 22px;
  border-bottom: 1px solid var(--r-border);
}
.modal-title-group { display: flex; align-items: flex-start; gap: 14px; }
.icon-box-themed {
  width: 48px; height: 48px; min-width: 48px;
  border-radius: 14px;
  background: linear-gradient(135deg, rgba(14,165,233,0.15), rgba(6,182,212,0.08));
  color: var(--accent);
  display: flex; align-items: center; justify-content: center;
  border: 1px solid rgba(14,165,233,0.2);
  box-shadow: 0 4px 14px rgba(14,165,233,0.18);
}
.modal-title-text { display: flex; flex-direction: column; gap: 3px; }
.modal-badge {
  display: inline-flex; align-items: center;
  padding: 2px 10px; border-radius: 100px;
  background: linear-gradient(135deg, rgba(14,165,233,0.12), rgba(6,182,212,0.08));
  border: 1px solid rgba(14,165,233,0.2);
  font-size: 10px; font-weight: 800; text-transform: uppercase; letter-spacing: 1.5px;
  color: var(--accent); width: fit-content; margin-bottom: 2px;
}
.celestial-modal-title {
  font-size: 20px; font-weight: 900;
  background: linear-gradient(135deg, var(--r-text-main) 55%, var(--accent));
  -webkit-background-clip: text; -webkit-text-fill-color: transparent; background-clip: text;
  letter-spacing: -0.5px; margin: 0; line-height: 1.2;
}
.modal-intro { font-size: 12.5px; color: var(--r-text-sub); margin: 0; font-weight: 500; }
.close-lux-btn {
  background: rgba(0,0,0,0.04); border: 1px solid var(--r-border);
  width: 32px; height: 32px; min-width: 32px;
  border-radius: 10px; color: var(--r-text-sub);
  cursor: pointer; display: flex; align-items: center; justify-content: center;
  transition: all 0.25s; margin-top: 2px; flex-shrink: 0;
  pointer-events: auto !important;
}
.close-lux-btn:hover {
  background: rgba(239,68,68,0.08);
  border-color: rgba(239,68,68,0.3);
  color: #ef4444;
  transform: rotate(90deg);
}

/* ── Form Layout ── */
.modal-form-wrapper { display: flex; flex-direction: column; flex: 1; min-height: 0; pointer-events: auto !important; }
.modal-body-premium {
  flex: 1; overflow-y: auto; overflow-x: hidden;
  margin-bottom: 24px;
}
.modal-body-premium::-webkit-scrollbar { width: 4px; }
.modal-body-premium::-webkit-scrollbar-thumb { background: rgba(0,0,0,0.1); border-radius: 10px; }

.lux-input-row { display: flex; gap: 14px; width: 100%; box-sizing: border-box; }
.lux-input-group { display: flex; flex-direction: column; gap: 8px; width: 100%; box-sizing: border-box; }
.flex-1 { flex: 1; min-width: 0; }
.mt-16 { margin-top: 16px; }
.imperial-label { font-size: 11px; font-weight: 800; color: var(--r-text-sub); text-transform: uppercase; letter-spacing: 1px; }

/* ── Input Fields ── */
.lux-input-wrap {
  display: flex; align-items: center; gap: 10px;
  background: rgba(0,0,0,0.02);
  padding: 6px 14px 6px 6px;
  border-radius: 14px;
  border: 1px solid var(--r-border);
  transition: all 0.25s cubic-bezier(0.16, 1, 0.3, 1);
  box-sizing: border-box; width: 100%;
  pointer-events: auto !important;
}
.lux-input-wrap:focus-within {
  background: var(--card-bg);
  border-color: var(--accent);
  box-shadow: 0 0 0 3px var(--accent-soft), 0 6px 14px -4px rgba(14,165,233,0.15);
  transform: translateY(-1px);
}
.lux-input-text {
  flex: 1; background: transparent; border: none; outline: none;
  color: var(--r-text-main); font-weight: 600; font-size: 14px;
  min-width: 0; padding: 8px 0;
  pointer-events: auto !important;
  cursor: text !important;
}
.lux-input-text::placeholder { color: #94a3b8; font-weight: 400; }

/* ── Icon Box ── */
.input-icon-box {
  width: 34px; height: 34px; border-radius: 10px;
  background: rgba(14,165,233,0.08); color: var(--accent);
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}
.lux-input-wrap:focus-within .input-icon-box {
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  color: white;
  box-shadow: 0 4px 10px -3px rgba(14,165,233,0.5);
}

/* ── Footer ── */
.modal-footer-lux {
  display: flex; justify-content: flex-end; align-items: center; gap: 12px;
  padding-top: 20px;
  border-top: 1px solid var(--r-border);
  pointer-events: auto !important;
}
.btn-cancel {
  background: transparent;
  border: 1px solid var(--r-border);
  padding: 10px 22px; border-radius: 12px;
  font-weight: 700; font-size: 13px;
  cursor: pointer !important; color: var(--r-text-sub);
  transition: all 0.2s;
  pointer-events: auto !important;
}
.btn-cancel:hover { background: rgba(0,0,0,0.04); color: var(--r-text-main); transform: translateY(-1px); }

/* ── Invite Button ── */
.btn-invite-member {
  display: flex; align-items: center; justify-content: center; gap: 8px;
  padding: 11px 22px; border-radius: 12px; border: none;
  background: linear-gradient(135deg, #0ea5e9, #06b6d4);
  color: white; font-weight: 800; font-size: 13px;
  cursor: pointer !important;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
  box-shadow: 0 6px 20px -4px rgba(14,165,233,0.4), 0 2px 0 #0891b2;
  text-transform: uppercase; letter-spacing: 0.5px;
  position: relative; overflow: hidden;
  pointer-events: auto !important;
}
.btn-invite-member::after {
  content: ''; position: absolute; top: 0; left: -100%;
  width: 100%; height: 100%;
  background: linear-gradient(90deg, transparent, rgba(255,255,255,0.2), transparent);
  transition: 0.5s;
}
.btn-invite-member:hover::after { left: 100%; }
.btn-invite-member:hover { transform: translateY(-2px); box-shadow: 0 10px 28px -6px rgba(14,165,233,0.5), 0 1px 0 #0891b2; }
.btn-invite-member:active { transform: translateY(1px); box-shadow: 0 2px 6px rgba(0,0,0,0.15); }
.btn-invite-member:disabled { opacity: 0.6; cursor: not-allowed !important; transform: none !important; pointer-events: auto !important; }

/* ── PremiumSelect Integration ── */
.custom-select-wrap { padding: 4px 4px 4px 6px !important; }
.custom-select-wrap :deep(.premium-select-display) {
  padding: 8px 10px; border-radius: 8px;
  background: transparent !important; border: none !important;
  font-size: 14px; font-weight: 600; color: var(--r-text-main);
  box-shadow: none !important;
  pointer-events: auto !important;
  cursor: pointer !important;
}
.custom-select-wrap :deep(.premium-select-display:hover),
.custom-select-wrap :deep(.premium-select-wrapper.is-open .premium-select-display) {
  background: transparent !important; border: none !important; box-shadow: none !important;
}

/* ── Animations ── */
.spin { animation: rotate 1.5s linear infinite; }
@keyframes rotate { from { transform: rotate(0deg); } to { transform: rotate(360deg); } }
.anim-scale-up { animation: scaleUp 0.35s cubic-bezier(0.175, 0.885, 0.32, 1.275) both; }
@keyframes scaleUp { from { opacity: 0; transform: scale(0.93); } to { opacity: 1; transform: scale(1); } }

.celestial-modal-enter-active, .celestial-modal-leave-active { transition: all 0.3s ease; }
.celestial-modal-enter-from, .celestial-modal-leave-to { opacity: 0; }
</style>
