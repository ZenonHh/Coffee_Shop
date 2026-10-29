using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.Views;

public partial class VoucherPage : ContentPage
{
    private readonly ApiService _api;
    private readonly CartService _cart;

    public VoucherPage(ApiService api, CartService cart)
    {
        InitializeComponent();
        _api = api;
        _cart = cart;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            VoucherList.ItemsSource = await _api.GetVouchersAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể tải ưu đãi", exception.Message, "Đóng");
        }
    }

    private async void OnUseVoucherClicked(object? sender, EventArgs e)
    {
        if (sender is Button { BindingContext: VoucherItem voucher })
        {
            _cart.VoucherCode = voucher.MaCode;
            await Shell.Current.GoToAsync("//menu");
        }
    }
}
