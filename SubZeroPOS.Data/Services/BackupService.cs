using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class BackupService : IBackupService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public BackupService(IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<(bool Success, string Message)> CreateBackupAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                // Find where SQL Server actually stores this database's data
                // file - guaranteed the SQL Server service account can write
                // there, so backing up into a "Backups" subfolder next to it
                // avoids permission issues a random app-chosen folder might hit.
                var dataFilePath = await context.Database
                    .SqlQueryRaw<string>(
                        "SELECT physical_name AS [Value] FROM sys.master_files WHERE database_id = DB_ID() AND type = 0")
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(dataFilePath))
                    return (false, "تعذر تحديد موقع قاعدة البيانات على السيرفر");

                var dataDir = Path.GetDirectoryName(dataFilePath);
                var backupDir = Path.Combine(dataDir!, "Backups");

                // Ensure the folder exists (SQL Server itself creates it via
                // this extended stored procedure, since the app process may
                // not have direct filesystem access to the DB server).
                await context.Database.ExecuteSqlRawAsync($"EXEC master.dbo.xp_create_subdir N'{backupDir}'");

                var dbName = context.Database.GetDbConnection().Database;
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
                var backupFileName = $"{dbName}_{timestamp}.bak";
                var backupFullPath = Path.Combine(backupDir, backupFileName);

                await context.Database.ExecuteSqlRawAsync(
                    $"BACKUP DATABASE [{dbName}] TO DISK = N'{backupFullPath}' WITH INIT, COMPRESSION");

                return (true, backupFullPath);
            }
            catch (Exception ex)
            {
                return (false, $"فشل النسخ الاحتياطي: {ex.Message}");
            }
        }
    }
}