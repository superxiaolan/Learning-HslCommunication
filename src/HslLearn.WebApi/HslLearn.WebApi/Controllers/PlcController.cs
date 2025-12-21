using HslLearn.Core;
using Microsoft.AspNetCore.Mvc;

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

        public class WriteParams
        {
            public string Address { get; set; } = "2";
            public float Value { get; set; }
        }
    }
}
