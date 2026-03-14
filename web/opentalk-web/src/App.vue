<template>
  <div style="display:flex; height:100vh; font-family: system-ui, sans-serif;">
    <div style="width:260px; border-right:1px solid #eee; padding:12px;">
      <h3>群列表</h3>
      <div v-for="g in groups" :key="g.id" @click="selectGroup(g)" style="padding:6px; cursor:pointer;">
        {{ g.name }}
      </div>
    </div>
    <div style="flex:1; display:flex; flex-direction:column;">
      <div style="flex:1; display:flex;">
        <div style="flex:1; padding:12px; overflow:auto;">
          <div v-for="m in messages" :key="m.id" style="margin-bottom:8px;">
            <b>{{ m.senderId }}</b>: <span v-if="m.type === 1">{{ m.content }}</span>
            <img v-else-if="m.type === 2" :src="m.fileUrl" style="max-width:240px;" />
            <a v-else :href="m.fileUrl" target="_blank">{{ m.content }}</a>
          </div>
        </div>
        <div style="width:240px; border-left:1px solid #eee; padding:12px;">
          <h3>成员</h3>
          <div>（示例占位）</div>
        </div>
      </div>
      <div style="border-top:1px solid #eee; padding:12px; display:flex; gap:8px;">
        <input v-model="text" @keyup.enter="sendText" style="flex:1; padding:8px;" placeholder="输入消息回车发送" />
        <input type="file" @change="uploadFile" />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'

type Group = { id: number; name: string }
type Message = { id:number; groupId:number; senderId:number; type:number; content:string; fileUrl?:string }

const api = 'http://localhost:5000'
const groups = ref<Group[]>([])
const messages = ref<Message[]>([])
const currentGroup = ref<Group | null>(null)
const text = ref('')
const userId = 1

const conn = new HubConnectionBuilder()
  .withUrl(`${api}/hubs/chat`)
  .configureLogging(LogLevel.Information)
  .withAutomaticReconnect()
  .build()

conn.on('ReceiveMessage', (m: Message) => {
  if (currentGroup.value && m.groupId === currentGroup.value.id) {
    messages.value.unshift(m)
  }
})

onMounted(async () => {
  await conn.start()
  const res = await fetch(`${api}/api/v1/groups/${userId}`)
  groups.value = await res.json()
  if (groups.value.length) selectGroup(groups.value[0])
})

async function selectGroup(g: Group) {
  currentGroup.value = g
  await conn.invoke('JoinGroup', g.id)
  const res = await fetch(`${api}/api/v1/messages/${g.id}?limit=50`)
  messages.value = await res.json()
}

async function sendText() {
  if (!currentGroup.value || !text.value.trim()) return
  const dto = { groupId: currentGroup.value.id, senderId: userId, type: 1, content: text.value.trim() }
  await conn.invoke('SendMessage', dto)
  text.value = ''
}

async function uploadFile(e: Event) {
  const input = e.target as HTMLInputElement
  if (!input.files?.length || !currentGroup.value) return
  const form = new FormData()
  form.append('file', input.files[0])
  const res = await fetch(`${api}/api/v1/files/upload`, { method: 'POST', body: form })
  const info = await res.json()
  const dto = { groupId: currentGroup.value.id, senderId: userId, type: 2, content: input.files[0].name, fileUrl: info.url }
  await conn.invoke('SendMessage', dto)
  input.value = ''
}
</script>
