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

            // Now uses the real ShiftId link instead of guessing by date
            // range - accurate even if an order gets edited/backdated later.
            var cashSales = await context.Orders
                .Where(o => o.ShiftId == shiftId && o.StatusCode == "Completed" && o.PaymentMethodCode == "Cash")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var periodEnd = shift.ClosedAt ?? DateTime.Now;
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

        public async Task<ShiftSummary> GetShiftSummaryAsync(int shiftId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var shift = await context.ShiftClosings.Include(s => s.User).FirstOrDefaultAsync(s => s.ShiftId == shiftId);
            if (shift is null) return new ShiftSummary();
            var totalExpenses = await context.Expenses
    .Where(e =>
        e.ExpenseDate >= shift.OpenedAt &&
        e.ExpenseDate <= shift.ClosedAt)
    .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var orders = await context.Orders
                .Where(o => o.ShiftId == shiftId && o.StatusCode == "Completed")
                .ToListAsync();

            return new ShiftSummary
            {
                Shift = shift,
                TotalOrderCount = orders.Count,
                CashOrderCount = orders.Count(o => o.PaymentMethodCode == "Cash"),
                BankOrderCount = orders.Count(o => o.PaymentMethodCode == "Bankak"),
                TotalSales = orders.Sum(o => o.TotalAmount),
                CashSales = orders.Where(o => o.PaymentMethodCode == "Cash").Sum(o => o.TotalAmount),
                BankSales = orders.Where(o => o.PaymentMethodCode == "Bankak").Sum(o => o.TotalAmount),
                TotalExpenses = totalExpenses
            };
        }
    }
}