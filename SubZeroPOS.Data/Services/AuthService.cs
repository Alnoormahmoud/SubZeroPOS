using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.DTOs;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public AuthService(IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<AuthResultDto> LoginAsync(string username, string password)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var user = await context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is null)
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة"
                };
            }

            if (!user.IsActive)
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "هذا الحساب غير مفعل"
                };
            }

            bool passwordOk = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            if (!passwordOk)
            {
                return new AuthResultDto
                {
                    Success = false,
                    ErrorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة"
                };
            }

            return new AuthResultDto
            {
                Success = true,
                UserId = user.UserId,
                FullName = user.FullName,
                RoleCode = user.Role.RoleCode,
                RoleNameAr = user.Role.RoleNameAr
            };
        }
    }
}
