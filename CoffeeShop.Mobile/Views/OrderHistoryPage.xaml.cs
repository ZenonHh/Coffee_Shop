using CoffeeShop.Mobile.ViewModels;

namespace CoffeeShop.Mobile.Views;

public partial class OrderHistoryPage : ContentPage
{
    private readonly OrderHistoryViewModel _viewModel;

    public OrderHistoryPage(OrderHistoryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }

    private void OnActiveOrdersClicked(object? sender, EventArgs e)
    {
        ActiveList.IsVisible = true;
        PastList.IsVisible = false;
    }

    private void OnPastOrdersClicked(object? sender, EventArgs e)
    {
        ActiveList.IsVisible = false;
        PastList.IsVisible = true;
    }
}
