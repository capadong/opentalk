<template>
  <div class="chat-toolbar">
    <button class="tool-btn" title="表情" @click="onEmojiClick">
      <svg viewBox="0 0 24 24" fill="none" width="18" height="18">
        <circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="1.5"/>
        <path d="M8.5 14.5C8.5 14.5 9.5 16 12 16C14.5 16 15.5 14.5 15.5 14.5" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
        <circle cx="9.5" cy="10.5" r="1" fill="currentColor"/>
        <circle cx="14.5" cy="10.5" r="1" fill="currentColor"/>
      </svg>
      <span class="tool-label">表情</span>
    </button>
    <el-upload
      :auto-upload="false"
      :show-file-list="false"
      :on-change="onImageChange"
      accept="image/*"
      class="tool-upload"
    >
      <button class="tool-btn" title="图片">
        <svg viewBox="0 0 24 24" fill="none" width="18" height="18">
          <rect x="3" y="3" width="18" height="18" rx="3" stroke="currentColor" stroke-width="1.5"/>
          <circle cx="8.5" cy="8.5" r="1.5" fill="currentColor" opacity="0.7"/>
          <path d="M3 15L8 10L12 14L15 11L21 17" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
        <span class="tool-label">图片</span>
      </button>
    </el-upload>
    <el-upload
      :auto-upload="false"
      :show-file-list="false"
      :on-change="onFileChange"
      class="tool-upload"
    >
      <button class="tool-btn" title="文件">
        <svg viewBox="0 0 24 24" fill="none" width="18" height="18">
          <path d="M21.44 11.05L12.25 20.24C11.12 21.37 9.59 22 7.99 22C6.39 22 4.86 21.37 3.73 20.24C2.6 19.11 1.97 17.58 1.97 15.98C1.97 14.38 2.6 12.85 3.73 11.72L12.92 2.53C13.68 1.77 14.71 1.34 15.79 1.34C16.87 1.34 17.9 1.77 18.66 2.53C19.42 3.29 19.85 4.32 19.85 5.4C19.85 6.48 19.42 7.51 18.66 8.27L9.46 17.46C9.08 17.84 8.57 18.06 8.03 18.06C7.49 18.06 6.97 17.84 6.59 17.46C6.21 17.08 5.99 16.56 5.99 16.02C5.99 15.48 6.21 14.97 6.59 14.59L15.07 6.12" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
        <span class="tool-label">文件</span>
      </button>
    </el-upload>
  </div>
</template>

<script setup lang="ts">
import type { UploadFile } from 'element-plus'

const emit = defineEmits<{
  (e: 'emoji', value: string): void
  (e: 'image', file: File): void
  (e: 'file', file: File): void
}>()

function onEmojiClick() {
  emit('emoji', '😊')
}

function onImageChange(file: UploadFile) {
  // @ts-ignore
  if (file?.raw) emit('image', file.raw as File)
}

function onFileChange(file: UploadFile) {
  // @ts-ignore
  if (file?.raw) emit('file', file.raw as File)
}
</script>

<style scoped>
.chat-toolbar {
  display: flex;
  align-items: center;
  gap: 4px;
  padding-bottom: 4px;
  border-bottom: 1px solid var(--ot-border);
}

.tool-upload {
  display: inline-flex;
}

.tool-btn {
  display: flex;
  align-items: center;
  gap: 5px;
  padding: 5px 10px;
  border-radius: 7px;
  border: none;
  background: transparent;
  color: var(--ot-text-muted);
  cursor: pointer;
  font-family: inherit;
  font-size: 12px;
  transition: background 0.15s, color 0.15s;
}

.tool-btn:hover {
  background: var(--ot-bg-hover);
  color: var(--ot-accent);
}

.tool-label {
  font-size: 12px;
  letter-spacing: 0.02em;
}
</style>
