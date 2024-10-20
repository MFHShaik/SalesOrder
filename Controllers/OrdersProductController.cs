using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesOrders.Models;
using SalesOrders.Services;
using System.Linq;
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
            var orderProducts = context.OrdersProducts
                .Select(op => new OrdersProductDto // OrderProductDto to represent each order line
                {
                    OrderId = op.OrderId,
                    ProductId = op.ProductId,
                    Quantity = op.Quantity,
                })
                .ToList();

            return View(orderProducts);
        }

        [HttpGet]
        public async Task<IActionResult> AddProductsToOrder(int orderId)
        {
            if (orderId == 0)
            {
                return BadRequest("Invalid order ID.");
            }

            // Check if the order exists
            var order = await context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            // Retrieve available products to display on the page
            var products = await context.Products.ToListAsync();

            // Map products to ProductDto
            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                SalesPrice = p.SalesPrice,
                StockQuantity = p.StockQuantity
            }).ToList();

            // Pass orderId to the view so it can be used in the form
            ViewBag.OrderId = orderId;
            return View(productDtos);  // Pass the mapped ProductDto list to the view
        }

        [HttpPost]
        public IActionResult AddProductsToOrder(int orderId, int productId, int quantity)
        {
            var order = context.Orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound(); // Order should exist
            }

            // Create a new OrdersProduct entry associated with the existing order
            var orderProduct = new OrdersProduct
            {
                OrderId = orderId,
                ProductId = productId,
                Quantity = quantity
            };

            context.OrdersProducts.Add(orderProduct);

            // You might need to update the total amount if necessary here
            context.SaveChanges();

            TempData["SuccessMessage"] = "Product added to order successfully!";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> RemoveProductFromOrder(int orderId, int productId)
        {
            var orderProduct = await context.OrdersProducts
                .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId);

            if (orderProduct == null)
            {
                return NotFound("Product not found in the order.");
            }

            // Optionally, you can also restock the product when removing it from the order
            var product = await context.Products.FindAsync(productId);
            if (product != null)
            {
                product.StockQuantity += orderProduct.Quantity; // Restock the quantity removed
            }

            context.OrdersProducts.Remove(orderProduct);
            await context.SaveChangesAsync();

            return Ok("Product removed from order.");
        }
    }
}
