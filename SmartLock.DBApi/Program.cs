using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SmartLock.DBApi.Data;
using SmartLock.DBApi.Operations;
using System.Security.Cryptography;

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
builder.Services.AddScoped<IEventsOperations, EventsOperations>();
builder.Services.AddScoped<IDevicesOperations, DevicesOperations>(); 

// Allow local dev CORS (adjust for production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDev", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// JWT Authentication configured
var publicKeyPem = builder.Configuration["BACKEND_PUBLIC_KEY"]
    ?? File.ReadAllText("public.pem");
var rsa = RSA.Create();
rsa.ImportFromPem(publicKeyPem);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "smartlock-backend",
            ValidateAudience = false,
            ValidateLifetime = true,  // enforces expiresIn: '30s'
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsa),
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();

// Apply pending EF Core migrations on startup
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

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
