using CoffeeShop.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeShop.Mobile;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        HomeTab.Content = services.GetRequiredService<HomePage>();
        VoucherTab.Content = services.GetRequiredService<VoucherPage>();
        OrderTab.Content = services.GetRequiredService<OrderModePage>();
        AccountTab.Content = services.GetRequiredService<AccountPage>();
        Routing.RegisterRoute("menu", typeof(MenuPage));
        Routing.RegisterRoute("order-history", typeof(OrderHistoryPage));
        Routing.RegisterRoute("feedback", typeof(FeedbackPage));
        Routing.RegisterRoute("favorites", typeof(FavoritesPage));
    }
}
