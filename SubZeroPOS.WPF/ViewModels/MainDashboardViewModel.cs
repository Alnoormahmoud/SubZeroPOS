using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SubZeroPOS.Core.Interfaces;
using SubZeroPOS.WPF.Session;

namespace SubZeroPOS.WPF.ViewModels
{
    public partial class MainDashboardViewModel : ObservableObject
    {
        private readonly IOrderService _orderService;
        private readonly IExpenseService _expenseService;
        private readonly IRestaurantSettingsService _settingsService;
        private readonly DispatcherTimer _clockTimer;
        private string _clockFormat = "yyyy/MM/dd - hh:mm:ss";

        public MainDashboardViewModel(IOrderService orderService, IExpenseService expenseService, IRestaurantSettingsService settingsService)
        {
            _orderService = orderService;
            _expenseService = expenseService;
            _settingsService = settingsService;

            CurrentDateTime = DateTime.Now.ToString(_clockFormat);
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += (_, _) => CurrentDateTime = DateTime.Now.ToString(_clockFormat);
            _clockTimer.Start();
        }

        public string WelcomeMessage => $"مرحباً، {CurrentSession.FullName}";
        public bool IsManager => CurrentSession.IsManager;

        [ObservableProperty]
        private string currentDateTime = string.Empty;

        [ObservableProperty]
        private string todaysSales = "0";

        [ObservableProperty]
        private string todaysExpenses = "0";

        [ObservableProperty]
        private string todaysOrderCount = "0";

        public event Action? LogoutRequested;
        public event Action? NewOrderRequested;
        public event Action? ExpensesRequested;
        public event Action? ReportsRequested;
        public event Action? UserManagementRequested;
        public event Action? MenuManagementRequested;
        public event Action? SettingsRequested;
        public event Action? PreviousOrdersRequested;

        // Called every time the dashboard becomes visible (Loaded fires on every
        // navigation back to it), so numbers always reflect the latest orders/expenses.
        public async Task RefreshStatsAsync()
        {
            // WelcomeMessage/IsManager read CurrentSession, which is empty when
            // this ViewModel is first constructed (before login happens) - re-notify
            // them here so the greeting and role-based visibility stay correct.
            OnPropertyChanged(nameof(WelcomeMessage));
            OnPropertyChanged(nameof(IsManager));

            // Keep the header clock in sync with the format chosen in Settings.
            var settings = await _settingsService.GetSettingsAsync();
            _clockFormat = settings.DateFormat;
            CurrentDateTime = DateTime.Now.ToString(_clockFormat);

            var today = DateTime.Today;

            var orders = await _orderService.GetOrdersByDateAsync(today);
            var completedOrders = orders.Where(o => o.StatusCode == "Completed").ToList();

             TodaysSales = completedOrders.Sum(o => o.TotalAmount).ToString("#,##0");
            TodaysOrderCount = completedOrders.Count.ToString();

            var expenses = await _expenseService.GetExpensesByDateRangeAsync(today, today);
            TodaysExpenses = expenses.Sum(e => e.Amount).ToString("#,##0");
        }

        [RelayCommand]
        private void Logout()
        {
            CurrentSession.Clear();
            LogoutRequested?.Invoke();
        }

        [RelayCommand]
        private void NewOrder() => NewOrderRequested?.Invoke();

        [RelayCommand]
        private void OpenExpenses() => ExpensesRequested?.Invoke();

        [RelayCommand]
        private void OpenReports() => ReportsRequested?.Invoke();

        [RelayCommand]
        private void OpenUserManagement() => UserManagementRequested?.Invoke();

        [RelayCommand]
        private void OpenMenuManagement() => MenuManagementRequested?.Invoke();

        [RelayCommand]
        private void OpenSettings() => SettingsRequested?.Invoke();

        [RelayCommand]
        private void OpenPreviousOrders() => PreviousOrdersRequested?.Invoke();
    }
}
