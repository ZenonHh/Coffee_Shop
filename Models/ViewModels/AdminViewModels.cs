using CoffeeShop.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int PendingOrderCount { get; set; }
        public int TotalCustomerCount { get; set; }
        public int PendingReviewCount { get; set; }
        public int PendingContactCount { get; set; }
        public int ActiveSurveyCount { get; set; }

        public List<DashboardDailyRevenueViewModel> DailyRevenues { get; set; } = new();
        public List<DashboardOrderStatusViewModel> OrderStatusesList { get; set; } = new();
        public List<DashboardTopProductViewModel> TopSellingProducts { get; set; } = new();
    }

    public class DashboardDailyRevenueViewModel
    {
        public string Date { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class DashboardOrderStatusViewModel
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public string Color { get; set; } = string.Empty;
    }

    public class DashboardTopProductViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CrmSegmentViewModel
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
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
        public int TotalPurchases { get; set; }
        public decimal TotalPurchasedAmount { get; set; }
    }

    public class SupplierManagementViewModel
    {
        public string ActiveTab { get; set; } = "suppliers";
        public List<SupplierListItemViewModel> Suppliers { get; set; } = new();
        public List<PurchaseListItemViewModel> Purchases { get; set; } = new();
        public int SupplierCount { get; set; }
        public int PurchaseCount { get; set; }
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

        [Range(1, 4)]
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

    public class CustomerAccountListItemViewModel
    {
        public int MaKH { get; set; }
        public string? TaiKhoan { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Sdt { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int DiemTichLuy { get; set; }
        public string HangTV { get; set; } = "Đồng";
        public DateTime NgayTao { get; set; }
        public bool TrangThai { get; set; } = true;
        public int TotalOrders { get; set; }
    }

    public class AdminCustomerEditViewModel
    {
        public int MaKH { get; set; }

        [StringLength(50, ErrorMessage = "Tên đăng nhập tối đa 50 ký tự.")]
        public string? TaiKhoan { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string Sdt { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự.")]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [StringLength(72, ErrorMessage = "Mật khẩu tối đa 72 ký tự.")]
        [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
        [DataType(DataType.Password)]
        public string? MatKhauMoi { get; set; }
    }

    public class AdminAccountIndexViewModel
    {
        public List<InternalAccountListItemViewModel> InternalAccounts { get; set; } = new();
        public int InternalAccountCount { get; set; }
    }

    public class CustomerManagementViewModel
    {
        public List<CustomerAccountListItemViewModel> Customers { get; set; } = new();
        public int CustomerAccountCount { get; set; }
        public int ActiveCustomerCount { get; set; }
        public int LockedCustomerCount { get; set; }
        public string? Search { get; set; }
        public string Status { get; set; } = "all";
        public string? Tier { get; set; }
        public string? AgeGroup { get; set; }
        public string? Interest { get; set; }
        public List<string> InterestOptions { get; set; } = new();
    }

    public class CustomerDetailsViewModel
    {
        public int MaKH { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string? TaiKhoan { get; set; }
        public string Sdt { get; set; } = string.Empty;
        public string? Email { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string HangTV { get; set; } = string.Empty;
        public int DiemTichLuy { get; set; }
        public List<string> Interests { get; set; } = new();
        public List<CustomerDetailsOrderViewModel> Orders { get; set; } = new();
        public List<CustomerDetailsFeedbackViewModel> Feedbacks { get; set; } = new();
        public List<CustomerDetailsSurveyViewModel> Surveys { get; set; } = new();
    }

    public class CustomerDetailsOrderViewModel
    {
        public int MaHD { get; set; }
        public DateTime NgayTao { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }

    public class CustomerDetailsFeedbackViewModel
    {
        public string? ProductName { get; set; }
        public int? Rating { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? AdminReply { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerDetailsSurveyViewModel
    {
        public string Title { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
    }

    public class PaginationViewModel
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public string PageParameter { get; set; } = "page";
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
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

    public class AdminProductEditViewModel
    {
        public int MaSP { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự.")]
        public string TenSP { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại sản phẩm.")]
        public int MaLoaiSP { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn không được âm.")]
        public int SoLuongTon { get; set; }

        public bool TrangThai { get; set; }

        public string? HinhAnh { get; set; }

        public IFormFile? HinhAnhMoi { get; set; }

        public List<AdminProductCategoryOptionViewModel> Categories { get; set; } = new();
        public List<AdminProductVariantEditViewModel> Variants { get; set; } = new();
    }

    public class AdminProductCategoryOptionViewModel
    {
        public int MaLoaiSP { get; set; }
        public string TenLoaiSP { get; set; } = string.Empty;
    }

    public class AdminProductVariantEditViewModel
    {
        public int MaBienThe { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập kích cỡ.")]
        [StringLength(20, ErrorMessage = "Kích cỡ không được vượt quá 20 ký tự.")]
        public string KichCo { get; set; } = string.Empty;

        [ProductPrice]
        public decimal GiaBan { get; set; }
    }

    public sealed class ProductPriceAttribute : ValidationAttribute
    {
        public ProductPriceAttribute()
        {
            ErrorMessage = "Giá bán phải lớn hơn 0.";
        }

        public override bool IsValid(object? value) => value is decimal price && price > 0;
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
        public string? MoTa { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string DoiTuong { get; set; } = string.Empty;
        public int AnswerCount { get; set; }
        public int AudienceCount { get; set; }
        public int QuestionCount { get; set; }
        public bool HasResponses { get; set; }
    }

    public class SurveyAdminIndexViewModel
    {
        public List<SurveyListItemViewModel> Surveys { get; set; } = new();
        public List<SurveyCustomerOptionViewModel> Customers { get; set; } = new();
        public SurveyEditorViewModel Editor { get; set; } = new();
        public bool EditingSurvey { get; set; }
        public SurveyResultViewModel? Results { get; set; }
        public string ResultTab { get; set; } = "summary";
        public string State { get; set; } = "all";
    }

    public class SurveyCustomerOptionViewModel
    {
        public int MaKH { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string? TaiKhoan { get; set; }
        public string? AgeGroup { get; set; }
        public List<string> Interests { get; set; } = new();
    }

    public class SurveyEditorViewModel
    {
        public int MaKhaoSat { get; set; }
        [Required, StringLength(255)]
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        [Required]
        public string DoiTuong { get; set; } = "TatCa";
        public List<int> MaKH { get; set; } = new();
        public List<SurveyQuestionEditorViewModel> Questions { get; set; } = new();
        public bool OnlyMetadata { get; set; }
    }

    public class SurveyQuestionEditorViewModel
    {
        [Required, StringLength(255)]
        public string NoiDung { get; set; } = string.Empty;
        [Required]
        public string Loai { get; set; } = "SAO";
        public List<string> LuaChon { get; set; } = new();
    }

    public class SurveyResultViewModel
    {
        public int MaKhaoSat { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public int TotalResponses { get; set; }
        public List<SurveyQuestionResultViewModel> Questions { get; set; } = new();
        public List<SurveyResponseResultViewModel> Responses { get; set; } = new();
    }

    public class SurveyResponseResultViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
        public List<SurveyAnswerResultViewModel> Answers { get; set; } = new();
    }

    public class SurveyAnswerResultViewModel
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
    }

    public class SurveyQuestionResultViewModel
    {
        public string NoiDung { get; set; } = string.Empty;
        public string Loai { get; set; } = string.Empty;
        public double? AverageRating { get; set; }
        public List<SurveyOptionResultViewModel> Options { get; set; } = new();
        public List<string> TextAnswers { get; set; } = new();
    }

    public class SurveyOptionResultViewModel
    {
        public string NoiDung { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class FeedbackAdminIndexViewModel
    {
        public string Tab { get; set; } = "reviews";
        public string Status { get; set; } = "all";
        public int? Stars { get; set; }
        public int? ProductId { get; set; }
        public string? Search { get; set; }
        public List<Feedback> Feedbacks { get; set; } = new();
        public List<ContactMessage> Contacts { get; set; } = new();
        public List<Product> Products { get; set; } = new();
        public int PendingFeedbackCount { get; set; }
        public int PendingContactCount { get; set; }
        public int TotalFeedbackCount { get; set; }
        public double? AverageFeedbackRating { get; set; }
        public PaginationViewModel Pagination { get; set; } = new();
    }
}
