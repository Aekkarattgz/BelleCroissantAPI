using System.ComponentModel.DataAnnotations;

namespace BelleCroissantAPI.Model
{
    public class Product
    {
        public int ProductId { get; set; } // Primary Key
        public string Name { get; set; } // ชื่อสินค้า
        public decimal Price { get; set; } // ราคา
        public string Description { get; set; } // คำอธิบาย
    }
}
