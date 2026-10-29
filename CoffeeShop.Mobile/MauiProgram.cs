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
        builder.Services.AddSingleton<MenuViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<OrderHistoryViewModel>();
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<MenuPage>();
        builder.Services.AddTransient<VoucherPage>();
        builder.Services.AddTransient<OrderHistoryPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<CartPage>();

        return builder.Build();
    }
}
