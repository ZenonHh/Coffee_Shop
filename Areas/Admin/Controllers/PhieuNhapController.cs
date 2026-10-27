using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff,NhanVien")]
public class PhieuNhapController : Controller
{
    private readonly CoffeeShopDbContext _context;

    public PhieuNhapController(CoffeeShopDbContext context) => _context = context;

    public IActionResult Index()
    {
        return RedirectToAction("Index", "NhaCungCap", new { tab = "purchases" });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateCreateListsAsync();
        return View(new PurchaseCreateViewModel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PurchaseCreateViewModel model)
    {
        model.Details = model.Details
            .Where(line => line.MaSP.HasValue || line.SoLuong.GetValueOrDefault() > 0 || line.DonGia.GetValueOrDefault() > 0)
            .ToList();
        if (model.Details.Count == 0)
            ModelState.AddModelError(nameof(model.Details), "Phiếu nhập cần ít nhất một dòng hàng.");

        if (!await _context.Suppliers.AsNoTracking().AnyAsync(supplier => supplier.MaNCC == model.MaNCC && supplier.TrangThai == true))
            ModelState.AddModelError(nameof(model.MaNCC), "Nhà cung cấp không tồn tại hoặc đã ngừng hoạt động.");

        var selectedProductIds = model.Details.Where(line => line.MaSP.HasValue).Select(line => line.MaSP!.Value).Distinct().ToList();
        var existingProductIds = await _context.Products.AsNoTracking()
            .Where(product => selectedProductIds.Contains(product.MaSP))
            .Select(product => product.MaSP)
            .ToListAsync();
        var existingProducts = existingProductIds.ToHashSet();
        foreach (var (line, index) in model.Details.Select((line, index) => (line, index)))
        {
            if (line.SoLuong.GetValueOrDefault() <= 0 || line.DonGia.GetValueOrDefault() <= 0)
                ModelState.AddModelError($"Details[{index}]", "Số lượng và đơn giá phải lớn hơn 0.");
            if (!line.MaSP.HasValue || !existingProducts.Contains(line.MaSP.Value))
                ModelState.AddModelError($"Details[{index}].MaSP", "Vui lòng chọn sản phẩm hợp lệ.");
            if (line.SoLuong.HasValue && line.SoLuong.Value != decimal.Truncate(line.SoLuong.Value))
                ModelState.AddModelError($"Details[{index}].SoLuong", "Số lượng sản phẩm phải là số nguyên.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateCreateListsAsync();
            return View(model);
        }

        var (employeeId, _) = await EnsureCurrentEmployeeAsync();
        var purchase = new Purchase
        {
            MaNCC = model.MaNCC,
            MaNV = employeeId,
            NgayNhap = DateTime.Now,
            NguoiLap = User.Identity?.Name ?? string.Empty,
            GhiChu = string.IsNullOrWhiteSpace(model.GhiChu) ? null : model.GhiChu.Trim(),
            TrangThai = "Chờ duyệt",
            Details = model.Details.Select(line => new PurchaseDetail
            {
                MaSP = line.MaSP!.Value,
                SoLuong = line.SoLuong!.Value,
                DonGia = line.DonGia!.Value,
                ThanhTien = line.SoLuong.Value * line.DonGia.Value
            }).ToList()
        };
        purchase.TongTien = purchase.Details.Sum(detail => detail.ThanhTien);
        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã tạo phiếu nhập #{purchase.MaPN} ở trạng thái chờ duyệt.";
        return RedirectToAction(nameof(Details), new { id = purchase.MaPN });
    }

    public async Task<IActionResult> Details(int id)
    {
        var purchase = await _context.Purchases
            .AsNoTracking()
            .Where(p => p.MaPN == id)
            .Select(p => new PurchaseDetailsViewModel
            {
                MaPN = p.MaPN,
                TenNCC = p.Supplier!.TenNCC,
                NguoiLap = p.NguoiLap,
                NguoiDuyet = p.NguoiDuyet,
                NgayNhap = p.NgayNhap,
                NgayDuyet = p.NgayDuyet,
                TongTien = p.TongTien,
                GhiChu = p.GhiChu,
                TrangThai = p.TrangThai,
                Details = p.Details.Select(detail => new PurchaseDetailsLineViewModel
                {
                    TenSanPham = detail.Product != null ? detail.Product.TenSP : detail.Ingredient!.TenNL,
                    SoLuong = detail.SoLuong,
                    DonGia = detail.DonGia,
                    ThanhTien = detail.ThanhTien
                }).ToList()
            })
            .FirstOrDefaultAsync();
        return purchase == null ? NotFound() : View(purchase);
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var purchase = await _context.Purchases.AsNoTracking()
            .Where(p => p.MaPN == id)
            .Select(p => new
            {
                p.TrangThai,
                ProductLines = p.Details.Where(detail => detail.MaSP.HasValue)
                    .Select(detail => new { ProductId = detail.MaSP!.Value, detail.SoLuong })
                    .ToList()
            })
            .FirstOrDefaultAsync();
        if (purchase == null) return NotFound();
        if (purchase.TrangThai != "Chờ duyệt")
        {
            TempData["Error"] = "Chỉ có thể duyệt phiếu đang chờ duyệt.";
            return RedirectToAction(nameof(Details), new { id });
        }

        foreach (var group in purchase.ProductLines.GroupBy(detail => detail.ProductId))
        {
            var quantity = checked((int)group.Sum(detail => detail.SoLuong));
            await _context.Products.Where(product => product.MaSP == group.Key)
                .ExecuteUpdateAsync(setters => setters.SetProperty(product => product.SoLuongTon, product => product.SoLuongTon + quantity));
        }

        if (!int.TryParse(User.FindFirstValue("AccountId"), out var approverId))
        {
            await transaction.RollbackAsync();
            return Forbid();
        }

        var approvedAt = DateTime.Now;
        var updated = await _context.Purchases
            .Where(p => p.MaPN == id && p.TrangThai == "Chờ duyệt")
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.TrangThai, "Đã duyệt")
                .SetProperty(p => p.NguoiDuyetId, approverId)
                .SetProperty(p => p.NguoiDuyet, User.Identity!.Name)
                .SetProperty(p => p.NgayDuyet, approvedAt));
        if (updated == 0)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Phiếu đã được xử lý bởi một người khác.";
            return RedirectToAction(nameof(Details), new { id });
        }
        await transaction.CommitAsync();
        TempData["Success"] = $"Đã duyệt phiếu nhập #{id} và cập nhật tồn kho.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancel(int id)
    {
        var updated = await _context.Purchases
            .Where(p => p.MaPN == id && p.TrangThai == "Chờ duyệt")
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.TrangThai, "Đã hủy"));
        if (updated == 0)
        {
            var exists = await _context.Purchases.AsNoTracking().AnyAsync(p => p.MaPN == id);
            if (!exists) return NotFound();
            TempData["Error"] = "Chỉ có thể hủy phiếu đang chờ duyệt.";
        }
        else
        {
            TempData["Success"] = $"Đã hủy phiếu nhập #{id}.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task PopulateCreateListsAsync()
    {
        ViewBag.Suppliers = await _context.Suppliers.AsNoTracking()
            .Where(supplier => supplier.TrangThai == true)
            .OrderBy(supplier => supplier.TenNCC)
            .Select(supplier => new SupplierOptionViewModel { MaNCC = supplier.MaNCC, TenNCC = supplier.TenNCC })
            .ToListAsync();
        ViewBag.Products = await _context.Products.AsNoTracking()
            .OrderBy(product => product.TenSP)
            .Select(product => new PurchaseProductOptionViewModel { MaSP = product.MaSP, TenSP = product.TenSP })
            .ToListAsync();

        var accountId = int.TryParse(User.FindFirstValue("AccountId"), out var id) ? id : 0;
        ViewBag.Creator = await _context.Accounts.AsNoTracking()
            .Where(account => account.MaTaiKhoan == accountId && account.TrangThai)
            .Select(account => new
            {
                account.HoTen,
                account.TenDangNhap,
                Role = account.MaVaiTro == 4 ? "Admin" : "Nhân viên"
            })
            .FirstOrDefaultAsync();
    }

    private async Task<(int EmployeeId, string RoleName)> EnsureCurrentEmployeeAsync()
    {
        if (!int.TryParse(User.FindFirstValue("AccountId"), out var accountId))
            throw new InvalidOperationException("Tài khoản nội bộ không hợp lệ.");

        var account = await _context.Accounts.AsNoTracking()
            .Where(value => value.MaTaiKhoan == accountId && value.TrangThai)
            .Select(value => new { value.HoTen, RoleName = value.Role!.TenVaiTro })
            .FirstOrDefaultAsync();
        if (account == null)
            throw new InvalidOperationException("Không tìm thấy tài khoản nội bộ đang hoạt động.");

        var fullName = account.HoTen.Trim();
        var separator = fullName.LastIndexOf(' ');
        var givenName = separator >= 0 ? fullName[(separator + 1)..] : "NV";
        var familyName = separator >= 0 ? fullName[..separator].Trim() : fullName;
        givenName = givenName.Length > 20 ? givenName[..20] : givenName;
        familyName = string.IsNullOrWhiteSpace(familyName) ? "Nhân viên" : familyName;
        familyName = familyName.Length > 50 ? familyName[..50] : familyName;

        var employee = await _context.StaffMembers.FirstOrDefaultAsync(value => value.HoVaDem == familyName && value.Ten == givenName);
        if (employee == null)
        {
            employee = new StaffMember
            {
                HoVaDem = familyName,
                Ten = givenName,
                ChucVu = account.RoleName
            };
            _context.StaffMembers.Add(employee);
            await _context.SaveChangesAsync();
        }

        return (employee.MaNV, account.RoleName);
    }
}