using CommunityToolkit.Maui;
using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.ViewModels;
using CoffeeShop.Mobile.Views;

namespace CoffeeShop.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

        builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri("http://10.0.2.2:5160/") });
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<CartService>();
        builder.Services.AddSingleton<FavoriteService>();
        builder.Services.AddSingleton<MenuViewModel>();
        builder.Services.AddTransient<SplashViewModel>();
        builder.Services.AddTransient<AuthViewModel>();
        builder.Services.AddTransient<AccountViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<CartViewModel>();
        builder.Services.AddTransient<FeedbackViewModel>();
        builder.Services.AddTransient<VoucherViewModel>();
        builder.Services.AddTransient<OrderHistoryViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<SplashPage>();
        builder.Services.AddTransient<AuthBottomSheet>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<MenuPage>();
        builder.Services.AddTransient<VoucherPage>();
        builder.Services.AddTransient<OrderModePage>();
        builder.Services.AddTransient<AccountPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<FeedbackPage>();
        builder.Services.AddTransient<FavoritesPage>();
        builder.Services.AddTransient<OrderHistoryPage>();
        builder.Services.AddTransient<CartPage>();

        return builder.Build();
    }
}
