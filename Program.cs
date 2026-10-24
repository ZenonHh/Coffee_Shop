using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Helpers;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình DbContext kết nối SQL Server
builder.Services.AddDbContext<CoffeeShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// 2. Cấu hình Session (dành cho Giỏ hàng)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. Cấu hình Cookie Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "SmartAuth";
    options.DefaultChallengeScheme = "SmartAuth";
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddPolicyScheme("SmartAuth", "Chọn phiên khách hàng hoặc quản trị", options =>
{
    options.ForwardDefaultSelector = context =>
        context.Request.Path.StartsWithSegments("/Admin")
            ? "AdminCookie"
            : CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.Cookie.Name = "CoffeeShop.Customer";
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
        context.Response.Redirect("/Account/Login?returnUrl=" + Uri.EscapeDataString(returnUrl));
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.Redirect("/Account/AccessDenied");
        return Task.CompletedTask;
    };
})
.AddCookie("AdminCookie", options =>
{
    options.Cookie.Name = "CoffeeShop.Admin";
    options.LoginPath = "/Admin/Account/Login";
    options.LogoutPath = "/Admin/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
        context.Response.Redirect("/Admin/Account/Login?returnUrl=" + Uri.EscapeDataString(returnUrl));
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.Redirect("/Account/AccessDenied");
        return Task.CompletedTask;
    };
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PersistentCartStore>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CoffeeShopDbContext>();
    await dbContext.Database.ExecuteSqlRawAsync("""
        IF OBJECT_ID(N'dbo.CoffeeShopBrowserCart', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.CoffeeShopBrowserCart
            (
                Id uniqueidentifier NOT NULL CONSTRAINT PK_CoffeeShopBrowserCart PRIMARY KEY,
                CartJson nvarchar(max) NOT NULL,
                UpdatedAtUtc datetime2 NOT NULL
            );
        END

        IF OBJECT_ID(N'dbo.Voucher', N'U') IS NOT NULL
        BEGIN
            IF COL_LENGTH(N'dbo.Voucher', N'TenChuongTrinh') IS NULL
                ALTER TABLE dbo.Voucher ADD TenChuongTrinh nvarchar(150) NULL;

            IF COL_LENGTH(N'dbo.Voucher', N'NgayBatDau') IS NULL
                ALTER TABLE dbo.Voucher ADD NgayBatDau datetime2 NULL;

            IF COL_LENGTH(N'dbo.Voucher', N'NgayKetThuc') IS NULL
                ALTER TABLE dbo.Voucher ADD NgayKetThuc datetime2 NULL;

            IF COL_LENGTH(N'dbo.Voucher', N'DonToiThieu') IS NULL
                ALTER TABLE dbo.Voucher ADD DonToiThieu decimal(12,2) NOT NULL
                    CONSTRAINT DF_Voucher_DonToiThieu DEFAULT (0) WITH VALUES;
        END
        """);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Route cho Area (Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

// Default Route (Storefront)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
