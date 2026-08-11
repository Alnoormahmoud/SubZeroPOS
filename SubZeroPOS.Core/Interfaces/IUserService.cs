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
    }
}
