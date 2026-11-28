using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Data.Entities;
using CoffeeShop.Business.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff,NhanVien")]
public class NhaCungCapController : Controller
{
    private readonly CoffeeShopDbContext _context;

    public NhaCungCapController(CoffeeShopDbContext context) => _context = context;

    public async Task<IActionResult> Index(
        string tab = "suppliers",
        int supplierPage = 1,
        int purchasePage = 1,
        string purchaseFilter = "all",
        int? purchaseYear = null,
        int? purchaseQuarter = null,
        int? purchaseMonths = null,
        DateTime? purchaseFromDate = null,
        DateTime? purchaseToDate = null,
        string purchaseSort = "newest")
    {
        const int pageSize = 10;
        var activeTab = tab?.ToLower() == "purchases" ? "purchases" : "suppliers";
        var supplierCount = await _context.Suppliers.CountAsync();
        var purchaseCount = await _context.Purchases.CountAsync();
        var purchaseYears = await _context.Purchases.AsNoTracking()
            .Where(p => p.NgayNhap.HasValue)
            .Select(p => p.NgayNhap!.Value.Year)
            .Distinct()
            .OrderByDescending(year => year)
            .ToListAsync();
        if (!purchaseYears.Contains(DateTime.Today.Year))
            purchaseYears.Insert(0, DateTime.Today.Year);

        purchaseFilter = purchaseFilter is "quarter" or "months" or "custom" ? purchaseFilter : "all";
        purchaseSort = purchaseSort == "oldest" ? "oldest" : "newest";
        var selectedYear = purchaseYear.HasValue && purchaseYears.Contains(purchaseYear.Value)
            ? purchaseYear.Value
            : DateTime.Today.Year;
        var selectedQuarter = purchaseQuarter is >= 1 and <= 4 ? purchaseQuarter.Value : 1;
        var selectedMonths = purchaseMonths is 3 or 6 or 9 ? purchaseMonths.Value : 3;
        var purchasesQuery = _context.Purchases.AsNoTracking();
        var filterSummary = "Tất cả thời gian";

        if (purchaseFilter == "quarter")
        {
            var quarterStart = new DateTime(selectedYear, (selectedQuarter - 1) * 3 + 1, 1);
            var quarterEnd = quarterStart.AddMonths(3);
            purchasesQuery = purchasesQuery.Where(p => p.NgayNhap >= quarterStart && p.NgayNhap < quarterEnd);
            filterSummary = $"Quý {selectedQuarter}/{selectedYear}";
        }
        else if (purchaseFilter == "months")
        {
            var now = DateTime.Now;
            var rangeStart = now.AddMonths(-selectedMonths);
            purchasesQuery = purchasesQuery.Where(p => p.NgayNhap >= rangeStart && p.NgayNhap <= now);
            filterSummary = $"{selectedMonths} tháng gần nhất ({rangeStart:dd/MM/yyyy} – {now:dd/MM/yyyy})";
        }
        else if (purchaseFilter == "custom")
        {
            if (!purchaseFromDate.HasValue || !purchaseToDate.HasValue || purchaseFromDate.Value.Date > purchaseToDate.Value.Date)
            {
                purchasesQuery = purchasesQuery.Where(_ => false);
                ViewBag.PurchaseFilterError = "Vui lòng chọn đầy đủ ngày và đảm bảo Từ ngày không sau Đến ngày.";
                filterSummary = "Khoảng ngày không hợp lệ";
            }
            else
            {
                var rangeStart = purchaseFromDate.Value.Date;
                var rangeEnd = purchaseToDate.Value.Date.AddDays(1);
                purchasesQuery = purchasesQuery.Where(p => p.NgayNhap >= rangeStart && p.NgayNhap < rangeEnd);
                filterSummary = $"{rangeStart:dd/MM/yyyy} – {purchaseToDate.Value:dd/MM/yyyy}";
            }
        }

        var filteredPurchaseCount = await purchasesQuery.CountAsync();
        var purchasePageCount = (int)Math.Ceiling(filteredPurchaseCount / (double)pageSize);
        supplierPage = Math.Clamp(supplierPage, 1, Math.Max(1, (int)Math.Ceiling(supplierCount / (double)pageSize)));
        purchasePage = Math.Clamp(purchasePage, 1, Math.Max(1, purchasePageCount));
        var model = new SupplierManagementViewModel
        {
            ActiveTab = activeTab,
            Suppliers = activeTab == "suppliers" ? await GetSuppliersAsync(supplierPage, pageSize) : new(),
            Purchases = activeTab == "purchases" ? await GetPurchasesAsync(purchasesQuery, purchasePage, pageSize, purchaseSort) : new(),
            SupplierCount = supplierCount,
            PurchaseCount = purchaseCount,
            FilteredPurchaseCount = filteredPurchaseCount,
            PurchaseFilter = purchaseFilter,
            PurchaseYear = selectedYear,
            PurchaseQuarter = selectedQuarter,
            PurchaseMonths = selectedMonths,
            PurchaseFromDate = purchaseFromDate,
            PurchaseToDate = purchaseToDate,
            PurchaseSort = purchaseSort,
            PurchaseFilterSummary = filterSummary,
            PurchaseYears = purchaseYears
        };
        ViewBag.Pagination = activeTab == "suppliers"
            ? new PaginationViewModel { PageNumber = supplierPage, PageSize = pageSize, TotalCount = supplierCount, PageParameter = "supplierPage" }
            : new PaginationViewModel { PageNumber = purchasePage, PageSize = pageSize, TotalCount = filteredPurchaseCount, PageParameter = "purchasePage" };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Required, StringLength(150)] string tenNCC,
        [StringLength(20)] string? sdt,
        [StringLength(255)] string? diaChi)
    {
        tenNCC = (tenNCC ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(tenNCC)) ModelState.AddModelError(nameof(tenNCC), "Vui lòng nhập tên nhà cung cấp.");
        if (tenNCC.Length > 150) ModelState.AddModelError(nameof(tenNCC), "Tên nhà cung cấp tối đa 150 ký tự.");
        if (!ModelState.IsValid)
        {
            ViewBag.FormError = "Vui lòng kiểm tra thông tin nhà cung cấp.";
            ViewBag.OpenSupplierModal = true;
            var model = new SupplierManagementViewModel
            {
                ActiveTab = "suppliers",
                Suppliers = await GetSuppliersAsync(1, 10),
                SupplierCount = await _context.Suppliers.CountAsync(),
                PurchaseCount = await _context.Purchases.CountAsync()
            };
            ViewBag.Pagination = new PaginationViewModel
            {
                PageNumber = 1,
                PageSize = 10,
                TotalCount = model.SupplierCount,
                PageParameter = "supplierPage"
            };
            return View("Index", model);
        }

        _context.Suppliers.Add(new Supplier
        {
            TenNCC = tenNCC,
            Sdt = string.IsNullOrWhiteSpace(sdt) ? null : sdt.Trim(),
            DiaChi = string.IsNullOrWhiteSpace(diaChi) ? null : diaChi.Trim(),
            TrangThai = true
        });
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã thêm nhà cung cấp {tenNCC}.";
        return RedirectToAction(nameof(Index), new { tab = "suppliers" });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        supplier.TrangThai = supplier.TrangThai != true;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã {(supplier.TrangThai == true ? "mở" : "ngừng")} nhà cung cấp {supplier.TenNCC}.";
        return RedirectToAction(nameof(Index), new { tab = "suppliers" });
    }

    private async Task<List<SupplierListItemViewModel>> GetSuppliersAsync(int page, int pageSize)
    {
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.TenNCC)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SupplierListItemViewModel
            {
                MaNCC = s.MaNCC,
                TenNCC = s.TenNCC,
                Sdt = s.Sdt,
                DiaChi = s.DiaChi,
                TrangThai = s.TrangThai,
                TotalPurchases = _context.Purchases.Count(p => p.MaNCC == s.MaNCC),
                TotalPurchasedAmount = _context.Purchases
                    .Where(p => p.MaNCC == s.MaNCC && p.TrangThai == "Đã duyệt")
                    .Sum(p => p.TongTien) ?? 0
            })
            .ToListAsync();

        return suppliers;
    }

    private async Task<List<PurchaseListItemViewModel>> GetPurchasesAsync(IQueryable<Purchase> query, int page, int pageSize, string sort)
    {
        var sortedQuery = sort == "oldest"
            ? query.OrderBy(p => p.NgayNhap).ThenBy(p => p.MaPN)
            : query.OrderByDescending(p => p.NgayNhap).ThenByDescending(p => p.MaPN);

        return await sortedQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PurchaseListItemViewModel
            {
                MaPN = p.MaPN,
                TenNCC = p.Supplier!.TenNCC,
                NguoiLap = p.NguoiLap,
                NgayNhap = p.NgayNhap,
                TongTien = p.TongTien,
                TrangThai = p.TrangThai
            })
            .ToListAsync();
    }
}