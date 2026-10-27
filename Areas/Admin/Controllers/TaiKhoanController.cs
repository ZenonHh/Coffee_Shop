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

    public async Task<IActionResult> Index(string tab = "internal")
    {
        ViewBag.Roles = await GetRolesAsync();
        var model = new AdminAccountIndexViewModel
        {
            ActiveTab = tab.Equals("customer", StringComparison.OrdinalIgnoreCase) ? "customer" : "internal",
            InternalAccounts = await GetAccountsAsync(),
            CustomerAccounts = await GetCustomerAccountsAsync()
        };

        return View(model);
    }

    private async Task<List<InternalAccountListItemViewModel>> GetAccountsAsync() => await _context.Accounts
        .AsNoTracking()
        .Where(account => account.MaVaiTro == 1 || account.MaVaiTro == 2 || account.MaVaiTro == 3 || account.MaVaiTro == 4)
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

    private async Task<List<CustomerAccountListItemViewModel>> GetCustomerAccountsAsync() => await _context.Customers
        .AsNoTracking()
        .OrderByDescending(c => c.NgayTao)
        .Select(c => new CustomerAccountListItemViewModel
        {
            MaKH = c.MaKH,
            TaiKhoan = c.TaiKhoan,
            HoTen = c.HoTen,
            Sdt = c.Sdt,
            Email = c.Email,
            DiemTichLuy = c.DiemTichLuy,
            HangTV = c.HangTV,
            NgayTao = c.NgayTao,
            TrangThai = c.TrangThai,
            TotalOrders = c.Orders.Count
        })
        .ToListAsync();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InternalAccountCreateViewModel model)
    {
        model.TenDangNhap = (model.TenDangNhap ?? string.Empty).Trim();
        model.HoTen = (model.HoTen ?? string.Empty).Trim();
        if (model.MaVaiTro is not (1 or 2 or 3 or 4)
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
            var indexModel = new AdminAccountIndexViewModel
            {
                ActiveTab = "internal",
                InternalAccounts = await GetAccountsAsync(),
                CustomerAccounts = await GetCustomerAccountsAsync()
            };
            return View("Index", indexModel);
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
        return RedirectToAction(nameof(Index), new { tab = "internal" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null) return NotFound();
        if (account.TenDangNhap == User.Identity?.Name)
        {
            TempData["Error"] = "Không thể khóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index), new { tab = "internal" });
        }

        account.TrangThai = !account.TrangThai;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã {(account.TrangThai ? "mở khóa" : "khóa")} tài khoản {account.TenDangNhap}.";
        return RedirectToAction(nameof(Index), new { tab = "internal" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCustomerStatus(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        customer.TrangThai = !customer.TrangThai;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã {(customer.TrangThai ? "mở khóa" : "khóa")} tài khoản khách hàng {customer.HoTen} ({customer.TaiKhoan ?? customer.Sdt}).";
        return RedirectToAction(nameof(Index), new { tab = "customer" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustCustomerPoints(int id, int pointsChange, string? reason)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        customer.DiemTichLuy = Math.Max(0, customer.DiemTichLuy + pointsChange);
        if (customer.DiemTichLuy >= 500) customer.HangTV = "Kim Cương";
        else if (customer.DiemTichLuy >= 200) customer.HangTV = "Vàng";
        else if (customer.DiemTichLuy >= 50) customer.HangTV = "Bạc";
        else customer.HangTV = "Đồng";

        _context.PointHistories.Add(new PointHistory
        {
            MaKH = customer.MaKH,
            DiemThayDoi = pointsChange,
            LyDo = string.IsNullOrWhiteSpace(reason) ? "Quản trị viên điều chỉnh điểm" : reason.Trim(),
            NgayThucHien = DateTime.Now
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật điểm cho khách hàng {customer.HoTen}. Điểm hiện tại: {customer.DiemTichLuy}.";
        return RedirectToAction(nameof(Index), new { tab = "customer" });
    }

    private async Task<List<RoleOptionViewModel>> GetRolesAsync() => await _context.Roles.AsNoTracking()
        .Where(role => role.MaVaiTro == 2 || role.MaVaiTro == 3 || role.MaVaiTro == 4)
        .OrderBy(role => role.MaVaiTro)
        .Select(role => new RoleOptionViewModel { MaVaiTro = role.MaVaiTro, TenVaiTro = role.TenVaiTro })
        .ToListAsync();
}