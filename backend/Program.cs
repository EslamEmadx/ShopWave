using System.Text;
using System.Threading.RateLimiting;
using backend;
using backend.Data;
using backend.Middleware;
using backend.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// ── Load .env file ──────────────────────────────────────────────────────────
Env.Load();

// ── Serilog bootstrap ───────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ShopWave")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        Environment.GetEnvironmentVariable("Logging__LogFilePath") ?? "logs/shopwave-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting ShopWave API...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ── Database ────────────────────────────────────────────────────────────
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? throw new InvalidOperationException("Database connection string is not configured.");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    // ── JWT Authentication ──────────────────────────────────────────────────
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? Environment.GetEnvironmentVariable("Jwt__Key")
        ?? throw new InvalidOperationException("JWT key is not configured. Set Jwt__Key env var.");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"]
        ?? Environment.GetEnvironmentVariable("Jwt__Issuer")
        ?? "ShopWave";
    var jwtAudience = builder.Configuration["Jwt:Audience"]
        ?? Environment.GetEnvironmentVariable("Jwt__Audience")
        ?? "ShopWave";

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

    builder.Services.AddAuthorization();

    // ── Services (DI) ───────────────────────────────────────────────────────
    builder.Services.AddScoped<TokenService>();
    builder.Services.AddScoped<AuditService>();
    builder.Services.AddScoped<InputSanitizer>();
    builder.Services.AddScoped<backend.Services.Payments.StripePaymentProviderPlaceholder>();
    builder.Services.AddScoped<backend.Services.Payments.PaymobPaymentProviderPlaceholder>();
    builder.Services.AddScoped<backend.Services.Payments.PaymentProviderFactory>();

    // ── CORS (from env) ─────────────────────────────────────────────────────
    var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"]
        ?? Environment.GetEnvironmentVariable("Cors__AllowedOrigins")
        ?? "http://localhost:5173,http://localhost:3000")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
    });

    // ── Rate Limiting ───────────────────────────────────────────────────────
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = 429;

        // General rate limit
        options.AddFixedWindowLimiter("general", opt =>
        {
            opt.PermitLimit = int.TryParse(
                Environment.GetEnvironmentVariable("RateLimiting__GeneralPermitLimit"), out var g) ? g : 100;
            opt.Window = TimeSpan.FromSeconds(
                int.TryParse(Environment.GetEnvironmentVariable("RateLimiting__GeneralWindowSeconds"), out var gw) ? gw : 60);
            opt.QueueLimit = 0;
        });

        // Auth rate limit (login/register/forgot)
        options.AddFixedWindowLimiter("auth", opt =>
        {
            opt.PermitLimit = int.TryParse(
                Environment.GetEnvironmentVariable("RateLimiting__LoginPermitLimit"), out var l) ? l : 5;
            opt.Window = TimeSpan.FromSeconds(
                int.TryParse(Environment.GetEnvironmentVariable("RateLimiting__LoginWindowSeconds"), out var lw) ? lw : 60);
            opt.QueueLimit = 0;
        });

        // Review creation rate limit
        options.AddFixedWindowLimiter("reviews", opt =>
        {
            opt.PermitLimit = int.TryParse(
                Environment.GetEnvironmentVariable("RateLimiting__ReviewPermitLimit"), out var r) ? r : 2;
            opt.Window = TimeSpan.FromSeconds(
                int.TryParse(Environment.GetEnvironmentVariable("RateLimiting__ReviewWindowSeconds"), out var rw) ? rw : 60);
            opt.QueueLimit = 0;
        });
    });

    // ── Output Caching ──────────────────────────────────────────────────────
    builder.Services.AddOutputCache(options =>
    {
        options.AddBasePolicy(b => b.NoCache());
        options.AddPolicy("PublicShort", b => b.Expire(TimeSpan.FromMinutes(2)).Tag("products"));
        options.AddPolicy("PublicMedium", b => b.Expire(TimeSpan.FromMinutes(10)).Tag("categories"));
    });

    // ── Health Checks ───────────────────────────────────────────────────────
    builder.Services.AddHealthChecks()
        .AddCheck("database", new DbHealthCheck(connectionString));

    // ── Controllers & Swagger ───────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // ── Middleware Pipeline (ORDER MATTERS) ──────────────────────────────────
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<ExceptionMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("CorrelationId",
                httpContext.Items["CorrelationId"]?.ToString() ?? "unknown");
        };
    });

    // Seed database
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        SeedData.Initialize(context);
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowFrontend");
    app.UseRateLimiter();
    app.UseOutputCache();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // ── Health Check Endpoint ────────────────────────────────────────────────
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
