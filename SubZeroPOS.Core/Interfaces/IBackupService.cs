using System.Threading.Tasks;

namespace SubZeroPOS.Core.Interfaces
{
    public interface IBackupService
    {
        Task<(bool Success, string Message)> CreateBackupAsync();
    }
}