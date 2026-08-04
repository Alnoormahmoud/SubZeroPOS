using System.Collections.Generic;

namespace SubZeroPOS.Core.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleCode { get; set; } = string.Empty;   // "Manager" / "Cashier"
        public string RoleNameAr { get; set; } = string.Empty; // "مدير" / "كاشير"

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
