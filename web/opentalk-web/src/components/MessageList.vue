<template>
  <div class="message-scroll-wrap" ref="scrollWrapRef" @scroll="onScroll">
    <div class="top-sentinel">
      <div v-if="loadingMore" class="loading-more">
        <span class="spinner"></span>
        <span>加载历史消息...</span>
      </div>
      <div v-else-if="noMore" class="no-more">— 已加载全部消息 —</div>
    </div>

    <div
      v-for="m in messages"
      :key="m.id"
      class="message-row"
      :class="m.senderId === selfId ? 'message-self' : 'message-other'"
    >
      <div class="bubble-wrap" :class="m.senderId === selfId ? 'bubble-wrap-self' : 'bubble-wrap-other'">
        <div class="avatar-wrap">
          <div class="avatar" :style="avatarStyle(m.senderId)">
            <img v-if="avatarOf(m.senderId)" :src="avatarOf(m.senderId)" class="avatar-img" />
            <span v-else class="avatar-letter">{{ displayName(m.senderId).charAt(0) }}</span>
          </div>
        </div>
        <div class="message-content-col">
          <div class="message-meta" :class="m.senderId === selfId ? 'meta-self' : 'meta-other'">
            <span class="sender-name">{{ displayName(m.senderId) }}</span>
            <span class="msg-time">{{ formatTime(m.createdAt) }}</span>
          </div>

          <div v-if="m.type === 1" class="bubble" :class="m.senderId === selfId ? 'bubble-self' : 'bubble-other'">
            {{ m.content }}
          </div>

          <div v-else-if="m.type === 2" class="bubble bubble-media" :class="m.senderId === selfId ? 'bubble-self' : 'bubble-other'">
            <el-image
              :src="apiBase + m.fileUrl"
              style="max-width:240px;max-height:200px;border-radius:8px;display:block;"
              fit="contain"
              :preview-src-list="[apiBase + m.fileUrl]"
            />
          </div>

          <div v-else-if="m.type === 3" class="bubble bubble-file" :class="m.senderId === selfId ? 'bubble-self' : 'bubble-other'">
            <div class="file-icon-wrap">
              <svg viewBox="0 0 24 24" fill="none" width="22" height="22">
                <path d="M13 2H6C5.47 2 4.96 2.21 4.59 2.59C4.21 2.96 4 3.47 4 4V20C4 20.53 4.21 21.04 4.59 21.41C4.96 21.79 5.47 22 6 22H18C18.53 22 19.04 21.79 19.41 21.41C19.79 21.04 20 20.53 20 20V9L13 2Z" stroke="var(--ot-accent)" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
                <polyline points="13 2 13 9 20 9" stroke="var(--ot-accent)" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
              </svg>
            </div>
            <div class="file-details">
              <span class="file-name">{{ m.content }}</span>
              <div class="file-actions">
                <a :href="apiBase + m.fileUrl" target="_blank" class="file-action-link">下载</a>
                <button v-if="isTextFile(m.content)" class="file-action-btn" @click="previewText(m)">预览</button>
              </div>
            </div>
          </div>

          <div v-else class="bubble" :class="m.senderId === selfId ? 'bubble-self' : 'bubble-other'">
            <a :href="apiBase + m.fileUrl" target="_blank" class="file-action-link">{{ m.content }}</a>
          </div>
        </div>
      </div>
    </div>
  </div>

  <el-dialog v-model="textPreviewVisible" :title="textPreviewTitle" width="60%" top="5vh">
    <el-scrollbar style="height:60vh">
      <pre class="text-preview-content">{{ textPreviewContent }}</pre>
    </el-scrollbar>
    <template #footer>
      <button class="dialog-close-btn" @click="textPreviewVisible = false">关闭</button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'
import { ElMessage } from 'element-plus'

type Message = { id: number; senderId: number; type: number; content: string; fileUrl?: string; createdAt?: string }
type Member = { userId: number; username: string; nickname?: string; avatar?: string }
type SelfUser = { Nickname?: string; Username?: string; Avatar?: string }

const props = defineProps<{
  messages: Message[]
  selfId: number
  members: Member[]
  selfUser: SelfUser
  apiBase: string
  loadingMore?: boolean
  noMore?: boolean
}>()

const emit = defineEmits<{ (e: 'load-more'): void }>()

const scrollWrapRef = ref<HTMLDivElement>()
const textPreviewVisible = ref(false)
const textPreviewTitle = ref('')
const textPreviewContent = ref('')

const TEXT_EXTS = ['.txt','.md','.log','.csv','.json','.xml','.yaml','.yml','.ini','.conf','.ts','.js','.html','.css','.py','.java','.cs','.go','.sh']

function isTextFile(name: string) {
  const lower = (name || '').toLowerCase()
  return TEXT_EXTS.some(ext => lower.endsWith(ext))
}

async function previewText(m: Message) {
  if (!m.fileUrl) return
  try {
    const res = await fetch(props.apiBase + m.fileUrl)
    if (!res.ok) throw new Error('')
    textPreviewTitle.value = m.content
    textPreviewContent.value = await res.text()
    textPreviewVisible.value = true
  } catch {
    ElMessage.error('预览失败，请尝试下载查看')
  }
}

function scrollToBottom() {
  nextTick(() => {
    const el = scrollWrapRef.value
    if (el) el.scrollTop = el.scrollHeight
  })
}

let prevScrollHeight = 0

function onScroll() {
  const el = scrollWrapRef.value
  if (!el) return
  if (el.scrollTop < 80 && !props.loadingMore && !props.noMore) {
    prevScrollHeight = el.scrollHeight
    emit('load-more')
  }
}

watch(
  () => props.messages.length,
  (newLen, oldLen) => {
    if (newLen > oldLen) {
      nextTick(() => {
        const el = scrollWrapRef.value
        if (el && prevScrollHeight > 0) {
          const delta = el.scrollHeight - prevScrollHeight
          if (delta > 0) { el.scrollTop = delta; prevScrollHeight = 0; return }
        }
        scrollToBottom()
      })
    }
  }
)

defineExpose({ scrollToBottom })

const AVATAR_COLORS = [
  ['#3b82f6','#6366f1'],['#10b981','#059669'],['#f59e0b','#d97706'],
  ['#ef4444','#dc2626'],['#8b5cf6','#7c3aed'],['#06b6d4','#0891b2']
]

function avatarStyle(uid: number) {
  const idx = uid % AVATAR_COLORS.length
  const [a, b] = AVATAR_COLORS[idx]
  return { background: `linear-gradient(135deg, ${a}, ${b})` }
}

function displayName(senderId: number) {
  if (senderId === props.selfId) return props.selfUser.Nickname || props.selfUser.Username || `用户 ${senderId}`
  const m = props.members.find(x => x.userId === senderId)
  return m ? (m.nickname || m.username || `用户 ${senderId}`) : `用户 ${senderId}`
}

function avatarOf(uid: number) {
  if (uid === props.selfId) return props.selfUser.Avatar
  return props.members.find(x => x.userId === uid)?.avatar
}

function formatTime(value?: string) {
  if (!value) return ''
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return ''
  const now = new Date()
  const isToday = d.toDateString() === now.toDateString()
  const hh = d.getHours().toString().padStart(2,'0')
  const mm = d.getMinutes().toString().padStart(2,'0')
  if (isToday) return `${hh}:${mm}`
  return `${(d.getMonth()+1).toString().padStart(2,'0')}-${d.getDate().toString().padStart(2,'0')} ${hh}:${mm}`
}
</script>

<style scoped>
.message-scroll-wrap {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 20px 24px 12px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  background: var(--ot-bg-base);
}

.message-scroll-wrap::-webkit-scrollbar { width: 4px; }
.message-scroll-wrap::-webkit-scrollbar-track { background: transparent; }
.message-scroll-wrap::-webkit-scrollbar-thumb { background: var(--ot-border); border-radius: 2px; }

.top-sentinel {
  min-height: 28px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 8px;
}

.loading-more {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--ot-text-muted);
  font-size: 12px;
}

.spinner {
  width: 14px;
  height: 14px;
  border: 2px solid var(--ot-border);
  border-top-color: var(--ot-accent);
  border-radius: 50%;
  animation: spin 0.7s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }

.no-more {
  color: var(--ot-text-muted);
  font-size: 11px;
  letter-spacing: 0.06em;
}

.message-row {
  display: flex;
  margin: 3px 0;
}

.message-self { justify-content: flex-end; }
.message-other { justify-content: flex-start; }

.bubble-wrap {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  max-width: 75%;
}

.bubble-wrap-self { flex-direction: row-reverse; }
.bubble-wrap-other { flex-direction: row; }

.avatar {
  width: 34px;
  height: 34px;
  min-width: 34px;
  border-radius: 9px;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.avatar-img { width: 100%; height: 100%; object-fit: cover; }
.avatar-letter { font-size: 14px; font-weight: 700; color: #fff; line-height: 1; }

.message-content-col {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.message-meta {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 11px;
}

.meta-self { flex-direction: row-reverse; }

.sender-name {
  color: var(--ot-text-secondary);
  font-weight: 500;
}

.msg-time {
  color: var(--ot-text-muted);
  font-size: 10px;
}

.bubble {
  padding: 9px 13px;
  border-radius: 14px;
  font-size: 14px;
  line-height: 1.55;
  word-break: break-word;
  max-width: 100%;
  position: relative;
}

.bubble-other {
  background: var(--ot-bg-bubble-other);
  color: var(--ot-text-primary);
  border-top-left-radius: 4px;
  border: 1px solid var(--ot-border);
}

.bubble-self {
  background: linear-gradient(135deg, var(--ot-accent-self-a), var(--ot-accent-self-b));
  color: #fff;
  border-top-right-radius: 4px;
  box-shadow: 0 2px 12px var(--ot-accent-glow);
}

.bubble-media {
  padding: 6px;
  background: transparent !important;
  border: none !important;
  box-shadow: none !important;
}

.bubble-file {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  min-width: 200px;
}

.file-icon-wrap {
  flex-shrink: 0;
  width: 38px;
  height: 38px;
  border-radius: 9px;
  background: var(--ot-accent-dim);
  display: flex;
  align-items: center;
  justify-content: center;
}

.file-details {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}

.file-name {
  font-size: 13px;
  font-weight: 500;
  word-break: break-all;
  color: var(--ot-text-primary);
}

.bubble-self .file-name { color: #fff; }

.file-actions {
  display: flex;
  gap: 10px;
  align-items: center;
}

.file-action-link {
  font-size: 12px;
  color: var(--ot-accent);
  text-decoration: none;
  transition: opacity 0.15s;
}
.file-action-link:hover { opacity: 0.75; }

.bubble-self .file-action-link { color: rgba(255,255,255,0.85); }

.file-action-btn {
  font-size: 12px;
  color: var(--ot-accent);
  background: transparent;
  border: none;
  cursor: pointer;
  padding: 0;
  font-family: inherit;
  transition: opacity 0.15s;
}
.file-action-btn:hover { opacity: 0.75; }

.bubble-self .file-action-btn { color: rgba(255,255,255,0.85); }

.text-preview-content {
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 13px;
  white-space: pre-wrap;
  word-break: break-all;
  line-height: 1.6;
  margin: 0;
  padding: 4px;
  color: var(--ot-text-primary);
}

.dialog-close-btn {
  padding: 8px 20px;
  border-radius: 8px;
  border: 1px solid var(--ot-border);
  background: transparent;
  color: var(--ot-text-secondary);
  font-size: 13px;
  cursor: pointer;
  font-family: inherit;
  transition: all 0.2s;
}
.dialog-close-btn:hover {
  border-color: var(--ot-accent-dim);
  color: var(--ot-accent);
}
</style>
