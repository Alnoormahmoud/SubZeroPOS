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
        private const string ConnectionString =
            "Server=.;Database=SubZeroPOS;Trusted_Connection=True;TrustServerCertificate=True;";

        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Database
                    // Using a Factory (not AddDbContext) is important for WPF specifically:
                    // WPF has no natural per-request scope like web apps do, so a single
                    // AddDbContext instance can end up shared/reused across ViewModels for
                    // the app's whole lifetime. If two operations ever overlap on the same
                    // DbContext, the SQL connection can get corrupted ("session is in the
                    // kill state" errors). The factory gives each operation its own
                    // short-lived context instead.
                    services.AddDbContextFactory<SubZeroDbContext>(options =>
                        options.UseSqlServer(ConnectionString));

                    // Services (Core interface -> Data implementation)
                    services.AddScoped<IAuthService, AuthService>();
                    services.AddScoped<IItemService, ItemService>();
                    services.AddScoped<IOrderService, OrderService>();
                    services.AddScoped<IExpenseService, ExpenseService>();
                    services.AddScoped<IRestaurantSettingsService, RestaurantSettingsService>();
                    services.AddScoped<IUserService, UserService>();
                    // TODO: services.AddScoped<IReportService, ReportService>();

                    // ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<MainDashboardViewModel>();
                    services.AddTransient<OrderEntryViewModel>();
                    services.AddTransient<ExpenseViewModel>();
                    services.AddTransient<OrderInvoiceViewModel>();
                    services.AddTransient<SettingsViewModel>();
                    services.AddTransient<UserManagementViewModel>();
                    services.AddTransient<MenuManagementViewModel>();

                    // Views
                    services.AddTransient<LoginView>();
                    services.AddTransient<MainDashboardView>();
                    services.AddTransient<OrderEntryView>();
                    services.AddTransient<ExpenseView>();
                    services.AddTransient<OrderInvoiceView>();
                    services.AddTransient<SettingsView>();
                    services.AddTransient<UserManagementView>();
                    services.AddTransient<MenuManagementView>();

                    // Main window
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}
