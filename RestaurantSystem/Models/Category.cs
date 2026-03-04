using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantSystem.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Името на категорията е задължително")]
        [MaxLength(50, ErrorMessage = "Името не може да надвишава 50 символа")]
        public string Name { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public double Priority { get; set; }

        
        public virtual ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}