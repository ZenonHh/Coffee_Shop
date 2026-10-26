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

    public async Task<IActionResult> Index() => View(await GetSuppliersAsync());

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
            return View("Index", await GetSuppliersAsync());
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
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null) return NotFound();
        supplier.TrangThai = supplier.TrangThai != true;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã {(supplier.TrangThai == true ? "mở" : "ngừng")} nhà cung cấp {supplier.TenNCC}.";
        return RedirectToAction(nameof(Index));
    }

    private Task<List<SupplierListItemViewModel>> GetSuppliersAsync() => _context.Suppliers
        .AsNoTracking()
        .OrderBy(supplier => supplier.TenNCC)
        .Select(supplier => new SupplierListItemViewModel
        {
            MaNCC = supplier.MaNCC,
            TenNCC = supplier.TenNCC,
            Sdt = supplier.Sdt,
            DiaChi = supplier.DiaChi,
            TrangThai = supplier.TrangThai
        })
        .ToListAsync();
}