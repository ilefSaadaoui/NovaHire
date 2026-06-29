<template>
  <div class="celestial-bg-system" :class="{ 'ca-light-elite': isCaLight }">
    <!-- Luxury Animated Mesh Layer (Isolated to this component) -->
    <div class="vivid-mesh-system">
      <div class="mesh-blob mesh-blob-1"></div>
      <div class="mesh-blob mesh-blob-2"></div>
      <div class="mesh-blob mesh-blob-3"></div>
    </div>

    <!-- Nebula Stars Layer (Existing) -->
    <div class="nebula-cloud blue"></div>
    <div class="nebula-cloud gold"></div>
    <div class="nebula-cloud purple"></div>
    <div class="noise-overlay"></div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import { useThemeStore } from '@/stores/themeStore'

const authStore = useAuthStore()
const themeStore = useThemeStore()

const isCaLight = computed(() => {
  return authStore.isCompanyAdmin && !themeStore.isDark
})
</script>

<style scoped>
.celestial-bg-system {
  display: none; /* Strictly hidden in Light Mode for non-admins */
}

/* ─── ELITE LIGHT MODE FOR COMPANY ADMIN ─── */
.ca-light-elite {
  display: block !important;
  position: fixed;
  inset: 0;
  z-index: -9999 !important;
  background: #f7fbff; /* Base Light Sky */
}

.ca-light-elite .mesh-blob {
  opacity: 0.28; /* Increased from 0.15 for better visibility */
  mix-blend-mode: multiply;
  filter: blur(140px);
  transition: all 1.5s ease-in-out;
}

.ca-light-elite .mesh-blob-1 {
  background: radial-gradient(circle, #fed7aa 0%, transparent 70%); /* More saturated Peach */
  top: -10%;
  left: -5%;
  width: 65vw;
  height: 65vw;
}

.ca-light-elite .mesh-blob-2 {
  background: radial-gradient(circle, #bae6fd 0%, transparent 70%); /* More saturated Sky Blue */
  bottom: -5%;
  right: -5%;
  width: 55vw;
  height: 55vw;
}

.ca-light-elite .mesh-blob-3 {
  background: radial-gradient(circle, #e0f2fe 0%, transparent 70%); /* Soft Cyan */
  top: 25%;
  left: 15%;
  opacity: 0.15;
}

/* HIDE NEBULA CLOUDS IN LIGHT ADMIN MODE */
.ca-light-elite .nebula-cloud { display: none; }

/* ─── DARK MODE (EXISTING) ─── */
:global(.dark-mode) .celestial-bg-system {
  display: block;
  position: fixed;
  inset: 0;
  pointer-events: none !important;
  z-index: -9999 !important;
  background: #020617; /* Base OLED Black */
}

/* Force pointer-events none on the background container and all descendants in all states */
.celestial-bg-system,
.celestial-bg-system *,
.ca-light-elite,
.ca-light-elite *,
:global(.dark-mode) .celestial-bg-system,
:global(.dark-mode) .celestial-bg-system * {
  pointer-events: none !important;
}

.nebula-cloud {
  position: absolute;
  width: 600px;
  height: 600px;
  border-radius: 50%;
  filter: blur(120px);
  transition: 1s cubic-bezier(0.4, 0, 0.2, 1);
  opacity: 0.35;
}

.nebula-cloud.blue {
  top: -100px;
  right: -50px;
  background: #0369a1;
  animation: float 15s infinite alternate;
}

.nebula-cloud.gold {
  bottom: 0%;
  left: 10%;
  background: #92400e;
  animation: float 20s infinite alternate-reverse;
}

.nebula-cloud.purple {
  top: 20%;
  right: 15%;
  background: #a855f7;
  animation: float 18s infinite alternate;
  opacity: 0.25;
}

.noise-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: url("data:image/svg+xml,%3Csvg viewBox='0 0 200 200' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='noiseFilter'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.65' numOctaves='3' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23noiseFilter)'/%3E%3C/svg%3E");
  opacity: 0.02;
  contrast: 150%;
  brightness: 100%;
}

@keyframes float {
  from { transform: translate(0, 0) scale(1); }
  to { transform: translate(30px, 40px) scale(1.1); }
}
</style>
