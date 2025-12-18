using HslCommunication;
using HslCommunication.ModBus;

// 1. 实例化 (IP: 127.0.0.1, Port: 502, Station: 1)
// 注意：ModbusTcpNet 默认站号是 255，而模拟器默认通常是 1，建议显式指定
ModbusTcpNet plc = new ModbusTcpNet("127.0.0.1", 502, 1);

Console.WriteLine("--- 开始连接模拟器 ---");

// 2. 建立连接 (Hsl 的所有通讯几乎都要先 ConnectServer)
OperateResult connect = plc.ConnectServer();

if (!connect.IsSuccess)
{
    Console.WriteLine($"连接失败：{connect.Message}");
    return;
}

Console.WriteLine("连接成功！");

// 3. 读取数据 (以刚才在模拟器改的地址 0 为例)
// OperateResult<T> 是 Hsl 的标准返回格式，包含是否成功、消息、以及核心内容 Content
OperateResult<short> read = plc.ReadInt16("0");

if (read.IsSuccess)
{
    Console.WriteLine($"[读取成功] 地址 0 的值为: {read.Content}");
}
else
{
    Console.WriteLine($"[读取失败] 原因: {read.Message}");
}

// 4. 写入数据
short valueToWrite = 456;
OperateResult write = plc.Write("0", valueToWrite);

if (write.IsSuccess)
{
    Console.WriteLine($"[写入成功] 已将 {valueToWrite} 写入地址 0");
    // 此时你去观察 Modbus Slave 模拟器，你会发现数值变了
}
else
{
    Console.WriteLine($"[写入失败] 原因: {write.Message}");
}

// 5. 断开连接
plc.ConnectClose();
Console.WriteLine("已安全断开连接。按任意键退出...");
Console.ReadKey();