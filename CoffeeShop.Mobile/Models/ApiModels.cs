using CommunityToolkit.Mvvm.ComponentModel;

namespace CoffeeShop.Mobile.Models;

public sealed record CustomerProfile(
    int MaKH, string TenDangNhap, string HoTen, string Sdt, string? Email,
    string? DiaChi, int DiemTichLuy, string HangTV);

public sealed record UpdateProfileRequest(string HoTen, string Sdt, string? Email, string? DiaChi);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record AuthResponse(string AccessToken, CustomerProfile Customer);
public sealed record UsernameCheckResponse(bool Exists);
public sealed record PointHistoryItem(
    int MaLSD, int? MaHD, int DiemThayDoi, string? LyDo, DateTimeOffset NgayThucHien);
public sealed record CategoryItem(int MaLoaiSP, string TenLoaiSP);
public sealed record ProductItem(
    int MaSP, string TenSP, string? HinhAnh, string TenLoaiSP, decimal? GiaTu, bool IsOutOfStock);
public sealed record ProductVariantItem(int MaBienThe, string KichCo, decimal GiaBan);
public sealed record ToppingItem(int MaSP, string TenSP, int SoLuongTon, decimal GiaBan);
public sealed record ProductDetail(
    int MaSP, string TenSP, string? HinhAnh, string TenLoaiSP, int SoLuongTon,
    IReadOnlyList<ProductVariantItem> Variants, IReadOnlyList<ToppingItem> Toppings);
public sealed record VoucherItem(
    int MaVoucher, string MaCode, string? TenChuongTrinh, string PhanLoaiGiam, decimal GiaTriGiam,
    decimal DonToiThieu, DateTime? NgayKetThuc)
{
    public string DisplayTitle => string.IsNullOrWhiteSpace(TenChuongTrinh) ? MaCode : TenChuongTrinh;
    public string DisplayDiscount => PhanLoaiGiam.Equals("PERCENT", StringComparison.OrdinalIgnoreCase)
        ? $"Giảm {GiaTriGiam:N0}%"
        : $"Giảm {GiaTriGiam:N0} đ";
    public string ExpiryText => NgayKetThuc.HasValue
        ? $"Hạn đến {NgayKetThuc.Value:dd/MM/yyyy}"
        : "Không thời hạn";
}
public sealed record VoucherCheckResult(int MaVoucher, string MaCode, decimal TienGiamGia);
public sealed record VoucherCheckRequest(string MaCode, decimal TamTinh);
public sealed record VoucherCheckResponse(int MaVoucher, string MaCode, decimal TienGiamGia);
public sealed record CheckoutItemRequest(int MaSP, int MaBienThe, int SoLuong, List<int> ToppingIds);
public sealed record CheckoutRequest(
    string LoaiDonHang, string? HoTenNguoiNhan, string? SdtNguoiNhan, string? DiaChiGiao,
    string? GhiChu, string? MaCode, string PhuongThucThanhToan, List<CheckoutItemRequest> Items,
    string? EmailNguoiNhan = null, bool LuuThongTinMacDinh = false);
public sealed record CheckoutResponse(
    int MaHD, string TrangThai, decimal TamTinh, decimal TienGiamGia, int PhiGiao, decimal TongTien);
public sealed record OrderHistoryLine(
    string TenSP, string KichCo, int SoLuong, decimal ThanhTien, IReadOnlyList<string> Toppings);
public sealed record OrderHistoryItem(
    int MaHD, DateTime NgayTao, decimal TongTien, string TrangThai, string? LyDoHuy,
    string LoaiDonHang, IReadOnlyList<OrderHistoryLine> Items)
{
    public bool HasCancelReason => !string.IsNullOrWhiteSpace(LyDoHuy);
    public string TrackerText => TrangThai switch
    {
        "Đã xác nhận" => "✓ Đã đặt  →  ● Đã xác nhận  →  ○ Đang giao  →  ○ Hoàn thành",
        "Đang giao" => "✓ Đã đặt  →  ✓ Đã xác nhận  →  ● Đang giao  →  ○ Hoàn thành",
        "Hoàn thành" => "✓ Đã đặt  →  ✓ Đã xác nhận  →  ✓ Đang giao  →  ✓ Hoàn thành",
        "Đã hủy" => "Đơn hàng đã hủy",
        _ => "● Đã đặt  →  ○ Đã xác nhận  →  ○ Đang giao  →  ○ Hoàn thành"
    };
}

public sealed partial class CartLine : ObservableObject
{
    public required ProductDetail Product { get; init; }
    public required ProductVariantItem Variant { get; init; }
    public required List<ToppingItem> Toppings { get; init; }
    public required string Note { get; init; }
    [ObservableProperty]
    private int quantity = 1;
    public decimal UnitPrice => Variant.GiaBan + Toppings.Sum(topping => topping.GiaBan);
    public decimal LineTotal => UnitPrice * Quantity;
    public string SizeAndToppings =>
        $"{Variant.KichCo} · {string.Join(", ", Toppings.Select(topping => topping.TenSP))}".TrimEnd(' ', '·');

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(LineTotal));
    }
}
