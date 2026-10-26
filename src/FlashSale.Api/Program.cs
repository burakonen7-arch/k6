using FlashSale.Application;
using FlashSale.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Prometheus;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContextPool<StoreDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Store"), p => p.CommandTimeout(15)));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
var app = builder.Build();
app.UseExceptionHandler();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseHttpMetrics();
app.MapControllers();
// Infrastructure middleware; business endpoints are in controllers.
app.UseMetricServer();
if (builder.Configuration.GetValue<bool>("Database:Initialize"))
{
 await using var scope = app.Services.CreateAsyncScope();
 await DatabaseSetup.InitializeAsync(scope.ServiceProvider.GetRequiredService<StoreDbContext>());
}
await app.RunAsync();
