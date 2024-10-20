using System.ComponentModel.DataAnnotations;

namespace SalesOrders.Models
{
    public class OrdersProductDto
    {
        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        public string ProductName { get; set; }

        public decimal SalesPrice { get; set; }
    }
}
