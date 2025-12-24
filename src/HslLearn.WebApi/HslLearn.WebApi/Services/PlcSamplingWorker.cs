using HslLearn.Core;
using HslLearn.Core.Data; // 确保指向你的 AppDbContext 命名空间
using HslLearn.Core.Entities;
using HslLearn.WebApi.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json; // 确保指向你的 DeviceDataLog 命名空间

namespace HslLearn.WebApi.Services
{
    public class PlcSamplingWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ModbusService _modbusService;
        private readonly ILogger<PlcSamplingWorker> _logger;
        private readonly IDistributedCache _cache;
        private readonly IHubContext<DeviceHub> _hubContext;// 注入上下文

        public PlcSamplingWorker(
            IServiceProvider serviceProvider,
            ModbusService modbusService,
            ILogger<PlcSamplingWorker> logger,
            IDistributedCache cache,
            IHubContext<DeviceHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _modbusService = modbusService;
            _logger = logger;
            _cache = cache;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(">>> PLC 自动采集服务已启动...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 因为 DbContext 是 Scoped (作用域) 的，后台服务是 Singleton
                    // 所以必须手动创建一个作用域来获取 DbContext
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        // 1. 调用写的反射方法一键读取实体
                        var data = await _modbusService.ReadEntityAsync<DeviceDataLog>();

                        // 2. 写入 Redis（作为实时看板数据）
                        var jsonData = JsonSerializer.Serialize(data);
                        await _cache.SetStringAsync("LatestDeviceData", jsonData);

                        // 3. 存入 PostgreSQL
                        db.DeviceLogs.Add(data);
                        await db.SaveChangesAsync(stoppingToken);

                        // 4. 通过 SignalR 广播到所有在线的连接的前端客户端
                        await _hubContext.Clients.All.SendAsync("ReceiveDeviceData", data, cancellationToken: stoppingToken);

                        _logger.LogInformation($"[SignalR 推送成功] Temp: {data.Temperature}");
                        _logger.LogInformation($"[自动采集成功] {DateTime.Now:HH:mm:ss} -> Temp: {data.Temperature}, Status: {data.StatusValue}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[采集发生异常]: {ex.Message}");
                }

                // 每 5 秒采集一次，可以按需调整
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}