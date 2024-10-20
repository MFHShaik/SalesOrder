using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                    Id = order.Id,
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
            var products = context.Products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                SalesPrice = p.SalesPrice,
                StockQuantity = p.StockQuantity
            }).ToList();

            ViewBag.Products = new SelectList(products, "Id", "Name"); // Pass products as SelectList to the view

            var ordersDto = new OrdersDto(); // Initialize OrdersDto

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

            var order = new Order
            {
                CustomerName = ordersDto.CustomerName,
                OrderDate = ordersDto.OrderDate,
                Status = ordersDto.Status ?? "Pending",
                TotalAmount = 0 // Initial TotalAmount
            };

            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();

            TempData["OrderCreated"] = "Order created successfully";
            return RedirectToAction("Index");
        }

        // Show the Edit form
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var order = context.Orders
                .Include(o => o.OrdersProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            var products = context.Products.ToList();
            ViewBag.Products = new SelectList(products, "Id", "Name");

            var statusOptions = new List<string> { "Pending", "Shipped", "Completed", "Cancelled" };
            ViewBag.StatusOptions = new SelectList(statusOptions);

            var orderDto = new OrdersDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                OrderProducts = order.OrdersProducts.Select(op => new OrdersProductDto
                {
                    ProductId = op.ProductId,
                    ProductName = op.Product.Name,
                    Quantity = op.Quantity,
                    SalesPrice = op.Product.SalesPrice
                }).ToList()
            };

            return View(orderDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OrdersDto ordersDto)
        {
            if (!ModelState.IsValid)
            {
                return View(ordersDto);
            }

            var order = await context.Orders
                .Include(o => o.OrdersProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == ordersDto.Id);

            if (order == null)
            {
                return NotFound();
            }

            order.CustomerName = ordersDto.CustomerName;
            order.OrderDate = ordersDto.OrderDate;
            order.Status = ordersDto.Status;

            foreach (var dto in ordersDto.OrderProducts)
            {
                var product = order.OrdersProducts.FirstOrDefault(p => p.ProductId == dto.ProductId);
                if (product != null)
                {
                    product.Quantity = dto.Quantity;
                }
                else
                {
                    var newProduct = new OrdersProduct
                    {
                        ProductId = dto.ProductId,
                        OrderId = order.Id,
                        Quantity = dto.Quantity
                    };
                    context.OrdersProducts.Add(newProduct);
                }
            }

            order.TotalAmount = ordersDto.OrderProducts.Sum(op => op.Quantity * op.SalesPrice);
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order updated successfully!";
            return RedirectToAction("Index");
        }

        // Confirm and delete order along with associated products
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await context.Orders
                .Include(o => o.OrdersProducts)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            context.OrdersProducts.RemoveRange(order.OrdersProducts);
            context.Orders.Remove(order);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // View the details of an order, including all associated products
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await context.Orders
                .Include(o => o.OrdersProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            var orderDto = new OrdersDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                OrderProducts = order.OrdersProducts.Select(op => new OrdersProductDto
                {
                    ProductId = op.ProductId,
                    ProductName = op.Product.Name,
                    Quantity = op.Quantity,
                    SalesPrice = op.Product.SalesPrice
                }).ToList()
            };

            return View(orderDto);
        }
    }
}