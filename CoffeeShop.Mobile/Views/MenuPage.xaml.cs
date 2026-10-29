using CoffeeShop.Mobile.ViewModels;

namespace CoffeeShop.Mobile.Views;

public partial class MenuPage : ContentPage
{
    private readonly MenuViewModel _viewModel;
    private CancellationTokenSource? _searchDelay;

    public MenuPage(MenuViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        MenuSearch.TextChanged += OnSearchTextChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_viewModel.Categories.Count == 1)
            await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        _searchDelay?.Cancel();
        _searchDelay?.Dispose();
        _searchDelay = new CancellationTokenSource();
        try
        {
            await Task.Delay(300, _searchDelay.Token);
            await _viewModel.RefreshProductsCommand.ExecuteAsync(null);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async void OnSearchPressed(object? sender, EventArgs e) =>
        await _viewModel.RefreshProductsCommand.ExecuteAsync(null);
}
