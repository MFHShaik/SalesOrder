using System.Threading.Tasks;
using System.Collections.Generic;
using SalesOrders.Models;

namespace SalesOrders.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> AddOrderAsync(Order order);
        Task<OrdersProduct> AddOrderProductAsync(OrdersProduct orderProduct); // Add this
        Task<IEnumerable<Order>> GetAllOrdersAsync();
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<bool> UpdateOrderAsync(Order order);
        Task<bool> DeleteOrderAsync(int orderId);
    }
}
