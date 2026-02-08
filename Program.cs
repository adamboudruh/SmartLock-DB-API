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

// Add services to the container.
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

try
{
    var applyMigrations = app.Configuration.GetValue<bool>("APPLY_MIGRATIONS", false);
    if (applyMigrations)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        try
        {
            logger.LogInformation("APPLY_MIGRATIONS is true — attempting to apply pending EF Core migrations...");
            var db = scope.ServiceProvider.GetRequiredService<SmartLockDbContext>();
            db.Database.Migrate();
            logger.LogInformation("EF Core migrations applied successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying migrations on startup.");
            // Rethrow to avoid running app in an inconsistent state. Remove throw if you prefer to continue.
            throw;
        }
    }
    else
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("APPLY_MIGRATIONS is false (or not set) — skipping automatic EF Core migrations.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Fatal error during migration step: {ex}");
    throw;
}

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
