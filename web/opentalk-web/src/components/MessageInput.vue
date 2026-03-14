<template>
  <div class="message-input-area">
    <ChatToolbar @emoji="onEmoji" @image="onToolbarImage" @file="onToolbarFile" />
    <div v-if="pendingImageUrl" class="pending-preview">
      <div class="pending-label">图片预览:</div>
      <el-image :src="pendingImageUrl" style="max-width:160px;max-height:160px" fit="contain" />
      <el-button size="small" circle type="danger" @click="clearPending">✕</el-button>
    </div>
    <div v-if="pendingFile && !pendingImageUrl" class="pending-file">
      <el-icon><Document /></el-icon>
      <span class="pending-file-name">{{ pendingFile.name }}</span>
      <span class="pending-file-size">({{ formatSize(pendingFile.size) }})</span>
      <el-button size="small" circle type="danger" @click="clearPending">✕</el-button>
    </div>
    <div class="input-row">
      <el-input
        v-model="text"
        type="textarea"
        :autosize="{ minRows: 3, maxRows: 6 }"
        placeholder="输入消息，Ctrl+Enter 发送"
        @keydown="handleKeydown"
      />
      <div class="send-btn-col">
        <el-button type="primary" @click="handleSend">发送</el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { Document } from '@element-plus/icons-vue'
import ChatToolbar from './ChatToolbar.vue'

const emit = defineEmits<{
  (e: 'send-text', text: string): void
  (e: 'send-image', file: File): void
  (e: 'send-file', file: File): void
}>()

const text = ref('')
const pendingImage = ref<File | null>(null)
const pendingImageUrl = ref<string | null>(null)
const pendingFile = ref<File | null>(null)

function formatSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

function onEmoji(emoji: string) {
  text.value += emoji
}

function clearPending() {
  if (pendingImageUrl.value) URL.revokeObjectURL(pendingImageUrl.value)
  pendingImage.value = null
  pendingImageUrl.value = null
  pendingFile.value = null
}

function setPendingImage(file: File) {
  clearPending()
  pendingImage.value = file
  pendingImageUrl.value = URL.createObjectURL(file)
}

function setPendingFile(file: File) {
  clearPending()
  pendingFile.value = file
}

function onToolbarImage(file: File) {
  if (file) setPendingImage(file)
}

function onToolbarFile(file: File) {
  if (file) setPendingFile(file)
}

function handleSend() {
  const hasText = !!text.value.trim()
  const hasImage = !!pendingImage.value
  const hasFile = !!pendingFile.value
  if (!hasText && !hasImage && !hasFile) return
  if (hasText) emit('send-text', text.value.trim())
  if (hasImage && pendingImage.value) emit('send-image', pendingImage.value)
  if (hasFile && pendingFile.value) emit('send-file', pendingFile.value)
  text.value = ''
  clearPending()
}

function handleKeydown(e: KeyboardEvent) {
  if (e.key === 'Enter' && e.ctrlKey) {
    e.preventDefault()
    handleSend()
  }
}

function handlePaste(e: ClipboardEvent) {
  const items = e.clipboardData?.items
  if (!items) return
  for (const item of items) {
    if (item.type.startsWith('image/')) {
      const file = item.getAsFile()
      if (file) {
        setPendingImage(file)
        e.preventDefault()
        break
      }
    }
  }
}

onMounted(() => window.addEventListener('paste', handlePaste))
onUnmounted(() => {
  window.removeEventListener('paste', handlePaste)
  clearPending()
})
</script>

<style scoped>
.message-input-area {
  border-top: 1px solid var(--el-border-color);
  padding: 8px 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.pending-preview {
  display: flex;
  align-items: center;
  gap: 8px;
  padding-bottom: 4px;
}
.pending-label {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}
.pending-file {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 10px;
  background: var(--el-fill-color-light);
  border-radius: 6px;
  font-size: 13px;
}
.pending-file-name {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 280px;
}
.pending-file-size {
  color: var(--el-text-color-secondary);
  white-space: nowrap;
}
.input-row {
  display: flex;
  align-items: flex-end;
  gap: 12px;
}
.send-btn-col {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
</style>
