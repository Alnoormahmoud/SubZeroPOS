using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        // TODO: move this to appsettings.json once the project is stable.
        // For now it's centralized here so it's the one place to edit
        // when deploying to your friend's machine.


        private IHost? _host;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        var connectionString =
            context.Configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextFactory<SubZeroDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IItemService, ItemService>();
        services.AddTransient<IOrderService, OrderService>();
        services.AddTransient<IExpenseService, ExpenseService>();
        services.AddTransient<IRestaurantSettingsService, RestaurantSettingsService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IReportService, ReportService>();
        services.AddTransient<IShiftService, ShiftService>();
        services.AddTransient<IBackupService, BackupService>();
        services.AddDbContextFactory<SubZeroDbContext>(options =>


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


            try
            {
                var settingsService =
                    _host.Services.GetRequiredService<IRestaurantSettingsService>();

                var settings = await settingsService.GetSettingsAsync();

                Session.CurrencyHolder.Symbol =
                    settings.CurrencySymbol;

                ThemeManager.ApplyTheme(settings.Theme);

                var mainWindow =
                    _host.Services.GetRequiredService<MainWindow>();

                MainWindow = mainWindow;

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
    }
}
