using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.Interfaces;

namespace SubZeroPOS.Data.Services
{
    public class BackupService : IBackupService
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;

        public BackupService(
            IDbContextFactory<SubZeroDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<(bool Success, string Message)> CreateBackupAsync(
            string backupPath)
        {
            if (string.IsNullOrWhiteSpace(backupPath))
                return (false, "مسار النسخة الاحتياطية غير صالح");

            try
            {
                backupPath = Path.GetFullPath(backupPath);

                var directory = Path.GetDirectoryName(backupPath);

                if (string.IsNullOrWhiteSpace(directory))
                    return (false, "تعذر تحديد مجلد النسخة الاحتياطية");

                Directory.CreateDirectory(directory);

                await using var context =
                    await _contextFactory.CreateDbContextAsync();

                var connection =
                    context.Database.GetDbConnection();

                await connection.OpenAsync();

                var databaseName = connection.Database;

                if (string.IsNullOrWhiteSpace(databaseName))
                    return (false, "تعذر تحديد اسم قاعدة البيانات");

                var escapedDatabaseName =
                    databaseName.Replace("]", "]]");

                var escapedBackupPath =
                    backupPath.Replace("'", "''");

                var sql = $@"
BACKUP DATABASE [{escapedDatabaseName}]
TO DISK = N'{escapedBackupPath}'
WITH INIT, FORMAT, STATS = 10;";

                await using var command =
                    connection.CreateCommand();

                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 600;

                await command.ExecuteNonQueryAsync();

                return (
                    true,
                    $"تم إنشاء النسخة الاحتياطية بنجاح:\n{backupPath}"
                );
            }
            catch (Exception ex)
            {
                return (
                    false,
                    $"فشل إنشاء النسخة الاحتياطية:\n{ex.Message}"
                );
            }
        }

        public async Task<(bool Success, string Message)> RestoreBackupAsync(
            string backupPath)
        {
            if (string.IsNullOrWhiteSpace(backupPath))
                return (false, "ملف النسخة الاحتياطية غير صالح");

            if (!File.Exists(backupPath))
                return (false, "ملف النسخة الاحتياطية غير موجود");

            try
            {
                backupPath = Path.GetFullPath(backupPath);

                await using var context =
                    await _contextFactory.CreateDbContextAsync();

                var connection =
                    context.Database.GetDbConnection();

                await connection.OpenAsync();

                var databaseName = connection.Database;

                if (string.IsNullOrWhiteSpace(databaseName))
                    return (false, "تعذر تحديد اسم قاعدة البيانات");

                var escapedDatabaseName =
                    databaseName.Replace("]", "]]");

                var escapedBackupPath =
                    backupPath.Replace("'", "''");

                /*
                 * We connect to the database's connection but execute
                 * the restore commands against master.
                 *
                 * SQL Server must not have active connections to the
                 * database while restoring it.
                 */

                var masterConnectionString =
                    connection.ConnectionString
                        .Replace(
                            $"Database={databaseName}",
                            "Database=master",
                            StringComparison.OrdinalIgnoreCase)
                        .Replace(
                            $"Initial Catalog={databaseName}",
                            "Initial Catalog=master",
                            StringComparison.OrdinalIgnoreCase);

                await using var masterConnection =
                    new SqlConnection(masterConnectionString);

                await masterConnection.OpenAsync();

                var sql = $@"
USE [master];

ALTER DATABASE [{escapedDatabaseName}]
SET SINGLE_USER
WITH ROLLBACK IMMEDIATE;

RESTORE DATABASE [{escapedDatabaseName}]
FROM DISK = N'{escapedBackupPath}'
WITH REPLACE, RECOVERY, STATS = 10;

ALTER DATABASE [{escapedDatabaseName}]
SET MULTI_USER;
";

                await using var command =
                    masterConnection.CreateCommand();

                command.CommandText = sql;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = 600;

                await command.ExecuteNonQueryAsync();

                return (
                    true,
                    "تمت استعادة قاعدة البيانات بنجاح.\n\n" +
                    "يرجى إعادة تشغيل البرنامج."
                );
            }
            catch (Exception ex)
            {
                return (
                    false,
                    $"فشل استعادة النسخة الاحتياطية:\n{ex.Message}"
                );
            }
        }
    }
}