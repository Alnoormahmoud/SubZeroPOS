using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class MenuManagementViewModel : ObservableObject
    {
        private const int PageSize = 20;

        private readonly IItemService _itemService;
        private List<Item> _allItems = new();

        public MenuManagementViewModel(IItemService itemService)
        {
            _itemService = itemService;
        }

        public ObservableCollection<Item> Items { get; } = new(); // current page only
        public ObservableCollection<Category> Categories { get; } = new();

        // Add-item form
        [ObservableProperty] private string newItemName = string.Empty;
        [ObservableProperty] private string newItemNameEn = string.Empty;
        [ObservableProperty] private string newItemPriceText = string.Empty;
        [ObservableProperty] private Category? selectedNewItemCategory;
        [ObservableProperty] private string? newItemImageFilePath; // full path to the chosen source file on disk
        [ObservableProperty] private string statusMessage = string.Empty;
        [ObservableProperty] private bool isBusy;
        [ObservableProperty] private int currentPage = 1;
        [ObservableProperty] private int totalPages = 1;
        [ObservableProperty] private int totalItemCount;

        public string PageInfoText => $"الصفحة {CurrentPage} من {TotalPages}";
        public bool CanGoNext => CurrentPage < TotalPages;
        public bool CanGoPrevious => CurrentPage > 1;

        // Add-category form
        [ObservableProperty] private string newCategoryName = string.Empty;
        [ObservableProperty] private string newCategoryNameEn = string.Empty;
        [ObservableProperty] private bool showAddCategoryForm;

        // Item edit panel - opens when tapping "تعديل" on a row, instead of
        // editing inline in the crowded list row.
        [ObservableProperty] private bool isItemEditOpen;
        [ObservableProperty] private Item? editingItem;
        [ObservableProperty] private string editItemName = string.Empty;
        [ObservableProperty] private string editItemNameEn = string.Empty;
        [ObservableProperty] private string editItemPriceText = string.Empty;
        [ObservableProperty] private Category? editItemCategory;
        [ObservableProperty] private bool editItemIsActive;
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EditItemHasNoImage))]
        private string? editItemImagePath; // current relative path shown as preview

        public bool EditItemHasNoImage => string.IsNullOrWhiteSpace(EditItemImagePath);

        // Category edit panel - same idea, kept separate from the add-category form.
        [ObservableProperty] private bool isCategoryEditOpen;
        [ObservableProperty] private Category? editingCategory;
        [ObservableProperty] private string editCategoryName = string.Empty;
        [ObservableProperty] private string editCategoryNameEn = string.Empty;

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
            _allItems = await _itemService.GetAllItemsForManagementAsync();
            TotalItemCount = _allItems.Count;
            CurrentPage = 1;
            UpdatePagedItems();
        }

        private void UpdatePagedItems()
        {
            TotalPages = Math.Max(1, (int)Math.Ceiling(_allItems.Count / (double)PageSize));
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            var pageItems = _allItems.Skip((CurrentPage - 1) * PageSize).Take(PageSize);

            Items.Clear();
            foreach (var i in pageItems)
                Items.Add(i);

            OnPropertyChanged(nameof(PageInfoText));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
        }

        [RelayCommand]
        private void NextPage()
        {
            if (!CanGoNext) return;
            CurrentPage++;
            UpdatePagedItems();
        }

        [RelayCommand]
        private void PreviousPage()
        {
            if (!CanGoPrevious) return;
            CurrentPage--;
            UpdatePagedItems();
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
        private async Task DeleteCategoryAsync(Category category)
        {
            var confirm = System.Windows.MessageBox.Show(
                $"هل أنت متأكد من حذف قسم \"{category.NameAr}\" نهائياً؟",
                "تأكيد الحذف",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (confirm != System.Windows.MessageBoxResult.Yes) return;

            var (success, errorMessage) = await _itemService.DeleteCategoryAsync(category.CategoryId);

            if (!success)
            {
                System.Windows.MessageBox.Show(errorMessage, "تعذر الحذف",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            StatusMessage = $"تم حذف قسم \"{category.NameAr}\"";
            await ReloadCategoriesAsync();
        }

        [RelayCommand]
        private void OpenEditItem(Item item)
        {
            EditingItem = item;
            EditItemName = item.ItemName;
            EditItemNameEn = item.NameEn ?? string.Empty;
            EditItemPriceText = item.Price.ToString("#,##0");
            EditItemCategory = System.Linq.Enumerable.FirstOrDefault(Categories, c => c.CategoryId == item.CategoryId);
            EditItemIsActive = item.IsActive;
            EditItemImagePath = item.ImagePath;
            IsItemEditOpen = true;
        }

        [RelayCommand]
        private void CancelEditItem()
        {
            IsItemEditOpen = false;
            EditingItem = null;
        }

        [RelayCommand]
        private void ChangeEditItemImage()
        {
            var path = ChooseImageFileRequested?.Invoke();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return;
            _pendingEditImageSourcePath = path;
            EditItemImagePath = path; // shows the new local file as a preview until saved
        }

        [RelayCommand]
        private void RemoveEditItemImage()
        {
            _pendingEditImageSourcePath = null;
            _pendingRemoveImage = true;
            EditItemImagePath = null;
        }

        private string? _pendingEditImageSourcePath;
        private bool _pendingRemoveImage;

        [RelayCommand]
        private async Task SaveEditedItemAsync()
        {
            if (EditingItem is null) return;

            if (string.IsNullOrWhiteSpace(EditItemName))
            {
                StatusMessage = "اسم الصنف لا يمكن أن يكون فارغاً";
                return;
            }

            if (!decimal.TryParse(EditItemPriceText, out var price) || price <= 0)
            {
                StatusMessage = "الرجاء إدخال سعر صحيح";
                return;
            }

            IsBusy = true;
            try
            {
                var itemId = EditingItem.ItemId;
                var nameEn = string.IsNullOrWhiteSpace(EditItemNameEn) ? null : EditItemNameEn;

                await _itemService.UpdateItemNameAsync(itemId, EditItemName, nameEn);
                await _itemService.UpdateItemPriceAsync(itemId, price);

                if (EditItemIsActive != EditingItem.IsActive)
                    await _itemService.SetItemActiveAsync(itemId, EditItemIsActive);

                if (_pendingEditImageSourcePath != null)
                {
                    var extension = Path.GetExtension(_pendingEditImageSourcePath);
                    var relativePath = $"Images/Items/{itemId}{extension}";
                    var destinationFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Items");
                    Directory.CreateDirectory(destinationFullPath);
                    File.Copy(_pendingEditImageSourcePath, Path.Combine(destinationFullPath, $"{itemId}{extension}"), overwrite: true);
                    await _itemService.UpdateItemImagePathAsync(itemId, relativePath);
                }
                else if (_pendingRemoveImage)
                {
                    await _itemService.RemoveItemImageAsync(itemId);
                }

                _pendingEditImageSourcePath = null;
                _pendingRemoveImage = false;

                StatusMessage = $"تم تحديث {EditItemName}";
                IsItemEditOpen = false;
                EditingItem = null;
                await ReloadItemsAsync();
            }
            finally
            {
                IsBusy = false;
            }
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

        // --- Category edit panel ---

        [RelayCommand]
        private void OpenEditCategory(Category category)
        {
            EditingCategory = category;
            EditCategoryName = category.NameAr;
            EditCategoryNameEn = category.NameEn ?? string.Empty;
            IsCategoryEditOpen = true;
        }

        [RelayCommand]
        private void CancelEditCategory()
        {
            IsCategoryEditOpen = false;
            EditingCategory = null;
        }

        [RelayCommand]
        private async Task SaveEditedCategoryAsync()
        {
            if (EditingCategory is null) return;

            if (string.IsNullOrWhiteSpace(EditCategoryName))
            {
                StatusMessage = "اسم القسم لا يمكن أن يكون فارغاً";
                return;
            }

            IsBusy = true;
            try
            {
                var nameEn = string.IsNullOrWhiteSpace(EditCategoryNameEn) ? null : EditCategoryNameEn;
                await _itemService.UpdateCategoryAsync(EditingCategory.CategoryId, EditCategoryName, nameEn);

                StatusMessage = $"تم تحديث قسم \"{EditCategoryName}\"";
                IsCategoryEditOpen = false;
                EditingCategory = null;
                await ReloadCategoriesAsync();
                await ReloadItemsAsync(); // item rows show Category.NameAr, refresh them too
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
