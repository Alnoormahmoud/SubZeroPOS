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
        Task<ShiftSummary> GetShiftSummaryAsync(int shiftId);
    }

    public class ShiftSummary
    {
        public ShiftClosing? Shift { get; set; }
        public int TotalOrderCount { get; set; }
        public int CashOrderCount { get; set; }
        public int BankOrderCount { get; set; }
        public decimal TotalSales { get; set; }
        public decimal CashSales { get; set; }
        public decimal BankSales { get; set; }
    }
}