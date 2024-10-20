using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SalesOrders.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100, ErrorMessage = "Customer name can't be longer than 100 characters.")]
        public string CustomerName { get; set; }  // Non-nullable

        [Required(ErrorMessage = "Order date is required.")]
        public DateTime OrderDate { get; set; }

        [Required(ErrorMessage = "Total amount is required.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Order status is required.")]
        public string Status { get; set; }  // Non-nullable

        // Initialized OrdersProducts collection to prevent null reference exceptions
        public ICollection<OrdersProduct> OrdersProducts { get; set; } = new List<OrdersProduct>();
    }
}
