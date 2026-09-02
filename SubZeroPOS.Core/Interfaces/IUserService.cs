using System.Collections.Generic;
using System.Threading.Tasks;
using SubZeroPOS.Core.Entities;

namespace SubZeroPOS.Core.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<List<Role>> GetRolesAsync();
        Task<bool> CreateUserAsync(string fullName, string username, string plainPassword, int roleId);
        Task<bool> SetUserActiveAsync(int userId, bool isActive);
        Task<bool> UsernameExistsAsync(string username);
        Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(int userId);


        // ==========================================
        // Manager
        // ==========================================

        Task<(bool Success, string? ErrorMessage)> UpdateUserAsync(
            int userId,
            string fullName,
            string username,
            int roleId,
            string? newPassword);

 

        // ==========================================
        // My Account
        // ==========================================

        Task<User?> GetUserByIdAsync(int userId);

        Task<(bool Success, string? ErrorMessage)>
            UpdateMyAccountAsync(
                int userId,
                string fullName,
                string username);

        Task<(bool Success, string? ErrorMessage)>
            ChangePasswordAsync(
                int userId,
                string currentPassword,
                string newPassword);
    }
}
