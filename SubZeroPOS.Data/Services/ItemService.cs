using System;
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

        public async Task<bool> UpdateItemNameAsync(int itemId, string itemName, string? nameEn)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var item = await context.Items.FindAsync(itemId);
            if (item is null) return false;

            item.ItemName = itemName;
            item.NameEn = nameEn;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveItemImageAsync(int itemId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var item = await context.Items.FindAsync(itemId);
            if (item is null) return false;

            // Also delete the physical file if it exists, so orphaned images
            // don't pile up in the Images/Items folder over time.
            if (!string.IsNullOrWhiteSpace(item.ImagePath))
            {
                var fullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, item.ImagePath);
                if (System.IO.File.Exists(fullPath))
                {
                    try { System.IO.File.Delete(fullPath); } catch { /* non-critical */ }
                }
            }

            item.ImagePath = null;
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

        public async Task<(bool Success, string? ErrorMessage)> DeleteItemAsync(int itemId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var item = await context.Items.FindAsync(itemId);
            if (item is null) return (false, "الصنف غير موجود");

            // Items referenced by past orders can't be truly deleted (would
            // corrupt order history / break the foreign key) - only items with
            // zero order history can be permanently removed. Anything else
            // should be disabled instead (soft-delete via IsActive).
            bool hasOrderHistory = await context.OrderItems.AnyAsync(oi => oi.ItemId == itemId);
            if (hasOrderHistory)
                return (false, "لا يمكن حذف هذا الصنف لوجود طلبات سابقة تحتوي عليه. استخدم زر التعطيل بدلاً من ذلك.");

            context.Items.Remove(item);
            await context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<Category> AddCategoryAsync(string nameAr, string? nameEn)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            int maxOrder = await context.Categories.AnyAsync()
                ? await context.Categories.MaxAsync(c => c.DisplayOrder)
                : 0;

            var category = new Category
            {
                NameAr = nameAr,
                NameEn = nameEn,
                DisplayOrder = maxOrder + 1,
                IsActive = true
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();
            return category;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteCategoryAsync(int categoryId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var category = await context.Categories.FindAsync(categoryId);
            if (category is null) return (false, "القسم غير موجود");

            // A category with items in it (active or disabled) can't be safely
            // removed - the items would be orphaned. Ask the user to move/delete
            // those items first.
            bool hasItems = await context.Items.AnyAsync(i => i.CategoryId == categoryId);
            if (hasItems)
                return (false, "لا يمكن حذف هذا القسم لوجود أصناف تابعة له. احذف أو انقل الأصناف أولاً.");

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return (true, null);
        }
    }
}
