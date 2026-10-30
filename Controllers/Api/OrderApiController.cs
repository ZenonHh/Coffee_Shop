using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Services;

namespace CoffeeShop.Controllers.Api;

[ApiController]
[Route("api/order")]
[ServiceFilter(typeof(ApiSessionAuthorizationFilter))]
public sealed class OrderApiController(CoffeeShopDbContext db) : ControllerBase
{
    /// <summary>Create an order and reserve product and topping inventory atomically.</summary>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(CheckoutResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CheckoutResponse>> Checkout(
        CheckoutRequest request, CancellationToken cancellationToken)
    {
        var customerId = (int)HttpContext.Items["MobileCustomerId"]!;
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        var customer = await db.Customers.AsNoTracking()
            .Where(item => item.MaKH == customerId && item.TrangThai)
            .Select(item => new CustomerOrderInfo(
                item.MaKH, item.TaiKhoan, item.HoTen, item.Sdt, item.Email, item.DiaChi,
                item.DiemTichLuy, item.HangTV))
            .FirstOrDefaultAsync(cancellationToken);
        if (customer is null)
            return Unauthorized();
        if (request.Items.Count == 0)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Giỏ hàng trống" });

        var isDelivery = string.Equals(request.LoaiDonHang, "Giao hàng", StringComparison.OrdinalIgnoreCase);
        var isPickup = string.Equals(request.LoaiDonHang, "Mang đi", StringComparison.OrdinalIgnoreCase);
        if (!isDelivery && !isPickup)
            return BadRequest(new ProblemDetails { Status = 400, Title = "Hình thức nhận hàng không hợp lệ" });
        var recipientName = string.IsNullOrWhiteSpace(request.HoTenNguoiNhan)
            ? customer.HoTen
            : request.HoTenNguoiNhan.Trim();
        var recipientPhone = string.IsNullOrWhiteSpace(request.SdtNguoiNhan)
            ? customer.Sdt
            : request.SdtNguoiNhan.Trim();
        var recipientAddress = string.IsNullOrWhiteSpace(request.DiaChiGiao)
            ? customer.DiaChi
            : request.DiaChiGiao.Trim();
        var recipientEmail = string.IsNullOrWhiteSpace(request.EmailNguoiNhan)
            ? null
            : request.EmailNguoiNhan.Trim();
        if (string.IsNullOrWhiteSpace(recipientName))
            return BadRequest(new ProblemDetails { Status = 400, Title = "Vui lòng cung cấp họ tên người nhận" });
        if (recipientPhone.Length != 10 || recipientPhone.Any(character => !char.IsAsciiDigit(character)))
            return BadRequest(new ProblemDetails { Status = 400, Title = "Số điện thoại phải gồm đúng 10 chữ số" });
        if (isDelivery && string.IsNullOrWhiteSpace(recipientAddress))
            return BadRequest(new ProblemDetails { Status = 400, Title = "Vui lòng cung cấp địa chỉ giao hàng" });
        if (request.PhuongThucThanhToan is not ("COD" or "Transfer"))
            return BadRequest(new ProblemDetails { Status = 400, Title = "Phương thức thanh toán không hợp lệ" });

        var preparedItems = new List<PreparedOrderItem>(request.Items.Count);
        foreach (var item in request.Items)
        {
            if (item.SoLuong is < 1 or > 50 || item.ToppingIds.Distinct().Count() != item.ToppingIds.Count)
                return BadRequest(new ProblemDetails { Status = 400, Title = "Thông tin món hoặc số lượng không hợp lệ" });

            var product = await db.Products.AsNoTracking()
                .Where(product => product.MaSP == item.MaSP && product.TrangThai && product.MaLoaiSP != 9)
                .Select(product => new { product.MaSP, product.TenSP })
                .FirstOrDefaultAsync(cancellationToken);
            var variant = await db.ProductVariants.AsNoTracking()
                .Where(variant => variant.MaBienThe == item.MaBienThe && variant.MaSP == item.MaSP && variant.TrangThai)
                .Select(variant => new { variant.MaBienThe, variant.GiaBan, variant.KichCo })
                .FirstOrDefaultAsync(cancellationToken);
            if (product is null || variant is null)
                return BadRequest(new ProblemDetails { Status = 400, Title = "Một món hoặc kích cỡ không còn khả dụng", Detail = $"Mã món {item.MaSP}." });

            var toppings = await db.Products.AsNoTracking()
                .Where(topping => item.ToppingIds.Contains(topping.MaSP)
                    && topping.TrangThai && topping.MaLoaiSP == 9)
                .Select(topping => new
                {
                    topping.MaSP,
                    topping.TenSP,
                    topping.SoLuongTon,
                    Price = topping.Variants.Where(option => option.TrangThai)
                        .OrderBy(option => option.MaBienThe)
                        .Select(option => (decimal?)option.GiaBan)
                        .FirstOrDefault() ?? 10000m
                })
                .ToListAsync(cancellationToken);
            if (toppings.Count != item.ToppingIds.Count || toppings.Any(topping => topping.SoLuongTon < item.SoLuong))
                return Conflict(new ProblemDetails { Status = 409, Title = "Topping đã chọn không còn đủ hàng" });

            preparedItems.Add(new PreparedOrderItem(
                item, variant.GiaBan,
                toppings.Select(topping => new PreparedTopping(topping.MaSP, topping.TenSP, topping.Price)).ToList()));
        }

        var subtotal = preparedItems.Sum(item =>
            (item.UnitPrice + item.Toppings.Sum(topping => topping.Price)) * item.Request.SoLuong);
        var shippingFee = isDelivery ? 15000 : 0;
        var voucherId = (int?)null;
        var discount = 0m;
        if (!string.IsNullOrWhiteSpace(request.MaCode))
        {
            var code = request.MaCode.Trim();
            var voucher = await db.Vouchers.AsNoTracking()
                .Where(item => item.MaCode == code)
                .Select(item => new
                {
                    item.MaVoucher, item.PhanLoaiGiam, item.GiaTriGiam, item.DonToiThieu,
                    item.GiamToiDa, item.NgayBatDau, item.NgayKetThuc, item.SoLuongDaDung,
                    item.SoLuongToiDa, item.TrangThai
                })
                .FirstOrDefaultAsync(cancellationToken);
            var now = DateTime.Now;
            if (voucher is null || !voucher.TrangThai || voucher.SoLuongDaDung >= voucher.SoLuongToiDa
                || (voucher.NgayBatDau.HasValue && now < voucher.NgayBatDau.Value)
                || (voucher.NgayKetThuc.HasValue && now > voucher.NgayKetThuc.Value)
                || subtotal < voucher.DonToiThieu)
                return BadRequest(new ProblemDetails { Status = 400, Title = "Voucher không hợp lệ" });

            var isPercent = string.Equals(voucher.PhanLoaiGiam, "PERCENT", StringComparison.OrdinalIgnoreCase);
            discount = isPercent ? subtotal * voucher.GiaTriGiam / 100m : voucher.GiaTriGiam;
            if (isPercent && voucher.GiamToiDa.HasValue)
                discount = Math.Min(discount, voucher.GiamToiDa.Value);
            discount = Math.Min(Math.Max(0, discount), subtotal);
            voucherId = voucher.MaVoucher;

            var reserved = await db.Vouchers
                .Where(item => item.MaVoucher == voucher.MaVoucher && item.TrangThai
                    && item.SoLuongDaDung < item.SoLuongToiDa
                    && (!item.NgayBatDau.HasValue || item.NgayBatDau <= now)
                    && (!item.NgayKetThuc.HasValue || item.NgayKetThuc >= now))
                .ExecuteUpdateAsync(update => update
                    .SetProperty(item => item.SoLuongDaDung, item => item.SoLuongDaDung + 1)
                    .SetProperty(item => item.TrangThai, item => item.SoLuongDaDung + 1 >= item.SoLuongToiDa ? false : item.TrangThai),
                    cancellationToken);
            if (reserved != 1)
                return Conflict(new ProblemDetails { Status = 409, Title = "Voucher vừa hết lượt sử dụng" });
        }

        var stockRequirements = new Dictionary<int, int>();
        foreach (var item in preparedItems)
        {
            stockRequirements[item.Request.MaSP] = stockRequirements.GetValueOrDefault(item.Request.MaSP) + item.Request.SoLuong;
            foreach (var topping in item.Toppings)
                stockRequirements[topping.Id] = stockRequirements.GetValueOrDefault(topping.Id) + item.Request.SoLuong;
        }
        foreach (var (productId, quantity) in stockRequirements)
        {
            var affected = await db.Products
                .Where(product => product.MaSP == productId && product.TrangThai && product.SoLuongTon >= quantity)
                .ExecuteUpdateAsync(update => update.SetProperty(product => product.SoLuongTon, product => product.SoLuongTon - quantity), cancellationToken);
            if (affected != 1)
                return Conflict(new ProblemDetails { Status = 409, Title = "Không đủ tồn kho", Detail = $"Mã sản phẩm {productId}." });
        }

        var order = new Order
        {
            MaKH = customer.MaKH,
            MaVoucher = voucherId,
            LoaiDonHang = isDelivery ? "Giao hàng" : "Mang đi",
            TamTinh = decimal.ToInt32(decimal.Round(subtotal, 0)),
            TienGiamGia = discount,
            PhiGiao = shippingFee,
            TongTien = Math.Max(0, subtotal - discount + shippingFee),
            TrangThai = OrderStatuses.WaitingForConfirmation,
            PhuongThucThanhToan = request.PhuongThucThanhToan,
            HoTenNguoiNhan = recipientName,
            SdtNguoiNhan = recipientPhone,
            DiaChiGiao = isDelivery ? recipientAddress : null,
            GhiChu = request.GhiChu?.Trim(),
            NgayTao = DateTime.Now
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        foreach (var prepared in preparedItems)
        {
            var toppingTotal = prepared.Toppings.Sum(topping => topping.Price);
            var detail = new OrderDetail
            {
                MaHD = order.MaHD,
                MaSP = prepared.Request.MaSP,
                MaBienThe = prepared.Request.MaBienThe,
                SoLuong = prepared.Request.SoLuong,
                GiaBan = prepared.UnitPrice,
                TienTopping = decimal.ToInt32(decimal.Round(toppingTotal, 0)),
                ThanhTien = (prepared.UnitPrice + toppingTotal) * prepared.Request.SoLuong
            };
            db.OrderDetails.Add(detail);
            foreach (var topping in prepared.Toppings)
            {
                db.OrderDetailToppings.Add(new OrderDetailTopping
                {
                    OrderDetail = detail,
                    MaSP = topping.Id,
                    GiaBan = decimal.ToInt32(decimal.Round(topping.Price, 0))
                });
            }
        }

        var pointsEarned = (int)(order.TongTien / 10000m);
        var newPoints = customer.DiemTichLuy + pointsEarned;
        var newRank = newPoints >= 500 ? "Kim Cương"
            : newPoints >= 200 ? "Vàng"
            : newPoints >= 50 ? "Bạc"
            : customer.HangTV;
        await db.Customers.Where(item => item.MaKH == customerId && item.TrangThai)
            .ExecuteUpdateAsync(update => update
                .SetProperty(item => item.DiemTichLuy, newPoints)
                .SetProperty(item => item.HangTV, newRank), cancellationToken);
        if (pointsEarned > 0)
        {
            db.PointHistories.Add(new PointHistory
            {
                MaKH = customer.MaKH,
                MaHD = order.MaHD,
                DiemThayDoi = pointsEarned,
                LyDo = $"Tích điểm đơn hàng #{order.MaHD}",
                NgayThucHien = DateTime.Now
            });
        }

        if (request.LuuThongTinMacDinh)
        {
            await db.Customers.Where(item => item.MaKH == customerId && item.TrangThai)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(item => item.HoTen, recipientName)
                    .SetProperty(item => item.Sdt, recipientPhone)
                    .SetProperty(item => item.Email, recipientEmail)
                    .SetProperty(item => item.DiaChi, recipientAddress), cancellationToken);

            if (!string.IsNullOrWhiteSpace(customer.TaiKhoan))
            {
                var account = await db.Accounts.FirstOrDefaultAsync(
                    item => item.TenDangNhap == customer.TaiKhoan, cancellationToken);
                if (account is not null)
                    account.HoTen = recipientName;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new CheckoutResponse(
            order.MaHD, order.TrangThai, subtotal, discount, shippingFee, order.TongTien));
    }

    /// <summary>List order history for the authenticated customer.</summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<OrderHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<OrderHistoryResponse>>> History(
        [FromQuery] string? username, CancellationToken cancellationToken)
    {
        var customerId = (int)HttpContext.Items["MobileCustomerId"]!;
        var customerUsername = await db.Customers.AsNoTracking()
            .Where(customer => customer.MaKH == customerId)
            .Select(customer => customer.TaiKhoan)
            .FirstOrDefaultAsync(cancellationToken);
        if (customerUsername is null
            || (!string.IsNullOrWhiteSpace(username) && !string.Equals(username, customerUsername, StringComparison.OrdinalIgnoreCase)))
            return StatusCode(StatusCodes.Status403Forbidden, new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = "Không thể tra cứu đơn hàng của tài khoản khác."
            });

        var orders = await db.Orders.AsNoTracking()
            .Where(order => order.MaKH == customerId)
            .OrderByDescending(order => order.NgayTao)
            .Select(order => new OrderHistoryResponse(
                order.MaHD, order.NgayTao, order.TongTien, order.TrangThai, order.LyDoHuy,
                order.LoaiDonHang,
                order.OrderDetails.Select(detail => new OrderHistoryLineResponse(
                    detail.Product!.TenSP,
                    detail.Variant!.KichCo,
                    detail.SoLuong,
                    detail.ThanhTien,
                    detail.Toppings.Select(topping => topping.ToppingProduct!.TenSP).ToList()))
                    .ToList()))
            .ToListAsync(cancellationToken);
        return Ok(orders);
    }

    private sealed record PreparedOrderItem(CheckoutItemRequest Request, decimal UnitPrice, List<PreparedTopping> Toppings);
    private sealed record PreparedTopping(int Id, string Name, decimal Price);
    private sealed record CustomerOrderInfo(
        int MaKH, string? TaiKhoan, string HoTen, string Sdt, string? Email,
        string? DiaChi, int DiemTichLuy, string HangTV);
}

/// <summary>Order line selection and quantity for checkout.</summary>
public sealed record CheckoutItemRequest
{
    [Range(1, int.MaxValue)]
    public int MaSP { get; init; }

    [Range(1, int.MaxValue)]
    public int MaBienThe { get; init; }

    [Range(1, 50)]
    public int SoLuong { get; init; } = 1;

    public List<int> ToppingIds { get; init; } = [];
}

/// <summary>Mobile checkout payload; customer identity is bound to the session token.</summary>
public sealed record CheckoutRequest
{
    [Required, StringLength(50)]
    public required string LoaiDonHang { get; init; }

    [StringLength(100)]
    public string? HoTenNguoiNhan { get; init; }

    [StringLength(20)]
    public string? SdtNguoiNhan { get; init; }

    [EmailAddress, StringLength(100)]
    public string? EmailNguoiNhan { get; init; }

    public bool LuuThongTinMacDinh { get; init; }

    [StringLength(255)]
    public string? DiaChiGiao { get; init; }

    [StringLength(255)]
    public string? GhiChu { get; init; }

    [StringLength(50)]
    public string? MaCode { get; init; }

    [Required, StringLength(50)]
    public required string PhuongThucThanhToan { get; init; }

    [Required, MinLength(1)]
    public required List<CheckoutItemRequest> Items { get; init; }
}

/// <summary>Successfully created order summary.</summary>
public sealed record CheckoutResponse(
    int MaHD, string TrangThai, decimal TamTinh, decimal TienGiamGia, int PhiGiao, decimal TongTien);

/// <summary>Order history summary with cancellation reason and lines.</summary>
public sealed record OrderHistoryResponse(
    int MaHD, DateTime NgayTao, decimal TongTien, string TrangThai, string? LyDoHuy,
    string LoaiDonHang, IReadOnlyList<OrderHistoryLineResponse> Items);

/// <summary>Order history line and selected toppings.</summary>
public sealed record OrderHistoryLineResponse(
    string TenSP, string KichCo, int SoLuong, decimal ThanhTien, IReadOnlyList<string> Toppings);
