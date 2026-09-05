using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.Data;
using SubZeroPOS.Data.Services;
using SubZeroPOS.WPF.ViewModels;
using SubZeroPOS.WPF.Views;

namespace SubZeroPOS.WPF
{
    public partial class App : Application
    {
        // TODO: move this to appsettings.json once the project is stable.
        // For now it's centralized here so it's the one place to edit
        // when deploying to your friend's machine.
        //  private const string ConnectionString = "Server=.\\SQLEXPRESS;Database=SubZeroPOS;Trusted_Connection=True;TrustServerCertificate=True;";
        private const string ConnectionString = "Server=.;Database=SubZeroPOS;Trusted_Connection=True;TrustServerCertificate=True;";


        private IHost? _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContextFactory<SubZeroDbContext>(options =>
                        options.UseSqlServer(ConnectionString));

                    services.AddTransient<IAuthService, AuthService>();
                    services.AddTransient<IItemService, ItemService>();
                    services.AddTransient<IOrderService, OrderService>();
                    services.AddTransient<IExpenseService, ExpenseService>();
                    services.AddTransient<IRestaurantSettingsService, RestaurantSettingsService>();
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

                    services.AddSingleton<MainWindow>();
                })
                .Build();

            // Create the window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();

            // Show it FIRST
            mainWindow.Show();

            // Then load settings
            try
            {
                var settingsService =
                    _host.Services.GetRequiredService<IRestaurantSettingsService>();

                var settings = await settingsService.GetSettingsAsync();

                SubZeroPOS.WPF.Session.CurrencyHolder.Symbol =
                    settings.CurrencySymbol;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load restaurant settings.\n\n{ex.Message}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
