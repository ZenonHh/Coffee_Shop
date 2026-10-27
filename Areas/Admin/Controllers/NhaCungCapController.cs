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

    public async Task<IActionResult> Index(string tab = "suppliers")
    {
        var model = new SupplierManagementViewModel
        {
            ActiveTab = tab?.ToLower() == "purchases" ? "purchases" : "suppliers",
            Suppliers = await GetSuppliersAsync(),
            Purchases = await GetPurchasesAsync()
        };
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
            var model = new SupplierManagementViewModel
            {
                ActiveTab = "suppliers",
                Suppliers = await GetSuppliersAsync(),
                Purchases = await GetPurchasesAsync()
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

    private async Task<List<SupplierListItemViewModel>> GetSuppliersAsync()
    {
        var suppliers = await _context.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.TenNCC)
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

    private async Task<List<PurchaseListItemViewModel>> GetPurchasesAsync()
    {
        return await _context.Purchases
            .AsNoTracking()
            .OrderByDescending(p => p.NgayNhap)
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