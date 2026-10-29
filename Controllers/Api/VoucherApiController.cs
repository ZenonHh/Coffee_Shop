using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;

namespace CoffeeShop.Controllers.Api;

[ApiController]
[Route("api/voucher")]
public sealed class VoucherApiController(CoffeeShopDbContext db) : ControllerBase
{
    /// <summary>List vouchers that are active within their validity period and still have uses remaining.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VoucherListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<VoucherListResponse>>> List(CancellationToken cancellationToken)
    {
        var now = DateTime.Now;
        var vouchers = await db.Vouchers.AsNoTracking()
            .Where(item => item.TrangThai && item.SoLuongDaDung < item.SoLuongToiDa
                && (!item.NgayBatDau.HasValue || item.NgayBatDau <= now)
                && (!item.NgayKetThuc.HasValue || item.NgayKetThuc >= now))
            .OrderBy(item => item.NgayKetThuc)
            .Select(item => new VoucherListResponse(
                item.MaVoucher, item.MaCode, item.PhanLoaiGiam, item.GiaTriGiam,
                item.DonToiThieu, item.NgayKetThuc))
            .ToListAsync(cancellationToken);
        return Ok(vouchers);
    }

    /// <summary>Validate a voucher against an order subtotal and calculate its discount.</summary>
    [HttpPost("check")]
    [ProducesResponseType(typeof(VoucherCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VoucherCheckResponse>> Check(
        VoucherCheckRequest request, CancellationToken cancellationToken)
    {
        var code = request.MaCode.Trim();
        var voucher = await db.Vouchers.AsNoTracking()
            .Where(item => item.MaCode == code)
            .Select(item => new
            {
                item.MaVoucher,
                item.MaCode,
                item.PhanLoaiGiam,
                item.GiaTriGiam,
                item.DonToiThieu,
                item.GiamToiDa,
                item.NgayBatDau,
                item.NgayKetThuc,
                item.SoLuongToiDa,
                item.SoLuongDaDung,
                item.TrangThai
            })
            .FirstOrDefaultAsync(cancellationToken);
        var now = DateTime.Now;
        if (voucher is null || !voucher.TrangThai || voucher.SoLuongDaDung >= voucher.SoLuongToiDa
            || (voucher.NgayBatDau.HasValue && now < voucher.NgayBatDau.Value)
            || (voucher.NgayKetThuc.HasValue && now > voucher.NgayKetThuc.Value)
            || request.TamTinh < voucher.DonToiThieu)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Voucher không hợp lệ", Detail = "Mã giảm giá không tồn tại, đã hết hạn hoặc đơn hàng chưa đạt giá trị tối thiểu." });

        var discount = string.Equals(voucher.PhanLoaiGiam, "PERCENT", StringComparison.OrdinalIgnoreCase)
            ? request.TamTinh * voucher.GiaTriGiam / 100m
            : voucher.GiaTriGiam;
        if (string.Equals(voucher.PhanLoaiGiam, "PERCENT", StringComparison.OrdinalIgnoreCase)
            && voucher.GiamToiDa.HasValue)
            discount = Math.Min(discount, voucher.GiamToiDa.Value);
        discount = Math.Min(Math.Max(0, discount), request.TamTinh);
        return Ok(new VoucherCheckResponse(voucher.MaVoucher, voucher.MaCode, discount));
    }
}

/// <summary>Voucher code and eligible order subtotal.</summary>
public sealed record VoucherCheckRequest
{
    [Required, StringLength(50)]
    public required string MaCode { get; init; }

    [Range(0, 100000000)]
    public decimal TamTinh { get; init; }
}

/// <summary>Voucher validation result and calculated discount.</summary>
public sealed record VoucherCheckResponse(int MaVoucher, string MaCode, decimal TienGiamGia);

/// <summary>Voucher available for display in the mobile app.</summary>
public sealed record VoucherListResponse(
    int MaVoucher, string MaCode, string PhanLoaiGiam, decimal GiaTriGiam,
    decimal DonToiThieu, DateTime? NgayKetThuc);
