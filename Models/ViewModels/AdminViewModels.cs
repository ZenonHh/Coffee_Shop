using CoffeeShop.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public List<AdminDashboardOrderViewModel> RecentOrders { get; set; } = new();
        public List<Product> TopProducts { get; set; } = new();
    }

    public class AdminDashboardOrderViewModel
    {
        public int MaHD { get; set; }
        public string HoTenNguoiNhan { get; set; } = string.Empty;
        public string SdtNguoiNhan { get; set; } = string.Empty;
        public string? DiaChiGiao { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }

    public class PurchaseCreateViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn nhà cung cấp.")]
        public int MaNCC { get; set; }

        [MaxLength(255)]
        public string? GhiChu { get; set; }

        [MinLength(1, ErrorMessage = "Phiếu nhập cần ít nhất một dòng hàng.")]
        public List<PurchaseLineInput> Details { get; set; } = new() { new() };
    }

    public class PurchaseLineInput
    {
        [Range(1, int.MaxValue)]
        public int? MaSP { get; set; }
        public decimal? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
    }

    public class PurchaseProductOptionViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; } = string.Empty;
    }

    public class SupplierOptionViewModel
    {
        public int MaNCC { get; set; }
        public string TenNCC { get; set; } = string.Empty;
    }

    public class SupplierListItemViewModel
    {
        public int MaNCC { get; set; }
        public string TenNCC { get; set; } = string.Empty;
        public string? Sdt { get; set; }
        public string? DiaChi { get; set; }
        public bool? TrangThai { get; set; }
    }

    public class PurchaseListItemViewModel
    {
        public int MaPN { get; set; }
        public string TenNCC { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public DateTime? NgayNhap { get; set; }
        public decimal? TongTien { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }

    public class PurchaseDetailsViewModel
    {
        public int MaPN { get; set; }
        public string TenNCC { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public string? NguoiDuyet { get; set; }
        public DateTime? NgayNhap { get; set; }
        public DateTime? NgayDuyet { get; set; }
        public decimal? TongTien { get; set; }
        public string? GhiChu { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public List<PurchaseDetailsLineViewModel> Details { get; set; } = new();
    }

    public class PurchaseDetailsLineViewModel
    {
        public string TenSanPham { get; set; } = string.Empty;
        public decimal SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class InternalAccountCreateViewModel
    {
        [Required, StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Range(2, 4)]
        public int MaVaiTro { get; set; } = 3;

        [Required, MinLength(8), StringLength(72)]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;
    }

    public class RoleOptionViewModel
    {
        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; } = string.Empty;
    }

    public class InternalAccountListItemViewModel
    {
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string TenVaiTro { get; set; } = string.Empty;
        public bool TrangThai { get; set; }
    }

    public class AdminProductListItemViewModel
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }
        public string TenLoaiSP { get; set; } = string.Empty;
        public int SoLuongTon { get; set; }
        public bool TrangThai { get; set; }
        public List<AdminProductVariantViewModel> Variants { get; set; } = new();
    }

    public class AdminProductVariantViewModel
    {
        public string KichCo { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
    }

    public class AdminOrderListItemViewModel
    {
        public int MaHD { get; set; }
        public string HoTenNguoiNhan { get; set; } = string.Empty;
        public string SdtNguoiNhan { get; set; } = string.Empty;
        public string? DiaChiGiao { get; set; }
        public string? GhiChu { get; set; }
        public int TamTinh { get; set; }
        public decimal TienGiamGia { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public int SoLuongMon { get; set; }
        public List<AdminOrderLineViewModel> Details { get; set; } = new();
    }

    public class AdminOrderLineViewModel
    {
        public string TenSP { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class VoucherListItemViewModel
    {
        public int MaVoucher { get; set; }
        public string MaCode { get; set; } = string.Empty;
        public string PhanLoaiGiam { get; set; } = string.Empty;
        public decimal GiaTriGiam { get; set; }
        public string? TenChuongTrinh { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public decimal DonToiThieu { get; set; }
        public decimal? GiamToiDa { get; set; }
        public int SoLuongToiDa { get; set; }
        public int SoLuongDaDung { get; set; }
        public bool TrangThai { get; set; }
    }

    public class SurveyListItemViewModel
    {
        public int MaKhaoSat { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int AnswerCount { get; set; }
        public List<SurveyQuestionListItemViewModel> Questions { get; set; } = new();
    }

    public class SurveyQuestionListItemViewModel
    {
        public int MaCauHoi { get; set; }
        public string NoiDungCauHoi { get; set; } = string.Empty;
        public SurveyAnswerCountViewModel Answers { get; set; } = new();
    }

    public class SurveyAnswerCountViewModel
    {
        public int Count { get; set; }
    }
}
