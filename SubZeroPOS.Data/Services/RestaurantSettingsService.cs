using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class RestaurantSettingsService : IRestaurantSettingsService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public RestaurantSettingsService(IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<RestaurantSettings> GetSettingsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var settings = await context.RestaurantSettings.FirstOrDefaultAsync();
            if (settings != null) return settings;

            // No row yet (fresh install) - create sensible defaults so the
            // invoice/settings screen always has something to show and edit.
            settings = new RestaurantSettings
            {
                RestaurantName = "سوب زيرو",
                InvoiceFooterPrimary = "شكراً لزيارتكم",
                InvoiceFooterSecondary = null
            };
            context.RestaurantSettings.Add(settings);
            await context.SaveChangesAsync();

            return settings;
        }

        public async Task SaveSettingsAsync(RestaurantSettings settings)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.RestaurantSettings.FirstOrDefaultAsync();
            if (existing is null)
            {
                context.RestaurantSettings.Add(settings);
            }
            else
            {
                existing.RestaurantName = settings.RestaurantName;
                existing.Phone = settings.Phone;
                existing.Address = settings.Address;
                existing.InvoiceFooterPrimary = settings.InvoiceFooterPrimary;
                existing.InvoiceFooterSecondary = settings.InvoiceFooterSecondary;
            }

            await context.SaveChangesAsync();
        }
    }
}
