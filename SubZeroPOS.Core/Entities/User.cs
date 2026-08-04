using System;
using System.Collections.Generic;

namespace SubZeroPOS.Core.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Role Role { get; set; } = null!;
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
        public ICollection<ShiftClosing> ShiftClosings { get; set; } = new List<ShiftClosing>();
    }
}
