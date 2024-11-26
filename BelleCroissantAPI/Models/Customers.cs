using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BelleCroissantAPI.Models
{
    public class Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }

        [Range(0, int.MaxValue)]
        public int? Age { get; set; }

        [MaxLength(1)]
        [RegularExpression("M|F|O", ErrorMessage = "Gender must be 'M', 'F', or 'O'.")]
        public string Gender { get; set; }

        [MaxLength(10)]
        public string PostalCode { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string MembershipStatus { get; set; } = "Basic";

        public DateTime? JoinDate { get; set; }
        public DateTime? LastPurchaseDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, double.MaxValue)]
        public decimal TotalSpending { get; set; } = 0;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? AverageOrderValue { get; set; }

        [MaxLength(50)]
        public string Frequency { get; set; }

        [MaxLength(50)]
        public string PreferredCategory { get; set; }

        public bool? Churned { get; set; }
        public virtual ICollection<Order> Orders { get; set; }


        // Navigation Properties - คอมเมนต์หรือเอาออก
        // public virtual ICollection<Order> Orders { get; set; }
    }
}
