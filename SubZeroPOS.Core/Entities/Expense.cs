using System;

namespace SubZeroPOS.Core.Entities
{
    public class Expense
    {
        public int ExpenseId { get; set; }
        public int ExpenseCategoryId { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; } = DateTime.Now;
        public int EnteredByUserId { get; set; }

        public ExpenseCategory ExpenseCategory { get; set; } = null!;
        public User EnteredByUser { get; set; } = null!;
    }
}
