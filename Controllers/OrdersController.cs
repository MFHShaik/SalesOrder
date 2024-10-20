using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesOrders.Models;
using SalesOrders.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SalesOrders.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext context;

        public OrdersController(ApplicationDbContext context)
        {
            this.context = context;
        }

        // List all orders
        public IActionResult Index()
        {
            var orders = context.Orders
                .Select(order => new OrdersDto
                {
                    Id = order.Id,  // Necessary for editing and deleting
                    CustomerName = order.CustomerName ?? "Unknown Customer",
                    OrderDate = order.OrderDate,
                    Status = order.Status ?? "Pending",
                    TotalAmount = order.TotalAmount
                }).ToList();

            return View(orders);
        }

        // Show order creation form
        public IActionResult Create(int? Id)
        {
            // Populate product list for product selection in the view
            var products = context.Products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                SalesPrice = p.SalesPrice,
                StockQuantity = p.StockQuantity
            }).ToList();

            ViewBag.Products = products; // Pass the products to the view

            var ordersDto = new OrdersDto(); // Initialize OrdersDto

            // Set order ID for new or existing orders
            if (!Id.HasValue)
            {
                var lastOrder = context.Orders.OrderByDescending(o => o.Id).FirstOrDefault();
                int newOrderId = (lastOrder != null) ? lastOrder.Id + 1 : 1; // Start from 1 if no orders exist
                ordersDto.Id = newOrderId;
            }
            else
            {
                ordersDto.Id = Id.Value;
            }

            return View(ordersDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrdersDto ordersDto)
        {
            if (!ModelState.IsValid)
            {
                return View(ordersDto);
            }

            // Create the order
            var order = new Order
            {
                CustomerName = ordersDto.CustomerName,
                OrderDate = ordersDto.OrderDate,
                Status = ordersDto.Status ?? "Pending",
                TotalAmount = 0 // Set initial TotalAmount to 0
            };

            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            // Alert the user that the order was created successfully
            TempData["OrderCreated"] = "Order created successfully";

            // Redirect back to the index page
            return RedirectToAction("Index");
        }

        // Edit an existing order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrdersDto ordersDto)
        {
            if (!ModelState.IsValid)
            {
                return View(ordersDto);
            }

            var order = await context.Orders.FindAsync(ordersDto.Id);
            if (order == null)
            {
                return NotFound();
            }

            order.CustomerName = ordersDto.CustomerName;
            order.OrderDate = ordersDto.OrderDate;
            order.Status = ordersDto.Status;
            order.TotalAmount = ordersDto.TotalAmount;

            await context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Show delete confirmation
        public IActionResult Delete(int id)
        {
            var order = context.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Confirm and delete order along with associated products
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await context.Orders
                .Include(o => o.OrdersProducts) // Include associated products for deletion
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            context.OrdersProducts.RemoveRange(order.OrdersProducts); // Remove products first
            context.Orders.Remove(order); // Then remove the order
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
