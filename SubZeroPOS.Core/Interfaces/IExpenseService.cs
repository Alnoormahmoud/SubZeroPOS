using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Core.Interfaces
{
    public interface IExpenseService
    {
        Task<Expense> AddExpenseAsync(Expense expense);
        Task<List<Expense>> GetExpensesByDateRangeAsync(DateTime from, DateTime to);
        Task<List<ExpenseCategory>> GetExpenseCategoriesAsync();
    }
}
