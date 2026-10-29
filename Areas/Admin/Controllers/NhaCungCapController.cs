using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Staff,NhanVien")]
public class NhaCungCapController : Controller
{
    private readonly CoffeeShopDbContext _context;

    public NhaCungCapController(CoffeeShopDbContext context) => _context = context;

    public async Task<IActionResult> Index(string tab = "suppliers", int supplierPage = 1, int purchasePage = 1)
    {
        const int pageSize = 10;
        var activeTab = tab?.ToLower() == "purchases" ? "purchases" : "suppliers";
        var supplierCount = await _context.Suppliers.CountAsync();
        var purchaseCount = await _context.Purchases.CountAsync();
        supplierPage = Math.Clamp(supplierPage, 1, Math.Max(1, (int)Math.Ceiling(supplierCount / (double)pageSize)));
        purchasePage = Math.Clamp(purchasePage, 1, Math.Max(1, (int)Math.Ceiling(purchaseCount / (double)pageSize)));
        var model = new SupplierManagementViewModel
        {
            ActiveTab = activeTab,
            Suppliers = activeTab == "suppliers" ? await GetSuppliersAsync(supplierPage, pageSize) : new(),
            Purchases = activeTab == "purchases" ? await GetPurchasesAsync(purchasePage, pageSize) : new(),
            SupplierCount = supplierCount,
            PurchaseCount = purchaseCount
        };
        ViewBag.Pagination = activeTab == "suppliers"
            ? new PaginationViewModel { PageNumber = supplierPage, PageSize = pageSize, TotalCount = supplierCount, PageParameter = "supplierPage" }
            : new PaginationViewModel { PageNumber = purchasePage, PageSize = pageSize, TotalCount = purchaseCount, PageParameter = "purchasePage" };
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

    private async Task<List<PurchaseListItemViewModel>> GetPurchasesAsync(int page, int pageSize)
    {
        return await _context.Purchases
            .AsNoTracking()
            .OrderByDescending(p => p.NgayNhap)
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