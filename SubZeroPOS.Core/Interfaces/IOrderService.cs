using System.Collections.Generic;
using System.Threading.Tasks;
using SubZeroPOS.Core.DTOs;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Core.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CreateOrderDto orderDto);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetOrdersByDateAsync(System.DateTime date);
        Task<bool> CancelOrderAsync(int orderId);
    }
}
