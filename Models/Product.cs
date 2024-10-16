using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesOrders.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = "";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Product Code is required.")]
        [StringLength(50, ErrorMessage = "Product Code cannot exceed 50 characters.")]
        public string ProductCode { get; set; } = "";

        [Required(ErrorMessage = "Product Type is required.")]
        [StringLength(50, ErrorMessage = "Product Type cannot exceed 50 characters.")]
        public string ProductType { get; set; } = "";

        [Required(ErrorMessage = "Cost Price is required.")]
        [Column(TypeName = "decimal(18,2)")] 
        [Range(0.01, double.MaxValue, ErrorMessage = "Cost Price must be greater than 0.")]
        public decimal CostPrice { get; set; }

        [Required(ErrorMessage = "Sales Price is required.")]
        [Column(TypeName = "decimal(18,2)")]  
        [Range(0.01, double.MaxValue, ErrorMessage = "Sales Price must be greater than 0.")]
        public decimal SalesPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")] 
        [Range(0.01, double.MaxValue, ErrorMessage = "Staff Discounted Price must be greater than 0.")]
        public decimal StaffDiscountedPrice { get; set; }

        [Required(ErrorMessage = "Stock Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity cannot be negative.")]
        public int StockQuantity { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public List<OrderProduct>? OrderProducts { get; set; }
    }
}
