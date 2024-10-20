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

        // Display all order products
        public async Task<IActionResult> Index()
        {
            var orderProducts = await context.OrdersProducts
                .Include(op => op.Product) // Assuming there is a relationship between OrdersProducts and Product
                .Select(op => new OrdersProductDto
                {
                    OrderId = op.OrderId,
                    ProductId = op.ProductId,
                    ProductName = op.Product.Name, // Map the product name from the related Product entity
                    Quantity = op.Quantity,
                    SalesPrice = op.Product.SalesPrice // Map the price from the related Product entity
                })
                .ToListAsync();

            return View(orderProducts); // Pass the list to the view
        }

        // GET: Display form to add products to an order
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

        // POST: Add selected products to the order
        [HttpPost]
        public async Task<IActionResult> AddProductsToOrder(int orderId, int[] productIds, int[] quantities)
        {
            if (productIds.Length != quantities.Length)
            {
                return BadRequest("Product IDs and quantities do not match.");
            }

            for (int i = 0; i < productIds.Length; i++)
            {
                int productId = productIds[i];
                int quantity = quantities[i];

                var existingOrderProduct = await context.OrdersProducts
                    .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId);

                if (existingOrderProduct != null)
                {
                    existingOrderProduct.Quantity += quantity;
                    context.OrdersProducts.Update(existingOrderProduct);
                }
                else
                {
                    var orderProduct = new OrdersProduct
                    {
                        OrderId = orderId,
                        ProductId = productId,
                        Quantity = quantity
                    };

                    await context.OrdersProducts.AddAsync(orderProduct);
                }
            }

            await context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Products added to order successfully!";
            return RedirectToAction("Index");
        }

        // POST: Generate and execute SQL script for adding products to the order
        [HttpPost]
        public async Task<IActionResult> GenerateAndExecuteSql(int orderId, string productIds, string quantities)
        {
            var productIdArray = productIds.Split(',').Select(int.Parse).ToArray();
            var quantityArray = quantities.Split(',').Select(int.Parse).ToArray();

            if (productIdArray.Length != quantityArray.Length)
            {
                return BadRequest("Product IDs and quantities do not match.");
            }

            for (int i = 0; i < productIdArray.Length; i++)
            {
                int productId = productIdArray[i];
                int quantity = quantityArray[i];

                var existingOrderProduct = await context.OrdersProducts
                    .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId);

                if (existingOrderProduct != null)
                {
                    // If the product already exists, update the quantity
                    existingOrderProduct.Quantity += quantity;
                    context.OrdersProducts.Update(existingOrderProduct);
                }
                else
                {
                    // If the product is new, create a new OrdersProduct entry
                    var orderProduct = new OrdersProduct
                    {
                        OrderId = orderId,
                        ProductId = productId,
                        Quantity = quantity
                    };
                    await context.OrdersProducts.AddAsync(orderProduct);
                }
            }

            // Save changes in the OrdersProducts table
            await context.SaveChangesAsync();

            // Calculate the new total amount for the order
            var totalAmount = await context.OrdersProducts
                .Where(op => op.OrderId == orderId)
                .SumAsync(op => op.Quantity * op.Product.SalesPrice);

            // Update the total amount in the Order table
            var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order != null)
            {
                order.TotalAmount = totalAmount;
                context.Orders.Update(order);
                await context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Products added and order total updated successfully!";
            return RedirectToAction("Index");
        }
    }
}
