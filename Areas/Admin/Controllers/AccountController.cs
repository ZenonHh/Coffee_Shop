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
        if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("NhanVien") || User.IsInRole("Staff")))
        {
            return User.IsInRole("Admin")
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

        var isStaff = account != null && account.MaVaiTro is 1 or 2 or 3 or 4;
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
        var roleName = account.MaVaiTro is 1 or 4 ? "Admin" : account.MaVaiTro == 2 ? "Staff" : "NhanVien";
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

        return account.MaVaiTro == 4
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Index", "Orders");
    }

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