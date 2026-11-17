using CoffeeShop.Mobile.Services;
using CoffeeShop.Mobile.ViewModels;

namespace CoffeeShop.Mobile.Views;

public partial class AccountPage : ContentPage
{
    private readonly ApiService _api;

    public AccountPage(ApiService api, AccountViewModel viewModel)
    {
        InitializeComponent();
        _api = api;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await LoadProfileAsync();
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể tải hồ sơ", exception.Message, "Đóng");
        }
    }

    private async Task LoadProfileAsync()
    {
        var profile = await _api.GetProfileAsync();
        if (profile is null)
            throw new InvalidOperationException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
        CustomerName.Text = profile.HoTen;
        CustomerPhone.Text = profile.Sdt;
        MemberRank.Text = $"{profile.HangTV.ToUpperInvariant()} MEMBER";
        MemberPoints.Text = $"{profile.DiemTichLuy:N0} điểm";
    }
}
