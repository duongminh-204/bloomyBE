using Bloomy.Data;
using Bloomy.Data.Interfaces;
using Bloomy.Data.Repositories;
using Bloomy.Models;
using Bloomy.Services;
using BloomyBE.Configuration;
using BloomyBE.Services;
using BloomyBE.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==================== SERVICES ====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "http://localhost:5175"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Database
builder.Services.AddDbContext<BloomyDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionString"));
});

// ====================== COOKIE AUTHENTICATION ======================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/api/auth/login";
        options.Cookie.Name = "BloomyAuth";
        options.Cookie.HttpOnly = true;
        // On development, allow the cookie in cross-origin fetches from the Vite dev server.
        if (builder.Environment.IsDevelopment())
        {
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.None;
        }
        else
        {
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None;
            options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
        }
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        // For API calls, return 401/403 instead of redirecting to login page
        options.Events = new Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
                {
                    ctx.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api") && ctx.Response.StatusCode == 200)
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

// Booking settings
builder.Services.Configure<BookingSettings>(builder.Configuration.GetSection("BookingSettings"));

// ====================== REPOSITORIES & SERVICES ======================
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

// ==================== BUILD & MIDDLEWARE ====================
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseCors("AllowFrontend");
app.UseStaticFiles();
// app.UseHttpsRedirection();

app.UseAuthentication();   
app.UseAuthorization();

app.MapControllers();

app.Run();