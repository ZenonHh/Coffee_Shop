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
        await Shell.Current.GoToAsync("menu");
    }

    private async void OnDeliveryClicked(object? sender, EventArgs e)
    {
        _cart.SelectedFulfillment = "Giao hàng";
        await Shell.Current.GoToAsync("menu");
    }
}
