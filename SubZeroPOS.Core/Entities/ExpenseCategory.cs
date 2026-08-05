using System.Collections.Generic;

namespace SubZeroPOS.Core.Entities
{
    public class ExpenseCategory
    {
        public int ExpenseCategoryId { get; set; }
        public string ExpenseName { get; set; } = string.Empty;

        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}
