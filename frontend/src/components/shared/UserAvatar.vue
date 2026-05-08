<template>
  <!-- Shows image if src is available, else colored initials fallback -->
  <div class="avatar-shell" :style="shellStyle" :title="name">
    <img
      v-if="resolvedSrc"
      :src="resolvedSrc"
      :alt="name"
      class="avatar-img"
      @error="onImgError"
    />
    <span v-else class="avatar-initials">{{ initials }}</span>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

const props = defineProps({
  /** URL of the profile picture or logo */
  src: { type: String, default: '' },
  /** Full name or company name (used for initials and title) */
  name: { type: String, default: '' },
  /** Fallback gradient when no image (e.g. "linear-gradient(...)") */
  gradient: { type: String, default: 'linear-gradient(135deg, #6C63FF, #A463FF)' },
  /** Size in px */
  size: { type: Number, default: 38 },
  /** Border-radius value e.g. "10px" or "50%" */
  radius: { type: String, default: '10px' },
})

const imgError = ref(false)

const backendOrigin = (() => {
  const envApiUrl = (typeof import.meta !== 'undefined' && import.meta.env && import.meta.env.VITE_API_URL)
    ? import.meta.env.VITE_API_URL
    : ''

  if (envApiUrl) {
    try {
      return new URL(envApiUrl).origin
    } catch {
      // Fall through to browser origin when env URL is malformed.
    }
  }

  if (typeof window !== 'undefined' && window.location?.origin) {
    return window.location.origin
  }

  return ''
})()

// Reset error when src changes
watch(() => props.src, () => { imgError.value = false })

const resolvedSrc = computed(() => {
  if (imgError.value) return null
  const s = props.src?.trim()
  if (!s) return null

  // Accept already-absolute URLs and in-memory image URLs as-is.
  if (/^(https?:|data:|blob:)/i.test(s)) return s

  // Protocol-relative URL (e.g. //cdn.example.com/image.png).
  if (s.startsWith('//')) {
    const protocol = typeof window !== 'undefined' ? window.location.protocol : 'https:'
    return `${protocol}${s}`
  }

  // Convert backend-relative paths such as /uploads/avatar.png.
  if (s.startsWith('/')) {
    return backendOrigin ? `${backendOrigin}${s}` : s
  }

  // Convert loose relative paths like uploads/avatar.png.
  return backendOrigin ? `${backendOrigin}/${s.replace(/^\.?\//, '')}` : s
})

const initials = computed(() => {
  const parts = (props.name || '').trim().split(/\s+/).filter(Boolean)
  if (parts.length === 0) return '?'
  if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
})

const shellStyle = computed(() => ({
  width: props.size + 'px',
  height: props.size + 'px',
  borderRadius: props.radius,
  background: resolvedSrc.value ? 'transparent' : props.gradient,
  flexShrink: 0,
}))

const onImgError = () => { imgError.value = true }
</script>

<style scoped>
.avatar-shell {
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  position: relative;
}

.avatar-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.avatar-initials {
  color: white;
  font-weight: 800;
  font-size: calc(var(--av-font, 13px));
  letter-spacing: 0.3px;
  line-height: 1;
  user-select: none;
}
</style>
