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

public sealed partial class ProductVariantSelection(ProductVariantItem item) : ObservableObject
{
    public ProductVariantItem Item { get; } = item;

    [ObservableProperty]
    private bool isSelected;
}

public partial class ProductDetailViewModel : ObservableObject
{
    private readonly CartService _cart;
    private readonly FavoriteService _favorites;
    public ProductDetail Product { get; }
    public ObservableCollection<ProductVariantSelection> Variants { get; } = [];
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

    [ObservableProperty]
    private int quantity = 1;

    [ObservableProperty]
    private bool isFavorite;

    public decimal TotalPrice => UnitPrice * Quantity;
    public bool CanAddToCart => SelectedVariant is not null && Product.SoLuongTon > 0;
    public decimal UnitPrice =>
        (SelectedVariant?.GiaBan ?? 0m) + Toppings.Where(topping => topping.IsSelected).Sum(topping => topping.Item.GiaBan);
    public string FavoriteButtonText => IsFavorite ? "♥ Đã lưu yêu thích" : "♡ Thêm vào yêu thích";

    public ProductDetailViewModel(ProductDetail product, CartService cart, FavoriteService favorites)
    {
        Product = product;
        _cart = cart;
        _favorites = favorites;
        IsFavorite = favorites.Contains(product.MaSP);
        SelectedVariant = product.Variants.FirstOrDefault();
        foreach (var variant in product.Variants)
            Variants.Add(new ProductVariantSelection(variant)
            {
                IsSelected = variant.MaBienThe == SelectedVariant?.MaBienThe
            });
        foreach (var topping in product.Toppings)
            Toppings.Add(new ToppingSelection(topping));
        foreach (var topping in Toppings)
            topping.PropertyChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(UnitPrice));
                OnPropertyChanged(nameof(TotalPrice));
            };
    }

    partial void OnSelectedVariantChanged(ProductVariantItem? value)
    {
        NotifyPriceChanged();
        OnPropertyChanged(nameof(CanAddToCart));
    }

    partial void OnIsFavoriteChanged(bool value) => OnPropertyChanged(nameof(FavoriteButtonText));

    [RelayCommand]
    private void SelectVariant(ProductVariantSelection? variant)
    {
        if (variant is null)
            return;

        foreach (var option in Variants)
            option.IsSelected = option == variant;
        SelectedVariant = variant.Item;
    }

    [RelayCommand]
    private void DecreaseQuantity()
    {
        if (Quantity > 1)
            Quantity--;
    }

    [RelayCommand]
    private void IncreaseQuantity()
    {
        if (Quantity < 50)
            Quantity++;
    }

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
            Quantity = Quantity,
            Note = string.Join("; ", new[]
            {
                string.IsNullOrWhiteSpace(SugarLevel) ? null : $"Đường {SugarLevel}",
                string.IsNullOrWhiteSpace(IceLevel) ? null : $"Đá {IceLevel}",
                string.IsNullOrWhiteSpace(Note) ? null : Note.Trim()
            }.Where(text => text is not null))
        });
    }

    [RelayCommand]
    private void ToggleFavorite()
    {
        IsFavorite = _favorites.Toggle(Product.MaSP);
    }

    partial void OnQuantityChanged(int value) => OnPropertyChanged(nameof(TotalPrice));

    private void NotifyPriceChanged()
    {
        OnPropertyChanged(nameof(UnitPrice));
        OnPropertyChanged(nameof(TotalPrice));
    }
}
