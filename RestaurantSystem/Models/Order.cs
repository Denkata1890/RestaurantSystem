using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantSystem.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public DateTime OrderTime { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public int TableId { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Статусът е задължителен")]
        [MaxLength(20, ErrorMessage = "Статусът не може да надвишава 20 символа")]
        public string Status { get; set; } = "Нова";

     
        [ForeignKey("TableId")]
        public virtual RestaurantTable Table { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Waiter { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}