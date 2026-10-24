using CoffeeShop.Models.Entities;

namespace CoffeeShop.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Product> FeaturedProducts { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Feedback> CustomerReviews { get; set; } = new();
    }

    public class ShopViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public int? SelectedCategoryId { get; set; }
        public string? Keyword { get; set; }
    }

    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;
        public List<ProductVariant> Variants { get; set; } = new();
        public List<Product> AvailableToppings { get; set; } = new();
        public List<Product> RelatedProducts { get; set; } = new();
        public List<Feedback> Reviews { get; set; } = new();
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
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
        public string PaymentMethod { get; set; } = "COD"; // COD, VNPay, Chuyển khoản
        public string? VoucherCode { get; set; }
    }
}
