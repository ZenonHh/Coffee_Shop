using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShop.Models.Entities
{
    [Table("PhanHoi")]
    public class Feedback
    {
        [Key]
        public int MaPhanHoi { get; set; }

        public int MaKH { get; set; }

        public int? MaSP { get; set; }

        [Required]
        public string NoiDung { get; set; } = string.Empty;

        [Range(1, 5)]
        public int? DanhGia { get; set; }

        public DateTime NgayGui { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string TrangThaiXL { get; set; } = "Chờ xử lý";

        [ForeignKey("MaKH")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("MaSP")]
        public virtual Product? Product { get; set; }
    }

    [Table("KhaoSat")]
    public class Survey
    {
        [Key]
        public int MaKhaoSat { get; set; }

        public int MaTaiKhoan { get; set; }

        [Required]
        [MaxLength(255)]
        public string TieuDe { get; set; } = string.Empty;

        public string? MoTa { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        [ForeignKey("MaTaiKhoan")]
        public virtual Account? Account { get; set; }

        public virtual ICollection<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();
    }

    [Table("CauHoiKhaoSat")]
    public class SurveyQuestion
    {
        [Key]
        public int MaCauHoi { get; set; }

        public int MaKhaoSat { get; set; }

        [Required]
        [MaxLength(255)]
        public string NoiDungCauHoi { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LoaiCauHoi { get; set; } = "TRAC_NGHIEM";

        [ForeignKey("MaKhaoSat")]
        public virtual Survey? Survey { get; set; }

        public virtual ICollection<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();
    }

    [Table("CauTraLoi")]
    public class SurveyAnswer
    {
        [Key]
        public int MaCauTraLoi { get; set; }

        public int MaCauHoi { get; set; }

        public string? NoiDungTraLoi { get; set; }

        public DateTime NgayTraLoi { get; set; } = DateTime.Now;

        [ForeignKey("MaCauHoi")]
        public virtual SurveyQuestion? Question { get; set; }
    }
}
