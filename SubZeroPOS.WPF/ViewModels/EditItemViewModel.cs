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
    public partial class EditItemViewModel : ObservableObject
    {
        private readonly IItemService _itemService;
        private int _itemId;
        private string? _currentImagePath; // already-saved path on disk, if any

        public EditItemViewModel(IItemService itemService)
        {
            _itemService = itemService;
        }

        public ObservableCollection<Category> Categories { get; } = new();

        [ObservableProperty] private string itemName = string.Empty;
        [ObservableProperty] private string itemNameEn = string.Empty;
        [ObservableProperty] private string priceText = string.Empty;
        [ObservableProperty] private Category? selectedCategory;
        [ObservableProperty] private string? imageDisplayPath; // what to show as current image
        [ObservableProperty] private bool hasImage;
        [ObservableProperty] private string? pendingNewImageFilePath; // freshly picked file, not yet saved
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;

        public event Action? BackRequested;
        public event Action? ChooseImageRequested; // code-behind opens the file dialog
        public event Action<string>? ImagePicked;

        public async Task InitializeAsync(int itemId, System.Collections.Generic.List<Category> categories)
        {
            IsBusy = true;
            try
            {
                Categories.Clear();
                foreach (var c in categories)
                    Categories.Add(c);

                var item = await _itemService.GetItemByIdAsync(itemId);
                if (item is null) return;

                _itemId = item.ItemId;
                ItemName = item.ItemName;
                ItemNameEn = item.NameEn ?? string.Empty;
                PriceText = item.Price.ToString("0.###");
                _currentImagePath = item.ImagePath;
                ImageDisplayPath = item.ImagePath;
                HasImage = !string.IsNullOrWhiteSpace(item.ImagePath);
                PendingNewImageFilePath = null;

                foreach (var c in Categories)
                {
                    if (c.CategoryId == item.CategoryId)
                    {
                        SelectedCategory = c;
                        break;
                    }
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void ChooseImage() => ChooseImageRequested?.Invoke();

        // Called by code-behind once the user picks a file from the dialog.
        public void OnImageFilePicked(string filePath)
        {
            PendingNewImageFilePath = filePath;
            HasImage = true;
        }

        [RelayCommand]
        private void RemoveImage()
        {
            PendingNewImageFilePath = null;
            ImageDisplayPath = null;
            HasImage = false;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            StatusMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(ItemName))
            {
                StatusMessage = "الرجاء إدخال اسم الصنف";
                return;
            }

            if (!decimal.TryParse(PriceText, out var price) || price <= 0)
            {
                StatusMessage = "الرجاء إدخال سعر صحيح";
                return;
            }

            if (SelectedCategory is null)
            {
                StatusMessage = "الرجاء اختيار القسم";
                return;
            }

            IsBusy = true;
            try
            {
                await _itemService.UpdateItemNameAsync(_itemId, ItemName,
                    string.IsNullOrWhiteSpace(ItemNameEn) ? null : ItemNameEn);
                await _itemService.UpdateItemPriceAsync(_itemId, price);
                await _itemService.UpdateItemCategoryAsync(_itemId, SelectedCategory.CategoryId);

                // Image: user picked a new file -> copy it in; user cleared
                // the image -> remove it; otherwise leave the existing one alone.
                if (!string.IsNullOrWhiteSpace(PendingNewImageFilePath))
                {
                    var extension = Path.GetExtension(PendingNewImageFilePath);
                    var fileName = $"{_itemId}{extension}";
                    var destFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Items");
                    Directory.CreateDirectory(destFolder);
                    var destPath = Path.Combine(destFolder, fileName);
                    File.Copy(PendingNewImageFilePath, destPath, overwrite: true);

                    await _itemService.UpdateItemImagePathAsync(_itemId, $"Images/Items/{fileName}");
                }
                else if (!HasImage && !string.IsNullOrWhiteSpace(_currentImagePath))
                {
                    await _itemService.RemoveItemImageAsync(_itemId);
                }

                StatusMessage = "تم حفظ التعديلات بنجاح";
                BackRequested?.Invoke();
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
        private void Back() => BackRequested?.Invoke();
    }
}
