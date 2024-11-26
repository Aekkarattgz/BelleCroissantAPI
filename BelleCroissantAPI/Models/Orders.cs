using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BelleCroissantAPI.Models
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; } // Primary Key

        [Required]
        public int CustomerId { get; set; } // Foreign Key

        [Required]
        public DateTime OrderDate { get; set; } // วันที่สั่งซื้อ

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; } // ยอดรวมคำสั่งซื้อ

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // สถานะคำสั่งซื้อ

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } // วิธีการชำระเงิน

        [Required]
        [MaxLength(20)]
        public string Channel { get; set; } // ช่องทางการสั่งซื้อ เช่น Online หรือ Store

        public int? StoreId { get; set; } // Store ที่สั่ง (ถ้ามี)
        public int? PromotionId { get; set; } // Promotion ที่ใช้ (ถ้ามี)

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? DiscountAmount { get; set; } // ส่วนลด (ถ้ามี)

        // ** Navigation Properties **
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } // เชื่อมกับตาราง Customer (FK)

        public virtual ICollection<OrderItem> OrderItems { get; set; } // รายการสินค้าภายใน Order
    }
}
