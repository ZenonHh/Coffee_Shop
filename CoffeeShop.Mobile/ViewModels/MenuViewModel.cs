using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeShop.Mobile.ViewModels;

public sealed record MenuCategory(int MaLoaiSP, string TenLoaiSP);

public partial class MenuViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly CartService _cart;
    private readonly FavoriteService _favorites;
    private readonly IServiceProvider _services;
    public ObservableCollection<MenuCategory> Categories { get; } =
    [
        new MenuCategory(0, "Tất cả")
    ];
    public ObservableCollection<ProductItem> Products { get; } = [];
    public int CartCount => _cart.ItemCount;
    public decimal CartTotal => _cart.Subtotal;
    public bool IsCartVisible => _cart.ItemCount > 0;

    public MenuViewModel(ApiService api, CartService cart, FavoriteService favorites, IServiceProvider services)
    {
        _api = api;
        _cart = cart;
        _favorites = favorites;
        _services = services;
        cart.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CartCount));
            OnPropertyChanged(nameof(CartTotal));
            OnPropertyChanged(nameof(IsCartVisible));
        };
    }

    [ObservableProperty]
    private MenuCategory? selectedCategory;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            var categories = await _api.GetCategoriesAsync();
            Categories.Clear();
            Categories.Add(new MenuCategory(0, "Tất cả"));
            foreach (var category in categories)
                Categories.Add(new MenuCategory(category.MaLoaiSP, category.TenLoaiSP));
            SelectedCategory ??= Categories[0];
            await RefreshProductsAsync();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể tải thực đơn", exception.Message, "Đóng");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshProductsAsync()
    {
        try
        {
            var products = await _api.GetProductsAsync(
                SelectedCategory?.MaLoaiSP, SearchText, CancellationToken.None);
            Products.Clear();
            foreach (var product in products)
                Products.Add(product with { HinhAnh = _api.ResolveImageUrl(product.HinhAnh) });
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể tải thực đơn", exception.Message, "Đóng");
        }
    }

    [RelayCommand]
    private async Task OpenProductAsync(ProductItem? product)
    {
        if (product is null)
            return;
        if (product.IsOutOfStock)
        {
            await Shell.Current.DisplayAlertAsync(
                "Đã hết hàng",
                "Rất tiếc! Món này hiện tại đã hết hàng. Quý khách vui lòng chọn món khác nhé!",
                "Đóng");
            return;
        }

        try
        {
            var detail = await _api.GetProductDetailAsync(product.MaSP);
            detail = detail with { HinhAnh = _api.ResolveImageUrl(detail.HinhAnh) };
            await Shell.Current.ShowPopupAsync(new ProductDetailBottomSheet(detail, _cart, _favorites));
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể mở món", exception.Message, "Đóng");
        }
    }

    [RelayCommand]
    private async Task OpenCartAsync()
    {
        await Shell.Current.Navigation.PushModalAsync(_services.GetRequiredService<CartPage>());
    }

    partial void OnSelectedCategoryChanged(MenuCategory? value)
    {
        if (value is not null)
            _ = RefreshProductsAsync();
    }
}
