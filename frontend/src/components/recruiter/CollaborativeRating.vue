<template>
  <div class="r-card rating-card premium-glass anim-reveal-up">
    <div class="card-header-compact">
      <div class="header-icon-box">
        <Star :size="20" class="star-icon" />
      </div>
      <h3 class="card-title-v7">Évaluation Collaborative</h3>
    </div>

    <div class="rating-body">
      <p class="rating-desc">Notez ce candidat pour affiner le score collectif.</p>
      
      <!-- Star Rating Interaction -->
      <div class="stars-interaction">
        <button 
          v-for="star in 5" 
          :key="star"
          class="star-btn"
          :class="{ 
            active: star <= (hoverRating || currentRating),
            hovering: star <= hoverRating,
            just_rated: star === currentRating && !hoverRating
          }"
          @mouseenter="hoverRating = star"
          @mouseleave="hoverRating = 0"
          @click="saveRating(star)"
        >
          <Star :size="32" :fill="star <= (hoverRating || currentRating) ? 'currentColor' : 'none'" stroke-width="2.5" />
        </button>
      </div>

      <div class="rating-status" v-if="currentRating > 0">
        <span class="rating-label">Votre note : <strong>{{ currentRating }}/5</strong></span>
        <div v-if="loading" class="mini-loader spin">
          <Loader2 :size="14" />
        </div>
      </div>

      <!-- Collective Info -->
      <div class="collective-stats" v-if="candidate?.averageRecruiterRating">
        <div class="stat-row">
          <span>Moyenne des recruteurs</span>
          <span class="stat-val">{{ candidate.averageRecruiterRating.toFixed(1) }}/5</span>
        </div>
        <div class="progress-track-sm">
          <div class="progress-bar-sm" :style="{ width: (candidate.averageRecruiterRating * 20) + '%' }"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { Star, Loader2 } from 'lucide-vue-next';
import api from '@/api/axios';

const props = defineProps({
  candidate: Object,
  applicationId: String
});

const emit = defineEmits(['update']);

const currentRating = ref(0);
const hoverRating = ref(0);
const loading = ref(false);

const saveRating = async (score) => {
  if (loading.value) return;
  loading.value = true;
  try {
    await api.post(`/Recruiter/applications/${props.applicationId}/ratings`, {
      score: score,
      comment: "" 
    });
    currentRating.value = score;
    emit('update'); // Notify parent to refresh average score
  } catch (error) {
    console.error(error);
  } finally {
    loading.value = false;
  }
};

onMounted(() => {
  // Check if candidate already has a rating from this recruiter
  // In a real app, this would come from the candidate object or a specific endpoint
  if (props.candidate?.myRating) {
    currentRating.value = props.candidate.myRating;
  }
});
</script>

<style scoped>
.rating-card { padding: 24px; }
.card-header-compact { display: flex; align-items: center; gap: 12px; margin-bottom: 20px; }
.header-icon-box {
  width: 40px; height: 40px; border-radius: 12px;
  background: rgba(245, 158, 11, 0.1); color: #f59e0b;
  display: flex; align-items: center; justify-content: center;
}
.card-title-v7 { font-size: 16px; font-weight: 800; margin: 0; }

.rating-desc { color: var(--r-text-sub); font-size: 13px; margin-bottom: 20px; }

.stars-interaction {
  display: flex; gap: 8px; justify-content: center; margin-bottom: 24px;
}

.star-btn {
  background: none; border: none; padding: 4px;
  color: #cbd5e1; cursor: pointer;
  transition: all 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.dark-mode .star-btn { color: rgba(255, 255, 255, 0.1); }

.star-btn:hover { transform: scale(1.3) rotate(8deg); }
.star-btn.active { color: #f59e0b; filter: drop-shadow(0 0 8px rgba(245, 158, 11, 0.4)); }
.star-btn.hovering { color: #fbbf24; }

@keyframes star-pop {
  0% { transform: scale(1); }
  50% { transform: scale(1.5); }
  100% { transform: scale(1); }
}

.star-btn.just_rated {
  animation: star-pop 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
}

.rating-status {
  display: flex; align-items: center; justify-content: center; gap: 10px;
  margin-bottom: 24px; font-size: 14px;
}

.collective-stats {
  padding-top: 20px; border-top: 1px solid var(--r-border);
}

.stat-row { display: flex; justify-content: space-between; font-size: 12px; font-weight: 700; color: var(--r-text-sub); margin-bottom: 8px; }
.stat-val { color: var(--r-text-main); font-weight: 900; }

.progress-track-sm { height: 6px; background: var(--r-main-bg); border-radius: 100px; overflow: hidden; }
.progress-bar-sm { height: 100%; background: #f59e0b; border-radius: 100px; transition: width 0.8s cubic-bezier(0.165, 0.84, 0.44, 1); }

.spin { animation: rotate 2s linear infinite; }
@keyframes rotate { from { transform: rotate(0); } to { transform: rotate(360deg); } }
</style>
