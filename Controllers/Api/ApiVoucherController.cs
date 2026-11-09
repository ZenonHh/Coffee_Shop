using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;

namespace CoffeeShop.Controllers.Api;

[ApiController]
[Route("api/voucher")]
public sealed class ApiVoucherController(CoffeeShopDbContext db) : ControllerBase
{
    [HttpGet("classified")]
    [ServiceFilter(typeof(ApiSessionAuthorizationFilter))]
    [ProducesResponseType(typeof(VoucherClassifiedDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<VoucherClassifiedDto>> Classified(
        [FromQuery] int maKhachHang, CancellationToken cancellationToken)
    {
        if (!HttpContext.Items.TryGetValue("MobileCustomerId", out var authenticatedId)
            || authenticatedId is not int customerId
            || customerId != maKhachHang)
            return Forbid();

        var now = DateTime.Now;
        var activeVouchers = await db.Vouchers.AsNoTracking()
            .Where(item => item.TrangThai && item.SoLuongDaDung < item.SoLuongToiDa
                && (!item.NgayBatDau.HasValue || item.NgayBatDau <= now)
                && (!item.NgayKetThuc.HasValue || item.NgayKetThuc >= now))
            .OrderBy(item => item.NgayKetThuc)
            .Select(item => new VoucherListResponse(
                item.MaVoucher, item.MaCode, item.TenChuongTrinh, item.PhanLoaiGiam, item.GiaTriGiam,
                item.DonToiThieu, item.NgayKetThuc))
            .ToListAsync(cancellationToken);

        var completedOrderTotal = await db.Orders.AsNoTracking()
            .Where(order => order.MaKH == customerId && order.TrangThai == "Hoàn thành")
            .Select(order => order.TongTien)
            .SumAsync(cancellationToken);
        var userPoints = (int)(completedOrderTotal / 10_000m);

        var usedVouchers = await (
                from order in db.Orders.AsNoTracking()
                join voucher in db.Vouchers.AsNoTracking()
                    on order.MaVoucher equals (int?)voucher.MaVoucher
                where order.MaKH == customerId && order.MaVoucher.HasValue
                select new VoucherListResponse(
                    voucher.MaVoucher, voucher.MaCode, voucher.TenChuongTrinh, voucher.PhanLoaiGiam,
                    voucher.GiaTriGiam, voucher.DonToiThieu, voucher.NgayKetThuc))
            .Distinct()
            .ToListAsync(cancellationToken);

        var usedCodes = usedVouchers.Select(voucher => voucher.MaCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var availableVouchers = activeVouchers
            .Where(voucher => !usedCodes.Contains(voucher.MaCode))
            .ToList();

        return Ok(new VoucherClassifiedDto(
            userPoints, availableVouchers, usedVouchers, activeVouchers));
    }

    [HttpGet("cart-upsell")]
    [ProducesResponseType(typeof(CartUpsellHintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartUpsellHintDto>> CartUpsell(
        [FromQuery] decimal currentTotal, CancellationToken cancellationToken)
    {
        if (currentTotal < 0)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Tổng giỏ hàng không hợp lệ",
                Detail = "Tổng giỏ hàng không thể nhỏ hơn 0."
            });

        var now = DateTime.Now;
        var nextVoucher = await db.Vouchers.AsNoTracking()
            .Where(item => item.TrangThai && item.SoLuongDaDung < item.SoLuongToiDa
                && (!item.NgayBatDau.HasValue || item.NgayBatDau <= now)
                && (!item.NgayKetThuc.HasValue || item.NgayKetThuc >= now)
                && item.DonToiThieu > currentTotal)
            .OrderBy(item => item.DonToiThieu)
            .ThenByDescending(item => item.GiaTriGiam)
            .Select(item => new
            {
                item.DonToiThieu,
                item.GiaTriGiam,
                item.PhanLoaiGiam
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (nextVoucher is null)
            return Ok(new CartUpsellHintDto(false, 0, null, null, 0, null));

        var amountNeeded = nextVoucher.DonToiThieu - currentTotal;
        var discountText = string.Equals(
            nextVoucher.PhanLoaiGiam, "PERCENT", StringComparison.OrdinalIgnoreCase)
            ? $"{nextVoucher.GiaTriGiam:N0}%"
            : $"{nextVoucher.GiaTriGiam:N0} đ";
        var progress = Math.Clamp(currentTotal / nextVoucher.DonToiThieu, 0m, 1m);
        var message = $"Mua thêm {amountNeeded:N0} đ để được giảm {discountText}!";

        return Ok(new CartUpsellHintDto(
            true, amountNeeded, nextVoucher.DonToiThieu, discountText, progress, message));
    }
}
