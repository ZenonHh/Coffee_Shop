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

    public async Task<IActionResult> Index(int internalPage = 1)
    {
        const int pageSize = 10;
        var internalCount = await GetInternalAccounts().CountAsync();
        internalPage = Math.Clamp(internalPage, 1, Math.Max(1, (int)Math.Ceiling(internalCount / (double)pageSize)));
        ViewBag.Roles = await GetRolesAsync();
        var model = new AdminAccountIndexViewModel
        {
            InternalAccounts = await GetAccountsAsync(internalPage, pageSize),
            InternalAccountCount = internalCount
        };

        ViewBag.Pagination = new PaginationViewModel
        {
            PageNumber = internalPage,
            PageSize = pageSize,
            TotalCount = internalCount,
            PageParameter = "internalPage"
        };
        return View(model);
    }

    private async Task<List<InternalAccountListItemViewModel>> GetAccountsAsync(int page, int pageSize) => await GetInternalAccounts()
        .AsNoTracking()
        .OrderBy(account => account.MaVaiTro)
        .ThenBy(account => account.TenDangNhap)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
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
        if (!await IsInternalRoleAsync(model.MaVaiTro))
            ModelState.AddModelError(nameof(model.MaVaiTro), "Vai trò được chọn không tồn tại.");
        if (Encoding.UTF8.GetByteCount(model.MatKhau ?? string.Empty) > 72)
            ModelState.AddModelError(nameof(model.MatKhau), "Mật khẩu không được vượt quá 72 byte UTF-8.");

        if (await _context.Accounts.AnyAsync(account => account.TenDangNhap == model.TenDangNhap))
            ModelState.AddModelError(nameof(model.TenDangNhap), "Tên đăng nhập đã được sử dụng.");

        if (!ModelState.IsValid)
        {
            ViewBag.FormModel = model;
            ViewBag.OpenInternalModal = true;
            ViewBag.Roles = await GetRolesAsync();
            const int pageSize = 10;
            var internalCount = await GetInternalAccounts().CountAsync();
            var indexModel = new AdminAccountIndexViewModel
            {
                InternalAccounts = await GetAccountsAsync(1, pageSize),
                InternalAccountCount = internalCount
            };
            ViewBag.Pagination = new PaginationViewModel
            {
                PageNumber = 1,
                PageSize = pageSize,
                TotalCount = internalCount,
                PageParameter = "internalPage"
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
        var account = await _context.Accounts.Include(item => item.Role).SingleOrDefaultAsync(item => item.MaTaiKhoan == id);
        if (account == null) return NotFound();
        if (account.TenDangNhap.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Không thể đổi trạng thái tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index), new { tab = "internal" });
        }
        if (account.TrangThai && IsAdminRole(account.Role?.TenVaiTro)
            && !await HasOtherActiveAdminAsync(account.MaTaiKhoan))
        {
            TempData["Error"] = "Không thể khóa quản trị viên hoạt động cuối cùng.";
            return RedirectToAction(nameof(Index), new { tab = "internal" });
        }

        account.TrangThai = !account.TrangThai;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã {(account.TrangThai ? "mở khóa" : "khóa")} tài khoản {account.TenDangNhap}.";
        return RedirectToAction(nameof(Index), new { tab = "internal" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int id, int maVaiTro)
    {
        var account = await _context.Accounts.Include(item => item.Role)
            .SingleOrDefaultAsync(item => item.MaTaiKhoan == id);
        if (account == null) return NotFound();

        var targetRole = await _context.Roles.SingleOrDefaultAsync(role => role.MaVaiTro == maVaiTro);
        if (targetRole == null || !IsInternalRole(targetRole.TenVaiTro))
        {
            TempData["Error"] = "Vai trò được chọn không hợp lệ.";
            return RedirectToAction(nameof(Index), new { tab = "internal" });
        }
        if (account.TenDangNhap.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase)
            && account.MaVaiTro != maVaiTro)
        {
            TempData["Error"] = "Không thể tự thay đổi vai trò của tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index), new { tab = "internal" });
        }
        if (IsAdminRole(account.Role?.TenVaiTro)
            && !IsAdminRole(targetRole.TenVaiTro)
            && !await HasOtherActiveAdminAsync(account.MaTaiKhoan))
        {
            TempData["Error"] = "Không thể hạ quyền quản trị viên hoạt động cuối cùng.";
            return RedirectToAction(nameof(Index), new { tab = "internal" });
        }

        account.MaVaiTro = targetRole.MaVaiTro;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật vai trò cho tài khoản {account.TenDangNhap}.";
        return RedirectToAction(nameof(Index), new { tab = "internal" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, string newPassword)
    {
        var password = newPassword?.Trim() ?? string.Empty;
        if (password.Length < 8 || Encoding.UTF8.GetByteCount(password) > 72)
        {
            TempData["Error"] = "Mật khẩu mới phải có ít nhất 8 ký tự và tối đa 72 byte UTF-8.";
            return RedirectToAction(nameof(Index), new { tab = "internal" });
        }

        var account = await _context.Accounts.FindAsync(id);
        if (account == null) return NotFound();

        account.MatKhau = BCrypt.Net.BCrypt.HashPassword(password);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã đặt lại mật khẩu cho tài khoản {account.TenDangNhap}.";
        return RedirectToAction(nameof(Index), new { tab = "internal" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var account = await _context.Accounts.Include(item => item.Role)
            .SingleOrDefaultAsync(item => item.MaTaiKhoan == id);
        if (account == null) return NotFound();
        if (account.TenDangNhap.Equals(User.Identity?.Name, StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập.";
            return RedirectToAction(nameof(Index), new { tab = "internal" });
        }
        if (IsAdminRole(account.Role?.TenVaiTro)
            && !await HasOtherActiveAdminAsync(account.MaTaiKhoan))
        {
            TempData["Error"] = "Không thể xóa quản trị viên hoạt động cuối cùng.";
            return RedirectToAction(nameof(Index), new { tab = "internal" });
        }
        if (await _context.Surveys.AnyAsync(survey => survey.MaTaiKhoan == id)
            || await _context.Purchases.AnyAsync(purchase => purchase.NguoiDuyetId == id))
        {
            TempData["Error"] = "Không thể xóa tài khoản đã được ghi nhận trong khảo sát hoặc phiếu nhập.";
            return RedirectToAction(nameof(Index), new { tab = "internal" });
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã xóa tài khoản {account.TenDangNhap}.";
        return RedirectToAction(nameof(Index), new { tab = "internal" });
    }

    private async Task<List<RoleOptionViewModel>> GetRolesAsync() => await _context.Roles.AsNoTracking()
        .Where(role => role.TenVaiTro == "Admin"
            || role.TenVaiTro == "Staff"
            || role.TenVaiTro == "NhanVien"
            || role.TenVaiTro == "CRM"
            || role.TenVaiTro == "QuanLy"
            || role.TenVaiTro == "Manager")
        .OrderBy(role => role.MaVaiTro)
        .Select(role => new RoleOptionViewModel { MaVaiTro = role.MaVaiTro, TenVaiTro = role.TenVaiTro })
        .ToListAsync();

    private async Task<bool> IsInternalRoleAsync(int roleId)
    {
        var role = await _context.Roles.AsNoTracking().SingleOrDefaultAsync(item => item.MaVaiTro == roleId);
        return role != null && IsInternalRole(role.TenVaiTro);
    }

    private IQueryable<Account> GetInternalAccounts() => _context.Accounts
        .Where(account => account.Role != null
            && (account.Role.TenVaiTro == "Admin"
                || account.Role.TenVaiTro == "Staff"
                || account.Role.TenVaiTro == "NhanVien"
                || account.Role.TenVaiTro == "CRM"
                || account.Role.TenVaiTro == "QuanLy"
                || account.Role.TenVaiTro == "Manager"));

    private static bool IsInternalRole(string? roleName) =>
        roleName is "Admin" or "Staff" or "NhanVien" or "CRM" or "QuanLy" or "Manager";

    private static bool IsAdminRole(string? roleName) =>
        string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase);

    private Task<bool> HasOtherActiveAdminAsync(int accountId) => _context.Accounts
        .AnyAsync(account => account.MaTaiKhoan != accountId
            && account.TrangThai
            && account.Role != null
            && account.Role.TenVaiTro == "Admin");
}