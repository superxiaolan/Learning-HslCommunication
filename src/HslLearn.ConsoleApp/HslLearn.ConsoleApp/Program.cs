using HslLearn.Core;

// 实例化封装的 Service
var service = new ModbusService("127.0.0.1", 502, 1);

try
{
    Console.WriteLine("正在连接...");
    service.Connect();

    // 核心修复：设置字节序为 CDAB (这是 Hsl 配合大多数模拟器的默认值)
    service.SetByteOrder(HslCommunication.Core.DataFormat.CDAB);
    // 测试读取浮点数 (地址 2)
    float temp = await service.ReadTemperatureAsync("2");
    Console.WriteLine($"[Service读取] 地址 2 的温度为: {temp} ℃");

    // 测试写入短整数 (地址 0)
    await service.WriteControlValueAsync("0", 789);
    Console.WriteLine("[Service写入] 地址 0 已更新为 789");
}
catch (Exception ex)
{
    Console.WriteLine($"发生错误: {ex.Message}");
}
finally
{
    service.Disconnect();
}