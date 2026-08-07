using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class ItemService : IItemService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public ItemService(IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<Item>> GetItemsByCategoryAsync(int categoryId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Items
                .Where(i => i.CategoryId == categoryId && i.IsActive)
                .OrderBy(i => i.ItemName)
                .ToListAsync();
        }

        public async Task<List<Item>> GetAllItemsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Items
                .Where(i => i.IsActive)
                .OrderBy(i => i.CategoryId)
                .ThenBy(i => i.ItemName)
                .ToListAsync();
        }

        public async Task<Item?> GetItemByIdAsync(int itemId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Items.FirstOrDefaultAsync(i => i.ItemId == itemId);
        }
    }
}
