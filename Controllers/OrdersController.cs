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
        public IActionResult Index(string searchTerm)
        {
            // Store the search term in ViewData to persist it in the search form
            ViewData["CurrentFilter"] = searchTerm;

            // Retrieve and filter orders based on the search term
            var ordersQuery = context.Orders
                .Select(order => new OrdersDto
                {
                    Id = order.Id,
                    CustomerName = order.CustomerName ?? "Unknown Customer",
                    OrderDate = order.OrderDate,
                    Status = order.Status ?? "Pending",
                    TotalAmount = order.TotalAmount
                });

            // If a search term is provided, filter the orders
            if (!string.IsNullOrEmpty(searchTerm))
            {
                ordersQuery = ordersQuery.Where(o =>
                    o.Id.ToString().Contains(searchTerm) ||
                    o.CustomerName.Contains(searchTerm) ||
                    o.OrderDate.ToString().Contains(searchTerm) ||
                    o.Status.Contains(searchTerm));
            }

            // Execute the query and return the filtered results
            var orders = ordersQuery.ToList();

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
            if (ModelState.IsValid)
            {
                return View(ordersDto);
            }

            // Fetch the order and its products
            var order = await context.Orders
                .Include(o => o.OrdersProducts)
                .ThenInclude(op => op.Product) // Include products to access stock
                .FirstOrDefaultAsync(o => o.Id == ordersDto.Id);

            if (order == null)
            {
                return NotFound();
            }

            // Update order details
            order.CustomerName = ordersDto.CustomerName;
            order.OrderDate = ordersDto.OrderDate;
            order.Status = ordersDto.Status;

            // Recalculate total amount
            decimal totalAmount = 0;

            // Iterate over the DTO products and update the corresponding order products
            foreach (var dto in ordersDto.OrderProducts)
            {
                var existingOrderProduct = order.OrdersProducts.FirstOrDefault(p => p.ProductId == dto.ProductId);

                if (existingOrderProduct != null)
                {
                    // Adjust stock based on quantity changes
                    var originalQuantity = existingOrderProduct.Quantity;
                    var newQuantity = dto.Quantity;
                    var difference = newQuantity - originalQuantity;

                    if (difference > 0)
                    {
                        // Reduce stock for increased quantity
                        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId);
                        if (product.StockQuantity < difference)
                        {
                            return BadRequest($"Not enough stock for product {product.Name}. Available: {product.StockQuantity}, requested: {difference}.");
                        }
                        product.StockQuantity -= difference;
                    }
                    else if (difference < 0)
                    {
                        // Increase stock for decreased quantity
                        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId);
                        product.StockQuantity += Math.Abs(difference);
                    }

                    // Update the order product's quantity
                    existingOrderProduct.Quantity = newQuantity;

                    // Calculate total amount for this product
                    totalAmount += existingOrderProduct.Quantity * existingOrderProduct.Product.SalesPrice;

                    // Mark the order product as modified
                    context.Entry(existingOrderProduct).State = EntityState.Modified;
                }
                else
                {
                    // Handle new products added to the order
                    var product = await context.Products.FirstOrDefaultAsync(p => p.Id == dto.ProductId);
                    if (product == null || product.StockQuantity < dto.Quantity)
                    {
                        return BadRequest($"Not enough stock for product {dto.ProductName}. Available: {product?.StockQuantity ?? 0}, requested: {dto.Quantity}.");
                    }

                    // Reduce the stock for the new product
                    product.StockQuantity -= dto.Quantity;

                    var newProduct = new OrdersProduct
                    {
                        ProductId = dto.ProductId,
                        OrderId = order.Id,
                        Quantity = dto.Quantity
                    };
                    await context.OrdersProducts.AddAsync(newProduct);

                    // Calculate total amount for the new product
                    totalAmount += newProduct.Quantity * product.SalesPrice;
                }
            }

            // Update the total amount for the order
            order.TotalAmount = totalAmount;

            // Explicitly mark the order as modified
            context.Entry(order).State = EntityState.Modified;

            // Save all changes to the database
            await context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Order updated successfully!";
            return RedirectToAction("Index");
        }



        // GET: Orders/Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await context.Orders
                .Include(o => o.OrdersProducts)
                .FirstOrDefaultAsync(o => o.Id == id);

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
                .Include(o => o.OrdersProducts)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            if (order.OrdersProducts != null && order.OrdersProducts.Any())
            {
                context.OrdersProducts.RemoveRange(order.OrdersProducts); // Remove associated products
            }

            context.Orders.Remove(order); // Remove the order itself

            try
            {
                await context.SaveChangesAsync(); // Persist the changes
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                return StatusCode(500, "Error deleting the order. Please try again later.");
            }

            TempData["SuccessMessage"] = "Order deleted successfully!";
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