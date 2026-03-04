using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantSystem.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Заглавието е задължително")]
        [MaxLength(100, ErrorMessage = "Заглавието не може да надвишава 100 символа")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Цената е задължителна")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Calories { get; set; }

        public long InternalBarcode { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public bool IsAvailable { get; set; } = true;

        
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}