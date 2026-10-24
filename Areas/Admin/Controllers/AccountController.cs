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
        if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("NhanVien")))
        {
            return RedirectToAction("Index", "Dashboard");
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
        var password = model.Password.Trim();
        var account = await _context.Accounts
            .Include(a => a.Role)
            .FirstOrDefaultAsync(a => a.TenDangNhap == username && a.TrangThai);

        var isStaff = account != null && (account.Role?.TenVaiTro == "Admin" || account.Role?.TenVaiTro == "NhanVien");
        var passwordValid = account != null && (account.MatKhau == password || password == account.MatKhau);
        if (!isStaff || !passwordValid)
        {
            ModelState.AddModelError(string.Empty, "Tài khoản hoặc mật khẩu quản trị không chính xác.");
            return View(model);
        }

        await HttpContext.SignOutAsync("AdminCookie");
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, account!.TenDangNhap),
            new Claim(ClaimTypes.GivenName, account.HoTen),
            new Claim(ClaimTypes.Role, account.Role!.TenVaiTro),
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

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("AdminCookie");
        return RedirectToAction(nameof(Login));
    }
}