using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.Data;
using SubZeroPOS.Data.Services;
using SubZeroPOS.WPF.Services;
using SubZeroPOS.WPF.ViewModels;
using SubZeroPOS.WPF.Views;
using System.Windows;

namespace SubZeroPOS.WPF
{
    public partial class App : Application
    {
        private IHost? _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                _host = Host.CreateDefaultBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        var connectionString =
                            context.Configuration.GetConnectionString(
                                "DefaultConnection");

                        if (string.IsNullOrWhiteSpace(connectionString))
                        {
                            throw new InvalidOperationException(
                                "DefaultConnection is missing from appsettings.json.");
                        }

                        services.AddDbContextFactory<SubZeroDbContext>(
                            options =>
                                options.UseSqlServer(connectionString));

                        // Services
                        services.AddTransient<IAuthService, AuthService>();
                        services.AddTransient<IItemService, ItemService>();
                        services.AddTransient<IOrderService, OrderService>();
                        services.AddTransient<IExpenseService, ExpenseService>();
                        services.AddTransient<
                            IRestaurantSettingsService,
                            RestaurantSettingsService>();
                        services.AddTransient<IUserService, UserService>();
                        services.AddTransient<IReportService, ReportService>();
                        services.AddTransient<IShiftService, ShiftService>();
                        services.AddTransient<IBackupService, BackupService>();
                        services.AddSingleton<WeeklyBackupScheduler>();

                        // ViewModels
                        services.AddTransient<LoginViewModel>();
                        services.AddTransient<MainDashboardViewModel>();
                        services.AddTransient<OrderEntryViewModel>();
                        services.AddTransient<ExpenseViewModel>();
                        services.AddTransient<OrderInvoiceViewModel>();
                        services.AddTransient<SettingsViewModel>();
                        services.AddTransient<UserManagementViewModel>();
                        services.AddTransient<MenuManagementViewModel>();
                        services.AddTransient<PreviousOrdersViewModel>();
                        services.AddTransient<ReportsViewModel>();
                        services.AddTransient<ShiftViewModel>();
                        services.AddTransient<MyAccountViewModel>();

                        // Views
                        services.AddTransient<LoginView>();
                        services.AddTransient<MainDashboardView>();
                        services.AddTransient<OrderEntryView>();
                        services.AddTransient<ExpenseView>();
                        services.AddTransient<OrderInvoiceView>();
                        services.AddTransient<SettingsView>();
                        services.AddTransient<UserManagementView>();
                        services.AddTransient<MenuManagementView>();
                        services.AddTransient<PreviousOrdersView>();
                        services.AddTransient<ReportsView>();
                        services.AddTransient<ShiftView>();
                        services.AddTransient<MyAccountView>();

                        // Main window
                        services.AddSingleton<MainWindow>();
                    })
                    .Build();

                // Start the Host
                await _host.StartAsync();

                // ============================================================
                // DATABASE MIGRATION
                // ============================================================

                var dbFactory =
                    _host.Services
                        .GetRequiredService<
                            IDbContextFactory<SubZeroDbContext>>();

                await using (var db =
                    await dbFactory.CreateDbContextAsync())
                {
                    // Creates the database if it doesn't exist
                    // and applies all pending migrations.
                    await db.Database.MigrateAsync();

                    await DatabaseSeeder.SeedAsync(db);
                }

                // ============================================================
                // LOAD SETTINGS
                // ============================================================

                var settingsService =
                    _host.Services
                        .GetRequiredService<IRestaurantSettingsService>();

                var settings =
                    await settingsService.GetSettingsAsync();

                // Apply application settings
                Session.CurrencyHolder.Symbol =
                    settings.CurrencySymbol;

                ThemeManager.ApplyTheme(settings.Theme);

                var weeklyBackupScheduler =
    _host.Services
        .GetRequiredService<WeeklyBackupScheduler>();

                weeklyBackupScheduler.Start();

                // ============================================================
                // CREATE MAIN WINDOW
                // ============================================================

                var mainWindow =
                    _host.Services.GetRequiredService<MainWindow>();

                MainWindow = mainWindow;

                // Show only after:
                // 1. Host started
                // 2. Database exists
                // 3. All migrations are applied
                // 4. Settings are loaded
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "SubZeroPOS - Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnExit(e);
        }
    }
}
