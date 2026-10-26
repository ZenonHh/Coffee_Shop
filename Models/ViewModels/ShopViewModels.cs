using CoffeeShop.Models.Entities;

namespace CoffeeShop.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<ShopProductCardViewModel> FeaturedProducts { get; set; } = new();
        public List<ShopCategoryViewModel> Categories { get; set; } = new();
        public List<ProductReviewViewModel> CustomerReviews { get; set; } = new();
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalReviews { get; set; }
    }

    public class HomeCategoryViewModel
    {
        public string TenLoaiSP { get; set; } = string.Empty;
        public List<ShopProductCardViewModel> Products { get; set; } = new();
    }

    public class ShopViewModel
    {
        public List<ShopCategoryViewModel> Categories { get; set; } = new();
        public List<ShopProductCardViewModel> Products { get; set; } = new();
        public int? SelectedCategoryId { get; set; }
        public string? Keyword { get; set; }
    }

    public class ShopCategoryViewModel
    {
        public int MaLoaiSP { get; set; }
        public string TenLoaiSP { get; set; } = string.Empty;
    }

    public class ShopProductCardViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }
        public string TenLoaiSP { get; set; } = string.Empty;
        public decimal? GiaBan { get; set; }
        public int SoLuongTon { get; set; }
    }

    public class ProductDetailViewModel
    {
        public ProductDetailProductViewModel Product { get; set; } = new();
        public List<ProductVariantOptionViewModel> Variants { get; set; } = new();
        public List<ToppingOptionViewModel> AvailableToppings { get; set; } = new();
        public List<ShopProductCardViewModel> RelatedProducts { get; set; } = new();
        public List<ProductReviewViewModel> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
    }

    public class ProductDetailProductViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }
        public string TenLoaiSP { get; set; } = string.Empty;
        public int SoLuongTon { get; set; }
    }

    public class ProductVariantOptionViewModel
    {
        public int MaBienThe { get; set; }
        public string KichCo { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
    }

    public class ToppingOptionViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
    }

    public class ProductReviewViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public int? DanhGia { get; set; }
        public DateTime NgayGui { get; set; }
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public int VariantId { get; set; }
        public string Size { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int Quantity { get; set; } = 1;
        public List<int> SelectedToppingIds { get; set; } = new();
        public List<string> SelectedToppingNames { get; set; } = new();
        public decimal ToppingPrice { get; set; }

        public decimal UnitPrice => BasePrice + ToppingPrice;
        public decimal TotalPrice => UnitPrice * Quantity;
        public string ItemKey => $"{ProductId}_{VariantId}_{string.Join("-", SelectedToppingIds.OrderBy(x => x))}";
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(i => i.TotalPrice);
        public decimal ShippingFee { get; set; } = 0;
        public decimal AutomaticDiscountAmount { get; set; } = 0;
        public decimal VoucherDiscountAmount { get; set; } = 0;
        public decimal DiscountAmount => AutomaticDiscountAmount + VoucherDiscountAmount;
        public string? VoucherCode { get; set; }
        public decimal GrandTotal => Math.Max(0, SubTotal + ShippingFee - DiscountAmount);
    }

    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; } = new();
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string PaymentMethod { get; set; } = "COD";
        public string? VoucherCode { get; set; }
    }
}
