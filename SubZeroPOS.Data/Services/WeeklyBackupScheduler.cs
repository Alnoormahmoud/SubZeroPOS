using Microsoft.EntityFrameworkCore;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SubZeroPOS.WPF.Services
{
    public class WeeklyBackupScheduler
    {
        private readonly IDbContextFactory<SubZeroDbContext> _contextFactory;
        private readonly IBackupService _backupService;

        private Timer? _timer;

        private static string BackupDirectory =>
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.CommonApplicationData),
                "SubZeroPOS",
                "Backups");

        public WeeklyBackupScheduler(
            IDbContextFactory<SubZeroDbContext> contextFactory,
            IBackupService backupService)
        {
            _contextFactory = contextFactory;
            _backupService = backupService;
        }

        public void Start()
        {
            /*
             * Check immediately when the application starts,
             * then check once every hour.
             */
            _timer = new Timer(
                async _ => await CheckBackupAsync(),
                null,
                TimeSpan.FromSeconds(30),
                TimeSpan.FromHours(1));
        }

        private async Task CheckBackupAsync()
        {
            try
            {
                await using var db =
                    await _contextFactory.CreateDbContextAsync();

                var settings =
                    await db.RestaurantSettings
                        .AsNoTracking()
                        .FirstOrDefaultAsync();

                if (settings == null)
                    return;

                if (!settings.WeeklyBackupEnabled)
                    return;

                var now = DateTime.Now;

                if (settings.LastWeeklyBackup.HasValue &&
                    now - settings.LastWeeklyBackup.Value
                    < TimeSpan.FromDays(7))
                {
                    return;
                }

                Directory.CreateDirectory(BackupDirectory);

                var timestamp =
                    now.ToString("yyyy-MM-dd_HHmmss");

                var backupPath =
                    Path.Combine(
                        BackupDirectory,
                        $"SubZeroPOS_Weekly_{timestamp}.bak");

                var result =
                    await _backupService.CreateBackupAsync(
                        backupPath);

                if (!result.Success)
                    return;

                await using var updateDb =
                    await _contextFactory.CreateDbContextAsync();

                var updateSettings =
                    await updateDb.RestaurantSettings
                        .FirstOrDefaultAsync();

                if (updateSettings == null)
                    return;

                updateSettings.LastWeeklyBackup = now;

                await updateDb.SaveChangesAsync();
            }
            catch
            {
                /*
                 * Automatic backup must never crash
                 * the POS application.
                 *
                 * You can add logging here later.
                 */
            }
        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }
}