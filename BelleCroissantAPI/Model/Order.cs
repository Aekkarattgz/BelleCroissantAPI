using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BelleCroissantAPI.Model
{
    public class Order
    {
        public int OrderId { get; set; }  // Primary key
        public int CustomerId { get; set; }  // Foreign key referencing Customer
        public DateTime OrderDate { get; set; }  // Date the order was placed
        public bool IsCompleted { get; set; }  // Indicates whether the order is complete
        public string Status { get; set; }  // Order status, e.g., 'Pending', 'Completed', 'Cancelled'

        // Navigation property for the related Customer
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
    }
}

//using System.ComponentModel.DataAnnotations;

//namespace BelleCroissantAPI.Model
//{
//    public class Order
//    {
//        public int OrderId { get; set; }
//        public int CustomerId { get; set; }
//        public DateTime OrderDate { get; set; }
//        public bool IsCompleted { get; set; }
//        public string Status { get; set; }
//    }
//}
