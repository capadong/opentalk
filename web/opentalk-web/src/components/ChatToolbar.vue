<template>
  <div class="chat-toolbar">
    <span class="tool-icon" title="表情" @click="onEmojiClick">😊</span>
    <el-upload
      :auto-upload="false"
      :show-file-list="false"
      :on-change="onImageChange"
    >
      <span class="tool-icon" title="图片">🖼</span>
    </el-upload>
  </div>
</template>

<script setup lang="ts">
import type { UploadFile } from 'element-plus'

const emit = defineEmits<{
  (e: 'emoji', value: string): void
  (e: 'image', file: File): void
}>()

function onEmojiClick() {
  emit('emoji', '😊')
}

function onImageChange(file: UploadFile) {
  // @ts-ignore raw 由 el-upload 提供
  if (file && file.raw) {
    emit('image', file.raw as File)
  }
}
</script>

<style scoped>
.chat-toolbar {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 18px;
  color: var(--el-text-color-secondary);
}

.tool-icon {
  cursor: pointer;
  user-select: none;
}
</style>

