<template>
  <el-container style="height:100vh">
    <GroupList
      :groups="groups"
      :active-id="currentGroup?.id"
      :unread="groupUnread"
      @select="onSelect"
    />
    <el-container style="flex-direction: column;">
      <ChatHeader
        :mode="conversationMode"
        :group-name="currentGroup?.name"
        :peer-name="peerDisplayName"
        @logout="logout"
      />
      <el-container>
        <el-main class="chat-main">
          <div class="chat-body">
            <MessageList
              :messages="messages"
              :self-id="userId"
              :members="members"
              :self-user="user"
              :api-base="API_BASE"
            />
            <MessageInput
              @send-text="onSendText"
              @send-image="onSendImage"
            />
          </div>
          <MemberList
            :members="members"
            :unread="unread"
            @open-direct="openDirect"
          />
        </el-main>
      </el-container>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { ElMessage, ElNotification } from 'element-plus'
import { useRouter } from 'vue-router'
import { API_BASE, getGroups, getMessages, getGroupMembers, getDirectMessages, uploadFile as apiUpload } from '../api/http'
import GroupList from '../components/GroupList.vue'
import ChatHeader from '../components/ChatHeader.vue'
import MessageList from '../components/MessageList.vue'
import MessageInput from '../components/MessageInput.vue'
import MemberList from '../components/MemberList.vue'

type Group = { id: number; name: string; lastContent?: string; lastCreatedAt?: string }
type Message = { id: number; groupId?: number; senderId: number; receiverId?: number; type: number; content: string; fileUrl?: string; createdAt?: string }
type Member = { userId: number; username: string; nickname?: string; avatar?: string; joinedAt: string }

const router = useRouter()
const user = JSON.parse(localStorage.getItem('user') || '{"id":1}')
const userId: number = user.id || 1

const groups = ref<Group[]>([])
const messages = ref<Message[]>([])
const members = ref<Member[]>([])
const currentGroup = ref<Group | null>(null)
const currentPeer = ref<Member | null>(null)
const conversationMode = ref<'group' | 'direct'>('group')
const unread = ref<Record<number, number>>({})
const groupUnread = ref<Record<number, number>>({})

const peerDisplayName = computed(() => {
  if (!currentPeer.value) return ''
  const p = currentPeer.value
  return p.nickname || p.username || `用户 ${p.userId}`
})

const conn = new HubConnectionBuilder()
  .withUrl(`${API_BASE}/hubs/chat`)
  .configureLogging(LogLevel.Information)
  .withAutomaticReconnect()
  .build()

conn.on('ReceiveMessage', (m: Message) => {
  if (!m.groupId) return
  const g = groups.value.find(x => x.id === m.groupId)
  if (g) updateGroupSummary(m, g)
  if (conversationMode.value === 'group' && currentGroup.value?.id === m.groupId) {
    messages.value.push(m)
  } else {
    groupUnread.value[m.groupId] = (groupUnread.value[m.groupId] || 0) + 1
  }
})

conn.on('ReceiveDirectMessage', (m: Message) => {
  const peerId = m.senderId === userId ? (m.receiverId || 0) : m.senderId
  if (conversationMode.value === 'direct' && currentPeer.value?.userId === peerId) {
    messages.value.push(m)
  } else if (peerId) {
    unread.value[peerId] = (unread.value[peerId] || 0) + 1
    ElNotification({
      title: '新消息',
      message: `来自 ${peerId} 的新消息`,
      position: 'bottom-right'
    })
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
  if (list.length) updateGroupSummary(list[list.length - 1], g)
  groupUnread.value[g.id] = 0
}

async function onSendText(content: string) {
  if (conversationMode.value === 'group') {
    if (!currentGroup.value) return
    await conn.invoke('SendMessage', {
      groupId: currentGroup.value.id,
      senderId: userId,
      type: 1,
      content
    })
  } else {
    if (!currentPeer.value) return
    await conn.invoke('SendDirectMessage', {
      senderId: userId,
      receiverId: currentPeer.value.userId,
      type: 1,
      content
    })
  }
}

async function onSendImage(file: File) {
  const info = await apiUpload(file)
  if (conversationMode.value === 'group') {
    if (!currentGroup.value) return
    await conn.invoke('SendMessage', {
      groupId: currentGroup.value.id,
      senderId: userId,
      type: 2,
      content: file.name,
      fileUrl: info.url
    })
  } else {
    if (!currentPeer.value) return
    await conn.invoke('SendDirectMessage', {
      senderId: userId,
      receiverId: currentPeer.value.userId,
      type: 2,
      content: file.name,
      fileUrl: info.url
    })
  }
}

async function openDirect(m: Member) {
  if (m.userId === userId) return
  conversationMode.value = 'direct'
  currentPeer.value = m
  currentGroup.value = null
  unread.value[m.userId] = 0
  messages.value = (await getDirectMessages(userId, m.userId, 50)).reverse()
}

function updateGroupSummary(m: Message, g: Group) {
  g.lastCreatedAt = m.createdAt || new Date().toISOString()
  if (m.type === 1) {
    const t = m.content || ''
    g.lastContent = t.length > 20 ? t.slice(0, 20) + '\u2026' : t
  } else if (m.type === 2) {
    g.lastContent = '[图片]'
  } else if (m.type === 3) {
    g.lastContent = '[文件]'
  } else {
    g.lastContent = m.content || ''
  }
}

function logout() {
  localStorage.removeItem('token')
  localStorage.removeItem('user')
  router.push('/login')
}
</script>

<style scoped>
.chat-main {
  padding: 0;
  display: flex;
  height: calc(100vh - 56px);
}
.chat-body {
  flex: 1;
  display: flex;
  flex-direction: column;
}
</style>
