using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.Views;

public partial class OrderModePage : ContentPage
{
    private readonly CartService _cart;

    public OrderModePage(CartService cart)
    {
        InitializeComponent();
        _cart = cart;
    }

    private async void OnPickupClicked(object? sender, EventArgs e)
    {
        _cart.SelectedFulfillment = "Mang đi";
        await NavigateToMenuAsync();
    }

    private async void OnDeliveryClicked(object? sender, EventArgs e)
    {
        _cart.SelectedFulfillment = "Giao hàng";
        await NavigateToMenuAsync();
    }

    private static async Task NavigateToMenuAsync()
    {
        try
        {
            await Shell.Current.GoToAsync("//order");
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"[NAVIGATION ERROR]: {exception}");
            await Shell.Current.DisplayAlertAsync(
                "Không thể mở thực đơn", "Vui lòng thử lại.", "Đóng");
        }
    }
}
