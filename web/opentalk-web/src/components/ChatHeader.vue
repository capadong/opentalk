<template>
  <el-header height="56px" class="chat-header">
    <div class="chat-header-title">
      <el-icon><ChatLineRound /></el-icon>
      <span class="title-text">
        <template v-if="mode === 'group'">
          {{ groupName || '未选择群组' }}
        </template>
        <template v-else>
          与 {{ peerName }} 私聊
        </template>
      </span>
    </div>
    <div>
      <el-button size="small" @click="emit('logout')">退出登录</el-button>
    </div>
  </el-header>
</template>

<script setup lang="ts">
import { ChatLineRound } from '@element-plus/icons-vue'

defineProps<{
  mode: 'group' | 'direct'
  groupName?: string | null
  peerName?: string
}>()

const emit = defineEmits<{
  (e: 'logout'): void
}>()
</script>

<style scoped>
.chat-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid var(--el-border-color);
}
.chat-header-title {
  display: flex;
  align-items: center;
  gap: 8px;
}
.title-text {
  font-weight: 600;
}
</style>
