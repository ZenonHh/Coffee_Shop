using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Helpers;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Controllers
{
    public class CartController : Controller
    {
        private readonly CoffeeShopDbContext _context;
        private readonly PersistentCartStore _cartStore;

        public CartController(CoffeeShopDbContext context, PersistentCartStore cartStore)
        {
            _context = context;
            _cartStore = cartStore;
        }

        private static decimal CalculateAutomaticDiscount(decimal subtotal) => subtotal switch
        {
            >= 500000m => 30000m,
            >= 300000m => 15000m,
            >= 200000m => 5000m,
            _ => 0m
        };

        private async Task RefreshDiscounts(CartViewModel cart)
        {
            cart.AutomaticDiscountAmount = Math.Min(CalculateAutomaticDiscount(cart.SubTotal), cart.SubTotal);
            cart.VoucherDiscountAmount = 0;

            if (string.IsNullOrWhiteSpace(cart.VoucherCode)) return;
            var code = cart.VoucherCode.Trim().ToUpperInvariant();
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.MaCode == code && v.TrangThai);
            var now = DateTime.Now;
            if (voucher == null || voucher.SoLuongDaDung >= voucher.SoLuongToiDa
                || (voucher.NgayBatDau.HasValue && now < voucher.NgayBatDau.Value)
                || (voucher.NgayKetThuc.HasValue && now > voucher.NgayKetThuc.Value)
                || cart.SubTotal < voucher.DonToiThieu)
            {
                cart.VoucherCode = null;
                return;
            }

            var discountBase = cart.SubTotal;
            var isPercent = voucher.PhanLoaiGiam.Equals("PERCENT", StringComparison.OrdinalIgnoreCase);
            var voucherDiscount = isPercent
                ? discountBase * voucher.GiaTriGiam / 100m
                : voucher.GiaTriGiam;
            if (isPercent && voucher.GiamToiDa.HasValue)
                voucherDiscount = Math.Min(voucherDiscount, voucher.GiamToiDa.Value);
            cart.VoucherDiscountAmount = Math.Min(Math.Max(0, voucherDiscount), Math.Max(0, cart.SubTotal - cart.AutomaticDiscountAmount));
        }

        public async Task<IActionResult> Index()
        {
            var cart = await _cartStore.GetCartAsync();
            await RefreshDiscounts(cart);
            await _cartStore.SaveCartAsync(cart);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int variantId, List<int>? toppingIds, int quantity = 1)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                var returnUrl = Url.Action("Detail", "Shop", new { id = productId });
                return Json(new
                {
                    success = false,
                    message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.",
                    loginUrl = Url.Action("Login", "Account", new { returnUrl })
                });
            }

            quantity = Math.Clamp(quantity, 1, 50);
            var product = await _context.Products.FirstOrDefaultAsync(p => p.MaSP == productId && p.TrangThai && p.SoLuongTon > 0 && p.MaLoaiSP != 9);
            var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.MaBienThe == variantId && v.MaSP == productId && v.TrangThai);
            if (product == null || variant == null) return Json(new { success = false, message = "Sản phẩm hoặc biến thể không tồn tại." });

            toppingIds = (toppingIds ?? new List<int>()).Distinct().ToList();
            var toppings = await _context.Products.AsNoTracking()
                .Where(p => toppingIds.Contains(p.MaSP) && p.TrangThai && p.SoLuongTon > 0 && p.MaLoaiSP == 9)
                .Select(product => new
                {
                    product.MaSP,
                    product.TenSP,
                    GiaBan = product.Variants.Where(variant => variant.TrangThai)
                        .OrderBy(variant => variant.MaBienThe)
                        .Select(variant => (decimal?)variant.GiaBan)
                        .FirstOrDefault()
                })
                .ToListAsync();
            if (toppings.Count != toppingIds.Count)
                return Json(new { success = false, message = "Một topping đã chọn hiện không đủ số lượng trong kho." });
            var toppingPrice = toppings.Sum(top => top.GiaBan ?? 10000m);
            var cart = await _cartStore.GetCartAsync();
            var newItem = new CartItemViewModel
            {
                ProductId = product.MaSP, ProductName = product.TenSP, ProductImage = product.HinhAnh,
                VariantId = variant.MaBienThe, Size = variant.KichCo, BasePrice = variant.GiaBan,
                Quantity = quantity, SelectedToppingIds = toppingIds,
                SelectedToppingNames = toppings.Select(t => t.TenSP).ToList(), ToppingPrice = toppingPrice
            };
            var existingItem = cart.Items.FirstOrDefault(i => i.ItemKey == newItem.ItemKey);
            if (existingItem != null) existingItem.Quantity += newItem.Quantity;
            else cart.Items.Add(newItem);
            await RefreshDiscounts(cart);
            await _cartStore.SaveCartAsync(cart);
            return Json(new { success = true, message = $"Đã thêm {product.TenSP} vào giỏ hàng!", cartCount = cart.Items.Count, cartTotal = cart.GrandTotal });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(string itemKey, int quantity)
        {
            var cart = await _cartStore.GetCartAsync();
            var item = cart.Items.FirstOrDefault(i => i.ItemKey == itemKey);
            if (item != null)
            {
                if (quantity <= 0) cart.Items.Remove(item);
                else item.Quantity = quantity;
            }
            await RefreshDiscounts(cart);
            await _cartStore.SaveCartAsync(cart);
            return Json(new { success = true, cartCount = cart.Items.Count, subTotal = cart.SubTotal, automaticDiscount = cart.AutomaticDiscountAmount, voucherDiscount = cart.VoucherDiscountAmount, discount = cart.DiscountAmount, grandTotal = cart.GrandTotal, itemTotal = item?.TotalPrice ?? 0, voucherCode = cart.VoucherCode });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(string itemKey)
        {
            var cart = await _cartStore.GetCartAsync();
            var item = cart.Items.FirstOrDefault(i => i.ItemKey == itemKey);
            if (item != null) cart.Items.Remove(item);
            await RefreshDiscounts(cart);
            await _cartStore.SaveCartAsync(cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApplyVoucher(string? code, decimal subTotal = 0, string? voucherCode = null)
        {
            var cart = await _cartStore.GetCartAsync();
            var submittedCode = string.IsNullOrWhiteSpace(code) ? voucherCode : code;
            if (string.IsNullOrWhiteSpace(submittedCode))
            {
                cart.VoucherCode = null;
                await RefreshDiscounts(cart);
                await _cartStore.SaveCartAsync(cart);
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá.", automaticDiscount = cart.AutomaticDiscountAmount, discount = cart.DiscountAmount, grandTotal = cart.GrandTotal });
            }

            var normalizedCode = submittedCode.Trim().ToUpperInvariant();
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.MaCode == normalizedCode && v.TrangThai);
            if (voucher == null || voucher.SoLuongDaDung >= voucher.SoLuongToiDa) return Json(new { success = false, message = "Mã giảm giá không hợp lệ, đã hết lượt hoặc đang tắt." });
            var now = DateTime.Now;
            if (voucher.NgayBatDau.HasValue && now < voucher.NgayBatDau.Value) return Json(new { success = false, message = $"Voucher bắt đầu từ {voucher.NgayBatDau.Value:dd/MM/yyyy HH:mm}." });
            if (voucher.NgayKetThuc.HasValue && now > voucher.NgayKetThuc.Value) return Json(new { success = false, message = "Voucher đã hết thời hạn sử dụng." });
            if (cart.SubTotal < voucher.DonToiThieu) return Json(new { success = false, message = $"Đơn hàng tối thiểu {voucher.DonToiThieu:N0} đ để dùng mã này." });

            cart.VoucherCode = voucher.MaCode;
            await RefreshDiscounts(cart);
            await _cartStore.SaveCartAsync(cart);
            return Json(new { success = true, discountAmount = cart.VoucherDiscountAmount, finalTotal = cart.GrandTotal, message = $"Đã áp dụng {voucher.MaCode}{(string.IsNullOrWhiteSpace(voucher.TenChuongTrinh) ? "" : $" - {voucher.TenChuongTrinh}")}.", automaticDiscount = cart.AutomaticDiscountAmount, voucherDiscount = cart.VoucherDiscountAmount, discount = cart.DiscountAmount, grandTotal = cart.GrandTotal });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var cart = await _cartStore.GetCartAsync();
            return Json(new { count = cart.Items.Count });
        }
    }
}