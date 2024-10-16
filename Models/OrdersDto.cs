using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SalesOrders.Models
{
    public class OrdersDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(100, ErrorMessage = "Customer name can't be longer than 100 characters.")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Order date is required.")]
        public DateTime OrderDate { get; set; } = DateTime.Now; // Default to current date

        [Required(ErrorMessage = "Total amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total amount must be a positive value.")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Order status is required.")]
        public string Status { get; set; }

        // Initialize the OrderProducts list to avoid null reference exceptions
        public List<OrderProductDto> OrderProducts { get; set; } = new List<OrderProductDto>();
    }
}   
