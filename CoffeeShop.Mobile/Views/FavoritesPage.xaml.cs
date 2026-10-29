using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.Views;

public partial class FavoritesPage : ContentPage
{
    private readonly ApiService _api;
    private readonly FavoriteService _favorites;
    private readonly CartService _cart;
    private readonly ObservableCollection<ProductItem> _products = [];

    public FavoritesPage(ApiService api, FavoriteService favorites, CartService cart)
    {
        InitializeComponent();
        _api = api;
        _favorites = favorites;
        _cart = cart;
        BindingContext = _products;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var ids = _favorites.GetProductIds();
            var products = await _api.GetProductsAsync(null, null);
            _products.Clear();
            foreach (var product in products.Where(product => ids.Contains(product.MaSP)))
                _products.Add(product with { HinhAnh = _api.ResolveImageUrl(product.HinhAnh) });
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể tải món yêu thích", exception.Message, "Đóng");
        }
    }

    private void OnRemoveClicked(object? sender, EventArgs e)
    {
        if (sender is not Button { CommandParameter: ProductItem product })
            return;
        _favorites.Remove(product.MaSP);
        _products.Remove(product);
    }

    private async void OnProductTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not ProductItem product)
            return;
        if (product.IsOutOfStock)
        {
            await DisplayAlertAsync("Đã hết hàng", "Món này hiện đã hết hàng.", "Đóng");
            return;
        }

        try
        {
            var detail = await _api.GetProductDetailAsync(product.MaSP);
            detail = detail with { HinhAnh = _api.ResolveImageUrl(detail.HinhAnh) };
            await Shell.Current.ShowPopupAsync(new ProductDetailBottomSheet(detail, _cart, _favorites));
            if (!_favorites.Contains(product.MaSP))
                _products.Remove(product);
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể mở món", exception.Message, "Đóng");
        }
    }
}
