using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Helpers;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

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
                Cart = cart
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

            int? customerId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var username = User.Identity.Name;
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.TaiKhoan == username);
                if (customer != null)
                {
                    customerId = customer.MaKH;
                }
            }

            // Tạo Hóa Đơn
            var order = new Order
            {
                MaKH = customerId,
                LoaiDonHang = "Giao hàng",
                TamTinh = (int)cart.SubTotal,
                TienGiamGia = cart.DiscountAmount,
                PhiGiao = (int)cart.ShippingFee,
                TongTien = cart.GrandTotal,
                TrangThai = "Hoàn tất",
                NgayTao = DateTime.Now,
                HoTenNguoiNhan = model.FullName,
                SdtNguoiNhan = model.Phone,
                DiaChiGiao = model.DeliveryAddress,
                GhiChu = model.Note
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Thêm chi tiết đơn hàng
            foreach (var item in cart.Items)
            {
                var orderDetail = new OrderDetail
                {
                    MaHD = order.MaHD,
                    MaSP = item.ProductId,
                    MaBienThe = item.VariantId,
                    SoLuong = item.Quantity,
                    GiaBan = item.BasePrice,
                    TienTopping = (int)item.ToppingPrice,
                    ThanhTien = item.TotalPrice
                };

                _context.OrderDetails.Add(orderDetail);
                await _context.SaveChangesAsync();

                // Lưu các Topping đã chọn
                if (item.SelectedToppingIds.Any())
                {
                    foreach (var topId in item.SelectedToppingIds)
                    {
                        var toppingProduct = await _context.Products
                            .Include(p => p.Variants)
                            .FirstOrDefaultAsync(p => p.MaSP == topId);

                        var topPrice = toppingProduct?.Variants.FirstOrDefault()?.GiaBan ?? 10000;

                        var orderDetailTopping = new OrderDetailTopping
                        {
                            MaCTHD = orderDetail.MaCTHD,
                            MaSP = topId,
                            GiaBan = (int)topPrice
                        };
                        _context.OrderDetailToppings.Add(orderDetailTopping);
                    }
                    await _context.SaveChangesAsync();
                }
            }

            // Tích điểm nếu là thành viên: 10.000 VNĐ = 1 điểm
            if (customerId.HasValue)
            {
                var customer = await _context.Customers.FindAsync(customerId.Value);
                if (customer != null)
                {
                    int pointsEarned = (int)(cart.GrandTotal / 10000m);
                    customer.DiemTichLuy += pointsEarned;

                    // Thăng hạng thành viên
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

                    await _context.SaveChangesAsync();
                }
            }

            // Xóa giỏ hàng
            await _cartStore.ClearCartAsync();

            return RedirectToAction(nameof(Success), new { id = order.MaHD });
        }

        public async Task<IActionResult> Success(int id)
        {
            var order = await _context.Orders
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
