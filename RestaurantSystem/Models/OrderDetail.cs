using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantSystem.Models
{
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Поръчката е задължителна")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Ястието е задължително")]
        public int MenuItemId { get; set; }

        [Required(ErrorMessage = "Количеството е задължително")]
        [Range(1, 100, ErrorMessage = "Количеството трябва да е между 1 и 100")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [MaxLength(200, ErrorMessage = "Бележката не може да надвишава 200 символа")]
        public string? Note { get; set; }

        
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }

        [ForeignKey("MenuItemId")]
        public virtual MenuItem MenuItem { get; set; }
    }
}