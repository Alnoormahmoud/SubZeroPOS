using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.DTOs;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class ReportService : IReportService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public ReportService(IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<ReportSummaryDto> GetSummaryAsync(DateTime from, DateTime to)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var start = from.Date;
            var end = to.Date.AddDays(1); // inclusive of the "to" day

            var orders = await context.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate < end && o.StatusCode == "Completed")
                .ToListAsync();

            var expenses = await context.Expenses
                .Include(e => e.ExpenseCategory)
                .Where(e => e.ExpenseDate >= start && e.ExpenseDate < end)
                .ToListAsync();

            var expensesByCategory = expenses
                .GroupBy(e => e.ExpenseCategory.NameAr)
                .Select(g => new ExpenseByCategoryDto
                {
                    CategoryNameAr = g.Key,
                    Total = g.Sum(e => e.Amount)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            return new ReportSummaryDto
            {
                FromDate = from,
                ToDate = to,
                TotalIncome = orders.Sum(o => o.TotalAmount),
                TotalOrders = orders.Count,
                TotalExpenses = expenses.Sum(e => e.Amount),
                ExpensesByCategory = expensesByCategory
            };
        }
    }
}
