using Bloomy.Data;
using Bloomy.Data.Interfaces;
using Bloomy.Data.Repositories;
using Bloomy.Models;
using Bloomy.Services;
using Bloomy.Hubs;
using BloomyBE.Configuration;
using BloomyBE.Data;
using BloomyBE.Services;
using BloomyBE.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Bloomy.Services.Interfaces;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// ==================== SERVICES ====================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
        // Ensure DateTime values are handled correctly (ISO 8601 format)
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// ==================== CORS ====================

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (string.IsNullOrWhiteSpace(origin))
                return false;

            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                return false;

            // ================= DEVELOPMENT =================
            if (builder.Environment.IsDevelopment())
            {
                return uri.Host is "localhost"
                    or "127.0.0.1"
                    || uri.Host.StartsWith("192.168.")
                    || uri.Host.StartsWith("10.");
            }

            // ================= PRODUCTION =================
            return uri.Host.Equals("bloomy-fe.vercel.app")
                || uri.Host.Contains("vercel.app");
        })
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});


// ==================== DATABASE ====================

var connectionString = builder.Environment.IsDevelopment()
    ? builder.Configuration.GetConnectionString("LocalConnection")
    : builder.Configuration.GetConnectionString("ProductionConnection");

builder.Services.AddDbContext<BloomyDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});


// ==================== JWT AUTH ====================
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // SignalR WebSocket: extract JWT from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// ==================== JWT TOKEN SERVICE ====================
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// ==================== SIGNALR ====================
builder.Services.AddSignalR();

// ==================== CONFIG ====================
builder.Services.Configure<BookingSettings>(
    builder.Configuration.GetSection("BookingSettings"));

// ==================== REPOSITORIES ====================
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddScoped<IPaymentSettingsService, PaymentSettingsService>();

builder.Services.AddScoped<IChatRepository, ChatRepository>();
builder.Services.AddScoped<IChatService, ChatService>();

// ==================== BUILD APP ====================
var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto
});

// ==================== DEVELOPMENT ====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseDeveloperExceptionPage();
}


// ==================== MIDDLEWARE ====================

// Production nên bật HTTPS
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();


// ==================== ROUTES ====================
app.MapControllers();
app.MapHub<ChatHub>("/api/chathub");

// ==================== DATABASE MIGRATION ====================
using (var scope = app.Services.CreateScope())
{
   var db = scope.ServiceProvider.GetRequiredService<BloomyDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {       await db.Database.MigrateAsync();

        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(
            ex,
            "EF migration failed. Run: dotnet ef database update --project BloomyBE"
        );
    }
    await DatabaseSeeder.SeedAsync(db);
    var paymentSettings =
        scope.ServiceProvider.GetRequiredService<IPaymentSettingsService>();
    await paymentSettings.GetAsync();
}
app.Run();