namespace SubZeroPOS.WPF.Session
{
    public static class CurrentSession
    {
        public static int UserId { get; set; }
        public static string FullName { get; set; } = string.Empty;
        public static string RoleCode { get; set; } = string.Empty; // "Manager" / "Cashier"
        public static string RoleNameAr { get; set; } = string.Empty;

        public static bool IsManager => RoleCode == "Manager";

        public static void Clear()
        {
            UserId = 0;
            FullName = string.Empty;
            RoleCode = string.Empty;
            RoleNameAr = string.Empty;
        }
    }
}
