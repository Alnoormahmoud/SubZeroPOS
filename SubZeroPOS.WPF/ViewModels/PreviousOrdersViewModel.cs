using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.DTOs;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class PreviousOrdersViewModel : ObservableObject
    {
        private readonly IOrderService _orderService;
        private readonly IRestaurantSettingsService _settingsService;

        public PreviousOrdersViewModel(IOrderService orderService, IRestaurantSettingsService settingsService)
        {
            _orderService = orderService;
            _settingsService = settingsService;
        }

        public ObservableCollection<Order> Orders { get; } = new();

        [ObservableProperty]
        private DateTime selectedDate = DateTime.Today;

        [ObservableProperty]
        private bool isBusy;

        public event Action? BackRequested;
        public event Action<OrderInvoiceDto>? ViewInvoiceRequested;

        public async Task InitializeAsync() => await LoadOrdersAsync();

        [RelayCommand]
        private async Task LoadOrdersAsync()
        {
            IsBusy = true;
            try
            {
                Orders.Clear();
                var orders = await _orderService.GetOrdersByDateAsync(SelectedDate);
                foreach (var o in orders)
                    Orders.Add(o);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ViewOrderAsync(Order order)
        {
            var fullOrder = await _orderService.GetOrderByIdAsync(order.OrderId);
            if (fullOrder is null) return;

            var settings = await _settingsService.GetSettingsAsync();

            var invoice = new OrderInvoiceDto
            {
                OrderId = fullOrder.OrderId,
                OrderDate = fullOrder.OrderDate,
                CashierName = fullOrder.CashierUser?.FullName ?? "",
                OrderTypeNameAr = fullOrder.OrderType?.NameAr ?? "",
                CustomerName = fullOrder.CustomerName,
                Notes = fullOrder.Notes,
                DeliveryFee = fullOrder.DeliveryFee,
                PaymentMethodNameAr = fullOrder.PaymentMethodCode == "Bankak" ? "بنكك" : "نقدي",
                TotalAmount = fullOrder.TotalAmount,
                Items = fullOrder.OrderItems.Select(oi => new CartItemDto
                {
                    ItemId = oi.ItemId,
                    ItemName = oi.Item?.ItemName ?? "",
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity
                }).ToList(),
                RestaurantName = settings.RestaurantName,
                RestaurantPhone = settings.Phone,
                RestaurantAddress = settings.Address,
                FooterPrimary = settings.InvoiceFooterPrimary,
                FooterSecondary = settings.InvoiceFooterSecondary
            };

            ViewInvoiceRequested?.Invoke(invoice);
        }

        [RelayCommand]
        private void Back() => BackRequested?.Invoke();
    }
}
