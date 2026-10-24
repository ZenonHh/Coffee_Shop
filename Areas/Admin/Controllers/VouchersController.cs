using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,NhanVien")]
public class VouchersController : Controller
{
    private readonly CoffeeShopDbContext _context;
    public VouchersController(CoffeeShopDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? editId)
    {
        var vouchers = await _context.Vouchers.OrderByDescending(v => v.MaVoucher).ToListAsync();
        if (editId.HasValue) ViewBag.EditVoucher = vouchers.FirstOrDefault(v => v.MaVoucher == editId.Value);
        return View(vouchers);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Voucher voucher)
    {
        Normalize(voucher);
        ValidateVoucher(voucher);
        if (await _context.Vouchers.AnyAsync(v => v.MaCode == voucher.MaCode)) ModelState.AddModelError(nameof(Voucher.MaCode), "Mã voucher đã tồn tại.");
        if (!ModelState.IsValid) return InvalidForm(voucher);
        _context.Vouchers.Add(voucher);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã tạo voucher {voucher.MaCode}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Voucher voucher)
    {
        Normalize(voucher);
        var existing = await _context.Vouchers.FindAsync(voucher.MaVoucher);
        if (existing == null) return NotFound();
        ValidateVoucher(voucher);
        if (await _context.Vouchers.AnyAsync(v => v.MaCode == voucher.MaCode && v.MaVoucher != voucher.MaVoucher)) ModelState.AddModelError(nameof(Voucher.MaCode), "Mã voucher đã tồn tại.");
        if (!ModelState.IsValid) return InvalidForm(voucher);

        existing.MaCode = voucher.MaCode;
        existing.PhanLoaiGiam = voucher.PhanLoaiGiam;
        existing.GiaTriGiam = voucher.GiaTriGiam;
        existing.TenChuongTrinh = voucher.TenChuongTrinh;
        existing.NgayBatDau = voucher.NgayBatDau;
        existing.NgayKetThuc = voucher.NgayKetThuc;
        existing.DonToiThieu = voucher.DonToiThieu;
        existing.TrangThai = voucher.TrangThai;
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Đã cập nhật voucher {voucher.MaCode}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var voucher = await _context.Vouchers.FindAsync(id);
        if (voucher != null) { voucher.TrangThai = !voucher.TrangThai; await _context.SaveChangesAsync(); TempData["Success"] = $"Đã {(voucher.TrangThai ? "bật" : "tắt")} voucher {voucher.MaCode}."; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var voucher = await _context.Vouchers.FindAsync(id);
        if (voucher != null) { _context.Vouchers.Remove(voucher); await _context.SaveChangesAsync(); TempData["Success"] = $"Đã xóa voucher {voucher.MaCode}."; }
        return RedirectToAction(nameof(Index));
    }

    private IActionResult InvalidForm(Voucher voucher)
    {
        ViewBag.EditVoucher = voucher.MaVoucher > 0 ? voucher : null;
        ViewBag.FormError = "Vui lòng kiểm tra lại thông tin voucher.";
        return View("Index", _context.Vouchers.OrderByDescending(v => v.MaVoucher).ToList());
    }

    private void ValidateVoucher(Voucher voucher)
    {
        if (voucher.PhanLoaiGiam == "PERCENT" && voucher.GiaTriGiam > 100) ModelState.AddModelError(nameof(Voucher.GiaTriGiam), "Phần trăm giảm không được vượt quá 100.");
        if (voucher.GiaTriGiam < 0) ModelState.AddModelError(nameof(Voucher.GiaTriGiam), "Giá trị giảm không được âm.");
        if (voucher.DonToiThieu < 0) ModelState.AddModelError(nameof(Voucher.DonToiThieu), "Đơn tối thiểu không được âm.");
        if (voucher.NgayBatDau.HasValue && voucher.NgayKetThuc.HasValue && voucher.NgayBatDau >= voucher.NgayKetThuc) ModelState.AddModelError(nameof(Voucher.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");
    }

    private static void Normalize(Voucher voucher)
    {
        voucher.MaCode = (voucher.MaCode ?? string.Empty).Trim().ToUpperInvariant();
        voucher.PhanLoaiGiam = (voucher.PhanLoaiGiam ?? "PERCENT").Trim().ToUpperInvariant() == "AMOUNT" ? "AMOUNT" : "PERCENT";
        voucher.TenChuongTrinh = string.IsNullOrWhiteSpace(voucher.TenChuongTrinh) ? null : voucher.TenChuongTrinh.Trim();
    }
}