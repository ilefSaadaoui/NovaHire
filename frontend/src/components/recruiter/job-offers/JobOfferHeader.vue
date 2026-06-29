<template>
  <header class="r-topbar anim-reveal-down">
    <div class="r-welcome">
      <div class="header-details-premium">
        <h1 class="premium-title-themed">{{ isEdit ? 'Modifier l\'offre' : 'Créer une offre' }}</h1>
        <p style="color: var(--r-text-sub); font-weight: 600;">{{ isEdit ? jobOfferTitle : 'Définissez les détails de votre future annonce' }}</p>
      </div>
    </div>
  </header>
</template>

<script setup>
defineProps({
  isEdit: Boolean,
  jobOfferTitle: String
})
</script>

<style scoped>
.r-topbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 0;
  margin-bottom: 16px;
  border-bottom: 1px solid var(--r-border);
}

.premium-title-themed {
  font-weight: 800;
  color: var(--r-text-main);
  margin-bottom: 4px;
}

.anim-reveal-down {
  animation: revealDown 0.6s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes revealDown {
  from { opacity: 0; transform: translateY(-20px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
