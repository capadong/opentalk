<template>
  <aside class="group-aside">
    <!-- Logo区 -->
    <div class="group-logo">
      <div class="logo-mark">
        <svg viewBox="0 0 32 32" fill="none" width="20" height="20">
          <circle cx="16" cy="16" r="13" stroke="var(--ot-accent)" stroke-width="2"/>
          <path d="M10 20 Q16 10 22 20" stroke="var(--ot-accent)" stroke-width="2" stroke-linecap="round" fill="none"/>
          <circle cx="16" cy="13" r="2.5" fill="var(--ot-accent)"/>
        </svg>
      </div>
      <span class="logo-text">OpenTalk</span>
    </div>

    <!-- 搜索框 -->
    <div class="group-search">
      <div class="search-inner">
        <svg viewBox="0 0 20 20" fill="none" width="14" height="14" class="search-icon">
          <circle cx="8.5" cy="8.5" r="5.5" stroke="currentColor" stroke-width="1.5"/>
          <path d="M13 13L17 17" stroke="currentColor" stroke-width="1.5" stroke-linecap="round"/>
        </svg>
        <input
          v-model="keyword"
          class="search-input"
          placeholder="搜索群组..."
          type="text"
        />
      </div>
    </div>

    <!-- 分组标题 -->
    <div class="section-label">群组列表</div>

    <!-- 群组列表 -->
    <div class="group-list-scroll">
      <div
        v-for="g in filteredGroups"
        :key="g.id"
        class="group-item"
        :class="{ 'group-item-active': String(activeId) === String(g.id) }"
        @click="emit('select', String(g.id))"
      >
        <div class="group-avatar">
          <span class="group-avatar-text">{{ g.name.charAt(0) }}</span>
          <span v-if="unread[g.id]" class="group-badge">{{ unread[g.id] > 99 ? '99+' : unread[g.id] }}</span>
        </div>
        <div class="group-info">
          <div class="group-info-top">
            <span class="group-name">{{ g.name }}</span>
            <span class="group-time">{{ formatTime(g.lastCreatedAt) }}</span>
          </div>
          <div class="group-last-msg">{{ g.lastContent || '暂无消息' }}</div>
        </div>
      </div>
      <div v-if="filteredGroups.length === 0" class="group-empty">
        <svg viewBox="0 0 48 48" fill="none" width="36" height="36">
          <circle cx="24" cy="24" r="20" stroke="var(--ot-border)" stroke-width="1.5" stroke-dasharray="4 3"/>
          <path d="M16 28C16 28 18 24 24 24C30 24 32 28 32 28" stroke="var(--ot-text-muted)" stroke-width="1.5" stroke-linecap="round"/>
          <circle cx="18" cy="20" r="2" fill="var(--ot-text-muted)"/>
          <circle cx="30" cy="20" r="2" fill="var(--ot-text-muted)"/>
        </svg>
        <span>暂无群组</span>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

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
  if (diffDays === 1) return '昨天'
  const month = d.getMonth() + 1
  const day = d.getDate()
  if (d.getFullYear() === now.getFullYear()) return `${month}/${day}`
  return `${d.getFullYear()}/${month}/${day}`
}
</script>

<style scoped>
.group-aside {
  width: 260px;
  min-width: 260px;
  height: 100vh;
  background: var(--ot-bg-sidebar);
  border-right: 1px solid var(--ot-border);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.group-logo {
  height: 60px;
  min-height: 60px;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 18px;
  border-bottom: 1px solid var(--ot-border);
}

.logo-mark {
  width: 34px;
  height: 34px;
  border-radius: 9px;
  background: linear-gradient(135deg, var(--ot-accent-dim), rgba(99,179,237,0.06));
  border: 1px solid var(--ot-accent-dim);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 0 14px var(--ot-accent-glow);
}

.logo-text {
  font-size: 16px;
  font-weight: 800;
  letter-spacing: 0.04em;
  background: linear-gradient(135deg, var(--ot-accent), #a78bfa);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.group-search {
  padding: 12px 14px;
}

.search-inner {
  display: flex;
  align-items: center;
  gap: 8px;
  background: var(--ot-bg-input);
  border: 1px solid var(--ot-border);
  border-radius: 10px;
  padding: 7px 12px;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.search-inner:focus-within {
  border-color: var(--ot-accent-dim);
  box-shadow: 0 0 0 3px var(--ot-accent-glow);
}

.search-icon {
  color: var(--ot-text-muted);
  flex-shrink: 0;
}

.search-input {
  flex: 1;
  background: transparent;
  border: none;
  outline: none;
  font-size: 13px;
  color: var(--ot-text-primary);
  font-family: inherit;
}

.search-input::placeholder {
  color: var(--ot-text-muted);
}

.section-label {
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: var(--ot-text-muted);
  padding: 0 18px 8px;
}

.group-list-scroll {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 0 8px 12px;
}

.group-list-scroll::-webkit-scrollbar {
  width: 4px;
}

.group-list-scroll::-webkit-scrollbar-track {
  background: transparent;
}

.group-list-scroll::-webkit-scrollbar-thumb {
  background: var(--ot-border);
  border-radius: 2px;
}

.group-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  border-radius: 10px;
  cursor: pointer;
  transition: background 0.15s;
  margin-bottom: 2px;
  position: relative;
}

.group-item:hover {
  background: var(--ot-bg-hover);
}

.group-item-active {
  background: var(--ot-bg-active) !important;
}

.group-item-active::before {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 3px;
  height: 60%;
  background: var(--ot-accent);
  border-radius: 0 3px 3px 0;
  box-shadow: 0 0 8px var(--ot-accent);
}

.group-avatar {
  width: 40px;
  height: 40px;
  min-width: 40px;
  border-radius: 11px;
  background: linear-gradient(135deg, var(--ot-avatar-a), var(--ot-avatar-b));
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  font-size: 15px;
  font-weight: 700;
  color: #fff;
  text-shadow: 0 1px 3px rgba(0,0,0,0.3);
}

.group-badge {
  position: absolute;
  top: -4px;
  right: -4px;
  min-width: 16px;
  height: 16px;
  padding: 0 4px;
  border-radius: 8px;
  background: var(--ot-red);
  color: #fff;
  font-size: 10px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 0 6px rgba(252,75,108,0.5);
}

.group-info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.group-info-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.group-name {
  font-size: 13px;
  font-weight: 600;
  color: var(--ot-text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 130px;
}

.group-time {
  font-size: 11px;
  color: var(--ot-text-muted);
  white-space: nowrap;
  flex-shrink: 0;
}

.group-last-msg {
  font-size: 12px;
  color: var(--ot-text-secondary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.group-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 40px 0;
  color: var(--ot-text-muted);
  font-size: 13px;
}
</style>
