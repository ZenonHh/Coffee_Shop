using System.Text;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
[Authorize(Roles = "Admin,Staff,NhanVien")]
public class KhachHangController : Controller
{
    private readonly CoffeeShopDbContext _context;

    public KhachHangController(CoffeeShopDbContext context) => _context = context;

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        const int pageSize = 10;
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var query = _context.Customers.AsNoTracking();
        if (search != null)
        {
            query = query.Where(customer =>
                customer.HoTen.Contains(search)
                || customer.Sdt.Contains(search)
                || (customer.Email != null && customer.Email.Contains(search))
                || (customer.TaiKhoan != null && customer.TaiKhoan.Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
        var model = new CustomerManagementViewModel
        {
            CustomerAccountCount = await _context.Customers.CountAsync(),
            ActiveCustomerCount = await _context.Customers.CountAsync(customer => customer.TrangThai),
            LockedCustomerCount = await _context.Customers.CountAsync(customer => !customer.TrangThai),
            Search = search,
            Customers = await query.OrderByDescending(customer => customer.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(customer => new CustomerAccountListItemViewModel
                {
                    MaKH = customer.MaKH,
                    TaiKhoan = customer.TaiKhoan,
                    HoTen = customer.HoTen,
                    Sdt = customer.Sdt,
                    Email = customer.Email,
                    DiemTichLuy = customer.DiemTichLuy,
                    HangTV = customer.HangTV,
                    NgayTao = customer.NgayTao,
                    TrangThai = customer.TrangThai,
                    TotalOrders = customer.Orders.Count
                })
                .ToListAsync()
        };
        ViewBag.Pagination = new PaginationViewModel
        {
            PageNumber = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            PageParameter = "page"
        };
        ViewData["Title"] = "Khách hàng";
        return View(model);
    }

    [HttpGet("Create")]
    public IActionResult Create() => PartialView("_CustomerModal", new AdminCustomerEditViewModel());

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminCustomerEditViewModel model)
    {
        Normalize(model);
        await ValidateCustomerAsync(model, null);
        if (model.NgaySinh?.Date > DateTime.Today)
            ModelState.AddModelError(nameof(model.NgaySinh), "Ngày sinh không được ở tương lai.");
        if (!string.IsNullOrWhiteSpace(model.MatKhauMoi)
            && Encoding.UTF8.GetByteCount(model.MatKhauMoi) > 72)
            ModelState.AddModelError(nameof(model.MatKhauMoi), "Mật khẩu không được vượt quá 72 byte UTF-8.");
        if (!string.IsNullOrWhiteSpace(model.TaiKhoan) && string.IsNullOrWhiteSpace(model.MatKhauMoi))
            ModelState.AddModelError(nameof(model.MatKhauMoi), "Vui lòng nhập mật khẩu khi tạo tài khoản đăng nhập.");
        if (string.IsNullOrWhiteSpace(model.TaiKhoan) && !string.IsNullOrWhiteSpace(model.MatKhauMoi))
            ModelState.AddModelError(nameof(model.TaiKhoan), "Vui lòng nhập tên đăng nhập để sử dụng mật khẩu.");

        if (!ModelState.IsValid)
            return PartialView("_CustomerModal", model);

        var customer = new Customer
        {
            TaiKhoan = model.TaiKhoan,
            MatKhau = string.IsNullOrWhiteSpace(model.MatKhauMoi) ? null : BCrypt.Net.BCrypt.HashPassword(model.MatKhauMoi),
            HoTen = model.HoTen,
            Sdt = model.Sdt,
            Email = model.Email,
            NgaySinh = model.NgaySinh?.Date,
            DiemTichLuy = 0,
            HangTV = "Đồng",
            NgayTao = DateTime.Now,
            TrangThai = true
        };
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã thêm khách hàng {customer.HoTen}.";
        return IsAjaxRequest() ? Json(new { success = true }) : RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _context.Customers.AsNoTracking()
            .Where(customer => customer.MaKH == id)
            .Select(customer => new AdminCustomerEditViewModel
            {
                MaKH = customer.MaKH,
                TaiKhoan = customer.TaiKhoan,
                HoTen = customer.HoTen,
                Sdt = customer.Sdt,
                Email = customer.Email,
                NgaySinh = customer.NgaySinh
            })
            .FirstOrDefaultAsync();
        return model == null ? NotFound() : PartialView("_CustomerModal", model);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminCustomerEditViewModel model)
    {
        if (id != model.MaKH) return BadRequest();
        Normalize(model);
        if (model.NgaySinh?.Date > DateTime.Today)
            ModelState.AddModelError(nameof(model.NgaySinh), "Ngày sinh không được ở tương lai.");
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        await ValidateCustomerAsync(model, id);
        if (!string.IsNullOrWhiteSpace(model.TaiKhoan)
            && string.IsNullOrWhiteSpace(customer.TaiKhoan)
            && string.IsNullOrWhiteSpace(model.MatKhauMoi))
            ModelState.AddModelError(nameof(model.MatKhauMoi), "Vui lòng đặt mật khẩu khi thêm tài khoản đăng nhập.");
        if (!string.IsNullOrWhiteSpace(model.MatKhauMoi)
            && Encoding.UTF8.GetByteCount(model.MatKhauMoi) > 72)
            ModelState.AddModelError(nameof(model.MatKhauMoi), "Mật khẩu không được vượt quá 72 byte UTF-8.");
        if (string.IsNullOrWhiteSpace(model.TaiKhoan) && !string.IsNullOrWhiteSpace(model.MatKhauMoi))
            ModelState.AddModelError(nameof(model.TaiKhoan), "Vui lòng nhập tên đăng nhập để sử dụng mật khẩu.");
        if (!ModelState.IsValid)
            return PartialView("_CustomerModal", model);

        customer.TaiKhoan = model.TaiKhoan;
        customer.HoTen = model.HoTen;
        customer.Sdt = model.Sdt;
        customer.Email = model.Email;
        customer.NgaySinh = model.NgaySinh?.Date;
        if (!string.IsNullOrWhiteSpace(model.MatKhauMoi))
            customer.MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhauMoi);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật khách hàng {customer.HoTen}.";
        return IsAjaxRequest() ? Json(new { success = true }) : RedirectToAction(nameof(Index));
    }

    [HttpPost("ToggleStatus")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        customer.TrangThai = !customer.TrangThai;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã {(customer.TrangThai ? "mở khóa" : "khóa")} tài khoản khách hàng {customer.HoTen} ({customer.TaiKhoan ?? customer.Sdt}).";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("AdjustPoints")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdjustPoints(int id, int pointsChange, string? reason)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        customer.DiemTichLuy = Math.Max(0, customer.DiemTichLuy + pointsChange);
        customer.HangTV = customer.DiemTichLuy >= 500 ? "Kim Cương"
            : customer.DiemTichLuy >= 200 ? "Vàng"
            : customer.DiemTichLuy >= 50 ? "Bạc" : "Đồng";
        _context.PointHistories.Add(new PointHistory
        {
            MaKH = customer.MaKH,
            DiemThayDoi = pointsChange,
            LyDo = string.IsNullOrWhiteSpace(reason) ? "Quản trị viên điều chỉnh điểm" : reason.Trim(),
            NgayThucHien = DateTime.Now
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật điểm cho khách hàng {customer.HoTen}. Điểm hiện tại: {customer.DiemTichLuy}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateCustomerAsync(AdminCustomerEditViewModel model, int? excludeCustomerId)
    {
        if (!string.IsNullOrWhiteSpace(model.TaiKhoan)
            && await CustomerExistsAsync(customer => customer.TaiKhoan == model.TaiKhoan, excludeCustomerId))
            ModelState.AddModelError(nameof(model.TaiKhoan), "Tên đăng nhập đã được sử dụng.");
        if (await CustomerExistsAsync(customer => customer.Sdt == model.Sdt, excludeCustomerId))
            ModelState.AddModelError(nameof(model.Sdt), "Số điện thoại đã được sử dụng.");
    }

    private Task<bool> CustomerExistsAsync(Expression<Func<Customer, bool>> predicate, int? excludeCustomerId)
    {
        var query = _context.Customers.AsNoTracking().Where(predicate);
        if (excludeCustomerId.HasValue)
            query = query.Where(customer => customer.MaKH != excludeCustomerId.Value);
        return query.AnyAsync();
    }

    private static void Normalize(AdminCustomerEditViewModel model)
    {
        model.TaiKhoan = string.IsNullOrWhiteSpace(model.TaiKhoan) ? null : model.TaiKhoan.Trim();
        model.HoTen = model.HoTen?.Trim() ?? string.Empty;
        model.Sdt = model.Sdt?.Trim() ?? string.Empty;
        model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
    }

    private bool IsAjaxRequest() =>
        string.Equals(Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
}
