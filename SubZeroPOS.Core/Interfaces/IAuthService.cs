using System.Threading.Tasks;
using SubZeroPOS.Core.DTOs;

namespace SubZeroPOS.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultDto> LoginAsync(string username, string password);
    }
}
