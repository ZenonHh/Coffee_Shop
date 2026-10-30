using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CoffeeShop.Mobile.Models;
using CoffeeShop.Mobile.Services;

namespace CoffeeShop.Mobile.ViewModels;

public sealed record PaymentOption(string Code, string Label);

public partial class CartViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly CartService _cart;
    public System.Collections.ObjectModel.ObservableCollection<CartLine> Items => _cart.Items;
    public decimal Subtotal => _cart.Subtotal;
    public decimal ShippingFee => IsPickup ? 0 : _cart.ShippingFee;
    public decimal Total => Subtotal + ShippingFee - VoucherDiscount;

    [ObservableProperty]
    private string? voucherCode;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private string recipientName = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private bool saveCustomerInfo;

    [ObservableProperty]
    private string selectedFulfillment = "Giao hàng";

    public bool IsPickup => SelectedFulfillment == "Mang đi";
    public bool IsDelivery => !IsPickup;
    public IReadOnlyList<string> FulfillmentOptions { get; } = ["Giao hàng", "Mang đi"];

    [ObservableProperty]
    private PaymentOption selectedPayment = new("COD", "Tiền mặt khi nhận hàng (COD)");

    [ObservableProperty]
    private decimal voucherDiscount;

    public IReadOnlyList<PaymentOption> PaymentMethods { get; } =
    [
        new("COD", "Tiền mặt khi nhận hàng (COD)"),
        new("Transfer", "Chuyển khoản QR ngân hàng")
    ];

    public string PaymentMethod => SelectedPayment.Code;

    public CartViewModel(ApiService api, CartService cart)
    {
        _api = api;
        _cart = cart;
        VoucherCode = cart.VoucherCode;
        SelectedFulfillment = cart.SelectedFulfillment;
        cart.PropertyChanged += (_, _) => RefreshTotals();
    }

    public async Task LoadCustomerDefaultsAsync()
    {
        var profile = await _api.RestoreProfileAsync();
        if (profile is null)
            return;

        if (string.IsNullOrWhiteSpace(RecipientName))
            RecipientName = profile.HoTen;
        if (string.IsNullOrWhiteSpace(Phone))
            Phone = profile.Sdt;
        if (string.IsNullOrWhiteSpace(Address))
            Address = profile.DiaChi ?? string.Empty;
        if (string.IsNullOrWhiteSpace(Email))
            Email = profile.Email ?? string.Empty;
    }

    partial void OnSelectedFulfillmentChanged(string value)
    {
        _cart.SelectedFulfillment = value;
        OnPropertyChanged(nameof(IsPickup));
        OnPropertyChanged(nameof(IsDelivery));
        RefreshTotals();
    }

    partial void OnSelectedPaymentChanged(PaymentOption value) =>
        OnPropertyChanged(nameof(PaymentMethod));
    partial void OnVoucherDiscountChanged(decimal value) => OnPropertyChanged(nameof(Total));

    [RelayCommand]
    private void Increase(CartLine item) => _cart.ChangeQuantity(item, 1);

    [RelayCommand]
    private void Decrease(CartLine item) => _cart.ChangeQuantity(item, -1);

    [RelayCommand]
    private async Task ApplyVoucherAsync()
    {
        if (string.IsNullOrWhiteSpace(VoucherCode))
            return;
        try
        {
            var result = await _api.CheckVoucherAsync(VoucherCode.Trim(), Subtotal);
            VoucherCode = result.MaCode;
            VoucherDiscount = result.TienGiamGia;
            _cart.VoucherCode = result.MaCode;
            RefreshTotals();
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Voucher chưa áp dụng được", exception.Message, "Đóng");
        }
    }

    [RelayCommand]
    private async Task CheckoutAsync()
    {
        if (Items.Count == 0)
        {
            await Shell.Current.DisplayAlertAsync("Giỏ hàng trống", "Hãy chọn món trước khi đặt hàng.", "Đóng");
            return;
        }

        try
        {
            var profile = await _api.GetProfileAsync();
            if (profile is null)
            {
                await Shell.Current.DisplayAlertAsync("Cần đăng nhập", "Vui lòng đăng nhập trong tab Tài khoản trước khi đặt hàng.", "Đóng");
                return;
            }

            var recipientName = RecipientName.Trim();
            var recipientPhone = Phone.Trim();
            if (string.IsNullOrWhiteSpace(recipientName))
            {
                await Shell.Current.DisplayAlertAsync("Thiếu họ tên", "Vui lòng nhập họ tên người nhận.", "Đóng");
                return;
            }
            if (recipientPhone.Length != 10 || recipientPhone.Any(character => !char.IsAsciiDigit(character)))
            {
                await Shell.Current.DisplayAlertAsync("Số điện thoại không hợp lệ", "Vui lòng nhập đúng 10 chữ số.", "Đóng");
                return;
            }
            if (IsDelivery && string.IsNullOrWhiteSpace(Address))
            {
                await Shell.Current.DisplayAlertAsync("Thiếu địa chỉ", "Vui lòng nhập địa chỉ giao hàng.", "Đóng");
                return;
            }

            var request = new CheckoutRequest(
                IsPickup ? "Mang đi" : "Giao hàng",
                recipientName,
                recipientPhone,
                IsPickup ? null : Address.Trim(),
                string.Join(" | ", Items.Where(item => !string.IsNullOrWhiteSpace(item.Note)).Select(item => $"{item.Product.TenSP}: {item.Note}")),
                VoucherCode,
                PaymentMethod,
                Items.Select(item => new CheckoutItemRequest(
                    item.Product.MaSP, item.Variant.MaBienThe, item.Quantity,
                    item.Toppings.Select(topping => topping.MaSP).ToList())).ToList(),
                string.IsNullOrWhiteSpace(Email) ? null : Email.Trim(),
                SaveCustomerInfo);
            var result = await _api.CheckoutAsync(request);
            _cart.Clear();
            await Shell.Current.DisplayAlertAsync("Đặt hàng thành công", $"Mã đơn #{result.MaHD} · {result.TongTien:N0} đ", "Đóng");
            await Shell.Current.Navigation.PopModalAsync();
            await Shell.Current.GoToAsync("order-history");
        }
        catch (Exception exception)
        {
            await Shell.Current.DisplayAlertAsync("Không thể đặt hàng", exception.Message, "Đóng");
        }
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(ShippingFee));
        OnPropertyChanged(nameof(Total));
    }
}
