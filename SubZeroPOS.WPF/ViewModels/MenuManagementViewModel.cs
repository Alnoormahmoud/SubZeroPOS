using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class MenuManagementViewModel : ObservableObject
    {
        private readonly IItemService _itemService;

        public MenuManagementViewModel(IItemService itemService)
        {
            _itemService = itemService;
        }

        public ObservableCollection<Item> Items { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        [ObservableProperty] private string newItemName = string.Empty;
        [ObservableProperty] private string newItemPriceText = string.Empty;
        [ObservableProperty] private Category? selectedNewItemCategory;
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;

        public event Action? BackRequested;

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                Categories.Clear();
                foreach (var c in await _itemService.GetCategoriesAsync())
                    Categories.Add(c);

                if (Categories.Count > 0) SelectedNewItemCategory = Categories[0];

                await ReloadItemsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ReloadItemsAsync()
        {
            Items.Clear();
            foreach (var i in await _itemService.GetAllItemsForManagementAsync())
                Items.Add(i);
        }

        [RelayCommand]
        private async Task AddItemAsync()
        {
            StatusMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewItemName))
            {
                StatusMessage = "الرجاء إدخال اسم الصنف";
                return;
            }

            if (!decimal.TryParse(NewItemPriceText, out var price) || price <= 0)
            {
                StatusMessage = "الرجاء إدخال سعر صحيح";
                return;
            }

            if (SelectedNewItemCategory is null)
            {
                StatusMessage = "الرجاء اختيار القسم";
                return;
            }

            IsBusy = true;
            try
            {
                await _itemService.AddItemAsync(SelectedNewItemCategory.CategoryId, NewItemName, price);
                NewItemName = string.Empty;
                NewItemPriceText = string.Empty;
                StatusMessage = "تم إضافة الصنف بنجاح";
                await ReloadItemsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UpdatePriceAsync(Item item)
        {
            await _itemService.UpdateItemPriceAsync(item.ItemId, item.Price);
            StatusMessage = $"تم تحديث سعر {item.ItemName}";
        }

        [RelayCommand]
        private async Task ToggleActiveAsync(Item item)
        {
            await _itemService.SetItemActiveAsync(item.ItemId, !item.IsActive);
            await ReloadItemsAsync();
        }

        [RelayCommand]
        private void Back() => BackRequested?.Invoke();
    }
}
