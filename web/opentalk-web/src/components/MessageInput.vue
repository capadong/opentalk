<template>
  <div class="message-input-area">
    <ChatToolbar @emoji="onEmoji" @image="onToolbarImage" @file="onToolbarFile" />
    <div v-if="pendingImageUrl" class="pending-preview">
      <span class="pending-label">图片预览</span>
      <div class="pending-img-wrap">
        <el-image :src="pendingImageUrl" style="max-width:120px;max-height:100px;border-radius:8px;" fit="contain" />
        <button class="clear-btn" @click="clearPending">
          <svg viewBox="0 0 16 16" fill="none" width="12" height="12">
            <path d="M12 4L4 12M4 4L12 12" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
          </svg>
        </button>
      </div>
    </div>
    <div v-if="pendingFile && !pendingImageUrl" class="pending-file">
      <svg viewBox="0 0 24 24" fill="none" width="16" height="16">
        <path d="M13 2H6C5.47 2 4.96 2.21 4.59 2.59C4.21 2.96 4 3.47 4 4V20C4 20.53 4.21 21.04 4.59 21.41C4.96 21.79 5.47 22 6 22H18C18.53 22 19.04 21.79 19.41 21.41C19.79 21.04 20 20.53 20 20V9L13 2Z" stroke="var(--ot-accent)" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
        <polyline points="13 2 13 9 20 9" stroke="var(--ot-accent)" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
      </svg>
      <span class="pending-file-name">{{ pendingFile.name }}</span>
      <span class="pending-file-size">{{ formatSize(pendingFile.size) }}</span>
      <button class="clear-btn" @click="clearPending">
        <svg viewBox="0 0 16 16" fill="none" width="12" height="12">
          <path d="M12 4L4 12M4 4L12 12" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
        </svg>
      </button>
    </div>
    <div class="input-row">
      <textarea
        v-model="text"
        class="msg-textarea"
        placeholder="输入消息，Ctrl+Enter 发送"
        rows="3"
        @keydown="handleKeydown"
      ></textarea>
      <button class="send-btn" :class="{ 'send-btn-active': canSend }" @click="handleSend">
        <svg viewBox="0 0 24 24" fill="none" width="18" height="18">
          <path d="M22 2L11 13" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          <path d="M22 2L15 22L11 13L2 9L22 2Z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
        <span>发送</span>
      </button>
    </div>
    <div class="input-hint">Ctrl+Enter 快速发送</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
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

const canSend = computed(() => !!text.value.trim() || !!pendingImage.value || !!pendingFile.value)

function formatSize(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`
}

function onEmoji(emoji: string) { text.value += emoji }

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

function onToolbarImage(file: File) { if (file) setPendingImage(file) }
function onToolbarFile(file: File) { if (file) setPendingFile(file) }

function handleSend() {
  if (!canSend.value) return
  if (text.value.trim()) emit('send-text', text.value.trim())
  if (pendingImage.value) emit('send-image', pendingImage.value)
  if (pendingFile.value) emit('send-file', pendingFile.value)
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
      if (file) { setPendingImage(file); e.preventDefault(); break }
    }
  }
}

onMounted(() => window.addEventListener('paste', handlePaste))
onUnmounted(() => { window.removeEventListener('paste', handlePaste); clearPending() })
</script>

<style scoped>
.message-input-area {
  border-top: 1px solid var(--ot-border);
  padding: 10px 16px 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
  background: var(--ot-bg-panel);
  flex-shrink: 0;
}

.pending-preview {
  display: flex;
  align-items: center;
  gap: 10px;
}

.pending-label {
  font-size: 11px;
  color: var(--ot-text-muted);
  letter-spacing: 0.04em;
}

.pending-img-wrap {
  position: relative;
  display: inline-flex;
}

.pending-file {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: var(--ot-bg-input);
  border: 1px solid var(--ot-border);
  border-radius: 8px;
  font-size: 13px;
}

.pending-file-name {
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 260px;
  color: var(--ot-text-primary);
}

.pending-file-size {
  color: var(--ot-text-muted);
  white-space: nowrap;
  font-size: 12px;
}

.clear-btn {
  width: 22px;
  height: 22px;
  border-radius: 50%;
  background: var(--ot-bg-hover);
  border: 1px solid var(--ot-border);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: var(--ot-text-secondary);
  transition: all 0.15s;
  flex-shrink: 0;
  padding: 0;
}

.clear-btn:hover {
  background: var(--ot-red-glow);
  border-color: var(--ot-red-dim);
  color: var(--ot-red);
}

.input-row {
  display: flex;
  align-items: flex-end;
  gap: 10px;
}

.msg-textarea {
  flex: 1;
  background: var(--ot-bg-input);
  border: 1px solid var(--ot-border);
  border-radius: 10px;
  padding: 10px 14px;
  font-size: 14px;
  color: var(--ot-text-primary);
  font-family: inherit;
  resize: none;
  outline: none;
  line-height: 1.5;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.msg-textarea::placeholder { color: var(--ot-text-muted); }

.msg-textarea:focus {
  border-color: var(--ot-accent-dim);
  box-shadow: 0 0 0 3px var(--ot-accent-glow);
}

.send-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 10px 18px;
  border-radius: 10px;
  border: 1px solid var(--ot-border);
  background: var(--ot-bg-hover);
  color: var(--ot-text-muted);
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  font-family: inherit;
  transition: all 0.2s;
  white-space: nowrap;
  letter-spacing: 0.02em;
}

.send-btn-active {
  background: linear-gradient(135deg, var(--ot-accent-self-a), var(--ot-accent-self-b));
  border-color: transparent;
  color: #fff;
  box-shadow: 0 2px 14px var(--ot-accent-glow);
}

.send-btn-active:hover {
  filter: brightness(1.1);
  box-shadow: 0 4px 20px var(--ot-accent-glow);
}

.input-hint {
  font-size: 11px;
  color: var(--ot-text-muted);
  text-align: right;
  letter-spacing: 0.03em;
}
</style>
