using CoffeeShop.Mobile.ViewModels;

namespace CoffeeShop.Mobile.Views;

public partial class SplashPage : ContentPage
{
    private readonly SplashViewModel _viewModel;
    private readonly IServiceProvider _services;
    private bool _initialized;

    public SplashPage(SplashViewModel viewModel, IServiceProvider services)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
        _services = services;
        _viewModel.StartRequested += OnStartRequested;
        _viewModel.SessionRestored += OnSessionRestored;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_initialized)
            return;
        _initialized = true;
        await _viewModel.CheckSessionAsync();
    }

    private async void OnStartRequested(object? sender, EventArgs e)
    {
        var authPage = _services.GetRequiredService<AuthBottomSheet>();
        await Navigation.PushModalAsync(authPage);
    }

    private async void OnSessionRestored(object? sender, EventArgs e) =>
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
