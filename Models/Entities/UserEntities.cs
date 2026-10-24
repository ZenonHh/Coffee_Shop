using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShop.Models.Entities
{
    [Table("VaiTro")]
    public class Role
    {
        [Key]
        public int MaVaiTro { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenVaiTro { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? MoTa { get; set; }

        public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();
    }

    [Table("TaiKhoan")]
    public class Account
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        public int MaVaiTro { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string MatKhau { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string HoTen { get; set; } = string.Empty;

        public bool TrangThai { get; set; } = true;

        [ForeignKey("MaVaiTro")]
        public virtual Role? Role { get; set; }
    }

    [Table("KhachHang")]
    public class Customer
    {
        [Key]
        public int MaKH { get; set; }

        [MaxLength(50)]
        public string? TaiKhoan { get; set; }

        [MaxLength(255)]
        public string? MatKhau { get; set; }

        [Required]
        [MaxLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Sdt { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        public int DiemTichLuy { get; set; } = 0;

        [MaxLength(20)]
        public string HangTV { get; set; } = "Đồng";

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [MaxLength(255)]
        public string? DiaChi { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<PointHistory> PointHistories { get; set; } = new List<PointHistory>();
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }

    [Table("LichSuDiem")]
    public class PointHistory
    {
        [Key]
        public int MaLSD { get; set; }

        public int? MaHD { get; set; }

        public int MaKH { get; set; }

        public int DiemThayDoi { get; set; }

        [MaxLength(255)]
        public string? LyDo { get; set; }

        public DateTime NgayThucHien { get; set; } = DateTime.Now;

        [ForeignKey("MaKH")]
        public virtual Customer? Customer { get; set; }
    }
}
