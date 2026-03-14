<template>
  <div class="member-panel">
    <div class="member-panel-title">成员</div>
    <el-scrollbar style="flex:1">
      <template v-if="members.length">
        <div
          v-for="m in members"
          :key="m.userId"
          class="member-item"
          @click="emit('open-direct', m)"
        >
          <div class="member-info">
            <el-avatar :size="32" :src="m.avatar">
              {{ memberName(m).charAt(0) }}
            </el-avatar>
            <span>{{ memberName(m) }}</span>
          </div>
          <el-badge v-if="unread[m.userId]" :value="unread[m.userId]" type="danger" />
        </div>
      </template>
      <el-empty v-else description="暂无成员" />
    </el-scrollbar>
  </div>
</template>

<script setup lang="ts">
type Member = { userId: number; username: string; nickname?: string; avatar?: string }

const props = defineProps<{
  members: Member[]
  unread: Record<number, number>
}>()

const emit = defineEmits<{
  (e: 'open-direct', member: Member): void
}>()

function memberName(m: Member) {
  return m.nickname || m.username || `用户 ${m.userId}`
}
</script>

<style scoped>
.member-panel {
  width: 260px;
  border-left: 1px solid var(--el-border-color);
  padding: 12px;
  display: flex;
  flex-direction: column;
}
.member-panel-title {
  font-weight: 600;
  margin-bottom: 8px;
}
.member-item {
  padding: 4px 0;
  display: flex;
  align-items: center;
  justify-content: space-between;
  cursor: pointer;
}
.member-info {
  display: flex;
  align-items: center;
  gap: 8px;
}
</style>
