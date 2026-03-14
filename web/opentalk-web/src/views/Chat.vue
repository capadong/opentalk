<template>
  <el-container style="height:100vh">
    <el-aside width="280px" style="border-right:1px solid var(--el-border-color);display:flex;flex-direction:column">
      <div style="padding:12px;border-bottom:1px solid var(--el-border-color)">
        <el-input v-model="keyword" placeholder="搜索群组" clearable></el-input>
      </div>
      <el-scrollbar style="flex:1">
        <el-menu :default-active="String(currentGroup?.id||'')" @select="onSelect">
          <el-menu-item
            v-for="g in filteredGroups"
            :key="g.id"
            :index="String(g.id)"
            :style="{height:'auto',padding:'6px 12px',alignItems:'stretch'}"
          >
            <div style="display:flex;flex-direction:column;width:100%;gap:4px">
              <div style="display:flex;justify-content:space-between;align-items:center">
                <div style="display:flex;align-items:center;gap:6px;overflow:hidden">
                  <el-icon><chat-dot-round /></el-icon>
                  <span style="font-weight:500;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                    {{ g.name }}
                  </span>
                </div>
                <span style="font-size:12px;opacity:.6">
                  {{ formatTime(g.lastCreatedAt) }}
                </span>
              </div>
              <div style="display:flex;justify-content:space-between;align-items:center;font-size:12px;opacity:.8">
                <span style="flex:1;white-space:nowrap;overflow:hidden;text-overflow:ellipsis">
                  {{ g.lastContent || '暂无消息' }}
                </span>
                <el-badge v-if="groupUnread[g.id]" :value="groupUnread[g.id]" type="danger" />
              </div>
            </div>
          </el-menu-item>
        </el-menu>
      </el-scrollbar>
    </el-aside>
    <el-container>
      <el-header height="56px" style="display:flex;align-items:center;justify-content:space-between;border-bottom:1px solid var(--el-border-color)">
        <div style="display:flex;align-items:center;gap:8px">
          <el-icon><chat-line-round /></el-icon>
          <span style="font-weight:600">
            <template v-if="conversationMode === 'group'">
              {{ currentGroup?.name || '未选择群组' }}
            </template>
            <template v-else>
              与 {{ currentPeer?.nickname || currentPeer?.username || ('用户 ' + (currentPeer?.userId || '')) }} 私聊
            </template>
          </span>
        </div>
        <div>
          <el-button size="small" @click="logout">退出登录</el-button>
        </div>
      </el-header>
      <el-container>
        <el-main style="padding:0;display:flex;height:calc(100vh - 56px)">
          <div style="flex:1;display:flex;flex-direction:column">
            <el-scrollbar ref="msgScroll" style="flex:1;padding:12px 16px 12px 16px">
              <div
                v-for="m in messages"
                :key="m.id"
                :style="{
                  margin:'8px 0',
                  display:'flex',
                  justifyContent: m.senderId === userId ? 'flex-end' : 'flex-start'
                }"
              >
                <div
                  :style="{
                    display:'flex',
                    flexDirection: m.senderId === userId ? 'row-reverse' : 'row',
                    alignItems:'flex-start',
                    gap:'8px',
                    maxWidth:'70%'
                  }"
                >
                  <el-avatar :size="36" :src="avatarOf(m.senderId)">
                    {{ displayName(m.senderId).charAt(0) }}
                  </el-avatar>
                  <el-card shadow="never" style="max-width:100%">
                    <template #header>
                      <div style="display:flex;justify-content:space-between">
                        <span>{{ displayName(m.senderId) }}</span>
                        <span style="opacity:.6">{{ new Date(m.createdAt || new Date()).toLocaleString() }}</span>
                      </div>
                    </template>
                    <div v-if="m.type===1">{{ m.content }}</div>
                    <div v-else-if="m.type===2">
                      <el-image :src="API_BASE + m.fileUrl" style="max-width:280px" fit="contain" :preview-src-list="[API_BASE + m.fileUrl]" />
                    </div>
                    <div v-else>
                      <el-link :href="API_BASE + m.fileUrl" target="_blank">{{ m.content }}</el-link>
                    </div>
                  </el-card>
                </div>
              </div>
            </el-scrollbar>
            <div style="height:150px;border-top:1px solid var(--el-border-color);padding:8px;display:flex;flex-direction:column;gap:8px">
              <div v-if="pendingImageUrl" style="padding-bottom:4px">
                <el-image :src="pendingImageUrl" style="max-width:160px;max-height:160px" fit="contain" />
              </div>
              <div style="display:flex;align-items:center;gap:8px">
                <el-input v-model="text" placeholder="输入消息，回车发送" @keyup.enter="sendText"></el-input>
                <el-upload :auto-upload="false" :show-file-list="false" :on-change="onSelectFile">
                  <el-button>图片</el-button>
                </el-upload>
                <el-button type="primary" @click="sendText">发送</el-button>
              </div>
            </div>
          </div>
          <div style="width:260px;border-left:1px solid var(--el-border-color);padding:12px;display:flex;flex-direction:column">
            <div style="font-weight:600;margin-bottom:8px">成员</div>
            <el-scrollbar style="flex:1">
              <template v-if="members.length">
                <div
                  v-for="m in members"
                  :key="m.userId"
                  style="padding:4px 0;display:flex;align-items:center;justify-content:space-between;cursor:pointer"
                  @click="openDirect(m)"
                >
                  <div style="display:flex;align-items:center;gap:8px">
                    <el-avatar :size="32" :src="m.avatar">
                      {{ (m.nickname || m.username || ('用户 ' + m.userId)).charAt(0) }}
                    </el-avatar>
                    <span>{{ m.nickname || m.username || ('用户 ' + m.userId) }}</span>
                  </div>
                  <el-badge v-if="unread[m.userId]" :value="unread[m.userId]" type="danger" />
                </div>
              </template>
              <el-empty v-else description="暂无成员"></el-empty>
            </el-scrollbar>
          </div>
        </el-main>
      </el-container>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref, computed } from 'vue'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { ElMessage, ElNotification } from 'element-plus'
import { useRouter } from 'vue-router'
import { ChatDotRound, ChatLineRound } from '@element-plus/icons-vue'
import { API_BASE, getGroups, getMessages, getGroupMembers, getDirectMessages, uploadFile as apiUpload } from '../api/http'

type Group = { id: number; name: string; lastContent?:string; lastCreatedAt?:string }
type Message = { id:number; groupId?:number; senderId:number; receiverId?:number; type:number; content:string; fileUrl?:string; createdAt?:string }
type Member = { userId:number; username:string; nickname?:string; avatar?:string; joinedAt:string }

const router = useRouter()
const user = JSON.parse(localStorage.getItem('user') || '{"id":1}')
const userId = user.id || 1

const groups = ref<Group[]>([])
const messages = ref<Message[]>([])
const members = ref<Member[]>([])
const currentGroup = ref<Group | null>(null)
const currentPeer = ref<Member | null>(null)
const conversationMode = ref<'group' | 'direct'>('group')
const text = ref('')
const keyword = ref('')
const msgScroll = ref()
const unread = ref<Record<number, number>>({})
const groupUnread = ref<Record<number, number>>({})
const pendingImage = ref<File | null>(null)
const pendingImageUrl = ref<string | null>(null)

const filteredGroups = computed(() =>
  groups.value.filter(g => g.name.toLowerCase().includes(keyword.value.toLowerCase()))
)

const conn = new HubConnectionBuilder()
  .withUrl(`${API_BASE}/hubs/chat`)
  .configureLogging(LogLevel.Information)
  .withAutomaticReconnect()
  .build()

conn.on('ReceiveMessage', (m: Message) => {
  if (!m.groupId) return
  const g = groups.value.find(x => x.id === m.groupId)
  if (g) {
    updateGroupSummaryFromMessage(m, g)
  }
  if (conversationMode.value === 'group' && currentGroup.value && m.groupId === currentGroup.value.id) {
    messages.value.push(m)
    setTimeout(() => msgScroll.value?.scrollTo?.({ top: 999999 }), 0)
  } else {
    groupUnread.value[m.groupId] = (groupUnread.value[m.groupId] || 0) + 1
  }
})

conn.on('ReceiveDirectMessage', (m: Message) => {
  const peerId = m.senderId === userId ? (m.receiverId || 0) : m.senderId
  if (
    conversationMode.value === 'direct' &&
    currentPeer.value &&
    peerId === currentPeer.value.userId
  ) {
    messages.value.push(m)
    setTimeout(() => msgScroll.value?.scrollTo?.({ top: 999999 }), 0)
  } else {
    if (peerId) {
      unread.value[peerId] = (unread.value[peerId] || 0) + 1
      ElNotification({
        title: '新消息',
        message: `来自 ${displayName(peerId)} 的新消息`,
        position: 'bottom-right'
      })
    }
  }
})

onMounted(async () => {
  try {
    await conn.start()
    await conn.invoke('Register', userId)
    groups.value = await getGroups(userId)
    if (groups.value.length) {
      await onSelect(String(groups.value[0].id))
    } else {
      ElMessage.info('暂无群组，可在后台创建')
    }
  } catch {
    ElMessage.error('连接失败')
  }
})

onUnmounted(() => {
  if (pendingImageUrl.value) {
    URL.revokeObjectURL(pendingImageUrl.value)
  }
})

function handlePaste(e: ClipboardEvent) {
  const items = e.clipboardData?.items
  if (!items) return
  for (const item of items) {
    if (item.type.startsWith('image/')) {
      const file = item.getAsFile()
      if (file) {
        if (pendingImageUrl.value) {
          URL.revokeObjectURL(pendingImageUrl.value)
        }
        pendingImage.value = file
        pendingImageUrl.value = URL.createObjectURL(file)
        e.preventDefault()
        break
      }
    }
  }
}

window.addEventListener('paste', handlePaste)

async function onSelect(index: string) {
  const id = Number(index)
  const g = groups.value.find(x => x.id === id)
  if (!g) return
  conversationMode.value = 'group'
  currentGroup.value = g
  currentPeer.value = null
  await conn.invoke('JoinGroup', g.id)
  members.value = await getGroupMembers(g.id)
  const list = (await getMessages(g.id, 50)).reverse()
  messages.value = list
  if (list.length) {
    updateGroupSummaryFromMessage(list[list.length - 1], g)
  }
  groupUnread.value[g.id] = 0
  setTimeout(() => msgScroll.value?.scrollTo?.({ top: 999999 }), 0)
}

async function sendText() {
  const hasText = !!text.value.trim()
  const hasImage = !!pendingImage.value
  if (!hasText && !hasImage) return

  if (conversationMode.value === 'group') {
    if (!currentGroup.value) return
    if (hasText) {
      const dto = { groupId: currentGroup.value.id, senderId: userId, type: 1, content: text.value.trim() }
      await conn.invoke('SendMessage', dto)
    }
    if (hasImage && pendingImage.value) {
      const info = await apiUpload(pendingImage.value)
      const dto = { groupId: currentGroup.value.id, senderId: userId, type: 2, content: pendingImage.value.name, fileUrl: info.url }
      await conn.invoke('SendMessage', dto)
    }
  } else {
    if (!currentPeer.value) return
    if (hasText) {
      const dto = { senderId: userId, receiverId: currentPeer.value.userId, type: 1, content: text.value.trim() }
      await conn.invoke('SendDirectMessage', dto)
    }
    if (hasImage && pendingImage.value) {
      const info = await apiUpload(pendingImage.value)
      const dto = { senderId: userId, receiverId: currentPeer.value.userId, type: 2, content: pendingImage.value.name, fileUrl: info.url }
      await conn.invoke('SendDirectMessage', dto)
    }
  }
  text.value = ''
  if (pendingImageUrl.value) {
    URL.revokeObjectURL(pendingImageUrl.value)
  }
  pendingImage.value = null
  pendingImageUrl.value = null
}

async function onSelectFile(file:any) {
  if (!file) return
  if (conversationMode.value === 'group') {
    if (!currentGroup.value) return
    const info = await apiUpload(file.raw)
    const dto = { groupId: currentGroup.value.id, senderId: userId, type: 2, content: file.name, fileUrl: info.url }
    await conn.invoke('SendMessage', dto)
    return
  }
  if (!currentPeer.value) return
  const info = await apiUpload(file.raw)
  const dto = { senderId: userId, receiverId: currentPeer.value.userId, type: 2, content: file.name, fileUrl: info.url }
  await conn.invoke('SendDirectMessage', dto)
}

async function openDirect(m: Member) {
  if (m.userId === userId) return
  conversationMode.value = 'direct'
  currentPeer.value = m
  currentGroup.value = null
  unread.value[m.userId] = 0
  messages.value = (await getDirectMessages(userId, m.userId, 50)).reverse()
  setTimeout(() => msgScroll.value?.scrollTo?.({ top: 999999 }), 0)
}

function displayName(senderId:number) {
  if (senderId === userId) {
    return user.Nickname || user.Username || `用户 ${senderId}`
  }
  const m = members.value.find(x => x.userId === senderId)
  if (m) return m.nickname || m.username || `用户 ${senderId}`
  return `用户 ${senderId}`
}

function avatarOf(userIdValue:number) {
  if (userIdValue === userId) {
    return user.Avatar
  }
  const m = members.value.find(x => x.userId === userIdValue)
  return m?.avatar
}

function updateGroupSummaryFromMessage(m: Message, g: Group) {
  g.lastCreatedAt = m.createdAt || new Date().toISOString()
  if (m.type === 1) {
    const text = m.content || ''
    g.lastContent = text.length > 20 ? text.slice(0, 20) + '…' : text
  } else if (m.type === 2) {
    g.lastContent = '[图片]'
  } else if (m.type === 3) {
    g.lastContent = '[文件]'
  } else {
    g.lastContent = m.content || ''
  }
}

function formatTime(value?:string) {
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
  if (d.getFullYear() === now.getFullYear()) {
    return `${month}-${day} ${hm}`
  }
  return `${d.getFullYear()}-${month}-${day} ${hm}`
}

function logout() {
  localStorage.removeItem('token'); localStorage.removeItem('user')
  router.push('/login')
}
</script>

<script lang="ts">
export default {
  components: { ChatDotRound, ChatLineRound }
}
</script>
<!-- <style lang="scss">
:root{
  --el-menu-item-height: 150px;
}
</style> -->