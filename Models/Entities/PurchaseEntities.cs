using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShop.Models.Entities;

[Table("NhaCungCap")]
public class Supplier
{
    [Key]
    public int MaNCC { get; set; }

    [Required, MaxLength(150)]
    public string TenNCC { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Sdt { get; set; }

    [MaxLength(255)]
    public string? DiaChi { get; set; }

    public bool? TrangThai { get; set; } = true;
}

[Table("NguyenLieu")]
public class Ingredient
{
    [Key]
    public int MaNL { get; set; }

    [Required, MaxLength(100)]
    public string TenNL { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? DonViTinh { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? GiaVon { get; set; }
}

[Table("NhanVien")]
public class StaffMember
{
    [Key]
    public int MaNV { get; set; }

    [Required, MaxLength(50)]
    public string HoVaDem { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Ten { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Sdt { get; set; }

    [MaxLength(50)]
    public string? ChucVu { get; set; }
}

[Table("PhieuNhap")]
public class Purchase
{
    [Key]
    public int MaPN { get; set; }

    public int MaNCC { get; set; }

    public int MaNV { get; set; }

    public DateTime? NgayNhap { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? TongTien { get; set; }

    [MaxLength(255)]
    public string? GhiChu { get; set; }

    [MaxLength(100)]
    public string NguoiLap { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NguoiDuyet { get; set; }

    [MaxLength(50)]
    public string TrangThai { get; set; } = "Chờ duyệt";

    public int? NguoiDuyetId { get; set; }

    public DateTime? NgayDuyet { get; set; }

    public virtual Supplier? Supplier { get; set; }
    public virtual StaffMember? StaffMember { get; set; }
    public virtual Account? Approver { get; set; }
    public virtual ICollection<PurchaseDetail> Details { get; set; } = new List<PurchaseDetail>();
}

[Table("ChiTietPhieuNhap")]
public class PurchaseDetail
{
    [Key]
    public int MaCTPN { get; set; }

    public int MaPN { get; set; }

    public int? MaNL { get; set; }

    public int? MaSP { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal SoLuong { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal DonGia { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal ThanhTien { get; set; }

    public virtual Purchase? Purchase { get; set; }
    public virtual Ingredient? Ingredient { get; set; }
    public virtual Product? Product { get; set; }
}