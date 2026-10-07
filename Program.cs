using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using StationeryWarehouse.Data;
using StationeryWarehouse.Entities;
using StationeryWarehouse.Models;
using StationeryWarehouse.Services;
using StationeryWarehouse.Services.Inventory;
using StationeryWarehouse.Services.Warehouse;
using StationeryWarehouse.Services.Inbound;
using StationeryWarehouse.Services.Outbound;
using StationeryWarehouse.Services.Transfer;
using StationeryWarehouse.Services.Stocktake;
using StationeryWarehouse.Services.Settings;
using StationeryWarehouse.Services.Product;
using StationeryWarehouse.Services.Supplier;

var builder = WebApplication.CreateBuilder(args);


// ============================================================
// 1. DATABASE
// ============================================================

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is not configured.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));


// ============================================================
// 2. MVC
// ============================================================

builder.Services.AddControllersWithViews();


// ============================================================
// 3. JWT SETTINGS
// ============================================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

if (jwtSettings == null ||
    string.IsNullOrWhiteSpace(jwtSettings.Key))
{
    throw new InvalidOperationException(
        "JwtSettings chưa được cấu hình trong appsettings.json."
    );
}

var key = Encoding.UTF8.GetBytes(
    jwtSettings.Key
);


// ============================================================
// 4. AUTHENTICATION - JWT
// ============================================================

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

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    jwtSettings.Issuer,

                ValidAudience =
                    jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(key),

                ClockSkew =
                    TimeSpan.Zero
            };


        // --------------------------------------------------------
        // Đọc JWT từ HttpOnly Cookie
        // --------------------------------------------------------

        options.Events =
            new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token =
                        context.Request.Cookies[
                            "access_token"
                        ];

                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        context.Token = token;
                    }

                    return Task.CompletedTask;
                },


                // ------------------------------------------------
                // Chưa đăng nhập
                // ------------------------------------------------

                OnChallenge = context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        context.HandleResponse();

                        var returnUrl =
                            context.Request.Path +
                            context.Request.QueryString;

                        context.Response.Redirect(
                            "/Auth/Login?returnUrl=" +
                            Uri.EscapeDataString(
                                returnUrl
                            )
                        );
                    }

                    return Task.CompletedTask;
                },


                // ------------------------------------------------
                // Không có quyền
                // ------------------------------------------------

                OnForbidden = context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.Redirect(
                            "/Auth/AccessDenied"
                        );
                    }

                    return Task.CompletedTask;
                }
            };
    });


// ============================================================
// 5. AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization();


// ============================================================
// 6. APPLICATION SERVICES
// ============================================================

// ------------------------------------------------------------
// Authentication
// ------------------------------------------------------------

builder.Services.AddScoped<
    IAuthService,
    AuthService
>();

builder.Services.AddScoped<
    IJwtService,
    JwtService
>();


// ------------------------------------------------------------
// Password Hashing
// ------------------------------------------------------------

builder.Services.AddScoped<
    IPasswordHasher<AppUser>,
    PasswordHasher<AppUser>
>();


// ------------------------------------------------------------
// S01 - Tồn kho
// ------------------------------------------------------------

builder.Services.AddScoped<
    IInventoryService,
    InventoryService
>();


// ------------------------------------------------------------
// S03 - Kho & vị trí
// ------------------------------------------------------------

builder.Services.AddScoped<
    IWarehouseService,
    WarehouseService
>();

// ------------------------------------------------------------
// S04 - Nhập kho
// ------------------------------------------------------------

builder.Services.AddScoped<
    IInboundService,
    InboundService
>();

// ------------------------------------------------------------
// S05 - Xuất kho
// ------------------------------------------------------------ 

builder.Services.AddScoped<
    IOutboundService,
    OutboundService>();

// ------------------------------------------------------------
// S06 - Chuyển kho
// ------------------------------------------------------------
builder.Services.AddScoped<
    ITransferService,
    TransferService>();

// ------------------------------------------------------------
// S07 - Kiểm kê
// ------------------------------------------------------------
builder.Services.AddScoped<
    IStocktakeService,
    StocktakeService>();

// ------------------------------------------------------------
// S08 - Cấu hình & người dùng
// ------------------------------------------------------------
builder.Services.AddScoped<
    ISettingsService,
    SettingsService>();

// ------------------------------------------------------------
// S09 - Sản phẩm
// ------------------------------------------------------------
builder.Services.AddScoped<
    IProductService,
    ProductService>();

// ------------------------------------------------------------
// S10 - Nhà cung cấp
// ------------------------------------------------------------
builder.Services.AddScoped<
    ISupplierService,
    SupplierService>();
// ============================================================
// 7. BUILD APPLICATION
// ============================================================

var app = builder.Build();


// ============================================================
// 8. DATABASE CONNECTION CHECK
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    if (!await context.Database.CanConnectAsync())
    {
        throw new InvalidOperationException(
            "Cannot connect to the configured SQL Server database. " +
            "Create and populate the database with " +
            "Database/StationeryWarehouse.sql, then verify " +
            "ConnectionStrings:DefaultConnection.");
    }

    if (!await context.AppRoles.AnyAsync())
    {
        throw new InvalidOperationException(
            "The configured database has no application roles. " +
            "Run Database/StationeryWarehouse.sql against a new or empty database.");
    }
}


// ============================================================
// 9. HTTP REQUEST PIPELINE
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error"
    );

    app.UseHsts();
}


// ------------------------------------------------------------
// HTTPS
// ------------------------------------------------------------

app.UseHttpsRedirection();


// ------------------------------------------------------------
// Static files
// ------------------------------------------------------------

app.UseStaticFiles();


// ------------------------------------------------------------
// Routing
// ------------------------------------------------------------

app.UseRouting();


// ------------------------------------------------------------
// Authentication
// ------------------------------------------------------------

app.UseAuthentication();


// ------------------------------------------------------------
// Authorization
// ------------------------------------------------------------

app.UseAuthorization();


// ============================================================
// 10. ROUTING
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}"
);


// ============================================================
// 11. RUN APPLICATION
// ============================================================

app.Run();