<template>
  <el-container style="height:100vh">
    <el-aside width="260px" style="border-right:1px solid var(--el-border-color);display:flex;flex-direction:column">
      <div style="padding:12px;border-bottom:1px solid var(--el-border-color)">
        <el-input v-model="keyword" placeholder="搜索群组" clearable></el-input>
      </div>
      <el-scrollbar>
        <el-menu :default-active="String(currentGroup?.id||'')" @select="onSelect">
          <el-menu-item v-for="g in filteredGroups" :key="g.id" :index="String(g.id)">
            <el-icon class="me-2"><chat-dot-round /></el-icon>{{ g.name }}
          </el-menu-item>
        </el-menu>
      </el-scrollbar>
    </el-aside>
    <el-container>
      <el-header height="56px" style="display:flex;align-items:center;justify-content:space-between;border-bottom:1px solid var(--el-border-color)">
        <div style="display:flex;align-items:center;gap:8px">
          <el-icon><chat-line-round /></el-icon>
          <span style="font-weight:600">{{ currentGroup?.name || '未选择群组' }}</span>
        </div>
        <div>
          <el-button size="small" @click="logout">退出登录</el-button>
        </div>
      </el-header>
      <el-container>
        <el-main style="padding:0;display:flex">
          <div style="flex:1;display:flex;flex-direction:column">
            <el-scrollbar ref="msgScroll" style="flex:1;padding:12px">
              <div v-for="m in messages" :key="m.id" style="margin:8px 0">
                <el-card shadow="never">
                  <template #header>
                    <div style="display:flex;justify-content:space-between">
                      <span>用户 {{ m.senderId }}</span>
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
            </el-scrollbar>
            <div style="border-top:1px solid var(--el-border-color);padding:8px;display:flex;align-items:center;gap:8px">
              <el-input v-model="text" placeholder="输入消息，回车发送" @keyup.enter="sendText"></el-input>
              <el-upload :auto-upload="false" :show-file-list="false" :on-change="onSelectFile">
                <el-button>图片</el-button>
              </el-upload>
              <el-button type="primary" @click="sendText">发送</el-button>
            </div>
          </div>
          <div style="width:260px;border-left:1px solid var(--el-border-color);padding:12px">
            <div style="font-weight:600;margin-bottom:8px">成员</div>
            <el-empty description="占位"></el-empty>
          </div>
        </el-main>
      </el-container>
    </el-container>
  </el-container>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { ElMessage } from 'element-plus'
import { useRouter } from 'vue-router'
import { ChatDotRound, ChatLineRound } from '@element-plus/icons-vue'
import { API_BASE, getGroups, getMessages, uploadFile as apiUpload } from '../api/http'

type Group = { id: number; name: string }
type Message = { id:number; groupId:number; senderId:number; type:number; content:string; fileUrl?:string; createdAt?:string }

const router = useRouter()
const user = JSON.parse(localStorage.getItem('user') || '{"id":1}')
const userId = user.id || 1

const groups = ref<Group[]>([])
const messages = ref<Message[]>([])
const currentGroup = ref<Group | null>(null)
const text = ref('')
const keyword = ref('')
const msgScroll = ref()

const filteredGroups = computed(() =>
  groups.value.filter(g => g.name.toLowerCase().includes(keyword.value.toLowerCase()))
)

const conn = new HubConnectionBuilder()
  .withUrl(`${API_BASE}/hubs/chat`)
  .configureLogging(LogLevel.Information)
  .withAutomaticReconnect()
  .build()

conn.on('ReceiveMessage', (m: Message) => {
  if (currentGroup.value && m.groupId === currentGroup.value.id) {
    messages.value.push(m)
    setTimeout(() => msgScroll.value?.scrollTo?.({ top: 999999 }), 0)
  }
})

onMounted(async () => {
  try {
    await conn.start()
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
  currentGroup.value = g
  await conn.invoke('JoinGroup', g.id)
  messages.value = (await getMessages(g.id, 50)).reverse()
  setTimeout(() => msgScroll.value?.scrollTo?.({ top: 999999 }), 0)
}

async function sendText() {
  if (!currentGroup.value || !text.value.trim()) return
  const dto = { groupId: currentGroup.value.id, senderId: userId, type: 1, content: text.value.trim() }
  await conn.invoke('SendMessage', dto)
  text.value = ''
}

async function onSelectFile(file:any) {
  if (!currentGroup.value) return
  const info = await apiUpload(file.raw)
  const dto = { groupId: currentGroup.value.id, senderId: userId, type: 2, content: file.name, fileUrl: info.url }
  await conn.invoke('SendMessage', dto)
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
