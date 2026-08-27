using System.Collections.Generic;
using System.Threading.Tasks;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Core.Interfaces
{
    public interface IItemService
    {
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Item>> GetItemsByCategoryAsync(int categoryId);
        Task<List<Item>> GetAllItemsAsync();
        Task<Item?> GetItemByIdAsync(int itemId);
        Task<Item> AddItemAsync(int categoryId, string itemName, decimal price, string? nameEn = null);
        Task<bool> UpdateItemPriceAsync(int itemId, decimal newPrice);
        Task<bool> UpdateItemNameAsync(int itemId, string itemName, string? nameEn);
        Task<bool> SetItemActiveAsync(int itemId, bool isActive);
        Task<bool> UpdateItemImagePathAsync(int itemId, string imagePath);
        Task<bool> RemoveItemImageAsync(int itemId);
        Task<bool> UpdateItemCategoryAsync(int itemId, int categoryId);
        Task<List<Item>> GetAllItemsForManagementAsync(); // includes inactive items
        Task<(bool Success, string? ErrorMessage)> DeleteItemAsync(int itemId);
        Task<Category> AddCategoryAsync(string nameAr, string? nameEn);
        Task<bool> UpdateCategoryAsync(int categoryId, string nameAr, string? nameEn);
        Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(int categoryId);
    }
}
