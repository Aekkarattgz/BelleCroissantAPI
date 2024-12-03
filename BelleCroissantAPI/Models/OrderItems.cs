using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BelleCroissantAPI.Models
{
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderItemId { get; set; } // Primary Key

        [Required]
        public int TransactionId { get; set; } // Foreign Key เชื่อมกับ Order

        [Required]
        public int ProductId { get; set; } // Foreign Key เชื่อมกับ Product

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } // จำนวนสินค้าที่สั่ง

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, double.MaxValue)]
        public decimal Price { get; set; } // ราคาต่อหน่วย

        // ** Navigation Properties **
        [ForeignKey("TransactionId")]
        public virtual Order Order { get; set; } // เชื่อมกับตาราง Order (FK)

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } // เชื่อมกับตาราง Product (FK)
    }
}

