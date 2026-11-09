using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.ViewModels;

namespace CoffeeShop.Mobile.Views;

public partial class CartPage : ContentPage
{
    private readonly CartViewModel _viewModel;

    public CartPage(CartViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.StartListeningToCart();
        try
        {
            await _viewModel.LoadCustomerDefaultsAsync();
            await _viewModel.RefreshCartUpsellAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể tải thông tin nhận hàng", exception.Message, "Đóng");
        }
    }

    protected override void OnDisappearing()
    {
        _viewModel.StopListeningToCart();
        base.OnDisappearing();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (Navigation.ModalStack.Count > 0)
            await Navigation.PopModalAsync();
    }
}
