using HslLearn.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 1. 将 ModbusService 注册为单例 (Singleton)
// 因为 PLC 通常维持一个长连接，不建议每个请求都建立新连接
var modbusService = new ModbusService("127.0.0.1", 502, 1);
modbusService.Connect(); // 启动时开启连接

// 【添加这一行】：对齐 Modbus Slave 的 Float CD AB 格式
modbusService.SetByteOrder(HslCommunication.Core.DataFormat.CDAB);
builder.Services.AddSingleton(modbusService);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
