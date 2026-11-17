using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeShop.Mobile.Views;

public partial class SplashPage : ContentPage
{
    private readonly AuthViewModel _viewModel;
    private readonly ApiService _api;
    private readonly IServiceProvider _services;
    private bool _initialized;

    public SplashPage(AuthViewModel viewModel, ApiService api, IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _api = api;
        _services = services;
        _viewModel.Authenticated += OnAuthenticated;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_initialized)
            return;
        _initialized = true;

        try
        {
            var profile = await _api.RestoreProfileAsync();
            if (profile is not null)
            {
                await NavigateToAppShellAsync();
            }
        }
        catch
        {
            // Chờ người dùng đăng nhập tại trang
        }
    }

    private async void OnAuthenticated(object? sender, EventArgs e) =>
        await NavigateToAppShellAsync();

    private async Task NavigateToAppShellAsync()
    {
        var window = Application.Current?.Windows.FirstOrDefault();
        if (window is not null)
        {
            var shell = _services.GetRequiredService<AppShell>();
            window.Page = shell;
            await shell.GoToAsync("//home");
        }
    }
}