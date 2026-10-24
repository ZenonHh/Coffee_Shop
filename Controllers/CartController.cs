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
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.MaCode == cart.VoucherCode && v.TrangThai);
            var now = DateTime.Now;
            if (voucher == null || (voucher.NgayBatDau.HasValue && now < voucher.NgayBatDau.Value) || (voucher.NgayKetThuc.HasValue && now > voucher.NgayKetThuc.Value) || cart.SubTotal < voucher.DonToiThieu)
            {
                cart.VoucherCode = null;
                return;
            }

            var discountBase = cart.SubTotal;
            var voucherDiscount = voucher.PhanLoaiGiam.Equals("PERCENT", StringComparison.OrdinalIgnoreCase)
                ? discountBase * voucher.GiaTriGiam / 100m
                : voucher.GiaTriGiam;
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
            var product = await _context.Products.FindAsync(productId);
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (product == null || variant == null) return Json(new { success = false, message = "Sản phẩm hoặc biến thể không tồn tại." });

            toppingIds ??= new List<int>();
            var toppings = await _context.Products.Include(p => p.Variants).Where(p => toppingIds.Contains(p.MaSP)).ToListAsync();
            var toppingPrice = toppings.Sum(top => top.Variants.FirstOrDefault()?.GiaBan ?? 10000);
            var cart = await _cartStore.GetCartAsync();
            var newItem = new CartItemViewModel
            {
                ProductId = product.MaSP, ProductName = product.TenSP, ProductImage = product.HinhAnh,
                VariantId = variant.MaBienThe, Size = variant.KichCo, BasePrice = variant.GiaBan,
                Quantity = Math.Max(1, quantity), SelectedToppingIds = toppingIds,
                SelectedToppingNames = toppings.Select(t => t.TenSP).ToList(), ToppingPrice = toppingPrice
            };
            var existingItem = cart.Items.FirstOrDefault(i => i.ItemKey == newItem.ItemKey);
            if (existingItem != null) existingItem.Quantity += newItem.Quantity;
            else cart.Items.Add(newItem);
            await RefreshDiscounts(cart);
            await _cartStore.SaveCartAsync(cart);
            return Json(new { success = true, message = $"Đã thêm {product.TenSP} vào giỏ hàng!", cartCount = cart.Items.Sum(i => i.Quantity), cartTotal = cart.GrandTotal });
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
            return Json(new { success = true, cartCount = cart.Items.Sum(i => i.Quantity), subTotal = cart.SubTotal, automaticDiscount = cart.AutomaticDiscountAmount, voucherDiscount = cart.VoucherDiscountAmount, discount = cart.DiscountAmount, grandTotal = cart.GrandTotal, itemTotal = item?.TotalPrice ?? 0, voucherCode = cart.VoucherCode });
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
        public async Task<IActionResult> ApplyVoucher(string voucherCode)
        {
            var cart = await _cartStore.GetCartAsync();
            if (string.IsNullOrWhiteSpace(voucherCode))
            {
                cart.VoucherCode = null;
                await RefreshDiscounts(cart);
                await _cartStore.SaveCartAsync(cart);
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá.", automaticDiscount = cart.AutomaticDiscountAmount, discount = cart.DiscountAmount, grandTotal = cart.GrandTotal });
            }

            var code = voucherCode.Trim().ToUpperInvariant();
            var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.MaCode == code && v.TrangThai);
            if (voucher == null) return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đang tắt." });
            var now = DateTime.Now;
            if (voucher.NgayBatDau.HasValue && now < voucher.NgayBatDau.Value) return Json(new { success = false, message = $"Voucher bắt đầu từ {voucher.NgayBatDau.Value:dd/MM/yyyy HH:mm}." });
            if (voucher.NgayKetThuc.HasValue && now > voucher.NgayKetThuc.Value) return Json(new { success = false, message = "Voucher đã hết thời hạn sử dụng." });
            if (cart.SubTotal < voucher.DonToiThieu) return Json(new { success = false, message = $"Đơn hàng tối thiểu {voucher.DonToiThieu:N0} đ để dùng mã này." });

            cart.VoucherCode = voucher.MaCode;
            await RefreshDiscounts(cart);
            await _cartStore.SaveCartAsync(cart);
            return Json(new { success = true, message = $"Đã áp dụng {voucher.MaCode}{(string.IsNullOrWhiteSpace(voucher.TenChuongTrinh) ? "" : $" - {voucher.TenChuongTrinh}")}.", automaticDiscount = cart.AutomaticDiscountAmount, voucherDiscount = cart.VoucherDiscountAmount, discount = cart.DiscountAmount, grandTotal = cart.GrandTotal });
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var cart = await _cartStore.GetCartAsync();
            return Json(new { count = cart.Items.Sum(i => i.Quantity) });
        }
    }
}