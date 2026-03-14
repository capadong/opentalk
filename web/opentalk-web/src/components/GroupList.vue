<template>
  <el-aside width="280px" class="group-aside">
    <div class="group-search">
      <el-input v-model="keyword" placeholder="搜索群组" clearable />
    </div>
    <el-scrollbar style="flex:1">
      <el-menu :default-active="String(activeId || '')" @select="emit('select', $event)">
        <el-menu-item
          v-for="g in filteredGroups"
          :key="g.id"
          :index="String(g.id)"
          class="group-item"
        >
          <div class="group-item-inner">
            <div class="group-item-top">
              <div class="group-name-row">
                <el-icon><ChatDotRound /></el-icon>
                <span class="group-name">{{ g.name }}</span>
              </div>
              <span class="group-time">{{ formatTime(g.lastCreatedAt) }}</span>
            </div>
            <div class="group-item-bottom">
              <span class="group-last-msg">{{ g.lastContent || '暂无消息' }}</span>
              <el-badge v-if="unread[g.id]" :value="unread[g.id]" type="danger" />
            </div>
          </div>
        </el-menu-item>
      </el-menu>
    </el-scrollbar>
  </el-aside>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { ChatDotRound } from '@element-plus/icons-vue'

type Group = { id: number; name: string; lastContent?: string; lastCreatedAt?: string }

const props = defineProps<{
  groups: Group[]
  activeId?: number | null
  unread: Record<number, number>
}>()

const emit = defineEmits<{
  (e: 'select', index: string): void
}>()

const keyword = ref('')

const filteredGroups = computed(() =>
  props.groups.filter(g => g.name.toLowerCase().includes(keyword.value.toLowerCase()))
)

function formatTime(value?: string) {
  if (!value) return ''
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return ''
  const now = new Date()
  const startToday = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  const startDate = new Date(d.getFullYear(), d.getMonth(), d.getDate())
  const diffDays = Math.floor((startToday.getTime() - startDate.getTime()) / 86400000)
  const hh = d.getHours().toString().padStart(2, '0')
  const mm = d.getMinutes().toString().padStart(2, '0')
  const hm = `${hh}:${mm}`
  if (diffDays === 0) return hm
  if (diffDays === 1) return `昨天 ${hm}`
  if (diffDays === 2) return `前天 ${hm}`
  const month = d.getMonth() + 1
  const day = d.getDate()
  if (d.getFullYear() === now.getFullYear()) return `${month}-${day} ${hm}`
  return `${d.getFullYear()}-${month}-${day} ${hm}`
}
</script>

<style scoped>
.group-aside {
  border-right: 1px solid var(--el-border-color);
  display: flex;
  flex-direction: column;
}
.group-search {
  padding: 12px;
  border-bottom: 1px solid var(--el-border-color);
}
.group-item {
  height: auto !important;
  padding: 6px 12px !important;
  align-items: stretch;
}
.group-item-inner {
  display: flex;
  flex-direction: column;
  width: 100%;
  gap: 4px;
}
.group-item-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.group-name-row {
  display: flex;
  align-items: center;
  gap: 6px;
  overflow: hidden;
}
.group-name {
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.group-time {
  font-size: 12px;
  opacity: 0.6;
  white-space: nowrap;
}
.group-item-bottom {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  opacity: 0.8;
}
.group-last-msg {
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
</style>
