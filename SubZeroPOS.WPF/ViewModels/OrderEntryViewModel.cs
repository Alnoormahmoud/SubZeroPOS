using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.DTOs;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.WPF.Session;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class OrderEntryViewModel : ObservableObject
    {
        private readonly IItemService _itemService;
        private readonly IOrderService _orderService;

        // Pseudo-category representing "show everything" - not a real DB row.
        // CategoryId = 0 is never used by a real category (IDENTITY starts at 1).
        private static readonly Category AllCategory = new()
        {
            CategoryId = 0,
            NameAr = "الكل",
            DisplayOrder = -1
        };

        public OrderEntryViewModel(IItemService itemService, IOrderService orderService)
        {
            _itemService = itemService;
            _orderService = orderService;
        }

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Item> ItemsInCategory { get; } = new();
        public ObservableCollection<CartLineItem> Cart { get; } = new();

        [ObservableProperty]
        private Category? selectedCategory;

        [ObservableProperty]
        private decimal cartTotal;

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        public event Action? OrderCompleted;

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                Categories.Clear();
                Categories.Add(AllCategory); // "الكل" always shows first

                var categories = await _itemService.GetCategoriesAsync();
                foreach (var c in categories)
                    Categories.Add(c);

                // Default view: show everything
                await SelectCategoryAsync(AllCategory);
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
                existing.Quantity++; // CartLineItem is observable - UI updates automatically
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
        }

        [RelayCommand]
        private async Task CompleteOrderAsync()
        {
            if (Cart.Count == 0)
            {
                StatusMessage = "السلة فارغة";
                return;
            }

            IsBusy = true;
            StatusMessage = string.Empty;

            try
            {
                var dto = new CreateOrderDto
                {
                    OrderTypeId = 1, // TODO: replace with a real selector (صالة/توصيل/استلام)
                    CashierUserId = CurrentSession.UserId,
                    DeliveryFee = 0,
                    // Convert UI-only CartLineItem rows into plain CartItemDto here,
                    // right before handing off to the service layer.
                    Items = Cart.Select(c => new CartItemDto
                    {
                        ItemId = c.ItemId,
                        ItemName = c.ItemName,
                        UnitPrice = c.UnitPrice,
                        Quantity = c.Quantity
                    }).ToList()
                };

                await _orderService.CreateOrderAsync(dto);

                Cart.Clear();
                CartTotal = 0;
                StatusMessage = "تم حفظ الطلب بنجاح";
                OrderCompleted?.Invoke();
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
    }
}
