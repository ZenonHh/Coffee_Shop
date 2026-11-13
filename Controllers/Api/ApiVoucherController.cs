using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

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
            .Where(item => item.MaKH.HasValue
                ? item.MaKH == customerId
                : item.TenChuongTrinh == null || !item.TenChuongTrinh.StartsWith("Đổi điểm thưởng"))
            .OrderBy(item => item.NgayKetThuc)
            .Select(item => new VoucherListResponse(
                item.MaVoucher, item.MaCode, item.TenChuongTrinh, item.PhanLoaiGiam, item.GiaTriGiam,
                item.DonToiThieu, item.NgayKetThuc))
            .ToListAsync(cancellationToken);

        var userPoints = await db.Customers.AsNoTracking()
            .Where(customer => customer.MaKH == customerId)
            .Select(customer => (int?)customer.DiemTichLuy)
            .SingleOrDefaultAsync(cancellationToken);
        if (!userPoints.HasValue)
            return NotFound(new ProblemDetails { Status = 404, Title = "Không tìm thấy tài khoản khách hàng." });

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
            userPoints.Value, availableVouchers, usedVouchers,
            PointRedeemRewardCatalog.All.Select(reward => new ExchangeRewardResponse(
                reward.Id, reward.PointsRequired, reward.Title, reward.Description, reward.CodePrefix, reward.Icon))
                .ToArray()));
    }

    [HttpPost("redeem")]
    [ServiceFilter(typeof(ApiSessionAuthorizationFilter))]
    [ProducesResponseType(typeof(RedeemRewardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RedeemRewardResponse>> Redeem(
        RedeemRewardRequest request, CancellationToken cancellationToken)
    {
        var reward = PointRedeemRewardCatalog.All.FirstOrDefault(item => item.Id == request.RewardId);
        if (reward is null)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Ưu đãi đổi điểm không hợp lệ." });

        var customerId = (int)HttpContext.Items["MobileCustomerId"]!;
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        var customer = await db.Customers
            .SingleOrDefaultAsync(item => item.MaKH == customerId, cancellationToken);
        if (customer is null)
            return NotFound(new ProblemDetails { Status = 404, Title = "Không tìm thấy tài khoản khách hàng." });

        if (customer.DiemTichLuy < reward.PointsRequired)
            return BadRequest(new ProblemDetails
            {
                Status = 400,
                Title = "Chưa đủ điểm",
                Detail = $"Bạn hiện có {customer.DiemTichLuy} điểm, cần {reward.PointsRequired} điểm để đổi ưu đãi này."
            });

        customer.DiemTichLuy -= reward.PointsRequired;
        var now = DateTime.Now;
        var voucherCode = $"{reward.CodePrefix}-{Guid.NewGuid():N}".ToUpperInvariant();
        var voucher = new Voucher
        {
            MaCode = voucherCode,
            MaKH = customer.MaKH,
            TenChuongTrinh = $"Đổi điểm thưởng ({reward.Title})",
            PhanLoaiGiam = reward.RewardType,
            GiaTriGiam = reward.DiscountAmount,
            DonToiThieu = reward.MinOrder,
            GiamToiDa = reward.DiscountAmount,
            SoLuongToiDa = 1,
            SoLuongDaDung = 0,
            TrangThai = true,
            NgayBatDau = now,
            NgayKetThuc = now.AddDays(30)
        };
        db.Vouchers.Add(voucher);
        db.PointHistories.Add(new PointHistory
        {
            MaKH = customer.MaKH,
            DiemThayDoi = -reward.PointsRequired,
            LyDo = $"Đổi {reward.PointsRequired} điểm lấy mã voucher {voucherCode}",
            NgayThucHien = now
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new RedeemRewardResponse(
            customer.DiemTichLuy,
            new VoucherListResponse(
                voucher.MaVoucher, voucher.MaCode, voucher.TenChuongTrinh,
                voucher.PhanLoaiGiam, voucher.GiaTriGiam, voucher.DonToiThieu, voucher.NgayKetThuc)));
    }

    [HttpGet("cart-upsell")]
    [ProducesResponseType(typeof(CartUpsellHintDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<CartUpsellHintDto> CartUpsell([FromQuery] decimal currentTotal)
    {
        if (currentTotal < 0)
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Tổng giỏ hàng không hợp lệ",
                Detail = "Tổng giỏ hàng không thể nhỏ hơn 0."
            });

        var currentRule = OrderDiscountRuleCatalog.All
            .LastOrDefault(rule => currentTotal >= rule.MinOrderAmount);
        var nextRule = OrderDiscountRuleCatalog.All
            .FirstOrDefault(rule => currentTotal < rule.MinOrderAmount);
        if (nextRule is null)
        {
            var topRule = currentRule!;
            return Ok(new CartUpsellHintDto(
                true, 0, topRule.MinOrderAmount, topRule.BadgeText, 1,
                $"Tuyệt vời! Bạn đã đạt ưu đãi {topRule.Title}: {topRule.Description}"));
        }

        var amountNeeded = nextRule.MinOrderAmount - currentTotal;
        var previousThreshold = currentRule?.MinOrderAmount ?? 0m;
        var progress = Math.Clamp(
            (currentTotal - previousThreshold) / (nextRule.MinOrderAmount - previousThreshold), 0m, 1m);
        var currentReward = currentRule is null
            ? string.Empty
            : $"Bạn đã đạt ưu đãi {currentRule.Title}: {currentRule.Description} ";
        var message = $"{currentReward}Mua thêm {amountNeeded:N0} đ để đạt ưu đãi {nextRule.Title}: {nextRule.Description}";

        return Ok(new CartUpsellHintDto(
            true, amountNeeded, nextRule.MinOrderAmount, nextRule.BadgeText, progress, message));
    }
}
