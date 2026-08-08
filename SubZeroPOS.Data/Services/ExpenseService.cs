using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class ExpenseService : IExpenseService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public ExpenseService(IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Expense> AddExpenseAsync(Expense expense)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.Expenses.Add(expense);
            await context.SaveChangesAsync();

            return expense;
        }

        public async Task<List<Expense>> GetExpensesByDateRangeAsync(DateTime from, DateTime to)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var start = from.Date;
            var end = to.Date.AddDays(1); // inclusive of the "to" day

            return await context.Expenses
                .Include(e => e.ExpenseCategory)
                .Include(e => e.EnteredByUser)
                .Where(e => e.ExpenseDate >= start && e.ExpenseDate < end)
                .OrderByDescending(e => e.ExpenseDate)
                .ToListAsync();
        }

        public async Task<List<ExpenseCategory>> GetExpenseCategoriesAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ExpenseCategories
                .OrderBy(ec => ec.NameAr)
                .ToListAsync();
        }
    }
}
