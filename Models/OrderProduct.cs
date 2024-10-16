using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesOrders.Models
{
    public class OrderProduct
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price must be a positive value.")]
        public decimal UnitPrice { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; } = new Order();  // Ensures a default value

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
