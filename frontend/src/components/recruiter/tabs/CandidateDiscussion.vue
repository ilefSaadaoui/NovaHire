<template>
  <div class="panel-fade">
    <div class="r-card premium-inner-card discussion-thread">
      <div v-if="candidate.comments && candidate.comments.length > 0" class="comment-list">
        <div v-for="comment in candidate.comments" :key="comment.id" class="comment-item">
          <div class="comment-avatar" :style="{ background: accentColor }">{{ comment.authorName[0] }}</div>
          <div class="comment-content-box">
            <div class="comment-meta">
              <span class="comment-author">{{ comment.authorName }}</span>
              <span class="comment-time">{{ comment.timeAgo }}</span>
            </div>
            <p class="comment-text">{{ comment.content }}</p>
          </div>
        </div>
      </div>
      <div v-else class="empty-discussion">
        <MessageSquare :size="48" stroke-width="1.5" class="empty-icon" />
        <p>Aucun commentaire pour le moment. Soyez le premier à noter ce candidat.</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { MessageSquare } from 'lucide-vue-next'

defineProps({
  candidate: Object,
  accentColor: String
})
</script>

<style scoped>
.premium-inner-card {
  background: var(--glass-bg);
  backdrop-filter: blur(25px) saturate(200%);
  border: 1px solid var(--glass-border-color);
  border-radius: 32px;
  padding: 24px;
  box-shadow: var(--premium-shadow);
}

.comment-list { display: flex; flex-direction: column; gap: 24px; }
.comment-item { display: flex; gap: 16px; }
.comment-avatar {
  width: 40px; height: 40px; border-radius: 12px;
  display: flex; align-items: center; justify-content: center;
  color: white; font-weight: 800; font-size: 16px; flex-shrink: 0;
}
.comment-content-box { 
  flex: 1; 
  background: rgba(255,255,255,0.03); 
  border-radius: 20px; 
  padding: 16px 20px; 
  border: 1px solid var(--r-border); 
}
.comment-meta { display: flex; justify-content: space-between; margin-bottom: 6px; }
.comment-author { font-size: 13px; font-weight: 800; color: var(--r-text-main); }
.comment-time { font-size: 11px; font-weight: 600; color: var(--r-text-sub); }
.comment-text { font-size: 14px; color: var(--r-text-main); line-height: 1.6; opacity: 0.9; margin: 0; }

.empty-discussion {
  text-align: center;
  padding: 60px 40px;
  color: var(--r-text-sub);
}
.empty-icon { margin-bottom: 24px; opacity: 0.3; color: var(--accent); }

.panel-fade { animation: fadeIn 0.4s ease-out; }
@keyframes fadeIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: translateY(0); } }
/* Dark Mode Overrides - Premium Enhancement */
.dark-mode .premium-inner-card {
  background: #0f172a;
  border-color: var(--accent-soft);
}

.dark-mode .comment-content-box {
  background: rgba(2, 6, 23, 0.3);
  border-color: rgba(255, 255, 255, 0.05);
}

.dark-mode .comment-author {
  color: var(--accent);
}

.dark-mode .comment-text {
  color: #f8fafc;
}

.dark-mode .empty-discussion p {
  color: #94a3b8;
}
</style>
