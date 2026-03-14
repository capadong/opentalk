<template>
  <div class="message-input-area">
    <ChatToolbar @emoji="onEmoji" @image="onToolbarImage" />
    <div v-if="pendingImageUrl" class="pending-preview">
      <el-image :src="pendingImageUrl" style="max-width:160px;max-height:160px" fit="contain" />
    </div>
    <div class="input-row">
      <el-input
        v-model="text"
        type="textarea"
        :autosize="{ minRows: 3, maxRows: 6 }"
        placeholder="输入消息，回车发送"
        @keyup.enter="handleSend"
      />
      <div class="send-btn-col">
        <el-button type="primary" @click="handleSend">发送</el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import ChatToolbar from './ChatToolbar.vue'

const emit = defineEmits<{
  (e: 'send-text', text: string): void
  (e: 'send-image', file: File): void
}>()

const text = ref('')
const pendingImage = ref<File | null>(null)
const pendingImageUrl = ref<string | null>(null)

function onEmoji(emoji: string) {
  text.value += emoji
}

function clearPending() {
  if (pendingImageUrl.value) URL.revokeObjectURL(pendingImageUrl.value)
  pendingImage.value = null
  pendingImageUrl.value = null
}

function setPendingImage(file: File) {
  clearPending()
  pendingImage.value = file
  pendingImageUrl.value = URL.createObjectURL(file)
}

function onToolbarImage(file: File) {
  if (file) setPendingImage(file)
}

function handleSend() {
  const hasText = !!text.value.trim()
  const hasImage = !!pendingImage.value
  if (!hasText && !hasImage) return
  if (hasText) emit('send-text', text.value.trim())
  if (hasImage && pendingImage.value) emit('send-image', pendingImage.value)
  text.value = ''
  clearPending()
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
  padding-bottom: 4px;
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
