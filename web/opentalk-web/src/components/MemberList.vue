<template>
  <aside class="member-panel">
    <div class="member-panel-header">
      <span class="member-panel-title">成员</span>
      <span class="member-count">{{ members.length }}</span>
    </div>
    <div class="member-list-scroll">
      <div
        v-for="m in members"
        :key="m.userId"
        class="member-item"
        @click="emit('open-direct', m)"
        :title="`与 ${memberName(m)} 私信`"
      >
        <div class="member-avatar" :style="avatarStyle(m.userId)">
          <img v-if="m.avatar" :src="m.avatar" class="member-avatar-img" />
          <span v-else class="member-avatar-letter">{{ memberName(m).charAt(0) }}</span>
          <span class="member-online-dot"></span>
        </div>
        <div class="member-info">
          <span class="member-name">{{ memberName(m) }}</span>
          <span v-if="unread[m.userId]" class="member-unread">{{ unread[m.userId] > 99 ? '99+' : unread[m.userId] }} 条新消息</span>
          <span v-else class="member-status">点击私信</span>
        </div>
        <span v-if="unread[m.userId]" class="member-badge">{{ unread[m.userId] > 99 ? '99+' : unread[m.userId] }}</span>
      </div>
      <div v-if="members.length === 0" class="member-empty">
        <svg viewBox="0 0 48 48" fill="none" width="32" height="32">
          <circle cx="24" cy="24" r="20" stroke="var(--ot-border)" stroke-width="1.5" stroke-dasharray="4 3"/>
          <circle cx="24" cy="18" r="5" stroke="var(--ot-text-muted)" stroke-width="1.5"/>
          <path d="M12 38C12 32.477 17.373 28 24 28C30.627 28 36 32.477 36 38" stroke="var(--ot-text-muted)" stroke-width="1.5" stroke-linecap="round"/>
        </svg>
        <span>暂无成员</span>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
type Member = { userId: number; username: string; nickname?: string; avatar?: string }

const props = defineProps<{
  members: Member[]
  unread: Record<number, number>
}>()

const emit = defineEmits<{ (e: 'open-direct', member: Member): void }>()

function memberName(m: Member) {
  return m.nickname || m.username || `用户 ${m.userId}`
}

const AVATAR_COLORS = [
  ['#3b82f6','#6366f1'],['#10b981','#059669'],['#f59e0b','#d97706'],
  ['#ef4444','#dc2626'],['#8b5cf6','#7c3aed'],['#06b6d4','#0891b2']
]

function avatarStyle(uid: number) {
  const idx = uid % AVATAR_COLORS.length
  const [a, b] = AVATAR_COLORS[idx]
  return { background: `linear-gradient(135deg, ${a}, ${b})` }
}
</script>

<style scoped>
.member-panel {
  width: 220px;
  min-width: 220px;
  background: var(--ot-bg-sidebar);
  border-left: 1px solid var(--ot-border);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.member-panel-header {
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  border-bottom: 1px solid var(--ot-border);
  flex-shrink: 0;
}

.member-panel-title {
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: var(--ot-text-muted);
}

.member-count {
  font-size: 11px;
  font-weight: 600;
  color: var(--ot-accent);
  background: var(--ot-accent-dim);
  padding: 2px 7px;
  border-radius: 10px;
}

.member-list-scroll {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 8px 8px;
}

.member-list-scroll::-webkit-scrollbar { width: 4px; }
.member-list-scroll::-webkit-scrollbar-track { background: transparent; }
.member-list-scroll::-webkit-scrollbar-thumb { background: var(--ot-border); border-radius: 2px; }

.member-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 7px 8px;
  border-radius: 10px;
  cursor: pointer;
  transition: background 0.15s;
  position: relative;
}

.member-item:hover {
  background: var(--ot-bg-hover);
}

.member-item:hover .member-status {
  color: var(--ot-accent);
}

.member-avatar {
  width: 34px;
  height: 34px;
  min-width: 34px;
  border-radius: 9px;
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  overflow: visible;
}

.member-avatar-img {
  width: 34px;
  height: 34px;
  border-radius: 9px;
  object-fit: cover;
}

.member-avatar-letter {
  font-size: 13px;
  font-weight: 700;
  color: #fff;
  text-shadow: 0 1px 3px rgba(0,0,0,0.3);
  line-height: 1;
}

.member-online-dot {
  position: absolute;
  bottom: -1px;
  right: -1px;
  width: 9px;
  height: 9px;
  border-radius: 50%;
  background: var(--ot-green);
  border: 2px solid var(--ot-bg-sidebar);
  box-shadow: 0 0 5px var(--ot-green);
}

.member-info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.member-name {
  font-size: 13px;
  font-weight: 500;
  color: var(--ot-text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.member-status {
  font-size: 11px;
  color: var(--ot-text-muted);
  transition: color 0.15s;
}

.member-unread {
  font-size: 11px;
  color: var(--ot-red);
  font-weight: 500;
}

.member-badge {
  min-width: 18px;
  height: 18px;
  padding: 0 5px;
  border-radius: 9px;
  background: var(--ot-red);
  color: #fff;
  font-size: 10px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 0 6px rgba(252,75,108,0.5);
  flex-shrink: 0;
}

.member-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 40px 0;
  color: var(--ot-text-muted);
  font-size: 12px;
}
</style>
