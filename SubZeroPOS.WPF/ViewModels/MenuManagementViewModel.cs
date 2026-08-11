using System;
using System.Collections.ObjectModel;
using System.IO;
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

        // Add-item form
        [ObservableProperty] private string newItemName = string.Empty;
        [ObservableProperty] private string newItemNameEn = string.Empty;
        [ObservableProperty] private string newItemPriceText = string.Empty;
        [ObservableProperty] private Category? selectedNewItemCategory;
        [ObservableProperty] private string? newItemImageFilePath; // full path to the chosen source file on disk
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;

        // Add-category form
        [ObservableProperty] private string newCategoryName = string.Empty;
        [ObservableProperty] private string newCategoryNameEn = string.Empty;
        [ObservableProperty] private bool showAddCategoryForm;

        public event Action? BackRequested;
        public event Func<string?>? ChooseImageFileRequested; // code-behind shows OpenFileDialog, returns chosen path or null

        public async Task InitializeAsync()
        {
            IsBusy = true;
            try
            {
                await ReloadCategoriesAsync();
                if (Categories.Count > 0) SelectedNewItemCategory = Categories[0];
                await ReloadItemsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ReloadCategoriesAsync()
        {
            var previouslySelectedId = SelectedNewItemCategory?.CategoryId;
            Categories.Clear();
            foreach (var c in await _itemService.GetCategoriesAsync())
                Categories.Add(c);

            if (previouslySelectedId.HasValue)
            {
                var stillThere = System.Linq.Enumerable.FirstOrDefault(Categories, c => c.CategoryId == previouslySelectedId.Value);
                if (stillThere != null) SelectedNewItemCategory = stillThere;
            }
        }

        private async Task ReloadItemsAsync()
        {
            Items.Clear();
            foreach (var i in await _itemService.GetAllItemsForManagementAsync())
                Items.Add(i);
        }

        [RelayCommand]
        private void ChooseImage()
        {
            var path = ChooseImageFileRequested?.Invoke();
            if (!string.IsNullOrWhiteSpace(path))
                NewItemImageFilePath = path;
        }

        [RelayCommand]
        private void RemoveNewItemImage()
        {
            NewItemImageFilePath = null;
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
                var nameEn = string.IsNullOrWhiteSpace(NewItemNameEn) ? null : NewItemNameEn;
                var item = await _itemService.AddItemAsync(SelectedNewItemCategory.CategoryId, NewItemName, price, nameEn);

                // If an image was chosen, copy it into Images/Items/{ItemId}.{ext}
                // next to the running app, then store that relative path.
                if (!string.IsNullOrWhiteSpace(NewItemImageFilePath) && File.Exists(NewItemImageFilePath))
                {
                    var extension = Path.GetExtension(NewItemImageFilePath);
                    var relativePath = $"Images/Items/{item.ItemId}{extension}";
                    var destinationFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Items");
                    Directory.CreateDirectory(destinationFullPath);
                    File.Copy(NewItemImageFilePath, Path.Combine(destinationFullPath, $"{item.ItemId}{extension}"), overwrite: true);

                    await _itemService.UpdateItemImagePathAsync(item.ItemId, relativePath);
                }

                NewItemName = string.Empty;
                NewItemNameEn = string.Empty;
                NewItemPriceText = string.Empty;
                NewItemImageFilePath = null;
                StatusMessage = "تم إضافة الصنف بنجاح";
                await ReloadItemsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"حدث خطأ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ToggleAddCategoryForm() => ShowAddCategoryForm = !ShowAddCategoryForm;

        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName))
            {
                StatusMessage = "الرجاء إدخال اسم القسم";
                return;
            }

            IsBusy = true;
            try
            {
                var nameEn = string.IsNullOrWhiteSpace(NewCategoryNameEn) ? null : NewCategoryNameEn;
                var category = await _itemService.AddCategoryAsync(NewCategoryName, nameEn);

                NewCategoryName = string.Empty;
                NewCategoryNameEn = string.Empty;
                ShowAddCategoryForm = false;
                StatusMessage = $"تم إضافة قسم \"{category.NameAr}\" بنجاح";

                await ReloadCategoriesAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task UpdateItemAsync(Item item)
        {
            if (string.IsNullOrWhiteSpace(item.ItemName))
            {
                StatusMessage = "اسم الصنف لا يمكن أن يكون فارغاً";
                return;
            }

            await _itemService.UpdateItemNameAsync(item.ItemId, item.ItemName, item.NameEn);
            await _itemService.UpdateItemPriceAsync(item.ItemId, item.Price);
            StatusMessage = $"تم تحديث {item.ItemName}";
        }

        [RelayCommand]
        private async Task RemoveItemImageAsync(Item item)
        {
            await _itemService.RemoveItemImageAsync(item.ItemId);
            await ReloadItemsAsync();
        }

        [RelayCommand]
        private async Task ChangeItemImageAsync(Item item)
        {
            var path = ChooseImageFileRequested?.Invoke();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;

            var extension = Path.GetExtension(path);
            var relativePath = $"Images/Items/{item.ItemId}{extension}";
            var destinationFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Items");
            Directory.CreateDirectory(destinationFullPath);
            File.Copy(path, Path.Combine(destinationFullPath, $"{item.ItemId}{extension}"), overwrite: true);

            await _itemService.UpdateItemImagePathAsync(item.ItemId, relativePath);
            await ReloadItemsAsync();
        }

        [RelayCommand]
        private async Task ToggleActiveAsync(Item item)
        {
            await _itemService.SetItemActiveAsync(item.ItemId, !item.IsActive);
            await ReloadItemsAsync();
        }

        [RelayCommand]
        private async Task DeleteItemAsync(Item item)
        {
            var result = System.Windows.MessageBox.Show(
                $"هل أنت متأكد من حذف \"{item.ItemName}\" نهائياً؟ لا يمكن التراجع عن هذا الإجراء.",
                "تأكيد الحذف",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result != System.Windows.MessageBoxResult.Yes) return;

            var (success, errorMessage) = await _itemService.DeleteItemAsync(item.ItemId);

            if (!success)
            {
                System.Windows.MessageBox.Show(errorMessage, "تعذر الحذف",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            StatusMessage = $"تم حذف {item.ItemName}";
            await ReloadItemsAsync();
        }

        [RelayCommand]
        private void Back() => BackRequested?.Invoke();
    }
}
