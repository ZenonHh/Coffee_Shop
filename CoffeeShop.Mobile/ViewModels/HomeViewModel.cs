using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.Views;

namespace CoffeeShop.Mobile.ViewModels;

public partial class HomeViewModel(ApiService api, CartService cart, FavoriteService favorites) : ObservableObject
{
    private CancellationTokenSource? _bannerRotation;

    public ObservableCollection<ProductItem> BestSellers { get; } = [];
    public IReadOnlyList<string> Banners { get; } =
    [
        api.ResolveImageUrl("images/bg_1.jpg")!,
        api.ResolveImageUrl("images/bg_2.jpg")!,
        api.ResolveImageUrl("images/bg_3.jpg")!
    ];

    [ObservableProperty]
    private string greeting = GreetingText();

    [ObservableProperty]
    private string customerName = "Khách hàng";

    [ObservableProperty]
    private int bannerPosition;

    [RelayCommand]
    private async Task SelectDeliveryAsync()
    {
        cart.SelectedFulfillment = "Giao hàng";
        await OpenMenuAsync();
    }

    [RelayCommand]
    private async Task SelectPickupAsync()
    {
        cart.SelectedFulfillment = "Mang đi";
        await OpenMenuAsync();
    }

    [RelayCommand]
    private async Task OpenMenuAsync()
    {
        await Shell.Current.GoToAsync("//order");
        await Shell.Current.GoToAsync("menu");
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
                "Món này hiện đã hết hàng. Vui lòng chọn món khác.",
                "Đóng");
            return;
        }

        try
        {
            var detail = await api.GetProductDetailAsync(product.MaSP);
            detail = detail with { HinhAnh = api.ResolveImageUrl(detail.HinhAnh) };
            await Shell.Current.ShowPopupAsync(new ProductDetailBottomSheet(detail, cart, favorites));
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể mở món", exception.Message, "Đóng");
        }
    }

    public async Task LoadAsync()
    {
        try
        {
            var profile = await api.GetProfileAsync();
            if (profile is not null)
                CustomerName = profile.HoTen;

            var products = await api.GetProductsAsync(null, null);
            BestSellers.Clear();
            foreach (var item in products.Take(8))
                BestSellers.Add(item with { HinhAnh = api.ResolveImageUrl(item.HinhAnh) });
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể tải trang chủ", exception.Message, "Đóng");
        }
    }

    public void StartBannerRotation()
    {
        if (_bannerRotation is not null)
            return;

        _bannerRotation = new CancellationTokenSource();
        _ = RotateBannersAsync(_bannerRotation.Token);
    }

    public void StopBannerRotation()
    {
        _bannerRotation?.Cancel();
        _bannerRotation?.Dispose();
        _bannerRotation = null;
    }

    private async Task RotateBannersAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                MainThread.BeginInvokeOnMainThread(() =>
                    BannerPosition = (BannerPosition + 1) % Banners.Count);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static string GreetingText() => DateTime.Now.Hour switch
    {
        < 11 => "Chào buổi sáng",
        < 18 => "Chào buổi chiều",
        _ => "Chào buổi tối"
    };
}
