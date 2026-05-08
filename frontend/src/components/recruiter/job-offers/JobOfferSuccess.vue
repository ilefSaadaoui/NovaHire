<template>
  <div class="success-screen anim-reveal-up">
    <div class="success-icon">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><polyline points="20 6 9 17 4 12"/></svg>
    </div>
    <h2>Félicitations !</h2>
    <p>Votre offre est maintenant en ligne et prête à recevoir des talents.</p>
    
    <div class="share-box">
      <label>Lien de diffusion public</label>
      <div class="share-input">
        <input type="text" readonly :value="publicLink">
        <button @click="$emit('copy-link')">Copier</button>
      </div>
    </div>

    <button class="btn-premium btn-secondary" @click="$emit('back')">Retour aux offres</button>
  </div>
</template>

<script setup>
defineProps({
  publicLink: String
})

defineEmits(['copy-link', 'back'])
</script>

<style scoped>
.success-screen { text-align: center; padding: 40px 0; }
.success-icon { 
  width: 80px; height: 80px; border-radius: 50%; background: #10b981; 
  color: white; display: flex; align-items: center; justify-content: center; 
  margin: 0 auto 24px; box-shadow: 0 10px 20px rgba(16, 185, 129, 0.2);
}
.success-icon svg { width: 40px; }

h2 { font-size: 28px; font-weight: 900; color: var(--r-text-main); margin-bottom: 12px; }
p { color: var(--r-text-sub); font-size: 16px; margin-bottom: 40px; }

.share-box { background: var(--bg-hover); padding: 32px; border-radius: 20px; border: 1px solid var(--r-border); margin-bottom: 40px; text-align: left; }
.share-box label { display: block; font-size: 11px; font-weight: 800; text-transform: uppercase; color: var(--r-text-sub); margin-bottom: 12px; }
.share-input { display: flex; gap: 12px; }
.share-input input { flex: 1; padding: 12px 16px; border-radius: 12px; border: 1px solid var(--r-border); background: var(--r-main-bg); color: var(--r-text-main); font-size: 14px; font-weight: 600; outline: none; }
.share-input button { padding: 10px 20px; border-radius: 12px; background: var(--accent); color: white; border: none; font-weight: 800; cursor: pointer; transition: 0.3s; }
.share-input button:hover { transform: translateY(-2px); box-shadow: 0 4px 10px var(--accent-soft); }

.btn-premium { display: flex; align-items: center; justify-content: center; gap: 8px; padding: 14px 28px; border-radius: 12px; font-size: 15px; font-weight: 800; cursor: pointer; transition: 0.3s; width: fit-content; margin: 0 auto; }
.btn-secondary { background: var(--r-main-bg); border: 1.5px solid var(--r-border); color: var(--r-text-main); }
.btn-secondary:hover { background: var(--bg-hover); transform: translateY(-2px); }

.anim-reveal-up { animation: revealUp 0.6s cubic-bezier(0.16, 1, 0.3, 1) both; }
@keyframes revealUp { from { opacity: 0; transform: translateY(20px); } to { opacity: 1; transform: translateY(0); } }
</style>
