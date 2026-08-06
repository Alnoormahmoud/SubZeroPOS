using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.DTOs;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class OrderService : IOrderService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public OrderService(IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Order> CreateOrderAsync(CreateOrderDto orderDto)
        {
            if (orderDto.Items == null || orderDto.Items.Count == 0)
                throw new InvalidOperationException("لا يمكن حفظ طلب بدون أصناف");

            await using var context = await _contextFactory.CreateDbContextAsync();

            var order = new Order
            {
                OrderTypeId = orderDto.OrderTypeId,
                CustomerName = orderDto.CustomerName,
                DeliveryFee = orderDto.DeliveryFee,
                CashierUserId = orderDto.CashierUserId,
                Notes = orderDto.Notes,
                OrderDate = DateTime.Now,
                StatusCode = "Completed",
                TotalAmount = orderDto.Items.Sum(i => i.UnitPrice * i.Quantity) + orderDto.DeliveryFee
            };

            foreach (var cartItem in orderDto.Items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ItemId = cartItem.ItemId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice // price snapshot at time of sale
                });
            }

            context.Orders.Add(order);
            await context.SaveChangesAsync();

            return order;
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Include(o => o.OrderType)
                .Include(o => o.CashierUser)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<List<Order>> GetOrdersByDateAsync(DateTime date)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var start = date.Date;
            var end = start.AddDays(1);

            return await context.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate < end)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var order = await context.Orders.FindAsync(orderId);
            if (order is null) return false;

            order.StatusCode = "Cancelled";
            await context.SaveChangesAsync();
            return true;
        }
    }
}
