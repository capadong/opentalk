<template>
  <header class="chat-header">
    <div class="header-left">
      <div class="header-icon-wrap">
        <svg v-if="mode === 'group'" viewBox="0 0 24 24" fill="none" class="header-svg">
          <path d="M17 8C17 10.7614 14.7614 13 12 13C9.23858 13 7 10.7614 7 8C7 5.23858 9.23858 3 12 3C14.7614 3 17 5.23858 17 8Z" fill="currentColor" opacity="0.8"/>
          <path d="M3 21C3 17.134 7.02944 14 12 14C16.9706 14 21 17.134 21 21" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          <circle cx="19" cy="8" r="2.5" fill="currentColor" opacity="0.5"/>
          <circle cx="5" cy="8" r="2.5" fill="currentColor" opacity="0.5"/>
        </svg>
        <svg v-else viewBox="0 0 24 24" fill="none" class="header-svg">
          <circle cx="12" cy="8" r="4" fill="currentColor" opacity="0.9"/>
          <path d="M4 20C4 16.6863 7.58172 14 12 14C16.4183 14 20 16.6863 20 20" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
          <path d="M18 3L21 6M21 3L18 6" stroke="var(--ot-accent)" stroke-width="1.5" stroke-linecap="round"/>
        </svg>
      </div>
      <div class="header-title-block">
        <span class="header-title">
          <template v-if="mode === 'group'">{{ groupName || '未选择群组' }}</template>
          <template v-else>{{ peerName }}</template>
        </span>
        <span class="header-subtitle">
          <template v-if="mode === 'group'">群聊</template>
          <template v-else>私信对话</template>
        </span>
      </div>
    </div>
    <div class="header-right">
      <div class="status-dot" :class="statusClass"></div>
      <span class="status-text" :class="statusClass">{{ statusText }}</span>
      <button class="theme-btn" @click="emit('toggle-theme')" :title="isDark ? '切换到浅色模式' : '切换到深色模式'">
        <!-- Sun icon (shown in dark mode → click to go light) -->
        <svg v-if="isDark" viewBox="0 0 24 24" fill="none" width="16" height="16">
          <circle cx="12" cy="12" r="4" stroke="currentColor" stroke-width="1.8"/>
          <path d="M12 2v2M12 20v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M2 12h2M20 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42" stroke="currentColor" stroke-width="1.8" stroke-linecap="round"/>
        </svg>
        <!-- Moon icon (shown in light mode → click to go dark) -->
        <svg v-else viewBox="0 0 24 24" fill="none" width="16" height="16">
          <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </button>
      <button class="logout-btn" @click="emit('logout')">
        <svg viewBox="0 0 24 24" fill="none" width="15" height="15">
          <path d="M9 21H5C4.46957 21 3.96086 20.7893 3.58579 20.4142C3.21071 20.0391 3 19.5304 3 19V5C3 4.46957 3.21071 3.96086 3.58579 3.58579C3.96086 3.21071 4.46957 3 5 3H9" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          <polyline points="16 17 21 12 16 7" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          <line x1="21" y1="12" x2="9" y2="12" stroke="currentColor" stroke-width="2" stroke-linecap="round"/>
        </svg>
        退出
      </button>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  mode: 'group' | 'direct'
  groupName?: string | null
  peerName?: string
  isDark?: boolean
  connectionState?: 'connecting' | 'connected' | 'reconnecting' | 'disconnected'
}>()

const emit = defineEmits<{
  (e: 'logout'): void
  (e: 'toggle-theme'): void
}>()

const statusText = computed(() => {
  switch (props.connectionState) {
    case 'connected':     return '在线'
    case 'connecting':    return '连接中'
    case 'reconnecting':  return '重连中'
    case 'disconnected':  return '已断线'
    default:              return '连接中'
  }
})

const statusClass = computed(() => {
  switch (props.connectionState) {
    case 'connected':    return 'status-online'
    case 'reconnecting': return 'status-warn'
    case 'disconnected': return 'status-offline'
    default:             return 'status-warn'
  }
})
</script>

<style scoped>
.chat-header {
  height: 60px;
  min-height: 60px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px;
  background: var(--ot-bg-panel);
  border-bottom: 1px solid var(--ot-border);
  backdrop-filter: blur(12px);
  position: relative;
  z-index: 10;
}

.chat-header::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 20px;
  right: 20px;
  height: 1px;
  background: linear-gradient(90deg, transparent, var(--ot-accent-dim), transparent);
  opacity: 0.6;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-icon-wrap {
  width: 38px;
  height: 38px;
  border-radius: 10px;
  background: linear-gradient(135deg, var(--ot-accent-dim), var(--ot-accent-glow));
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--ot-accent);
  border: 1px solid var(--ot-accent-dim);
  box-shadow: 0 0 12px var(--ot-accent-glow);
}

.header-svg {
  width: 20px;
  height: 20px;
}

.header-title-block {
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.header-title {
  font-size: 15px;
  font-weight: 700;
  color: var(--ot-text-primary);
  letter-spacing: 0.02em;
  line-height: 1.2;
}

.header-subtitle {
  font-size: 11px;
  color: var(--ot-text-muted);
  letter-spacing: 0.05em;
  text-transform: uppercase;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 10px;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  animation: pulse-dot 2s ease-in-out infinite;
}

.status-dot.status-online  { background: var(--ot-green);  box-shadow: 0 0 6px var(--ot-green); }
.status-dot.status-warn    { background: #f59e0b;            box-shadow: 0 0 6px #f59e0b; }
.status-dot.status-offline { background: var(--ot-red);    box-shadow: 0 0 6px var(--ot-red);  animation: none; }

@keyframes pulse-dot {
  0%, 100% { opacity: 1; transform: scale(1); }
  50% { opacity: 0.6; transform: scale(0.85); }
}

.status-text {
  font-size: 12px;
  letter-spacing: 0.04em;
}
.status-text.status-online  { color: var(--ot-green); }
.status-text.status-warn    { color: #f59e0b; }
.status-text.status-offline { color: var(--ot-red); }

.theme-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 34px;
  height: 34px;
  border-radius: 8px;
  border: 1px solid var(--ot-border);
  background: transparent;
  color: var(--ot-text-secondary);
  cursor: pointer;
  transition: all 0.2s;
  flex-shrink: 0;
  padding: 0;
}

.theme-btn:hover {
  border-color: var(--ot-accent-dim);
  color: var(--ot-accent);
  background: var(--ot-accent-dim);
  box-shadow: 0 0 10px var(--ot-accent-glow);
}

.logout-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  border-radius: 8px;
  border: 1px solid var(--ot-border);
  background: transparent;
  color: var(--ot-text-secondary);
  font-size: 13px;
  cursor: pointer;
  transition: all 0.2s;
  font-family: inherit;
  letter-spacing: 0.02em;
}

.logout-btn:hover {
  border-color: var(--ot-red-dim);
  color: var(--ot-red);
  background: var(--ot-red-glow);
  box-shadow: 0 0 10px var(--ot-red-glow);
}
</style>
