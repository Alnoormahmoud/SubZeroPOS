using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.Data;
using SubZeroPOS.Data.Services;
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

                // Load settings BEFORE showing the application
                var settingsService =
                    _host.Services
                        .GetRequiredService<IRestaurantSettingsService>();

                var settings =
                    await settingsService.GetSettingsAsync();

                // Apply application settings
                Session.CurrencyHolder.Symbol =
                    settings.CurrencySymbol;

                ThemeManager.ApplyTheme(settings.Theme);

                // Create MainWindow only after settings are loaded
                var mainWindow =
                    _host.Services.GetRequiredService<MainWindow>();

                MainWindow = mainWindow;

                // Show only after everything is ready
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