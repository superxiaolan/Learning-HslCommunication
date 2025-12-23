using HslLearn.Core;
using HslLearn.Core.Data; // 确保指向你的 AppDbContext 命名空间
using HslLearn.Core.Entities;
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

        public PlcSamplingWorker(
            IServiceProvider serviceProvider,
            ModbusService modbusService,
            ILogger<PlcSamplingWorker> logger,
            IDistributedCache cache)
        {
            _serviceProvider = serviceProvider;
            _modbusService = modbusService;
            _logger = logger;
            _cache = cache;
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