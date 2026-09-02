using System;
using System.Collections.Generic;
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
        private const int PageSize = 20;

        private readonly IOrderService _orderService;
        private readonly IRestaurantSettingsService _settingsService;
        private List<Order> _allLoadedOrders = new();
        private List<Order> _filteredOrders = new();

        public PreviousOrdersViewModel(IOrderService orderService, IRestaurantSettingsService settingsService)
        {
            _orderService = orderService;
            _settingsService = settingsService;

            OrderTypeFilterOptions = new ObservableCollection<string> { "الكل", "صالة", "توصيل", "استلام" };
            PaymentFilterOptions = new ObservableCollection<string> { "الكل", "نقدي", "بنكك" };
            SelectedOrderTypeFilter = OrderTypeFilterOptions[0];
            SelectedPaymentFilter = PaymentFilterOptions[0];
        }

        public ObservableCollection<Order> Orders { get; } = new(); // current page only
        public ObservableCollection<string> OrderTypeFilterOptions { get; }
        public ObservableCollection<string> PaymentFilterOptions { get; }

        [ObservableProperty]
        private DateTime fromDate = DateTime.Today;

        [ObservableProperty]
        private DateTime toDate = DateTime.Today;

        [ObservableProperty]
        private string selectedOrderTypeFilter = string.Empty;

        [ObservableProperty]
        private string selectedPaymentFilter = string.Empty;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string totalForRange = "0";

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int totalPages = 1;

        [ObservableProperty]
        public int totalFilteredCount;

        public string PageInfoText => $"الصفحة {CurrentPage} من {TotalPages} ({TotalFilteredCount} طلب)";
        public bool CanGoNext => CurrentPage < TotalPages;
        public bool CanGoPrevious => CurrentPage > 1;

        public event Action? BackRequested;
        public event Action<OrderInvoiceDto>? ViewInvoiceRequested;

        public async Task InitializeAsync() => await LoadOrdersAsync();

        [RelayCommand]
        private async Task LoadOrdersAsync()
        {
            IsBusy = true;
            try
            {
                _allLoadedOrders.Clear();

                // GetOrdersByDateAsync is per single day - loop the range and combine.
                for (var d = FromDate.Date; d <= ToDate.Date; d = d.AddDays(1))
                {
                    var dayOrders = await _orderService.GetOrdersByDateAsync(d);
                    _allLoadedOrders.AddRange(dayOrders);
                }

                ApplyFilters();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SetTodayAsync()
        {
            FromDate = DateTime.Today;
            ToDate = DateTime.Today;
            await LoadOrdersAsync();
        }

        [RelayCommand]
        private async Task SetThisWeekAsync()
        {
            FromDate = DateTime.Today.AddDays(-6);
            ToDate = DateTime.Today;
            await LoadOrdersAsync();
        }

        [RelayCommand]
        private async Task SetThisMonthAsync()
        {
            FromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDate = DateTime.Today;
            await LoadOrdersAsync();
        }

        partial void OnSelectedOrderTypeFilterChanged(string value) => ApplyFilters();
        partial void OnSelectedPaymentFilterChanged(string value) => ApplyFilters();

        private void ApplyFilters()
        {
            var filtered = _allLoadedOrders.AsEnumerable();

            if (SelectedOrderTypeFilter != "الكل")
            {
                var typeCode = SelectedOrderTypeFilter switch
                {
                    "صالة" => "DineIn",
                    "توصيل" => "Delivery",
                    "استلام" => "Pickup",
                    _ => null
                };
                if (typeCode != null)
                    filtered = filtered.Where(o => o.OrderType?.TypeCode == typeCode);
            }

            if (SelectedPaymentFilter != "الكل")
            {
                var paymentCode = SelectedPaymentFilter == "بنكك" ? "Bankak" : "Cash";
                filtered = filtered.Where(o => o.PaymentMethodCode == paymentCode);
            }

            _filteredOrders = filtered.OrderByDescending(o => o.OrderDate).ToList();
            TotalForRange = _filteredOrders.Sum(o => o.TotalAmount).ToString("#,##0");
            TotalFilteredCount = _filteredOrders.Count;

            CurrentPage = 1; // any filter change resets to page 1
            UpdatePagedOrders();
        }

        private void UpdatePagedOrders()
        {
            TotalPages = Math.Max(1, (int)Math.Ceiling(_filteredOrders.Count / (double)PageSize));
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            var pageItems = _filteredOrders
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            Orders.Clear();
            foreach (var o in pageItems)
                Orders.Add(o);

            OnPropertyChanged(nameof(PageInfoText));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
        }

        [RelayCommand]
        private void NextPage()
        {
            if (!CanGoNext) return;
            CurrentPage++;
            UpdatePagedOrders();
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (!CanGoPrevious) return;
            CurrentPage--;
            UpdatePagedOrders();
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
                FooterSecondary = settings.InvoiceFooterSecondary,
                CurrencySymbol = settings.CurrencySymbol,
                DateFormat = settings.DateFormat,
                ShowCashierName = settings.ShowCashierNameOnInvoice,
              };

            ViewInvoiceRequested?.Invoke(invoice);
        }

        [RelayCommand]
        private void Back() => BackRequested?.Invoke();
    }
}
