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
                .OrderBy(i => i.ItemName)
                .ToListAsync();
        }

        public async Task<Item?> GetItemByIdAsync(int itemId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Items.FirstOrDefaultAsync(i => i.ItemId == itemId);
        }

        public async Task<Item> AddItemAsync(int categoryId, string itemName, decimal price, string? nameEn = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var item = new Item
            {
                CategoryId = categoryId,
                ItemName = itemName,
                NameEn = nameEn,
                Price = price,
                IsActive = true
            };

            context.Items.Add(item);
            await context.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateItemImagePathAsync(int itemId, string imagePath)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var item = await context.Items.FindAsync(itemId);
            if (item is null) return false;

            item.ImagePath = imagePath;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateItemPriceAsync(int itemId, decimal newPrice)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var item = await context.Items.FindAsync(itemId);
            if (item is null) return false;

            item.Price = newPrice;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetItemActiveAsync(int itemId, bool isActive)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var item = await context.Items.FindAsync(itemId);
            if (item is null) return false;

            item.IsActive = isActive;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Item>> GetAllItemsForManagementAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Items
                .Include(i => i.Category)
                .OrderBy(i => i.CategoryId).ThenBy(i => i.ItemName)
                .ToListAsync();
        }
    }
}
