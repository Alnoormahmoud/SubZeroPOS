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

        public async Task<(bool Success, string? ErrorMessage)> DeleteUserAsync(int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var user = await context.Users.FindAsync(userId);
            if (user is null) return (false, "المستخدم غير موجود");

            // A user who ever cashiered an order or logged an expense can't be
            // hard-deleted (would corrupt that history) - only disable is safe.
            bool hasOrders = await context.Orders.AnyAsync(o => o.CashierUserId == userId);
            bool hasExpenses = await context.Expenses.AnyAsync(e => e.EnteredByUserId == userId);

            if (hasOrders || hasExpenses)
                return (false, "لا يمكن حذف هذا المستخدم لوجود طلبات أو مصروفات مرتبطة به. استخدم زر التعطيل بدلاً من ذلك.");

            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return (true, null);
        }

        // =========================================================
        // MANAGER: UPDATE ANY USER
        // =========================================================

        public async Task<(bool Success, string? ErrorMessage)>
            UpdateUserAsync(
                int userId,
                string fullName,
                string username,
                int roleId,
                string? newPassword)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            var user = await context.Users.FindAsync(userId);

            if (user is null)
                return (false, "المستخدم غير موجود");

            fullName = fullName.Trim();
            username = username.Trim();

            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "الاسم الكامل مطلوب");

            if (string.IsNullOrWhiteSpace(username))
                return (false, "اسم المستخدم مطلوب");

            // Check username, but ignore the current user.
            bool usernameExists =
                await context.Users.AnyAsync(
                    u => u.Username == username &&
                         u.UserId != userId);

            if (usernameExists)
                return (false, "اسم المستخدم مستخدم بالفعل");

            user.FullName = fullName;
            user.Username = username;
            user.RoleId = roleId;

            // Password is optional when manager edits a user.
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                if (newPassword.Length < 4)
                    return (
                        false,
                        "كلمة المرور يجب أن تكون ٤ أحرف على الأقل"
                    );

                user.PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            await context.SaveChangesAsync();

            return (true, null);
        }

      
  
        // ==========================================
        // Get user by ID
        // ==========================================

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();

            return await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(
                    u => u.UserId == userId);
        }


        // ==========================================
        // Update my account
        // ==========================================

        public async Task<(bool Success, string? ErrorMessage)>
            UpdateMyAccountAsync(
                int userId,
                string fullName,
                string username)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();


            var user = await context.Users
                .FirstOrDefaultAsync(
                    u => u.UserId == userId);


            if (user is null)
            {
                return (
                    false,
                    "المستخدم غير موجود");
            }


            // Check whether another user already has this username.
            bool usernameExists =
                await context.Users.AnyAsync(
                    u => u.Username == username &&
                         u.UserId != userId);


            if (usernameExists)
            {
                return (
                    false,
                    "اسم المستخدم مستخدم بالفعل");
            }


            user.FullName = fullName.Trim();
            user.Username = username.Trim();


            await context.SaveChangesAsync();


            return (
                true,
                null);
        }


        // ==========================================
        // Change password
        // ==========================================

        public async Task<(bool Success, string? ErrorMessage)>
            ChangePasswordAsync(
                int userId,
                string currentPassword,
                string newPassword)
        {
            await using var context =
                await _contextFactory.CreateDbContextAsync();


            var user = await context.Users
                .FirstOrDefaultAsync(
                    u => u.UserId == userId);


            if (user is null)
            {
                return (
                    false,
                    "المستخدم غير موجود");
            }


            // Verify current password.
            bool passwordCorrect =
                BCrypt.Net.BCrypt.Verify(
                    currentPassword,
                    user.PasswordHash);


            if (!passwordCorrect)
            {
                return (
                    false,
                    "كلمة المرور الحالية غير صحيحة");
            }


            // Minimum password validation.
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                return (
                    false,
                    "كلمة المرور الجديدة مطلوبة");
            }


            if (newPassword.Length < 4)
            {
                return (
                    false,
                    "كلمة المرور الجديدة يجب أن تكون 4 أحرف على الأقل");
            }


            // Create new hash.
            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    newPassword);


            await context.SaveChangesAsync();


            return (
                true,
                null);
        }
    }
}
