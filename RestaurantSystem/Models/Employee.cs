using System.ComponentModel.DataAnnotations;

namespace RestaurantSystem.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Името на служителя е задължително")]
        [MaxLength(100, ErrorMessage = "Името не може да надвишава 100 символа")]
        public string FullName { get; set; }

        public double Salary { get; set; }

        public long Phone { get; set; }

        public DateTime HireDate { get; set; }

        [MaxLength(50, ErrorMessage = "Ролята не може да надвишава 50 символа")]
        public string? Role { get; set; }


        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}