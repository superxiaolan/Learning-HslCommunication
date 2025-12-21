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
    }
}
