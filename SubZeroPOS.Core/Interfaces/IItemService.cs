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
        Task<Item> AddItemAsync(int categoryId, string itemName, decimal price);
        Task<bool> UpdateItemPriceAsync(int itemId, decimal newPrice);
        Task<bool> SetItemActiveAsync(int itemId, bool isActive);
        Task<List<Item>> GetAllItemsForManagementAsync(); // includes inactive items
    }
}
