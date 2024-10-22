using SalesOrders.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SalesOrders.Services
{
    public interface IOrderService
    {
        Task<Order> AddOrderAsync(Order order);
        Task<OrdersProduct> AddOrderProductAsync(OrdersProduct orderProduct);
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int orderId);
    }
}
