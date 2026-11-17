using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.Views;

public partial class AddressBookPage : ContentPage
{
    private readonly ApiService _api;
    private bool _isBusy;

    public AddressBookPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            var profile = await _api.GetProfileAsync();
            if (profile is null)
                throw new InvalidOperationException("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            AddressEditor.Text = profile.DiaChi ?? string.Empty;
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể tải sổ địa chỉ", exception.Message, "Đóng");
        }
    }

    private async void OnSaveAddressClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
            return;

        _isBusy = true;
        SaveButton.IsEnabled = false;
        try
        {
            await _api.UpdateAddressAsync(new UpdateAddressRequest(AddressEditor.Text));
            await DisplayAlertAsync("Thành công", "Địa chỉ nhận hàng đã được cập nhật.", "Đóng");
        }
        catch (Exception exception)
        {
            await DisplayAlertAsync("Không thể cập nhật địa chỉ", exception.Message, "Đóng");
        }
        finally
        {
            _isBusy = false;
            SaveButton.IsEnabled = true;
        }
    }
}
