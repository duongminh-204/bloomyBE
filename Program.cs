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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Bloomy.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ==================== SERVICES ====================

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
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
        .AllowAnyMethod()
        .AllowCredentials();
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


// ==================== COOKIE AUTH ====================

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/api/auth/login";

        options.Cookie.Name = "BloomyAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.Path = "/";

        // ================= DEVELOPMENT =================
        if (builder.Environment.IsDevelopment())
        {
            options.Cookie.SameSite =
                Microsoft.AspNetCore.Http.SameSiteMode.Lax;

            options.Cookie.SecurePolicy =
                Microsoft.AspNetCore.Http.CookieSecurePolicy.None;
        }
        // ================= PRODUCTION =================
        else
        {
            options.Cookie.SameSite =
                Microsoft.AspNetCore.Http.SameSiteMode.None;

            options.Cookie.SecurePolicy =
                Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        }

        options.ExpireTimeSpan = TimeSpan.FromDays(7);

        // API trả về 401/403 thay vì redirect HTML
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            },

            OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = 403;
                    return Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();


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


// ==================== DEVELOPMENT ====================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseDeveloperExceptionPage();
}


// ==================== MIDDLEWARE ====================

// Production nên bật HTTPS
app.UseHttpsRedirection();

app.UseStaticFiles();

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
    {
        await db.Database.MigrateAsync();

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