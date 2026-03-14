<template>
  <el-scrollbar ref="scrollbarRef" class="message-scrollbar" @scroll="onScroll">
    <div class="top-sentinel">
      <div v-if="loadingMore" class="loading-more">
        <el-icon class="is-loading"><Loading /></el-icon>
        <span>加载历史消息...</span>
      </div>
      <div v-else-if="noMore" class="no-more">没有更多消息了</div>
    </div>
    <div
      v-for="m in messages"
      :key="m.id"
      class="message-row"
      :class="m.senderId === selfId ? 'message-self' : 'message-other'"
    >
      <div
        class="message-bubble-wrap"
        :class="m.senderId === selfId ? 'bubble-wrap-self' : 'bubble-wrap-other'"
      >
        <el-avatar :size="36" :src="avatarOf(m.senderId)">
          {{ displayName(m.senderId).charAt(0) }}
        </el-avatar>
        <div class="message-content-col">
          <div class="message-sender" :class="m.senderId === selfId ? 'sender-self' : 'sender-other'">
            {{ displayName(m.senderId) }}
            <span class="message-time">{{ formatTime(m.createdAt) }}</span>
          </div>
          <el-card shadow="never" class="message-card">
            <div v-if="m.type === 1">{{ m.content }}</div>
            <div v-else-if="m.type === 2">
              <el-image
                :src="apiBase + m.fileUrl"
                style="max-width:280px"
                fit="contain"
                :preview-src-list="[apiBase + m.fileUrl]"
              />
            </div>
            <div v-else-if="m.type === 3" class="file-msg">
              <el-icon class="file-icon"><Document /></el-icon>
              <div class="file-info">
                <span class="file-name">{{ m.content }}</span>
                <div class="file-actions">
                  <el-link :href="apiBase + m.fileUrl" target="_blank" type="primary">下载</el-link>
                  <el-link
                    v-if="isTextFile(m.content)"
                    type="success"
                    @click="previewText(m)"
                  >预览</el-link>
                </div>
              </div>
            </div>
            <div v-else>
              <el-link :href="apiBase + m.fileUrl" target="_blank">{{ m.content }}</el-link>
            </div>
          </el-card>
        </div>
      </div>
    </div>
  </el-scrollbar>

  <!-- 文本文件预览对话框 -->
  <el-dialog v-model="textPreviewVisible" :title="textPreviewTitle" width="60%" top="5vh">
    <el-scrollbar style="height:60vh">
      <pre class="text-preview-content">{{ textPreviewContent }}</pre>
    </el-scrollbar>
    <template #footer>
      <el-button @click="textPreviewVisible = false">关闭</el-button>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'
import { Loading, Document } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

type Message = {
  id: number
  senderId: number
  type: number
  content: string
  fileUrl?: string
  createdAt?: string
}

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

const emit = defineEmits<{
  (e: 'load-more'): void
}>()

const scrollbarRef = ref()
const textPreviewVisible = ref(false)
const textPreviewTitle = ref('')
const textPreviewContent = ref('')

const TEXT_EXTS = ['.txt', '.md', '.log', '.csv', '.json', '.xml', '.yaml', '.yml', '.ini', '.conf', '.ts', '.js', '.html', '.css', '.py', '.java', '.cs', '.go', '.sh']

function isTextFile(name: string) {
  const lower = (name || '').toLowerCase()
  return TEXT_EXTS.some(ext => lower.endsWith(ext))
}

async function previewText(m: Message) {
  if (!m.fileUrl) return
  try {
    const res = await fetch(props.apiBase + m.fileUrl)
    if (!res.ok) throw new Error('获取文件失败')
    const text = await res.text()
    textPreviewTitle.value = m.content
    textPreviewContent.value = text
    textPreviewVisible.value = true
  } catch {
    ElMessage.error('预览失败，请尝试下载查看')
  }
}

function scrollToBottom() {
  nextTick(() => {
    const wrap = scrollbarRef.value?.wrapRef
    if (wrap) wrap.scrollTop = wrap.scrollHeight
  })
}

let prevScrollHeight = 0

function onScroll({ scrollTop }: { scrollTop: number }) {
  if (scrollTop < 60 && !props.loadingMore && !props.noMore) {
    const wrap = scrollbarRef.value?.wrapRef
    prevScrollHeight = wrap ? wrap.scrollHeight : 0
    emit('load-more')
  }
}

watch(
  () => props.messages.length,
  (newLen, oldLen) => {
    if (newLen > oldLen) {
      nextTick(() => {
        const wrap = scrollbarRef.value?.wrapRef
        if (wrap && prevScrollHeight > 0) {
          const delta = wrap.scrollHeight - prevScrollHeight
          if (delta > 0) {
            wrap.scrollTop = delta
            prevScrollHeight = 0
            return
          }
        }
        scrollToBottom()
      })
    }
  }
)

defineExpose({ scrollToBottom })

function displayName(senderId: number) {
  if (senderId === props.selfId) {
    return props.selfUser.Nickname || props.selfUser.Username || `用户 ${senderId}`
  }
  const m = props.members.find(x => x.userId === senderId)
  return m ? (m.nickname || m.username || `用户 ${senderId}`) : `用户 ${senderId}`
}

function avatarOf(userIdValue: number) {
  if (userIdValue === props.selfId) return props.selfUser.Avatar
  return props.members.find(x => x.userId === userIdValue)?.avatar
}

function formatTime(value?: string) {
  if (!value) return ''
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return ''
  const now = new Date()
  const isToday = d.toDateString() === now.toDateString()
  const hh = d.getHours().toString().padStart(2, '0')
  const mm = d.getMinutes().toString().padStart(2, '0')
  if (isToday) return `${hh}:${mm}`
  const month = (d.getMonth() + 1).toString().padStart(2, '0')
  const day = d.getDate().toString().padStart(2, '0')
  return `${month}-${day} ${hh}:${mm}`
}
</script>

<style scoped>
.message-scrollbar {
  flex: 1;
  padding: 12px 16px;
}
.top-sentinel {
  min-height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 8px;
}
.loading-more {
  display: flex;
  align-items: center;
  gap: 6px;
  color: var(--el-text-color-secondary);
  font-size: 13px;
}
.no-more {
  color: var(--el-text-color-placeholder);
  font-size: 12px;
}
.message-row {
  margin: 8px 0;
  display: flex;
}
.message-self {
  justify-content: flex-end;
}
.message-other {
  justify-content: flex-start;
}
.message-bubble-wrap {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  max-width: 80%;
}
.bubble-wrap-self {
  flex-direction: row-reverse;
}
.bubble-wrap-other {
  flex-direction: row;
}
.message-content-col {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}
.message-sender {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  display: flex;
  align-items: center;
  gap: 6px;
}
.sender-self {
  flex-direction: row-reverse;
}
.message-time {
  font-size: 11px;
  opacity: 0.7;
}
.message-card {
  max-width: 520px;
}
.file-msg {
  display: flex;
  align-items: center;
  gap: 10px;
}
.file-icon {
  font-size: 28px;
  color: var(--el-color-primary);
  flex-shrink: 0;
}
.file-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  min-width: 0;
}
.file-name {
  font-size: 14px;
  word-break: break-all;
}
.file-actions {
  display: flex;
  gap: 12px;
}
.text-preview-content {
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 13px;
  white-space: pre-wrap;
  word-break: break-all;
  line-height: 1.6;
  margin: 0;
  padding: 4px;
}
</style>
