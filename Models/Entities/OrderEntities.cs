using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShop.Models.Entities
{
    [Table("Voucher")]
    public class Voucher
    {
        [Key]
        public int MaVoucher { get; set; }

        [Required]
        [MaxLength(50)]
        public string MaCode { get; set; } = string.Empty;

        [MaxLength(20)]
        public string PhanLoaiGiam { get; set; } = "PERCENT";

        [Column(TypeName = "decimal(12,2)")]
        public decimal GiaTriGiam { get; set; }

        

        [MaxLength(150)]
        public string? TenChuongTrinh { get; set; }

        public DateTime? NgayBatDau { get; set; }

        public DateTime? NgayKetThuc { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal DonToiThieu { get; set; } = 0;

        public bool TrangThai { get; set; } = true;
    }

    [Table("HoaDon")]
    public class Order
    {
        [Key]
        public int MaHD { get; set; }

        public int? MaKH { get; set; }

        [MaxLength(50)]
        public string LoaiDonHang { get; set; } = "Giao hàng";

        [Column(TypeName = "decimal(12,2)")]
        public decimal TongTien { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TienGiamGia { get; set; }

        public int TamTinh { get; set; }

        public int PhiGiao { get; set; }

        [MaxLength(50)]
        public string TrangThai { get; set; } = "Chờ xác nhận";

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(100)]
        public string HoTenNguoiNhan { get; set; } = string.Empty;

        [Required]
        [MaxLength(15)]
        public string SdtNguoiNhan { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? DiaChiGiao { get; set; }

        [MaxLength(255)]
        public string? GhiChu { get; set; }

        [ForeignKey("MaKH")]
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }

    [Table("ChiTietHoaDon")]
    public class OrderDetail
    {
        [Key]
        public int MaCTHD { get; set; }

        public int MaHD { get; set; }

        public int MaSP { get; set; }

        public int MaBienThe { get; set; }

        public int SoLuong { get; set; } = 1;

        [Column(TypeName = "decimal(12,2)")]
        public decimal GiaBan { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal ThanhTien { get; set; }

        public int TienTopping { get; set; } = 0;

        [ForeignKey("MaHD")]
        public virtual Order? Order { get; set; }

        [ForeignKey("MaSP")]
        public virtual Product? Product { get; set; }

        [ForeignKey("MaBienThe")]
        public virtual ProductVariant? Variant { get; set; }

        public virtual ICollection<OrderDetailTopping> Toppings { get; set; } = new List<OrderDetailTopping>();
    }

    [Table("ChiTietHoaDon_Topping")]
    public class OrderDetailTopping
    {
        public int MaCTHD { get; set; }

        public int MaSP { get; set; }

        public int GiaBan { get; set; }

        [ForeignKey("MaCTHD")]
        public virtual OrderDetail? OrderDetail { get; set; }

        [ForeignKey("MaSP")]
        public virtual Product? ToppingProduct { get; set; }
    }
}
