using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.ViewModels;

namespace CoffeeShop.Mobile.Views;

public partial class CartPage : ContentPage
{
    public CartPage(ApiService api, CartService cart)
    {
        InitializeComponent();
        BindingContext = new CartViewModel(api, cart);
    }
}
