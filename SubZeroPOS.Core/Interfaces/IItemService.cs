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
    }
}
