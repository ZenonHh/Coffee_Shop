using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models.Entities;
using CoffeeShop.Models.ViewModels;

namespace CoffeeShop.Helpers
{
    public sealed class PersistentCartStore
    {
        private const string CartCookieName = "CoffeeShop.CartId";
        private static readonly TimeSpan CartLifetime = TimeSpan.FromDays(30);

        private readonly CoffeeShopDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PersistentCartStore(CoffeeShopDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CartViewModel> GetCartAsync()
        {
            var cartId = GetCurrentCartId();
            var snapshot = await _context.CartSnapshots.FindAsync(cartId);
            return snapshot == null
                ? new CartViewModel()
                : JsonSerializer.Deserialize<CartViewModel>(snapshot.CartJson)
                    ?? throw new InvalidOperationException("Dữ liệu giỏ hàng đã lưu không hợp lệ.");
        }

        public async Task SaveCartAsync(CartViewModel cart)
        {
            if (cart.Items.Count == 0)
            {
                await ClearCartAsync();
                return;
            }

            var cartId = GetCurrentCartId();
            var snapshot = await _context.CartSnapshots.FindAsync(cartId);
            if (snapshot == null)
            {
                snapshot = new CartSnapshot { Id = cartId };
                _context.CartSnapshots.Add(snapshot);
            }

            snapshot.CartJson = JsonSerializer.Serialize(cart);
            snapshot.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync()
        {
            var snapshot = await _context.CartSnapshots.FindAsync(GetCurrentCartId());
            if (snapshot == null) return;

            _context.CartSnapshots.Remove(snapshot);
            await _context.SaveChangesAsync();
        }

        public async Task AssociateGuestCartWithCustomerAsync(int customerId)
        {
            var guestCartId = GetGuestCartId();
            var customerCartId = GetCustomerCartId(customerId);

            if (guestCartId.HasValue && guestCartId.Value != customerCartId)
            {
                var guestSnapshot = await _context.CartSnapshots.FindAsync(guestCartId.Value);
                if (guestSnapshot != null)
                {
                    var customerSnapshot = await _context.CartSnapshots.FindAsync(customerCartId);
                    if (customerSnapshot == null)
                    {
                        _context.CartSnapshots.Add(new CartSnapshot
                        {
                            Id = customerCartId,
                            CartJson = guestSnapshot.CartJson,
                            UpdatedAtUtc = DateTime.UtcNow
                        });
                        _context.CartSnapshots.Remove(guestSnapshot);
                    }
                    else
                    {
                        var guestCart = DeserializeCart(guestSnapshot.CartJson);
                        var customerCart = DeserializeCart(customerSnapshot.CartJson);
                        MergeCart(customerCart, guestCart);
                        customerSnapshot.CartJson = JsonSerializer.Serialize(customerCart);
                        customerSnapshot.UpdatedAtUtc = DateTime.UtcNow;
                        _context.CartSnapshots.Remove(guestSnapshot);
                    }

                    await _context.SaveChangesAsync();
                }
            }

            SetGuestCartId(Guid.NewGuid());
        }

        private Guid GetCurrentCartId()
        {
            var context = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("Không tìm thấy HTTP context để đọc giỏ hàng.");
            if (context.User.Identity?.IsAuthenticated == true
                && int.TryParse(context.User.FindFirstValue("CustomerId"), out var customerId)
                && customerId > 0)
            {
                return GetCustomerCartId(customerId);
            }

            return GetOrCreateGuestCartId();
        }

        private Guid GetOrCreateGuestCartId()
        {
            var cartId = GetGuestCartId();
            if (cartId.HasValue) return cartId.Value;

            var newCartId = Guid.NewGuid();
            SetGuestCartId(newCartId);
            return newCartId;
        }

        private Guid? GetGuestCartId()
        {
            var context = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("Không tìm thấy HTTP context để đọc giỏ hàng.");
            if (!context.Request.Cookies.TryGetValue(CartCookieName, out var cookieValue))
                return null;

            return Guid.TryParseExact(cookieValue, "N", out var cartId) ? cartId : null;
        }

        private void SetGuestCartId(Guid cartId)
        {
            var context = _httpContextAccessor.HttpContext
                ?? throw new InvalidOperationException("Không tìm thấy HTTP context để lưu giỏ hàng.");
            context.Response.Cookies.Append(
                CartCookieName,
                cartId.ToString("N"),
                new CookieOptions
                {
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = context.Request.IsHttps,
                    MaxAge = CartLifetime
                });
        }

        private static Guid GetCustomerCartId(int customerId)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"CoffeeShop.CustomerCart:{customerId}"));
            return new Guid(hash.AsSpan(0, 16));
        }

        private static CartViewModel DeserializeCart(string cartJson)
        {
            return JsonSerializer.Deserialize<CartViewModel>(cartJson)
                ?? throw new InvalidOperationException("Dữ liệu giỏ hàng đã lưu không hợp lệ.");
        }

        private static void MergeCart(CartViewModel target, CartViewModel source)
        {
            foreach (var sourceItem in source.Items)
            {
                var targetItem = target.Items.FirstOrDefault(item => item.ItemKey == sourceItem.ItemKey);
                if (targetItem == null)
                {
                    target.Items.Add(sourceItem);
                }
                else
                {
                    targetItem.Quantity += sourceItem.Quantity;
                }
            }

            if (string.IsNullOrWhiteSpace(target.VoucherCode))
                target.VoucherCode = source.VoucherCode;
        }
    }
}
