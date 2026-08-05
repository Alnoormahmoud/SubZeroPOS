namespace SubZeroPOS.Core.DTOs
{
    public class AuthResultDto
    {
        public bool Success { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RoleCode { get; set; } = string.Empty;   // "Manager" / "Cashier"
        public string RoleName { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
}
