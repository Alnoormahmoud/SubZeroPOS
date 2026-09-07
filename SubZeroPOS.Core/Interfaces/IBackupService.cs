using System.Threading.Tasks;

namespace SubZeroPOS.Core.Interfaces
{
    public interface IBackupService
    {
        Task<(bool Success, string Message)> CreateBackupAsync(
            string backupPath);

        Task<(bool Success, string Message)> RestoreBackupAsync(
            string backupPath);
    }
}