using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Helpers;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;
using CoffeeShop.Services;

namespace CoffeeShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CoffeeShopDbContext _context;
        private readonly PersistentCartStore _cartStore;

        public CheckoutController(CoffeeShopDbContext context, PersistentCartStore cartStore)
        {
            _context = context;
            _cartStore = cartStore;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var cart = await _cartStore.GetCartAsync();
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Shop");
            }

            var viewModel = new CheckoutViewModel
            {
                Cart = cart,
                VoucherCode = cart.VoucherCode
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var customer = await FindCurrentCustomerAsync(tracking: false, cancellationToken);
                if (customer != null)
                {
                    viewModel.FullName = customer.HoTen;
                    viewModel.Phone = customer.Sdt;
                    viewModel.Email = customer.Email;
                    viewModel.DeliveryAddress = customer.DiaChi ?? string.Empty;
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(
            CheckoutViewModel model, CancellationToken cancellationToken)
        {
            var cart = await _cartStore.GetCartAsync();
            if (!cart.Items.Any())
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng của bạn đang trống.");
                model.Cart = cart;
                return View("Index", model);
            }

            model.FullName = model.FullName?.Trim() ?? string.Empty;
            model.Phone = model.Phone?.Trim() ?? string.Empty;
            model.Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim();
            model.DeliveryAddress = model.DeliveryAddress?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(model.FullName))
                ModelState.AddModelError(nameof(model.FullName), "Vui lòng nhập họ tên người nhận.");
            if (!Regex.IsMatch(model.Phone, @"^[0-9]{10}$"))
                ModelState.AddModelError(nameof(model.Phone), "Số điện thoại phải gồm đúng 10 chữ số.");
            if (!string.IsNullOrWhiteSpace(model.Email)
                && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(model.Email))
                ModelState.AddModelError(nameof(model.Email), "Email không hợp lệ.");
            if (string.IsNullOrWhiteSpace(model.DeliveryAddress))
                ModelState.AddModelError(nameof(model.DeliveryAddress), "Vui lòng nhập địa chỉ nhận hàng.");
            if (!ModelState.IsValid)
            {
                model.Cart = cart;
                return View("Index", model);
            }

            if (model.PaymentMethod is not ("COD" or "Transfer"))
            {
                ModelState.AddModelError(nameof(model.PaymentMethod), "Phương thức thanh toán không hợp lệ.");
                model.Cart = cart;
                return View("Index", model);
            }

            cart.AutomaticDiscountAmount = Math.Min(CalculateAutomaticDiscount(cart.SubTotal), cart.SubTotal);
            Voucher? voucher = null;
            if (!string.IsNullOrWhiteSpace(cart.VoucherCode))
            {
                var code = cart.VoucherCode.Trim().ToUpperInvariant();
                voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.MaCode == code && v.TrangThai);
                var now = DateTime.Now;
                if (voucher == null || voucher.SoLuongDaDung >= voucher.SoLuongToiDa
                    || (voucher.NgayBatDau.HasValue && now < voucher.NgayBatDau.Value)
                    || (voucher.NgayKetThuc.HasValue && now > voucher.NgayKetThuc.Value)
                    || cart.SubTotal < voucher.DonToiThieu)
                {
                    ModelState.AddModelError(string.Empty, "Voucher không còn hợp lệ. Vui lòng áp dụng lại mã giảm giá.");
                    model.Cart = cart;
                    return View("Index", model);
                }

                var isPercent = voucher.PhanLoaiGiam.Equals("PERCENT", StringComparison.OrdinalIgnoreCase);
                var voucherDiscount = isPercent
                    ? cart.SubTotal * voucher.GiaTriGiam / 100m
                    : voucher.GiaTriGiam;
                if (isPercent && voucher.GiamToiDa.HasValue)
                    voucherDiscount = Math.Min(voucherDiscount, voucher.GiamToiDa.Value);
                cart.VoucherDiscountAmount = Math.Min(Math.Max(0, voucherDiscount), Math.Max(0, cart.SubTotal - cart.AutomaticDiscountAmount));
            }
            else
            {
                cart.VoucherDiscountAmount = 0;
            }

            Customer? customerForOrder = null;
            if (User.Identity?.IsAuthenticated == true)
                customerForOrder = await FindCurrentCustomerAsync(tracking: true, cancellationToken);
            var customerId = customerForOrder?.MaKH;

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            foreach (var item in cart.Items)
            {
                var validItem = await _context.Products.AnyAsync(product => product.MaSP == item.ProductId
                    && product.TrangThai && product.MaLoaiSP != 9
                    && product.Variants.Any(variant => variant.MaBienThe == item.VariantId && variant.TrangThai));
                if (!validItem)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, $"Món {item.ProductName} không còn khả dụng. Vui lòng cập nhật giỏ hàng.");
                    model.Cart = cart;
                    return View("Index", model);
                }
            }

            var toppingIds = cart.Items.SelectMany(item => item.SelectedToppingIds).Distinct().ToList();
            var validToppingCount = await _context.Products.CountAsync(product => toppingIds.Contains(product.MaSP)
                && product.TrangThai && product.MaLoaiSP == 9);
            if (validToppingCount != toppingIds.Count)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Một topping đã chọn không còn khả dụng. Vui lòng cập nhật giỏ hàng.");
                model.Cart = cart;
                return View("Index", model);
            }

            foreach (var requirement in GetStockRequirements(cart))
            {
                var affected = await _context.Products
                    .Where(p => p.MaSP == requirement.Key && p.TrangThai && p.SoLuongTon >= requirement.Value)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.SoLuongTon, p => p.SoLuongTon - requirement.Value));
                if (affected == 0)
                {
                    await transaction.RollbackAsync();
                    var productName = await _context.Products.Where(p => p.MaSP == requirement.Key).Select(p => p.TenSP).FirstOrDefaultAsync() ?? "Sản phẩm";
                    ModelState.AddModelError(string.Empty, $"Món {productName} hiện không đủ số lượng trong kho.");
                    model.Cart = cart;
                    return View("Index", model);
                }
            }

            if (voucher != null)
            {
                var currentTime = DateTime.Now;
                var affected = await _context.Vouchers
                    .Where(v => v.MaVoucher == voucher.MaVoucher && v.TrangThai && v.SoLuongDaDung < v.SoLuongToiDa
                        && (!v.NgayBatDau.HasValue || v.NgayBatDau <= currentTime)
                        && (!v.NgayKetThuc.HasValue || v.NgayKetThuc >= currentTime)
                        && cart.SubTotal >= v.DonToiThieu)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(v => v.SoLuongDaDung, v => v.SoLuongDaDung + 1)
                        .SetProperty(v => v.TrangThai, v => v.SoLuongDaDung + 1 >= v.SoLuongToiDa ? false : v.TrangThai));
                if (affected == 0)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, "Voucher vừa hết lượt sử dụng. Vui lòng kiểm tra lại đơn hàng.");
                    model.Cart = cart;
                    return View("Index", model);
                }
            }

            var isPaid = model.PaymentMethod == "Transfer";
            var order = new Order
            {
                MaKH = customerId,
                MaVoucher = voucher?.MaVoucher,
                LoaiDonHang = "Giao hàng",
                TamTinh = (int)cart.SubTotal,
                TienGiamGia = cart.DiscountAmount,
                PhiGiao = (int)cart.ShippingFee,
                TongTien = cart.GrandTotal,
                TrangThai = OrderStatuses.WaitingForConfirmation,
                PhuongThucThanhToan = isPaid ? "Chuyển khoản (Đã thanh toán)" : "COD",
                NguoiXuLy = "Khách hàng đặt hàng trực tuyến",
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now,
                HoTenNguoiNhan = model.FullName,
                SdtNguoiNhan = model.Phone,
                DiaChiGiao = model.DeliveryAddress,
                GhiChu = model.Note
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderDetails = cart.Items.Select(item => new OrderDetail
            {
                MaHD = order.MaHD,
                MaSP = item.ProductId,
                MaBienThe = item.VariantId,
                SoLuong = item.Quantity,
                GiaBan = item.BasePrice,
                TienTopping = (int)item.ToppingPrice,
                ThanhTien = item.TotalPrice
            }).ToList();
            _context.OrderDetails.AddRange(orderDetails);
            await _context.SaveChangesAsync();

            foreach (var (item, orderDetail) in cart.Items.Zip(orderDetails))
            {
                foreach (var toppingId in item.SelectedToppingIds.Distinct())
                {
                    var toppingPrice = await _context.Products
                        .Where(p => p.MaSP == toppingId)
                        .Select(p => p.Variants.OrderBy(v => v.MaBienThe).Select(v => (decimal?)v.GiaBan).FirstOrDefault())
                        .FirstOrDefaultAsync() ?? 10000m;
                    _context.OrderDetailToppings.Add(new OrderDetailTopping
                    {
                        MaCTHD = orderDetail.MaCTHD,
                        MaSP = toppingId,
                        GiaBan = (int)toppingPrice
                    });
                }
            }

            if (customerForOrder is not null)
            {
                var pointsEarned = (int)(cart.GrandTotal / 10000m);
                customerForOrder.DiemTichLuy += pointsEarned;
                if (customerForOrder.DiemTichLuy >= 500) customerForOrder.HangTV = "Kim Cương";
                else if (customerForOrder.DiemTichLuy >= 200) customerForOrder.HangTV = "Vàng";
                else if (customerForOrder.DiemTichLuy >= 50) customerForOrder.HangTV = "Bạc";

                if (pointsEarned > 0)
                {
                    _context.PointHistories.Add(new PointHistory
                    {
                        MaKH = customerForOrder.MaKH,
                        MaHD = order.MaHD,
                        DiemThayDoi = pointsEarned,
                        LyDo = $"Tích điểm đơn hàng #{order.MaHD}",
                        NgayThucHien = DateTime.Now
                    });
                }

                if (model.SaveCustomerInfo)
                {
                    customerForOrder.HoTen = model.FullName;
                    customerForOrder.Sdt = model.Phone;
                    customerForOrder.Email = model.Email;
                    customerForOrder.DiaChi = model.DeliveryAddress;
                    var account = await _context.Accounts.FirstOrDefaultAsync(
                        item => item.TenDangNhap == customerForOrder.TaiKhoan, cancellationToken);
                    if (account is not null)
                        account.HoTen = model.FullName;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await _cartStore.ClearCartAsync();
            return RedirectToAction(nameof(Success), new { id = order.MaHD });
        }

        private async Task<Customer?> FindCurrentCustomerAsync(bool tracking, CancellationToken cancellationToken)
        {
            IQueryable<Customer> query = _context.Customers;
            if (!tracking)
                query = query.AsNoTracking();

            if (int.TryParse(User.FindFirstValue("CustomerId"), out var customerId) && customerId > 0)
                return await query.FirstOrDefaultAsync(item => item.MaKH == customerId, cancellationToken);

            var username = User.Identity?.Name;
            return string.IsNullOrWhiteSpace(username)
                ? null
                : await query.FirstOrDefaultAsync(item => item.TaiKhoan == username, cancellationToken);
        }

        private static decimal CalculateAutomaticDiscount(decimal subtotal) => subtotal switch
        {
            >= 500000m => 30000m,
            >= 300000m => 15000m,
            >= 200000m => 5000m,
            _ => 0m
        };

        private static Dictionary<int, int> GetStockRequirements(CartViewModel cart)
        {
            var requirements = new Dictionary<int, int>();
            foreach (var item in cart.Items)
            {
                requirements[item.ProductId] = requirements.GetValueOrDefault(item.ProductId) + item.Quantity;
                foreach (var toppingId in item.SelectedToppingIds.Distinct())
                    requirements[toppingId] = requirements.GetValueOrDefault(toppingId) + item.Quantity;
            }
            return requirements;
        }

        public async Task<IActionResult> Success(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Variant)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Toppings)
                .ThenInclude(odt => odt.ToppingProduct)
                .FirstOrDefaultAsync(o => o.MaHD == id);

            if (order == null)
            {
                return RedirectToAction("Index", "Shop");
            }

            return View(order);
        }
    }
}
