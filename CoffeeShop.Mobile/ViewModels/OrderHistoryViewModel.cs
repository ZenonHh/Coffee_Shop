using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.ViewModels;

public partial class OrderHistoryViewModel(ApiService api) : ObservableObject
{
    public ObservableCollection<OrderHistoryItem> ActiveOrders { get; } = [];
    public ObservableCollection<OrderHistoryItem> PastOrders { get; } = [];

    [ObservableProperty]
    private bool showingActive = true;

    [ObservableProperty]
    private bool isBusy;

    public async Task LoadAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            var profile = await api.GetProfileAsync();
            if (profile is null)
            {
                ActiveOrders.Clear();
                PastOrders.Clear();
                return;
            }

            var orders = await api.GetOrderHistoryAsync(profile.TenDangNhap);
            ActiveOrders.Clear();
            PastOrders.Clear();
            foreach (var order in orders)
            {
                if (order.TrangThai is "Chờ xác nhận" or "Đã xác nhận" or "Đang giao")
                    ActiveOrders.Add(order);
                else
                    PastOrders.Add(order);
            }
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể tải đơn hàng", exception.Message, "Đóng");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
