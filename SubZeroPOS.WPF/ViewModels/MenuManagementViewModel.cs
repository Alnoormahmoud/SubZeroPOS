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

        public ObservableCollection<Item> Items { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();

        // =========================================================
        // ADD ITEM
        // =========================================================

        [ObservableProperty]
        private string newItemName = string.Empty;

        [ObservableProperty]
        private string newItemNameEn = string.Empty;

        [ObservableProperty]
        private string newItemPriceText = string.Empty;

        [ObservableProperty]
        private Category? selectedNewItemCategory;

        [ObservableProperty]
        private string? newItemImageFilePath;

        [ObservableProperty]
        private string addItemStatusMessage = string.Empty;

        // =========================================================
        // GENERAL
        // =========================================================

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private int currentPage = 1;

        [ObservableProperty]
        private int totalPages = 1;

        [ObservableProperty]
        private int totalItemCount;

        public string PageInfoText => $"الصفحة {CurrentPage} من {TotalPages}";

        public bool CanGoNext => CurrentPage < TotalPages;

        public bool CanGoPrevious => CurrentPage > 1;


        // =========================================================
        // ADD CATEGORY
        // =========================================================

        [ObservableProperty]
        private string newCategoryName = string.Empty;

        [ObservableProperty]
        private string newCategoryNameEn = string.Empty;

        [ObservableProperty]
        private bool showAddCategoryForm;

        [ObservableProperty]
        private string addCategoryStatusMessage = string.Empty;


        // =========================================================
        // EDIT ITEM
        // =========================================================

        [ObservableProperty]
        private bool isItemEditOpen;

        [ObservableProperty]
        private Item? editingItem;

        [ObservableProperty]
        private string editItemName = string.Empty;

        [ObservableProperty]
        private string editItemNameEn = string.Empty;

        [ObservableProperty]
        private string editItemPriceText = string.Empty;

        [ObservableProperty]
        private Category? editItemCategory;

        [ObservableProperty]
        private bool editItemIsActive;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(EditItemHasNoImage))]
        private string? editItemImagePath;

        [ObservableProperty]
        private string editItemStatusMessage = string.Empty;

        public bool EditItemHasNoImage =>
            string.IsNullOrWhiteSpace(EditItemImagePath);


        // =========================================================
        // EDIT CATEGORY
        // =========================================================

        [ObservableProperty]
        private bool isCategoryEditOpen;

        [ObservableProperty]
        private Category? editingCategory;

        [ObservableProperty]
        private string editCategoryName = string.Empty;

        [ObservableProperty]
        private string editCategoryNameEn = string.Empty;

        [ObservableProperty]
        private string editCategoryStatusMessage = string.Empty;


        // =========================================================
        // PRIVATE IMAGE VARIABLES
        // =========================================================

        private string? _pendingEditImageSourcePath;
        private bool _pendingRemoveImage;


        // =========================================================
        // EVENTS
        // =========================================================

        public event Action? BackRequested;

        public event Func<string?>? ChooseImageFileRequested;


        // =========================================================
        // INITIALIZATION
        // =========================================================

        public async Task InitializeAsync()
        {
            IsBusy = true;

            try
            {
                await ReloadCategoriesAsync();

                if (Categories.Count > 0)
                    SelectedNewItemCategory = Categories[0];

                await ReloadItemsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // LOAD CATEGORIES
        // =========================================================

        private async Task ReloadCategoriesAsync()
        {
            var previouslySelectedId =
                SelectedNewItemCategory?.CategoryId;

            Categories.Clear();

            foreach (var c in await _itemService.GetCategoriesAsync())
                Categories.Add(c);

            if (previouslySelectedId.HasValue)
            {
                var stillThere =
                    Categories.FirstOrDefault(
                        c => c.CategoryId == previouslySelectedId.Value);

                if (stillThere != null)
                    SelectedNewItemCategory = stillThere;
            }
        }


        // =========================================================
        // LOAD ITEMS
        // =========================================================

        private async Task ReloadItemsAsync()
        {
            _allItems =
                await _itemService.GetAllItemsForManagementAsync();

            TotalItemCount = _allItems.Count;

            CurrentPage = 1;

            UpdatePagedItems();
        }


        // =========================================================
        // PAGINATION
        // =========================================================

        private void UpdatePagedItems()
        {
            TotalPages = Math.Max(
                1,
                (int)Math.Ceiling(
                    _allItems.Count / (double)PageSize));

            if (CurrentPage > TotalPages)
                CurrentPage = TotalPages;

            var pageItems =
                _allItems
                    .Skip((CurrentPage - 1) * PageSize)
                    .Take(PageSize);

            Items.Clear();

            foreach (var item in pageItems)
                Items.Add(item);

            OnPropertyChanged(nameof(PageInfoText));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanGoPrevious));
        }


        [RelayCommand]
        private void NextPage()
        {
            if (!CanGoNext)
                return;

            CurrentPage++;

            UpdatePagedItems();
        }


        [RelayCommand]
        private void PreviousPage()
        {
            if (!CanGoPrevious)
                return;

            CurrentPage--;

            UpdatePagedItems();
        }


        // =========================================================
        // ADD ITEM IMAGE
        // =========================================================

        [RelayCommand]
        private void ChooseImage()
        {
            var path =
                ChooseImageFileRequested?.Invoke();

            if (!string.IsNullOrWhiteSpace(path))
                NewItemImageFilePath = path;
        }


        [RelayCommand]
        private void RemoveNewItemImage()
        {
            NewItemImageFilePath = null;
        }


        // =========================================================
        // ADD ITEM
        // =========================================================

        [RelayCommand]
        private async Task AddItemAsync()
        {
            AddItemStatusMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(NewItemName))
            {
                AddItemStatusMessage =
                    "الرجاء إدخال اسم الصنف";

                return;
            }

            if (!decimal.TryParse(
                    NewItemPriceText,
                    out var price)
                || price <= 0)
            {
                AddItemStatusMessage =
                    "الرجاء إدخال سعر صحيح";

                return;
            }

            if (SelectedNewItemCategory is null)
            {
                AddItemStatusMessage =
                    "الرجاء اختيار القسم";

                return;
            }

            IsBusy = true;

            try
            {
                var nameEn =
                    string.IsNullOrWhiteSpace(NewItemNameEn)
                        ? null
                        : NewItemNameEn;

                var item =
                    await _itemService.AddItemAsync(
                        SelectedNewItemCategory.CategoryId,
                        NewItemName,
                        price,
                        nameEn);


                // Save selected image
                if (!string.IsNullOrWhiteSpace(
                        NewItemImageFilePath)
                    &&
                    File.Exists(NewItemImageFilePath))
                {
                    var extension =
                        Path.GetExtension(
                            NewItemImageFilePath);

                    var relativePath =
                        $"Images/Items/{item.ItemId}{extension}";

                    var destinationFolder =
                        Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            "Images",
                            "Items");

                    Directory.CreateDirectory(
                        destinationFolder);

                    var destinationPath =
                        Path.Combine(
                            destinationFolder,
                            $"{item.ItemId}{extension}");

                    File.Copy(
                        NewItemImageFilePath,
                        destinationPath,
                        overwrite: true);

                    await _itemService
                        .UpdateItemImagePathAsync(
                            item.ItemId,
                            relativePath);
                }


                // Clear form
                NewItemName = string.Empty;
                NewItemNameEn = string.Empty;
                NewItemPriceText = string.Empty;
                NewItemImageFilePath = null;


                AddItemStatusMessage =
                    "تم إضافة الصنف بنجاح";

                await ReloadItemsAsync();

                // Automatically clear message
                _ = ClearMessageAfterDelayAsync(
                    () => AddItemStatusMessage = string.Empty);
            }
            catch (Exception ex)
            {
                AddItemStatusMessage =
                    $"حدث خطأ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // ADD CATEGORY
        // =========================================================

        [RelayCommand]
        private void ToggleAddCategoryForm()
        {
            ShowAddCategoryForm =
                !ShowAddCategoryForm;

            AddCategoryStatusMessage =
                string.Empty;
        }


        [RelayCommand]
        private async Task AddCategoryAsync()
        {
            AddCategoryStatusMessage =
                string.Empty;

            if (string.IsNullOrWhiteSpace(
                    NewCategoryName))
            {
                AddCategoryStatusMessage =
                    "الرجاء إدخال اسم القسم";

                return;
            }

            IsBusy = true;

            try
            {
                var nameEn =
                    string.IsNullOrWhiteSpace(
                        NewCategoryNameEn)
                        ? null
                        : NewCategoryNameEn;

                var category =
                    await _itemService.AddCategoryAsync(
                        NewCategoryName,
                        nameEn);


                NewCategoryName = string.Empty;
                NewCategoryNameEn = string.Empty;


                AddCategoryStatusMessage =
                    $"تم إضافة قسم \"{category.NameAr}\" بنجاح";

                await ReloadCategoriesAsync();


                _ = ClearMessageAfterDelayAsync(
                    () => AddCategoryStatusMessage =
                        string.Empty);
            }
            catch (Exception ex)
            {
                AddCategoryStatusMessage =
                    $"حدث خطأ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // DELETE CATEGORY
        // =========================================================

        [RelayCommand]
        private async Task DeleteCategoryAsync(
            Category category)
        {
            var confirm =
                System.Windows.MessageBox.Show(
                    $"هل أنت متأكد من حذف قسم \"{category.NameAr}\" نهائياً؟",
                    "تأكيد الحذف",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

            if (confirm !=
                System.Windows.MessageBoxResult.Yes)
                return;


            var (success, errorMessage) =
                await _itemService.DeleteCategoryAsync(
                    category.CategoryId);


            if (!success)
            {
                System.Windows.MessageBox.Show(
                    errorMessage,
                    "تعذر الحذف",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                return;
            }


            AddCategoryStatusMessage =
                $"تم حذف قسم \"{category.NameAr}\"";

            await ReloadCategoriesAsync();


            _ = ClearMessageAfterDelayAsync(
                () => AddCategoryStatusMessage =
                    string.Empty);
        }


        // =========================================================
        // OPEN EDIT ITEM
        // =========================================================

        [RelayCommand]
        private void OpenEditItem(Item item)
        {
            EditItemStatusMessage =
                string.Empty;

            _pendingEditImageSourcePath = null;
            _pendingRemoveImage = false;


            EditingItem = item;

            EditItemName =
                item.ItemName;

            EditItemNameEn =
                item.NameEn ?? string.Empty;

            EditItemPriceText =
                item.Price.ToString("#,##0");

            EditItemCategory =
                Categories.FirstOrDefault(
                    c => c.CategoryId ==
                         item.CategoryId);

            EditItemIsActive =
                item.IsActive;

            EditItemImagePath =
                item.ImagePath;

            IsItemEditOpen =
                true;
        }


        [RelayCommand]
        private void CancelEditItem()
        {
            IsItemEditOpen =
                false;

            EditingItem =
                null;

            EditItemStatusMessage =
                string.Empty;

            _pendingEditImageSourcePath =
                null;

            _pendingRemoveImage =
                false;
        }


        // =========================================================
        // EDIT ITEM IMAGE
        // =========================================================

        [RelayCommand]
        private void ChangeEditItemImage()
        {
            var path =
                ChooseImageFileRequested?.Invoke();


            if (string.IsNullOrWhiteSpace(path)
                || !File.Exists(path))
                return;


            _pendingEditImageSourcePath =
                path;

            _pendingRemoveImage =
                false;


            // Preview the selected image
            EditItemImagePath =
                path;
        }


        [RelayCommand]
        private void RemoveEditItemImage()
        {
            _pendingEditImageSourcePath =
                null;

            _pendingRemoveImage =
                true;

            EditItemImagePath =
                null;
        }


        // =========================================================
        // SAVE EDITED ITEM
        // =========================================================

        [RelayCommand]
        private async Task SaveEditedItemAsync()
        {
            EditItemStatusMessage =
                string.Empty;


            if (EditingItem is null)
                return;


            if (string.IsNullOrWhiteSpace(
                    EditItemName))
            {
                EditItemStatusMessage =
                    "اسم الصنف لا يمكن أن يكون فارغاً";

                return;
            }


            if (!decimal.TryParse(
                    EditItemPriceText,
                    out var price)
                || price <= 0)
            {
                EditItemStatusMessage =
                    "الرجاء إدخال سعر صحيح";

                return;
            }


            if (EditItemCategory is null)
            {
                EditItemStatusMessage =
                    "الرجاء اختيار القسم";

                return;
            }


            IsBusy = true;

            try
            {
                var itemId =
                    EditingItem.ItemId;


                var nameEn =
                    string.IsNullOrWhiteSpace(
                        EditItemNameEn)
                        ? null
                        : EditItemNameEn;


                // Update name
                await _itemService
                    .UpdateItemNameAsync(
                        itemId,
                        EditItemName,
                        nameEn);


                // Update price
                await _itemService
                    .UpdateItemPriceAsync(
                        itemId,
                        price);


                // Update category
                await _itemService
                    .UpdateItemCategoryAsync(
                        itemId,
                        EditItemCategory.CategoryId);


                // Update active state
                if (EditItemIsActive !=
                    EditingItem.IsActive)
                {
                    await _itemService
                        .SetItemActiveAsync(
                            itemId,
                            EditItemIsActive);
                }


                // Change image
                if (!string.IsNullOrWhiteSpace(
                        _pendingEditImageSourcePath))
                {
                    var extension =
                        Path.GetExtension(
                            _pendingEditImageSourcePath);

                    var relativePath =
                        $"Images/Items/{itemId}{extension}";


                    var destinationFolder =
                        Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory,
                            "Images",
                            "Items");


                    Directory.CreateDirectory(
                        destinationFolder);


                    var destinationPath =
                        Path.Combine(
                            destinationFolder,
                            $"{itemId}{extension}");


                    File.Copy(
                        _pendingEditImageSourcePath,
                        destinationPath,
                        overwrite: true);


                    await _itemService
                        .UpdateItemImagePathAsync(
                            itemId,
                            relativePath);
                }


                // Remove image
                else if (_pendingRemoveImage)
                {
                    await _itemService
                        .RemoveItemImageAsync(
                            itemId);
                }


                _pendingEditImageSourcePath =
                    null;

                _pendingRemoveImage =
                    false;


                // Close edit window
                IsItemEditOpen =
                    false;

                EditingItem =
                    null;


                EditItemStatusMessage =
                    string.Empty;


                await ReloadItemsAsync();
            }
            catch (Exception ex)
            {
                EditItemStatusMessage =
                    $"حدث خطأ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // DELETE ITEM
        // =========================================================

        [RelayCommand]
        private async Task DeleteItemAsync(
            Item item)
        {
            var result =
                System.Windows.MessageBox.Show(
                    $"هل أنت متأكد من حذف \"{item.ItemName}\" نهائياً؟ لا يمكن التراجع عن هذا الإجراء.",
                    "تأكيد الحذف",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);


            if (result !=
                System.Windows.MessageBoxResult.Yes)
                return;


            var (success, errorMessage) =
                await _itemService.DeleteItemAsync(
                    item.ItemId);


            if (!success)
            {
                System.Windows.MessageBox.Show(
                    errorMessage,
                    "تعذر الحذف",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);

                return;
            }


            AddItemStatusMessage =
                $"تم حذف {item.ItemName}";

            await ReloadItemsAsync();


            _ = ClearMessageAfterDelayAsync(
                () => AddItemStatusMessage =
                    string.Empty);
        }


        // =========================================================
        // OPEN EDIT CATEGORY
        // =========================================================

        [RelayCommand]
        private void OpenEditCategory(
            Category category)
        {
            EditCategoryStatusMessage =
                string.Empty;


            EditingCategory =
                category;


            EditCategoryName =
                category.NameAr;


            EditCategoryNameEn =
                category.NameEn ?? string.Empty;


            IsCategoryEditOpen =
                true;
        }


        [RelayCommand]
        private void CancelEditCategory()
        {
            IsCategoryEditOpen =
                false;

            EditingCategory =
                null;

            EditCategoryStatusMessage =
                string.Empty;
        }


        // =========================================================
        // SAVE EDITED CATEGORY
        // =========================================================

        [RelayCommand]
        private async Task SaveEditedCategoryAsync()
        {
            EditCategoryStatusMessage =
                string.Empty;


            if (EditingCategory is null)
                return;


            if (string.IsNullOrWhiteSpace(
                    EditCategoryName))
            {
                EditCategoryStatusMessage =
                    "اسم القسم لا يمكن أن يكون فارغاً";

                return;
            }


            IsBusy = true;

            try
            {
                var nameEn =
                    string.IsNullOrWhiteSpace(
                        EditCategoryNameEn)
                        ? null
                        : EditCategoryNameEn;


                await _itemService
                    .UpdateCategoryAsync(
                        EditingCategory.CategoryId,
                        EditCategoryName,
                        nameEn);


                // Close modal
                IsCategoryEditOpen =
                    false;

                EditingCategory =
                    null;


                EditCategoryStatusMessage =
                    string.Empty;


                await ReloadCategoriesAsync();

                await ReloadItemsAsync();
            }
            catch (Exception ex)
            {
                EditCategoryStatusMessage =
                    $"حدث خطأ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }


        // =========================================================
        // CLEAR MESSAGE AFTER DELAY
        // =========================================================

        private async Task ClearMessageAfterDelayAsync(
            Action clearAction)
        {
            await Task.Delay(3000);

            clearAction();
        }


        // =========================================================
        // BACK
        // =========================================================

        [RelayCommand]
        private void Back()
        {
            BackRequested?.Invoke();
        }
    }
}
 