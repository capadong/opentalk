<template>
  <div style="height:100vh;display:flex;align-items:center;justify-content:center;background:#f5f7fa">
    <el-card style="width:380px">
      <template #header>OpenTalk 登录</template>
      <el-form :model="form" label-width="80px" @keyup.enter="login">
        <el-form-item label="用户名">
          <el-input v-model="form.username" placeholder="username"></el-input>
        </el-form-item>
        <el-form-item label="密码">
          <el-input v-model="form.password" type="password" placeholder="password" show-password></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" class="w-100" @click="login" :loading="loading">登录</el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
  </template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessage } from 'element-plus'
import { login as apiLogin } from '../api/http'

const router = useRouter()
const loading = ref(false)
const form = reactive({ username: 'capad', password: 'capad' })

async function login() {
  loading.value = true
  try {
    const data = await apiLogin(form.username, form.password)
    localStorage.setItem('token', data.token || 'token')
    localStorage.setItem('user', JSON.stringify(data.user || {}))
    router.push('/chat')
  } catch (e:any) {
    ElMessage.error(e.message || '登录出错')
  } finally {
    loading.value = false
  }
}
</script>
