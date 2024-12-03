using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BelleCroissantAPI.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, double.MaxValue)]
        public decimal Cost { get; set; }

        public string? Description { get; set; }
        [Required]
        public bool Seasonal { get; set; }
        [Required]
        public bool Active { get; set; }

        [Required]
        public DateTime IntroducedDate { get; set; }

        public string? Ingredients { get; set; }

        // Navigation Properties
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
