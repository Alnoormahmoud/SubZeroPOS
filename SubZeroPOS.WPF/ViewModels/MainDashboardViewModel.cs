using System;
using System.Globalization;
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

        // Default Gregorian date format
        private string _clockFormat = "yyyy/MM/dd - hh:mm:ss";

        // Use English culture so the date remains Gregorian/English
        private readonly CultureInfo _englishCulture = new("en-US");

        public MainDashboardViewModel(
            IOrderService orderService,
            IExpenseService expenseService,
            IRestaurantSettingsService settingsService)
        {
            _orderService = orderService;
            _expenseService = expenseService;
            _settingsService = settingsService;

            CurrentDateTime = FormatDateTime(DateTime.Now);

            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _clockTimer.Tick += (_, _) =>
            {
                CurrentDateTime = FormatDateTime(DateTime.Now);
            };

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
        public event Action? ShiftRequested;


        // ============================================================
        // Format Date/Time
        // ============================================================
        // Example:
        // 2026/08/30 - 05:17:32 مساءً
        //
        // The date stays Gregorian/English.
        // Only AM/PM is converted to Arabic.
        // ============================================================
        private string FormatDateTime(DateTime dateTime)
        {
            string formattedDate = dateTime.ToString(
                _clockFormat,
                _englishCulture);

            string amPm = dateTime.ToString(
                "tt",
                _englishCulture);

            string arabicAmPm = amPm == "AM"
                ? "صباحًا"
                : "مساءً";

            // Remove AM/PM if it exists in the format
            formattedDate = formattedDate
                .Replace("AM", "")
                .Replace("PM", "")
                .Trim();

            return $"{formattedDate} {arabicAmPm}";
        }


        // ============================================================
        // Refresh Dashboard Statistics
        // ============================================================
        public async Task RefreshStatsAsync()
        {
            // WelcomeMessage/IsManager read CurrentSession.
            // Notify the UI after login.
            OnPropertyChanged(nameof(WelcomeMessage));
            OnPropertyChanged(nameof(IsManager));


            // Keep the header clock in sync with the format
            // selected in Settings.
            var settings = await _settingsService.GetSettingsAsync();

            _clockFormat = settings.DateFormat;

            CurrentDateTime = FormatDateTime(DateTime.Now);


            // ========================================================
            // Today's Date
            // ========================================================
            var today = DateTime.Today;


            // ========================================================
            // Today's Orders
            // ========================================================
            var orders = await _orderService.GetOrdersByDateAsync(today);

            var completedOrders = orders
                .Where(o => o.StatusCode == "Completed")
                .ToList();


            // ========================================================
            // Today's Sales
            // ========================================================
            TodaysSales = completedOrders
                .Sum(o => o.TotalAmount)
                .ToString("#,##0");


            // ========================================================
            // Today's Order Count
            // ========================================================
            TodaysOrderCount = completedOrders.Count.ToString();


            // ========================================================
            // Today's Expenses
            // ========================================================
            var expenses = await _expenseService
                .GetExpensesByDateRangeAsync(today, today);

            TodaysExpenses = expenses
                .Sum(e => e.Amount)
                .ToString("#,##0");
        }


        // ============================================================
        // Commands
        // ============================================================

        [RelayCommand]
        private void Logout()
        {
            CurrentSession.Clear();

            LogoutRequested?.Invoke();
        }


        [RelayCommand]
        private void NewOrder()
        {
            NewOrderRequested?.Invoke();
        }


        [RelayCommand]
        private void OpenExpenses()
        {
            ExpensesRequested?.Invoke();
        }


        [RelayCommand]
        private void OpenReports()
        {
            ReportsRequested?.Invoke();
        }


        [RelayCommand]
        private void OpenUserManagement()
        {
            UserManagementRequested?.Invoke();
        }


        [RelayCommand]
        private void OpenMenuManagement()
        {
            MenuManagementRequested?.Invoke();
        }


        [RelayCommand]
        private void OpenSettings()
        {
            SettingsRequested?.Invoke();
        }


        [RelayCommand]
        private void OpenPreviousOrders()
        {
            PreviousOrdersRequested?.Invoke();
        }

        [RelayCommand]
        private void OpenShift() => ShiftRequested?.Invoke();
    }
}
 