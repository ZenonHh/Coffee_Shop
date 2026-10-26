using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Models.ViewModels
{
    public class OrderListViewModel
    {
        public List<OrderListItemViewModel> Orders { get; set; } = new();
        public string? SelectedStatus { get; set; }
    }

    public class OrderListItemViewModel
    {
        public int MaHD { get; set; }
        public string HoTenNguoiNhan { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public decimal TongTien { get; set; }
        public string PhuongThucThanhToan { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public string? LyDoHuy { get; set; }
    }

    public class OrderDetailViewModel
    {
        public int MaHD { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; } = string.Empty;
        public string PhuongThucThanhToan { get; set; } = string.Empty;
        public string? NguoiXuLy { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string? LyDoHuy { get; set; }
        public string HoTenNguoiNhan { get; set; } = string.Empty;
        public string SdtNguoiNhan { get; set; } = string.Empty;
        public string? EmailKhachHang { get; set; }
        public string? DiaChiGiao { get; set; }
        public string? GhiChu { get; set; }
        public int TamTinh { get; set; }
        public decimal TienGiamGia { get; set; }
        public int PhiGiao { get; set; }
        public decimal TongTien { get; set; }
        public List<OrderDetailLineViewModel> Details { get; set; } = new();
    }

    public class OrderDetailLineViewModel
    {
        public string TenSP { get; set; } = string.Empty;
        public string KichCo { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public decimal GiaBan { get; set; }
        public decimal ThanhTien { get; set; }
        public List<string> Toppings { get; set; } = new();
    }

    public class CancelOrderRequestModel
    {
        [Range(1, int.MaxValue)]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do hủy đơn.")]
        [StringLength(500, ErrorMessage = "Lý do hủy không được vượt quá 500 ký tự.")]
        public string CancelReason { get; set; } = string.Empty;
    }
}
