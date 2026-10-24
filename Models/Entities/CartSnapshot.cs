using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShop.Models.Entities
{
    [Table("CoffeeShopBrowserCart")]
    public class CartSnapshot
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string CartJson { get; set; } = string.Empty;

        public DateTime UpdatedAtUtc { get; set; }
    }
}
