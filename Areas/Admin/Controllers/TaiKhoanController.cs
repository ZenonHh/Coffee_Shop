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

    public async Task<IActionResult> Index(string tab = "internal", int internalPage = 1, int customerPage = 1)
    {
        const int pageSize = 10;
        var activeTab = tab.Equals("customer", StringComparison.OrdinalIgnoreCase) ? "customer" : "internal";
        var internalCount = await _context.Accounts.CountAsync(account =>
            account.MaVaiTro == 1 || account.MaVaiTro == 2 || account.MaVaiTro == 3 || account.MaVaiTro == 4);
        var customerCount = await _context.Customers.CountAsync();
        var activeCustomerCount = await _context.Customers.CountAsync(customer => customer.TrangThai);
        internalPage = Math.Clamp(internalPage, 1, Math.Max(1, (int)Math.Ceiling(internalCount / (double)pageSize)));
        customerPage = Math.Clamp(customerPage, 1, Math.Max(1, (int)Math.Ceiling(customerCount / (double)pageSize)));
        ViewBag.Roles = await GetRolesAsync();
        var model = new AdminAccountIndexViewModel
        {
            ActiveTab = activeTab,
            InternalAccounts = activeTab == "internal" ? await GetAccountsAsync(internalPage, pageSize) : new(),
            CustomerAccounts = activeTab == "customer" ? await GetCustomerAccountsAsync(customerPage, pageSize) : new(),
            InternalAccountCount = internalCount,
            CustomerAccountCount = customerCount,
            ActiveCustomerCount = activeCustomerCount,
            LockedCustomerCount = customerCount - activeCustomerCount
        };

        ViewBag.Pagination = activeTab == "internal"
            ? new PaginationViewModel { PageNumber = internalPage, PageSize = pageSize, TotalCount = internalCount, PageParameter = "internalPage" }
            : new PaginationViewModel { PageNumber = customerPage, PageSize = pageSize, TotalCount = customerCount, PageParameter = "customerPage" };
        return View(model);
    }

    [HttpGet]
    public IActionResult CreateCustomer()
    {
        return PartialView("_CustomerModal", new AdminCustomerEditViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCustomer(AdminCustomerEditViewModel model)
    {
        NormalizeCustomer(model);
        await ValidateCustomerAsync(model, null);
        if (!string.IsNullOrWhiteSpace(model.MatKhauMoi)
            && Encoding.UTF8.GetByteCount(model.MatKhauMoi) > 72)
        {
            ModelState.AddModelError(nameof(model.MatKhauMoi), "Mật khẩu không được vượt quá 72 byte UTF-8.");
        }
        if (!string.IsNullOrWhiteSpace(model.TaiKhoan) && string.IsNullOrWhiteSpace(model.MatKhauMoi))
        {
            ModelState.AddModelError(nameof(model.MatKhauMoi), "Vui lòng nhập mật khẩu khi tạo tài khoản đăng nhập.");
        }
        if (string.IsNullOrWhiteSpace(model.TaiKhoan) && !string.IsNullOrWhiteSpace(model.MatKhauMoi))
        {
            ModelState.AddModelError(nameof(model.TaiKhoan), "Vui lòng nhập tên đăng nhập để sử dụng mật khẩu.");
        }

        if (!ModelState.IsValid)
        {
            return PartialView("_CustomerModal", model);
        }

        var customer = new Customer
        {
            TaiKhoan = model.TaiKhoan,
            MatKhau = string.IsNullOrWhiteSpace(model.MatKhauMoi)
                ? null
                : BCrypt.Net.BCrypt.HashPassword(model.MatKhauMoi),
            HoTen = model.HoTen,
            Sdt = model.Sdt,
            Email = model.Email,
            DiemTichLuy = 0,
            HangTV = "Đồng",
            NgayTao = DateTime.Now,
            TrangThai = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã thêm khách hàng {customer.HoTen}.";
        return IsAjaxRequest() ? Json(new { success = true }) : RedirectToAction(nameof(Index), new { tab = "customer" });
    }

    [HttpGet]
    public async Task<IActionResult> EditCustomer(int id)
    {
        var model = await _context.Customers.AsNoTracking()
            .Where(customer => customer.MaKH == id)
            .Select(customer => new AdminCustomerEditViewModel
            {
                MaKH = customer.MaKH,
                TaiKhoan = customer.TaiKhoan,
                HoTen = customer.HoTen,
                Sdt = customer.Sdt,
                Email = customer.Email
            })
            .FirstOrDefaultAsync();
        return model == null ? NotFound() : PartialView("_CustomerModal", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCustomer(int id, AdminCustomerEditViewModel model)
    {
        if (id != model.MaKH) return BadRequest();
        NormalizeCustomer(model);
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        await ValidateCustomerAsync(model, id);
        if (!string.IsNullOrWhiteSpace(model.TaiKhoan)
            && string.IsNullOrWhiteSpace(customer.TaiKhoan)
            && string.IsNullOrWhiteSpace(model.MatKhauMoi))
        {
            ModelState.AddModelError(nameof(model.MatKhauMoi), "Vui lòng đặt mật khẩu khi thêm tài khoản đăng nhập.");
        }
        if (!string.IsNullOrWhiteSpace(model.MatKhauMoi)
            && Encoding.UTF8.GetByteCount(model.MatKhauMoi) > 72)
        {
            ModelState.AddModelError(nameof(model.MatKhauMoi), "Mật khẩu không được vượt quá 72 byte UTF-8.");
        }
        if (string.IsNullOrWhiteSpace(model.TaiKhoan) && !string.IsNullOrWhiteSpace(model.MatKhauMoi))
        {
            ModelState.AddModelError(nameof(model.TaiKhoan), "Vui lòng nhập tên đăng nhập để sử dụng mật khẩu.");
        }

        if (!ModelState.IsValid)
        {
            return PartialView("_CustomerModal", model);
        }

        customer.TaiKhoan = model.TaiKhoan;
        customer.HoTen = model.HoTen;
        customer.Sdt = model.Sdt;
        customer.Email = model.Email;
        if (!string.IsNullOrWhiteSpace(model.MatKhauMoi))
        {
            customer.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhauMoi);
        }
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật khách hàng {customer.HoTen}.";
        return IsAjaxRequest() ? Json(new { success = true }) : RedirectToAction(nameof(Index), new { tab = "customer" });
    }

    private async Task<List<InternalAccountListItemViewModel>> GetAccountsAsync(int page, int pageSize) => await _context.Accounts
        .AsNoTracking()
        .Where(account => account.MaVaiTro == 1 || account.MaVaiTro == 2 || account.MaVaiTro == 3 || account.MaVaiTro == 4)
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

    private async Task<List<CustomerAccountListItemViewModel>> GetCustomerAccountsAsync(int page, int pageSize) => await _context.Customers
        .AsNoTracking()
        .OrderByDescending(c => c.NgayTao)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
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
            ViewBag.OpenInternalModal = true;
            ViewBag.Roles = await GetRolesAsync();
            const int pageSize = 10;
            var internalCount = await _context.Accounts.CountAsync(account =>
                account.MaVaiTro == 1 || account.MaVaiTro == 2 || account.MaVaiTro == 3 || account.MaVaiTro == 4);
            var customerCount = await _context.Customers.CountAsync();
            var activeCustomerCount = await _context.Customers.CountAsync(customer => customer.TrangThai);
            var indexModel = new AdminAccountIndexViewModel
            {
                ActiveTab = "internal",
                InternalAccounts = await GetAccountsAsync(1, pageSize),
                InternalAccountCount = internalCount,
                CustomerAccountCount = customerCount,
                ActiveCustomerCount = activeCustomerCount,
                LockedCustomerCount = customerCount - activeCustomerCount
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

    private async Task ValidateCustomerAsync(AdminCustomerEditViewModel model, int? excludeCustomerId)
    {
        if (!string.IsNullOrWhiteSpace(model.TaiKhoan)
            && await CustomerExistsAsync(customer => customer.TaiKhoan == model.TaiKhoan, excludeCustomerId))
        {
            ModelState.AddModelError(nameof(model.TaiKhoan), "Tên đăng nhập đã được sử dụng.");
        }

        if (await CustomerExistsAsync(customer => customer.Sdt == model.Sdt, excludeCustomerId))
        {
            ModelState.AddModelError(nameof(model.Sdt), "Số điện thoại đã được sử dụng.");
        }
    }

    private Task<bool> CustomerExistsAsync(
        System.Linq.Expressions.Expression<Func<Customer, bool>> predicate,
        int? excludeCustomerId)
    {
        var query = _context.Customers.AsNoTracking().Where(predicate);
        if (excludeCustomerId.HasValue)
        {
            query = query.Where(customer => customer.MaKH != excludeCustomerId.Value);
        }
        return query.AnyAsync();
    }

    private static void NormalizeCustomer(AdminCustomerEditViewModel model)
    {
        model.TaiKhoan = string.IsNullOrWhiteSpace(model.TaiKhoan) ? null : model.TaiKhoan.Trim();
        model.HoTen = model.HoTen?.Trim() ?? string.Empty;
        model.Sdt = model.Sdt?.Trim() ?? string.Empty;
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
    }

    private bool IsAjaxRequest() =>
        string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

    private async Task<List<RoleOptionViewModel>> GetRolesAsync() => await _context.Roles.AsNoTracking()
        .Where(role => role.MaVaiTro == 2 || role.MaVaiTro == 3 || role.MaVaiTro == 4)
        .OrderBy(role => role.MaVaiTro)
        .Select(role => new RoleOptionViewModel { MaVaiTro = role.MaVaiTro, TenVaiTro = role.TenVaiTro })
        .ToListAsync();
}