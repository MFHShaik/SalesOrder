using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesOrders.Models;
using SalesOrders.Services;
using System.Threading.Tasks;

namespace SalesOrders.Controllers
{
    public class OrdersProductController : Controller
    {
        private readonly ApplicationDbContext context;

        public OrdersProductController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var orderProducts = context.OrderProducts
                .Select(op => new OrderProductDto // Change from OrdersDto to OrderProductDto
                {
                    OrderId = op.OrderId,
                    ProductId = op.ProductId,
                    Quantity = op.Quantity,
                    UnitPrice = op.UnitPrice
                })
                .ToList();

            return View(orderProducts);
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToOrder(int orderId, int productId, int quantity)
        {
            // Retrieve the product from the database
            var product = await context.Products.FindAsync(productId);
            if (product == null || quantity <= 0)
            {
                return BadRequest("Invalid product or quantity.");
            }

            // Create an OrderProduct entry
            var orderProduct = new OrderProduct
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = product.SalesPrice // Fetch unit price from product
            };

            // Add the new product to the order
            await context.OrderProducts.AddAsync(orderProduct);
            await context.SaveChangesAsync();

            // Optionally, return the updated list of products in the order
            var updatedOrderProducts = await context.OrderProducts
                .Where(op => op.OrderId == orderId)
                .Include(op => op.Product) // Include Product details if needed
                .ToListAsync();

            // Return the updated list or success message
            return Ok(updatedOrderProducts); // Can return JSON if needed
        }

        [HttpPost]
        public async Task<IActionResult> RemoveProductFromOrder(int orderId, int productId)
        {
            var orderProduct = await context.OrderProducts
                .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId);

            if (orderProduct == null)
            {
                return NotFound("Product not found in the order.");
            }

            context.OrderProducts.Remove(orderProduct);
            await context.SaveChangesAsync();

            return Ok("Product removed from order.");
        }
    }
}
