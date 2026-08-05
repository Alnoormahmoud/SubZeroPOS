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
            "Server=.\\SQLEXPRESS;Database=SubZeroPOS;User Id=sa;Password=12345;Trusted_Connection = True; TrustServerCertificate = True;";
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Database
                    services.AddDbContext<SubZeroDbContext>(options =>
                        options.UseSqlServer(ConnectionString));

                    // Services (Core interface -> Data implementation)
                    services.AddScoped<IAuthService, AuthService>();
                    // TODO as each is implemented:
                    // services.AddScoped<IOrderService, OrderService>();
                    // services.AddScoped<IItemService, ItemService>();
                    // services.AddScoped<IExpenseService, ExpenseService>();
                    // services.AddScoped<IReportService, ReportService>();

                    // ViewModels
                    services.AddTransient<LoginViewModel>();

                    // Views
                    services.AddTransient<LoginView>();

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
