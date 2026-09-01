using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public ShiftService(IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<ShiftClosing?> GetOpenShiftAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.ShiftClosings
                .Include(s => s.User)
                .Where(s => s.ClosedAt == null)
                .OrderByDescending(s => s.OpenedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<ShiftClosing> OpenShiftAsync(int userId, decimal openingCash)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var shift = new ShiftClosing
            {
                UserId = userId,
                OpenedAt = DateTime.Now,
                OpeningCash = openingCash
            };

            context.ShiftClosings.Add(shift);
            await context.SaveChangesAsync();
            return shift;
        }

        public async Task<decimal> CalculateExpectedCashAsync(int shiftId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var shift = await context.ShiftClosings.FindAsync(shiftId);
            if (shift is null) return 0;

            var periodEnd = shift.ClosedAt ?? DateTime.Now;

            // Only cash orders affect the physical drawer - bank transfers don't.
            var cashSales = await context.Orders
                .Where(o => o.OrderDate >= shift.OpenedAt && o.OrderDate < periodEnd
                            && o.StatusCode == "Completed" && o.PaymentMethodCode == "Cash")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            // Expenses are assumed paid out of the till, same as most small shops.
            var expensesPaid = await context.Expenses
                .Where(e => e.ExpenseDate >= shift.OpenedAt && e.ExpenseDate < periodEnd)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            return shift.OpeningCash + cashSales - expensesPaid;
        }

        public async Task<(bool Success, string? ErrorMessage)> CloseShiftAsync(int shiftId, decimal actualCash, string? notes)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var shift = await context.ShiftClosings.FindAsync(shiftId);
            if (shift is null) return (false, "الوردية غير موجودة");
            if (shift.ClosedAt != null) return (false, "تم إغلاق هذه الوردية مسبقاً");

            var expected = await CalculateExpectedCashAsync(shiftId);

            shift.ClosedAt = DateTime.Now;
            shift.ExpectedCash = expected;
            shift.ActualCash = actualCash;
            shift.Notes = notes;

            await context.SaveChangesAsync();
            return (true, null);
        }
    }
}