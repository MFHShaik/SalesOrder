using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesOrders.Services;
using SalesOrders.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace SalesOrders.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/OrdersApi/{orderNumber}
        [AllowAnonymous]
        [HttpGet("{orderNumber}")]
        public async Task<IActionResult> GetOrderDetails(int orderNumber)
        {
            var order = await _context.Orders
                .Include(o => o.OrdersProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Id == orderNumber);

            if (order == null)
            {
                return NotFound("Order not found.");
            }

            // Map Order entity to OrdersDto
            var orderDto = new OrdersDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                OrderProducts = order.OrdersProducts.Select(op => new OrdersProductDto
                {
                    OrderId = op.OrderId,
                    ProductId = op.ProductId,
                    ProductName = op.Product.Name,
                    Quantity = op.Quantity,
                    SalesPrice = op.Product.SalesPrice
                }).ToList()
            };

            return Ok(orderDto);
        }
    }
}
