using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.ViewModels;

public sealed partial class ToppingSelection(ToppingItem item) : ObservableObject
{
    public ToppingItem Item { get; } = item;

    [ObservableProperty]
    private bool isSelected;
}

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly CartService _cart;
    public ProductDetail Product { get; }
    public ObservableCollection<ToppingSelection> Toppings { get; } = [];
    public IReadOnlyList<string> SugarOptions { get; } = ["100%", "70%", "50%", "0%"];
    public IReadOnlyList<string> IceOptions { get; } = ["100%", "70%", "50%", "0%"];

    [ObservableProperty]
    private ProductVariantItem? selectedVariant;

    [ObservableProperty]
    private string sugarLevel = "100%";

    [ObservableProperty]
    private string iceLevel = "100%";

    [ObservableProperty]
    private string note = string.Empty;

    public decimal TotalPrice =>
        (SelectedVariant?.GiaBan ?? 0m) + Toppings.Where(topping => topping.IsSelected).Sum(topping => topping.Item.GiaBan);

    public ProductDetailViewModel(ProductDetail product, CartService cart)
    {
        Product = product;
        _cart = cart;
        SelectedVariant = product.Variants.FirstOrDefault();
        foreach (var topping in product.Toppings)
            Toppings.Add(new ToppingSelection(topping));
        foreach (var topping in Toppings)
            topping.PropertyChanged += (_, _) => OnPropertyChanged(nameof(TotalPrice));
    }

    partial void OnSelectedVariantChanged(ProductVariantItem? value) =>
        OnPropertyChanged(nameof(TotalPrice));

    [RelayCommand]
    private void SelectVariant(ProductVariantItem? variant) => SelectedVariant = variant;

    [RelayCommand]
    private void AddToCart()
    {
        if (SelectedVariant is null)
            return;
        _cart.Add(new CartLine
        {
            Product = Product,
            Variant = SelectedVariant,
            Toppings = Toppings.Where(topping => topping.IsSelected).Select(topping => topping.Item).ToList(),
            Note = string.Join("; ", new[]
            {
                string.IsNullOrWhiteSpace(SugarLevel) ? null : $"Đường {SugarLevel}",
                string.IsNullOrWhiteSpace(IceLevel) ? null : $"Đá {IceLevel}",
                string.IsNullOrWhiteSpace(Note) ? null : Note.Trim()
            }.Where(text => text is not null))
        });
    }
}

public partial class HomeViewModel(ApiService api, CartService cart) : ObservableObject
{
    public ObservableCollection<ProductItem> BestSellers { get; } = [];
    public string MemberSummary { get; private set; } = "Thành viên Đồng · 0 điểm";
    public IReadOnlyList<string?> Banners { get; } =
    [
        api.ResolveImageUrl("images/bg_1.jpg"),
        api.ResolveImageUrl("images/bg_2.jpg"),
        api.ResolveImageUrl("images/bg_3.jpg")
    ];

    [ObservableProperty]
    private string greeting = GreetingText();

    [ObservableProperty]
    private string customerName = "Khách hàng";

    [ObservableProperty]
    private bool isDelivery = true;

    [RelayCommand]
    private void SelectDelivery()
    {
        IsDelivery = true;
        cart.SelectedFulfillment = "Giao tận nơi";
    }

    [RelayCommand]
    private void SelectPickup()
    {
        IsDelivery = false;
        cart.SelectedFulfillment = "Mang đi";
    }

    public async Task LoadAsync()
    {
        try
        {
            var profile = await api.GetProfileAsync();
            if (profile is not null)
            {
                CustomerName = profile.HoTen;
                MemberSummary = $"Thành viên {profile.HangTV} · {profile.DiemTichLuy} điểm";
                OnPropertyChanged(nameof(MemberSummary));
            }
            var products = await api.GetProductsAsync(null, null);
            BestSellers.Clear();
            foreach (var item in products.Take(8))
                BestSellers.Add(item with { HinhAnh = api.ResolveImageUrl(item.HinhAnh) });
        }
        catch (InvalidOperationException)
        {
            CustomerName = "Khách hàng";
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể tải trang chủ", exception.Message, "Đóng");
        }
    }

    private static string GreetingText() => DateTime.Now.Hour switch
    {
        < 11 => "Chào buổi sáng",
        < 18 => "Chào buổi chiều",
        _ => "Chào buổi tối"
    };
}
