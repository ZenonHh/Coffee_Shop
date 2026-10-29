using CoffeeShop.Mobile.ViewModels;

namespace CoffeeShop.Mobile.Views;

public partial class HomePage : ContentPage
{
    private readonly HomeViewModel _viewModel;

    public HomePage(HomeViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.StartBannerRotation();
        await _viewModel.LoadAsync();
    }

    protected override void OnDisappearing()
    {
        _viewModel.StopBannerRotation();
        base.OnDisappearing();
    }
}
