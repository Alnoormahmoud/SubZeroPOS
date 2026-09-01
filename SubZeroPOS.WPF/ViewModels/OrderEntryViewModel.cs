using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.DTOs;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;
using System.Globalization;
using SubZeroPOS.WPF.Session;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class OrderEntryViewModel : ObservableObject
    {
        private readonly IItemService _itemService;
        private readonly IOrderService _orderService;
        private readonly IRestaurantSettingsService _settingsService;

        // Pseudo-category representing "show everything" - not a real DB row.
        private static readonly Category AllCategory = new()
        {
            CategoryId = 0,
            NameAr = "الكل",
            DisplayOrder = -1
        };

        public OrderEntryViewModel(IItemService itemService, IOrderService orderService, IRestaurantSettingsService settingsService)
        {
            _itemService = itemService;
            _orderService = orderService;
            _settingsService = settingsService;

            PaymentMethods = new ObservableCollection<PaymentMethodOption>
            {
                new() { Code = "Cash", NameAr = "نقدي" },
                new() { Code = "Bankak", NameAr = "بنكك" }
            };
            SelectedPaymentMethod = PaymentMethods[0];
        }

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Item> ItemsInCategory { get; } = new();
        public ObservableCollection<CartLineItem> Cart { get; } = new();
        public ObservableCollection<OrderType> OrderTypes { get; } = new();
        public ObservableCollection<PaymentMethodOption> PaymentMethods { get; }

        [ObservableProperty]
        private Category? selectedCategory;

        [ObservableProperty]
        private decimal cartTotal;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private OrderType? selectedOrderType;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GrandTotal))]
        private string customerName = string.Empty;

        [ObservableProperty]
        private string notes = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(GrandTotal))]
        private string deliveryFeeText = "0";

        [ObservableProperty]
        private PaymentMethodOption? selectedPaymentMethod;

        // Shows/hides the delivery-fee field based on selected order type
        public bool IsDeliverySelected => SelectedOrderType?.TypeCode == "DELIVERY";

        partial void OnSelectedOrderTypeChanged(OrderType? value)
        {
            OnPropertyChanged(nameof(IsDeliverySelected));
            OnPropertyChanged(nameof(GrandTotal));
        }

 

        public decimal GrandTotal =>
    CartTotal + (TryParseMoney(DeliveryFeeText, out var fee) ? fee : 0);

        public event Action<OrderInvoiceDto>? OrderCompleted;

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                // Run these concurrently instead of one-after-another - each opens
                // its own DB connection (safe, since each uses its own DbContext),
                // so doing them in parallel avoids paying connection-setup cost
                // three times in a row.
                var categoriesTask = _itemService.GetCategoriesAsync();
                var allItemsTask = _itemService.GetAllItemsAsync();
                var orderTypesTask = _orderService.GetOrderTypesAsync();

                await Task.WhenAll(categoriesTask, allItemsTask, orderTypesTask);

                Categories.Clear();
                Categories.Add(AllCategory);
                foreach (var c in categoriesTask.Result)
                    Categories.Add(c);

                ItemsInCategory.Clear();
                foreach (var i in allItemsTask.Result)
                    ItemsInCategory.Add(i);
                SelectedCategory = AllCategory;

                OrderTypes.Clear();
                foreach (var ot in orderTypesTask.Result)
                    OrderTypes.Add(ot);

                if (OrderTypes.Count > 0)
                    SelectedOrderType = OrderTypes[0];
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task SelectCategoryAsync(Category category)
        {
            SelectedCategory = category;
            ItemsInCategory.Clear();

            var items = category.CategoryId == 0
                ? await _itemService.GetAllItemsAsync()
                : await _itemService.GetItemsByCategoryAsync(category.CategoryId);

            foreach (var i in items)
                ItemsInCategory.Add(i);
        }

        [RelayCommand]
        private void AddToCart(Item item)
        {
            var existing = Cart.FirstOrDefault(c => c.ItemId == item.ItemId);
            if (existing != null)
            {
                existing.Quantity++;
                RefreshCartTotal();
                return;
            }

            Cart.Add(new CartLineItem
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                UnitPrice = item.Price,
                Quantity = 1
            });

            RefreshCartTotal();
        }

        [RelayCommand]
        private void RemoveFromCart(CartLineItem cartItem)
        {
            Cart.Remove(cartItem);
            RefreshCartTotal();
        }

        [RelayCommand]
        private void IncreaseQuantity(CartLineItem cartItem)
        {
            cartItem.Quantity++;
            RefreshCartTotal();
        }

        [RelayCommand]
        private void DecreaseQuantity(CartLineItem cartItem)
        {
            if (cartItem.Quantity <= 1)
            {
                Cart.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity--;
            }
            RefreshCartTotal();
        }

        private void RefreshCartTotal()
        {
            CartTotal = Cart.Sum(c => c.LineTotal);
            OnPropertyChanged(nameof(GrandTotal));
        }

        public event Action? CancelOrderRequested;

        [RelayCommand]
        private void CancelOrder()
        {
            if (Cart.Count == 0)
            {
                // Nothing to lose - just go back without asking.
                CancelOrderRequested?.Invoke();
                return;
            }

            var result = System.Windows.MessageBox.Show(
                "هل أنت متأكد من إلغاء الطلب؟ سيتم فقدان جميع الأصناف المضافة.",
                "تأكيد الإلغاء",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes) return;

            Cart.Clear();
            CartTotal = 0;
            CustomerName = string.Empty;
            Notes = string.Empty;
            DeliveryFeeText = "0";
            SelectedOrderType = OrderTypes.Count > 0 ? OrderTypes[0] : null;
            SelectedPaymentMethod = PaymentMethods[0];
            StatusMessage = string.Empty;

            CancelOrderRequested?.Invoke();
        }

        [RelayCommand]
        private async Task CompleteOrderAsync()
        {
            if (Cart.Count == 0)
            {
                StatusMessage = "السلة فارغة";
                return;
            }

            if (SelectedOrderType is null)
            {
                StatusMessage = "الرجاء اختيار نوع الطلب";
                return;
            }

            if (SelectedPaymentMethod is null)
            {
                StatusMessage = "الرجاء اختيار طريقة الدفع";
                return;
            }

  
            decimal deliveryFee = 0;

            if (IsDeliverySelected &&
                !TryParseMoney(DeliveryFeeText, out deliveryFee))
            {
                StatusMessage = "الرجاء إدخال رسوم توصيل صحيحة";
                return;
            }
         

            IsBusy = true;
            StatusMessage = string.Empty;

            try
            {
                var dto = new CreateOrderDto
                {
                    OrderTypeId = SelectedOrderType.OrderTypeId,
                    CustomerName = string.IsNullOrWhiteSpace(CustomerName) ? null : CustomerName,
                    Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                    DeliveryFee = IsDeliverySelected ? deliveryFee : 0,
                    PaymentMethodCode = SelectedPaymentMethod.Code,
                    CashierUserId = CurrentSession.UserId,
                    Items = Cart.Select(c => new CartItemDto
                    {
                        ItemId = c.ItemId,
                        ItemName = c.ItemName,
                        UnitPrice = c.UnitPrice,
                        Quantity = c.Quantity
                    }).ToList()
                };

                var createdOrder = await _orderService.CreateOrderAsync(dto);
                var restaurantSettings = await _settingsService.GetSettingsAsync();

                var invoice = new OrderInvoiceDto
                {
// ==========================================
// Order information
// ==========================================

 OrderId = createdOrder.OrderId,

                    OrderDate = createdOrder.OrderDate,

                    CashierName = CurrentSession.FullName,

                    OrderTypeNameAr = SelectedOrderType.NameAr,

                    CustomerName = dto.CustomerName,

                    Notes = dto.Notes,

                    DeliveryFee = dto.DeliveryFee,

                    PaymentMethodNameAr =
                    SelectedPaymentMethod.NameAr,

                    TotalAmount =
                    createdOrder.TotalAmount,

                    Items =
                    dto.Items,


                    // ==========================================
                    // Restaurant information
                    // ==========================================

                    RestaurantName =
                    restaurantSettings.RestaurantName,

                    RestaurantPhone =
                    restaurantSettings.Phone,

                    RestaurantAddress =
                    restaurantSettings.Address,


                    // ==========================================
                    // Invoice footer
                    // ==========================================

                    FooterPrimary =
                    restaurantSettings.InvoiceFooterPrimary,

                    FooterSecondary =
                    restaurantSettings.InvoiceFooterSecondary,


                    // ==========================================
                    // Currency
                    // ==========================================

                    CurrencySymbol =
                    restaurantSettings.CurrencySymbol,


                    // ==========================================
                    // Date format
                    // ==========================================

                    DateFormat =
                    restaurantSettings.DateFormat,


                    // ==========================================
                    // Invoice display settings
                    // ==========================================

                    ShowCashierName =
                    restaurantSettings.ShowCashierNameOnInvoice,

                    ShowLogoOnInvoice =
                    restaurantSettings.ShowLogoOnInvoice,

                    ShowOrderNumberOnInvoice =
                    restaurantSettings.ShowOrderNumberOnInvoice,

                    ShowCustomerNameOnInvoice =
                    restaurantSettings.ShowCustomerNameOnInvoice,


                    // ==========================================
                    // Receipt paper size
                    // ==========================================

                    ReceiptPaperWidthMm =
                    restaurantSettings.ReceiptPaperWidthMm,


                    // ==========================================
                    // Printing behavior
                    // ==========================================

                    OpenPdfAfterPrinting =
                    restaurantSettings.OpenPdfAfterPrinting,

                    AutoPrintReceipt =
    restaurantSettings.AutoPrintReceipt,


                };


                // Reset the form for the next order
                Cart.Clear();
                CartTotal = 0;
                CustomerName = string.Empty;
                Notes = string.Empty;
                DeliveryFeeText = "0";
                SelectedOrderType = OrderTypes.Count > 0 ? OrderTypes[0] : null;
                SelectedPaymentMethod = PaymentMethods[0];
                StatusMessage = "تم حفظ الطلب بنجاح";

                OrderCompleted?.Invoke(invoice);
            }
            catch (Exception ex)
            {
                StatusMessage = $"حدث خطأ أثناء حفظ الطلب: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
        private static bool TryParseMoney(string? text, out decimal amount)
        {
            amount = 0;

            if (string.IsNullOrWhiteSpace(text))
                return true;

            // Remove common thousands separators
            text = text
                .Trim()
                .Replace(",", "")
                .Replace("٬", "");

            return decimal.TryParse(
                text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out amount);
        }
    }
}
