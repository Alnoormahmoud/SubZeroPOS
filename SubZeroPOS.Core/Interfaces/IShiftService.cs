using System.Threading.Tasks;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Core.Interfaces
{
    public interface IShiftService
    {
        Task<ShiftClosing?> GetOpenShiftAsync();
        Task<ShiftClosing> OpenShiftAsync(int userId, decimal openingCash);
        Task<decimal> CalculateExpectedCashAsync(int shiftId);
        Task<(bool Success, string? ErrorMessage)> CloseShiftAsync(int shiftId, decimal actualCash, string? notes);
    }
}