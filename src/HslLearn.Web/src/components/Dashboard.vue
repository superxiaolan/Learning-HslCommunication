<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import * as signalR from '@microsoft/signalr'
import axios from 'axios'

const deviceData = ref({ 
  temperature: 0, 
  statusValue: 0, 
  timestamp: '' 
})
let connection = null
const targetTemp = ref(30.0)
const isSending = ref(false)
const statusMsg = ref('')

const sendTemperature = async () => {
  isSending.value = true
  statusMsg.value = '正在下发...'
  try {
    // 注意：这里建议下发到你 Modbus Slave 里对应的地址（例如 "2"）
    await axios.post('https://localhost:7071/api/plc/write-temp', {
      address: "2", 
      value: targetTemp.value
    })
    statusMsg.value = '下发成功！'
    setTimeout(() => statusMsg.value = '', 3000)
  } catch (err) {
    statusMsg.value = '下发失败：' + err.message
  } finally {
    isSending.value = false
  }
}

// Dashboard.vue
onMounted(async () => {
  // 1. 先尝试获取首屏快照（解决刚打开页面显示 0 的问题）
  try {
    const response = await axios.get('https://localhost:7071/api/plc/latest');
    deviceData.value = response.data;
    console.log('首屏数据加载成功');
  } catch (err) {
    console.error('首屏数据加载失败:', err);
  }

  // 2. 初始化并启动 SignalR
  connection = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7071/deviceHub')
    .withAutomaticReconnect()
    .build();

  connection.on('ReceiveDeviceData', (data) => {
    console.log('收到实时推送:', data);
    deviceData.value = data;
  });

  try {
    await connection.start();
    console.log('SignalR 连接成功！');
  } catch (err) {
    console.error('SignalR 连接失败:', err);
  }
});

onUnmounted(() => {
  if (connection) connection.stop()
})
</script>

<template>
  <div class="mes-dashboard">
    <h2>MES 实时监控看板</h2>
    <div class="card-container">
      <div class="card">
        <label>设备温度</label>
        <div class="value">{{ deviceData.temperature }} °C</div>
      </div>
      <div class="card">
        <label>状态码</label>
        <div class="value">{{ deviceData.statusValue }}</div>
      </div>
    </div>

    <div class="card control-card">
      <h3>远程控制</h3>
      <div class="input-group">
        <input v-model="targetTemp" type="number" placeholder="输入温度" />
        <button @click="sendTemperature" :disabled="isSending">下发指令</button>
      </div>
      <p v-if="statusMsg">{{ statusMsg }}</p>
    </div>

    <p class="footer">最后更新时间: {{ new Date(deviceData.timestamp).toLocaleString() }}</p>
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