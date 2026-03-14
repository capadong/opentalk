<template>
  <el-scrollbar ref="scrollbarRef" class="message-scrollbar">
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
          <div v-else>
            <el-link :href="apiBase + m.fileUrl" target="_blank">{{ m.content }}</el-link>
          </div>
        </el-card>
      </div>
    </div>
  </el-scrollbar>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'

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
}>()

const scrollbarRef = ref()

function scrollToBottom() {
  nextTick(() => {
    scrollbarRef.value?.scrollTo?.({ top: 999999 })
  })
}

watch(
  () => props.messages.length,
  () => scrollToBottom()
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
</script>

<style scoped>
.message-scrollbar {
  flex: 1;
  padding: 12px 16px;
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
  max-width: 100%;
}
.bubble-wrap-self {
  flex-direction: row-reverse;
}
.bubble-wrap-other {
  flex-direction: row;
}
.message-card {
  max-width: 520px;
}
</style>
