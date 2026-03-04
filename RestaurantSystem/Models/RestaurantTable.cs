using System.ComponentModel.DataAnnotations;

namespace RestaurantSystem.Models
{
    public class RestaurantTable
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Номерът на масата е задължителен")]
        public int Number { get; set; }

        public int Capacity { get; set; }

        [MaxLength(30, ErrorMessage = "Зоната не може да надвишава 30 символа")]
        public string? Zone { get; set; }

        public bool IsAvailable { get; set; } = true;

        public DateTime LastCleaned { get; set; }


        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}