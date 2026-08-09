using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.Entities;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class UserService : IUserService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public UserService(IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<List<Role>> GetRolesAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Roles.ToListAsync();
        }

        public async Task<bool> CreateUserAsync(string fullName, string username, string plainPassword, int roleId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            if (await context.Users.AnyAsync(u => u.Username == username))
                return false;

            var user = new User
            {
                FullName = fullName,
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                RoleId = roleId,
                IsActive = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetUserActiveAsync(int userId, bool isActive)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var user = await context.Users.FindAsync(userId);
            if (user is null) return false;

            user.IsActive = isActive;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            return await context.Users.AnyAsync(u => u.Username == username);
        }
    }
}
