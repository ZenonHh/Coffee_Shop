using CoffeeShop.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeShop.Mobile;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        HomeTab.ContentTemplate = new DataTemplate(() => services.GetRequiredService<HomePage>());
        VoucherTab.ContentTemplate = new DataTemplate(() => services.GetRequiredService<VoucherPage>());
        OrderTab.ContentTemplate = new DataTemplate(() => services.GetRequiredService<MenuPage>());
        AccountTab.ContentTemplate = new DataTemplate(() => services.GetRequiredService<AccountPage>());

        Routing.RegisterRoute("menu", typeof(MenuPage));
        Routing.RegisterRoute(nameof(MenuPage), typeof(MenuPage));
        Routing.RegisterRoute("cart", typeof(CartPage));
        Routing.RegisterRoute(nameof(CartPage), typeof(CartPage));
        Routing.RegisterRoute("order-mode", typeof(OrderModePage));
        Routing.RegisterRoute(nameof(OrderModePage), typeof(OrderModePage));
        Routing.RegisterRoute("profile", typeof(ProfilePage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Routing.RegisterRoute("order-history", typeof(OrderHistoryPage));
        Routing.RegisterRoute(nameof(OrderHistoryPage), typeof(OrderHistoryPage));
        Routing.RegisterRoute("feedback", typeof(FeedbackPage));
        Routing.RegisterRoute(nameof(FeedbackPage), typeof(FeedbackPage));
        Routing.RegisterRoute("favorites", typeof(FavoritesPage));
        Routing.RegisterRoute(nameof(FavoritesPage), typeof(FavoritesPage));
    }
}
