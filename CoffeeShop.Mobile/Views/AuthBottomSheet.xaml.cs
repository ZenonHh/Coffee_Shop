using CoffeeShop.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeShop.Mobile.Views;

public partial class AuthBottomSheet : ContentPage
{
    private readonly AuthViewModel _viewModel;
    private readonly IServiceProvider _services;

    public AuthBottomSheet(AuthViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _services = services;
        _viewModel.Authenticated += OnAuthenticated;
    }

    private async void OnAuthenticated(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
        {
            var shell = _services.GetRequiredService<AppShell>();
            window.Page = shell;
            await shell.GoToAsync("//home");
        }
    }

    private async void OnCloseClicked(object? sender, EventArgs e) =>
        await Navigation.PopModalAsync();
}
