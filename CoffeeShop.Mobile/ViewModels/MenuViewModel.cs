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
    public int CartItemCount => _cart.ItemCount;
    public bool HasItemsInCart => _cart.ItemCount > 0;
    public string TotalPriceFormatted => $"{_cart.Subtotal:N0} đ";

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
            OnPropertyChanged(nameof(CartItemCount));
            OnPropertyChanged(nameof(HasItemsInCart));
            OnPropertyChanged(nameof(TotalPriceFormatted));
        };
    }

    [ObservableProperty]
    private MenuCategory? selectedCategory;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            var categories = await _api.GetCategoriesAsync();
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Categories.Clear();
                Categories.Add(new MenuCategory(0, "Tất cả"));
                foreach (var category in categories.Where(category => category is not null))
                    Categories.Add(new MenuCategory(category.MaLoaiSP, category.TenLoaiSP ?? string.Empty));

                _isLoadingCategories = true;
                SelectedCategory ??= Categories[0];
                _isLoadingCategories = false;
            });
            await RefreshProductsAsync();
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"[MENU ERROR]: {exception}");
            await Shell.Current.DisplayAlertAsync(
                "Không thể tải thực đơn",
                "Không thể tải thực đơn. Vui lòng kiểm tra kết nối và thử lại.",
                "Đóng");
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
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Products.Clear();
                foreach (var product in products.Where(product => product is not null))
                {
                    Products.Add(product with
                    {
                        TenSP = product.TenSP ?? string.Empty,
                        HinhAnh = _api.ResolveImageUrl(product.HinhAnh),
                        TenLoaiSP = product.TenLoaiSP ?? string.Empty
                    });
                }
            });
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"[MENU ERROR]: {exception}");
            await Shell.Current.DisplayAlertAsync(
                "Không thể tải thực đơn",
                "Không thể tải thực đơn. Vui lòng kiểm tra kết nối và thử lại.",
                "Đóng");
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
        if (value is not null && !_isLoadingCategories)
            _ = RefreshProductsAsync();
    }

    private bool _isLoadingCategories;
}
