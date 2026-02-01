using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.Operations;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var configuration = builder.Configuration;

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext: expects "DefaultConnection" in appsettings
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SmartLockDbContext>(options =>
    options.UseSqlServer(connectionString));
// Register operations
builder.Services.AddScoped<IKeysOperations, KeysOperations>();

// Allow local dev CORS (adjust for production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDev", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("LocalDev");
app.UseAuthorization();
app.MapControllers();

app.Run();