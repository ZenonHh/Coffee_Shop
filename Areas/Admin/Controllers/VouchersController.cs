using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class VouchersController : Controller
{
    private readonly CoffeeShopDbContext _context;
    public VouchersController(CoffeeShopDbContext context) => _context = context;

    public async Task<IActionResult> Index(int? editId, int page = 1)
    {
        const int pageSize = 10;
        var totalCount = await _context.Vouchers.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize)));
        var vouchers = await GetVouchersAsync(page, pageSize);
        ViewBag.Pagination = new PaginationViewModel { PageNumber = page, PageSize = pageSize, TotalCount = totalCount };
        if (editId.HasValue)
            ViewBag.EditVoucher = await _context.Vouchers.AsNoTracking().FirstOrDefaultAsync(voucher => voucher.MaVoucher == editId.Value);
        return View(vouchers);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Voucher voucher)
    {
        Normalize(voucher);
        ValidateVoucher(voucher);
        if (await _context.Vouchers.AnyAsync(v => v.MaCode == voucher.MaCode)) ModelState.AddModelError(nameof(Voucher.MaCode), "Mã voucher đã tồn tại.");
        if (!ModelState.IsValid) return await InvalidFormAsync(voucher);
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
        if (voucher.SoLuongToiDa < existing.SoLuongDaDung)
            ModelState.AddModelError(nameof(Voucher.SoLuongToiDa), "Số lượt tối đa không thể thấp hơn số lượt đã dùng.");
        if (await _context.Vouchers.AnyAsync(v => v.MaCode == voucher.MaCode && v.MaVoucher != voucher.MaVoucher)) ModelState.AddModelError(nameof(Voucher.MaCode), "Mã voucher đã tồn tại.");
        if (!ModelState.IsValid) return await InvalidFormAsync(voucher);

        existing.MaCode = voucher.MaCode;
        existing.PhanLoaiGiam = voucher.PhanLoaiGiam;
        existing.GiaTriGiam = voucher.GiaTriGiam;
        existing.TenChuongTrinh = voucher.TenChuongTrinh;
        existing.NgayBatDau = voucher.NgayBatDau;
        existing.NgayKetThuc = voucher.NgayKetThuc;
        existing.DonToiThieu = voucher.DonToiThieu;
        existing.GiamToiDa = voucher.GiamToiDa;
        existing.SoLuongToiDa = voucher.SoLuongToiDa;
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
        if (voucher != null)
        {
            if (await _context.Orders.AnyAsync(order => order.MaVoucher == id))
                TempData["Error"] = "Voucher đã được dùng cho hóa đơn nên không thể xóa. Hãy tắt voucher thay thế.";
            else
            {
                _context.Vouchers.Remove(voucher);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã xóa voucher {voucher.MaCode}.";
            }
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> InvalidFormAsync(Voucher voucher)
    {
        ViewBag.EditVoucher = voucher.MaVoucher > 0 ? voucher : null;
        ViewBag.FormError = "Vui lòng kiểm tra lại thông tin voucher.";
        var totalCount = await _context.Vouchers.CountAsync();
        ViewBag.Pagination = new PaginationViewModel
        {
            PageNumber = 1,
            PageSize = 10,
            TotalCount = totalCount
        };
        return View("Index", await GetVouchersAsync(1, 10));
    }

    private Task<List<VoucherListItemViewModel>> GetVouchersAsync(int page, int pageSize) => _context.Vouchers
        .AsNoTracking()
        .OrderByDescending(voucher => voucher.MaVoucher)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(voucher => new VoucherListItemViewModel
        {
            MaVoucher = voucher.MaVoucher,
            MaCode = voucher.MaCode,
            PhanLoaiGiam = voucher.PhanLoaiGiam,
            GiaTriGiam = voucher.GiaTriGiam,
            TenChuongTrinh = voucher.TenChuongTrinh,
            NgayBatDau = voucher.NgayBatDau,
            NgayKetThuc = voucher.NgayKetThuc,
            DonToiThieu = voucher.DonToiThieu,
            GiamToiDa = voucher.GiamToiDa,
            SoLuongToiDa = voucher.SoLuongToiDa,
            SoLuongDaDung = voucher.SoLuongDaDung,
            TrangThai = voucher.TrangThai
        })
        .ToListAsync();

    private void ValidateVoucher(Voucher voucher)
    {
        if (voucher.PhanLoaiGiam == "PERCENT" && voucher.GiaTriGiam > 100) ModelState.AddModelError(nameof(Voucher.GiaTriGiam), "Phần trăm giảm không được vượt quá 100.");
        if (voucher.GiaTriGiam < 0) ModelState.AddModelError(nameof(Voucher.GiaTriGiam), "Giá trị giảm không được âm.");
        if (voucher.DonToiThieu < 0) ModelState.AddModelError(nameof(Voucher.DonToiThieu), "Đơn tối thiểu không được âm.");
        if (voucher.GiamToiDa < 0) ModelState.AddModelError(nameof(Voucher.GiamToiDa), "Mức giảm tối đa không được âm.");
        if (voucher.SoLuongToiDa < 1) ModelState.AddModelError(nameof(Voucher.SoLuongToiDa), "Số lượt tối đa phải ít nhất là 1.");
        if (voucher.SoLuongDaDung < 0 || voucher.SoLuongDaDung > voucher.SoLuongToiDa) ModelState.AddModelError(nameof(Voucher.SoLuongDaDung), "Số lượt đã dùng không hợp lệ.");
        if (voucher.NgayBatDau.HasValue && voucher.NgayKetThuc.HasValue && voucher.NgayBatDau >= voucher.NgayKetThuc) ModelState.AddModelError(nameof(Voucher.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");
    }

    private static void Normalize(Voucher voucher)
    {
        voucher.MaCode = (voucher.MaCode ?? string.Empty).Trim().ToUpperInvariant();
        voucher.PhanLoaiGiam = (voucher.PhanLoaiGiam ?? "PERCENT").Trim().ToUpperInvariant() == "AMOUNT" ? "AMOUNT" : "PERCENT";
        voucher.TenChuongTrinh = string.IsNullOrWhiteSpace(voucher.TenChuongTrinh) ? null : voucher.TenChuongTrinh.Trim();
    }
}