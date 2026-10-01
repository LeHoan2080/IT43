using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StationeryWarehouse.Data;
using StationeryWarehouse.Entities;
using StationeryWarehouse.Models;
using StationeryWarehouse.Services;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// SQL SERVER
// ========================================

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration
            .GetConnectionString("DefaultConnection")
    )
);

// ========================================
// MVC
// ========================================

builder.Services.AddControllersWithViews();

// ========================================
// JWT SETTINGS
// ========================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt")
);

var jwtSettings =
    builder.Configuration
        .GetSection("Jwt")
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing."
    );

// ========================================
// JWT AUTHENTICATION
// ========================================

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.Key
                        )
                    ),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies
                    .TryGetValue(
                        "access_token",
                        out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };
    });

// ========================================
// AUTHORIZATION
// ========================================

builder.Services.AddAuthorization();

// ========================================
// PASSWORD HASHER
// ========================================

builder.Services.AddScoped<
    IPasswordHasher<AppUser>,
    PasswordHasher<AppUser>
>();

// ========================================
// SERVICES
// ========================================

builder.Services.AddScoped<
    IAuthService,
    AuthService
>();

builder.Services.AddScoped<
    IJwtService,
    JwtService
>();

var app = builder.Build();

// ========================================
// HTTP PIPELINE
// ========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}"
);

app.Run();