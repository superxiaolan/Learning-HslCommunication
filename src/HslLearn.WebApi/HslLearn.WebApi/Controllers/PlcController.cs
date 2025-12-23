using HslLearn.Core;
using HslLearn.Core.Data;
using HslLearn.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace HslLearn.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlcController : ControllerBase
    {
        private readonly ModbusService _modbusService;

        // 【关键修复】：添加构造函数，让 DI 容器把单例注入进来
        public PlcController(ModbusService modbusService)
        {
            _modbusService = modbusService;
        }

        [HttpGet("data")]
        public async Task<IActionResult> GetPlcData()
        {
            try
            {
                // 假设 PLC 中温度数据(Float)存储在 "2" 地址
                float temp = await _modbusService.ReadTemperatureAsync("2");
                return Ok(new
                {
                    success = true,
                    temperature = temp,
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("write-temp")]
        public async Task<IActionResult> WriteTemp([FromBody] WriteParams data)
        {
            try
            {
                await _modbusService.WriteTemperatureAsync(data.Address, data.Value);
                return Ok(new
                {
                    success = true,
                    message = $"已成功将 {data.Value} 写入地址 {data.Address}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPost("snapshot")]
        public async Task<IActionResult> TakeSnapshot([FromServices] AppDbContext db)
        {
            // 1. 自动从 PLC 读取整套实体数据
            var data = await _modbusService.ReadEntityAsync<DeviceDataLog>();

            // 2. 存入 PostgreSQL
            db.DeviceLogs.Add(data);
            await db.SaveChangesAsync();

            return Ok(data);
        }

        [HttpGet("realtime")]
        public async Task<IActionResult> GetRealtimeData([FromServices] IDistributedCache cache)
        {
            var jsonData = await cache.GetStringAsync("LatestDeviceData");
            if (string.IsNullOrEmpty(jsonData)) return NotFound("暂无实时数据");

            return Ok(JsonSerializer.Deserialize<DeviceDataLog>(jsonData));
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestFromRedis([FromServices] IDistributedCache cache)
        {
            // 获取 Hash 中的 data 字段（注意：如果你用的是 IDistributedCache，它通常映射为简单的 String 或 Byte[]）
            // 如果你之前是用 IDistributedCache 存的，Key 应该是你设定的那个
            var json = await cache.GetStringAsync("LatestDeviceData");

            if (string.IsNullOrEmpty(json)) return NotFound();

            return Content(json, "application/json");
        }

        public class WriteParams
        {
            public string Address { get; set; } = "2";
            public float Value { get; set; }
        }
    }
}
