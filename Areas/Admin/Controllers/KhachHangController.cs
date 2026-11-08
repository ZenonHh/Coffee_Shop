using System.Text;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;
using CoffeeShop.Services;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Route("Admin/[controller]")]
[Authorize(Roles = "Admin,Staff,NhanVien,CRM")]
public class KhachHangController : Controller
{
    private readonly CoffeeShopDbContext _context;

    public KhachHangController(CoffeeShopDbContext context) => _context = context;

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        int page = 1,
        string? search = null,
        string status = "all",
        string? tier = null,
        string? ageGroup = null,
        string? interest = null)
    {
        const int pageSize = 10;
        search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        status = status is "active" or "locked" ? status : "all";
        tier = string.IsNullOrWhiteSpace(tier) ? null : tier.Trim();
        ageGroup = string.IsNullOrWhiteSpace(ageGroup) ? null : ageGroup.Trim();
        interest = string.IsNullOrWhiteSpace(interest) ? null : interest.Trim();
        var query = _context.Customers.AsNoTracking();
        if (search != null)
        {
            query = query.Where(customer =>
                customer.HoTen.Contains(search)
                || customer.Sdt.Contains(search)
                || (customer.Email != null && customer.Email.Contains(search))
                || (customer.TaiKhoan != null && customer.TaiKhoan.Contains(search)));
        }
        if (status == "active") query = query.Where(customer => customer.TrangThai);
        if (status == "locked") query = query.Where(customer => !customer.TrangThai);
        if (tier != null) query = query.Where(customer => customer.HangTV == tier);
        if (ageGroup != null)
        {
            var today = DateTime.Today;
            query = ageGroup switch
            {
                "under18" => query.Where(customer => customer.NgaySinh.HasValue
                    && customer.NgaySinh > today.AddYears(-18) && customer.NgaySinh <= today),
                "18-24" => query.Where(customer => customer.NgaySinh <= today.AddYears(-18)
                    && customer.NgaySinh > today.AddYears(-25)),
                "25-34" => query.Where(customer => customer.NgaySinh <= today.AddYears(-25)
                    && customer.NgaySinh > today.AddYears(-35)),
                "35-44" => query.Where(customer => customer.NgaySinh <= today.AddYears(-35)
                    && customer.NgaySinh > today.AddYears(-45)),
                "45plus" => query.Where(customer => customer.NgaySinh <= today.AddYears(-45)),
                "missing" => query.Where(customer => !customer.NgaySinh.HasValue || customer.NgaySinh > today),
                _ => query
            };
        }
        if (interest != null)
        {
            var customersWithInterest = _context.OrderDetails.AsNoTracking()
                .Where(detail => detail.Order != null
                    && detail.Order.MaKH.HasValue
                    && (detail.Order.TrangThai == OrderStatuses.Completed
                        || detail.Order.TrangThai == OrderStatuses.LegacyCompleted)
                    && detail.Product != null
                    && detail.Product.Category != null
                    && detail.Product.Category.TenLoaiSP == interest)
                .Select(detail => detail.Order!.MaKH!.Value)
                .Distinct();
            query = query.Where(customer => customersWithInterest.Contains(customer.MaKH));
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
            Status = status,
            Tier = tier,
            AgeGroup = ageGroup,
            Interest = interest,
            InterestOptions = await _context.Categories.AsNoTracking()
                .OrderBy(category => category.TenLoaiSP)
                .Select(category => category.TenLoaiSP)
                .Distinct()
                .ToListAsync(),
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

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var customer = await _context.Customers.AsNoTracking()
            .Where(item => item.MaKH == id)
            .Select(item => new CustomerDetailsViewModel
            {
                MaKH = item.MaKH,
                HoTen = item.HoTen,
                TaiKhoan = item.TaiKhoan,
                Sdt = item.Sdt,
                Email = item.Email,
                NgaySinh = item.NgaySinh,
                HangTV = item.HangTV,
                DiemTichLuy = item.DiemTichLuy
            })
            .FirstOrDefaultAsync();
        if (customer == null) return NotFound();

        customer.Interests = await _context.OrderDetails.AsNoTracking()
            .Where(detail => detail.Order != null
                && detail.Order.MaKH == id
                && (detail.Order.TrangThai == OrderStatuses.Completed
                    || detail.Order.TrangThai == OrderStatuses.LegacyCompleted)
                && detail.Product != null
                && detail.Product.Category != null)
            .Select(detail => detail.Product!.Category!.TenLoaiSP)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync();
        customer.Orders = await _context.Orders.AsNoTracking()
            .Where(order => order.MaKH == id)
            .OrderByDescending(order => order.NgayTao)
            .Take(8)
            .Select(order => new CustomerDetailsOrderViewModel
            {
                MaHD = order.MaHD,
                NgayTao = order.NgayTao,
                TongTien = order.TongTien,
                TrangThai = order.TrangThai
            })
            .ToListAsync();
        customer.Feedbacks = await _context.Feedbacks.AsNoTracking()
            .Where(feedback => feedback.MaKH == id)
            .OrderByDescending(feedback => feedback.NgayGui)
            .Take(8)
            .Select(feedback => new CustomerDetailsFeedbackViewModel
            {
                ProductName = feedback.Product == null ? null : feedback.Product.TenSP,
                Rating = feedback.DanhGia,
                Content = feedback.NoiDung,
                AdminReply = feedback.PhanHoiQuanTri,
                CreatedAt = feedback.NgayGui
            })
            .ToListAsync();
        customer.Surveys = await _context.SurveyResponses.AsNoTracking()
            .Where(response => response.MaKH == id)
            .OrderByDescending(response => response.NgayHoanThanh)
            .Take(8)
            .Select(response => new CustomerDetailsSurveyViewModel
            {
                Title = response.Survey == null ? "Khảo sát" : response.Survey.TieuDe,
                CompletedAt = response.NgayHoanThanh
            })
            .ToListAsync();
        return PartialView("_CustomerDetails", customer);
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

    [HttpPost("Delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        var hasHistory = await _context.Orders.AnyAsync(order => order.MaKH == id)
            || await _context.Feedbacks.AnyAsync(feedback => feedback.MaKH == id)
            || await _context.SurveyResponses.AnyAsync(response => response.MaKH == id)
            || await _context.SurveyAudienceCustomers.AnyAsync(target => target.MaKH == id)
            || await _context.PointHistories.AnyAsync(history => history.MaKH == id);
        if (hasHistory)
        {
            TempData["Error"] = "Không thể xóa khách hàng đã có đơn hàng, phản hồi hoặc lịch sử khảo sát. Bạn có thể khóa tài khoản thay thế.";
            return RedirectToAction(nameof(Index));
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã xóa khách hàng {customer.HoTen}.";
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
