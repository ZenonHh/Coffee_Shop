using CoffeeShop.Mobile.Views;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeShop.Mobile;

public partial class AppShell : Shell
{
    public AppShell(IServiceProvider services)
    {
        InitializeComponent();
        HomeTab.Content = services.GetRequiredService<HomePage>();
        MenuTab.Content = services.GetRequiredService<MenuPage>();
        VoucherTab.Content = services.GetRequiredService<VoucherPage>();
        OrderHistoryTab.Content = services.GetRequiredService<OrderHistoryPage>();
        ProfileTab.Content = services.GetRequiredService<ProfilePage>();
    }
}
