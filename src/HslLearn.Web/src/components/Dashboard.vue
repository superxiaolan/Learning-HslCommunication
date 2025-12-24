<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import axios from 'axios'

// 定义响应式数据对象
const deviceData = ref({
  Temperature: 0,
  StatusValue: 0,
  Timestamp: ''
})

let timer = null

// 获取数据的函数
const fetchLatestData = async () => {
  try {
    // 替换为你真实的后端 API 地址
    const response = await axios.get('https://localhost:7071/api/plc/latest') 
    console.log('Raw Response:', response.data)
    
    deviceData.value = response.data
    console.log('数据已更新:', deviceData.value)
  } catch (error) {
    console.error('无法获取 Redis 数据:', error)
  }
}

onMounted(() => {
  fetchLatestData() // 初始加载一次
  // 设置每 1 秒轮询一次，模拟实时监控
  timer = setInterval(fetchLatestData, 1000)
})

onUnmounted(() => {
  // 组件卸载时清除定时器，防止内存泄漏
  if (timer) clearInterval(timer)
})
</script>

<template>
  <div class="mes-dashboard">
    <h2>MES 实时监控看板</h2>
    <div class="card-container">
      <div class="card">
        <label>设备温度</label>
        <div class="value">{{ deviceData.Temperature }} °C</div>
      </div>
      <div class="card">
        <label>状态码</label>
        <div class="value">{{ deviceData.StatusValue }}</div>
      </div>
    </div>
    <p class="footer">最后更新时间: {{ new Date(deviceData.Timestamp).toLocaleString() }}</p>
  </div>
</template>

<style scoped>
.mes-dashboard { font-family: 'Segoe UI', sans-serif; }
.card-container { display: flex; gap: 20px; margin-top: 20px; }
.card { 
  background: white; 
  padding: 20px; 
  border-radius: 8px; 
  box-shadow: 0 4px 6px rgba(0,0,0,0.1); 
  min-width: 150px;
}
.value { font-size: 2rem; font-weight: bold; color: #2c3e50; }
.footer { margin-top: 20px; color: #7f8c8d; font-size: 0.9rem; }
</style>