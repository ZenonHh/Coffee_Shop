using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TaiKhoanController : Controller
{
    private readonly CoffeeShopDbContext _context;

    public TaiKhoanController(CoffeeShopDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        ViewBag.Roles = await GetRolesAsync();
        var accounts = await GetAccountsAsync();
        return View(accounts);
    }

    private async Task<List<InternalAccountListItemViewModel>> GetAccountsAsync() => await _context.Accounts
            .AsNoTracking()
            .Where(account => account.MaVaiTro == 2 || account.MaVaiTro == 3 || account.MaVaiTro == 4)
            .OrderBy(account => account.MaVaiTro)
            .ThenBy(account => account.TenDangNhap)
            .Select(account => new InternalAccountListItemViewModel
            {
                MaTaiKhoan = account.MaTaiKhoan,
                TenDangNhap = account.TenDangNhap,
                HoTen = account.HoTen,
                TenVaiTro = account.Role!.TenVaiTro,
                TrangThai = account.TrangThai
            })
            .ToListAsync();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InternalAccountCreateViewModel model)
    {
        model.TenDangNhap = (model.TenDangNhap ?? string.Empty).Trim();
        model.HoTen = (model.HoTen ?? string.Empty).Trim();
        if (model.MaVaiTro is not (2 or 3 or 4)
            || !await _context.Roles.AnyAsync(role => role.MaVaiTro == model.MaVaiTro))
            ModelState.AddModelError(nameof(model.MaVaiTro), "Vai trò được chọn không tồn tại.");
        if (Encoding.UTF8.GetByteCount(model.MatKhau ?? string.Empty) > 72)
            ModelState.AddModelError(nameof(model.MatKhau), "Mật khẩu không được vượt quá 72 byte UTF-8.");

        if (await _context.Accounts.AnyAsync(account => account.TenDangNhap == model.TenDangNhap))
            ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã được sử dụng.");

        if (!ModelState.IsValid)
        {
            ViewBag.FormModel = model;
            ViewBag.Roles = await GetRolesAsync();
            return View("Index", await GetAccountsAsync());
        }

        _context.Accounts.Add(new Account
        {
            MaVaiTro = model.MaVaiTro,
            TenDangNhap = model.TenDangNhap,
            HoTen = model.HoTen,
            MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau),
            TrangThai = true
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã tạo tài khoản {model.TenDangNhap}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null) return NotFound();
        if (account.TenDangNhap == User.Identity?.Name)
        {
            TempData["Error"] = "Không thể khóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index));
        }

        account.TrangThai = !account.TrangThai;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã {(account.TrangThai ? "mở khóa" : "khóa")} tài khoản {account.TenDangNhap}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<RoleOptionViewModel>> GetRolesAsync() => await _context.Roles.AsNoTracking()
        .Where(role => role.MaVaiTro == 2 || role.MaVaiTro == 3 || role.MaVaiTro == 4)
        .OrderBy(role => role.MaVaiTro)
        .Select(role => new RoleOptionViewModel { MaVaiTro = role.MaVaiTro, TenVaiTro = role.TenVaiTro })
        .ToListAsync();
}