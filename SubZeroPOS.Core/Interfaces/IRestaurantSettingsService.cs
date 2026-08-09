using System.Threading.Tasks;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Core.Interfaces
{
    public interface IRestaurantSettingsService
    {
        Task<RestaurantSettings> GetSettingsAsync();
        Task SaveSettingsAsync(RestaurantSettings settings);
    }
}
