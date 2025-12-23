using HslLearn.Core;
using HslLearn.Core.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 1. 将 ModbusService 注册为单例 (Singleton)
// 因为 PLC 通常维持一个长连接，不建议每个请求都建立新连接
var modbusService = new ModbusService("127.0.0.1", 502, 1);
modbusService.Connect(); // 启动时开启连接

// 【添加这一行】：对齐 Modbus Slave 的 Float CD AB 格式
modbusService.SetByteOrder(HslCommunication.Core.DataFormat.CDAB);
builder.Services.AddSingleton(modbusService);

// 找到这行代码并修改
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 设置属性名称策略为 null，即保持原样（大写开头）
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// 配置 CORS 策略，允许所有来源访问 API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 在 builder.Build() 之前
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        "Host=localhost;Database=mes_db;Username=postgres;Password=123456",
        x => x.MigrationsAssembly("HslLearn.WebApi") // 【关键：让迁移文件生成在 WebApi 项目中】
    ));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Program.cs 中添加
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "MES_";
});
// 注册后台定时采集服务
builder.Services.AddHostedService<HslLearn.WebApi.Services.PlcSamplingWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
//启用中间件 (必须放在 MapControllers 之前)
app.UseCors("AllowAll");
app.MapControllers();

app.Run();
