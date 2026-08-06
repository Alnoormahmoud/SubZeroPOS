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

        public OrderEntryViewModel(IItemService itemService, IOrderService orderService)
        {
            _itemService = itemService;
            _orderService = orderService;
        }

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Item> ItemsInCategory { get; } = new();
        public ObservableCollection<CartItemDto> Cart { get; } = new();

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
                var categories = await _itemService.GetCategoriesAsync();
                foreach (var c in categories)
                    Categories.Add(c);

                if (Categories.Count > 0)
                    await SelectCategoryAsync(Categories.First());
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

            var items = await _itemService.GetItemsByCategoryAsync(category.CategoryId);
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
                // Force the collection to refresh totals shown in the UI
                RefreshCartTotal();
                return;
            }

            Cart.Add(new CartItemDto
            {
                ItemId = item.ItemId,
                ItemName = item.NameAr,
                UnitPrice = item.Price,
                Quantity = 1
            });

            RefreshCartTotal();
        }

        [RelayCommand]
        private void RemoveFromCart(CartItemDto cartItem)
        {
            Cart.Remove(cartItem);
            RefreshCartTotal();
        }

        [RelayCommand]
        private void IncreaseQuantity(CartItemDto cartItem)
        {
            cartItem.Quantity++;
            RefreshCartTotal();
        }

        [RelayCommand]
        private void DecreaseQuantity(CartItemDto cartItem)
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
                    Items = Cart.ToList()
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
