using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
public class AccountController : Controller
{
    private readonly CoffeeShopDbContext _context;

    public AccountController(CoffeeShopDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true
            && (User.IsInRole("Admin") || User.IsInRole("NhanVien") || User.IsInRole("Staff") || User.IsInRole("CRM")))
        {
            return User.IsInRole("Admin") || User.IsInRole("CRM")
                ? RedirectToAction("Index", "Dashboard")
                : RedirectToAction("Index", "Orders");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var username = model.Username.Trim();
        var password = model.Password;
        var account = await _context.Accounts
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.TenDangNhap == username && a.TrangThai);

        var roleName = account?.Role?.TenVaiTro;
        var isStaff = IsInternalRole(roleName);
        var passwordValid = account != null && VerifyPassword(password, account.MatKhau);
        if (!isStaff || !passwordValid)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu quản trị không chính xác.");
            return View(model);
        }

        if (!account!.MatKhau.StartsWith("$2", StringComparison.Ordinal))
        {
            account.MatKhau = BCrypt.Net.BCrypt.HashPassword(password);
            await _context.SaveChangesAsync();
        }

        await HttpContext.SignOutAsync("AdminCookie");
        roleName = roleName switch
        {
            "Admin" => "Admin",
            "Staff" => "Staff",
            "CRM" => "CRM",
            _ => "NhanVien"
        };
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, account!.TenDangNhap),
            new Claim(ClaimTypes.GivenName, account.HoTen),
            new Claim(ClaimTypes.Role, roleName),
            new Claim("AccountId", account.MaTaiKhoan.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var properties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };
        await HttpContext.SignInAsync("AdminCookie", new ClaimsPrincipal(identity), properties);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect(model.ReturnUrl);
        }

        return roleName is "Admin" or "CRM"
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Index", "Orders");
    }

    private static bool IsInternalRole(string? roleName) =>
        roleName is "Admin" or "Staff" or "NhanVien" or "CRM" or "QuanLy" or "Manager";

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("AdminCookie");
        return RedirectToAction(nameof(Login));
    }

    private static bool VerifyPassword(string password, string storedPassword)
    {
        if (storedPassword.StartsWith("$2", StringComparison.Ordinal))
        {
            try { return BCrypt.Net.BCrypt.Verify(password, storedPassword); }
            catch { return false; }
        }

        return string.Equals(password, storedPassword, StringComparison.Ordinal);
    }
}