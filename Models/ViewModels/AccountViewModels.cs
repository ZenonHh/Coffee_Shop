using System.ComponentModel.DataAnnotations;
using CoffeeShop.Models.Entities;

namespace CoffeeShop.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(8, ErrorMessage = "Mật khẩu tối thiểu 8 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ProfileViewModel
    {
        public ProfileCustomerViewModel Customer { get; set; } = new();
        public List<ProfileOrderViewModel> RecentOrders { get; set; } = new();
        public List<ProfilePointHistoryViewModel> PointHistories { get; set; } = new();
    }

    public class ProfileCustomerViewModel
    {
        public int MaKH { get; set; }
        public string? TaiKhoan { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Sdt { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int DiemTichLuy { get; set; }
        public string HangTV { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
    }

    public class ProfileOrderViewModel
    {
        public int MaHD { get; set; }
        public DateTime NgayTao { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string? LyDoHuy { get; set; }
        public List<ProfileOrderLineViewModel> OrderDetails { get; set; } = new();

        public string TrackerText => TrangThai switch
        {
            "Đã xác nhận" => "✓ Chờ xác nhận → ● Đã xác nhận → ○ Đang giao → ○ Hoàn thành",
            "Đang giao" => "✓ Chờ xác nhận → ✓ Đã xác nhận → ● Đang giao → ○ Hoàn thành",
            "Hoàn thành" => "✓ Chờ xác nhận → ✓ Đã xác nhận → ✓ Đang giao → ✓ Hoàn thành",
            _ => "● Chờ xác nhận → ○ Đã xác nhận → ○ Đang giao → ○ Hoàn thành"
        };
    }

    public class ProfileOrderLineViewModel
    {
        public string TenSP { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }

    public class ProfilePointHistoryViewModel
    {
        public string? LyDo { get; set; }
        public DateTime NgayThucHien { get; set; }
        public int DiemThayDoi { get; set; }
    }
}
