using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShop.Models.Entities
{
    [Table("LoaiSanPham")]
    public class Category
    {
        [Key]
        public int MaLoaiSP { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenLoaiSP { get; set; } = string.Empty;

        public int ThuTuHienThi { get; set; } = 1;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    [Table("SanPham")]
    public class Product
    {
        [Key]
        public int MaSP { get; set; }

        public int MaLoaiSP { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenSP { get; set; } = string.Empty;

        public bool TrangThai { get; set; } = true;

        [MaxLength(255)]
        public string? HinhAnh { get; set; }

        public int SoLuongTon { get; set; }

        [ForeignKey("MaLoaiSP")]
        public virtual Category? Category { get; set; }

        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }

    [Table("BienTheSanPham")]
    public class ProductVariant
    {
        [Key]
        public int MaBienThe { get; set; }

        public int MaSP { get; set; }

        [Required]
        [MaxLength(20)]
        public string KichCo { get; set; } = string.Empty;

        [Column(TypeName = "decimal(12,2)")]
        public decimal GiaBan { get; set; }

        public bool TrangThai { get; set; } = true;

        [ForeignKey("MaSP")]
        public virtual Product? Product { get; set; }
    }
}
