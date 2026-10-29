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
        ActiveOrdersTab.BackgroundColor = Color.FromArgb("#C49B63");
        ActiveOrdersTab.TextColor = Colors.Black;
        PastOrdersTab.BackgroundColor = Color.FromArgb("#2A2521");
        PastOrdersTab.TextColor = Color.FromArgb("#D0C8BE");
    }

    private void OnPastOrdersClicked(object? sender, EventArgs e)
    {
        ActiveList.IsVisible = false;
        PastList.IsVisible = true;
        PastOrdersTab.BackgroundColor = Color.FromArgb("#C49B63");
        PastOrdersTab.TextColor = Colors.Black;
        ActiveOrdersTab.BackgroundColor = Color.FromArgb("#2A2521");
        ActiveOrdersTab.TextColor = Color.FromArgb("#D0C8BE");
    }
}
