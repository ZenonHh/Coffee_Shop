using System.Security.Claims;
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

        public async Task<IActionResult> Index()
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

            // Nếu user đã đăng nhập, tự động điền thông tin
            if (User.Identity?.IsAuthenticated == true)
            {
                var username = User.Identity.Name;
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaiKhoan == username);
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
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var cart = await _cartStore.GetCartAsync();
            if (!cart.Items.Any())
            {
                ModelState.AddModelError(string.Empty, "Giỏ hàng của bạn đang trống.");
                model.Cart = cart;
                return View("Index", model);
            }

            if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Phone) || string.IsNullOrWhiteSpace(model.DeliveryAddress))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ họ tên, số điện thoại và địa chỉ nhận hàng.");
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

            int? customerId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaiKhoan == User.Identity.Name);
                customerId = customer?.MaKH;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
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

            if (customerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(customerId.Value);
                if (customer != null)
                {
                    var pointsEarned = (int)(cart.GrandTotal / 10000m);
                    customer.DiemTichLuy += pointsEarned;
                    if (customer.DiemTichLuy >= 500) customer.HangTV = "Kim Cương";
                    else if (customer.DiemTichLuy >= 200) customer.HangTV = "Vàng";
                    else if (customer.DiemTichLuy >= 50) customer.HangTV = "Bạc";
                    _context.PointHistories.Add(new PointHistory
                    {
                        MaKH = customer.MaKH,
                        MaHD = order.MaHD,
                        DiemThayDoi = pointsEarned,
                        LyDo = $"Tích điểm đơn hàng #{order.MaHD}",
                        NgayThucHien = DateTime.Now
                    });
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await _cartStore.ClearCartAsync();
            return RedirectToAction(nameof(Success), new { id = order.MaHD });
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
