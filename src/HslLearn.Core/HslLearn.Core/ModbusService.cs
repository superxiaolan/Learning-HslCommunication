using HslCommunication.ModBus;

namespace HslLearn.Core
{
    public class ModbusService
    {
        private ModbusTcpNet _plc;

        public ModbusService(string ipAddress, int port, byte station)
        {
            _plc = new ModbusTcpNet(ipAddress, port, station);
        }

        public async Task<float> ReadTemperatureAsync(string address) 
        {
            // 使用异步方法读取数据，这在 Web 开发中非常重要，以避免阻塞线程
            //var result = await _plc.ReadInt16Async(address);

            // ReadFloat 会自动读取连续的 2 个寄存器
            var result = await _plc.ReadFloatAsync(address);
            if (result.IsSuccess)
            {
                return result.Content;
            }
            else
            {
                throw new Exception($"读取浮点数失败: {result.Message}");
            }
        }

        public async Task WriteControlValueAsync(string address, short value)
        {
            var result = await _plc.WriteAsync(address, value);
            if (!result.IsSuccess)
            {
                throw new Exception($"写入失败: {result.Message}");
            }
        }

        public void SetByteOrder(HslCommunication.Core.DataFormat format)
        {
            // 工业界常见的四种顺序：ABCD, BADC, CDAB, DCBA
            // 如果你发现读出来的数值极其诡异，通常就是这里需要调整
            _plc.ByteTransform.DataFormat = format;
        }

        public void Connect()
        {
            var result = _plc.ConnectServer();
            if (!result.IsSuccess)
            {
                throw new Exception($"连接失败: {result.Message}");
            }
        }

        public void Disconnect() => _plc.ConnectClose();

    }
}
