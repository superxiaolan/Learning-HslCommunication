using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HslLearn.Core.Entities
{
    public class DeviceDataLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [PlcAddress("2")] // 对应 Modbus 地址 2 (温度)
        public float Temperature { get; set; }

        [PlcAddress("0")] // 对应 Modbus 地址 0 (压力/状态)
        public short StatusValue { get; set; }
    }
}
