<template>
  <div class="redirect-container">
    <div class="loader-pulse">Redirection vers le portail unifié...</div>
  </div>
</template>

<script>
export default {
  name: 'SuperAdminLogin',
  mounted() {
    // Redirect to the unified login page
    this.$router.replace('/connexion')
  }
}
</script>

<style scoped>
.redirect-container {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #080B12;
  color: #00A7E1;
  font-family: 'Outfit', sans-serif;
  font-weight: 800;
  font-size: 18px;
}

@keyframes pulse {
  0% { opacity: 0.6; transform: scale(0.98); }
  100% { opacity: 1; transform: scale(1); }
}

.loader-pulse {
  animation: pulse 1s infinite alternate ease-in-out;
}
</style>
